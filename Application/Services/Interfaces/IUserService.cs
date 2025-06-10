using simple_rank_backend.Application.Common;
using simple_rank_backend.DTOs.Ranking.Responses;
using simple_rank_backend.DTOs.Ranking;
using simple_rank_backend.DTOs.User;
using simple_rank_backend.DTOs;
using simple_rank_backend.Models;

namespace simple_rank_backend.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result<ResponseBase>> SetUserMetadata(SetMetadataDTO rq);
        Task<UserMetadata> GetUserMetadataAsync(string userId);
    }
}
