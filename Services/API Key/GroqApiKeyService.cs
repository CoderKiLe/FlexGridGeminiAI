using FlexGridGeminiAI.Interface;
using FlexGridGeminiAI.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexGridGeminiAI.Services
{
    public class GroqApiKeyService : IApiKeyService
    {
        private static string key = "GROQ_API_KEY";

        public bool ApiKeyExists()
        {
            string? GROQ_API_KEY = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
            return !string.IsNullOrEmpty(GROQ_API_KEY);
        }

        public string? GetApiKey()
        {
            string? GROQ_API_KEY = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
            return GROQ_API_KEY ?? string.Empty;
        }

        public void SetApiKey(string value)
        {
            Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.User);
        }
    }
}
