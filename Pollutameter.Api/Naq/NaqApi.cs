using System.Text;

namespace Pollutameter.Api.Naq;

public class NaqApi
{
    private readonly HttpClient _naqHttpClient;

    public NaqApi(string baseUri)
    {
        _naqHttpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUri)
        };

        _naqHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<IEnumerable<NaqObservationResult>> FetchObservations()
    {
        var observationsResponse = (await _naqHttpClient.PostAsync("/api/Data/get_Observations", new StringContent(
                "{\"\"}",
                Encoding.UTF8,
                "application/json")))
            .EnsureSuccessStatusCode();
        var observations = await observationsResponse.Content.ReadFromJsonAsync<IEnumerable<NaqObservationResult>>() ??
                           throw new InvalidOperationException("No observation results found");
        return observations.Where(observation => observation.Value != null);
    }

    public async Task<IEnumerable<NaqSite>> FetchSites()
    {
        var observationsResponse = (await _naqHttpClient.GetAsync("/api/Data/get_Sitedetails"))
            .EnsureSuccessStatusCode();
        var sites = await observationsResponse.Content.ReadFromJsonAsync<IEnumerable<NaqSite>>() ??
                    throw new InvalidOperationException("No sites found");
        return sites.Where(site => site is { Latitude: not null, Longitude: not null, SiteName: not "Test Site" });
    }
}
