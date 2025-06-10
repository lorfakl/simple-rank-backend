namespace simple_rank_backend.DTOs.Ranking
{
    public class CreateRankingItemRequest
    {
        public RankItemDto Item { get; set; }
        public string RankingId { get; set; }
     

        public CreateRankingItemRequest()
        {
            Item = new RankItemDto();
            RankingId = string.Empty;
            
        }

        public CreateRankingItemRequest(string name, string description, string rankingId, string id, int rank)
        {
            Item = new RankItemDto(id, name, description, (uint)rank);
            RankingId = rankingId;
        }
    }
}
