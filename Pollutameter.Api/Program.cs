using Geolocation;
using Pollutameter.Web.Models;
using Pollutameter.Web.Naq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthorization();
var naqApi = new NaqApi();

app.MapGet("/air-quality", async (double latitude, double longitude, double maxKm = 10) =>
    {
        var observationsTask = naqApi.FetchObservations();
        var sites = await naqApi.FetchSitesWithLocation();
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
