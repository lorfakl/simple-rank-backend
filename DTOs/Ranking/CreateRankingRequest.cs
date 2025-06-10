namespace simple_rank_backend.DTOs.Ranking
{
    public class CreateRankingRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public RankItemDto[] Items { get; set; }
        public bool IsPublic { get; set; } = false;

        public CreateRankingRequest()
        {
            Title = string.Empty;
            Description = string.Empty;
            Items = Array.Empty<RankItemDto>();
            IsPublic = false;
        }

        public CreateRankingRequest(string name, string description, RankItemDto[] items, bool isPublic = true)
        {
            Title = name;
            Description = description;
            Items = items;
            IsPublic = isPublic;
        }
    }
}