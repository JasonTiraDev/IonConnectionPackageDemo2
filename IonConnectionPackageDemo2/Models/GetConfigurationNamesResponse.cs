namespace IonConnectionPackageDemo2.Models;

public class GetConfigurationNamesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Configurations { get; set; } = new List<string>();
}
