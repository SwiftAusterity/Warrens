using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;

namespace NetMud.LiveData
{
    /// <summary>
    /// The engine behind the system that constantly writes out live data so we can reboot into the prior state if needs be
    /// BaseDirectory should end with a trailing slash
    /// </summary>
    public class HotBackup
    {
        public LiveCache LiveWorld { get; private set; }
        public string BaseDirectory { get; private set; }

        public HotBackup(string baseDirectory)
        {
            LiveWorld = new LiveCache();
            BaseDirectory = baseDirectory;
        }

        public bool WriteLiveBackup()
        {
            //No backup directory? Make it
            if (!Directory.Exists(BaseDirectory))
                Directory.CreateDirectory(BaseDirectory);

            //Get all the entities (which should be a ton of stuff)
            var entities = LiveWorld.GetAll();

            //Dont save players to the hot section
            foreach(var entity in entities.Where(ent => !ent.GetType().GetInterfaces().Contains(typeof(IPlayer))))
            {
                var baseTypeName = entity.GetType().Name;

                DirectoryInfo entityDirectory;

                //Is there a directory for this entity type? If not, then create it
                if (!Directory.Exists(HostingEnvironment.MapPath(BaseDirectory + baseTypeName)))
                    entityDirectory = Directory.CreateDirectory(BaseDirectory + baseTypeName);
                else
                    entityDirectory = new DirectoryInfo(BaseDirectory + baseTypeName);

                WriteEntity(entityDirectory, entity);
            }

            return true;
        }

        private void WriteEntity(DirectoryInfo dir, IEntity entity)
        {
            var entityFileName = GetEntityFilename(entity);

            if(String.IsNullOrWhiteSpace(entityFileName))
                return;

            var fullFileName = dir.FullName + "/" + entityFileName;

            FileStream entityFile = null;

            try
            {
                if (File.Exists(fullFileName))
                    entityFile = File.Open(fullFileName, FileMode.Truncate);
                else
                    entityFile = File.Create(fullFileName);

                var bytes = entity.Serialize();
                entityFile.Write(bytes, 0, bytes.Length);

                //Don't forget to write the file out
                entityFile.Flush();
            }
            catch
            {
                //boogey boogey
            }
            finally
            {
                //dont not do this everEVERVERRFCFEVVEEV
                if(entityFile != null)
                    entityFile.Dispose();
            }
        }

        private string GetEntityFilename(IEntity entity)
        {
             return String.Format("{0}.{1}", entity.BirthMark, entity.GetType().Name);
        }

        public bool RestoreLiveBackup()
        {
            //No backup directory? No live data.
            if (!Directory.Exists(BaseDirectory))
                return false;

            return false;
        }

        public bool NewWorldFallback()
        {
            LiveWorld.PreLoadAll<RoomData>();
            LiveWorld.PreLoadAll<PathData>();

            return true;
        }
    }
}
