using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookController> _logger;

    public BookController(
        IMediator mediator,
        ILogger<BookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookListDto>>> GetBooks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = null,
        [FromQuery] string sortOrder = null)
    {
        var queryParams = new BookQueryParametersDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        var query = BookMapper.ToQuery(queryParams);
        var result = await _mediator.Send(query);
        
        var countQuery = BookMapper.ToCountQuery();
        var totalCount = await _mediator.Send(countQuery);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page-Number"] = pageNumber.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDetailsDto>> GetBook(Guid id)
    {
        var query = BookMapper.ToQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound($"Book with ID {id} not found");
        }

        return Ok(result);
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<BookListDto>>> GetBooksByCategory(
        Guid categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (categoryId == Guid.Empty)
        {
            return BadRequest("Invalid category ID");
        }

        var query = BookMapper.ToBooksByCategoryQuery(categoryId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        var countQuery = BookMapper.ToBooksByCategoryCountQuery(categoryId);
        var totalCount = await _mediator.Send(countQuery);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page-Number"] = pageNumber.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(result);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<BookListDto>>> GetAvailableBooks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = BookMapper.ToAvailableBooksQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        
        var countQuery = BookMapper.ToAvailableBooksCountQuery();
        var totalCount = await _mediator.Send(countQuery);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page-Number"] = pageNumber.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = BookMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        if (result)
        {
            return StatusCode(201, new { message = "Book created successfully" });
        }
        else
        {
            return BadRequest(new { message = "Failed to create book" });
        }
    }

    [HttpPut]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateBook([FromBody] BookUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = BookMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Book updated successfully" });
        }
        else
        {
            return NotFound(new { message = $"Book with ID {dto.BookId} not found" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid book ID" });
        }

        var command = BookMapper.ToCommand(id);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Book deleted successfully" });
        }
        else
        {
            return NotFound(new { 
                message = $"Book with ID {id} not found or cannot be deleted because it has active borrowings" 
            });
        }
    }
}