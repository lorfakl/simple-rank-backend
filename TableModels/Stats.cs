using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace simple_rank_backend.TableModels
{
    [Table("Stats")]
    public class Stats: BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("ranking_id")]
        public string RankingId { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("value")]
        public int Value { get; set; }

        public Stats() { }

        public Stats(Application.Services.Interfaces.Statistics stat, string user, string rankingId)
        {
            Name = stat.ToString();
            Value = 1;
            RankingId = rankingId;
            UserId = user;
        }


    }
}
