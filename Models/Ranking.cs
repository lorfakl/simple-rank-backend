using simple_rank_backend.Application.Services;
using simple_rank_backend.Application.Services.Interfaces;
using simple_rank_backend.DTOs.Ranking;
using System.Xml.Linq;

namespace simple_rank_backend.Models
{
    public class Ranking
    {
        private string RankingId { get; set; } = string.Empty;
        public string Owner { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public List<RankItem> Items { get; private set; } = new List<RankItem>();
        public uint ItemCount { get; private set; } = 0;
        public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;
        public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
        public DateTime PinnedTime { get; private set; }
        public bool IsPublic { get; private set; } = false;
        public bool IsShared { get; private set; } = false;
        public bool IsPinned { get; private set; } = false;
        public UserMetadata CreatedBy { get; set; } = default;

        public string Id
        {
            get => RankingId;
        }


        public Ranking() { }

        public Ranking(CreateRankingRequest rq, string owner)
        {
            RankingId = HashService.GenerateRobustHash($"{rq.Title}:{owner}");
            Owner = owner;
            Title = rq.Title;
            Description = rq.Description;
            IsPublic = rq.IsPublic;
            Items = rq.Items.Select(item => new RankItem(item.Name, RankingId, owner, item.Description, item.Rank)).ToList();
            ItemCount = (uint)Items.Count;
            LastUpdated = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
        }

        public Ranking(string owner, string name, string description)
        {
            RankingId = HashService.GenerateRobustHash($"{name}:{owner}");
            Owner = owner;
            Title = name;
            Description = description;
            ItemCount = 0;
            LastUpdated = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
        }

        public Ranking(string owner, string name, string description, List<RankItem> items)
        {
            RankingId = HashService.GenerateRobustHash($"{name}:{owner}");
            Owner = owner;
            Title = name;
            Description = description;
            ItemCount = (uint)items.Count;
            Items = items;
            LastUpdated = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
        }

        public Ranking(TableModels.Ranking ranking)
        {
            RankingId = ranking.RankingId;
            Owner = ranking.Owner;
            Title = ranking.Name;
            Description = ranking.Description;
            ItemCount = ranking.ItemCount;
            LastUpdated = ranking.LastUpdated;
            CreatedDate = ranking.CreatedDate;
            PinnedTime = ranking.PinnedTime;
            IsPublic = ranking.IsPublic;
            IsShared = ranking.IsShared;
            IsPinned = ranking.IsPinned;
            // Assuming we have a method to fetch items by RankingId
            if(ranking.RankingItems.Any())
            {
                try
                {
                    Items = ranking.RankingItems
                    .Select(item => new RankItem(item.Name, item.Description, ranking.ItemPlacement[item.ItemId], item.ItemId))
                    .OrderBy(item => ranking.ItemPlacement[item.ItemId])
                    .ToList();
                }
                catch(Exception ex)
                {
                    Items = new List<RankItem>();
                }
                
            }
            else
            {
                Items = new List<RankItem>();
            }
            
        }

        public void AddItem(RankItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.Name) || string.IsNullOrEmpty(item.ItemId))
                throw new ArgumentException("Invalid item provided.");
            item.Rank = ItemCount + 1;
            Items.Add(item);
            ItemCount++;
            LastUpdated = DateTime.UtcNow;
        }

        public void RemoveItem(string itemId)
        {
            var item = Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                Items.Remove(item);
                ItemCount--;
                LastUpdated = DateTime.UtcNow;
            }
            else
            {
                throw new KeyNotFoundException("Item not found in the ranking.");
            }
        }

        public void UpdateItemRank(string itemId, uint newRank)
        {
            var item = Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                if (newRank < 1 || newRank > ItemCount)
                    throw new ArgumentOutOfRangeException("Rank must be between 1 and the total number of items.");

                item.Rank = newRank;
                LastUpdated = DateTime.UtcNow;
            }
            else
            {
                throw new KeyNotFoundException("Item not found in the ranking.");
            }
        }
    }
}
