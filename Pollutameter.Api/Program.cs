using System.Collections.Frozen;
using Geolocation;
using Pollutameter.Api.Domain;
using Pollutameter.Api.Naq;
using Pollutameter.Api.Response;

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
        var sites = await naqApi.FetchSites();
        var sitesWithDistance = sites.Select(site =>
        {
            var distanceInKm = GeoCalculator.GetDistance(latitude, longitude, (double)site.Latitude,
                (double)site.Longitude, 1, DistanceUnit.Kilometers);
            return new NaqSiteWithDistance(site.SiteId, site.SiteName,
                distanceInKm, Math.Round(1 / distanceInKm, 3, MidpointRounding.AwayFromZero));
        });
        var siteGroups = sitesWithDistance
            .Where(site => site.DistanceInKm < maxKm)
            .ToFrozenDictionary(site => site.SiteId,
                site => new SiteGroup(new List<NaqObservationResult>(), site));

        var observations = await observationsTask;
        foreach (var observation in observations)
        {
            if (!siteGroups.TryGetValue(observation.SiteId, out var siteGroup)) continue;
            if (observation is not
                {
                    Parameter: { Frequency: NaqParameterFrequency.HourlyAverage },
                    Value: not null
                } || !NaqParameterCode.AllCodes.Contains(observation.Parameter.ParameterCode))
                continue;

            siteGroup.Observations.Add(observation);
        }

        return new AirQualityResponse(siteGroups.Values);
    })
    .WithName("GetAirQuality")
    .WithOpenApi();

app.Run();
