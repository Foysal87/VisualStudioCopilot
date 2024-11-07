
using System.Collections.Generic;

public class ApplicationSettings
{
    public AzureOpenAiSettings AzureOpenAi { get; set; }
    public ClaudeAiSettings ClaudeAi { get; set; }
}

public class AzureOpenAiSettings
{
    public List<Config> GPT4 { get; set; }
    public List<Config> GPT35 { get; set; }
    public List<Config> GPT3516k { get; set; }
    public List<Config> GPT4120K { get; set; }
}


public class ClaudeAiSettings
{
    public List<Config> Haiku { get; set; }
    public List<Config> Sonnet { get; set; }
    public List<Config> Opus { get; set; }
    public List<Config> Sonnet35 { get; set; }
}

