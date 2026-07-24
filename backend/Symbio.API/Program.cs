using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Symbio.API.Data;
using Symbio.API.Hubs;
using Symbio.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SymbioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=SymbioHub.db"));

builder.Services.AddScoped<Symbio.Core.Repositories.IProjectRepository, Symbio.Infrastructure.ProjectRepository>();
builder.Services.AddScoped<Symbio.Core.Repositories.ITalentDiscoveryRepository, Symbio.Infrastructure.TalentDiscoveryRepository>();
builder.Services.AddScoped<Symbio.Core.Repositories.ICompletionEvidenceRepository, Symbio.Infrastructure.CompletionEvidenceRepository>();
builder.Services.AddScoped<Symbio.Core.Repositories.IPinchOnboardingGateway, Symbio.Infrastructure.MockPinchOnboardingGateway>();
builder.Services.AddSecurityServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

SeedData.Initialize(app);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DeliveryWorkbenchHub>("/hubs/workbench");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

public partial class Program { }
