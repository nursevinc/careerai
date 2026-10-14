using System;
using System.Collections.Generic;
using System.Text;
using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
using CareerAI.Application.Exceptions;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace CareerAI.Infrastructure.AI;

// OpenAI ile "yapılandırılmış JSON yanıt" konuşmasının ortak mantığı:
// strict JSON schema, zaman aşımı, hata kodlarına çevirme, token/süre loglama.
public class OpenAiJsonClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ChatClient _chat; // thread-safe, tek örnek yeterli
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiJsonClient> _logger;

    public OpenAiJsonClient(OpenAiSettings settings, ILogger<OpenAiJsonClient> logger)
    {
        _settings = settings;
        _logger = logger;
        _chat = new ChatClient(model: settings.Model, apiKey: settings.ApiKey);
    }

    public async Task<T> CompleteAsync<T>(
        string operation,
        string systemPrompt,
        string userMessage,
        string jsonSchema,
        CancellationToken ct = default) where T : class
    {
        var json = await RequestJsonAsync(operation, systemPrompt, userMessage, jsonSchema, ct);
        return Deserialize<T>(json);
    }

    private async Task<string> RequestJsonAsync(
        string operation, string systemPrompt, string userMessage, string jsonSchema, CancellationToken ct)
    {
        var options = new ChatCompletionOptions
        {
            // Strict structured outputs: yanıt şemaya birebir uymak zorunda
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: operation,
                jsonSchema: BinaryData.FromString(jsonSchema),
                jsonSchemaIsStrict: true),
            MaxOutputTokenCount = _settings.MaxOutputTokens
        };

        if (_settings.Temperature is { } temperature)
            options.Temperature = temperature;

        List<ChatMessage> messages =
        [
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userMessage)
        ];

        // Toplam süre sınırı (SDK'nın kendi yeniden denemeleri dahil)
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

        var stopwatch = Stopwatch.StartNew();
        ChatCompletion completion;

        try
        {
            completion = await _chat.CompleteChatAsync(messages, options, timeoutCts.Token);
        }
        catch (ClientResultException ex)
        {
            // Prompt, CV metni ve API anahtarı loglanmaz: sadece durum kodu ve OpenAI'ın kısa hata mesajı
            _logger.LogError("OpenAI request failed ({Operation}). Status {Status}: {Message}",
                operation, ex.Status, Truncate(ex.Message, 300));

            // 429: istek sınırı ya da bakiye/kota bitti. Diğerleri: anahtar, model adı, sunucu hatası...
            throw new AiServiceException(
                ex.Status == 429 ? "ai_busy" : "ai_unavailable",
                "The AI service could not process the request.",
                ex);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // İstemci vazgeçmedi, bizim zaman aşımımız doldu
            _logger.LogError("OpenAI request timed out ({Operation}) after {Seconds} s",
                operation, _settings.TimeoutSeconds);
            throw new AiServiceException("ai_unavailable", "The AI service did not respond in time.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "OpenAI request could not be sent ({Operation})", operation);
            throw new AiServiceException("ai_unavailable", "The AI service could not be reached.", ex);
        }

        stopwatch.Stop();
        _logger.LogInformation(
            "AI call {Operation} finished in {ElapsedMs} ms (model {Model}, input {InputTokens} tokens, output {OutputTokens} tokens, finish: {FinishReason})",
            operation,
            stopwatch.ElapsedMilliseconds,
            _settings.Model,
            completion.Usage?.InputTokenCount,
            completion.Usage?.OutputTokenCount,
            completion.FinishReason);

        if (!string.IsNullOrEmpty(completion.Refusal))
            throw new AiServiceException("ai_refused", "The AI model refused to process this content.");

        // Length = çıktı token sınırına takıldı, JSON yarım kalmış olabilir
        if (completion.FinishReason != ChatFinishReason.Stop)
            throw new AiServiceException(
                "ai_invalid_response",
                $"The AI response was incomplete (finish reason: {completion.FinishReason}).");

        var text = completion.Content.Count > 0 ? completion.Content[0].Text : null;
        if (string.IsNullOrWhiteSpace(text))
            throw new AiServiceException("ai_invalid_response", "The AI response was empty.");

        return text;
    }

    private static T Deserialize<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                   ?? throw new JsonException("Deserialized value was null.");
        }
        catch (JsonException ex)
        {
            throw new AiServiceException("ai_invalid_response", "The AI response could not be parsed.", ex);
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}