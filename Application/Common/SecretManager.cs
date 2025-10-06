namespace simple_rank_backend.Application.Common
{
    public enum SecretKeys
    {
        supabaseUrl = 1,
        supabaseServiceKey = 2,
        supabaseJwt = 3,
        apiUrl = 4,
        AZURE_CLIENT_ID = 5,
        AZURE_TENANT_ID = 6,
        AZURE_CLIENT_SECRET = 7,
        azureKeyVaultUri = 8

    }

    public static class SecretManager
    {
        

        public static string GetSecret(SecretKeys key, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            var value = Environment.GetEnvironmentVariable(key.ToString(), target);
            
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException($"{key} is not set in environment variables.");
            }
            return value;
        }

        public static Dictionary<SecretKeys, string> LoadEnvironmentVariables(EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            var secrets = new Dictionary<SecretKeys, string>();
            foreach (var key in Enum.GetValues(typeof(SecretKeys)))
            {
                var value = Environment.GetEnvironmentVariable(key.ToString(), target);
                
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException($"{key} is not set in environment variables.");
                }
                secrets.Add((SecretKeys)key, value);
            }

            return secrets;
        }
    }
}
