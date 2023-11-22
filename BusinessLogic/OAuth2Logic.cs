using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Models;
using BusinessLogic.Interfaces;
using Data.Interfaces;

namespace BusinessLogic
{
    public class OAuth2Logic : IOAuth2Logic
    {
        private readonly IOAuth2Source oauth2Source;

        public OAuth2Logic(IOAuth2Source oauth2Source) => this.oauth2Source = oauth2Source ?? throw new ArgumentNullException(nameof(oauth2Source));

        #region Interface functions
        public async Task<Client> GetClientAsync(string clientId) => await this.oauth2Source.GetClientAsync(clientId);

        public async Task<IEnumerable<Client>> FindClientsAsync(int? userId) => await this.oauth2Source.FindClientsAsync(userId);

        public async Task<IEnumerable<Client>> FindClientsAsync(params string[] clientIDs) => await this.oauth2Source.FindClientsAsync(clientIDs);

        public async Task<Client> RegisterClientAsync(Client client) => await this.oauth2Source.RegisterClientAsync(client);

        public async Task RemoveClientAsync(string clientId) => await this.oauth2Source.RemoveClientAsync(clientId);

        public async Task<PKCE> FindPKCEAsync(string authorizationCode) => await this.oauth2Source.FindPKCEAsync(authorizationCode);

        public async Task<AuthCode> FindAuthCodeAsync(string authorizationCode) => await this.oauth2Source.FindAuthCodeAsync(authorizationCode);

        public async Task SaveAuthorizationCodeAsync(AuthCode authCode) => await oauth2Source.SaveAuthorizationCodeAsync(authCode);

        public async Task SavePKCEAsync(string authorizationCode, PKCE pkce) => await oauth2Source.SavePKCEAsync(authorizationCode, pkce);

        public async Task RemoveAuthorizationCodeAsync(string authorizationCode) => await oauth2Source.RemoveAuthorizationCodeAsync(authorizationCode);

        public async Task<LastRefreshToken> GetLastRefreshTokenAsync(int userId, string clientId) => await oauth2Source.GetLastRefreshTokenAsync(userId, clientId);

        public async Task SaveLastRefreshTokenAsync(LastRefreshToken lastRefreshToken) => await oauth2Source.SaveLastRefreshTokenAsync(lastRefreshToken);

        public async Task RemoveLastRefreshTokenAsync(string refreshToken) => await oauth2Source.RemoveLastRefreshTokenAsync(refreshToken);

        public async Task RemoveLastRefreshTokenAsync(int userId, string clientId) => await oauth2Source.RemoveLastRefreshTokenAsync(userId, clientId);

        public async Task<IEnumerable<Scope>> GetScopesAsync(string cCode, params string[] names) => await oauth2Source.GetScopesAsync(cCode, names);

        public async Task<IEnumerable<UserScope>> GetUserScopesAsync(int userId, string clientId, string cCode) => await oauth2Source.GetUserScopesAsync(userId, clientId, cCode);

        public async Task<IEnumerable<UserScope>> GetAllUserScopesAsync(int userId, string cCode) => await oauth2Source.GetAllUserScopesAsync(userId, cCode);

        public async Task<bool> AddUserScopesAsync(params UserScope[] userScopes) => await oauth2Source.AddUserScopesAsync(userScopes);

        public async Task RemoveUserScopesAsync(params UserScope[] userScopes) => await oauth2Source.RemoveUserScopesAsync(userScopes);
        #endregion // Interface functions
    }
}
