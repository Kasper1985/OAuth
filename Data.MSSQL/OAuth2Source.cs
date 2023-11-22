using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Models;
using Models.Enumerations;
using Models.Extensions;

using Data.Interfaces;
using Data.Common.Exceptions;

namespace Data.MSSQL
{
    public class OAuth2Source : SourceBase, IOAuth2Source
    {
        public OAuth2Source(string connectionString) : base(connectionString) { }

        #region Interface functions
        public async Task<Client> GetClientAsync(string clientId)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "SELECT ClientID, Secret, URI, Name, Developer, Type, UserID " +
                                     "FROM tblOAuth2Clients " +
                                     "WHERE ClientID = @ClientID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("ClientID", clientId);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var clients = new List<Client>();
                        int ordinal;
                        while (reader.Read())
                            clients.Add(new Client
                            {
                                Id = reader.GetString(reader.GetOrdinal("ClientID")),
                                Secret = !reader.IsDBNull(ordinal = reader.GetOrdinal("Secret")) ? reader.GetString(ordinal) : null,
                                Uri = reader.GetString(reader.GetOrdinal("URI")),
                                Name = !reader.IsDBNull(ordinal = reader.GetOrdinal("Name")) ? reader.GetString(ordinal) : null,
                                Developer = !reader.IsDBNull(ordinal = reader.GetOrdinal("Developer")) ? reader.GetString(ordinal) : null,
                                Type = reader.GetInt32(reader.GetOrdinal("Type")).Convert<ClientType>(),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserID"))
                            });

