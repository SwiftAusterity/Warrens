using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.Models.Admin
{ 
    public class AddEditDecayEventViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditDecayEventViewModel() 
        {
            ValidInanimateDatas = TemplateCache.GetAll<IInanimateTemplate>(true);
            ValidTileDatas = TemplateCache.GetAll<ITileTemplate>(true);
            ValidNPCDatas = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true);
        }

        public IEnumerable<INonPlayerCharacterTemplate> ValidNPCDatas { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateDatas { get; set; }
        public IEnumerable<ITileTemplate> ValidTileDatas { get; set; }

        public string ClassType { get; set; }
        public IKeyedData ParentObject { get; set; }
        public IDecayEvent DataObject { get; set; }
    }

    public class AddEditInteractionViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditInteractionViewModel()
        {
            ValidInanimateDatas = TemplateCache.GetAll<IInanimateTemplate>(true);
            ValidTileDatas = TemplateCache.GetAll<ITileTemplate>(true);
            ValidNPCDatas = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true);
        }

        public IEnumerable<INonPlayerCharacterTemplate> ValidNPCDatas { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateDatas { get; set; }
        public IEnumerable<ITileTemplate> ValidTileDatas { get; set; }

        public string ClassType { get; set; }
        public IKeyedData ParentObject { get; set; }
        public IInteraction DataObject { get; set; }
    }

    public class AddEditUseViewModel : IBaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public AddEditUseViewModel()
        {
            ValidInanimateDatas = TemplateCache.GetAll<IInanimateTemplate>(true);
            ValidTileDatas = TemplateCache.GetAll<ITileTemplate>(true);
            ValidNPCDatas = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true);
        }

        public IEnumerable<INonPlayerCharacterTemplate> ValidNPCDatas { get; set; }
        public IEnumerable<IInanimateTemplate> ValidInanimateDatas { get; set; }
        public IEnumerable<ITileTemplate> ValidTileDatas { get; set; }

        public string ClassType { get; set; }
        public IKeyedData ParentObject { get; set; }
        public IUse DataObject { get; set; }
    }
}