using Pollutameter.Web.NAQ;

namespace Pollutameter.Web.Models;

public record ObservationWithSite(
    NaqObservationResult Observation,
    NaqSiteWithDistance Site);