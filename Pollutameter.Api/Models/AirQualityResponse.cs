namespace Pollutameter.Web.Models;

public record AirQualityResponse(IEnumerable<ObservationResponse> Observations, double WeightedPm25);