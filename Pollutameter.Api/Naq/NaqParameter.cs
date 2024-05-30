namespace Pollutameter.Api.Naq;

public record NaqParameter(
    string ParameterCode,
    string ParameterDescription,
    string Units,
    string UnitsDescription,
    string Category,
    string SubCategory,
    string Frequency);

public static class NaqParameterFrequency
{
    public const string AverageForHour = "Hourly average";
    public const string AverageFor24H = "24h rolling average derived from 1h average";
    public static IReadOnlySet<string> AllFrequencies = new HashSet<string>([AverageForHour, AverageFor24H]);
}

public static class NaqParameterCode
{
    public const string Pm25 = "PM2.5";
    public const string Pm10 = "PM10";
    public static IReadOnlySet<string> AllCodes = new HashSet<string>([Pm25, Pm10]);
}
