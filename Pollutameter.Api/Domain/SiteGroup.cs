using Pollutameter.Api.Naq;

namespace Pollutameter.Api.Domain;

public record SiteGroup(
    IList<NaqObservationResult> Observations,
    NaqSiteWithDistance Site)
{
    public AirQualitySummary AirQualitySummary => new(Pm25, Pm10);

    private double? Pm25 => Observations
        .SingleOrDefault(observation => observation.Parameter.ParameterCode == NaqParameterCode.Pm25)?.Value!.Value;

    private double? Pm10 => Observations
        .SingleOrDefault(observation => observation.Parameter.ParameterCode == NaqParameterCode.Pm10)?.Value!.Value;
}
