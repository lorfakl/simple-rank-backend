using simple_rank_backend.DTOs.User;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace simple_rank_backend.TableModels
{

    [Table("UserMetadata")]
    public class UserMetadata : BaseModel
    {
        [Column(columnName: "user_id")]
        public string UserId { get; set; }

        [Column(columnName: "display_name")]
        public string DisplayName { get; set; }

        [Column(columnName: "avatar_url")]
        public string AvatarUrl { get; set; }

        public UserMetadata() 
        {
            UserId = string.Empty;
            DisplayName = string.Empty;
            AvatarUrl = string.Empty;
        }

        public UserMetadata(string userId, string displayName, string avatarUrl)
        {
            UserId = userId;
            DisplayName = displayName;
            AvatarUrl = avatarUrl;
        }

        public UserMetadata(SetMetadataDTO dto)
        {
            UserId = dto.Id;
            DisplayName = dto.Name;
            AvatarUrl = dto.AvatarUrl;
        }

    }
}
