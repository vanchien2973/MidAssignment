using LibraryManagementSystem.Application.DTOs.Auth;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var command = AuthMapper.ToCommand(request);
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return Unauthorized(AuthMapper.ToAuthResponseDto(result));
        }
        
        return Ok(AuthMapper.ToAuthResponseDto(result));
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var command = AuthMapper.ToCommand(request);
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(AuthMapper.ToAuthResponseDto(result));
        }
        
        return Ok(AuthMapper.ToAuthResponseDto(result));
    }
    
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var command = AuthMapper.ToCommand(request);
        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return Unauthorized(AuthMapper.ToAuthResponseDto(result));
        }
        
        return Ok(AuthMapper.ToAuthResponseDto(result));
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new CurrentUserDto 
            { 
                Success = false, 
                Message = "User not authenticated" 
            });
        }
        
        var query = AuthMapper.ToQuery(userId);
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return NotFound(AuthMapper.ToCurrentUserDto(result));
        }
        
        return Ok(AuthMapper.ToCurrentUserDto(result));
    }
}