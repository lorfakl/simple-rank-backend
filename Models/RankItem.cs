using simple_rank_backend.Application.Services;

namespace simple_rank_backend.Models
{
    public class RankItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public uint Rank { get; set; } = 0;
        public string ItemId { get; set; }

        public RankItem()
        {
            Name = string.Empty;
            Description = string.Empty;
            ItemId = string.Empty;
        }

        public RankItem(string name, string description, int rank, string itemId)
        {
            Name = name;
            Description = description;
            Rank = (uint)rank;
            ItemId = itemId;
        }

        public RankItem(string name, string rankingId, string owner, string description)
        {
            Name = name;
            Description = description;
            ItemId = HashService.GenerateRobustHash($"{name}:{rankingId}:{owner}");
        }

        public RankItem(string name, string rankingId, string owner, string description, uint rank)
        {
            Name = name;
            Description = description;
            ItemId = HashService.GenerateRobustHash($"{name}:{rankingId}:{owner}");
            Rank = rank;
        }
    }
}
