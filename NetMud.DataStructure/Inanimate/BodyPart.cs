using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Inanimate
{
    [Serializable]
    public class BodyPart
    {
        [UIHint("IndividualInanimateComponent")]
        [Display(Name = "Body Part", Description = "The # of and object the part is made up of.")]
        public IInanimateComponent Part { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Name", Description = "The name of the body part.")]
        public string Name { get; set; }

        public BodyPart()
        {
        }

        public BodyPart(IInanimateComponent item, string name)
        {
            Part = item;
            Name = name;
        }
    }
}
