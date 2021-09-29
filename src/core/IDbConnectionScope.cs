using System;

namespace System.Data
{
    public interface IDbConnectionScope : IDisposable
    {
        IDbConnection Connection { get; }

        bool KeepOpen { get; }
    }
}
