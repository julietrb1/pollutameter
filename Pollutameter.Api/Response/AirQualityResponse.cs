using Pollutameter.Api.Domain;

namespace Pollutameter.Api.Response;

public record AirQualityResponse(
    IEnumerable<SiteGroup> SiteGroups
)
{
    // TODO: This property scales significantly with metric count. Refactor this to make it more flexible.
    public AirQuality WeightedAirQuality
    {
        get
        {
            var sumInverseDistancePm25ForHour = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + (siteGroup.AirQuality.Pm25ForHour != null ? siteGroup.Site.InverseDistance : 0));
            var sumInverseDistancePm25For24H = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + (siteGroup.AirQuality.Pm25For24H != null ? siteGroup.Site.InverseDistance : 0));
            var sumInverseDistancePm10ForHour = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + (siteGroup.AirQuality.Pm10ForHour != null ? siteGroup.Site.InverseDistance : 0));
            var sumInverseDistancePm10For24H = SiteGroups.Aggregate((double)0, (current, siteGroup) =>
                current + (siteGroup.AirQuality.Pm10For24H != null ? siteGroup.Site.InverseDistance : 0));
            return SiteGroups.Aggregate(new AirQuality(0, 0, 0, 0),
                (airQualitySummary, siteGroup) =>
                {
                    var inverseDistanceRatioPm25ForHour = sumInverseDistancePm25ForHour > 0
                        ? siteGroup.Site.InverseDistance / sumInverseDistancePm25ForHour
                        : 0;
                    var inverseDistanceRatioPm25For24H = sumInverseDistancePm25For24H > 0
                        ? siteGroup.Site.InverseDistance / sumInverseDistancePm25For24H
                        : 0;
                    var inverseDistanceRatioPm10ForHour = sumInverseDistancePm10ForHour > 0
                        ? siteGroup.Site.InverseDistance / sumInverseDistancePm10ForHour
                        : 0;
                    var inverseDistanceRatioPm10For24H = sumInverseDistancePm10For24H > 0
                        ? siteGroup.Site.InverseDistance / sumInverseDistancePm10For24H
                        : 0;
                    return new AirQuality(
                        airQualitySummary.Pm25ForHour +
                        (siteGroup.AirQuality.Pm25ForHour ?? 0) * inverseDistanceRatioPm25ForHour,
                        airQualitySummary.Pm25For24H +
                        (siteGroup.AirQuality.Pm25For24H ?? 0) * inverseDistanceRatioPm25For24H,
                        airQualitySummary.Pm10ForHour +
                        (siteGroup.AirQuality.Pm10ForHour ?? 0) * inverseDistanceRatioPm10ForHour,
                        airQualitySummary.Pm10For24H +
                        (siteGroup.AirQuality.Pm10For24H ?? 0) * inverseDistanceRatioPm10For24H
                    );
                });
        }
    }
}
