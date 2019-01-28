namespace NetMud.Gaia.Meteorological
{
    public static class MeteorologicalUtilities
    {
        public static TemperatureType ConvertTemperatureToType(int temperature)
        {
            if (temperature < -15)
            {
                return TemperatureType.Freezing;
            }
            else if (temperature < 0)
            {
                return TemperatureType.Cold;
            }
            else if (temperature < 5)
            {
                return TemperatureType.Chilly;
            }
            else if (temperature < 10)
            {
                return TemperatureType.Moderate;
            }
            else if (temperature < 15)
            {
                return TemperatureType.Temperate;
            }
            else if (temperature < 20)
            {
                return TemperatureType.Warm;
            }
            else if (temperature < 25)
            {
                return TemperatureType.Hot;
            }
            else if (temperature < 40)
            {
                return TemperatureType.Scorching;
            }

            return TemperatureType.Australia;
        }

        public static HumidityType ConvertHumidityToType(int pressure)
        {
            if (pressure < 1060)
            {
                return HumidityType.Arid;
            }
            else if (pressure < 1020)
            {
                return HumidityType.Dry;
            }
            else if (pressure < 1000)
            {
                return HumidityType.Moderate;
            }
            else if (pressure < 975)
            {
                return HumidityType.Humid;
            }
            else if (pressure < 950)
            {
                return HumidityType.Balmy;
            }

            return HumidityType.Swampy;
        }
    }
}
