using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Application.Interfaces;
using LibraryManagementSystem.Application.Interfaces.Services;
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
public class UserControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<ILogger<UserController>> _loggerMock;
    private UserController _controller;
    private ClaimsPrincipal _normalUserPrincipal;
    private ClaimsPrincipal _adminPrincipal;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<UserController>>();
        _controller = new UserController(_mediatorMock.Object, _currentUserServiceMock.Object, _loggerMock.Object);
        
        // Thiết lập HttpContext
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        // Tạo normal user claims
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "2"),
            new Claim(ClaimTypes.Name, "user"),
            new Claim(ClaimTypes.Role, UserType.NormalUser.ToString())
        };
        var userIdentity = new ClaimsIdentity(userClaims, "Test");
        _normalUserPrincipal = new ClaimsPrincipal(userIdentity);
        
        // Tạo admin claims
        var adminClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, UserType.SuperUser.ToString())
        };
        var adminIdentity = new ClaimsIdentity(adminClaims, "Test");
        _adminPrincipal = new ClaimsPrincipal(adminIdentity);
    }
    
    [Test]
    public async Task GetProfile_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var userDto = new UserDto
        {
            UserId = 2,
            Username = "user",
            Email = "user@example.com",
            FullName = "Normal User",
            UserType = UserType.NormalUser,
            IsActive = true
        };
        
        _currentUserServiceMock.Setup(s => s.UserId).Returns(2);
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<UserDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as UserResponseDto;
        returnedResponse?.Success.Should().BeTrue();
        returnedResponse?.User?.Username.Should().Be("user");
    }
    
    [Test]
    public async Task UpdateProfile_WithValidData_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updateProfileDto = new UpdateUserProfileDto
        {
            Email = "updated.user@example.com",
            FullName = "Updated User Name"
        };
        
        _currentUserServiceMock.Setup(s => s.UserId).Returns(2);
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateProfile(updateProfileDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as UserResponseDto;
        returnedResponse?.Success.Should().BeTrue();
        returnedResponse?.Message.Should().Contain("Profile updated successfully");
    }
    
    [Test]
    public async Task UpdatePassword_WithValidData_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = "User@123",
            NewPassword = "NewPassword@123",
            ConfirmPassword = "NewPassword@123"
        };
        
        _currentUserServiceMock.Setup(s => s.UserId).Returns(2);
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdatePassword(updatePasswordDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as UserResponseDto;
        returnedResponse?.Success.Should().BeTrue();
        returnedResponse?.Message.Should().Contain("Password updated successfully");
    }
    
    [Test]
    public async Task UpdatePassword_WithIncorrectCurrentPassword_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewPassword@123",
            ConfirmPassword = "NewPassword@123"
        };
        
        _currentUserServiceMock.Setup(s => s.UserId).Returns(2);
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdatePassword(updatePasswordDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        
        var returnedResponse = badRequestResult?.Value as UserResponseDto;
        returnedResponse?.Success.Should().BeFalse();
    }
    
    [Test]
    public async Task UpdatePassword_WithMismatchConfirmPassword_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = "User@123",
            NewPassword = "NewPassword@123",
            ConfirmPassword = "DifferentPassword@123" // Không khớp
        };
        
        // Không cần setup mock vì validation xảy ra trước khi gọi mediator

        // Act
        var result = await _controller.UpdatePassword(updatePasswordDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
} 