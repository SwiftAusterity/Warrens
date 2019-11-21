using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
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
        /// The "user" level for commands and accessibility
        /// </summary>
        public StaffRank GamePermissionsRank { get; set; }

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
        /// News up an empty entity
        /// </summary>
        public Player()
        {
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="character">the backing data</param>
        public Player(IPlayerTemplate character)
        {
            TemplateId = character.Id;
            AccountHandle = character.AccountHandle;
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

            return Descriptor.SendOutput(strings);
        }
        #endregion
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPlayer GetLiveInstance()
        {
            return this;
        }

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
                GamePermissionsRank = me.GamePermissionsRank;
            }
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld()
        {
            //We can't even try this until we know if the data is there
            IPlayerTemplate ch = Template<IPlayerTemplate>() ?? throw new InvalidOperationException("Missing backing data store on player spawn event.");

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(ch);
                Birthdate = DateTime.Now;
            }

            GamePermissionsRank = ch.GamePermissionsRank;

            //Set the data context's stuff too so we don't have to do this over again
            ch.Save(ch.Account, StaffRank.Player); //characters/players dont actually need approval

            UpsertToLiveWorldCache(true);

            KickoffProcesses();

            Save();
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

        public override object Clone()
        {
            throw new NotImplementedException("Can't clone player objects.");
        }
        #endregion
    }
}
