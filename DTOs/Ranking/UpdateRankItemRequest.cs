namespace simple_rank_backend.DTOs.Ranking
{
    public class UpdateRankItemRequest
    {
        public string RankingId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public uint Rank { get; set; } = 0;
        public UpdateRankItemRequest() { }
        public UpdateRankItemRequest(string rankingId, string itemId, string name, string description, uint rank)
        {
            RankingId = rankingId;
            ItemId = itemId;
            Name = name;
            Description = description;
            Rank = rank;
        }
    }
}
