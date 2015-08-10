using NetMud.DataAccess;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Physics;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Reference
{
    /// <summary>
    /// Backing data for physical models
    /// </summary>
    public class DimensionalModelData : ReferenceDataPartial, IDimensionalModelData
    {
        /// <summary>
        /// Create an empty model
        /// </summary>
        public DimensionalModelData()
        {
            ModelPlanes = new HashSet<IDimensionalModelPlane>();
        }

        /// <summary>
        /// Create model serialized from a comma delimited string of model planes (all 11 11x11 planes)
        /// </summary>
        /// <param name="delimitedPlanes">comma delimited string of model planes (all 11 11x11 planes)</param>
        public DimensionalModelData(string delimitedPlanes)
        {
            ModelPlanes = new HashSet<IDimensionalModelPlane>();
            SerializeModelFromDelimitedList(delimitedPlanes);
        }

        /// <summary>
        /// The 11 planes that compose the physical model
        /// </summary>
        public HashSet<IDimensionalModelPlane> ModelPlanes { get; set; }

        /// <summary>
        /// Gets a node based on the X and Y axis
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <param name="yAxis">the Y-Axis of the node to get</param>
        /// <param name="zAxis">the Z-Axis of the node to get</param>
        /// <returns>the node</returns>
        public IDimensionalModelNode GetNode(short xAxis, short yAxis, short zAxis)
        {
            var plane = ModelPlanes.FirstOrDefault(pl => pl.YAxis.Equals(yAxis));

            if (plane != null)
                return plane.GetNode(xAxis, zAxis);

            return null;
        }

        /// <summary>
        /// Gets the node behind the indicated node
        /// </summary>
        /// <param name="xAxis">the X-Axis of the initial node to get</param>
        /// <param name="yAxis">the Y-Axis of the initial node to get</param>
        /// <param name="zAxis">the Z-Axis of the initial node to get</param>
        /// <param name="pitch">rotation on the z-axis</param>
        /// <param name="yaw">rotation on the Y-axis</param>
        /// <param name="roll">rotation on the x-axis</param>
        /// <returns>the node "behind" the node asked for (can be null)</returns>
        public IDimensionalModelNode GetNodeBehindNode(short xAxis, short yAxis, short zAxis, short pitch, short yaw, short roll)
        {
            var plane = ModelPlanes.FirstOrDefault(pl => pl.YAxis.Equals(yAxis));
            IDimensionalModelNode node = null;

            //Get the initial node first
            if (plane != null)
                node = plane.GetNode(xAxis, zAxis);

            if (node != null)
            {
                //We need to determine what the actuals are here, we might have axis flipped depending on the perspective angle
                //yaw 1-10 - length = model xAxis 11-1, height = yAxis 11-1, depth = zAxis 1-11
                //yaw 11-21 - length = model zAxis 1-11, height = yAxis 11-1, depth = xAxis 1-11
                //yaw 22-32 - length = model xAxis 1-11, height = yAxis 11-1, depth = zAxis 11-1
                //yaw 33-43 - length = model zAxis 11-1, height = yAxis 11-1, depth = xAxis 11-1

                //roll 1-10 - length = model xAxis 11-1, height = yAxis 11-1, depth = zAxis 1-11
                //roll 11-21 - length = model yAxis 1-11, height = xAxis 1-1, depth = zAxis 1-11
                //roll 22-32 - length = model xAxis 1-11, height = yAxis 1-1, depth = zAxis 1-11
                //roll 33-43 - length = model yAxis 11-1, height = xAxis 11-1, depth = zAxis 1-11

                //pitch 1-10 - length = model xAxis 11-1, height = yAxis 11-1, depth = zAxis 1-11
                //pitch 11-21 - length = model xAxis 11-1, height = zAxis 1-11, depth = yAxis 1-11
                //pitch 22-32 - length = model xAxis 11-1, height = yAxis 1-11, depth = zAxis 11-1
                //pitch 33-43 - length = model xAxis 11-1, height = zAxis 11-1, depth = yAxis 11-1

                short zChange = 0, yChange = 0, xChange = 0;

                if (yaw > 0)
                {
                    if (yaw <= 10)
                    {
                        zChange++;
                        xChange++;
                    }
                    else if (yaw == 11)
                        xChange--;
                    else if (yaw <= 21)
                    {
                        xChange--;
                        zChange--;
                    }
                    else if (yaw == 22)
                        zChange++;
                    else if (yaw <= 32)
                    {
                        zChange++;
                        xChange--;
                    }
                    else if (yaw == 33)
                        xChange--;
                    else
                    {
                        xChange--;
                        zChange++;
                    }
                }

                if (roll > 0)
                    zChange++;

                if (pitch > 0)
                {
                    if (pitch <= 10)
                    {
                        zChange++;
                        yChange++;
                    }
                    else if (pitch == 11)
                        yChange++;
                    else if (pitch <= 21)
                    {
                        yChange++;
                        zChange--;
                    }
                    else if (pitch == 22)
                        zChange--;
                    else if (pitch <= 32)
                    {
                        zChange--;
                        yChange--;
                    }
                    else if (pitch == 33)
                        yChange--;
                    else
                    {
                        yChange--;
                        zChange++;
                    }
                }

                if (roll == 0 && yaw == 0 && pitch == 0)
                    zChange++;

                if (xChange > 1)
                    xChange = 1;
                if (xChange < -1)
                    xChange = -1;

                if (yChange > 1)
                    yChange = 1;
                if (yChange < -1)
                    yChange = -1;

                if (zChange > 1)
                    zChange = 1;
                if (zChange < -1)
                    zChange = -1;

                short newX = (short)(xAxis + xChange);
                short newY = (short)(yAxis + yChange);
                short newZ = (short)(zAxis + zChange);

                return GetNode(newX, newY, newZ);
            }

            return null;
        }

        /// <summary>
        /// View the flattened model based on view angle; TODO: ONLY SUPPORTS THE FRONT FACE ATM
        /// </summary>
        /// <param name="pitch">rotation on the z-axis</param>
        /// <param name="yaw">rotation on the Y-axis</param>
        /// <param name="roll">rotation on the x-axis</param>
        /// <returns>the flattened model face based on the view angle</returns>
        public string ViewFlattenedModel(short pitch, short yaw, short roll)
        {
            return Render.FlattenModel(this, pitch, yaw, roll);
        }

        /// <summary>
        /// Checks if the model is valid for the physics engine
        /// </summary>
        /// <returns>validity</returns>
        public bool IsModelValid()
        {
            return ModelPlanes.Count == 11 && !ModelPlanes.Any(plane => String.IsNullOrWhiteSpace(plane.TagName) || plane.ModelNodes.Count != 121);
        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(global::System.Data.DataRow dr)
        {
            int outId = default(int);
            DataUtility.GetFromDataRow<int>(dr, "ID", ref outId);
            ID = outId;

            DateTime outCreated = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "Created", ref outCreated);
            Created = outCreated;

            DateTime outRevised = default(DateTime);
            DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised", ref outRevised);
            LastRevised = outRevised;

            string outName = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Name", ref outName);
            Name = outName;

            string outModel = default(string);
            DataUtility.GetFromDataRow<string>(dr, "Model", ref outModel);
            SerializeModel(outModel);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            DimensionalModelData returnValue = default(DimensionalModelData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[DimensionalModelData]([Name], [Model])");
            sql.AppendFormat(" values('{0}','{1}')", Name, DeserializeModel());
            sql.Append(" select * from [dbo].[DimensionalModelData] where ID = Scope_Identity()");

            try
            {
                var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text);

                if (ds.Rows != null)
                {
                    foreach (DataRow dr in ds.Rows)
                    {
                        Fill(dr);
                        returnValue = this;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return returnValue;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>

        public override bool Remove()
        {
            var sql = new StringBuilder();
            sql.AppendFormat("delete from [dbo].[DimensionalModelData] where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save()
        {
            var sql = new StringBuilder();
            sql.Append("update [dbo].[DimensionalModelData] set ");
            sql.AppendFormat(" [Name] = '{0}' ", Name);
            sql.AppendFormat(" , [Model] = '{0}' ", DeserializeModel());
            sql.AppendFormat(" , [LastRevised] = GetUTCDate()");
            sql.AppendFormat(" where ID = {0}", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            //TODO: Render the actual model flattened in ascii, probably require a fair bit of work so just returning name for now
            sb.Add(Name);

            return sb;
        }

        /// <summary>
        /// Turn the modelPlanes into a json string we can store in the db
        /// </summary>
        /// <returns></returns>
        private string DeserializeModel()
        {
            return JsonConvert.SerializeObject(ModelPlanes);
        }

        /// <summary>
        /// Turn the json we store in the db into the modelplanes
        /// </summary>
        /// <param name="modelJson">json we store in the db</param>
        private void SerializeModel(string modelJson)
        {
            dynamic planes = JsonConvert.DeserializeObject(modelJson);

            foreach (dynamic plane in planes)
            {
                var newPlane = new DimensionalModelPlane();
                newPlane.TagName = plane.TagName;
                newPlane.YAxis = plane.YAxis;

                foreach (dynamic node in plane.ModelNodes)
                {
                    var newNode = new DimensionalModelNode();
                    newNode.XAxis = node.XAxis;
                    newNode.ZAxis = node.ZAxis;
                    newNode.YAxis = newPlane.YAxis;
                    newNode.Style = node.Style;
                    newNode.Composition = node.Composition;
                    newPlane.ModelNodes.Add(newNode);
                }

                ModelPlanes.Add(newPlane);
            }

        }
        /// <summary>
        /// Turn a comma delimited list of planes into the modelplane set
        /// </summary>
        /// <param name="delimitedPlanes">comma delimited list of planes</param>
        private void SerializeModelFromDelimitedList(string delimitedPlanes)
        {
            var newPlane = new DimensionalModelPlane();
            short lineCount = 11;
            short yCount = 11;

            try
            {
                foreach (var myString in delimitedPlanes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //This is the tagName line
                    if (lineCount == 11)
                    {
                        newPlane.TagName = myString;
                        newPlane.YAxis = yCount;
                    }
                    else
                    {
                        var currentLineNodes = myString.Split(new char[] { ',' });

                        short xCount = 1;
                        foreach (var nodeString in currentLineNodes)
                        {
                            var newNode = new DimensionalModelNode();
                            var nodeStringComponents = nodeString.Split(new char[] { '|' });

                            newNode.XAxis = xCount;
                            newNode.ZAxis = lineCount;
                            newNode.YAxis = yCount;

                            newNode.Style = String.IsNullOrWhiteSpace(nodeStringComponents[0])
                                                ? DamageType.None
                                                : Render.CharacterToDamageType(nodeStringComponents[0]);

                            newNode.Composition = nodeStringComponents.Count() < 2 || String.IsNullOrWhiteSpace(nodeStringComponents[1])
                                                ? default(IMaterial)
                                                : default(IMaterial); //TODO: Implement materials -- ReferenceAccess.GetOne<IMaterial>(long.Parse(nodeStringComponents[1]));

                            newPlane.ModelNodes.Add(newNode);
                            xCount++;
                        }

                        if (lineCount == 1)
                        {
                            ModelPlanes.Add(newPlane);
                            lineCount = 11;
                            yCount--;

                            newPlane = new DimensionalModelPlane();
                            continue;
                        }
                    }

                    lineCount--;
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
