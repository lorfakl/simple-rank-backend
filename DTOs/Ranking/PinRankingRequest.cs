namespace simple_rank_backend.DTOs.Ranking
{
    public class PinRankingRequest
    {
        public bool PinOperation { get; set; }
        public string RankingId { get; set; } = string.Empty;
        
        public PinRankingRequest() { }

    }
}
