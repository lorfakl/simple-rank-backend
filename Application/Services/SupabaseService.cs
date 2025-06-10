using Supabase;

namespace simple_rank_backend.Application.Services
{
    public class SupabaseService
    {
        private Client _client;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public Client Client => _client;

        public SupabaseService(string supabaseUrl, string supabaseKey, SupabaseOptions options)
        {
            _supabaseUrl = supabaseUrl;
            _supabaseKey = supabaseKey;
            _client = new Client(supabaseUrl, supabaseKey, options);
        }

        public async Task<Client> GetClientAsync()
        {
            if (_client == null)
            {
                _client = new Client(_supabaseUrl, _supabaseKey);
            }
            return await Task.FromResult(_client);
        }


        //public async Task<bool> CheckIfEmailExists(string email)
        //{
            
        //}
    }
}
