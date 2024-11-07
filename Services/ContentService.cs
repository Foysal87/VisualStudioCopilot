using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XatBotVisualStudioCopilot.Services
{
    public static class ContentService
    {
        private static readonly string _contentFolderPath = @"C:\LocalStore\XatBotVisualStudioCopilot\Contents";

        public static string GetAllContents()
        {
            var files = Directory.GetFiles(_contentFolderPath, "*.*")
                .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"))
                .ToArray();

            var allContent = string.Empty;

            foreach (var file in files)
            {
                allContent += File.ReadAllText(file) + Environment.NewLine;
            }

            return allContent.TrimEnd();
        }

        public static string GetContentList(List<string> fileNames)
        {
            var combinedContent = string.Empty;

            foreach (var fileName in fileNames)
            {
                var txtFilePath = Path.Combine(_contentFolderPath, $"{fileName}.txt");
                var mdFilePath = Path.Combine(_contentFolderPath, $"{fileName}.md");

                if (File.Exists(txtFilePath))
                {
                    combinedContent += File.ReadAllText(txtFilePath) + Environment.NewLine;
                }
                else if (File.Exists(mdFilePath))
                {
                    combinedContent += File.ReadAllText(mdFilePath) + Environment.NewLine;
                }
                else
                {
                    throw new FileNotFoundException($"Content file not found: {fileName}.txt or {fileName}.md");
                }
            }

            return combinedContent.TrimEnd();
        }
    }
}
