using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using simple_rank_backend.Application.Common;
using simple_rank_backend.Application.Common.Extensions;
using simple_rank_backend.Application.Services.Interfaces;
using simple_rank_backend.DTOs.Ranking;
using simple_rank_backend.DTOs.Ranking.Responses;
using simple_rank_backend.Models;
using System.Security.Claims;


namespace simple_rank_backend.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RankController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly IRankService _rankService;
        public RankController(IRankService rankingService, IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _rankService = rankingService;
        }


        [HttpOptions]
        public IActionResult Options()
        {
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRanking([FromBody] CreateRankingRequest rq)
        {
            // Logic to create a ranking
            // This would typically involve saving the ranking to a database
            // and possibly returning the created ranking object or its ID.

            if (ModelState.IsValid)
            {
                string ownerId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                if (string.IsNullOrEmpty(ownerId))
                {
                    return Unauthorized("User is not authenticated");
                }
                else
                {
                    var result = await _rankService.CreateRankingAsync(rq, ownerId);
                    return this.HandleResult(result);
                }
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRankItem([FromBody] CreateRankingItemRequest rq)
        {
            if (ModelState.IsValid)
            {
                string ownerId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                if (string.IsNullOrEmpty(ownerId))
                {
                    return Unauthorized("User is not authenticated");
                }
                else
                {
                    if(string.IsNullOrEmpty(rq.Item.Id))
                    {
                        return BadRequest();
                    }

                    if (string.IsNullOrEmpty(rq.Item.Name))
                    {
                        return BadRequest();
                    }

                    if (string.IsNullOrEmpty(rq.Item.Description))
                    {
                        return BadRequest();
                    }

                    var result = await _rankService.CreateRankItemAsync(rq, ownerId);
                    return this.HandleResult(result);
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserRankings(string userId)
        {
            string ownerId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(ownerId))
            {
                return Unauthorized("User is not authenticated");
            }

            if (ownerId != userId)
            {
                return Forbid();
            }
            else
            {
                Result<List<Ranking>> result = await _rankService.GetUserRankingsAsync(ownerId);
                return this.HandleResult(result);
            }
        }


        [HttpGet("{rankingId}")]
        [Authorize]
        public async Task<IActionResult> GetRanking(string rankingId)
        {
            Result<Ranking> result = await _rankService.GetRankingAsync(new GetRankingByIdRequest { Id = rankingId });
            return this.HandleResult(result);
        }


        [HttpPost("{rankingId}")]
        [Authorize]
        public async Task<IActionResult> GetShareableLink(string rankingId)
        {
            if (ModelState.IsValid)
            {
                Result<ShareableLinkResponse> result = await _rankService.GetShareableLinkAsync(rankingId);
                return this.HandleResult<ShareableLinkResponse>(result);
            }
            else
            {
                return this.HandleError(Error.InvalidModelState);
            }
        }

        [HttpGet("{rankingId}")]
        public async Task<IActionResult> GetSharedRanking(string rankingId)
        {
            if (ModelState.IsValid)
            {
                Result<SharedRankResponse> result = await _rankService.GetSharedRankAsync(rankingId);
                return this.HandleResult<SharedRankResponse>(result);
            }
            else
            {
                return this.HandleError(Error.InvalidModelState);
            }
        }

        //[HttpGet("{rankingId}")]
        //public async Task<IActionResult> GetRankingMetadata(string rankingId)
        //{
            
        //}

        [HttpPut("{rankingId}")]
        [Authorize]
        public async Task<IActionResult> EditRanking(string rankingId, [FromBody] UpdateRankingRequest rq)
        {
            if (ModelState.IsValid)
            {
                if (rq.Items.Count == 0)
                {
                    Result result = await _rankService.UpdateBasicRankingInfoAsync(rq);
                    return this.HandleResult(result);
                }
                else
                {
                    Result result = await _rankService.UpdateRankingAsync(rq);
                    return this.HandleResult(result);
                }
            }
            else
            {
                return this.HandleError(Error.InvalidModelState);
            }
        }

        [HttpPost("{rankingId}")]
        [Authorize]
        public async Task<IActionResult> UpdateRankItemsPlacement(string rankingId, [FromBody] UpdateRankItemPlacementRequest rq)
        {
            if (ModelState.IsValid)
            {
                if(rq.ItemIdToRank.Values.Count == 0 || string.IsNullOrEmpty(rq.RankingId))
                {
                    return this.HandleError(Error.InvalidModelState);
                }

                if(rankingId == rq.RankingId)
                {
                    Result result = await _rankService.UpdateRankItemPlacement(rq);
                    return this.HandleResult(result);
                }
                else
                {
                    return this.HandleError(Error.Custom("InvalidRankingId", "The ranking ID in the request body does not match the ranking ID in the URL."));
                }
            }
            else
            {
                return this.HandleError(Error.InvalidModelState);
            }
        }

        [HttpPost("{rankingId}")]
        [Authorize]
        public async Task<IActionResult> PinRanking([FromBody] PinRankingRequest rq, string rankingId)
        {
            if(ModelState.IsValid)
            {
                if(String.IsNullOrEmpty(rq.RankingId))
                {
                    return BadRequest();
                }

                if(rankingId == rq.RankingId)
                {
                    var result = await _rankService.PinRankingAsync(rankingId, rq.PinOperation);
                    return this.HandleResult(result);
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("{rankingId}")]
        [Authorize]
        public async Task<IActionResult> DeleteRanking(string rankingId)
        {
            if(Guid.TryParse(rankingId, out _))
            {
                var result = await _rankService.DeleteRankingAsync(rankingId);
                return this.HandleResult(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("{rankItemId}")]
        [Authorize]
        public async Task<IActionResult> DeleteRankItem([FromBody] DeleteRankItemRequest rq)
        {
            if(ModelState.IsValid)
            {
                if(string.IsNullOrEmpty(rq.RankingId) || string.IsNullOrEmpty(rq.RankItemId))
                {
                    return BadRequest();
                }
                //check that the caller owns the ranking that owns the rank item attempting to be deleted
                Result result = await _rankService.DeleteRankItem(rq);
                return this.HandleResult(result);
            }
            else
            {
                return BadRequest();
            }
           
        }
    }
}
