using simple_rank_backend.Models;
using Supabase.Gotrue;

namespace simple_rank_backend.DTOs.Ranking.Responses
{
    public class SharedRankResponse
    {
        public string RankingId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public UserMetadata CreatedBy { get; set; }
        public List<RankItem> Items { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsShared { get; set; }

        public SharedRankResponse() 
        {
            RankingId = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
            CreatedBy = new UserMetadata();
            Items = new List<RankItem>();
            LastUpdated = DateTime.MinValue;
            CreatedDate = DateTime.MinValue;
            IsPublic = false;
            IsShared = false;
        }

        public SharedRankResponse(TableModels.Ranking ranking, UserMetadata user)
        {
            Models.Ranking createdRank = new Models.Ranking(ranking);
            RankingId = createdRank.Id;
            Title = createdRank.Title;
            Description = createdRank.Description;
            CreatedBy = user;
            Items = createdRank.Items;
            LastUpdated = createdRank.LastUpdated;
            CreatedDate = createdRank.CreatedDate;
            IsPublic = createdRank.IsPublic;
            IsShared = createdRank.IsShared;
        }
    }
}
