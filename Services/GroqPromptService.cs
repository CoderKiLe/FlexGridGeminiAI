using FlexGridGeminiAI.Data;
using FlexGridGeminiAI.ErrorHandling;
using FlexGridGeminiAI.Helpers;
using FlexGridGeminiAI.Interface;
using FlexGridGeminiAI.Services.ResponseParser;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace FlexGridGeminiAI.Services
{
    public class GroqPromptService : IAIModel
    {
        public string Name => "Groq";

        private readonly string? _apiKey;
        private readonly string? _baseUrl;

        public GroqPromptService(string? apiKey, string? baseUrl)
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl;
        }
        public async Task<string> GetResponse(string prompt)
        {
            try
            {
                using var client = new HttpClient();

                if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_baseUrl))
                {
                    var error = ErrorHandler.FromModelError(
                        ErrorCode.ModelMissingConfig,
                        "Groq API key or base URL is missing.",
                        "GroqPromptService initialization issue");
                    return error.ToString();
                }

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _apiKey);

                var requestPayload = new
                {
                    model = "meta-llama/llama-4-scout-17b-16e-instruct",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var json = JsonConvert.SerializeObject(requestPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_baseUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = ErrorHandler.FromHttpStatus((int)response.StatusCode, response.ReasonPhrase);
                    return error.ToString();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var parsedResponse = GroqResponseParser.ParseGroqResponse(responseJson);

                return parsedResponse;
            }
            catch (HttpRequestException httpEx)
            {
                var error = ErrorHandler.FromModelError(
                    ErrorCode.ModelTimeout,
                    "Network error while contacting Groq API.",
                    httpEx.Message,
                    httpEx);
                return error.ToString();
            }
            catch (Exception ex)
            {
                var error = ErrorHandler.FromModelError(
                    ErrorCode.ModelUnsupportedOperation,
                    "Unexpected error while parsing Groq response.",
                    ex.Message,
                    ex);
                return error.ToString();
            }
        }
    }
}