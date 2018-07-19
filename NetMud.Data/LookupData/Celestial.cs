using NetMud.Communication.Messaging;
using NetMud.Data.DataIntegrity;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [IntDataIntegrity("Apogee must be between 0 and 1,000,000.", 0, 1000000)]
        public int Apogee { get; set; }

        /// <summary>
        /// Minimal distance of an elliptical orbit
        /// </summary>
        [IntDataIntegrity("Perigree must be between 0 and 1,000,000.", 0, 1000000)]
        public int Perigree { get; set; }

        /// <summary>
        /// How fast is this going through space
        /// </summary>
        [IntDataIntegrity("Velocity must be between 0 and 10000.", 0, 10000)]
        public int Velocity { get; set; }

        /// <summary>
        /// How bright is this thing
        /// </summary>
        [IntDataIntegrity("Luminosity must be between 0 and 10000.", 0, 10000)]
        public int Luminosity { get; set; }

        /// <summary>
        /// Physical model for the celestial object
        /// </summary>
        [NonNullableDataIntegrity("Physical model is invalid.")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<IOccurrence> Descriptives { get; set; }

        public Celestial()
        {
            Descriptives = new HashSet<IOccurrence>();
        }

        internal IOccurrence GetSelf(MessagingType type, int strength = 100)
        {
            return new Occurrence()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = Name, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            };
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (!IsVisibleTo(viewer))
                return new Occurrence(MessagingType.Visible);

            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            var self = GetSelf(MessagingType.Visible);

            foreach (var descriptive in GetVisibleDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence GetImmediateDescription(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (!IsVisibleTo(viewer))
                return new Occurrence(MessagingType.Visible);

            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            return GetSelf(MessagingType.Visible);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return string.Empty;

            return GetSelf(MessagingType.Visible).ToString();
        }

        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IOccurrence RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            return GetImmediateDescription(viewer, sensoryTypes);
        }

        #region Visual Rendering
        /// <summary>
        /// Gets the actual vision Range taking into account blindness and other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public Tuple<float, float> GetVisualRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new Tuple<float, float>(-999999, 999999);
        }

        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        public bool IsVisibleTo(IEntity viewer)
        {
            var value = Luminosity;
            var range = viewer.GetVisualRange();

            return value >= range.Item1 && value <= range.Item2;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public IOccurrence RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return new Occurrence(MessagingType.Visible);

            return GetFullDescription(viewer, new[] { MessagingType.Visible });
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public IEnumerable<IOccurrence> GetVisibleDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible);
        }
        #endregion


        #region Psychic (sense) Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public Tuple<float, float> GetPsychicRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new Tuple<float, float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public bool IsSensibleTo(IEntity viewer)
        {
            var value = 0;
            var range = viewer.GetPsychicRange();

            return value >= range.Item1 && value <= range.Item2;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public IOccurrence RenderToSense(IEntity viewer)
        {
            if (!IsSensibleTo(viewer))
                return new Occurrence(MessagingType.Psychic);

            var self = GetSelf(MessagingType.Psychic);

            foreach (var descriptive in GetPsychicDescriptives(viewer))
                self.Event.TryModify(descriptive.Event);

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public IEnumerable<IOccurrence> GetPsychicDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Psychic);
        }
        #endregion


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

            foreach (var desc in Descriptives)
                returnList.Add("Descriptives", string.Format("{0} ({1}): {2}", desc.SensoryType, desc.Strength, desc.Event.ToString()));

            return returnList;
        }

    }
}
