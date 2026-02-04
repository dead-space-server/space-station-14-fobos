namespace Content.Shared._Frostheim.Map;

[ByRefEvent]
public record struct FrostheimWeatherChangedEvent(EntityUid MapUid, FrostheimWeather OldWeather, FrostheimWeather NewWeather);
