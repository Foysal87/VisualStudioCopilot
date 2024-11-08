using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XatBotVisualStudioCopilot.Services
{
    public static class SystemMessageService
    {
        private static readonly string _systemMessagesFolderPath = @"C:\LocalStore\XatBotVisualStudioCopilot\SystemMessages";

        public static string GetSystemMessage(string type)
        {
            var filePath = Path.Combine(_systemMessagesFolderPath, $"{type}ActionSystemMessage.txt");
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"System message file not found: {filePath}");

            return File.ReadAllText(filePath);
        }
    }
}
