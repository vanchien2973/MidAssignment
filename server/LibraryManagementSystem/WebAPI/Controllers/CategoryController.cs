using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(
        IMediator mediator,
        ILogger<CategoryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryListDto>>> GetCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = null,
        [FromQuery] string sortOrder = null,
        [FromQuery] string searchTerm = null)
    {
        var queryParams = new CategoryQueryParametersDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder,
            SearchTerm = searchTerm
        };

        var query = CategoryMapper.ToQuery(queryParams);
        var result = await _mediator.Send(query);
        
        var countQuery = CategoryMapper.ToCountQuery(searchTerm);
        var totalCount = await _mediator.Send(countQuery);

        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Page-Number"] = pageNumber.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDetailsDto>> GetCategory(Guid id)
    {
        var query = CategoryMapper.ToQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound($"Category with ID {id} not found");
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = CategoryMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        return StatusCode(201, new { message = "Category created successfully" });
    }

    [HttpPut]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = CategoryMapper.ToCommand(dto);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Category updated successfully" });
        }
        else
        {
            return NotFound(new { message = $"Category with ID {dto.CategoryId} not found" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid category ID" });
        }

        var command = CategoryMapper.ToCommand(id);
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { message = "Category deleted successfully" });
        }
        else
        {
            return NotFound(new { 
                message = $"Category with ID {id} not found or cannot be deleted because it has associated books" 
            });
        }
    }
}