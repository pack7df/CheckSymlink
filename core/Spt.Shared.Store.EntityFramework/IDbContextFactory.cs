using Microsoft.EntityFrameworkCore;

namespace Spt.Shared.Store.EntityFramework
{
    public interface IDbContextFactory
    {
        DbContext GetContext();
    }

    public interface IDbContextFactory<out TContext> : IDbContextFactory where TContext : DbContext
    {
        new TContext GetContext();
    }
}
