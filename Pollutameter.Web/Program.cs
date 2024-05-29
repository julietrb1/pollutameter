using System.Text;
using Geolocation;
using Pollutameter.Web.Models;
using Pollutameter.Web.NAQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

HttpClient naqHttpClient = new()
{
    BaseAddress = new Uri("https://data.airquality.nsw.gov.au")
};

naqHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");

app.MapPost("/air-quality", async (double latitude, double longitude, double maxKm = 10) =>
    {
        var observationsTask = FetchObservations();
        var sites = await FetchSitesWithLocation();
        var sitesWithDistance = sites.Select(site =>
        {
            var distanceInKm = GeoCalculator.GetDistance(latitude, longitude, (double)site.Latitude,
                (double)site.Longitude, 1, DistanceUnit.Kilometers);
            return new NaqSiteWithDistance(site.SiteId, site.SiteName,
                distanceInKm, Math.Round(1 / distanceInKm, 3, MidpointRounding.AwayFromZero));
        });
        var closeSites = sitesWithDistance
            .Where(site => site.DistanceInKm < maxKm)
            .OrderBy(site => site.DistanceInKm);

        var observations = await observationsTask;
        IList<ObservationWithSite> filteredObservationWithSites = [];
        double totalInvertedDistance = 0;
        foreach (var observation in observations)
        {
            var site = closeSites.SingleOrDefault(site => site.SiteId == observation.SiteId);
            if (site == null) continue;
            if (observation is not
                {
                    Parameter: { Frequency: NaqParameterFrequency.HourlyAverage, ParameterCode: NaqParameterCode.Pm25 },
                    Value: not null
                })
                continue;

            totalInvertedDistance += site.InvertedDistance;
            filteredObservationWithSites.Add(new ObservationWithSite(observation, site));
        }

        var totalWeightedPm25 =
            filteredObservationWithSites.Aggregate((double)0,
                (total, joined) => total + (double)joined.Observation.Value *
                    (joined.Site.InvertedDistance / totalInvertedDistance));

        var observationResponses = filteredObservationWithSites.Select(filtered =>
            new ObservationResponse((double)filtered.Observation.Value, filtered.Site, filtered.Observation.Hour));
        return new AirQualityResponse(observationResponses,
            Math.Round(totalWeightedPm25, 3, MidpointRounding.AwayFromZero));
    })
    .WithName("GetAirQuality")
    .WithOpenApi();

app.Run();
return;

async Task<IEnumerable<NaqObservationResult>> FetchObservations()
{
    var observationsResponse = (await naqHttpClient.PostAsync("/api/Data/get_Observations", new StringContent("{\"\"}",
            Encoding.UTF8,
            "application/json")))
        .EnsureSuccessStatusCode();
    var observations = await observationsResponse.Content.ReadFromJsonAsync<IEnumerable<NaqObservationResult>>() ??
                       throw new InvalidOperationException("No observation results found");
    return observations.Where(observation => observation.Value != null);
}

async Task<IEnumerable<NaqSite>> FetchSitesWithLocation()
{
    var observationsResponse = (await naqHttpClient.GetAsync("/api/Data/get_Sitedetails"))
        .EnsureSuccessStatusCode();
    var sites = await observationsResponse.Content.ReadFromJsonAsync<IEnumerable<NaqSite>>() ??
                throw new InvalidOperationException("No sites found");
    return sites.Where(site => site is { Latitude: not null, Longitude: not null, SiteName: not "Test Site" });
}
