namespace simple_rank_backend.DTOs.Ranking
{
    public class UpdateRankingRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<RankItemDto> Items { get; set; }
        public bool IsPublic { get; set; } = false;
        
        public UpdateRankingRequest()
        {
            Id = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            Items = new List<RankItemDto>();
            IsPublic = false;
        }

        public UpdateRankingRequest(string id, string title, string description, List<RankItemDto> items, bool isPublic = true)
        {
            Id = id;
            Title = title;
            Description = description;
            Items = items;
            IsPublic = isPublic;
        }
    }
}
