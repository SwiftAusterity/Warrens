using NetMud.CentralControl;
using NetMud.Combat;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    /// <summary>
    /// live player character entities
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class Player : EntityPartial, IPlayer
    {
        #region Template and Framework Values
        public override bool IsPlayer()
        {
            return true;
        }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IPlayerTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), AccountHandle, TemplateId));
        }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public bool StillANoob { get; set; }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        public StaffRank GamePermissionsRank { get; set; }

        public MobilityState StancePosition { get; set; }

        #endregion

        [ScriptIgnore]
        [JsonIgnore]
        private LiveCacheKey _descriptorKey;

        /// <summary>
        /// The connection the player is using to chat with us
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IDescriptor Descriptor
        {
            get
            {
                if (_descriptorKey == null)
                {
                    return default;
                }

                return LiveCache.Get<IDescriptor>(_descriptorKey);
            }

            set
            {
                _descriptorKey = new LiveCacheKey(value);

                PersistToCache();
            }
        }

        /// <summary>
        /// Type of connection this has, doesn't get saved as it's transitory information
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override IChannelType ConnectionType
        {
            get
            {
                //All player descriptors should be of ichanneltype too
                return (IChannelType)Descriptor;
            }
        }

        /// <summary>
        /// The account this character belongs to
        /// </summary>
        public string AccountHandle { get; set; }

        /// <summary>
        /// The description
        /// </summary>
        public override string Description
        {
            get
            {
                return string.Format("{0} is here.", TemplateName);
            }
            set
            {
                //none
            }
        }

        /// <summary>
        /// fArt Combos
        /// </summary>
        public HashSet<IFightingArtCombination> Combos { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Player()
        {
            Qualities = new HashSet<IQuality>();
            Combos = new HashSet<IFightingArtCombination>();
            EnemyGroup = new HashSet<Tuple<IPlayer, int>>();
            AllianceGroup = new HashSet<IPlayer>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(IPlayerTemplate character)
        {
            Qualities = new HashSet<IQuality>();
            TemplateId = character.Id;
            AccountHandle = character.AccountHandle;
            Combos = new HashSet<IFightingArtCombination>();
            EnemyGroup = new HashSet<Tuple<IPlayer, int>>();
            AllianceGroup = new HashSet<IPlayer>();
            GetFromWorldOrSpawn();
        }

        #region Connectivity Details
        /// <summary>
        /// Function used to close this connection
        /// </summary>
        public void CloseConnection()
        {
            Descriptor.Disconnect(string.Empty);
        }

        public override bool WriteTo(IEnumerable<string> output, bool delayed = false)
        {
            IEnumerable<string> strings = MessagingUtility.TranslateColorVariables(output.ToArray(), this);

            if (delayed)
            {
                var working = OutputBuffer.Any();

                //enforce the output buffer
                OutputBuffer.Add(strings);

                if (!working)
                {
                    Processor.StartSingeltonLoop(string.Format("PlayerOutputWriter_{0}", TemplateName), 1, 0, 2, () => SendOutput());
                }
            }
            else
            {
                return Descriptor.SendOutput(strings);
            }

            return true;
        }

        private bool SendOutput()
        {
            var sent = Descriptor.SendOutput(string.Join(" ", OutputBuffer.Select(cluster => string.Join(" ", cluster))));

            OutputBuffer.Clear();

            return sent;
        }
        #endregion

        #region health and combat
        /// <summary>
        /// Max stamina
        /// </summary>
        public int TotalStamina { get; set; }

        /// <summary>
        /// Max Health
        /// </summary>
        public ulong TotalHealth { get; set; }

        /// <summary>
        /// Current stamina for this
        /// </summary>
        public int CurrentStamina { get; set; }

        /// <summary>
        /// Current health for this
        /// </summary>
        public ulong CurrentHealth { get; set; }

        /// <summary>
        /// How much stagger this currently has
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public int Stagger { get; set; }

        /// <summary>
        /// How much stagger resistance this currently has
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public int Sturdy { get; set; }

        /// <summary>
        /// How off balance this is. Positive is forward leaning, negative is backward leaning, 0 is in balance
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public int Balance { get; set; }

        /// <summary>
        /// What stance this is currently in (for fighting art combo choosing)
        /// </summary>
        public string Stance { get; set; }

        /// <summary>
        /// Is the current attack executing
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public bool Executing { get; set; }

        /// <summary>
        /// Last attack executed
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IFightingArt LastAttack { get; set; }

        /// <summary>
        /// Last combo used for attacking
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IFightingArtCombination LastCombo { get; set; }

        /// <summary>
        /// Who you're primarily attacking
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IPlayer PrimaryTarget { get; set; }

        /// <summary>
        /// Who you're fighting in general
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<Tuple<IPlayer, int>> EnemyGroup { get; set; }

        /// <summary>
        /// Who you're support/tank focus is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IPlayer PrimaryDefending { get; set; }

        /// <summary>
        /// Who is in your group
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IPlayer> AllianceGroup { get; set; }


        /// <summary>
        /// Stop all aggression
        /// </summary>
        public void StopFighting()
        {
            LastCombo = null;
            LastAttack = null;
            Executing = false;
            PrimaryTarget = null;
            EnemyGroup = new HashSet<Tuple<IPlayer, int>>();
        }

        /// <summary>
        /// Start a fight or switch targets forcibly
        /// </summary>
        /// <param name="victim"></param>
        public void StartFighting(IPlayer victim)
        {
            var wasFighting = IsFighting();

            if (victim == null)
            {
                victim = this;
            }

            if (victim != GetTarget())
            {
                PrimaryTarget = victim;
            }

            if (!EnemyGroup.Any(enemy => enemy.Item1 == PrimaryTarget))
            {
                EnemyGroup.Add(new Tuple<IPlayer, int>(PrimaryTarget, 0));
            }

            if (!wasFighting)
            {
                Processor.StartSubscriptionLoop("Fighting", () => Round.ExecuteRound(this, victim), 50, false);
            }
        }

        /// <summary>
        /// Get the target to attack
        /// </summary>
        /// <returns>A target or self if shadowboxing</returns>
        public IPlayer GetTarget()
        {
            var target = PrimaryTarget;

            //TODO: AI for NPCs for other branches
            if (PrimaryTarget == null || (PrimaryTarget.BirthMark.Equals(BirthMark) && EnemyGroup.Count() > 0))
            {
                PrimaryTarget = EnemyGroup.OrderByDescending(enemy => enemy.Item2).FirstOrDefault()?.Item1;
                target = PrimaryTarget;
            }

            return target;
        }

        /// <summary>
        /// Is this actor in combat
        /// </summary>
        /// <returns>yes or no</returns>
        public bool IsFighting()
        {
            return GetTarget() != null;
        }

        public int Exhaust(int exhaustionAmount)
        {
            int stam = Sleep(-1 * exhaustionAmount);

            //TODO: Check for total exhaustion

            return stam;
        }

        public ulong Harm(ulong damage)
        {
            CurrentHealth = Math.Max(0, TotalHealth - damage);

            //TODO: Check for DEATH

            return CurrentHealth;
        }

        public ulong Recover(ulong recovery)
        {
            CurrentHealth = Math.Max(0, Math.Min(TotalHealth, TotalHealth + recovery));

            return CurrentHealth;
        }

        public int Sleep(int hours)
        {
            CurrentStamina = Math.Max(0, Math.Min(TotalStamina, TotalStamina + hours * 10));

            return CurrentStamina;
        }
        #endregion

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPlayer GetLiveInstance()
        {
            return this;
        }

        public override void KickoffProcesses()
        {
            //quality degredation and stam/health regen
            Processor.StartSubscriptionLoop("Regeneration", Regen, 250, false);
            Processor.StartSubscriptionLoop("QualityDecay", QualityTimer, 500, false);
        }

        public bool Regen()
        {
            if(!IsFighting())
            {
                Recover(TotalHealth / 100);
            }

            Sleep(1);

            Descriptor.SendWrapper();
            return true;
        }

        public bool QualityTimer()
        {
            foreach (var quality in Qualities.Where(qual => qual.Value > 0))
            {
                SetQuality(-1, quality.Name, true);
            }

            Descriptor.SendWrapper();

            return true;
        }

        #region Rendering
        #endregion

        #region SpawnBehavior
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IPlayer me = LiveCache.Get<IPlayer>(TemplateId);

            //Isn't in the world currently
            if (me == default(IPlayer))
            {
                SpawnNewInWorld();
            }
            else
            {
                IPlayerTemplate ch = me.Template<IPlayerTemplate>();
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = ch.Id;
                Keywords = me.Keywords;
                CurrentHealth = me.CurrentHealth;
                CurrentStamina = me.CurrentStamina;

                Qualities = me.Qualities;

                TotalHealth = me.TotalHealth;
                TotalStamina = me.TotalStamina;
                SurName = me.SurName;
                StillANoob = me.StillANoob;
                GamePermissionsRank = me.GamePermissionsRank;
                Combos = me.Combos;

                if (CurrentHealth == 0)
                {
                    CurrentHealth = ch.TotalHealth;
                }

                if (CurrentStamina == 0)
                {
                    CurrentStamina = ch.TotalStamina;
                }

                if (me.CurrentLocation == null)
                {
                    TryMoveTo(GetBaseSpawn());
                }
                else
                {
                    TryMoveTo((IGlobalPosition)me.CurrentLocation.Clone());
                }
            }
        }


        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            IPlayerTemplate ch = Template<IPlayerTemplate>();

            SpawnNewInWorld(new GlobalPosition(ch.CurrentSlice));
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            //We can't even try this until we know if the data is there
            IPlayerTemplate ch = Template<IPlayerTemplate>() ?? throw new InvalidOperationException("Missing backing data store on player spawn event.");

            Keywords = ch.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(ch);
                Birthdate = DateTime.Now;
            }

            Qualities = ch.Qualities;
            CurrentHealth = ch.TotalHealth;
            CurrentStamina = ch.TotalStamina;
            TotalHealth = ch.TotalHealth;
            TotalStamina = ch.TotalStamina;
            SurName = ch.SurName ?? "";
            StillANoob = ch.StillANoob;
            GamePermissionsRank = ch.GamePermissionsRank;
            Combos = new HashSet<IFightingArtCombination>(ch.Account.Config.Combos);

            IGlobalPosition spawnTo = position ?? GetBaseSpawn();

            TryMoveTo(spawnTo);

            KickoffProcesses();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;
            IPlayerTemplate ch = Template<IPlayerTemplate>();

            //validate position
            if (newPosition != null)
            {
                CurrentLocation = newPosition;
                UpsertToLiveWorldCache();

                ch.CurrentSlice = newPosition.CurrentSection;
                ch.SystemSave();
                ch.PersistToCache();
                Save();
                UpsertToLiveWorldCache(true);
            }
            else
            {
                error = "Cannot move to an invalid location";
            }

            return error;
        }

        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        public override bool Save()
        {
            try
            {
                PlayerData dataAccessor = new PlayerData();
                dataAccessor.WriteOnePlayer(this);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Find the emergency we dont know where to spawn this guy spawn location
        /// </summary>
        /// <returns>The emergency spawn location</returns>
        private IGlobalPosition GetBaseSpawn()
        {
            return new GlobalPosition(0);
        }

        public override object Clone()
        {
            throw new NotImplementedException("Can't clone player objects.");
        }
        #endregion
    }
}
