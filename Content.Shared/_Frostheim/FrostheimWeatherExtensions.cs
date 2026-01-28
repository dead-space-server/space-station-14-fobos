namespace Content.Shared._Frostheim;

public static class FrostheimWeatherExtensions
{
    public static readonly Dictionary<FrostheimWeather, float> OutdoorTemperatureByWeather = new()
    {
        { FrostheimWeather.None, 243f },       // -30°C
        { FrostheimWeather.SnowfallLight, 223f },  // -50°C
        { FrostheimWeather.SnowfallMedium, 203f }, // -70°C
        { FrostheimWeather.SnowfallHeavy, 183f }   // -90°C
    };

    public static readonly Dictionary<FrostheimWeather, float> IndoorTemperatureByWeather = new()
    {
        { FrostheimWeather.None, 273f },       // 0°C
        { FrostheimWeather.SnowfallLight, 253f },  // -20°C
        { FrostheimWeather.SnowfallMedium, 233f }, // -40°C
        { FrostheimWeather.SnowfallHeavy, 213f }   // -60°C
    };

    public static float GetOutdoorTemperature(this FrostheimWeather weather)
    {
        return OutdoorTemperatureByWeather.TryGetValue(weather, out var temp)
            ? temp
            : 223f;
    }

    public static float GetIndoorTemperature(this FrostheimWeather weather)
    {
        return IndoorTemperatureByWeather.TryGetValue(weather, out var temp)
            ? temp
            : 253f;
    }

    public static FrostheimWeather GetNextWeather(this FrostheimWeather current, float roll)
    {
        return current switch
        {
            FrostheimWeather.None => roll < 0.7f ? FrostheimWeather.None : FrostheimWeather.SnowfallLight,
            FrostheimWeather.SnowfallLight => roll < 0.3f ? FrostheimWeather.None : roll < 0.7f ? FrostheimWeather.SnowfallLight : FrostheimWeather.SnowfallMedium,
            FrostheimWeather.SnowfallMedium => roll < 0.3f ? FrostheimWeather.SnowfallLight : roll < 0.7f ? FrostheimWeather.SnowfallMedium : FrostheimWeather.SnowfallHeavy,
            FrostheimWeather.SnowfallHeavy => roll < 0.4f ? FrostheimWeather.SnowfallMedium : FrostheimWeather.SnowfallHeavy,
            _ => FrostheimWeather.None
        };
    }
}
