using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ShortValueRangeValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public ShortValueRangeValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            ValueRange<short> item = value as ValueRange<short>;

            if (item == null)
                return !Optional;

            return item.Low <= item.High;
        }
    }
}