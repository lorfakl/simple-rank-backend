using Azure.Security.KeyVault.Secrets;
using simple_rank_backend.Application.Common;

namespace simple_rank_backend.Application.Services.Interfaces
{
    public interface ISecretClientService
    {
        public Task<Result> CreateOrUpdateSecretAsync();
        public Task<Result<string>> GetSecretAsync(string secretName, CancellationToken cancellationToken);
    }
}
