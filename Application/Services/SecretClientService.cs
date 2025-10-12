using Azure.Security.KeyVault.Secrets;
using simple_rank_backend.Application.Common;
using simple_rank_backend.Application.Services.Interfaces;

namespace simple_rank_backend.Application.Services
{
    public enum SecretNames
    {
        MasterKey
    }

    public class SecretClientService : ISecretClientService
    {
        private readonly SecretClient _secretClient;

        public SecretClientService(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        public async Task<Result> CreateOrUpdateSecretAsync()
        {
            try
            {
                // Example: Create or update a secret
                string secretName = SecretNames.MasterKey.ToString();
                string secretValue = $"{HashService.GenerateLongHash(Guid.NewGuid().ToString())}"; ; // Generate a new master key
                await _secretClient.SetSecretAsync(secretName, secretValue);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Exception("Failed to create or update secret", ex));
            }
        }

        public async Task<Result<string>> CreateUserSecret(UserSecret userSecret, CancellationToken t)
        {
            try
            {
                string secretName = userSecret.SecretName; 

            }
            catch (Exception ex)
            {
                return Result<string>.Failure(Error.Exception($"Failed to create user secret: {secretName}", ex));
            }   
        }

        public async Task<Result<string>> GetSecretAsync(string secretName, CancellationToken t)
        {
            try
            {
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName, cancellationToken: t);
                return Result<string>.Success(secret.Value);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(Error.Exception($"Failed to retrieve secret: {secretName}", ex));
            }
        }

        public Task<Result> UpdateMasterSecret(SecretNames secretName)
        {
            throw new NotImplementedException();
        }
    }
}
