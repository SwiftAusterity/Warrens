using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Inanimates;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
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
        public bool IsPlayer()
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
        /// Gender data string for player characters
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for player character
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Has this character "graduated" from the tutorial yet
        /// </summary>
        public bool StillANoob { get; set; }

        /// <summary>
        /// Sensory overrides for staff member characters
        /// </summary>
        public bool SuperVision { get; set; }

        /// <summary>
        /// The "user" level for commands and accessibility
        /// </summary>
        public StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// Max stamina
        /// </summary>
        public int TotalStamina { get; set; }

        /// <summary>
        /// Max Health
        /// </summary>
        public int TotalHealth { get; set; }
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
                    return default(IDescriptor);

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
        /// Abilities this can use freely
        /// </summary>
        public HashSet<IUse> UsableAbilities { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Player()
        {
            Inventory = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
            UsableAbilities = new HashSet<IUse>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(IPlayerTemplate character)
        {
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
            Inventory = new EntityContainer<IInanimate>();
            TemplateId = character.Id;
            UsableAbilities = new HashSet<IUse>();
            AccountHandle = character.AccountHandle;
            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Function used to close this connection
        /// </summary>
        public void CloseConnection()
        {
            Descriptor.Disconnect(string.Empty);
        }

        public override bool WriteTo(IEnumerable<string> input)
        {
            IEnumerable<string> strings = MessagingUtility.TranslateColorVariables(input.ToArray(), this);

            return Descriptor.SendOutput(strings);
        }

        public int Exhaust(int exhaustionAmount)
        {
            int stam = Sleep(-1 * exhaustionAmount);

            //TODO: Check for total exhaustion

            return stam;
        }

        public int Harm(int damage)
        {
            int health = Recover(-1 * damage);

            //TODO: Check for DEATH

            return health;
        }

        public int Recover(int recovery)
        {
            CurrentHealth = Math.Max(0, Math.Min(TotalHealth, TotalHealth + recovery));

            return CurrentHealth;
        }

        public int Sleep(int hours)
        {
            CurrentStamina = Math.Max(0, Math.Min(TotalStamina, TotalStamina + hours * 10));

            return CurrentStamina;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPlayer GetLiveInstance()
        {
            return this;
        }

        #region sensory range checks
        /// <summary>
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetVisualRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperVision)
                return new ValueRange<float>(-999999, 999999);

            int returnTop = 1;
            int returnBottom = 100;

            //TODO: Check for blindess/magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            foreach (IInanimate dude in Inventory.EntitiesContained())
                lumins += dude.GetCurrentLuminosity();

            //TODO: Magical light, equipment, make inventory less bright depending on where it is

            return lumins;
        }
        #endregion

        #region Rendering
        #endregion

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Inventory { get; set; }
        public int Capacity => 50;

        public int CurrentStamina { get; set; }

        public int CurrentHealth { get; set; }

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained().Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (Inventory.Contains(obj))
                    return "That is already in the container";

                IEnumerable<IInanimate> contents = Inventory.EntitiesContained();

                //Should we even check? we have 100 slots make sure there's not one just open anyways
                if (contents.Count() == 100)
                {
                    //Check for stacking
                    int sameType = contents.Where(entity => entity.TemplateId.Equals(obj.TemplateId)).Count();
                    int numStacks = Math.DivRem(sameType, obj.Template<IInanimateTemplate>().AccumulationCap, out int extraSpace);

                    //We need an extra slot for this to go in, we're operating on 100 slots of inventory and they're all full and no room in an existing stack so error
                    if (extraSpace == 0)
                    {
                        return "Your inventory is full and no stacks have room.";
                    }
                }

                Inventory.Add(obj);
                UpsertToLiveWorldCache();

                //update the player's inventory
                Descriptor.SendUIUpdates();
                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (!Inventory.Contains(obj))
                    return "That is not in the container";

                Inventory.Remove(obj);

                obj.TryMoveTo(null);

                UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }

        /// <summary>
        /// Returns this entity as a container position
        /// </summary>
        /// <returns></returns>
        public IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(this);
        }

        /// <summary>
        /// Show the stacks in this container, only for inanimates
        /// </summary>
        /// <returns>A list of the item stacks</returns>
        public HashSet<IItemStack> ShowStacks(IEntity observer)
        {
            HashSet<IItemStack> stacks = new HashSet<IItemStack>();

            foreach(IGrouping<long, IInanimate> itemGroup in Inventory.EntitiesContained().GroupBy(item => item.TemplateId))
            {
                stacks.Add(new ItemStack(itemGroup));
            }

            return stacks;
        }
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
                SpawnNewInWorld();
            else
            {
                var ch = me.Template<IPlayerTemplate>();
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = ch.Id;
                Inventory = me.Inventory;
                Keywords = me.Keywords;
                CurrentHealth = me.CurrentHealth;
                CurrentStamina = me.CurrentStamina;

                Qualities = me.Qualities;
                DecayEvents = me.DecayEvents;
                Interactions = me.Interactions;

                TotalHealth = me.TotalHealth;
                TotalStamina = me.TotalStamina;
                SurName = me.SurName;
                SuperVision = me.SuperVision;
                StillANoob = me.StillANoob;
                GamePermissionsRank = me.GamePermissionsRank;

                if (CurrentHealth == 0)
                    CurrentHealth = ch.TotalHealth;

                if (CurrentStamina == 0)
                    CurrentStamina = ch.TotalStamina;

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

            if (ch.CurrentLocation?.CurrentZone == null)
                ch.CurrentLocation = GetBaseSpawn();

            SpawnNewInWorld(ch.CurrentLocation);
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

            Inventory = new EntityContainer<IInanimate>();

            Qualities = ch.Qualities;
            DecayEvents = ch.DecayEvents;
            Interactions = ch.Interactions;
            CurrentHealth = ch.TotalHealth;
            CurrentStamina = ch.TotalStamina;
            TotalHealth = ch.TotalHealth;
            TotalStamina = ch.TotalStamina;
            SurName = ch.SurName;
            SuperVision = ch.SuperVision;
            StillANoob = ch.StillANoob;
            GamePermissionsRank = ch.GamePermissionsRank;

            IGlobalPosition spawnTo = position ?? GetBaseSpawn();

            //Set the data context's stuff too so we don't have to do this over again
            ch.CurrentLocation = spawnTo;
            ch.Save(ch.Account, StaffRank.Player); //characters/players dont actually need approval

            TryMoveTo(spawnTo);

            UpsertToLiveWorldCache(true);

            KickoffProcesses();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentContainer != null)
            {
                error = CurrentLocation.CurrentContainer.MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentContainer != null)
                {
                    error = newPosition.CurrentContainer.MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
                    //Check for intruders
                    newPosition = IntruderSlide(newPosition);

                    CurrentLocation = newPosition;
                    UpsertToLiveWorldCache();
                    error = string.Empty;

                    IPlayerTemplate dt = Template<IPlayerTemplate>();
                    dt.CurrentLocation = newPosition;
                    dt.SystemSave();
                }
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
            //TODO
            int zoneId = StillANoob ? 0 : 0;

            return new GlobalPosition(LiveCache.Get<IZone>(zoneId));
        }

        public override object Clone()
        {
            throw new NotImplementedException("Can't clone player objects.");
        }
        #endregion
    }
}
