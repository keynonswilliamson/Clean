# EF Core Performance Playground

A .NET 8 console application demonstrating various EF Core 8 performance techniques side-by-side with timing comparisons.

## Features

This project demonstrates the following EF Core performance scenarios:

1. **Tracking vs AsNoTracking** - Comparing entity tracking overhead
2. **IQueryable vs IEnumerable** - Deferred execution vs in-memory filtering
3. **Projection to DTO vs Returning Entities** - Selecting only needed data
4. **Compiled Queries** - Pre-compiled queries for repeated execution
5. **Batching** - Multiple queries vs single Include (N+1 problem)

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB) or SQL Server Express

## Setup

1. Clone or navigate to the project directory
2. Restore packages:
   ```bash
   dotnet restore
   ```
3. Update the connection string in `Program.cs` if needed

## Running Benchmarks

### Run All Scenarios

```bash
dotnet run
```

or

```bash
dotnet run -- scenario=all
```

### Run Individual Scenarios

```bash
# Tracking vs AsNoTracking
dotnet run -- scenario=tracking

# IQueryable vs IEnumerable
dotnet run -- scenario=iqueryable

# Projection vs Entities
dotnet run -- scenario=projection

# Compiled Queries
dotnet run -- scenario=compiled

# Batching (Multiple Queries vs Single Include)
dotnet run -- scenario=batching
```

## Database Setup

The application automatically:
- Creates the database on first run
- Seeds ~500 Products across 8 categories
- Seeds ~2,000 Orders with 2-10 OrderItems each
- Totaling approximately ~10,000 OrderItems

## Benchmark Details

### Scenario 1: Tracking vs AsNoTracking

**What it tests:** The performance difference between tracking entities (for updates) vs read-only queries.

**Expected result:** `AsNoTracking` should be significantly faster for read-only scenarios.

### Scenario 2: IQueryable vs IEnumerable

**What it tests:** Database-side filtering vs in-memory filtering.

**Expected result:** `IQueryable` should be faster as it filters at the database level.

### Scenario 3: Projection to DTO vs Returning Entities

**What it tests:** Selecting only needed fields vs returning full entity graphs.

**Expected result:** Projections should be faster due to less data transfer and no entity tracking.

### Scenario 4: Compiled Queries

**What it tests:** Pre-compiled queries for scenarios where the same query is executed multiple times.

**Expected result:** Compiled queries eliminate query compilation overhead on each execution.

### Scenario 5: Batching

**What it tests:** The N+1 query problem vs using Include for eager loading.

**Expected result:** Single query with Include should dramatically outperform multiple separate queries.

## Results

Each scenario runs 100 iterations and reports:
- Execution time for each approach
- Speedup factor (how many times faster)
- Time saved in milliseconds and percentage

## Project Structure

```
EFCore.PerformancePlayground/
├── Data/
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── PerformanceDbContext.cs
├── Services/
│   └── BenchmarkRunner.cs
├── Program.cs
└── README.md
```

## Notes

- All benchmarks use a fixed random seed (42) for reproducibility
- The database is created automatically on first run
- Each benchmark includes warmup iterations before timing
- Results may vary based on hardware and database configuration

## License

This is a sample project for educational purposes.

