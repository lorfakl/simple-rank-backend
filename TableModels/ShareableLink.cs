using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace simple_rank_backend.TableModels
{
    [Table("SharedLinks")]
    public class ShareableLink : BaseModel
    {
        [PrimaryKey(columnName:"shared_id")]
        public string SharedLinkId { get; set; }

        [Column(columnName:"ranking_id")]
        [Reference(typeof(Ranking))]
        public string RankingId { get; set; }

        public ShareableLink() 
        { 
            SharedLinkId = RankingId = string.Empty;
        }
    }
}
