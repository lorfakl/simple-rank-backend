using simple_rank_backend.Application.Services;
using simple_rank_backend.DTOs.Ranking;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace simple_rank_backend.TableModels
{
    [Table("Ranking")]
    public class Ranking: BaseModel
    {
        [PrimaryKey("ranking_id")]
        public string RankingId { get; set; } = string.Empty;

        [Column("owner")]  
        public string Owner { get; set; } = string.Empty;

        [Column("ranking_name")]
        public string Name { get; set; } = string.Empty;

        [Column("ranking_desc")]
        public string Description { get; set; } = string.Empty;

        [Column("item_count")]
        public uint ItemCount { get; set; } = 0;

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("pinned_time")]
        public DateTime PinnedTime { get; set; }

        [Reference(typeof(RankingItems), useInnerJoin: false, columnName: "ranking_id")]
        public List<RankingItems> RankingItems { get; set; } = [];

        [Reference(typeof(ShareableLink), useInnerJoin: false, columnName: "ranking_id")]
        public ShareableLink ShareableId { get; set; }

        [Column("is_public")]
        public bool IsPublic { get; set; } = false;

        [Column("is_shared")]
        public bool IsShared { get; set; } = false;

        [Column("is_pinned")]
        public bool IsPinned { get; set; } = false;

        [Column("item_place")]
        public Dictionary<string, int> ItemPlacement {  get; set; }

        public Ranking() { }

        public Ranking(CreateRankingRequest rq, string owner)
        {
            RankingId = HashService.GenerateRobustHash($"{rq.Title}:{owner}");
            Owner = owner;
            Name = rq.Title;
            Description = rq.Description;
            ItemCount = (uint)rq.Items.Length;
            LastUpdated = DateTime.Now;
            IsPublic = rq.IsPublic;
            ShareableId = new ShareableLink();
        }

        public static Dictionary<string, int> CreateItemPlacement(List<RankingItems> items)
        {
            Dictionary<string, int> itemPlaceDictionary = new Dictionary<string, int>();
            foreach (var item in items)
            {
                itemPlaceDictionary.Add(item.ItemId, (int)item.Rank);
            }
            return itemPlaceDictionary;
        }
    }
}
