using NetMud.Data.Architectural.Serialization;
using Newtonsoft.Json;
using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class AnatomicalPart
    {
        public string Name { get; set; }

        [JsonConverter(typeof(DescriptiveEnumConverter<OverallStatus>))]
        public OverallStatus Overall { get; set; }

        public string[] Wounds { get; set; }
    }
}
