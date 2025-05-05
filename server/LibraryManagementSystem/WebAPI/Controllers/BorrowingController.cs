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
        try
        {
            var query = BorrowingMapper.ToQuery(id);
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Borrowing request with ID {id} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrowing request {RequestId}", id);
            throw;
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserBorrowingRequests(
        int userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = BorrowingMapper.ToQuery(userId, pageNumber, pageSize);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrowing requests for user {UserId}", userId);
            throw;
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> GetPendingBorrowingRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = BorrowingMapper.ToPendingQuery(pageNumber, pageSize);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending borrowing requests");
            throw;
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> GetAllBorrowingRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all borrowing requests");
            throw;
        }
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> GetOverdueBorrowings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = BorrowingMapper.ToOverdueQuery(pageNumber, pageSize);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue borrowings");
            throw;
        }
    }

    [HttpPost]
    [Authorize(Roles = "NormalUser,SuperUser")]
    public async Task<IActionResult> CreateBorrowingRequest([FromBody] CreateBorrowingRequestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = BorrowingMapper.ToCommand(dto);
            var result = await _mediator.Send(command);

            return StatusCode(201, new { requestId = result, message = "Borrowing request created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating borrowing request for user {UserId}", dto.RequestorId);
            throw;
        }
    }

    [HttpPut("status")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateBorrowingRequestStatus([FromBody] BorrowingRequestStatusUpdateDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating borrowing request status {RequestId}", dto.RequestId);
            throw;
        }
    }

    [HttpPut("return")]
    [Authorize(Roles = "NormalUser,SuperUser")]
    public async Task<IActionResult> ReturnBook([FromBody] ReturnBookDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning book {DetailId}", dto.DetailId);
            throw;
        }
    }

    [HttpPut("extend")]
    [Authorize(Roles = "NormalUser,SuperUser")]
    public async Task<IActionResult> ExtendBorrowing([FromBody] ExtendBorrowingDto dto)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending borrowing period {DetailId}", dto.DetailId);
            throw;
        }
    }
}