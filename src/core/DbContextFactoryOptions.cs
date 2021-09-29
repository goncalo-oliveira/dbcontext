using System;

namespace System.Data
{
    internal class DbContextFactoryOptions
    {
        public Type ContextType { get; set; }
        public DbContextOptions ContextOptions { get; set; }
    }
}
