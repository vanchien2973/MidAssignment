using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.DTOs.Auth;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Auth;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Controllers;

namespace LibraryManagementSystem.FunctionalTests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<ILogger<AuthController>> _loggerMock;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mediatorMock.Object, _loggerMock.Object);
        
        // Thiết lập HttpContext cho controller
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Test]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "Admin@123"
        };
        
        var loginCommand = new LoginCommand
        {
            Username = "admin",
            Password = "Admin@123"
        };
        
        var loginResponse = new LoginResponse
        {
            Success = true,
            Token = "mock_token_value",
            RefreshToken = "mock_refresh_token_value",
            User = new User
            {
                UserId = 1,
                Username = "admin",
                UserType = UserType.SuperUser,
                Email = "admin@example.com",
                FullName = "Admin User"
            }
        };
        
        _mediatorMock.Setup(m => m.Send(It.Is<LoginCommand>(cmd => 
            cmd.Username == loginCommand.Username && 
            cmd.Password == loginCommand.Password), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var response = okResult?.Value as AuthResponseDto;
        response?.Success.Should().BeTrue();
        response?.Token.Should().NotBeNull();
        response?.User?.Username.Should().Be("admin");
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "WrongPassword"
        };
        
        var loginCommand = new LoginCommand
        {
            Username = "admin",
            Password = "WrongPassword"
        };
        
        var loginResponse = new LoginResponse
        {
            Success = false
        };
        
        _mediatorMock.Setup(m => m.Send(It.Is<LoginCommand>(cmd => 
            cmd.Username == loginCommand.Username && 
            cmd.Password == loginCommand.Password), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult?.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Password = "NewUser@123",
            Email = "newuser@example.com",
            FullName = "New Test User"
        };
        
        var registerCommand = new RegisterCommand
        {
            Username = "newuser",
            Password = "NewUser@123",
            Email = "newuser@example.com",
            FullName = "New Test User"
        };
        
        var registerResponse = new RegisterResponse
        {
            Success = true
        };
        
        _mediatorMock.Setup(m => m.Send(It.Is<RegisterCommand>(cmd => 
            cmd.Username == registerCommand.Username && 
            cmd.Password == registerCommand.Password), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Test]
    public async Task Register_WithExistingUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "admin", // Đã tồn tại
            Password = "Test@123",
            Email = "anotheremail@example.com",
            FullName = "Another User"
        };
        
        var registerCommand = new RegisterCommand
        {
            Username = "admin",
            Password = "Test@123",
            Email = "anotheremail@example.com",
            FullName = "Another User"
        };
        
        var registerResponse = new RegisterResponse
        {
            Success = false,
            Message = "Username already exists"
        };
        
        _mediatorMock.Setup(m => m.Send(It.Is<RegisterCommand>(cmd => 
            cmd.Username == registerCommand.Username && 
            cmd.Password == registerCommand.Password), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var userId = "1";
        
        var currentUserResponse = new CurrentUserResponse
        {
            Success = true,
            UserId = "1",
            Username = "admin",
            Email = "admin@example.com",
            FullName = "Admin User",
            UserType = UserType.SuperUser.ToString()
        };
        
        // Thiết lập Claims Identity cho request context
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, UserType.SuperUser.ToString())
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext.User = claimsPrincipal;
        
        _mediatorMock.Setup(m => m.Send(It.Is<GetCurrentUserQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentUserResponse);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as CurrentUserDto;
        returnedResponse?.Success.Should().BeTrue();
        returnedResponse?.Data?.Username.Should().Be("admin");
    }

    [Test]
    public async Task RefreshToken_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            Token = "expired_token",
            RefreshToken = "valid_refresh_token"
        };
        
        var refreshTokenCommand = new RefreshTokenCommand
        {
            Token = "expired_token",
            RefreshToken = "valid_refresh_token"
        };
        
        var refreshResponse = new RefreshTokenResponse
        {
            Success = true,
            Token = "new_token",
            RefreshToken = "new_refresh_token"
        };
        
        _mediatorMock.Setup(m => m.Send(It.Is<RefreshTokenCommand>(cmd => 
            cmd.Token == refreshTokenCommand.Token && 
            cmd.RefreshToken == refreshTokenCommand.RefreshToken), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshResponse);

        // Act
        var result = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var response = okResult?.Value as AuthResponseDto;
        response?.Success.Should().BeTrue();
    }
} 