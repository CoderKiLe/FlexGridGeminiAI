using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlexGridGeminiAI.Services.ResponseParser
{
    public class GroqResponseParser
    {
        /// <summary>
        /// Parses the raw JSON response from the Groq API and extracts the assistant's reply content.
        /// </summary>
        /// <remarks>
        /// Expects OpenAI-compatible structure: "choices" → "message" → "content".
        /// </remarks>
        /// <param name="rawJson">Raw JSON string returned by the Groq API.</param>
        /// <returns>Extracted assistant response or descriptive error message.</returns>
        public static string ParseGroqResponse(string rawJson)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(rawJson))
                {
                    var root = doc.RootElement;

                    var content = root.GetProperty("choices")[0]
                                      .GetProperty("message")
                                      .GetProperty("content")
                                      .GetString();

                    if (string.IsNullOrWhiteSpace(content))
                        return "Groq response is empty or malformed.";

                    var code = ExtractXmlFromBackticks(content);

                    return string.IsNullOrWhiteSpace(code)
                        ? content.Trim() // fallback: return full content
                        : code.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"Failed to parse Groq response: {ex.Message}";
            }
        }

        private static string ExtractXmlFromBackticks(string text)
        {
            var match = Regex.Match(text, @"```(?:xml)?\s*(.*?)\s*```", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
