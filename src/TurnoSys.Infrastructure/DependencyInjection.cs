using Hangfire;
using Hangfire.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TurnoSys.Application.Common.Interfaces;
using TurnoSys.Domain.Interfaces.Services;
using TurnoSys.Infrastructure.Jobs;
using TurnoSys.Infrastructure.Persistence;
using TurnoSys.Infrastructure.Persistence.Seeders;
using TurnoSys.Infrastructure.Services.Auth;
using TurnoSys.Infrastructure.Services.Email;

namespace TurnoSys.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database (MySQL via Pomelo)
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' no configurada.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<DatabaseSeeder>();

        // Auth
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();

        // Email
        services.AddHttpClient("Resend");
        services.AddScoped<EmailConfigResolver>();
        services.AddScoped<ResendEmailService>();
        services.AddScoped<SmtpEmailService>();
        services.AddScoped<IEmailService, EmailServiceFactory>();

        // Hangfire (MySQL storage)
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(
                connectionString,
                new MySqlStorageOptions
                {
                    TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    // Intervalos altos: la BD es externa (cada query es egress) y
                    // solo corremos 2 jobs diarios. Menos polling = menos egress.
                    QueuePollInterval = TimeSpan.FromSeconds(60),
                    JobExpirationCheckInterval = TimeSpan.FromHours(6),
                    CountersAggregateInterval = TimeSpan.FromMinutes(30),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "Hangfire"
                })));

        // Pocos workers y pollers espaciados: alcanza de sobra para 2 jobs diarios.
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2;
            options.SchedulePollingInterval = TimeSpan.FromSeconds(60);
            options.HeartbeatInterval = TimeSpan.FromMinutes(1);
            options.ServerCheckInterval = TimeSpan.FromMinutes(5);
        });
        services.AddScoped<RecordatoriosTurnosJob>();
        services.AddScoped<RecordatoriosControlJob>();
        services.AddScoped<IRecordatorioScheduler, HangfireRecordatorioScheduler>();

        return services;
    }
}
