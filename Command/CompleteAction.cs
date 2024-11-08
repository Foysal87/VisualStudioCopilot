using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;
using XatBotVisualStudioCopilot.Services;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using DotNetCopilot.Models;
using XatBotVisualStudioCopilot.Models;

namespace XatBotVisualStudioCopilot.Command
{
    public class CompleteAction : BaseAction
    {
        
        public override async Task<AiRequest> GetAiRequest(SelectionMetaData selectionMetaData)
        {
            var config = ConfigurationService.Instance.GetConfig();
            if (config == null)
            {
                throw new Exception("No Configuration found for GPT 4o");
            }

            var promptContents = GetPromptContents(selectionMetaData);
            var systemMessage = SystemMessageService.GetSystemMessage(GetType());
            var aiRequest = new AiRequest(config, promptContents, systemMessage);
            return aiRequest;
        }

        public override string GetType()
        {
            return "Complete";
        }

        public override string GetCommandType()
        {
            return SelectionCommandType.InsertAfter;
        }

        protected override List<PromptContent> GetPromptContents(SelectionMetaData selectionMetaData)
        {
            var promptContents = new List<PromptContent>();
            var stringBuilder = new StringBuilder();
            var allContents = ContentService.GetAllContents();

            stringBuilder.AppendLine("[Platform Content]:");
            stringBuilder.AppendLine($"{allContents}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"[Full File code] :");
            stringBuilder.AppendLine($"{selectionMetaData.SelectedFileContent}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("[Upper part of the code, where currently user cursor is located] : ");
            stringBuilder.AppendLine($"{selectionMetaData.SelectionAboveCursorContent}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"[Selected Text]:");
            stringBuilder.AppendLine($"{selectionMetaData.SelectionText}");
            stringBuilder.AppendLine();
            promptContents.Add(new PromptContent(PromptContentType.Text, stringBuilder.ToString()));

            return promptContents;
        }


        private static string SelectedFileContent(string selectedFilePath)
        {
            var selectedFileContent = string.Empty;
            if (string.IsNullOrEmpty(selectedFilePath) == false) selectedFileContent = File.ReadAllText(selectedFilePath);
            return selectedFileContent;
        }

        public static (string SelectedText, string GeneratedCode) GetTextAndCode(string jsonString)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                return (obj["SelectedText"], obj["GeneratedCode"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message);
                return (string.Empty, string.Empty);
            }
        }

    }

    public class SelectionCommandType
    {
        public const string InsertAfter = "InsertAfter";
        public const string Replace = "Replace";
        public const string InsertBefore = "InsertBefore";
    }
}