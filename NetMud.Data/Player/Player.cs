using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Current stamina for this
        /// </summary>
        public int CurrentStamina { get; set; }

        /// <summary>
        /// Current health for this
        /// </summary>
        public int CurrentHealth { get; set; }
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
        /// Sensory overrides for staff member characters
        /// </summary>
        public IDictionary<MessagingType, bool> SuperSenses { get; set; }

        /// <summary>
        /// NPC's race data
        /// </summary>
        [Display(Name = "Race", Description = "Your genetic basis. Many races must be unlocked through specific means.")]
        public IRace Race { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Player()
        {
            Inventory = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(IPlayerTemplate character)
        {
            Qualities = new HashSet<IQuality>();
            Inventory = new EntityContainer<IInanimate>();
            TemplateId = character.Id;
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

            if (dT.SuperSenses[MessagingType.Visible])
                return new ValueRange<float>(-999999, 999999);

            int returnTop = 1;
            int returnBottom = 100;

            //TODO: Check for blindess/magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetAuditoryRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses[MessagingType.Audible])
                return new ValueRange<float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetPsychicRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses[MessagingType.Psychic])
                return new ValueRange<float>(-999999, 999999);

            var returnTop = 0; //TODO: Add this to race or something
            var returnBottom = 0;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetTasteRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses[MessagingType.Taste])
                return new ValueRange<float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetTactileRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses[MessagingType.Tactile])
                return new ValueRange<float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetOlefactoryRange()
        {
            IPlayerTemplate dT = Template<IPlayerTemplate>();

            if (dT.SuperSenses[MessagingType.Olefactory])
                return new ValueRange<float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

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

        /// <summary>
        /// Any mobiles (players, npcs) contained in this
        /// </summary>
        public IEntityContainer<IMobile> MobilesInside { get; set; }

        public int Capacity => 50;

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(MobilesInside.EntitiesContained().Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained().Select(ent => (T)ent));

            return contents;
        }

        /// <summary>
        /// Get all of the entities matching a type inside this in a named container
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        /// <param name="containerName">the name of the container</param>
        public IEnumerable<T> GetContents<T>(string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IMobile)))
                contents.AddRange(MobilesInside.EntitiesContained(containerName).Select(ent => (T)ent));

            if (implimentedTypes.Contains(typeof(IInanimate)))
                contents.AddRange(Inventory.EntitiesContained(containerName).Select(ent => (T)ent));

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
            return MoveInto(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity into a named container in this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (Inventory.Contains(obj, containerName))
                    return "That is already in the container";

                string moveError = MoveInto(obj);
                if (!string.IsNullOrWhiteSpace(moveError))
                    return moveError;

                Inventory.Add(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (MobilesInside.Contains(obj, containerName))
                    return "That is already in the container";

                string moveError = MoveInto(obj);
                if (!string.IsNullOrWhiteSpace(moveError))
                    return moveError;

                MobilesInside.Add(obj, containerName);
                UpsertToLiveWorldCache();

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
            return MoveFrom(thing, string.Empty);
        }

        /// <summary>
        /// Move an entity out of this' named container
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <param name="containerName">the name of the container</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing, string containerName)
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                var obj = (IInanimate)thing;

                if (!Inventory.Contains(obj, containerName))
                    return "That is not in the container";

                obj.TryMoveTo(null);
                Inventory.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            if (implimentedTypes.Contains(typeof(IMobile)))
            {
                var obj = (IMobile)thing;

                if (!MobilesInside.Contains(obj, containerName))
                    return "That is not in the container";

                obj.TryMoveTo(null);
                MobilesInside.Remove(obj, containerName);
                UpsertToLiveWorldCache();

                return string.Empty;
            }

            return "Invalid type to move from container.";
        }

        public IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(CurrentLocation.CurrentZone, CurrentLocation.CurrentLocale, CurrentLocation.CurrentRoom) { CurrentContainer = this };
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

                TotalHealth = me.TotalHealth;
                TotalStamina = me.TotalStamina;
                SurName = me.SurName;
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
            CurrentHealth = ch.TotalHealth;
            CurrentStamina = ch.TotalStamina;
            TotalHealth = ch.TotalHealth;
            TotalStamina = ch.TotalStamina;
            SurName = ch.SurName;
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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            var height = Race.Head.Model.Height + Race.Torso.Model.Height + Race.Legs.Item1.Model.Height;
            var length = Race.Torso.Model.Length;
            var width = Race.Torso.Model.Width;

            return new Dimensions(height, length, width);
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
