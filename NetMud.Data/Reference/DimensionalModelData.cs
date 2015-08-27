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

namespace NetMud.Data.Reference
{
    /// <summary>
    /// Backing data for physical models
    /// </summary>
    public class DimensionalModelData : ReferenceDataPartial, IDimensionalModelData
    {
        /// <summary>
        /// Governs what sort of model planes we're looking for
        /// </summary>
        public DimensionalModelType ModelType { get; set; }

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
        /// Create model serialized from existing xml with a valid ID from the database
        /// </summary>
        /// <param name="dataId">the data id</param>
        /// <param name="modelXML">JSON of model planes (all 11 11x11 planes)</param>
        public DimensionalModelData(long dataId, string modelJson)
        {
            ModelPlanes = new HashSet<IDimensionalModelPlane>();

            var backingModel = ReferenceWrapper.GetOne<DimensionalModelData>(dataId);
            ID = backingModel.ID;
            Name = backingModel.Name;
            Created = backingModel.Created;
            LastRevised = backingModel.LastRevised;
            ModelType = backingModel.ModelType;

            DeserializeModel(modelJson);
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
            //Just cut off erroneous requests
            if (ModelType != DimensionalModelType.ThreeD && zAxis > 0)
                return null;

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
            //Just cut off erroneous requests
            if (ModelType != DimensionalModelType.ThreeD)
                return null;

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
        /// View the flattened model based on view angle
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
            switch(ModelType)
            {
                case DimensionalModelType.Flat: //2d has 11 planes, but they're all flat (11 X nodes)
                    return ModelPlanes.Count == 11 && !ModelPlanes.Any(plane => String.IsNullOrWhiteSpace(plane.TagName) || plane.ModelNodes.Count != 11);
                case DimensionalModelType.None: //0d is always valid, it doesn't care about the model
                    return true;
                case DimensionalModelType.ThreeD://3d has 11 planes with 11 depth nodes and 11 X nodes
                    return ModelPlanes.Count == 11 && !ModelPlanes.Any(plane => String.IsNullOrWhiteSpace(plane.TagName) || plane.ModelNodes.Count != 121);
            }

            return false;
        }

        /// <summary>
        /// Fills a data object with data from a data row
        /// </summary>
        /// <param name="dr">the data row to fill from</param>
        public override void Fill(DataRow dr)
        {
            ID = DataUtility.GetFromDataRow<long>(dr, "ID");
            Created = DataUtility.GetFromDataRow<DateTime>(dr, "Created");
            LastRevised = DataUtility.GetFromDataRow<DateTime>(dr, "LastRevised");
            Name = DataUtility.GetFromDataRow<string>(dr, "Name");
            ModelType = (DimensionalModelType)DataUtility.GetFromDataRow<short>(dr, "ModelType");

            string outModel = DataUtility.GetFromDataRow<string>(dr, "Model");
            DeserializeModel(outModel);
        }

        /// <summary>
        /// insert this into the db
        /// </summary>
        /// <returns>the object with ID and other db fields set</returns>
        public override IData Create()
        {
            var parms = new Dictionary<string, object>();

            DimensionalModelData returnValue = default(DimensionalModelData);
            var sql = new StringBuilder();
            sql.Append("insert into [dbo].[DimensionalModelData]([Name], [Model], [ModelType])");
            sql.Append(" values(@Name,@Model,@ModelType)");
            sql.Append(" select * from [dbo].[DimensionalModelData] where ID = Scope_Identity()");

            parms.Add("Name", Name);
            parms.Add("Model", SerializeModel());
            parms.Add("ModelType", (short)ModelType);

            try
            {
                var ds = SqlWrapper.RunDataset(sql.ToString(), CommandType.Text, parms);

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
            var parms = new Dictionary<string, object>();

            var sql = new StringBuilder();
            sql.Append("delete from [dbo].[DimensionalModelData] where ID = @id");

            parms.Add("id", ID);

            SqlWrapper.RunNonQuery(sql.ToString(), CommandType.Text);

            return true;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save()
        {
            var parms = new Dictionary<string, object>();

            var sql = new StringBuilder();
            sql.Append("update [dbo].[DimensionalModelData] set ");
            sql.Append(" [Name] = @Name ");
            sql.Append(" , [Model] = @Model ");
            sql.Append(" , [ModelType] = @ModelType ");
            sql.Append(" , [LastRevised] = GetUTCDate()");
            sql.Append(" where ID = @id");

            parms.Add("id", ID);
            parms.Add("Name", Name);
            parms.Add("Model", SerializeModel());
            parms.Add("ModelType", (short)ModelType);

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
        public string SerializeModel()
        {
            return JsonConvert.SerializeObject(ModelPlanes);
        }

        /// <summary>
        /// Turn the json we store in the db into the modelplanes
        /// </summary>
        /// <param name="modelJson">json we store in the db</param>
        private void DeserializeModel(string modelJson)
        {
            //Don't bother even trying
            if (ModelType == DimensionalModelType.None)
                return;

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
            //don't need to serialize nothing
            if (ModelType == DimensionalModelType.None)
                return;

            var newPlane = new DimensionalModelPlane();
            short lineCount = 12;
            short yCount = 11;

            try
            {
                foreach (var myString in delimitedPlanes.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    //This is the tagName line
                    if (lineCount == 12)
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
                            newNode.ZAxis = ModelType == DimensionalModelType.Flat ? (short)0 : lineCount;
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

                        //This ensures the linecount is always 12 for flats
                        if (lineCount == 1 || ModelType == DimensionalModelType.Flat)
                        {
                            ModelPlanes.Add(newPlane);
                            lineCount = 12;
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
