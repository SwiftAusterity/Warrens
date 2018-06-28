using NetMud.Data.Serialization;
using Newtonsoft.Json;
using System;

namespace NetMud.Websock.OutputFormatting
{
    [Serializable]
    public class BodyStatus
    {
        [JsonConverter(typeof(DescriptiveEnumConverter<OverallStatus>))]
        public OverallStatus Overall { get; set; }
        public AnatomicalPart[] Anatomy { get; set; }
    }
}
