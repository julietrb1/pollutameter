using Pollutameter.Web.Naq;

namespace Pollutameter.Web.Models;

public record ObservationWithSite(
    NaqObservationResult Observation,
    NaqSiteWithDistance Site);
