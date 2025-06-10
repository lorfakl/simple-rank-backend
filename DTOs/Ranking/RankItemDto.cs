namespace simple_rank_backend.DTOs.Ranking
{
    public class RankItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public uint Rank { get; set; } = 0;

        public RankItemDto() { }

        public RankItemDto(string id, string name, string description, uint rank)
        {
            Id = id;
            Name = name;
            Description = description;
            Rank = rank;
        }
    }
}
