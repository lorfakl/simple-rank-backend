using simple_rank_backend.Application.Common;

namespace simple_rank_backend.Application.Services.Interfaces
{
    public interface IEncryptionService
    {
        Task<Result<T>> Encrypt<T>(T obj, string dek) where T : class;
        Task<Result<T>> Decrypt<T>(string encryptedData, string dek) where T : class;
    }
}
