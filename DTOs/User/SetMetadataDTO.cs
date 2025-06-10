namespace simple_rank_backend.DTOs.User
{
    public class SetMetadataDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }

        public SetMetadataDTO()
        {
            Id = string.Empty;
            Name = string.Empty;
            AvatarUrl = string.Empty;
        }

        public SetMetadataDTO(string id, string name, string profileUrl)
        {
            Id = id;
            Name = name;
            AvatarUrl = profileUrl;
        }
    }
}