                        if (clients.Count > 1)
                            throw new AmbiguousClientException(clientId);
                        if (clients.Count < 1)
                            throw new ClientNotFoundException(clientId);
                        return clients[0];
                    }
                }
            }
        }

        public async Task<IEnumerable<Client>> FindClientsAsync(int? userId)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "SELECT ClientID, URI, Name, Developer, Type, UserID " +
                                     "FROM tblOAuth2Clients " +
                                     "WHERE UserID = ISNULL(@UserID, UserID)";
                using (var cmd = new SqlCommand(query, con))
                {
                    if (userId.HasValue)
                        cmd.Parameters.AddWithValue("UserID", userId.Value);
                    else
                        cmd.Parameters.AddWithValue("UserID", null);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var clients = new List<Client>();
                        int ordinal;
                        while (reader.Read())
                            clients.Add(new Client
                            {
                                Id = reader.GetString(reader.GetOrdinal("ClientID")),
                                Uri = reader.GetString(reader.GetOrdinal("URI")),
                                Name = !reader.IsDBNull(ordinal = reader.GetOrdinal("Name")) ? reader.GetString(ordinal) : "",
                                Developer = !reader.IsDBNull(ordinal = reader.GetOrdinal("Developer")) ? reader.GetString(ordinal) : "",
                                Type = reader.GetInt32(reader.GetOrdinal("Type")).Convert<ClientType>(),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserID"))
                            });

                        return clients;
                    }
                }
            }
        }

        public async Task<IEnumerable<Client>> FindClientsAsync(params string[] clientIDs)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                var query = "SELECT DISTINCT ClientID, URI, Name, Developer, Type, UserID " +
                                 "FROM tblOAuth2Clients AS clients " +
                                 (clientIDs?.Length > 0 ? "JOIN (SELECT i.value('.', 'nvarchar(max)') AS ID FROM @IDs.nodes('/id') AS x(i)) AS ids ON ids.ID = clients.ClientID" : "");
                using (var cmd = new SqlCommand(query, con))
                {
                    if (clientIDs != null && clientIDs.Length > 0)
                        cmd.Parameters.Add(new SqlParameter("IDs", SqlDbType.Xml) { Value = string.Join("", clientIDs.Select(id => $"<id>{id}</id>")) });

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var clients = new List<Client>();
                        int ordinal;
                        while (reader.Read())
                            clients.Add(new Client
                            {
                                Id = reader.GetString(reader.GetOrdinal("ClientID")),
                                Uri = reader.GetString(reader.GetOrdinal("URI")),
                                Name = !reader.IsDBNull(ordinal = reader.GetOrdinal("Name")) ? reader.GetString(ordinal) : "",
                                Developer = !reader.IsDBNull(ordinal = reader.GetOrdinal("Developer")) ? reader.GetString(ordinal) : "",
                                Type = reader.GetInt32(reader.GetOrdinal("Type")).Convert<ClientType>(),
                                UserId = reader.GetInt32(reader.GetOrdinal("UserID"))
                            });

                        return clients;
                    }
                }
            }
        }

        public async Task<Client> RegisterClientAsync(Client client)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "INSERT INTO tblOAuth2Clients (ClientID, Secret, URI, Name, Developer, Type, UserID) " +
                                     "VALUES (@ClientID, @Secret, @URI, @Name, @Developer, @Type, @UserID)";
                using (var cmd = new SqlCommand(query, con))
                {
                    // generate new client id
                    var clientId = Guid.NewGuid().ToString("n").Substring(0, 12);

                    cmd.Parameters.AddWithValue("ClientID", clientId);
                    cmd.Parameters.AddWithValue("Secret", client.Secret ?? "");
                    cmd.Parameters.AddWithValue("URI", client.Uri);
                    cmd.Parameters.AddWithValue("Name", client.Name ?? "");
                    cmd.Parameters.AddWithValue("Developer", client.Developer ?? "");
                    cmd.Parameters.AddWithValue("Type", (int)client.Type);
                    cmd.Parameters.AddWithValue("UserID", client.UserId);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    var results = await cmd.ExecuteNonQueryAsync();
                    con.Close();

                    if (results != 1) 
                        return null;
                    
                    client.Id = clientId;
                    client.Secret = null;
                    return client;

                }
            }
        }

        public async Task RemoveClientAsync(string clientId)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "DELETE FROM tblOAuth2AuthCodes WHERE ClientID = @ClientID; " +      // Delete all not deleted Authorization Codes and/or PCKE codes
                                     "DELETE FROM tblOAuth2RefreshToken WHERE ClientID = @ClientID; " +   // Delete all remaining refresh tokens
                                     "DELETE FROM tblOAuth2UserScopes WHERE ClientID = @ClientID; " +     // Delete all connected user scopes for this client
                                     "DELETE FROM tblOAuth2Clients WHERE ClientID = @ClientID"; // Delete the client itself
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("ClientID", clientId);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }

        public async Task<PKCE> FindPKCEAsync(string authorizationCode)
        {
            using (var con = new SqlConnection(this.ConnectionString))
            {
                const string query = "SELECT ClientID, CodeChallenge, CodeChallengeMethod " +
                                     "FROM tblOAuth2AuthCodes " +
                                     "WHERE AuthorizationCode = @AuthorizationCode";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("AuthorizationCode", authorizationCode);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var PKCEs = new List<PKCE>();
                        int ordinal;
                        while (reader.Read())
                            PKCEs.Add(new PKCE
                            {
                                ClientId = reader.GetString(reader.GetOrdinal("ClientID")),
                                CodeChallenge = !reader.IsDBNull(ordinal = reader.GetOrdinal("CodeChallenge")) ? reader.GetString(ordinal) : null,
                                CodeChallengeMethod = !reader.IsDBNull(ordinal = reader.GetOrdinal("CodeChallengeMethod")) ? reader.GetInt32(ordinal).Convert<EncryptionMethod>() : default
                            });

                        return PKCEs.Count == 1 ? PKCEs[0] : null;
                    }
                }
            }
        }

        public async Task<AuthCode> FindAuthCodeAsync(string authorizationCode)
        {
            using (var con = new SqlConnection(this.ConnectionString))
            {
                const string query = "SELECT ClientID, AuthorizationCode, AuthorizationTicket " +
                                     "FROM tblOAuth2AuthCodes " +
                                     "WHERE AuthorizationCode = @AuthorizationCode";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("AuthorizationCode", authorizationCode);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var authCodes = new List<AuthCode>();
                        while (reader.Read())
                            authCodes.Add(new AuthCode
                            {
                                ClientId = reader.GetString(reader.GetOrdinal("ClientID")),
                                AuthorizationCode = reader.GetString(reader.GetOrdinal("AuthorizationCode")),
                                AuthorizationTicket = reader.GetString(reader.GetOrdinal("AuthorizationTicket"))
                            });

                        return authCodes.Count == 1 ? authCodes[0] : null;
                    }
                }
            }
        }

        public async Task SaveAuthorizationCodeAsync(AuthCode authCode)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "INSERT INTO tblOAuth2AuthCodes (ClientID, AuthorizationCode, AuthorizationTicket) " +
                                     "VALUES (@ClientID, @AuthorizationCode, @AuthorizationTicket)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("ClientID", authCode.ClientId);
                    cmd.Parameters.AddWithValue("AuthorizationCode", authCode.AuthorizationCode);
                    cmd.Parameters.AddWithValue("AuthorizationTicket", authCode.AuthorizationTicket);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }

        public async Task SavePKCEAsync(string authorizationCode, PKCE pkce)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "UPDATE tblOAuth2AuthCodes " +
                                     "SET CodeChallenge = @CodeChallenge, CodeChallengeMethod = @CodeChellangeMethod " +
                                     "WHERE AuthorizationCode = @AuthorizationCode";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("AuthorizationCode", authorizationCode);
                    cmd.Parameters.AddWithValue("CodeChallenge", pkce.CodeChallenge);
                    cmd.Parameters.AddWithValue("CodeChellangeMethod", pkce.CodeChallengeMethod);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }

        public async Task RemoveAuthorizationCodeAsync(string authorizationCode)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "DELETE FROM tblOAuth2AuthCodes WHERE AuthorizationCode = @AuthorizationCode";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("AuthorizationCode", authorizationCode);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }

        public async Task<LastRefreshToken> GetLastRefreshTokenAsync(int userId, string clientId)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "SELECT UserID, ClientID, RefreshToken, ExpireTime " +
                                     "FROM tblOAuth2RefreshToken " +
                                     "WHERE UserID = @UserID AND ClientID = @ClientID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("UserID", userId);
                    cmd.Parameters.AddWithValue("ClientID", clientId);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var refreshTokens = new List<LastRefreshToken>();
                        int ordinal;
                        while (reader.Read())
                            refreshTokens.Add(new LastRefreshToken
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                                ClientId = reader.GetString(reader.GetOrdinal("ClientID")),
                                RefreshToken = reader.GetString(reader.GetOrdinal("RefreshToken")),
                                ExpireTime = !reader.IsDBNull(ordinal = reader.GetOrdinal("ExpireTime")) ? reader.GetDateTime(ordinal) : DateTime.Now
                            });

                        return refreshTokens.Count == 1 ? refreshTokens[0] : null;
                    }
                }
            }
        }

        public async Task SaveLastRefreshTokenAsync(LastRefreshToken lastRefreshToken)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "INSERT INTO tblOAuth2RefreshToken (UserID, ClientID, RefreshToken, ExpireTime) " +
                                     "VALUES (@UserID, @ClientID, @RefreshToken, @ExpireTime)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("UserID", lastRefreshToken.UserId);
                    cmd.Parameters.AddWithValue("ClientID", lastRefreshToken.ClientId);
                    cmd.Parameters.AddWithValue("RefreshToken", lastRefreshToken.RefreshToken);
                    cmd.Parameters.AddWithValue("ExpireTime", lastRefreshToken.ExpireTime);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }

        public async Task RemoveLastRefreshTokenAsync(string refreshToken)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "DELETE FROM tblOAuth2RefreshToken " +
                                     "WHERE RefreshToken = @RefreshToken";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("RefreshToken", refreshToken);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }
        
        public async Task RemoveLastRefreshTokenAsync(int userId, string clientId)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "DELETE FROM tblOAuth2RefreshToken " +
                                     "WHERE UserID = @UserID AND ClientID = @ClientID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("UserID", userId);
                    cmd.Parameters.AddWithValue("ClientID", clientId);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }

        public async Task<IEnumerable<Scope>> GetScopesAsync(string cCode, params string[] names)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "IF @Scopes IS NULL " +
                                     "SELECT s.ScopeID, s.Name, ISNULL(s_d.Description, s.Description) AS SCOPE_Description " +
                                     "FROM tblOAuth2Scopes AS s " +
                                     "LEFT JOIN tblOAuth2Scopes_Dict AS s_d ON s_d.ScopeID = s.ScopeID AND s_d.CCode = @CCode " +
                                     "ELSE " +
                                     "SELECT s.ScopeID, s.Name, ISNULL(s_d.Description, s.Description) AS SCOPE_Description " +
                                     "FROM tblOAuth2Scopes AS s " +
                                     "LEFT JOIN tblOAuth2Scopes_Dict AS s_d ON s_d.ScopeID = s.ScopeID AND s_d.CCode = @CCode " +
                                     "JOIN(SELECT s.value('(.)[1]', 'nvarchar(50)') AS Name FROM @Scopes.nodes('/scope') AS x(s)) AS scopes ON scopes.Name = s.Name";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("CCode", cCode);
                    if (names != null)
                        cmd.Parameters.Add(new SqlParameter("Scopes", SqlDbType.Xml) { Value = string.Join("", names.Select(s => $"<scope>{s}</scope>")) });
                    else
                        cmd.Parameters.AddWithValue("Scopes", null);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var scopes = new List<Scope>();
                        int ordinal;
                        while (reader.Read())
                            scopes.Add(new Scope
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ScopeID")),
                                Name = !reader.IsDBNull(ordinal = reader.GetOrdinal("Name")) ? reader.GetString(ordinal) : "",
                                Description = !reader.IsDBNull(ordinal = reader.GetOrdinal("SCOPE_Description")) ? reader.GetString(ordinal) : ""
                            });

                        return scopes;
                    }
                }
            }
        }

        public async Task<IEnumerable<UserScope>> GetUserScopesAsync(int userId, string clientId, string cCode)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "SELECT us.UserID, us.ClientID, us.[Grant], " +
                                     "s.ScopeID AS SCOPE_ID, s.Name AS SCOPE_Name, ISNULL(s_d.Description, s.Description) AS SCOPE_Description " +
                                     "FROM tblOAuth2UserScopes AS us " +
                                     "JOIN tblOAuth2Scopes AS s ON s.ScopeID = us.ScopeID " +
                                     "LEFT JOIN tblOAuth2Scopes_Dict AS s_d ON s_d.ScopeID = s.ScopeID AND s_d.CCode = @CCode " +
                                     "WHERE us.UserID = @UserID AND us.ClientID = @ClientID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("UserID", userId);
                    cmd.Parameters.AddWithValue("ClientID", clientId);
                    cmd.Parameters.AddWithValue("CCode", cCode);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var userScopes = new List<UserScope>();
                        int ordinal;
                        while (reader.Read())
                            userScopes.Add(new UserScope
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                                ClientId = reader.GetString(reader.GetOrdinal("ClientID")),
                                Grant = reader.GetBoolean(reader.GetOrdinal("Grant")),

                                Scope = new Scope
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("SCOPE_ID")),
                                    Name = reader.GetString(reader.GetOrdinal("SCOPE_Name")),
                                    Description = !reader.IsDBNull(ordinal = reader.GetOrdinal("SCOPE_Description")) ? reader.GetString(ordinal) : null
                                }
                            });

                        return userScopes;
                    }
                }
            }
        }

        public async Task<IEnumerable<UserScope>> GetAllUserScopesAsync(int userId, string cCode)
        {
            using (var con = new SqlConnection(this.ConnectionString))
            {
                const string query = "SELECT us.UserID, us.ClientID, us.[Grant], " +
                                     "s.ScopeID AS SCOPE_ID, s.Name AS SCOPE_Name, ISNULL(s_d.Description, s.Description) AS SCOPE_Description " +
                                     "FROM tblOAuth2UserScopes AS us " +
                                     "JOIN tblOAuth2Scopes AS s ON s.ScopeID = us.ScopeID " +
                                     "LEFT JOIN tblOAuth2Scopes_Dict AS s_d ON s_d.ScopeID = s.ScopeID AND s_d.CCode = @CCode " +
                                     "WHERE us.UserID = @UserID";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("UserID", userId);
                    cmd.Parameters.AddWithValue("CCode", cCode);

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        var userScopes = new List<UserScope>();
                        int ordinal;
                        while (reader.Read())
                            userScopes.Add(new UserScope
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("UserID")),
                                ClientId = reader.GetString(reader.GetOrdinal("ClientID")),
                                Grant = reader.GetBoolean(reader.GetOrdinal("Grant")),

                                Scope = new Scope
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("SCOPE_ID")),
                                    Name = reader.GetString(reader.GetOrdinal("SCOPE_Name")),
                                    Description = !reader.IsDBNull(ordinal = reader.GetOrdinal("SCOPE_Description")) ? reader.GetString(ordinal) : null
                                }
                            });

                        return userScopes;
                    }
                }
            }
        }

        public async Task<bool> AddUserScopesAsync(params UserScope[] userScopes)
        {
            using (var con = new SqlConnection(this.ConnectionString))
            {
                const string query = "INSERT INTO tblOAuth2UserScopes " +
                                     "SELECT us.value('(./uid)[1]', 'int') AS UserID, us.value('(./cid)[1]', 'nvarchar(15)') AS ClientID, us.value('(./sid)[1]', 'int') AS ScopeID, us.value('(./grant)[1]', 'bit') AS [Grant] " +
                                     "FROM @UserScopes.nodes('/userscope') AS x(us)";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(new SqlParameter("UserScopes", SqlDbType.Xml)
                    {
                        Value = string.Join("", userScopes.Select(us => $"<userscope><uid>{us.UserId}</uid><cid>{us.ClientId}</cid><sid>{us.Scope.Id}</sid><grant>{(us.Grant ? 1 : 0)}</grant></userscope>"))
                    });

                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    var count = await cmd.ExecuteNonQueryAsync();
                    con.Close();

                    return count == userScopes.Length;
                }
            }
        }

        public async Task RemoveUserScopesAsync(params UserScope[] userScopes)
        {
            using (var con = new SqlConnection(this.ConnectionString))
            {
                var query = "DELETE FROM tblOAuth2UserScopes WHERE ";
                var where = "";
                foreach (var us in userScopes)
                {
                    if (!string.IsNullOrEmpty(where))
                        where += " OR ";
                    where += $"(UserID = {us.UserId} AND ClientID = '{us.ClientId}' AND ScopeID = {us.Scope.Id})";
                }
                query += where;

                using (var cmd = new SqlCommand(query, con))
                {
                    await con.OpenAsync();
                    con.ChangeDatabase("ASPState");
                    await cmd.ExecuteNonQueryAsync();
                    con.Close();
                }
            }
        }
        #endregion // Interface functions
    }
}
