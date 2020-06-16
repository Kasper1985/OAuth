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
        public async Task<Client> GetClientAsync(string clientID) => await this.oauth2Source.GetClientAsync(clientID);

        public async Task<IEnumerable<Client>> FindClientsAsync(int? userID) => await this.oauth2Source.FindClientsAsync(userID);

        public async Task<IEnumerable<Client>> FindClientsAsync(params string[] clientIDs) => await this.oauth2Source.FindClientsAsync(clientIDs);

        public async Task<Client> RegisterClientAsync(Client client) => await this.oauth2Source.RegisterClientAsync(client);

        public async Task RemoveClientAsync(string clientID) => await this.oauth2Source.RemoveClientAsync(clientID);

        public async Task<PKCE> FindPKCEAsync(string authorizationCode) => await this.oauth2Source.FindPKCEAsync(authorizationCode);

        public async Task<AuthCode> FindAuthCodeAsync(string authorizationCode) => await this.oauth2Source.FindAuthCodeAsync(authorizationCode);

        public async Task SaveAuthorizationCodeAsync(AuthCode authCode) => await this.oauth2Source.SaveAuthorizationCodeAsync(authCode);

        public async Task SavePKCEAsync(string authorizationCode, PKCE pkce) => await this.oauth2Source.SavePKCEAsync(authorizationCode, pkce);

        public async Task RemoveAuthorizationCodeAsync(string authorizationCode) => await this.oauth2Source.RemoveAuthorizationCodeAsync(authorizationCode);

        public async Task<LastRefreshToken> GetLastRefreshTokenAsync(int userID, string clientID) => await this.oauth2Source.GetLastRefreshTokenAsync(userID, clientID);

        public async Task SaveLastRefreshTokenAsync(LastRefreshToken lastRefreshToken) => await this.oauth2Source.SaveLastRefreshTokenAsync(lastRefreshToken);

        public async Task RemoveLastRefreshTokenAsync(string refreshToken) => await this.oauth2Source.RemoveLastRefreshTokenAsync(refreshToken);

        public async Task RemoveLastRefreshTokenAsync(int userID, string clientID) => await this.oauth2Source.RemoveLastRefreshTokenAsync(userID, clientID);

        public async Task<IEnumerable<Scope>> GetScopesAsync(string ccode, params string[] names) => await this.oauth2Source.GetScopesAsync(ccode, names);

        public async Task<IEnumerable<UserScope>> GetUserScopesAsync(int userID, string clientID, string ccode) => await this.oauth2Source.GetUserScopesAsync(userID, clientID, ccode);

        public async Task<IEnumerable<UserScope>> GetAllUserScopesAsync(int userID, string ccode) => await this.oauth2Source.GetAllUserScopesAsync(userID, ccode);

        public async Task<bool> AddUserScopesAsync(params UserScope[] userScopes) => await this.oauth2Source.AddUserScopesAsync(userScopes);

        public async Task RemoveUserScopesAsync(params UserScope[] userScopes) => await this.oauth2Source.RemoveUserScopesAsync(userScopes);
        #endregion // Interface functions
    }
}
