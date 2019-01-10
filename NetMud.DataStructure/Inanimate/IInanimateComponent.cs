using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Inanimate
{
    public interface IInanimateComponent
    {
        [Display(Name = "Component", Description = "A required component of the crafting recipe.")]
        [UIHint("InanimateTemplateList")]
        IInanimateTemplate Item { get; set; }

        [Display(Name = "Needed", Description = "Amount of the component item needed.")]
        [DataType(DataType.Text)]
        int Amount { get; set; }
    }
}
