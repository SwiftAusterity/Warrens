using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MarkdownStringLengthValidator : StringLengthAttribute
    {
        public bool Optional { get; set; }

        public MarkdownStringLengthValidator() : base(5000)
        {
            MinimumLength = 20;
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return !Optional;
            }

            MarkdownString mdString = (MarkdownString)value;

            return mdString.Length >= MinimumLength && mdString.Length <= MaximumLength;
        }
    }
}