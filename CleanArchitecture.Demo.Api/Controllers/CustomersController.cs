using CleanArchitecture.Demo.Application.Commands.CreateCustomer;
using CleanArchitecture.Demo.Application.Queries.GetCustomerById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCustomer([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to create customer with email: {Email}", command.Email);
        
        try
        {
            var customerId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerById), new { id = customerId }, customerId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create customer");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating customer");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to get customer by ID: {CustomerId}", id);
        
        var query = new GetCustomerByIdQuery(id);
        var customer = await _mediator.Send(query, cancellationToken);

        if (customer == null)
        {
            return NotFound($"Customer with ID {id} not found.");
        }

        return Ok(customer);
    }
}

