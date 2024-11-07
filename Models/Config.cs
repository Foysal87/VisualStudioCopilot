
public class Config
{
    public string ApiUrl { get; set; }
    public string ApiKey { get; set; }
    public string ModelName { get; set; }
    public string ApiVersion { get; set; }
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public float? NucleusSamplingFactor { get; set; }
    public float? PresencePenalty { get; set; }
    public float? FrequencyPenalty { get; set; }
}