using System.Text.Json.Serialization;

namespace Pollutameter.Web.Naq;

public record NaqSite(
    [property: JsonPropertyName("Site_Id")]
    int SiteId,
    string SiteName,
    double? Latitude,
    double? Longitude,
    string Region);
