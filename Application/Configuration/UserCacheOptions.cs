namespace simple_rank_backend.Application.Configuration
{
    public class UserCacheOptions
    {
        public int SlidingExpirationMinutes { get; set; } = 30;
        public int AbsoluteExpirationMinutes { get; set; } = 120;
        public long SizeLimit { get; set; } = 1000; // Maximum number of cached users
    }
}
