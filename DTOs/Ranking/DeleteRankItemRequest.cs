namespace simple_rank_backend.DTOs.Ranking
{
    public class DeleteRankItemRequest
    {
        public string RankingId { get; set; } = string.Empty;
        public string RankItemId { get; set; } = string.Empty;
        public DeleteRankItemRequest() { }
        public DeleteRankItemRequest(string rankingId, string itemId)
        {
            RankingId = rankingId;
            RankItemId = itemId;
        }
    }
}
