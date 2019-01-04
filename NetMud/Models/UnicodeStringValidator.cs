using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NetMud.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class UnicodeCharacterValidator : StringLengthAttribute
    {
        public bool Optional { get; set; }

        public UnicodeCharacterValidator() : base(1)
        {
            MinimumLength = 1;
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return !Optional;

            bool returnValue = true;

            byte[] bytes = Encoding.UTF32.GetBytes(value.ToString());

            if (bytes.Length > 4)
                returnValue = false;

            return returnValue;
        }
    }
}