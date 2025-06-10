namespace simple_rank_backend.DTOs.Ranking
{
    public class UpdateRankItemPlacementRequest
    {
        public string RankingId { get; set; } = string.Empty;
        public Dictionary<string, int> ItemIdToRank { get; set; } = new Dictionary<string, int>();
        public UpdateRankItemPlacementRequest() { }
        public UpdateRankItemPlacementRequest(string rankingId, Dictionary<string, int> idToRankMap)
        {
            RankingId = rankingId;
            ItemIdToRank = idToRankMap;
        }
    }
}
