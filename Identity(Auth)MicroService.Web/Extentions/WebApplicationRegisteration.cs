using Identity_Auth_MicroService.Domain.Contracts;
using Identity_Auth_MicroService.Presistance.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Identity_Auth_MicroService.Web.Extentions
{
    public static class WebApplicationRegisteration
    {
        public static async Task<WebApplication> MigrateIdentityDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateAsyncScope();
            var DbContextService = scope.ServiceProvider.GetRequiredService<ClinicIdentityDbContext>();
            var pendingMigrations = await DbContextService.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await DbContextService.Database.MigrateAsync();
            }
            return app;
        }
        public static async Task<WebApplication> SeedIdentityDataAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateAsyncScope();
            var DataIntializerService = scope.ServiceProvider.GetRequiredService<IDataIntializer>();
            await DataIntializerService.IntializeAsync();
            return app;
        }
    }
}
