using System.Collections.Frozen;
using Geolocation;
using Microsoft.Extensions.Options;
using Pollutameter.Api.Configuration;
using Pollutameter.Api.Domain;
using Pollutameter.Api.Naq;
using Pollutameter.Api.Response;

var builder = WebApplication.CreateBuilder(args);

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<NaqOptions>().BindConfiguration(nameof(NaqOptions));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var naqOptions = app.Services.GetService<IOptions<NaqOptions>>();
if (naqOptions == null)
    throw new InvalidOperationException("NaqOptions section required in appsettings.json or similar");
var naqApi = new NaqApi(naqOptions.Value.BaseUri);

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
            if (
                observation.Value == null
                || !NaqParameterFrequency.AllFrequencies.Contains(observation.Parameter.Frequency)
                || !NaqParameterCode.AllCodes.Contains(observation.Parameter.ParameterCode)
            )
                continue;

            siteGroup.Observations.Add(observation);
        }

        return new AirQualityResponse(siteGroups.Values);
    })
    .WithName("GetAirQuality")
    .WithOpenApi();

await app.RunAsync();
