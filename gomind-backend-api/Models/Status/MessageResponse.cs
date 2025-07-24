namespace gomind_backend_api.Models.Errors
{
    public class MessageResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; } 

        private MessageResponse(string message, bool success = true)
        {
            Success = success;
            Message = message;            
        }

        private MessageResponse(string message, Dictionary<string, string[]> errors, bool success = false)
        {
            Success = success;
            Message = message;
            Errors = errors;
        }
        public static MessageResponse Create(string message)
        {
            return new MessageResponse(message, false);
        }
        public static MessageResponse Create(string message, bool success)
        {
            return new MessageResponse(message, success);
        }
        public static MessageResponse Create(string message, Dictionary<string, string[]> errors)
        {
            return new MessageResponse(message, errors, false);
        }
    }
}
