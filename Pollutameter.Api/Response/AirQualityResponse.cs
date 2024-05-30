using Pollutameter.Api.Domain;

namespace Pollutameter.Api.Response;

public record AirQualityResponse(
    IEnumerable<SiteGroup> SiteGroups
)
{
    public AirQuality WeightedAirQuality
    {
        get
        {
            var sumInverseDistancePm25 = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + (siteGroup.AirQuality.Pm25 != null ? siteGroup.Site.InverseDistance : 0));
            var sumInverseDistancePm10 = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + (siteGroup.AirQuality.Pm10 != null ? siteGroup.Site.InverseDistance : 0));
            return SiteGroups.Aggregate(new AirQuality(0, 0),
                (airQualitySummary, siteGroup) =>
                {
                    var inverseDistanceRatioPm25 = sumInverseDistancePm25 > 0
                        ? siteGroup.Site.InverseDistance / sumInverseDistancePm25
                        : 0;
                    var inverseDistanceRatioPm10 = sumInverseDistancePm10 > 0
                        ? siteGroup.Site.InverseDistance / sumInverseDistancePm10
                        : 0;
                    return new AirQuality(
                        airQualitySummary.Pm25 + (siteGroup.AirQuality.Pm25 ?? 0) * inverseDistanceRatioPm25,
                        airQualitySummary.Pm10 + (siteGroup.AirQuality.Pm10 ?? 0) * inverseDistanceRatioPm10
                    );
                });
        }
    }
}
