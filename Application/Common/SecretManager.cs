using System.Collections;

namespace simple_rank_backend.Application.Common
{
    public enum SecretKeys
    {
        SupabaseUrl = 1,
        SupabaseServiceKey = 2,
        SupabaseJwt = 3,
        ApiUrl = 4,
        AzureClientId = 5,
        AzureTenantId = 6,
        AzureClientSecret = 7,
        AzureKeyVaultUri = 8,
        SentryDsn = 9,
        AzureMasterKeyUrl = 10 
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
#if DEBUG
        private const EnvironmentVariableTarget DefaultTarget = EnvironmentVariableTarget.Process;
#else
        private const EnvironmentVariableTarget DefaultTarget = EnvironmentVariableTarget.Machine;
#endif
        public static Dictionary<SecretKeys, string> LoadEnvironmentVariables(EnvironmentVariableTarget target = DefaultTarget)
        {
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();

            Console.WriteLine($"Environment Variables for the current {DefaultTarget}:");
            ;
            var secrets = new Dictionary<SecretKeys, string>();
            var nullOrEmptyFound = new List<string>();
            foreach (var key in Enum.GetNames(typeof(SecretKeys)))
            {
                Console.WriteLine($"{key} and value: {Environment.GetEnvironmentVariable(key, target)}");
                var value = Environment.GetEnvironmentVariable(key, target);
                
                if (string.IsNullOrEmpty(value))
                {
                    if (environmentVariables.Contains(key))
                    {
                        secrets.Add(Enum.Parse<SecretKeys>(key), environmentVariables[key].ToString());
                    }
                    else
                    {
                        nullOrEmptyFound.Add(key);
                    }
                }
                else
                {
                    secrets.Add(Enum.Parse<SecretKeys>(key), value);
                }
            }
            
            if (nullOrEmptyFound.Count > 0)
            {
                throw new ArgumentNullException($"The following environment variables are not set: {string.Join(", ", nullOrEmptyFound)}");
            }
            return secrets;
        }
    }
}
