using System;
using System.Collections.Generic;

namespace Unity.Services.Authentication
{
    class WebRequestException : Exception
    {
        public bool NetworkError { get; private set; }
        public bool DeserializationError { get; private set; }
        public bool ServerError { get; private set; }
        public long ResponseCode { get; private set; }
        public IDictionary<string, string> ResponseHeaders { get; private set; }

        internal WebRequestException(bool isNetworkError, bool isServerError, bool isDeserializationError, long responseCode, string errorMessage, IDictionary<string, string> responseHeaders = null) : base(errorMessage)
        {
            NetworkError = isNetworkError;
            ServerError = isServerError;
            DeserializationError = isDeserializationError;
            ResponseCode = responseCode;
            ResponseHeaders = responseHeaders;
        }
    }
}
