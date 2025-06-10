
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace simple_rank_backend.TableModels
{


    [Table("Statistics")]
    public class Statistics : BaseModel
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("views")]
        public int Views { get; set; }

        [Column("clones")]
        public int Clones { get; set; }

        [Column("like_reactions")]
        public int LikeReactions { get; set; }

        [Column("love_reactions")]
        public int LoveReactions { get; set; }

        [Column("laugh_reactions")]
        public int LaughReactions { get; set; }

        [Column("wow_reactions")]
        public int WowReactions { get; set; }

        [Column("sad_reactions")]
        public int SadReactions { get; set; }

        [Column("angry_reactions")]
        public int AngryReactions { get; set; }

        [Column("last_update")]
        public DateTimeOffset LastUpdate { get; set; }

        // Constructor
        public Statistics()
        {
            Id = string.Empty;
            LastUpdate = DateTimeOffset.UtcNow;
        }

        public Statistics(string id)
        {
            Id = id;
            LastUpdate = DateTimeOffset.UtcNow;
        }
        // Optional: Override ToString for debugging
        public override string ToString()
        {
            return $"Statistics [Id: {Id}, Views: {Views}, Clones: {Clones}, LastUpdate: {LastUpdate}]";
        }
    }
}
