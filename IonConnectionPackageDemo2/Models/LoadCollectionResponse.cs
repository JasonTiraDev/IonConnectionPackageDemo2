namespace IonConnectionPackageDemo2.Models;

public class LoadCollectionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<Dictionary<string, object>> Items { get; set; } = new List<Dictionary<string, object>>();
    public string Bookmark { get; set; }
    public bool MoreRowsExist { get; set; }
}
