using System;

namespace Data.MSSQL
{
    public abstract class SourceBase
    {
        protected readonly string ConnectionString;

        protected SourceBase(string connectionString) => ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
}
