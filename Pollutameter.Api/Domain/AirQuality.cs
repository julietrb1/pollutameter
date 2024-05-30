namespace Pollutameter.Api.Domain;

/// <summary>
/// Represents a set of air quality metrics for a known dimension/data point.
/// </summary>
/// <param name="Pm25ForHour">The <see href="https://en.wikipedia.org/wiki/Particulates">PM2.5</see> value (µg/m³) averaged over the last hour.</param>
/// <param name="Pm10ForHour">The <see href="https://en.wikipedia.org/wiki/Particulates">PM10</see> value (µg/m³) averaged over the last hour.</param>
public record AirQuality(
    double? Pm25ForHour, double? Pm10ForHour);
