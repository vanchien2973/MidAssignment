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
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var query = UserMapper.ToQuery(searchDto);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }
    
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
            throw;
        }
    }
    
    [HttpGet("users/activity-logs")]
    public async Task<IActionResult> GetUserActivityLogs([FromQuery] UserActivityLogSearchDto searchDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var query = UserMapper.ToQuery(searchDto);
            var logs = await _mediator.Send(query);
            
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity logs");
            throw;
        }
    }
    
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto request)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role {UserId}", id);
            throw;
        }
    }
    
    [HttpPost("users/{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user {UserId}", id);
            throw;
        }
    }
    
    [HttpPost("users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            throw;
        }
    }
    
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            throw;
        }
    }
}