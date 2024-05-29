using System.Text.Json.Serialization;

namespace Pollutameter.Web.NAQ;

public record NaqObservationResult(
    [property: JsonPropertyName("Site_Id")]
    int SiteId,
    NaqParameter Parameter,
    string Date,
    int Hour,
    string HourDescription,
    double? Value,
    string AirQualityCategory,
    string DeterminingPollutant);