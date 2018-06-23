using NetMud.Data.ConfigData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.Database;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace NetMud.Data.System
{
    /// <summary>
    /// User account
    /// </summary>
    public class Account : IAccount
    {
        /// <summary>
        /// Unique identifier AND forum/chat handle for player's user account
        /// </summary>
        [Key]
        public string GlobalIdentityHandle { get; set; }

        /// <summary>
        /// New up a blank account
        /// </summary>
        public Account()
        {
            GlobalIdentityHandle = "Nobody";
            LogChannelSubscriptions = new List<string>();
        }

        /// <summary>
        /// New up an account with the GlobalIdentityHandle
        /// </summary>
        /// <param name="handle">GlobalIdentityHandle</param>
        public Account(string handle)
        {
            GlobalIdentityHandle = handle;
            LogChannelSubscriptions = new List<string>();

            var forceLoad = Config;
        }

        /// <summary>
        /// New up an account with the GlobalIdentityHandle and the log streams it wants to subscribe to
        /// </summary>
        /// <param name="handle">GlobalIdentityHandle</param>
        /// <param name="logSubscriptions">| delimeted list of log channel names</param>
        public Account(string handle, string logSubscriptions)
        {
            GlobalIdentityHandle = handle;

            if (!string.IsNullOrEmpty(logSubscriptions))
                LogChannelSubscriptions = logSubscriptions.Split('|');
            else
                LogChannelSubscriptions = new List<string>();

            var forceLoad = Config;
        }

        /// <summary>
        /// Id for the currently selected character for the account to log into the game as
        /// </summary>
        public long CurrentlySelectedCharacter { get; set; }

        /// <summary>
        /// List of log channel names subscribed to
        /// </summary>
        public IList<string> LogChannelSubscriptions { get; set; }

        /// <summary>
        /// For EF purposes
        /// </summary>
        public string LogSubs
        {
            get { return string.Join(",", LogChannelSubscriptions); }
            set { LogChannelSubscriptions = value.Split(',').ToList(); }
        }

        /// <summary>
        /// List of valid characters this account owns
        /// </summary>
        private IList<ICharacter> _characters;

        /// <summary>
        /// List of valid characters this account owns
        /// </summary>
        public IList<ICharacter> Characters
        {
            get
            {
                if (_characters == null)
                {
                    IEnumerable<ICharacter> returnValue = PlayerDataCache.GetAllForAccountHandle(GlobalIdentityHandle);
                    _characters = returnValue.ToList();
                }

                //Cause there are none
                if (_characters == null)
                    _characters = new List<ICharacter>();

                return _characters;
            }
            set
            {
                _characters = value.ToList();
            }
        }

        /// <summary>
        /// The config values for this account
        /// </summary>
        [JsonIgnore]
        public IAccountConfig Config
        {
            get
            {
                IAccountConfig returnValue = ConfigDataCache.Get<IAccountConfig>(new ConfigDataCacheKey(typeof(IAccountConfig), GlobalIdentityHandle, ConfigDataType.Player));

                if (returnValue == null)
                {
                    //Try and get it from the file
                    returnValue = new AccountConfig(this);

                    if (!returnValue.RestoreConfig())
                    {
                        //Just make it new and save it
                        returnValue = new AccountConfig(this)
                        {
                            UITutorialMode = true
                        };

                        returnValue.Save(this, DataStructure.SupportingClasses.StaffRank.Player); //personal config doesnt need approval yet but your rank is ALWAYS player here
                    }
                }

                return returnValue;
            }
            set
            {
                ConfigDataCache.Add(value);
            }

        }

        /// <summary>
        /// Add a character to this account
        /// </summary>
        /// <param name="newChar">the character data to add</param>
        /// <returns>errors or Empty if successful</returns>
        public string AddCharacter(ICharacter newChar)
        {
            IEnumerable<ICharacter> systemChars = PlayerDataCache.GetAll();

            if (systemChars.Any(ch => ch.Name.Equals(newChar.Name, StringComparison.InvariantCultureIgnoreCase) && newChar.SurName.Equals(newChar.SurName, StringComparison.InvariantCultureIgnoreCase)))
                return "A character with that name already exists, please choose another.";

            newChar.AccountHandle = GlobalIdentityHandle;
            newChar.Create(this, DataStructure.SupportingClasses.StaffRank.Player); //characters dont need approval yet but your rank is ALWAYS player here

            Characters.Add(newChar);

            return string.Empty;
        }

        /// <summary>
        /// Get an account by its GlobalIdentityHandle
        /// </summary>
        /// <param name="handle">GlobalIdentityHandle to get</param>
        /// <returns>the account</returns>
        public static IAccount GetByHandle(string handle)
        {
            var parms = new Dictionary<string, object>();

            var sql = "select * from [dbo].[Accounts] where GlobalIdentityHandle = @handle";

            parms.Add("handle", handle);

            IAccount account = null;

            try
            {
                var ds = SqlWrapper.RunDataset(sql, CommandType.Text, parms);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                        account = Fill(dr);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, "AccountDatabaseFailures");
            }

            return account;
        }

        /// <summary>
        /// Fill an account from the database
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        /// <returns>the account</returns>
        private static IAccount Fill(DataRow dr)
        {
            string outHandle = DataUtility.GetFromDataRow<string>(dr, "GlobalIdentityHandle");
            string outLogSubs = DataUtility.GetFromDataRow<string>(dr, "LogChannelSubscriptions");

            return new Account(outHandle, outLogSubs);
        }


        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IAccount other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.GlobalIdentityHandle.Equals(GlobalIdentityHandle))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IAccount other)
        {
            if (other != default(IAccount))
            {
                try
                {
                    return other.GetType() == GetType() && other.GlobalIdentityHandle.Equals(GlobalIdentityHandle);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IAccount x, IAccount y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(IAccount obj)
        {
            return obj.GetType().GetHashCode() + obj.GlobalIdentityHandle.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + GlobalIdentityHandle.GetHashCode();
        }
        #endregion

    }
}
