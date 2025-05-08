using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Queries.User;
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
public class AdminControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<ILogger<AdminController>> _loggerMock;
    private AdminController _controller;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AdminController>>();
        _controller = new AdminController(_mediatorMock.Object, _loggerMock.Object);
        
        // Thiết lập HttpContext để mô phỏng authenticated request
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Test]
    public async Task GetAllUsers_ShouldReturnOkResult()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new() { UserId = 1, Username = "admin", UserType = UserType.SuperUser },
            new() { UserId = 2, Username = "user", UserType = UserType.NormalUser }
        };
        
        var response = new PaginatedResponseDto<UserDto>
        {
            Data = users,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<UserDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAllUsers(new UserSearchDto());

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnValue = okResult?.Value as PaginatedResponseDto<UserDto>;
        returnValue?.Data.Should().HaveCount(2);
    }

    [Test]
    public async Task GetUserById_WithExistingUserId_ShouldReturnOkResult()
    {
        // Arrange
        var userDto = new UserDto
        {
            UserId = 1,
            Username = "admin",
            UserType = UserType.SuperUser,
            IsActive = true
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<UserDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUserById(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as UserResponseDto;
        returnedResponse?.Success.Should().BeTrue();
        returnedResponse?.User?.Username.Should().Be("admin");
    }

    [Test]
    public async Task GetUserById_WithNonExistingUserId_ShouldReturnNotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<User>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _controller.GetUserById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateUserRole_WithValidData_ShouldReturnOkResult()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateUserRole(2, new UpdateUserRoleDto { UserType = UserType.NormalUser });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Test]
    public async Task ActivateUser_WithValidUserId_ShouldReturnOkResult()
    {
        // Arrange  
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateUser(3);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Test]
    public async Task DeactivateUser_WithSuperUserAccount_ShouldReturnNotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeactivateUser(1);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
} 