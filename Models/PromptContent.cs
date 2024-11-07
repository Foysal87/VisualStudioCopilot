namespace DotNetCopilot.Models
{
    public class PromptContent
    {
        public string Type { get; set; }
        public string Content { get; set; }

        public PromptContent(string type, string content)
        {
            Type = type;
            Content = content;
        }
    }
}
