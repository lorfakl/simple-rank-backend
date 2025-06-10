using simple_rank_backend.Application.Common;
using simple_rank_backend.DTOs.Ranking.Responses;
using simple_rank_backend.DTOs.Ranking;
using simple_rank_backend.DTOs;
using simple_rank_backend.DTOs.Statistic;

namespace simple_rank_backend.Application.Services.Interfaces
{
    public enum RankingReaction
    {
        like_reactions = 1,
        love_reactions = 2,
        laugh_reactions = 3,
        wow_reactions = 4,
        sad_reactions = 5,
        angry_reactions = 6
    }

    public enum Statistics
    {
        VIEW = 1,
        CLONE = 2,
        LIKE= 3,
        LOVE= 4,
        LAUGH= 5,
        SAD= 6,
        WOW= 7,
        ANGRY = 8
    }

    public interface IStatisticService
    {
        Task<Result<RankStatisticResponse>> GetRankStatistics(string rankingId);
        Task<Result<RankStatisticResponse>> GetSharedRankStatistics(string rankingId);
        Task<Result<ResponseBase>> RecordView(string rankingId);
        Task<Result<ResponseBase>> RecordReaction(ReactDto rq, string rankingId);
        Task<Result<RankingsCountResponse>> GetTotalRankingsCountAsync();
        Task<Result<UsersCountResponse>> GetTotalUsersCountAsync();


    }
}
