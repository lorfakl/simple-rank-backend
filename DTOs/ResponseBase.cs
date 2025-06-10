namespace simple_rank_backend.DTOs
{
    public class ResponseBase
    {
        public string Message { get; set; } = string.Empty;

        public ResponseBase() { }

        public ResponseBase(string message) 
        { 
            Message = message;
        }
    }
}
