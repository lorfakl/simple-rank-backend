using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace simple_rank_backend.TableModels
{
    [Table("HeartBeat")]
    public class Heartbeat : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; } 

        [Column("last_heartbeat")]
        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    }
}
