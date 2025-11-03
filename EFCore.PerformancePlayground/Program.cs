using EFCore.PerformancePlayground.Data;
using EFCore.PerformancePlayground.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var connectionString = "Server=(localdb)\\mssqllocaldb;Database=EFCorePerformancePlayground;Trusted_Connection=True;MultipleActiveResultSets=true";

var builder = new DbContextOptionsBuilder<PerformanceDbContext>();
builder.UseSqlServer(connectionString);

using var context = new PerformanceDbContext(builder.Options);

// Ensure database is created and seeded
Console.WriteLine("Initializing database...");
// Drop database if it exists to ensure clean schema with ValueGeneratedNever
if (context.Database.CanConnect())
{
    Console.WriteLine("Dropping existing database...");
    context.Database.EnsureDeleted();
}
context.Database.EnsureCreated();
context.SeedData();
Console.WriteLine("Database initialized!\n");

var benchmarkRunner = new BenchmarkRunner(context, connectionString);

// Parse command line arguments
// Support: dotnet run -- scenario=tracking
// Support: dotnet run -- mode=web
string? scenario = null;
string? mode = null;

if (args.Length > 0)
{
    foreach (var arg in args)
    {
        if (arg.Contains("scenario="))
        {
            var parts = arg.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                scenario = parts[1].ToLower().Trim();
            }
        }
        else if (arg.Contains("mode="))
        {
            var parts = arg.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                mode = parts[1].ToLower().Trim();
            }
        }
    }
}

// Check if web mode is requested
if (mode == "web")
{
    // Run web UI
    var webBuilder = WebApplication.CreateBuilder(args);
    
    webBuilder.Services.AddSingleton(benchmarkRunner);
    webBuilder.Services.AddSingleton(context);
    webBuilder.Services.AddCors();
    webBuilder.Services.AddRazorPages();
    
    var app = webBuilder.Build();
    
    app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
    
    // Serve static files from wwwroot
    app.UseStaticFiles();
    
    // Map Razor Pages
    app.MapRazorPages();
    
    // API endpoints
    var api = app.MapGroup("/api");
    
    api.MapGet("/scenarios", () =>
    {
        return Results.Ok(new[]
        {
            new { Name = "tracking", DisplayName = "Tracking vs AsNoTracking" },
            new { Name = "iqueryable", DisplayName = "IQueryable vs IEnumerable" },
            new { Name = "projection", DisplayName = "Projection vs Entities" },
            new { Name = "compiled", DisplayName = "Compiled Queries" },
            new { Name = "batching", DisplayName = "Batching (Multiple Queries vs Single Include)" },
            new { Name = "querymethod", DisplayName = "Query Method Comparison (LINQ vs FromSqlRaw vs Raw SQL)" },
            new { Name = "all", DisplayName = "All Scenarios" }
        });
    });
    
    api.MapPost("/run", async (BenchmarkRunner runner, RunRequest request) =>
    {
        try
        {
            var results = new List<BenchmarkResult>();
            
            if (request.Scenario == "all" || string.IsNullOrEmpty(request.Scenario))
            {
                var allResults = await runner.RunAllAsync();
                results.AddRange(allResults);
            }
            else
            {
                var (result, _) = request.Scenario switch
                {
                    "tracking" => runner.RunTrackingVsAsNoTrackingAsync(),
                    "iqueryable" => runner.RunIQueryableVsIEnumerableAsync(),
                    "projection" => runner.RunProjectionVsEntitiesAsync(),
                    "compiled" => runner.RunCompiledQueriesAsync(),
                    "batching" => runner.RunBatchingAsync(),
                    "querymethod" => runner.RunQueryMethodComparisonAsync(),
                    _ => (null, Task.CompletedTask)
                };
                
                if (result != null)
                {
                    results.Add(result);
                }
            }
            
            return Results.Ok(new { Success = true, Results = results });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    });
    
    api.MapGet("/status", () => Results.Ok(new { Status = "Ready", Database = "Connected" }));
    
    var port = 5000;
    Console.WriteLine($"\nWeb UI starting on http://localhost:{port}");
    Console.WriteLine($"Open http://localhost:{port} in your browser to view the UI\n");
    
    app.Run($"http://localhost:{port}");
}
else
{
    // Console mode (existing functionality)
    if (string.IsNullOrEmpty(scenario) || scenario == "all")
    {
        // Run all scenarios and show summary table
        var results = await benchmarkRunner.RunAllAsync();
        benchmarkRunner.PrintSummaryTable(results);
    }
    else
    {
        // Run specific scenario
        switch (scenario)
        {
            case "tracking":
                await benchmarkRunner.RunTrackingVsAsNoTrackingAsync().task;
                break;
            case "iqueryable":
                await benchmarkRunner.RunIQueryableVsIEnumerableAsync().task;
                break;
            case "projection":
                await benchmarkRunner.RunProjectionVsEntitiesAsync().task;
                break;
            case "compiled":
                await benchmarkRunner.RunCompiledQueriesAsync().task;
                break;
            case "batching":
                await benchmarkRunner.RunBatchingAsync().task;
                break;
            case "querymethod":
            case "queries":
                await benchmarkRunner.RunQueryMethodComparisonAsync().task;
                break;
            default:
                Console.WriteLine($"Unknown scenario: {scenario}");
                Console.WriteLine("\nAvailable scenarios:");
                Console.WriteLine("  tracking   - Tracking vs AsNoTracking");
                Console.WriteLine("  iqueryable  - IQueryable vs IEnumerable");
                Console.WriteLine("  projection - Projection vs Entities");
                Console.WriteLine("  compiled   - Compiled Queries");
                Console.WriteLine("  batching   - Batching (Multiple Queries vs Single Include)");
                Console.WriteLine("  querymethod - Query Method Comparison (LINQ vs FromSqlRaw vs Raw SQL)");
                Console.WriteLine("  all        - Run all scenarios with summary table");
                Console.WriteLine("\nModes:");
                Console.WriteLine("  mode=web   - Start web UI on http://localhost:5000");
                break;
        }
    }
}

// Request DTO for API
public record RunRequest(string Scenario);
