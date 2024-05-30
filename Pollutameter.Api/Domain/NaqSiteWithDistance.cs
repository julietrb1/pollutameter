namespace Pollutameter.Api.Domain;

public record NaqSiteWithDistance(int SiteId, string SiteName, double DistanceInKm, double InverseDistance);
