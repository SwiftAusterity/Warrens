using NetMud.Data.DataIntegrity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Celestial bodies
    /// </summary>
    [Serializable]
    public class Celestial : LookupDataPartial, ICelestial
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// Orbit Type
        /// </summary>
        public CelestialOrientation OrientationType { get; set; }

        /// <summary>
        /// Zenith distance of an elliptical orbit
        /// </summary>
        [IntDataIntegrity("Apogee must be between 0 and 10000.", -1, 10001)]
        public int Apogee { get; set; }

        /// <summary>
        /// Minimal distance of an elliptical orbit
        /// </summary>
        [IntDataIntegrity("Perigree must be between 0 and 10000.", -1, 10001)]
        public int Perigree { get; set; }

        /// <summary>
        /// How fast is this going through space
        /// </summary>
        [IntDataIntegrity("Perigree must be between 0 and 10000.", -1, 10001)]
        public int Velocity { get; set; }

        /// <summary>
        /// How bright is this thing
        /// </summary>
        [IntDataIntegrity("Luminosity must be between 0 and 10000.", -10001, 10001)]
        public int Luminosity { get; set; }

        /// <summary>
        /// Physical model for the celestial object
        /// </summary>
        [NonNullableDataIntegrity("Physical model is invalid.")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Celestial Orientation", OrientationType.ToString());
            returnList.Add("Apogee", Apogee.ToString());
            returnList.Add("Perigree", Perigree.ToString());
            returnList.Add("Velocity", Velocity.ToString());
            returnList.Add("Luminosity", Luminosity.ToString());

            return returnList;
        }

    }
}
