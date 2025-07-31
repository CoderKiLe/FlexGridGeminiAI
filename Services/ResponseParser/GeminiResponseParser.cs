using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FlexGridGeminiAI.Services.ResponseParser
{
    public class GeminiResponseParser
    {
        /// <summary>
        /// Parses a raw JSON response from the Gemini API and extracts relevant content.
        /// </summary>
        /// <remarks>This method expects the JSON to follow a specific structure, including a "candidates"
        /// array  with nested "content" and "parts" objects. If the structure is invalid or the required fields  are
        /// missing, the method will return an error message instead of throwing an exception.</remarks>
        /// <param name="rawJson">The raw JSON string returned by the Gemini API.</param>
        /// <returns>A string containing the extracted content if successful. If the content is empty or not relevant,  a
        /// descriptive message is returned. If parsing fails, an error message is returned.</returns>
        public static string ParseGeminiResponse(string rawJson)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(rawJson))
                {
                    var root = doc.RootElement;

                    var text = root.GetProperty("candidates")[0]
                                   .GetProperty("content")
                                   .GetProperty("parts")[0]
                                   .GetProperty("text")
                                   .GetString();

                    if (string.IsNullOrWhiteSpace(text))
                        return "No text found in response.";

                    // Extract the part inside triple backticks
                    var code = ExtractXmlFromBackticks(text);

                    return string.IsNullOrWhiteSpace(code)
                        ? "Prompt is not related to the Data."
                        : code.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"Failed to parse Gemini response: {ex.Message}";
            }
        }

        private static string ExtractXmlFromBackticks(string text)
        {
            var match = Regex.Match(text, @"```(?:xml)?\s*(.*?)\s*```", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
