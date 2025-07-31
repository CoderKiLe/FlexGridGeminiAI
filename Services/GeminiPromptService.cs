using FlexGridGeminiAI.Data;
using FlexGridGeminiAI.ErrorHandling;
using FlexGridGeminiAI.Helpers;
using FlexGridGeminiAI.Interface;
using FlexGridGeminiAI.Services.ResponseParser;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace FlexGridGeminiAI.Services
{
    /// <summary>
    /// Gemini Service for connecting to the Gemini and giving the prompting response
    /// </summary>
    public class GeminiPromptService : IAIModel
    {
        public string Name => "Gemini";

        private readonly IApiKeyService geminKey = new GeminiApiKeyService();

        private readonly string? _urlToModel;
        private readonly string? _baseUrl;
        private readonly string? _apiKey;

        public GeminiPromptService(string? apiKey, string? baseUrl)
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl;
            _urlToModel = BuildUrl.BuildRequestUrl(_apiKey, _baseUrl);
        }

        public async Task<string> GetResponse(string prompt)
        {
            try
            {
                using var client = new HttpClient();

                var requestPayload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(requestPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_urlToModel, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = ErrorHandler.FromHttpStatus((int)response.StatusCode, response.ReasonPhrase);
                    return error.ToString();
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var parsedResponse = GeminiResponseParser.ParseGeminiResponse(responseJson);

                return parsedResponse;
            }
            catch (HttpRequestException httpEx)
            {
                var error = ErrorHandler.FromModelError(
                    ErrorCode.ModelTimeout,
                    "Network error while contacting Gemini API.",
                    httpEx.Message,
                    httpEx);
                return error.ToString();
            }
            catch (Exception ex)
            {
                var error = ErrorHandler.FromModelError(
                    ErrorCode.ModelUnsupportedOperation,
                    "Unexpected error in Gemini response flow.",
                    ex.Message,
                    ex);
                return error.ToString();
            }
        }
    }
}