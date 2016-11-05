using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public class EntityContainerViewModel
    {
        public EntityContainerViewModel(string name)
        {
            ContainerName = name;
            Containers = new HashSet<IEntityContainerData<IEntity>>();
        }

        public EntityContainerViewModel(HashSet<IEntityContainerData<IEntity>> containers, string name)
        {
            ContainerName = name;
            Containers = containers;
        }

        public string ContainerName { get; set; }

        [Display(Name = "Name")]
        public string[] ContainerNames { get; set; }

        [Display(Name = "Weight Capacity")]
        public long[] ContainerWeights { get; set; }

        [Display(Name = "Internal Volume")]
        public long[] ContainerVolumes { get; set; }

        public HashSet<IEntityContainerData<IEntity>> Containers { get; set; }
    }
}