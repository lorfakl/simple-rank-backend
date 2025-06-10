namespace simple_rank_backend.DTOs
{

    public class ReactDto
    {
        public string ReactionId { get; set; } = string.Empty;
        public string RankingId { get; set; } = string.Empty;
        public bool IsShared { get; set; }
    }
}
