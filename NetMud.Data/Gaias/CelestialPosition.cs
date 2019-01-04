using NetMud.DataAccess.Cache;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.DataStructure.Gaia
{
    [Serializable]
    public class CelestialPosition : ICelestialPosition
    {
        [JsonProperty("CelestialObject")]
        public TemplateCacheKey _celestialObject { get; set; }

        /// <summary>
        /// Where the various celestial bodies are along their paths
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public ICelestial CelestialObject
        {
            get
            {
                if (_celestialObject == null)
                    return null;

                return TemplateCache.Get<ICelestial>(_celestialObject);
            }
            set
            {
                if (value == null)
                    return;

                _celestialObject = new TemplateCacheKey(value);
            }
        }


        public float Position { get; set; }

        public CelestialPosition()
        {
            Position = 0;
        }

        public CelestialPosition(ICelestial celestialObject, float position)
        {
            CelestialObject = celestialObject;
            Position = position;
        }
    }
}
