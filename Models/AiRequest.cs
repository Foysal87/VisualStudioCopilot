using System.Collections.Generic;
using DotNetCopilot.Models;


public class AiRequest
{
    public Config Config { get; set; }
    public List<PromptContent> PromptContents { get; set; }
    public string SystemMessage { get; set; }
    public List<RawHistory> RawHistories { get; set; }
    public Dictionary<string,string> FieldValues { get; set; }

    public AiRequest(Config config, List<PromptContent> promptContents, string systemMessage, List<RawHistory> rawHistories = null)
    {
        Config = config;
        PromptContents = promptContents;
        SystemMessage = systemMessage;
        RawHistories = rawHistories;
        FieldValues = new Dictionary<string,string>();
    }

    public void SetValue(string key, string value)
    {
        FieldValues.Add(key, value);
    }

    public string GetValue(string key)
    {
        return FieldValues[key];
    }
}