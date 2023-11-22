using System.Collections.Generic;
using System.Threading.Tasks;

using Models;

namespace Data.Interfaces
{
    public interface IOAuth2Source
    {
        Task<Client> GetClientAsync(string clientId);
        Task<IEnumerable<Client>> FindClientsAsync(int? userId);
        Task<IEnumerable<Client>> FindClientsAsync(params string[] clientIDs);
        Task<Client> RegisterClientAsync(Client client);
        Task RemoveClientAsync(string clientId);

        Task<PKCE> FindPKCEAsync(string authorizationCode);
        Task<AuthCode> FindAuthCodeAsync(string authorizationCode);
        Task SaveAuthorizationCodeAsync(AuthCode authCode);
        Task SavePKCEAsync(string authorizationCode, PKCE pkce);
        Task RemoveAuthorizationCodeAsync(string authorizationCode);

        Task<LastRefreshToken> GetLastRefreshTokenAsync(int userId, string clientId);
        Task SaveLastRefreshTokenAsync(LastRefreshToken lastRefreshToken);
        Task RemoveLastRefreshTokenAsync(string refreshToken);
        Task RemoveLastRefreshTokenAsync(int userId, string clientId);

        Task<IEnumerable<Scope>> GetScopesAsync(string cCode, params string[] names);
        Task<IEnumerable<UserScope>> GetUserScopesAsync(int userId, string clientId, string cCode);
        Task<IEnumerable<UserScope>> GetAllUserScopesAsync(int userId, string cCode);
        Task<bool> AddUserScopesAsync(params UserScope[] userScopes);
        Task RemoveUserScopesAsync(params UserScope[] userScopes);
    }
}
