using FlexGridGeminiAI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FlexGridGeminiAI.Models
{
    public class AppError
    {
        public ErrorCode Code { get; }
        public string Message { get; }
        public string? Details { get; }
        public Exception? Exception { get; }

        public AppError(ErrorCode code, string message, string? details = null, Exception exception = null)
        {
            Code = code;
            Message = message;
            Details = details;
            Exception = exception;
        }
        public override string ToString()
        {
            return $"[{(int)Code} {Code}] {Message}" +
               (Details != null ? $" - {Details}" : "") +
               (Exception != null ? $" - Exception: {Exception.Message}" : "");
        }
    }
}
