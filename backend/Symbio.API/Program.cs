using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Symbio.API.Data;
using Symbio.API.Endpoints;
using Symbio.API.Hubs;
using Symbio.API.Middleware;
using Symbio.API.Services;
using Symbio.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddDbContext<SymbioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=SymbioHub.db"));

builder.Services.AddScoped<Symbio.Core.Repositories.IProjectRepository, Symbio.Infrastructure.ProjectRepository>();
builder.Services.AddScoped<Symbio.Core.Repositories.ITalentDiscoveryRepository, Symbio.Infrastructure.TalentDiscoveryRepository>();
builder.Services.AddScoped<Symbio.Core.Repositories.ICompletionEvidenceRepository, Symbio.Infrastructure.CompletionEvidenceRepository>();
builder.Services.AddScoped<Symbio.Core.Repositories.IPinchOnboardingGateway, Symbio.Infrastructure.MockPinchOnboardingGateway>();
builder.Services.AddScoped<Symbio.Core.Repositories.IIdentityVerificationService, Symbio.Infrastructure.IdentityVerificationService>();
builder.Services.AddHttpClient<Symbio.Core.Repositories.IPinchMerchantService, Symbio.Infrastructure.PinchMerchantService>();
builder.Services.AddHttpClient<Symbio.Core.Repositories.IPinchDebitService, Symbio.Infrastructure.PinchDebitService>();
builder.Services.AddHttpClient<Symbio.Core.Repositories.IRecurringBillingService, Symbio.Infrastructure.PinchRecurringBillingService>();
builder.Services.AddScoped<Symbio.Core.Repositories.IAccountingInvoicingService, Symbio.Infrastructure.PinchInvoicingService>();
builder.Services.AddSingleton<Symbio.Core.Services.IPaymentSplitCalculator, Symbio.Core.Services.PaymentSplitCalculator>();
builder.Services.AddSingleton<Symbio.Core.Services.IUsageMeteringEngine, Symbio.Core.Services.UsageMeteringEngine>();
builder.Services.AddScoped<PinchSignatureValidationFilter>();
builder.Services.AddHostedService<MilestoneDebitWorker>();
builder.Services.AddHostedService<RetainerBillingWorker>();
builder.Services.AddSecurityServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

SeedData.Initialize(app);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapAdminOperationsEndpoints();
app.MapHub<DeliveryWorkbenchHub>("/hubs/workbench");
app.MapHub<MarketplaceHub>("/hubs/marketplace");
app.MapHub<AccountingHub>("/hubs/accounting");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.UseSwagger();
app.UseSwaggerUI();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
