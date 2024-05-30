using System.Text.Json.Serialization;
using Pollutameter.Api.Naq;

namespace Pollutameter.Api.Domain;

/// <summary>
/// A group of air quality observations for a known site.
/// </summary>
/// <param name="Observations">Observations for the group's site, identified by parameter code.</param>
/// <param name="Site">The site that observations pertain to.</param>
public record SiteGroup(
    [property: JsonIgnore] IList<NaqObservationResult> Observations,
    NaqSiteWithDistance Site)
{
    /// <summary>
    /// Air quality metrics for this site.
    /// </summary>
    public AirQuality AirQuality => new(Pm25ForHour, Pm10ForHour);

    /// <summary>
    /// The <see href="https://en.wikipedia.org/wiki/Particulates">PM2.5</see> value (µg/m³) for this site averaged over the last hour.
    /// </summary>
    private double? Pm25ForHour => Observations
        .SingleOrDefault(observation => observation.Parameter.ParameterCode == NaqParameterCode.Pm25)?.Value!.Value;

    /// The <see href="https://en.wikipedia.org/wiki/Particulates">PM10</see> value (µg/m³) for this site averaged over the last hour.
    private double? Pm10ForHour => Observations
        .SingleOrDefault(observation => observation.Parameter.ParameterCode == NaqParameterCode.Pm10)?.Value!.Value;
}
