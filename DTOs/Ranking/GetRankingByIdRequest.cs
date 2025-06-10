namespace simple_rank_backend.DTOs.Ranking
{
    public class GetRankingByIdRequest
    {
        public string Id { get; set; } = string.Empty;
        public GetRankingByIdRequest() { }
        public GetRankingByIdRequest(string id)
        {
            Id = id;
        }
    }
}
