namespace HydraEnergyAPI.Models.DTOs;

public class InsightResult
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info"; // info, warning, critical
    public DateTime? RelatedDate { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class InsightsSummary
{
    public List<InsightResult> Insights { get; set; } = new();
    public string OverallAssessment { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
