﻿using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

using Models;
using Data.Interfaces;
using Data.Common.Exceptions;

namespace Data.MSSQL
{
    public class UserSource : SourceBase, IUserSource
    {
        public UserSource(string connectionString) : base(connectionString) { }

        #region Interface functions
        public async Task<User> GetUserAsync(int userId)
        {
            using (SqlConnection con = new SqlConnection(this.connectionString))
            {
                string query = "SELECT PSID AS ID, AnredeID AS Salutation, Titel AS Title, Vorname AS NameFirst, Nachname AS NameLast, dwTel AS Phone, dwFax AS Fax, Mail AS EMail " +
                               "FROM tblPs " +
                               "WHERE PSID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("id", userId);

                    await con.OpenAsync();
                    con.ChangeDatabase("PCMData");
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        List<User> users = new List<User>();
                        int ordinal;
                        while (reader.Read())
                            users.Add(new User
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                Salutation = !reader.IsDBNull(ordinal = reader.GetOrdinal("Salutation")) ? ((User.Salutations)reader.GetInt32(ordinal)).ToString() : "",
                                Title = !reader.IsDBNull(ordinal = reader.GetOrdinal("Title")) ? reader.GetString(ordinal) : "",
                                NameFirst = !reader.IsDBNull(ordinal = reader.GetOrdinal("NameFirst")) ? reader.GetString(ordinal) : "",
                                NameLast = !reader.IsDBNull(ordinal = reader.GetOrdinal("NameLast")) ? reader.GetString(ordinal) : "",
                                Phone = !reader.IsDBNull(ordinal = reader.GetOrdinal("Phone")) ? reader.GetString(ordinal) : "",
                                Fax = !reader.IsDBNull(ordinal = reader.GetOrdinal("Fax")) ? reader.GetString(ordinal) : "",
                                EMail = !reader.IsDBNull(ordinal = reader.GetOrdinal("EMail")) ? reader.GetString(ordinal) : "",
                            });

                        if (users.Count > 1)
                            throw new AmbiguousLoginException(userId);
                        else if (users.Count < 1)
                            throw new UserNotFoundException(userId);
                        else
                            return users[0];
                    }
                }
            }
        }

        public async Task<User> LoginAsync(string login, string password)
        {
            using (SqlConnection con = new SqlConnection(this.connectionString))
            {
                string query = "SELECT DISTINCT [user].PSID AS USER_ID, [user].AnredeID AS USER_Salutation, [user].Titel AS USER_Title, [user].Vorname AS USER_NameFirst, [user].Nachname AS USER_NameLast, " +
                                               "[user].dwTel AS USER_Phone, [user].dwFax AS USER_Fax, [user].Mail AS USER_EMail " +
                               "FROM tblStPortalPers AS person " +
                               "JOIN PCMData.dbo.tblPs AS [user] ON [user].PSID = person.PsId " +
                               "WHERE person.UserName = @login AND person.Passwort = @password COLLATE Latin1_General_CS_AS " +
                                 "AND NOT EXISTS(SELECT 1 FROM ASPState.dbo.tblSysSystemwartung AS ssrv WHERE ssrv.Systemwartung = 1)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("login", login);
                    cmd.Parameters.AddWithValue("password", password);

                    await con.OpenAsync();
                    con.ChangeDatabase("CRM");
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        List<User> users = new List<User>();
                        while (reader.Read())
                        {
                            int ordinal;
                            users.Add(new User
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("USER_ID")),
                                Salutation = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_Salutation")) ? ((User.Salutations)reader.GetInt32(ordinal)).ToString() : "",
                                Title = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_Title")) ? reader.GetString(ordinal) : "",
                                NameFirst = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_NameFirst")) ? reader.GetString(ordinal) : "",
                                NameLast = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_NameLast")) ? reader.GetString(ordinal) : "",
                                Phone = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_Phone")) ? reader.GetString(ordinal) : "",
                                Fax = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_Fax")) ? reader.GetString(ordinal) : "",
                                EMail = !reader.IsDBNull(ordinal = reader.GetOrdinal("USER_EMail")) ? reader.GetString(ordinal) : "",
                            });
                        }

                        if (users.Count > 1)
                            throw new AmbiguousLoginException(login);
                        else if (users.Count < 1)
                            return null;
                        else
                            return users[0];
                    }
                }
            }
        }
        #endregion // Interface functions
    }
}