using simple_rank_backend.DTOs.User;

namespace simple_rank_backend.Models
{
    public class UserMetadata
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime LastUpdated { get; set; }

        public UserMetadata() 
        {
            UserId = string.Empty;
            DisplayName = string.Empty;
            AvatarUrl = string.Empty;
        }

        public UserMetadata(SetMetadataDTO metadataDTO)
        {
            UserId = metadataDTO.Id;
            DisplayName = metadataDTO.Name;
            AvatarUrl = metadataDTO.AvatarUrl;
        }

        public UserMetadata(TableModels.UserMetadata tableModel)
        {
            UserId = tableModel.UserId.ToString();
            DisplayName = tableModel.DisplayName;
            AvatarUrl = tableModel.AvatarUrl;
            LastUpdated = DateTime.Now;
        }
    }
}
