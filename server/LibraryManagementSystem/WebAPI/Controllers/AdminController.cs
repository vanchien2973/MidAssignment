using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperUserOnly")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;
    
    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserSearchDto searchDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var query = UserMapper.ToQuery(searchDto);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
    
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var query = UserMapper.ToQuery(id);
        var user = await _mediator.Send(query);
        
        if (user == null)
        {
            return NotFound(new UserResponseDto 
            { 
                Success = false, 
                Message = $"User with ID {id} not found" 
            });
        }
        
        return Ok(UserMapper.ToResponseDto(user));
    }
    
    [HttpGet("users/activity-logs")]
    public async Task<IActionResult> GetUserActivityLogs([FromQuery] UserActivityLogSearchDto searchDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var query = UserMapper.ToQuery(searchDto);
        var logs = await _mediator.Send(query);
        
        return Ok(logs);
    }
    
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        // Ensure the request contains the user ID
        request.UserId = id;
        
        var updateRoleCommand = UserMapper.ToCommand(request);
        
        var success = await _mediator.Send(updateRoleCommand);
        
        if (!success)
        {
            return NotFound(UserMapper.ToResponseDto(success, $"User with ID {id} not found or could not be updated"));
        }
        
        return Ok(UserMapper.ToResponseDto(success, $"User role with ID {id} has been updated successfully"));
    }
    
    [HttpPost("users/{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var dto = new ActivateUserDto { UserId = id };
        var command = UserMapper.ToCommand(dto);
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return NotFound(UserMapper.ToResponseDto(success, $"User with ID {id} not found or could not be activated"));
        }
        
        return Ok(UserMapper.ToResponseDto(success, $"User with ID {id} has been activated successfully"));
    }
    
    [HttpPost("users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var dto = new DeactivateUserDto { UserId = id };
        var command = UserMapper.ToCommand(dto);
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return NotFound(UserMapper.ToResponseDto(success,
                $"User with ID {id} not found or could not be deactivated. SuperUser accounts cannot be deactivated.")); 
        }
        
        return Ok(UserMapper.ToResponseDto(success, $"User with ID {id} has been deactivated successfully"));
    }
    
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var dto = new DeleteUserDto { UserId = id };
        var command = UserMapper.ToCommand(dto);
        var success = await _mediator.Send(command);
        
        if (!success)
        {
            return NotFound(UserMapper.ToResponseDto(success, $"User with ID {id} not found or could not be deleted"));
        }
        
        return Ok(UserMapper.ToResponseDto(success, $"User with ID {id} has been deleted successfully"));
    }
}