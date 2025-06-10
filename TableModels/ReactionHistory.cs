using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace simple_rank_backend.TableModels
{
    [Table("ReactionHistory")]
    public class ReactionHistory:BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("ranking_id")]
        public string RankingId { get; set; } = string.Empty;

        [Column("shared_id")]
        public string SharedId { get; set; } = string.Empty;

        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("is_real_user_id")]
        public bool IsRealUser { get; set; }

        public ReactionHistory() { }

        public ReactionHistory(string rankingId, string sharedId, string ipAddress, string userId)
        {
            RankingId = rankingId;
            SharedId = sharedId;
            IpAddress = ipAddress;
            UserId = userId;
            IsRealUser = true;
        }

        public ReactionHistory(string rankReference, string ipAddress, string userId, bool isReal, bool isUsingShared)
        {
            if(isUsingShared)
            {
                RankingId = string.Empty;
                SharedId = rankReference;
            }
            else
            {
                RankingId = rankReference;
                SharedId = string.Empty;
            }

            IpAddress = ipAddress;
            UserId = userId;
            IsRealUser = isReal;
        }

        public static ReactionHistory RecordWithRankingId(string rankingId, string ipAddress, string userId, bool isReal)
        {
            return new ReactionHistory(rankReference: rankingId, ipAddress: ipAddress, userId: userId, isReal: isReal, isUsingShared: false);
        }

        public static ReactionHistory RecordWithsharedId(string sharedId, string ipAddress, string userId, bool isReal)
        {
            return new ReactionHistory(rankReference: sharedId, ipAddress: ipAddress, userId: userId, isReal: isReal, isUsingShared: true);
        }
    }
}
