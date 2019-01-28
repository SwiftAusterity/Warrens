using NetMud.DataStructure.Architectural.ActorBase;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.PropertyValidation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RaceValidator : ValidationAttribute
    {
        public bool Optional { get; set; }

        public RaceValidator() : base()
        {
            Optional = false;
        }

        public override bool IsValid(object value)
        {
            IRace item = value as IRace;

            if (item == null)
            {
                return !Optional;
            }

            return !string.IsNullOrWhiteSpace(item.CollectiveNoun) 
                && !string.IsNullOrWhiteSpace(item.Name);
        }
    }
}