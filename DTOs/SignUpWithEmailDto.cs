namespace simple_rank_backend.DTOs
{
    public class SignUpWithEmailDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public SignUpWithEmailDto()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        public SignUpWithEmailDto(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
