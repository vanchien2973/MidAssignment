using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Queries.Category;
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
public class CategoryControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<ILogger<CategoryController>> _loggerMock;
    private CategoryController _controller;
    private ClaimsPrincipal _normalUserPrincipal;
    private ClaimsPrincipal _adminPrincipal;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CategoryController>>();
        _controller = new CategoryController(_mediatorMock.Object, _loggerMock.Object);
        
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
    public async Task GetCategories_ShouldReturnOkWithPagination()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var categories = new List<CategoryListDto>
        {
            new() { CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"), CategoryName = "Fiction" },
            new() { CategoryId = Guid.Parse("11234567-89ab-cdef-0123-456789abcdef"), CategoryName = "Non-fiction" }
        };
        
        var response = new PaginatedResponseDto<CategoryListDto>
        {
            Data = categories,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<CategoryListDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        // Kiểm tra header pagination
        _controller.Response.Headers.Should().ContainKey("X-Total-Count");
        _controller.Response.Headers.Should().ContainKey("X-Page-Number");
        _controller.Response.Headers.Should().ContainKey("X-Page-Size");
        
        var returnedCategories = okResult?.Value as PaginatedResponseDto<CategoryListDto>;
        returnedCategories?.Data.Should().HaveCount(2);
    }
    
    [Test]
    public async Task GetCategory_WithValidId_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var categoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        var category = new CategoryDetailsDto
        {
            CategoryId = categoryId,
            CategoryName = "Fiction",
            Description = "Fiction books"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<CategoryDetailsDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetCategory(categoryId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedCategory = okResult?.Value as CategoryDetailsDto;
        returnedCategory?.CategoryId.Should().Be(categoryId);
        returnedCategory?.CategoryName.Should().Be("Fiction");
    }
    
    [Test]
    public async Task GetCategory_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var invalidId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<CategoryDetailsDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDetailsDto)null);

        // Act
        var result = await _controller.GetCategory(invalidId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task CreateCategory_WithAdminRole_ShouldReturnCreated()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var newCategory = new CategoryCreateDto
        {
            CategoryName = "Test Category",
            Description = "Test category description"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateCategory(newCategory);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
    
    [Test]
    public async Task CreateCategory_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var newCategory = new CategoryCreateDto
        {
            CategoryName = "Another Test Category",
            Description = "Another test category description"
        };

        // Act
        var result = await _controller.CreateCategory(newCategory);

        // Assert
        // Kết quả có thể là ForbidResult hoặc UnauthorizedResult, tùy thuộc vào cài đặt bảo mật
        (result is ForbidResult || result is UnauthorizedResult).Should().BeTrue();
    }
    
    [Test]
    public async Task UpdateCategory_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var updateCategory = new CategoryUpdateDto
        {
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            CategoryName = "Updated Fiction",
            Description = "Updated fiction category description"
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateCategory(updateCategory);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        okResult?.Value.Should().NotBeNull();
    }
    
    [Test]
    public async Task UpdateCategory_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updateCategory = new CategoryUpdateDto
        {
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            CategoryName = "Cannot Update Category",
            Description = "This should not work with normal user"
        };

        // Act
        var result = await _controller.UpdateCategory(updateCategory);

        // Assert
        // Kết quả có thể là ForbidResult hoặc UnauthorizedResult, tùy thuộc vào cài đặt bảo mật
        (result is ForbidResult || result is UnauthorizedResult).Should().BeTrue();
    }
    
    [Test]
    public async Task DeleteCategory_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var categoryId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        okResult?.Value.Should().NotBeNull();
    }
    
    [Test]
    public async Task DeleteCategory_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var categoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        // Kết quả có thể là ForbidResult hoặc UnauthorizedResult, tùy thuộc vào cài đặt bảo mật
        (result is ForbidResult || result is UnauthorizedResult).Should().BeTrue();
    }
    
    [Test]
    public async Task DeleteCategory_WithAssociatedBooks_ShouldReturnNotFound()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var categoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        notFoundResult?.Value.ToString().Should().Contain("cannot be deleted because it has associated books");
    }
} 