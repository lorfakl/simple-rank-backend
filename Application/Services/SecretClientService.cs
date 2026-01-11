using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
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
        private readonly CryptographyClient _cryptoClient;
        
        public SecretClientService(CryptographyClient cryptoClient)
        {
            _cryptoClient = cryptoClient;
        }

        public async Task<Result> CreateOrUpdateSecretAsync()
        {
            throw new NotImplementedException();
        }

        /*public async Task<Result<string>> CreateUserSecret(UserSecret userSecret, CancellationToken t)
        {
            try
            {
                string secretName = userSecret.SecretName; 

            }
            catch (Exception ex)
            {
                return Result<string>.Failure(Error.Exception($"Failed to create user secret: {secretName}", ex));
            }   
        }*/

        public async Task<Result<string>> GetSecretAsync(string secretName, CancellationToken t)
        {
            throw new NotImplementedException();
        }

        public Task<Result> UpdateMasterSecret(SecretNames secretName)
        {
            throw new NotImplementedException();
        }
    }
}
