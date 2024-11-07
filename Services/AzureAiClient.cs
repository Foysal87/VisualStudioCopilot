
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using DotNetCopilot.Models;
using OpenAI;
using OpenAI.Chat;


public class AzureAiClient
{
    public string Type { get; set; }
    public string ModelName { get; set; }

    private readonly OpenAIClient _openAiClient;
    private readonly ChatCompletionOptions _chatCompletionsOptions;
    private readonly ChatClient _chatClient;

    public AzureAiClient(Config config, string type = "AzureOpenAi")
    {
        _openAiClient = CreateOpenAiClient(config);
        _chatCompletionsOptions = CreateChatCompletionsOptions(config);
        Type = type;
        ModelName = config.ModelName;
        _chatClient = _openAiClient.GetChatClient(ModelName);
    }

    private AzureOpenAIClient CreateOpenAiClient(Config xatBotConfig)
    {
        var apiBase = xatBotConfig.ApiUrl;
        var apiKey = xatBotConfig.ApiKey;
        var openAiClientOptions = new AzureOpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromSeconds(300)
        };
        var endpoint = new Uri(apiBase);
        return new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey), openAiClientOptions);
    }

    private ChatCompletionOptions CreateChatCompletionsOptions(Config config)
    {
        return new ChatCompletionOptions
        {
            Temperature = config.Temperature ?? 0.7f,
            MaxOutputTokenCount = config.MaxTokens ?? 3000,
            TopP = config.NucleusSamplingFactor ?? 1.0f,
            PresencePenalty = config.PresencePenalty ?? 0.0f,
            FrequencyPenalty = config.FrequencyPenalty ?? 0.0f
        };
    }

    public async Task<RawResponse> ChatAsync(List<ChatMessage> chatMessages, CancellationToken cancellationToken = default)
    {
        var chatResponse = await _chatClient.CompleteChatAsync(chatMessages, _chatCompletionsOptions, cancellationToken);
        var rawResponse = ParseRawResponse(chatResponse);
        return rawResponse;
    }

    public AsyncCollectionResult<StreamingChatCompletionUpdate> ChatStreamingAsync(List<ChatMessage> chatMessages, CancellationTokenSource tokenSource)
    {
        return _chatClient.CompleteChatStreamingAsync(chatMessages, _chatCompletionsOptions, tokenSource.Token);
    }

    public List<ChatMessage> GetChatMessages(string systemMessage, List<PromptContent> promptContents, List<RawHistory> histories = null)
    {
        var chatMessages = new List<ChatMessage>
            {
                new SystemChatMessage(systemMessage)
            };

        if (histories != null)
        {
            foreach (var rawHistory in histories)
            {
                chatMessages.Add(new UserChatMessage(rawHistory.UserMessage));
                chatMessages.Add(new AssistantChatMessage(rawHistory.AssistantMessage));
            }
        }
        chatMessages.Add(new UserChatMessage(GetContentPartList(promptContents)));
        return chatMessages;
    }

    private List<ChatMessageContentPart> GetContentPartList(List<PromptContent> promptContents)
    {
        var contentPartList = new List<ChatMessageContentPart>();
        foreach (var promptContent in promptContents)
        {
            switch (promptContent.Type)
            {
                case PromptContentType.Text:
                    contentPartList.Add(ChatMessageContentPart.CreateTextPart(promptContent.Content));
                    break;
            }

        }
        return contentPartList;
    }

    private RawResponse ParseRawResponse(ChatCompletion chatCompletion)
    {
        return new RawResponse()
        {
            Text = chatCompletion.Content.FirstOrDefault()?.Text
        };
    }
}



