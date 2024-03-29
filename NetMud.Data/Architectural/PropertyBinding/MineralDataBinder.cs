﻿using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.NaturalResource;

namespace NetMud.Data.Architectural.PropertyBinding
{
    public class MineralDataBinder : PropertyBinderAttribute
    {
        public override object Convert(object input)
        {
            string stringInput = input.ToString();
            if (string.IsNullOrWhiteSpace(stringInput))
            {
                return null;
            }

            return TemplateCache.Get<IMineral>(long.Parse(stringInput));
        }
    }
}
