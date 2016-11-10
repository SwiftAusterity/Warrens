using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Backing data for physical models
    /// </summary>
    [Serializable]
    public class DimensionalModelData : LookupDataPartial, IDimensionalModelData
    {
        /// <summary>
        /// Governs what sort of model planes we're looking for
        /// </summary>
        public DimensionalModelType ModelType { get; set; }

        /// <summary>
        /// The 11 planes that compose the physical model
        /// </summary>
        public HashSet<IDimensionalModelPlane> ModelPlanes { get; set; }

        /// <summary>
        /// How hollow something is
        /// </summary>
        public int Vacuity { get; set; }

        /// <summary>
        /// Create an empty model
        /// </summary>
        public DimensionalModelData()
        {
            ModelPlanes = new HashSet<IDimensionalModelPlane>();
        }

        /// <summary>
        /// Create model serialized from a comma delimited string of an 11x11 plane
        /// </summary>
        /// <param name="delimitedPlane">comma delimited string of an 11x11 plane</param>
        public DimensionalModelData(string delimitedPlanes, DimensionalModelType type)
        {
            ModelType = type;
            ModelPlanes = new HashSet<IDimensionalModelPlane>();
            SerializeModelFromDelimitedList(delimitedPlanes);
        }

        /// <summary>
        /// View the flattened model based on view angle
        /// </summary>
        /// <returns>the flattened model face based on the view angle</returns>
        public string ViewFlattenedModel(bool forWeb = false)
        {
            if(forWeb)
                return Render.FlattenModelForWeb(this);

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
            var plane = ModelPlanes.FirstOrDefault(pl => pl.YAxis.Equals(yAxis));

            if (plane != null)
                return plane.GetNode(xAxis);

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
                case DimensionalModelType.Flat: //2d has 11 planes, but they're all flat (11 X nodes)
                    return ModelPlanes.Count == 11 && !ModelPlanes.Any(plane => String.IsNullOrWhiteSpace(plane.TagName) || plane.ModelNodes.Count != 11);
                case DimensionalModelType.None: //0d is always valid, it doesn't care about the model
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            //TODO: Render the actual model flattened in ascii, probably require a fair bit of work so just returning name for now
            return base.RenderHelpBody();
        }

        /// <summary>
        /// Turn a comma delimited list of planes into the modelplane set
        /// </summary>
        /// <param name="delimitedPlanes">comma delimited list of planes</param>
        private void SerializeModelFromDelimitedList(string delimitedPlanes)
        {
            //don't need to serialize nothing
            if (ModelType == DimensionalModelType.None)
                return;

            try
            {
                short yCount = 11;
                foreach (var myString in delimitedPlanes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var newPlane = new DimensionalModelPlane();
                    var currentLineNodes = myString.Split(new char[] { ',' });

                    //Name is first
                    newPlane.TagName = currentLineNodes[0];
                    newPlane.YAxis = yCount;

                    short xCount = 1;
                    foreach (var nodeString in currentLineNodes.Skip(1))
                    {
                        var newNode = new DimensionalModelNode();
                        var nodeStringComponents = nodeString.Split(new char[] { '|' });

                        newNode.XAxis = xCount;
                        newNode.YAxis = yCount;

                        newNode.Style = String.IsNullOrWhiteSpace(nodeStringComponents[0])
                                            ? DamageType.None
                                            : Render.CharacterToDamageType(nodeStringComponents[0]);

                        //May not always indicate material id
                        if (nodeStringComponents.Count() > 1 && String.IsNullOrWhiteSpace(nodeStringComponents[1]))
                            newNode.Composition = BackingDataCache.Get<IMaterial>(long.Parse(nodeStringComponents[1]));

                        newPlane.ModelNodes.Add(newNode);
                        xCount++;
                    }

                    //This ensures the linecount is always 11 for flats
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
    }
}
