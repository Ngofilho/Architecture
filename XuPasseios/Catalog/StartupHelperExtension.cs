using Catalog.Infra.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Catalog
{
    internal static class StartupHelperExtension
    {
        public static async Task ResetDataBaseAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetService<CatalogContext>();
                    if (context != null)
                    {
                        await context.Database.EnsureDeletedAsync();
                        await context.Database.MigrateAsync();
                    }
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
        }
    }
}
