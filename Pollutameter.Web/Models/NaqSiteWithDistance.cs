namespace Pollutameter.Web.Models;

public record NaqSiteWithDistance(int SiteId, string SiteName, double DistanceInKm, double InvertedDistance);