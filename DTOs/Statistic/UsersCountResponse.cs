namespace simple_rank_backend.DTOs.Statistic
{
    public class UsersCountResponse
    {
        public int TotalCount { get; set; }
        public string Message { get; set; } = string.Empty;

        public UsersCountResponse() { }
    }
}
