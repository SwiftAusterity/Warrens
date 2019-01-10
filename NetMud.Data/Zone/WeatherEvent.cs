using NetMud.DataStructure.Zone;
using System;

namespace NetMud.Data.Zone
{
    /// <summary>
    /// Individual weather events
    /// </summary>
    [Serializable]
    public class WeatherEvent : IWeatherEvent
    {
        /// <summary>
        /// The event type
        /// </summary>
        public WeatherEventType Type { get; set; }

        /// <summary>
        /// The strength of the event (size of cloud, windspeed for cyclones)
        /// </summary>
        public float Strength { get; set; }

        /// <summary>
        /// How much strength does this bleed per cycle, important for cyclones and typhoons and for earthquake aftershocks
        /// </summary>
        public float Drain { get; set; }

        /// <summary>
        /// How high up is the event
        /// </summary>
        public float Altitude { get; set; }

        /// <summary>
        /// How much does this obscure the sky, Percentage
        /// </summary>
        public float Coverage { get; set; }

        /// <summary>
        /// How much precipitation does this produce per cycle?
        /// </summary>
        public float PrecipitationAmount { get; set; }
    }

}
