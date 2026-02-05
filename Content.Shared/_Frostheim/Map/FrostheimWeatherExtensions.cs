namespace Content.Shared._Frostheim.Map;

public static class FrostheimWeatherExtensions
{
    public static readonly Dictionary<FrostheimWeather, float> OutdoorTemperatureByWeather = new()
    {
        { FrostheimWeather.None, 233f },
        { FrostheimWeather.SnowfallLight, 213f },
        { FrostheimWeather.SnowfallMedium, 193f },
        { FrostheimWeather.SnowfallHeavy, 173f }
    };

    public static readonly Dictionary<FrostheimWeather, float> IndoorTemperatureByWeather = new()
    {
        { FrostheimWeather.None, 263f },
        { FrostheimWeather.SnowfallLight, 243f },
        { FrostheimWeather.SnowfallMedium, 223f },
        { FrostheimWeather.SnowfallHeavy, 203f }
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
            FrostheimWeather.None => roll < 0.2f ? FrostheimWeather.None : FrostheimWeather.SnowfallLight,
            FrostheimWeather.SnowfallLight => roll < 0.1f ? FrostheimWeather.None : roll < 0.35f ? FrostheimWeather.SnowfallLight : FrostheimWeather.SnowfallMedium,
            FrostheimWeather.SnowfallMedium => roll < 0.15f ? FrostheimWeather.SnowfallLight : roll < 0.45f ? FrostheimWeather.SnowfallMedium : FrostheimWeather.SnowfallHeavy,
            FrostheimWeather.SnowfallHeavy => roll < 0.3f ? FrostheimWeather.SnowfallMedium : FrostheimWeather.SnowfallHeavy,
            _ => FrostheimWeather.None
        };
    }
}
