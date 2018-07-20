using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// live player character entities
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class Player : EntityPartial, IPlayer
    {
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
        /// News up an empty entity
        /// </summary>
        public Player()
        {
            Inventory = new EntityContainer<IInanimate>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(ICharacter character)
        {
            Inventory = new EntityContainer<IInanimate>();
            DataTemplateId = character.Id;
            AccountHandle = character.AccountHandle;
            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string DataTemplateName
        {
            get
            {
                return DataTemplate<ICharacter>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)PlayerDataCache.Get(new PlayerDataCacheKey(typeof(ICharacter), AccountHandle, DataTemplateId));
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
            var strings = MessagingUtility.TranslateColorVariables(input.ToArray(), this);

            return Descriptor.SendWrapper(strings);
        }
        
        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            var charData = DataTemplate<ICharacter>(); ;
            var height = charData.RaceData.Head.Model.Height + charData.RaceData.Torso.Model.Height + charData.RaceData.Legs.Item1.Model.Height;
            var length = charData.RaceData.Torso.Model.Length;
            var width = charData.RaceData.Torso.Model.Width;

            return new Tuple<int, int, int>(height, length, width);
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
        public override Tuple<float, float> GetVisualRange()
        {
            var dT = DataTemplate<ICharacter>();

            if(dT.SuperSenses[MessagingType.Visible])
                return new Tuple<float, float>(-999999, 999999);

            var returnTop = (float)dT.RaceData.VisionRange.Item1;
            var returnBottom = (float)dT.RaceData.VisionRange.Item2;

            //TODO: Check for blindess/magical type affects

            return new Tuple<float, float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override Tuple<float, float> GetAuditoryRange()
        {
            var dT = DataTemplate<ICharacter>();

            if (dT.SuperSenses[MessagingType.Audible])
                return new Tuple<float, float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new Tuple<float, float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override Tuple<float, float> GetPsychicRange()
        {
            var dT = DataTemplate<ICharacter>();

            if (dT.SuperSenses[MessagingType.Psychic])
                return new Tuple<float, float>(-999999, 999999);

            var returnTop = 0; //TODO: Add this to race or something
            var returnBottom = 0;

            //TODO: Check for magical type affects

            return new Tuple<float, float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override Tuple<float, float> GetTasteRange()
        {
            var dT = DataTemplate<ICharacter>();

            if (dT.SuperSenses[MessagingType.Taste])
                return new Tuple<float, float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new Tuple<float, float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override Tuple<float, float> GetTactileRange()
        {
            var dT = DataTemplate<ICharacter>();

            if (dT.SuperSenses[MessagingType.Tactile])
                return new Tuple<float, float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new Tuple<float, float>(returnTop, returnBottom);
        }

        /// <summary>
        /// Gets the actual modifier taking into account other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override Tuple<float, float> GetOlefactoryRange()
        {
            var dT = DataTemplate<ICharacter>();

            if (dT.SuperSenses[MessagingType.Olefactory])
                return new Tuple<float, float>(-999999, 999999);

            var returnTop = 1; //TODO: Add this to race or something
            var returnBottom = 100;

            //TODO: Check for magical type affects

            return new Tuple<float, float>(returnTop, returnBottom);
        }


        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            foreach (var dude in Inventory.EntitiesContained())
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
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            var implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            var contents = new List<T>();

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
            return MoveInto<T>(thing, string.Empty);
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

                if (!obj.TryMoveInto(this))
                    return "Unable to move into that container.";

                Inventory.Add(obj, containerName);
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
            return MoveFrom<T>(thing, string.Empty);
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

                obj.TryMoveInto(null);

                Inventory.Remove(obj, containerName);
                UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }
        #endregion

        #region SpawnBehavior
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<Player>(DataTemplateId);

            //Isn't in the world currently
            if (me == default(IPlayer))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplate<ICharacter>().Id;
                Inventory = me.Inventory;
                Keywords = me.Keywords;

                if (me.CurrentLocation == null)
                {
                    var newLoc = GetBaseSpawn();
                    newLoc.MoveInto<IPlayer>(this);
                }
                else
                    me.CurrentLocation.CurrentLocation.MoveInto<IPlayer>(this);
            }
        }


        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            var ch = DataTemplate<ICharacter>(); ;

            SpawnNewInWorld(ch.CurrentLocation);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            //We can't even try this until we know if the data is there
            var ch = DataTemplate<ICharacter>() ?? throw new InvalidOperationException("Missing backing data store on player spawn event.");

            Keywords = new string[] { ch.Name.ToLower(), ch.SurName.ToLower() };

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(ch);
                Birthdate = DateTime.Now;
            }

            var spawnTo = position?.CurrentLocation ?? GetBaseSpawn();

            if (position == null)
                position = new GlobalPosition(spawnTo);

            //Set the data context's stuff too so we don't have to do this over again
            ch.CurrentLocation = position;
            ch.Save(ch.Account, StaffRank.Player); //characters/players dont actually need approval

            spawnTo.MoveInto<IPlayer>(this);

            UpsertToLiveWorldCache(true);
        }

        /// <summary>
        /// Save this to the filesystem in Current
        /// </summary>
        /// <returns>Success</returns>
        internal override bool Save()
        {
            try
            {
                var dataAccessor = new PlayerData();
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
        private ILocation GetBaseSpawn()
        {
            var chr = DataTemplate<ICharacter>(); ;

            var zoneId = chr.StillANoob ? chr.RaceData.StartingLocation.Id : chr.RaceData.EmergencyLocation.Id;

            return LiveCache.Get<Zone>(zoneId);
        }
        #endregion
    }
}
