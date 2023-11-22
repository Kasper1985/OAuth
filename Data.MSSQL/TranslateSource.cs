using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Data.Interfaces;

namespace Data.MSSQL
{
    public class TranslateSource : SourceBase, ITranslateSource
    {
        public TranslateSource(string connectionString) : base(connectionString) { }

        #region Interface functions
        public async Task<string> GetTranslationAsync(string text, string cCode)
        {
            using (var con = new SqlConnection(ConnectionString))
            {
                const string query = "SELECT BezValue " +
                                     "FROM tblStPortalTranslator " +
                                     "WHERE BezName = @text AND CCode = @ccode";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("text", text);
                    cmd.Parameters.AddWithValue("ccode", cCode);

                    await con.OpenAsync();
                    con.ChangeDatabase("CRM");
                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        int ordinal;
                        reader.Read();
                        return !reader.IsDBNull(ordinal = reader.GetOrdinal("BezValue")) ? reader.GetString(ordinal) : "";
                    }
                }
            }
        }
        #endregion // Interface functions
    }
}
