using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Physics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Backing data for physical models
    /// </summary>
    [Serializable]
    public class DimensionalModelData : TemplatePartial, IDimensionalModelData
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>

        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.ReviewOnly; } }

        /// <summary>
        /// Governs what sort of model planes we're looking for
        /// </summary>
        [Display(Name = "Model Type", Description = "The type of model this is. Flat models are used for everything.")]
        [UIHint("EnumDropDownList")]
        public DimensionalModelType ModelType { get; set; }

        /// <summary>
        /// The 21 planes that compose the physical model
        /// </summary>
        [UIHint("DimensionalModelPlanes")]
        public HashSet<IDimensionalModelPlane> ModelPlanes { get; set; }

        /// <summary>
        /// How hollow something is
        /// </summary>
        [IntDataIntegrity("Vacuity must be at least zero.", 0)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Hollowness", Description = "The hollowness of the model. Hollowness can increase sturctural integrity up to a point.")]
        [DataType(DataType.Text)]
        public int Vacuity { get; set; }

        /// <summary>
        /// Create an empty model
        /// </summary>
        public DimensionalModelData()
        {
            ModelPlanes = new HashSet<IDimensionalModelPlane>();
        }

        /// <summary>
        /// Create model serialized from a comma delimited string of an 21x21 plane
        /// </summary>
        /// <param name="delimitedPlane">comma delimited string of an 21x21 plane</param>
        public DimensionalModelData(string delimitedPlanes, DimensionalModelType type)
        {
            ModelType = type;
            ModelPlanes = new HashSet<IDimensionalModelPlane>();
            SerializeModelFromDelimitedList(delimitedPlanes);
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> dataProblems = base.FitnessReport();

            if (ModelPlanes == null || !ModelPlanes.Any() 
                || ModelPlanes.Any(m => m == null || string.IsNullOrWhiteSpace(m.TagName) || !m.ModelNodes.Any()))
            {
                dataProblems.Add("Model Planes are invalid.");
            }

            if (!IsModelValid())
            {
                dataProblems.Add("Model is invalid entirely.");
            }

            return dataProblems;
        }

        /// <summary>
        /// View the flattened model based on view angle
        /// </summary>
        /// <returns>the flattened model face based on the view angle</returns>
        public string ViewFlattenedModel(bool forWeb = false)
        {
            if(forWeb)
            {
                return Render.FlattenModelForWeb(this);
            }

            return Render.FlattenModel(this);        
        }

        /// <summary>
        /// Gets a node based on the X and Y axis
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <param name="yAxis">the Y-Axis of the node to get</param>
        /// <returns>the node</returns>
        public IDimensionalModelNode GetNode(short xAxis, short yAxis)
        {
            IDimensionalModelPlane plane = ModelPlanes.FirstOrDefault(pl => pl.YAxis.Equals(yAxis));

            if (plane != null)
            {
                return plane.GetNode(xAxis);
            }

            return null;
        }

        /// <summary>
        /// Checks if the model is valid for the physics engine
        /// </summary>
        /// <returns>validity</returns>
        public bool IsModelValid()
        {
            switch (ModelType)
            {
                case DimensionalModelType.Flat: //2d has 21 planes, but they're all flat (21 X nodes)
                    return ModelPlanes.Count == 21 && !ModelPlanes.Any(plane => string.IsNullOrWhiteSpace(plane.TagName) || plane.ModelNodes.Count != 21);
                case DimensionalModelType.None: //0d is always valid, it doesn't care about the model
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Turn a comma delimited list of planes into the modelplane set
        /// </summary>
        /// <param name="delimitedPlanes">comma delimited list of planes</param>
        private void SerializeModelFromDelimitedList(string delimitedPlanes)
        {
            //don't need to serialize nothing
            if (ModelType == DimensionalModelType.None)
            {
                return;
            }

            try
            {
                short yCount = 21;
                foreach (string myString in delimitedPlanes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    DimensionalModelPlane newPlane = new();
                    string[] currentLineNodes = myString.Split(new char[] { ',' });

                    //Name is first
                    newPlane.TagName = currentLineNodes[0];
                    newPlane.YAxis = yCount;

                    short xCount = 1;
                    foreach (string nodeString in currentLineNodes.Skip(1))
                    {
                        DimensionalModelNode newNode = new();
                        string[] nodeStringComponents = nodeString.Split(new char[] { '|' });

                        newNode.XAxis = xCount;
                        newNode.YAxis = yCount;

                        newNode.Style = string.IsNullOrWhiteSpace(nodeStringComponents[0])
                                            ? DamageType.None
                                            : Render.CharacterToDamageType(nodeStringComponents[0]);

                        //May not always indicate material id
                        if (nodeStringComponents.Count() > 1 && string.IsNullOrWhiteSpace(nodeStringComponents[1]))
                        {
                            newNode.Composition = TemplateCache.Get<IMaterial>(long.Parse(nodeStringComponents[1]));
                        }

                        newPlane.ModelNodes.Add(newNode);
                        xCount++;
                    }

                    //This ensures the linecount is always 21 for flats
                    ModelPlanes.Add(newPlane);
                    yCount--;
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                throw new FormatException("Invalid delimitedPlanes format.", ex);
            }
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Vacuity", Vacuity.ToString());
            returnList.Add("Model", ViewFlattenedModel(true));
            
            return returnList;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
