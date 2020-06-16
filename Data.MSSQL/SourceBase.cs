using System;

namespace Data.MSSQL
{
    public abstract class SourceBase
    {
        protected readonly string connectionString;

        protected SourceBase(string connectionString) => this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
}
