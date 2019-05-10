using NetMud.Authentication;
using NetMud.DataStructure.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;

namespace NetMud.Models.Admin
{
    public class ManageGameViewModel : PagedDataModel<IGameTemplate>
    {
        public ManageGameViewModel(IEnumerable<IGameTemplate> items)
            : base(items)
        {
            CurrentPageNumber = 1;
            ItemsPerPage = 20;
        }

        internal override Func<IGameTemplate, bool> SearchFilter
        {
            get
            {
                return item => item.Name.ToLower().Contains(SearchTerms.ToLower()) || item.Name.ToLower().Contains(SearchTerms.ToLower());
            }
        }

        internal override Func<IGameTemplate, object> OrderPrimary
        {
            get
            {
                return item => item.Name;
            }
        }


        internal override Func<IGameTemplate, object> OrderSecondary
        {
            get
            {
                return item => item.CreatorHandle;
            }
        }
    }

    public class AddEditGameTemplateViewModel : IBaseViewModel
    {
        public ApplicationUser AuthedUser { get; set; }

        public AddEditGameTemplateViewModel()
        {
        }


        [Display(Name = "Lua Engine", Description = "The lua code that will be the game engine.")]
        [DataType(DataType.Upload)]
        public FileUpload LuaEngine { get; set; }

        [UIHint("GameTemplate")]
        public IGameTemplate DataObject { get; set; }
    }
}