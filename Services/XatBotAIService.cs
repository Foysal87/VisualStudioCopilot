using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XatBotVisualStudioCopilot.Services
{
    public class XatBotAIService
    {
        private static readonly object LockObject = new object();
        private static XatBotAIService _instance;

        private readonly AzureAiClient _azureAiClient;

        private XatBotAIService(ConfigurationService configService)
        {
            // Initialize the AzureAiClient with the first available config for AzureOpenAi
            var config = configService.GetConfig();
            if (config != null)
            {
                _azureAiClient = new AzureAiClient(config);
            }
            else
            {
                throw new Exception("Configuration for AzureOpenAi GPT-4o-mini not found.");
            }
        }

        public static XatBotAIService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new XatBotAIService(ConfigurationService.Instance);
                        }
                    }
                }

                return _instance;
            }
        }

        public async Task<RawResponse> ChatAsync(List<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            if (_azureAiClient == null)
                throw new InvalidOperationException("AzureAiClient is not initialized.");

            return await _azureAiClient.ChatAsync(messages, cancellationToken);
        }

        public AsyncCollectionResult<StreamingChatCompletionUpdate> ChatStreamingAsync(List<ChatMessage> messages, CancellationTokenSource tokenSource)
        {
            if (_azureAiClient == null)
                throw new InvalidOperationException("AzureAiClient is not initialized.");

            return _azureAiClient.ChatStreamingAsync(messages, tokenSource);
        }

    }
}
