using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexGridGeminiAI.Data
{
    public enum ErrorCode
    {
        ApiUnauthorized = 401,
        ApiPaymentRequired = 402,
        ApiForbidden = 403,
        ApiNotFound = 404,
        ApiInternalError = 500,
        ApiServiceUnavailable = 503,
        ModelMissingConfig = 600,
        ModelInvalidInput = 601,
        ModelTimeout = 602,
        ModelUnsupportedOperation = 603
    }
}
