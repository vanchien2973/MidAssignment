using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BorrowingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BorrowingController> _logger;

    public BorrowingController(
        IMediator mediator,
        ILogger<BorrowingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookBorrowingRequestDto>> GetBorrowingRequest(Guid id)
    {
        var query = BorrowingMapper.ToQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound($"Borrowing request with ID {id} not found");
        }

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserBorrowingRequests(
        int userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = BorrowingMapper.ToQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> GetPendingBorrowingRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = BorrowingMapper.ToPendingQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> GetAllBorrowingRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = BorrowingMapper.ToPendingQuery(pageNumber, pageSize, includeAllStatuses: true);
        var result = await _mediator.Send(query);
        
        var countQuery = BorrowingMapper.ToCountAllBorrowingRequestsQuery();
        var totalCount = await _mediator.Send(countQuery);
        
        var paginatedResult = new
        {
            totalCount = totalCount,
            pageNumber = pageNumber,
            pageSize = pageSize,
            results = result
        };
        
        return Ok(paginatedResult);
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> GetOverdueBorrowings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = BorrowingMapper.ToOverdueQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "NormalUser,SuperUser")]
    public async Task<IActionResult> CreateBorrowingRequest([FromBody] CreateBorrowingRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = BorrowingMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        return StatusCode(201, new { requestId = result, message = "Borrowing request created successfully" });
    }

    [HttpPut("status")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateBorrowingRequestStatus([FromBody] BorrowingRequestStatusUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = BorrowingMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Borrowing request status updated successfully" });
        }
        else
        {
            return NotFound(new { message = $"Borrowing request with ID {dto.RequestId} not found or cannot be updated" });
        }
    }

    [HttpPut("return")]
    [Authorize(Roles = "NormalUser,SuperUser")]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnBookDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = BorrowingMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Book returned successfully" });
        }
        else
        {
            return NotFound(new { message = $"Borrowing detail with ID {dto.DetailId} not found or cannot be returned" });
        }
    }

    [HttpPut("extend")]
    [Authorize(Roles = "NormalUser,SuperUser")]
    public async Task<IActionResult> ExtendBorrowing([FromBody] ExtendBorrowingDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = BorrowingMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Borrowing period extended successfully" });
        }
        else
        {
            return NotFound(new { message = $"Borrowing detail with ID {dto.DetailId} not found or cannot be extended" });
        }
    }
}