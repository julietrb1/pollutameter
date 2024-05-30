using Pollutameter.Api.Domain;

namespace Pollutameter.Api.Response;

public record AirQualityResponse(
    IEnumerable<SiteGroup> SiteGroups
)
{
    public AirQualitySummary WeightedAirQualitySummary
    {
        get
        {
            var sumInverseDistancePm25 = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + siteGroup.AirQualitySummary.Pm25 != null ? siteGroup.Site.InverseDistance : 0);
            var sumInverseDistancePm10 = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + siteGroup.AirQualitySummary.Pm10 != null ? siteGroup.Site.InverseDistance : 0);
            return SiteGroups.Aggregate(new AirQualitySummary(0, 0),
                (airQualitySummary, siteGroup) =>
                {
                    var inverseDistanceRatioPm25 = siteGroup.Site.InverseDistance / sumInverseDistancePm25;
                    var inverseDistanceRatioPm10 = siteGroup.Site.InverseDistance / sumInverseDistancePm10;
                    return new AirQualitySummary(
                        airQualitySummary.Pm25 + siteGroup.AirQualitySummary.Pm25 * inverseDistanceRatioPm25,
                        airQualitySummary.Pm10 + siteGroup.AirQualitySummary.Pm10 * inverseDistanceRatioPm10
                    );
                });
        }
    }
}
