using System.Reflection;
using BatterySwap.Application.Services.Auth;
using BatterySwap.Application.Services.Batteries;
using BatterySwap.Application.Services.Cabinets;
using BatterySwap.Application.Services.Dashboard;
using BatterySwap.Application.Services.Reports;
using BatterySwap.Application.Services.Sessions;
using BatterySwap.Application.Services.Slots;
using BatterySwap.Application.Services.Stations;
using BatterySwap.Application.Services.Transactions;
using BatterySwap.Application.Services.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BatterySwap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<ICabinetService, CabinetService>();
        services.AddScoped<ISlotService, SlotService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBatteryService, BatteryService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
