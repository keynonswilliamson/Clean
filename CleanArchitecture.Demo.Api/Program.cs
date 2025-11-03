using CleanArchitecture.Demo.Application.Commands.CreateCustomer;
using CleanArchitecture.Demo.Application.Commands.CreateOrder;
using CleanArchitecture.Demo.Application.Interfaces;
using CleanArchitecture.Demo.Application.Queries.GetCustomerById;
using CleanArchitecture.Demo.Application.Queries.GetCustomers;
using CleanArchitecture.Demo.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Clean Architecture Demo API",
        Version = "v1",
        Description = "A sample API demonstrating Clean Architecture principles with CQRS, MediatR, EF Core, FluentValidation, and Serilog"
    });
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Register IApplicationDbContext to use ApplicationDbContext implementation
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly));

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateCustomerCommandValidator).Assembly);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

// Minimal API Endpoints
var customersGroup = app.MapGroup("/api/customers").WithTags("Customers");

// POST /api/customers
customersGroup.MapPost("/", async (
    CreateCustomerRequest request,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    logger.LogInformation("Received request to create customer with email: {Email}", request.Email);
    
    try
    {
        var command = new CreateCustomerCommand(request.Name, request.Email);
        var customerId = await mediator.Send(command, cancellationToken);
        return Results.CreatedAtRoute(
            "GetCustomerById",
            new { id = customerId },
            customerId);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "Failed to create customer");
        return Results.Conflict(ex.Message);
    }
    catch (ValidationException ex)
    {
        logger.LogWarning(ex, "Validation failed for create customer request");
        return Results.BadRequest(ex.Errors);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error creating customer");
        return Results.Problem("An error occurred while processing your request.");
    }
})
.WithName("CreateCustomer")
.WithOpenApi();

// GET /api/customers/{id}
customersGroup.MapGet("/{id:guid}", async (
    Guid id,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    logger.LogInformation("Received request to get customer by ID: {CustomerId}", id);
    
    var query = new GetCustomerByIdQuery(id);
    var customer = await mediator.Send(query, cancellationToken);

    if (customer == null)
    {
        return Results.NotFound($"Customer with ID {id} not found.");
    }

    return Results.Ok(customer);
})
.WithName("GetCustomerById")
.WithOpenApi();

// GET /api/customers (paginated)
customersGroup.MapGet("/", async (
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken cancellationToken,
    int pageNumber = 1,
    int pageSize = 10) =>
{
    logger.LogInformation("Received request to get customers page {PageNumber} with size {PageSize}", 
        pageNumber, pageSize);
    
    // Validate pagination parameters
    if (pageNumber < 1)
    {
        return Results.BadRequest("Page number must be greater than 0.");
    }
    
    if (pageSize < 1 || pageSize > 100)
    {
        return Results.BadRequest("Page size must be between 1 and 100.");
    }

    var query = new GetCustomersQuery(pageNumber, pageSize);
    var result = await mediator.Send(query, cancellationToken);

    return Results.Ok(result);
})
.WithName("GetCustomers")
.WithOpenApi();

// POST /api/customers/{id}/orders
customersGroup.MapPost("/{id:guid}/orders", async (
    Guid id,
    CreateOrderRequest request,
    IMediator mediator,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    logger.LogInformation("Received request to create order for customer ID: {CustomerId} with total: {Total}", 
        id, request.Total);
    
    try
    {
        var command = new CreateOrderCommand(id, request.Total);
        var orderId = await mediator.Send(command, cancellationToken);
        
        return Results.CreatedAtRoute(
            "GetOrderById",
            new { customerId = id, orderId = orderId },
            orderId);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "Failed to create order");
        return Results.BadRequest(ex.Message);
    }
    catch (ValidationException ex)
    {
        logger.LogWarning(ex, "Validation failed for create order request");
        return Results.BadRequest(ex.Errors);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error creating order");
        return Results.Problem("An error occurred while processing your request.");
    }
})
.WithName("CreateOrder")
.WithOpenApi();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

// Request DTOs for minimal APIs
file record CreateCustomerRequest(string Name, string Email);
file record CreateOrderRequest(decimal Total);
