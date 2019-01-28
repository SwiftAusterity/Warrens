using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Player;
using System;
using System.IO;
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
                return string.Format("{0}{1}{2}_{3}{4}{5}/",
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
            string playersDir = BaseDirectory;

            if (!VerifyDirectory(playersDir))
            {
                throw new Exception("Players directory unable to be verified or created during full backup.");
            }

            LoggingUtility.Log("Backing up player character " + entity.TemplateId + ".", LogChannels.Backup, true);

            try
            {
                IPlayerTemplate charData = entity.Template<IPlayerTemplate>();

                string currentDirName = playersDir + charData.AccountHandle + "/" + CurrentDirectoryName + charData.Id + "/";
                string archiveDirName = playersDir + charData.AccountHandle + "/" + ArchiveDirectoryName + charData.Id + "/";

                //Wipe out the existing one so we can create all new files
                if (VerifyDirectory(currentDirName, false))
                {
                    DirectoryInfo currentRoot = new DirectoryInfo(currentDirName);

                    if (!VerifyDirectory(archiveDirName))
                    {
                        return false;
                    }

                    //Already exists?
                    if (VerifyDirectory(archiveDirName + DatedBackupDirectory))
                    {
                        return false;
                    }

                    //move is literal move, no need to delete afterwards
                    currentRoot.MoveTo(archiveDirName + DatedBackupDirectory);
                }

                DirectoryInfo entityDirectory = Directory.CreateDirectory(currentDirName);

                WritePlayer(entityDirectory, entity);
                WriteCharacter(charData);
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
        public IPlayer RestorePlayer(string accountHandle, IPlayerTemplate chr)
        {
            IPlayer newPlayerToLoad = null;

            try
            {
                string currentBackupDirectory = BaseDirectory + accountHandle + "/" + CurrentDirectoryName + chr.Id.ToString() + "/";

                //No backup directory? No live data.
                if (!VerifyDirectory(currentBackupDirectory, false))
                {
                    return null;
                }

                DirectoryInfo playerDirectory = new DirectoryInfo(currentBackupDirectory);

                byte[] fileData = new byte[0];
                using (FileStream stream = File.Open(currentBackupDirectory + GetPlayerFilename(chr.Id), FileMode.Open))
                {
                    fileData = new byte[stream.Length];
                    stream.Read(fileData, 0, (int)stream.Length);
                }

                //no player file to load, derp
                if (fileData.Length == 0)
                {
                    return null;
                }

                IPlayer blankEntity = Activator.CreateInstance(chr.EntityClass) as IPlayer;
                newPlayerToLoad = (IPlayer)blankEntity.FromBytes(fileData);

                //bad load, dump it
                if (newPlayerToLoad == null)
                {
                    return null;
                }

                //We have the player in live cache now so make it move to the right place
                newPlayerToLoad.GetFromWorldOrSpawn();
                newPlayerToLoad.UpsertToLiveWorldCache(true);

                //We'll need one of these per container on players
                if (Directory.Exists(playerDirectory + "Inventory/"))
                {
                    DirectoryInfo inventoryDirectory = new DirectoryInfo(playerDirectory + "Inventory/");

                    foreach (FileInfo file in inventoryDirectory.EnumerateFiles())
                    {
                        IInanimate blankObject = Activator.CreateInstance("NetMud.Data", "NetMud.Data.Game.Inanimate") as IInanimate;

                        IInanimate newObj = (IInanimate)blankObject.FromBytes(ReadFile(file));
                        newObj.UpsertToLiveWorldCache(true);
                        newObj.TryMoveTo(newPlayerToLoad.GetContainerAsLocation());
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
            string currentBackupDirectory = BaseDirectory + accountHandle + "/" + CurrentDirectoryName;

            //No current directory? WTF
            if (!VerifyDirectory(currentBackupDirectory, false))
            {
                return false;
            }

            DirectoryInfo charDirectory = new DirectoryInfo(currentBackupDirectory);

            foreach (FileInfo file in charDirectory.EnumerateFiles("*.playertemplate", SearchOption.AllDirectories))
            {
                try
                {
                    byte[] fileData = ReadFile(file);
                    System.Runtime.Remoting.ObjectHandle blankEntity = Activator.CreateInstance("NetMud.Data", "NetMud.Data.Players.PlayerTemplate");

                    IPlayerTemplate objRef = blankEntity.Unwrap() as IPlayerTemplate;

                    IPlayerTemplate newChar = objRef.FromBytes(fileData) as IPlayerTemplate;

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
            string entityFileName = GetPlayerFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
            {
                return;
            }

            string fullFileName = dir.FullName + entityFileName;

            try
            {
                WriteToFile(fullFileName, entity.ToBytes());
                LiveData liveDataWrapper = new LiveData();

                //We also need to write out all the inventory
                foreach (IInanimate obj in entity.Inventory.EntitiesContained())
                {
                    string baseTypeName = "Inventory";

                    DirectoryInfo entityDirectory;

                    //Is there a directory for this entity type? If not, then create it
                    if (!Directory.Exists(dir.FullName + baseTypeName))
                    {
                        entityDirectory = Directory.CreateDirectory(dir.FullName + baseTypeName);
                    }
                    else
                    {
                        entityDirectory = new DirectoryInfo(dir.FullName + baseTypeName);
                    }

                    liveDataWrapper.WriteSpecificEntity(entityDirectory, obj);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }
        }

        /// <summary>
        /// Write one character to its player current data
        /// </summary>
        /// <param name="entity">the char to write</param>
        public void WriteCharacter(IPlayerTemplate entity)
        {
            string dirName = BaseDirectory + entity.AccountHandle + "/" + CurrentDirectoryName + entity.Id + "/";

            if (!VerifyDirectory(dirName))
            {
                throw new Exception("Unable to locate or create base player directory.");
            }

            string entityFileName = GetCharacterFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
            {
                return;
            }

            string fullFileName = dirName + entityFileName;
            string archiveFileDirectory = BaseDirectory + entity.AccountHandle + "/" + ArchiveDirectoryName + entity.Id + "/" + DatedBackupDirectory;

            try
            {
                ArchiveCharacter(entity);
                WriteToFile(fullFileName, entity.ToBytes());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }
        }

        /// <summary>
        /// Archive a character
        /// </summary>
        /// <param name="entity">the thing to archive</param>
        public void ArchiveCharacter(IPlayerTemplate entity)
        {
            string dirName = BaseDirectory + entity.AccountHandle + "/" + CurrentDirectoryName + entity.Id + "/";

            if (!VerifyDirectory(dirName))
            {
                throw new Exception("Unable to locate or create current player directory.");
            }

            string entityFileName = GetCharacterFilename(entity);

            if (string.IsNullOrWhiteSpace(entityFileName))
            {
                return;
            }

            string fullFileName = dirName + entityFileName;

            if (!File.Exists(fullFileName))
            {
                return;
            }

            string archiveFileDirectory = BaseDirectory + entity.AccountHandle + "/" + ArchiveDirectoryName + entity.Id + "/" + DatedBackupDirectory;

            if (!VerifyDirectory(archiveFileDirectory))
            {
                throw new Exception("Unable to locate or create archive player directory.");
            }

            CullDirectoryCount(BaseDirectory + entity.AccountHandle + "/" + ArchiveDirectoryName + entity.Id + "/");

            try
            {
                string archiveFile = archiveFileDirectory + entityFileName;

                if (File.Exists(archiveFile))
                {
                    File.Delete(archiveFile);
                }

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
            return GetPlayerFilename(entity.TemplateId);
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
        private string GetCharacterFilename(IPlayerTemplate entity)
        {
            return string.Format("{0}.playertemplate", entity.Id);
        }

    }
}
