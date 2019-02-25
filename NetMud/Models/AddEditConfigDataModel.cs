using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetMud.Models
{
    public abstract class AddEditConfigDataModel<T> where T : IConfigData
    {
        public T DataTemplate { get; set; }
        public IEnumerable<T> ValidTemplateBases { get; set; }

        public abstract T Template { get; set; }

        public string ArchivePath { get; set; }
        public string[] Archives { get; set; }

        public AddEditConfigDataModel(string uniqueKey)
        {
            DataTemplate = ConfigDataCache.Get<T>(uniqueKey);
            ValidTemplateBases = ConfigDataCache.GetAll<T>();
            Archives = new string[0];
        }

        public AddEditConfigDataModel(string archivePath, T item)
        {
            ConfigData fileAccessor = new ConfigData();

            DataTemplate = default;
            ValidTemplateBases = ConfigDataCache.GetAll<T>();
            Archives = GetArchiveNames(fileAccessor);

            ArchivePath = archivePath;

            if (!string.IsNullOrWhiteSpace(ArchivePath))
            {
                GetArchivedTemplate(fileAccessor, item);
            }
        }

        internal void GetArchivedTemplate(ConfigData fileAccessor, T item)
        {
            var typeName = typeof(T).Name;
            Type templateType = typeof(T);

            if (typeof(T).IsInterface)
            {
                typeName = typeName.Substring(1);
                templateType = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(typeof(T)));
            }

            DirectoryInfo archiveDir = new DirectoryInfo(fileAccessor.BaseDirectory + fileAccessor.ArchiveDirectoryName + ArchivePath + "/");

            var potentialFiles = archiveDir.GetFiles(item.UniqueKey + "." + typeName);

            if (potentialFiles.Any())
            {
                DataTemplate = (T)fileAccessor.ReadEntity(potentialFiles.First(), templateType);
            }
        }

        internal string[] GetArchiveNames(ConfigData fileAccessor)
        {
            DirectoryInfo filesDirectory = new DirectoryInfo(fileAccessor.BaseDirectory + fileAccessor.ArchiveDirectoryName);

            var typeName = typeof(T).Name;

            if (typeof(T).IsInterface)
            {
                typeName = typeName.Substring(1);
            }

            return filesDirectory.EnumerateDirectories().Where(dir => dir.GetFiles("*." + typeName, SearchOption.TopDirectoryOnly).Any())
                                                        .Select(dir => dir.Name).ToArray();
        }
    }

}