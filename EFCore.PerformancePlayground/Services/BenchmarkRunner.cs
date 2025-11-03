using System.Data;
using System.Diagnostics;
using EFCore.PerformancePlayground.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EFCore.PerformancePlayground.Services;

public class BenchmarkRunner
{
    private readonly PerformanceDbContext _context;
    private readonly string _connectionString;
    private const int Iterations = 100;
    private readonly List<BenchmarkResult> _results = new();

    public BenchmarkRunner(PerformanceDbContext context, string connectionString)
    {
        _context = context;
        _connectionString = connectionString;
    }

    public (BenchmarkResult? result, Task task) RunTrackingVsAsNoTrackingAsync()
    {
        Console.WriteLine("\n=== Tracking vs AsNoTracking ===");
        Console.WriteLine($"Running {Iterations} iterations...\n");

        // Warmup
        _context.Products.Take(100).ToList();
        _context.Products.AsNoTracking().Take(100).ToList();

        // Test with Tracking (default)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var products = _context.Products
                .Where(p => p.IsActive)
                .Take(100)
                .ToList();
        }
        sw.Stop();
        var trackingTime = sw.ElapsedMilliseconds;

        // Test with AsNoTracking
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Take(100)
                .ToList();
        }
        sw.Stop();
        var noTrackingTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"With Tracking:     {trackingTime} ms");
        Console.WriteLine($"With AsNoTracking: {noTrackingTime} ms");
        Console.WriteLine($"Speedup:            {(double)trackingTime / noTrackingTime:F2}x faster");
        Console.WriteLine($"Time saved:         {trackingTime - noTrackingTime} ms ({100.0 * (trackingTime - noTrackingTime) / trackingTime:F1}%)");

        var result = new BenchmarkResult
        {
            ScenarioName = "Tracking vs AsNoTracking",
            Approach1 = "With Tracking",
            Approach1Time = trackingTime,
            Approach2 = "AsNoTracking",
            Approach2Time = noTrackingTime,
            Speedup = (double)trackingTime / noTrackingTime,
            TimeSaved = trackingTime - noTrackingTime,
            TimeSavedPercent = 100.0 * (trackingTime - noTrackingTime) / trackingTime
        };

        return (result, Task.CompletedTask);
    }

    public (BenchmarkResult? result, Task task) RunIQueryableVsIEnumerableAsync()
    {
        Console.WriteLine("\n=== IQueryable vs IEnumerable ===");
        Console.WriteLine($"Running {Iterations} iterations...\n");

        // Test IQueryable (deferred execution - filtering in database)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            IQueryable<Product> query = _context.Products
                .Where(p => p.IsActive && p.Price > 100);
            
            var results = query
                .Where(p => p.Category == "Electronics")
                .OrderBy(p => p.Price)
                .Take(50)
                .ToList();
        }
        sw.Stop();
        var iqueryableTime = sw.ElapsedMilliseconds;

        // Test IEnumerable (in-memory filtering)
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            IEnumerable<Product> enumerable = _context.Products
                .Where(p => p.IsActive && p.Price > 100)
                .ToList(); // Force execution - now in memory
            
            var results = enumerable
                .Where(p => p.Category == "Electronics")
                .OrderBy(p => p.Price)
                .Take(50)
                .ToList();
        }
        sw.Stop();
        var ienumerableTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"IQueryable (DB):    {iqueryableTime} ms");
        Console.WriteLine($"IEnumerable (Mem):  {ienumerableTime} ms");
        Console.WriteLine($"Speedup:            {(double)ienumerableTime / iqueryableTime:F2}x faster");
        Console.WriteLine($"Time saved:         {ienumerableTime - iqueryableTime} ms ({100.0 * (ienumerableTime - iqueryableTime) / ienumerableTime:F1}%)");

        var result = new BenchmarkResult
        {
            ScenarioName = "IQueryable vs IEnumerable",
            Approach1 = "IQueryable (DB)",
            Approach1Time = iqueryableTime,
            Approach2 = "IEnumerable (Mem)",
            Approach2Time = ienumerableTime,
            Speedup = (double)ienumerableTime / iqueryableTime,
            TimeSaved = ienumerableTime - iqueryableTime,
            TimeSavedPercent = 100.0 * (ienumerableTime - iqueryableTime) / ienumerableTime
        };

        return (result, Task.CompletedTask);
    }

    public (BenchmarkResult? result, Task task) RunProjectionVsEntitiesAsync()
    {
        Console.WriteLine("\n=== Projection to DTO vs Returning Entities ===");
        Console.WriteLine($"Running {Iterations} iterations...\n");

        // Warmup
        _context.Orders.Include(o => o.OrderItems).Take(50).ToList();

        // Test returning full entities
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate > DateTime.Now.AddDays(-30))
                .Take(50)
                .ToList();
            
            // Simulate processing
            var results = orders.Select(o => new
            {
                o.Id,
                o.CustomerName,
                Total = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
            }).ToList();
        }
        sw.Stop();
        var entitiesTime = sw.ElapsedMilliseconds;

        // Test with projection
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            var results = _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate > DateTime.Now.AddDays(-30))
                .Select(o => new
                {
                    o.Id,
                    o.CustomerName,
                    Total = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .Take(50)
                .ToList();
        }
        sw.Stop();
        var projectionTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"Full Entities:      {entitiesTime} ms");
        Console.WriteLine($"Projection (DTO):   {projectionTime} ms");
        Console.WriteLine($"Speedup:            {(double)entitiesTime / projectionTime:F2}x faster");
        Console.WriteLine($"Time saved:         {entitiesTime - projectionTime} ms ({100.0 * (entitiesTime - projectionTime) / entitiesTime:F1}%)");

        var result = new BenchmarkResult
        {
            ScenarioName = "Projection vs Entities",
            Approach1 = "Full Entities",
            Approach1Time = entitiesTime,
            Approach2 = "Projection (DTO)",
            Approach2Time = projectionTime,
            Speedup = (double)entitiesTime / projectionTime,
            TimeSaved = entitiesTime - projectionTime,
            TimeSavedPercent = 100.0 * (entitiesTime - projectionTime) / entitiesTime
        };

        return (result, Task.CompletedTask);
    }

    public (BenchmarkResult? result, Task task) RunCompiledQueriesAsync()
    {
        Console.WriteLine("\n=== Compiled Queries ===");
        Console.WriteLine($"Running {Iterations} iterations...\n");

        // Warmup
        _context.Products.Where(p => p.Category == "Electronics" && p.Price > 100).ToList();

        // Test regular query
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.Category == "Electronics" && p.Price > 100)
                .OrderBy(p => p.Price)
                .ToList();
        }
        sw.Stop();
        var regularTime = sw.ElapsedMilliseconds;

        // Test compiled query
        // Note: Compiled queries must return IQueryable<T>, not IOrderedQueryable<T>
        // So we do OrderBy after the compiled query returns
        var compiledQuery = EF.CompileQuery(
            (PerformanceDbContext context, string category, decimal minPrice) =>
                context.Products
                    .AsNoTracking()
                    .Where(p => p.Category == category && p.Price > minPrice));

        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var products = compiledQuery(_context, "Electronics", 100)
                .OrderBy(p => p.Price)
                .ToList();
        }
        sw.Stop();
        var compiledTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"Regular Query:      {regularTime} ms");
        Console.WriteLine($"Compiled Query:     {compiledTime} ms");
        Console.WriteLine($"Speedup:            {(double)regularTime / compiledTime:F2}x faster");
        Console.WriteLine($"Time saved:         {regularTime - compiledTime} ms ({100.0 * (regularTime - compiledTime) / regularTime:F1}%)");

        var result = new BenchmarkResult
        {
            ScenarioName = "Compiled Queries",
            Approach1 = "Regular Query",
            Approach1Time = regularTime,
            Approach2 = "Compiled Query",
            Approach2Time = compiledTime,
            Speedup = (double)regularTime / compiledTime,
            TimeSaved = regularTime - compiledTime,
            TimeSavedPercent = 100.0 * (regularTime - compiledTime) / regularTime
        };

        return (result, Task.CompletedTask);
    }

    public (BenchmarkResult? result, Task task) RunBatchingAsync()
    {
        Console.WriteLine("\n=== Batching (Multiple Queries vs Single Include) ===");
        Console.WriteLine($"Running {Iterations} iterations...\n");

        var orderIds = _context.Orders.Select(o => o.Id).Take(100).ToList();

        // Test: Multiple separate queries (N+1 problem)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var orders = _context.Orders
                .AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .ToList();

            foreach (var order in orders)
            {
                var items = _context.OrderItems
                    .AsNoTracking()
                    .Where(oi => oi.OrderId == order.Id)
                    .Include(oi => oi.Product)
                    .ToList();
            }
        }
        sw.Stop();
        var multipleQueriesTime = sw.ElapsedMilliseconds;

        // Test: Single query with Include
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var orders = _context.Orders
                .AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToList();
        }
        sw.Stop();
        var singleQueryTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"Multiple Queries:   {multipleQueriesTime} ms");
        Console.WriteLine($"Single Include:     {singleQueryTime} ms");
        Console.WriteLine($"Speedup:            {(double)multipleQueriesTime / singleQueryTime:F2}x faster");
        Console.WriteLine($"Time saved:         {multipleQueriesTime - singleQueryTime} ms ({100.0 * (multipleQueriesTime - singleQueryTime) / multipleQueriesTime:F1}%)");

        var result = new BenchmarkResult
        {
            ScenarioName = "Batching",
            Approach1 = "Multiple Queries",
            Approach1Time = multipleQueriesTime,
            Approach2 = "Single Include",
            Approach2Time = singleQueryTime,
            Speedup = (double)multipleQueriesTime / singleQueryTime,
            TimeSaved = multipleQueriesTime - singleQueryTime,
            TimeSavedPercent = 100.0 * (multipleQueriesTime - singleQueryTime) / multipleQueriesTime
        };

        return (result, Task.CompletedTask);
    }

    public (BenchmarkResult? result, Task task) RunQueryMethodComparisonAsync()
    {
        Console.WriteLine("\n=== Query Method Comparison ===");
        Console.WriteLine("Comparing: EF Core LINQ vs FromSqlRaw vs Raw SQL (ADO.NET)");
        Console.WriteLine($"Running {Iterations} iterations...\n");

        // Warmup
        _context.Products.Where(p => p.IsActive && p.Price > 100).Take(50).ToList();
        _context.Products.FromSqlRaw("SELECT * FROM Products WHERE IsActive = 1 AND Price > 100").Take(50).ToList();

        // Test 1: EF Core LINQ
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var products = _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.Price > 100)
                .OrderBy(p => p.Price)
                .Take(50)
                .ToList();
        }
        sw.Stop();
        var linqTime = sw.ElapsedMilliseconds;

        // Test 2: EF Core FromSqlRaw
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            _context.ChangeTracker.Clear();
            var products = _context.Products
                .FromSqlRaw("SELECT * FROM Products WHERE IsActive = 1 AND Price > {0} ORDER BY Price OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY", 100)
                .AsNoTracking()
                .ToList();
        }
        sw.Stop();
        var fromSqlTime = sw.ElapsedMilliseconds;

        // Test 3: Raw SQL with ADO.NET (simulating Dapper)
        sw.Restart();
        for (int i = 0; i < Iterations; i++)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            
            using var command = new SqlCommand(
                "SELECT Id, Name, Category, Price, IsActive FROM Products WHERE IsActive = 1 AND Price > @price ORDER BY Price OFFSET 0 ROWS FETCH NEXT 50 ROWS ONLY",
                connection);
            
            command.Parameters.AddWithValue("@price", 100);
            
            using var reader = command.ExecuteReader();
            var products = new List<Product>();
            
            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Category = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    IsActive = reader.GetBoolean(4)
                });
            }
        }
        sw.Stop();
        var rawSqlTime = sw.ElapsedMilliseconds;

        // Print all three results
        Console.WriteLine($"EF Core LINQ:       {linqTime} ms");
        Console.WriteLine($"EF Core FromSqlRaw: {fromSqlTime} ms");
        Console.WriteLine($"Raw SQL (ADO.NET): {rawSqlTime} ms");
        Console.WriteLine();

        // Calculate speedups
        var linqVsFromSql = (double)linqTime / fromSqlTime;
        var linqVsRawSql = (double)linqTime / rawSqlTime;
        var fromSqlVsRawSql = (double)fromSqlTime / rawSqlTime;

        var fastest = Math.Min(Math.Min(linqTime, fromSqlTime), rawSqlTime);
        var fastestMethod = linqTime == fastest ? "LINQ" : (fromSqlTime == fastest ? "FromSqlRaw" : "Raw SQL");

        Console.WriteLine($"Fastest: {fastestMethod} ({fastest} ms)");
        var linqVsFromSqlMessage = linqTime > fromSqlTime ? "FromSqlRaw faster" : "LINQ faster";
        var linqVsRawSqlMessage = linqTime > rawSqlTime ? "Raw SQL faster" : "LINQ faster";
        var fromSqlVsRawSqlMessage = fromSqlTime > rawSqlTime ? "Raw SQL faster" : "FromSqlRaw faster";
        Console.WriteLine($"LINQ vs FromSqlRaw: {linqVsFromSql:F2}x ({linqVsFromSqlMessage})");
        Console.WriteLine($"LINQ vs Raw SQL:    {linqVsRawSql:F2}x ({linqVsRawSqlMessage})");
        Console.WriteLine($"FromSqlRaw vs Raw SQL: {fromSqlVsRawSql:F2}x ({fromSqlVsRawSqlMessage})");

        // Return a BenchmarkResult comparing LINQ vs Raw SQL (fastest alternative)
        var result = new BenchmarkResult
        {
            ScenarioName = "Query Method Comparison",
            Approach1 = "EF Core LINQ",
            Approach1Time = linqTime,
            Approach2 = rawSqlTime < fromSqlTime ? "Raw SQL (ADO.NET)" : "EF Core FromSqlRaw",
            Approach2Time = Math.Min(fromSqlTime, rawSqlTime),
            Speedup = linqTime > Math.Min(fromSqlTime, rawSqlTime) 
                ? (double)linqTime / Math.Min(fromSqlTime, rawSqlTime)
                : (double)Math.Min(fromSqlTime, rawSqlTime) / linqTime,
            TimeSaved = linqTime > Math.Min(fromSqlTime, rawSqlTime)
                ? linqTime - Math.Min(fromSqlTime, rawSqlTime)
                : Math.Min(fromSqlTime, rawSqlTime) - linqTime,
            TimeSavedPercent = 100.0 * Math.Abs(linqTime - Math.Min(fromSqlTime, rawSqlTime)) / linqTime
        };

        return (result, Task.CompletedTask);
    }

    public Task<List<BenchmarkResult>> RunAllAsync()
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("EF Core Performance Benchmark");
        Console.WriteLine(new string('=', 60));
        
        var results = new List<BenchmarkResult>();

        var (r1, _) = RunTrackingVsAsNoTrackingAsync();
        if (r1 != null) results.Add(r1);

        var (r2, _) = RunIQueryableVsIEnumerableAsync();
        if (r2 != null) results.Add(r2);

        var (r3, _) = RunProjectionVsEntitiesAsync();
        if (r3 != null) results.Add(r3);

        var (r4, _) = RunCompiledQueriesAsync();
        if (r4 != null) results.Add(r4);

        var (r5, _) = RunBatchingAsync();
        if (r5 != null) results.Add(r5);

        var (r6, _) = RunQueryMethodComparisonAsync();
        if (r6 != null) results.Add(r6);
        
        return Task.FromResult(results);
    }

    public void PrintSummaryTable(List<BenchmarkResult> results)
    {
        Console.WriteLine("\n" + new string('=', 100));
        Console.WriteLine("BENCHMARK SUMMARY");
        Console.WriteLine(new string('=', 100));
        Console.WriteLine();

        // Table header
        Console.WriteLine($"{"Scenario",-35} {"Approach 1",-20} {"Time (ms)",-12} {"Approach 2",-20} {"Time (ms)",-12} {"Speedup",-10}");
        Console.WriteLine(new string('-', 100));

        // Table rows
        foreach (var result in results)
        {
            Console.WriteLine($"{result.ScenarioName,-35} {result.Approach1,-20} {result.Approach1Time,-12} {result.Approach2,-20} {result.Approach2Time,-12} {result.Speedup:F2}x");
        }

        Console.WriteLine(new string('-', 100));
        Console.WriteLine();

        // Summary statistics
        Console.WriteLine("Summary:");
        Console.WriteLine($"  Total scenarios run: {results.Count}");
        Console.WriteLine($"  Average speedup: {results.Average(r => r.Speedup):F2}x");
        Console.WriteLine($"  Best speedup: {results.Max(r => r.Speedup):F2}x ({results.OrderByDescending(r => r.Speedup).First().ScenarioName})");
        Console.WriteLine($"  Total time saved: {results.Sum(r => r.TimeSaved)} ms");
        
        Console.WriteLine("\n" + new string('=', 100));
    }
}
