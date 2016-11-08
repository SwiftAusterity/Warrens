using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Models.Admin
{
    public class EntityContainerViewModel
    {
        public EntityContainerViewModel(string name)
        {
            ContainerName = name;
            Containers = new HashSet<IEntityContainerData<IEntity>>();
        }

        public EntityContainerViewModel(HashSet<IEntityContainerData<IInanimate>> containers, string name)
        {
            ContainerName = name;
            Containers = new HashSet<IEntityContainerData<IEntity>>(containers.Select(c => c as IEntityContainerData<IEntity>));
        }

        public EntityContainerViewModel(HashSet<IEntityContainerData<IMobile>> containers, string name)
        {
            ContainerName = name;
            Containers = new HashSet<IEntityContainerData<IEntity>>(containers.Select(c => c as IEntityContainerData<IEntity>));
        }

        public EntityContainerViewModel(HashSet<IEntityContainerData<IPathway>> containers, string name)
        {
            ContainerName = name;
            Containers = new HashSet<IEntityContainerData<IEntity>>(containers.Select(c => c as IEntityContainerData<IEntity>));
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