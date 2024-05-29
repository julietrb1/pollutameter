namespace Pollutameter.Web.NAQ;

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
    public const string HourlyAverage = "Hourly average";
}

public static class NaqParameterCode
{
    public const string Pm25 = "PM2.5";
}