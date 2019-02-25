using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural
{
    [Serializable]
    public class QualityValue
    {
        [Display(Name = "Quality", Description = "The name of a quality the entity will spawn with.")]
        [DataType(DataType.Text)]
        public string Quality { get; set; }

        [Display(Name = "Value", Description = "The value of the quality the entity will spawn with.")]
        [DataType(DataType.Text)]
        public int Value { get; set; }

        public QualityValue()
        {
            Value = -1;
        }

        public QualityValue(string quality, int value)
        {
            Quality = quality;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} ", Value, Quality);
        }
    }
}
