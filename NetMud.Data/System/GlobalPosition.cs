using NetMud.DataStructure.Behaviors.Existential;
using System;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Rendering;

namespace NetMud.Data.System
{
    [Serializable]
    public class GlobalPosition : IGlobalPosition
    {
        public IZone CurrentZone { get; set; }
        public IContains CurrentLocation { get; set; }
    }
}
