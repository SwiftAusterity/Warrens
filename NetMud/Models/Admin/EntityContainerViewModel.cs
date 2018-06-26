using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public partial class EntityContainerViewModel<T> where T : IEntity
    {
        public EntityContainerViewModel()
        {
            Containers = new HashSet<IEntityContainerData<T>>();
        }

        public EntityContainerViewModel(string name, string friendlyName)
        {
            ContainerName = name;
            ContainerFriendlyName = friendlyName;
            Containers = new HashSet<IEntityContainerData<T>>();
        }

        public EntityContainerViewModel(HashSet<IEntityContainerData<T>> containers, string name, string friendlyName)
        {
            ContainerName = name;
            ContainerFriendlyName = friendlyName;
            Containers = containers;
        }

        public virtual string ContainerName { get; set; }

        public virtual string ContainerFriendlyName { get; set; }

        [Display(Name = "Name", Description = "The descriptive name of the pocket.")]
        public virtual string[] ContainerNames { get; set; }

        [Display(Name = "Weight Capacity", Description = "The total weight capacity of this pocket.")]
        public virtual long[] ContainerWeights { get; set; }

        [Display(Name = "Internal Volume", Description = "The total dimensional size of the pocket internally.")]
        public virtual long[] ContainerVolumes { get; set; }

        public virtual HashSet<IEntityContainerData<T>> Containers { get; set; }
    }
}