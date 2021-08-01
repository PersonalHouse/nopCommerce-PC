using Newtonsoft.Json;

namespace SignalChannel.Common
{
    public class ErrorInfo
    {
        public string Error { get; }

        public string Message { get; }

        public ErrorInfo(string error, string message)
        {
            this.Error = error;
            this.Message = message;
        }

        public static ErrorInfo FromJson(string json)
        {
            try
            {
                var errorInfo = JsonConvert.DeserializeObject<ErrorInfo>(json);
                if (!string.IsNullOrWhiteSpace(errorInfo.Error))
                {
                    return errorInfo;
                }
            }
            catch
            {
                // Ignore
            }
            return null;
        }
    }

    public static class ApiErrors
    {
        public static ErrorInfo Invalid_Request(string message) => new ErrorInfo("invalid_request", message);

        public static ErrorInfo Invalid_Client(string message) => new ErrorInfo("invalid_client", message);

        public static ErrorInfo Invalid_Grant(string message) => new ErrorInfo("invalid_grant", message);

        public static ErrorInfo Fatal_Error(string message) => new ErrorInfo("fatal_error", message);
    }
}
