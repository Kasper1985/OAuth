using System.Collections.Generic;
using System.Threading.Tasks;

using Models;

namespace BusinessLogic.Interfaces
{
    public interface IOAuth2Logic
    {
        Task<Client> GetClientAsync(string clientID);
        Task<IEnumerable<Client>> FindClientsAsync(int? userID);
        Task<IEnumerable<Client>> FindClientsAsync(params string[] clientIDs);
        Task<Client> RegisterClientAsync(Client client);
        Task RemoveClientAsync(string clientID);

        Task<PKCE> FindPKCEAsync(string authorizationCode);
        Task<AuthCode> FindAuthCodeAsync(string authorizationCode);
        Task SaveAuthorizationCodeAsync(AuthCode authCode);
        Task SavePKCEAsync(string authorizationCode, PKCE pkce);
        Task RemoveAuthorizationCodeAsync(string authorizationCode);

        Task<LastRefreshToken> GetLastRefreshTokenAsync(int userID, string clientID);
        Task SaveLastRefreshTokenAsync(LastRefreshToken lastRefreshToken);
        Task RemoveLastRefreshTokenAsync(string refreshToken);
        Task RemoveLastRefreshTokenAsync(int userID, string clientID);

        Task<IEnumerable<Scope>> GetScopesAsync(string ccode, params string[] names);
        Task<IEnumerable<UserScope>> GetUserScopesAsync(int userID, string clientID, string ccode);
        Task<IEnumerable<UserScope>> GetAllUserScopesAsync(int userID, string ccode);
        Task<bool> AddUserScopesAsync(params UserScope[] scopes);
        Task RemoveUserScopesAsync(params UserScope[] userScopes);
    }
}
