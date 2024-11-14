namespace IonConnectionPackageDemo2.Models;

public class ConfigurationResponse
{
    public string? Message { get; set; }
    public bool Success { get; set; }
    public List<string> Configurations { get; set; } = new List<string>();
}
