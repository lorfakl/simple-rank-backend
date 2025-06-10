namespace simple_rank_backend.DTOs.Ranking.Responses
{
    public class CreateRankingResponse
    {
        public string Id { get; set; }
        public string Message { get; set; }

        public CreateRankingResponse() 
        {
            Id = string.Empty;
            Message = string.Empty;
        }

        public CreateRankingResponse(string id, string message)
        {
            Id = id;
            Message = message;
        }

    }
}
