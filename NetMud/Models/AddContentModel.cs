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
    public abstract class AddEditTemplateModel<T> where T : IKeyedData
    {
        public T DataTemplate { get; set; }
        public IEnumerable<T> ValidTemplateBases { get; set; }

        public abstract T Template { get; set; }

        public string ArchivePath { get; set; }
        public string[] Archives { get; set; }

        public AddEditTemplateModel(long templateId)
        {
            DataTemplate = TemplateCache.Get<T>(templateId);
            ValidTemplateBases = TemplateCache.GetAll<T>(true);
            Archives = new string[0];
        }

        public AddEditTemplateModel(string archivePath, T item)
        {
            TemplateData fileAccessor = new TemplateData();

            DataTemplate = default;
            ValidTemplateBases = TemplateCache.GetAll<T>(true);
            Archives = GetArchiveNames(fileAccessor);

            ArchivePath = archivePath;

            if (!string.IsNullOrWhiteSpace(ArchivePath))
            {
                GetArchivedTemplate(fileAccessor, item);
            }
        }

        internal void GetArchivedTemplate(TemplateData fileAccessor, T item)
        {
            var typeName = typeof(T).Name;
            Type templateType = typeof(T);

            if (typeof(T).IsInterface)
            {
                typeName = typeName.Substring(1);
                templateType = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(typeof(T)));
            }

            DirectoryInfo archiveDir = new DirectoryInfo(fileAccessor.BaseDirectory + fileAccessor.ArchiveDirectoryName + ArchivePath + "/" + typeName + "/");

            var potentialFiles = archiveDir.GetFiles(item.Id + "." + typeName);

            if(potentialFiles.Any())
            {
                DataTemplate = (T)fileAccessor.ReadEntity(potentialFiles.First(), templateType);
            }
        }

        internal string[] GetArchiveNames(TemplateData fileAccessor)
        {
            DirectoryInfo filesDirectory = new DirectoryInfo(fileAccessor.BaseDirectory + fileAccessor.ArchiveDirectoryName);

            var typeName = typeof(T).Name;

            if(typeof(T).IsInterface)
            {
                typeName = typeName.Substring(1);
            }

            return filesDirectory.EnumerateDirectories().Where(dir => dir.GetDirectories(typeName, SearchOption.TopDirectoryOnly).Any())
                                                        .Select(dir => dir.Name).ToArray();
        }
    }
}