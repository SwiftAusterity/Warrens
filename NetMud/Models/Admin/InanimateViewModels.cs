﻿using NetMud.Authentication;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class ManageInanimateTemplateViewModel : PagedDataModel<IInanimateTemplate>, IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public ManageInanimateTemplateViewModel(IEnumerable<IInanimateTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IInanimateTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }
    }

    public class AddEditInanimateTemplateViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditInanimateTemplateViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name", Description = "The descriptive name of the object.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Inanimate Containers", Description = "The pockets of space this object has to store objects.")]
        public string[] InanimateContainerNames { get; set; }

        [Display(Name = "Inanimate Container Weights", Description = "The weight capacity of this pocket.")]
        public long[] InanimateContainerWeights { get; set; }

        [Display(Name = "Inanimate Container Volumes", Description = "The volume capacity of this pocket.")]
        public long[] InanimateContainerVolumes { get; set; }

        [Display(Name = "Character Containers", Description = "The pockets of space this object has to store players and NPCs.")]
        public string[] MobileContainerNames { get; set; }

        [Display(Name = "Character Container Weights", Description = "The weight capacity of this pocket.")]
        public long[] MobileContainerWeights { get; set; }

        [Display(Name = "Character Container Volumes", Description = "The volume capacity of this pocket.")]
        public long[] MobileContainerVolumes { get; set; }

        [Display(Name = "Composition", Description = "What other objects this is made of internally. (like a sword that has a dagger in the hilt)")]
        public long[] InternalCompositionIds { get; set; }

        [Display(Name = "Percentage", Description = "The percentage of the total object body this specific object comprises of the whole.")]
        public short[] InternalCompositionPercentages { get; set; }

        public IEnumerable<IInanimateTemplate> ValidInanimateTemplates { get; set; }

        public IInanimateTemplate DataObject { get; set; }
    }
}