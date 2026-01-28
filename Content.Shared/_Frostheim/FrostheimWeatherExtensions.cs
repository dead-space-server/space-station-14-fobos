namespace Content.Shared._Frostheim;

public static class FrostheimWeatherExtensions
{
    public static readonly Dictionary<FrostheimWeather, float> OutdoorTemperatureByWeather = new()
    {
        { FrostheimWeather.None, 223f },
        { FrostheimWeather.SnowfallLight, 193f },
        { FrostheimWeather.SnowfallMedium, 183f },
        { FrostheimWeather.SnowfallHeavy, 173f }
    };

    public static readonly Dictionary<FrostheimWeather, float> IndoorTemperatureByWeather = new()
    {
        { FrostheimWeather.None, 253f },
        { FrostheimWeather.SnowfallLight, 223f },
        { FrostheimWeather.SnowfallMedium, 213f },
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
}
