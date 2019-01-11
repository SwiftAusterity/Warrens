using NetMud.DataStructure.Zone;
using System;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Type", Description = "The type of weather.")]
        [UIHint("EnumDropDownList")]
        public WeatherEventType Type { get; set; }

        /// <summary>
        /// The strength of the event (size of cloud, windspeed for cyclones)
        /// </summary>
        [Display(Name = "Strength", Description = "The type of weather.")]
        [DataType(DataType.Text)]
        public float Strength { get; set; }

        /// <summary>
        /// How much strength does this bleed per cycle, important for cyclones and typhoons and for earthquake aftershocks
        /// </summary>
        [Display(Name = "Drain", Description = "How much strength does this bleed per cycle, important for cyclones and typhoons and for earthquake aftershocks.")]
        [DataType(DataType.Text)]
        public float Drain { get; set; }

        /// <summary>
        /// How high up is the event
        /// </summary>
        [Display(Name = "Altitude", Description = "How high up is the event.")]
        [DataType(DataType.Text)]
        public float Altitude { get; set; }

        /// <summary>
        /// How much does this obscure the sky, Percentage
        /// </summary>
        [Display(Name = "Coverage", Description = "THow much does this obscure the sky, Percentage.")]
        [DataType(DataType.Text)]
        public float Coverage { get; set; }

        /// <summary>
        /// How much precipitation does this produce per cycle?
        /// </summary>
        [Display(Name = "PrecipitationAmount", Description = "How much precipitation does this produce per cycle?.")]
        [DataType(DataType.Text)]
        public float PrecipitationAmount { get; set; }
    }

}
