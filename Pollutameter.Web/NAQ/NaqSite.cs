using System.Text.Json.Serialization;

namespace Pollutameter.Web.NAQ;

public record NaqSite(
    [property: JsonPropertyName("Site_Id")]
    int SiteId,
    string SiteName,
    double? Latitude,
    double? Longitude,
    string Region);