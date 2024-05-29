namespace Pollutameter.Web.Models;

public record ObservationResponse(double RawPm25, NaqSiteWithDistance Site, int UpdatedAtHour);
