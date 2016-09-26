using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;

namespace NetMud.DataAccess.FileSystem
{
    public class PlayerData : FileAccessor
    {
        /// <summary>
        /// Root directory where all the backup stuff gets saved too
        /// </summary>
        public override string BaseDirectory
        {
            get
            {
                return HostingEnvironment.MapPath(base.BaseDirectory + "Players/");
            }
        }

        /// <summary>
        /// The default directory name for when files are rolled over or archived
        /// </summary>
        public override string DatedBackupDirectory
        {
            get
            {
                return String.Format("{0}{1}{2}_{3}{4}{5}/",
                                        DateTime.Now.Year
                                        , DateTime.Now.Month
                                        , DateTime.Now.Day
                                        , DateTime.Now.Hour
                                        , DateTime.Now.Minute
                                        , DateTime.Now.Second);
            }
        }

        /// <summary>
        /// Writes one player out to disk
        /// </summary>
        /// <param name="player">the player to write</param>
        /// <param name="checkDirectories">skip checking directories or not</param>
        /// <returns></returns>
        public bool WriteOnePlayer(IPlayer entity)
        {
            var playersDir = BaseDirectory;

            if (!VerifyDirectory(playersDir))
                throw new Exception("Players directory unable to be verified or created during full backup.");

            LoggingUtility.Log("Backing up player character " + entity.DataTemplateId + ".", LogChannels.Backup, true);

            try
            {
                var charData = entity.DataTemplate<ICharacter>();

                var currentDirName = playersDir + charData.AccountHandle + "/" + CurrentDirectoryName + charData.ID;
                var archiveDirName = playersDir + charData.AccountHandle + "/" + ArchiveDirectoryName + charData.ID;

                //Wipe out the existing one so we can create all new files
                if (VerifyDirectory(currentDirName, false))
                {
                    var currentRoot = new DirectoryInfo(currentDirName);

                    if (!VerifyDirectory(archiveDirName))
                        return false;

                    //move is literal move, no need to delete afterwards
                    currentRoot.MoveTo(archiveDirName + DatedBackupDirectory);
                }

                DirectoryInfo entityDirectory = Directory.CreateDirectory(currentDirName);

                WritePlayer(entityDirectory, entity);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Restores one character from their Current backup
        /// </summary>
        /// <param name="accountHandle">Global Account Handle for the account</param>
        /// <param name="charID">Which character to load</param>
        /// <returns></returns>
        public IPlayer RestorePlayer(string accountHandle, long charID)
        {
            IPlayer newPlayerToLoad = null;

            try
            {
                var currentBackupDirectory = BaseDirectory + accountHandle + "/" + CurrentDirectoryName + charID.ToString();

                //No backup directory? No live data.
                if (!VerifyDirectory(currentBackupDirectory, false))
                    return null;

                var playerDirectory = new DirectoryInfo(currentBackupDirectory);

                var playerFilePath = playerDirectory + GetPlayerFilename(charID);

                var fileData = ReadCurrentFileByPath(playerFilePath);

                //no player file to load, derp
                if (fileData.Length == 0)
                    return null;

                var blankEntity = Activator.CreateInstance(typeof(IPlayer)) as IPlayer;
                newPlayerToLoad = (IPlayer)blankEntity.FromBytes(fileData);

                //bad load, dump it
                if (newPlayerToLoad == null)
                    return null;

                //abstract this out to a helper maybe?
                var locationAssembly = Assembly.GetAssembly(typeof(ILocation));

                var ch = newPlayerToLoad.DataTemplate<ICharacter>();
                if (ch.LastKnownLocationType == null)
                    ch.LastKnownLocationType = typeof(IRoom).Name;

                var lastKnownLocType = locationAssembly.DefinedTypes.FirstOrDefault(tp => tp.Name.Equals(ch.LastKnownLocationType));

                ILocation lastKnownLoc = null;
                if (lastKnownLocType != null && !string.IsNullOrWhiteSpace(ch.LastKnownLocation))
                {
                    if (lastKnownLocType.GetInterfaces().Contains(typeof(ISpawnAsSingleton)))
                    {
                        long lastKnownLocID = long.Parse(ch.LastKnownLocation);
                        lastKnownLoc = LiveCache.Get<ILocation>(lastKnownLocID, lastKnownLocType);
                    }
                    else
                    {
                        var cacheKey = new LiveCacheKey(lastKnownLocType, ch.LastKnownLocation);
                        lastKnownLoc = LiveCache.Get<ILocation>(cacheKey);
                    }
                }

                newPlayerToLoad.CurrentLocation = lastKnownLoc;

                //We have the player in live cache now so make it move to the right place
                newPlayerToLoad.GetFromWorldOrSpawn();
                newPlayerToLoad.UpsertToLiveWorldCache();

                //We'll need one of these per container on players
                if (Directory.Exists(playerDirectory + "Inventory/"))
                {
                    var inventoryDirectory = new DirectoryInfo(playerDirectory + "Inventory/");

                    foreach (var file in inventoryDirectory.EnumerateFiles())
                    {
                        var blankObject = Activator.CreateInstance(typeof(IInanimate)) as IInanimate;

                        var newObj = (IInanimate)blankObject.FromBytes(ReadFile(file));
                        newObj.UpsertToLiveWorldCache();
                        newPlayerToLoad.MoveInto(newObj);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return newPlayerToLoad;
        }

        /// <summary>
        /// Dumps everything of a single type into the cache from the filesystem for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>full or partial success</returns>
        public bool LoadAllCharactersForAccountToCache(string accountHandle)
        {
            var currentBackupDirectory = BaseDirectory + accountHandle + "/" + CurrentDirectoryName;

            //No current directory? WTF
            if (!VerifyDirectory(currentBackupDirectory, false))
                return false;

            var charDirectory = new DirectoryInfo(currentBackupDirectory);

            foreach (var file in charDirectory.EnumerateFiles("*.character", SearchOption.AllDirectories))
            {
                try
                {
                    var fileData = ReadFile(file);
                    var blankEntity = Activator.CreateInstance("NetMud.Data", "NetMud.Data.EntityBackingData.Character");

                    var objRef = blankEntity.Unwrap() as ICharacter;

                    var newChar = objRef.FromBytes(fileData) as ICharacter;

                    PlayerDataCache.Add(newChar);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                    //Let it keep going
                }
            }

            return true;
        }

        /// <summary>
        /// Writes one player out (and only one character) and their inventory to Current and archives whatever used to be Current
        /// </summary>
        /// <param name="dir">Directory to write to</param>
        /// <param name="entity">The player to write</param>
        private void WritePlayer(DirectoryInfo dir, IPlayer entity)
        {
            var entityFileName = GetPlayerFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dir.FullName + entityFileName;

            try
            {
                WriteToFile(fullFileName, entity.ToBytes());
                var liveDataWrapper = new LiveData();

                //We also need to write out all the inventory
                foreach (var obj in entity.Inventory.EntitiesContained())
                {
                    var baseTypeName = "Inventory";

                    DirectoryInfo entityDirectory;

                    //Is there a directory for this entity type? If not, then create it
                    if (!Directory.Exists(dir.FullName + baseTypeName))
                        entityDirectory = Directory.CreateDirectory(dir.FullName + baseTypeName);
                    else
                        entityDirectory = new DirectoryInfo(dir.FullName + baseTypeName);

                    liveDataWrapper.WriteSpecificEntity(entityDirectory, obj);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Write one character to its player current data
        /// </summary>
        /// <param name="entity">the char to write</param>
        public void WriteCharacter(ICharacter entity)
        {
            var dirName = BaseDirectory + entity.AccountHandle + "/" + CurrentDirectoryName;

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create base player directory.");

            var entityFileName = GetCharacterFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dirName + entityFileName;
            var archiveFileDirectory = BaseDirectory + entity.AccountHandle + "/" + ArchiveDirectoryName + DatedBackupDirectory;

            try
            {
                ArchiveCharacter(entity);
                WriteToFile(fullFileName, entity.ToBytes());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Archive a character
        /// </summary>
        /// <param name="entity">the thing to archive</param>
        public void ArchiveCharacter(ICharacter entity)
        {
            var dirName = BaseDirectory + entity.AccountHandle + "/" + CurrentDirectoryName;

            if (!VerifyDirectory(dirName))
                throw new Exception("Unable to locate or create current player directory.");

            var entityFileName = GetCharacterFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dirName + entityFileName;
            var archiveFileDirectory = BaseDirectory + entity.AccountHandle + "/" + ArchiveDirectoryName + DatedBackupDirectory;

            if (!VerifyDirectory(archiveFileDirectory))
                throw new Exception("Unable to locate or create archive player directory.");

            if (!File.Exists(fullFileName))
                return;

            try
            {
                var archiveFile = archiveFileDirectory + entityFileName;

                if (File.Exists(archiveFile))
                    File.Delete(archiveFile);

                File.Move(fullFileName, archiveFile);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Gets the statically formatted filename for a player
        /// </summary>
        /// <param name="entity">The player in question</param>
        /// <returns>the filename</returns>
        private string GetPlayerFilename(IPlayer entity)
        {
            return GetPlayerFilename(entity.DataTemplateId);
        }

        private string GetPlayerFilename(long charId)
        {
            return string.Format("{0}.player", charId);
        }

        /// <summary>
        /// Gets the statically formatted filename for a player
        /// </summary>
        /// <param name="entity">The player in question</param>
        /// <returns>the filename</returns>
        private string GetCharacterFilename(ICharacter entity)
        {
            return string.Format("{0}.character", entity.ID);
        }

    }
}
