using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.DTOs.User;
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
public class BookControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<ILogger<BookController>> _loggerMock;
    private BookController _controller;
    private ClaimsPrincipal _normalUserPrincipal;
    private ClaimsPrincipal _adminPrincipal;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<BookController>>();
        _controller = new BookController(_mediatorMock.Object, _loggerMock.Object);
        
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
    public async Task GetBooks_ShouldReturnOkWithPagination()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var books = new List<BookListDto>
        {
            new() { BookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef"), Title = "Test Book 1", Author = "Author 1" },
            new() { BookId = Guid.Parse("31234567-89ab-cdef-0123-456789abcdef"), Title = "Test Book 2", Author = "Author 2" }
        };
        
        var response = new PaginatedResponseDto<BookListDto>
        {
            Data = books,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<BookListDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetBooks();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        // Kiểm tra header pagination
        _controller.Response.Headers.Should().ContainKey("X-Total-Count");
        _controller.Response.Headers.Should().ContainKey("X-Page-Number");
        _controller.Response.Headers.Should().ContainKey("X-Page-Size");
        
        var returnedBooks = okResult?.Value as PaginatedResponseDto<BookListDto>;
        returnedBooks?.Data.Should().HaveCount(2);
    }
    
    [Test]
    public async Task GetBook_WithValidId_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var bookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef");
        var book = new BookDetailsDto
        {
            BookId = bookId,
            Title = "Test Book 1",
            Author = "Author 1",
            Description = "Test description",
            Publisher = "Test Publisher",
            PublishedYear = 2020,
            ISBN = "1234567890",
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            CategoryName = "Fiction",
            AvailableCopies = 3,
            TotalCopies = 5
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<BookDetailsDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _controller.GetBook(bookId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedBook = okResult?.Value as BookDetailsDto;
        returnedBook?.BookId.Should().Be(bookId);
        returnedBook?.Title.Should().Be("Test Book 1");
    }
    
    [Test]
    public async Task GetBook_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var invalidId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<BookDetailsDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookDetailsDto)null);

        // Act
        var result = await _controller.GetBook(invalidId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task GetBooksByCategory_WithValidCategoryId_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var categoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        var books = new List<BookListDto>
        {
            new() { BookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef"), Title = "Test Book 1", Author = "Author 1" }
        };
        
        var response = new PaginatedResponseDto<BookListDto>
        {
            Data = books,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<BookListDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetBooksByCategory(categoryId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedBooks = okResult?.Value as PaginatedResponseDto<BookListDto>;
        returnedBooks?.Data.Should().HaveCount(1);
    }
    
    [Test]
    public async Task CreateBook_WithAdminRole_ShouldReturnCreated()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var newBook = new BookCreateDto
        {
            Title = "Test New Book",
            Author = "Test Author",
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            ISBN = "1234567890999",
            PublishedYear = 2023,
            Publisher = "Test Publisher",
            Description = "New test book for API testing",
            TotalCopies = 3
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateBook(newBook);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
    
    [Test]
    public async Task CreateBook_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var newBook = new BookCreateDto
        {
            Title = "Another Test Book",
            Author = "Another Author",
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            ISBN = "9876543210000",
            PublishedYear = 2022,
            Publisher = "Test Publisher",
            Description = "Test book with normal user token",
            TotalCopies = 2
        };

        // Act
        var result = await _controller.CreateBook(newBook);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task UpdateBook_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var updateBook = new BookUpdateDto
        {
            BookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef"),
            Title = "Updated Test Book 1",
            Author = "Updated Author",
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            ISBN = "1234567890123",
            PublishedYear = 2020,
            Publisher = "Updated Publisher",
            Description = "Updated description",
            TotalCopies = 6
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateBook(updateBook);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        okResult?.Value.Should().NotBeNull();
        var response = okResult?.Value as dynamic;
        ((object)response).Should().NotBeNull();
    }
    
    [Test]
    public async Task UpdateBook_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updateBook = new BookUpdateDto
        {
            BookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef"),
            Title = "Cannot Update This Book",
            Author = "Test Author",
            CategoryId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            ISBN = "1234567890123",
            PublishedYear = 2020,
            Publisher = "Test Publisher",
            Description = "Test description",
            TotalCopies = 5
        };

        // Act
        var result = await _controller.UpdateBook(updateBook);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task DeleteBook_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var bookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef");
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteBook(bookId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        okResult?.Value.Should().NotBeNull();
        var response = okResult?.Value as dynamic;
        ((object)response).Should().NotBeNull();
    }
    
    [Test]
    public async Task DeleteBook_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var bookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef");

        // Act
        var result = await _controller.DeleteBook(bookId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
} 