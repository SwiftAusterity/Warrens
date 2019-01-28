using NetMud.DataStructure.NPC;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MerchandiseValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public MerchandiseValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            IMerchandise item = value as IMerchandise;

            if (item == null)
            {
                return !Optional;
            }

            return item.Item != null && item.QualityRange.Low <= item.QualityRange.High;
        }
    }
}