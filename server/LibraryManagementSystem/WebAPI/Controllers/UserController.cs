using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Interfaces.Services;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UserController> _logger;
    
    public UserController(
        IMediator mediator, 
        ICurrentUserService currentUserService,
        ILogger<UserController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            int userId = _currentUserService.UserId;
            var query = UserMapper.ToQuery(userId);
            var user = await _mediator.Send(query);
            
            if (user == null)
            {
                return NotFound(new UserResponseDto 
                { 
                    Success = false, 
                    Message = "User profile not found"
                });
            }
            
            return Ok(UserMapper.ToResponseDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            throw;
        }
    }
    
    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetMyActivityLogs([FromQuery] UserActivityLogSearchDto searchDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Ensure the user is only retrieving their own logs
            searchDto.UserId = _currentUserService.UserId;
            
            var query = UserMapper.ToQuery(searchDto);
            var logs = await _mediator.Send(query);
            
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user activity logs");
            throw;
        }
    }
    
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Ensure the request contains the user ID
            request.UserId = _currentUserService.UserId;
            
            var command = UserMapper.ToCommand(request);
            var success = await _mediator.Send(command);
            
            if (!success)
            {
                return BadRequest(UserMapper.ToResponseDto(success, "Failed to update profile"));
            }
            
            return Ok(UserMapper.ToResponseDto(success, "Profile updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            throw;
        }
    }
    
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Ensure the request contains the user ID
            request.UserId = _currentUserService.UserId;
            
            var command = UserMapper.ToCommand(request);
            var success = await _mediator.Send(command);
            
            if (!success)
            {
                return BadRequest(UserMapper.ToResponseDto(success, "Failed to update password"));
            }
            
            return Ok(UserMapper.ToResponseDto(success, "Password updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user password");
            throw;
        }
    }
}