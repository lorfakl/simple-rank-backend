using Supabase.Postgrest.Attributes;

namespace simple_rank_backend.TableModels
{
    [Table("DataEncryptionKeys")]
    public class DataEncryptionKey
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = null!;
        [Column("key")]
        public string Key { get; set; } = null!;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("last_accessed")]
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    }
}
