using Microsoft.Extensions.Caching.Memory;

namespace simple_rank_backend.Application.Middleware
{
    public class AuthTokenValidator
    {
        private readonly RequestDelegate _next;

        public AuthTokenValidator(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {


            await _next(context);

        }
    }
}
