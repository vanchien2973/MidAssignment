using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.DTOs.Borrowing;
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
public class BorrowingControllerTests
{
    private Mock<IMediator> _mediatorMock;
    private Mock<ILogger<BorrowingController>> _loggerMock;
    private BorrowingController _controller;
    private ClaimsPrincipal _normalUserPrincipal;
    private ClaimsPrincipal _adminPrincipal;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<BorrowingController>>();
        _controller = new BorrowingController(_mediatorMock.Object, _loggerMock.Object);
        
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
    public async Task GetBorrowingRequest_WithValidId_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var requestId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef");
        var borrowingRequest = new BookBorrowingRequestDto
        {
            RequestId = requestId,
            RequestorId = 2,
            Status = BorrowingRequestStatus.Waiting,
            RequestDate = DateTime.Now.AddDays(-1),
            RequestDetails = new List<BookBorrowingRequestDetailDto>
            {
                new() { BookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef"), BookTitle = "Test Book 1" }
            }
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<BookBorrowingRequestDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(borrowingRequest);

        // Act
        var result = await _controller.GetBorrowingRequest(requestId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedRequest = okResult?.Value as BookBorrowingRequestDto;
        returnedRequest?.RequestId.Should().Be(requestId);
    }
    
    [Test]
    public async Task GetBorrowingRequest_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var invalidId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<BookBorrowingRequestDto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookBorrowingRequestDto)null);

        // Act
        var result = await _controller.GetBorrowingRequest(invalidId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
    
    [Test]
    public async Task GetUserBorrowingRequests_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var userId = 2;
        var requests = new List<BookBorrowingRequestDto>
        {
            new() { RequestId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef"), Status = BorrowingRequestStatus.Waiting }
        };
        
        var response = new PaginatedResponseDto<BookBorrowingRequestDto>
        {
            Data = requests,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<BookBorrowingRequestDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetUserBorrowingRequests(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as PaginatedResponseDto<BookBorrowingRequestDto>;
        returnedResponse?.Data.Should().HaveCount(1);
    }
    
    [Test]
    public async Task GetPendingBorrowingRequests_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var requests = new List<BookBorrowingRequestDto>
        {
            new() { RequestId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef"), Status = BorrowingRequestStatus.Waiting }
        };
        
        var response = new PaginatedResponseDto<BookBorrowingRequestDto>
        {
            Data = requests,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<BookBorrowingRequestDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetPendingBorrowingRequests();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as PaginatedResponseDto<BookBorrowingRequestDto>;
        returnedResponse?.Data.Should().HaveCount(1);
    }
    
    [Test]
    public async Task GetPendingBorrowingRequests_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;

        // Act
        var result = await _controller.GetPendingBorrowingRequests();

        // Assert
        // Kết quả có thể là ForbidResult hoặc UnauthorizedResult, tùy thuộc vào cài đặt bảo mật
        (result is ForbidResult || result is UnauthorizedResult).Should().BeTrue();
    }
    
    [Test]
    public async Task GetAllBorrowingRequests_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var requests = new List<BookBorrowingRequestDto>
        {
            new() { RequestId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef"), Status = BorrowingRequestStatus.Waiting },
            new() { RequestId = Guid.Parse("51234567-89ab-cdef-0123-456789abcdef"), Status = BorrowingRequestStatus.Approved }
        };
        
        var response = new PaginatedResponseDto<BookBorrowingRequestDto>
        {
            Data = requests,
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<PaginatedResponseDto<BookBorrowingRequestDto>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAllBorrowingRequests();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        var returnedResponse = okResult?.Value as PaginatedResponseDto<BookBorrowingRequestDto>;
        returnedResponse?.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }
    
    [Test]
    public async Task CreateBorrowingRequest_ShouldReturnCreated()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var borrowingRequest = new CreateBorrowingRequestDto
        {
            RequestorId = 2,
            Books = new List<BorrowingBookItemDto>
            {
                new() { BookId = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef") }
            }
        };
        
        var requestId = Guid.NewGuid();
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestId);

        // Act
        var result = await _controller.CreateBorrowingRequest(borrowingRequest);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult?.StatusCode.Should().Be((int)HttpStatusCode.Created);
    }
    
    [Test]
    public async Task UpdateBorrowingRequestStatus_WithAdminRole_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _adminPrincipal;
        
        var requestId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef");
        var updateStatusDto = new BorrowingRequestStatusUpdateDto
        {
            RequestId = requestId,
            Status = BorrowingRequestStatus.Approved,
            ApproverId = 1
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateBorrowingRequestStatus(updateStatusDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        okResult?.Value.Should().NotBeNull();
    }
    
    [Test]
    public async Task UpdateBorrowingRequestStatus_WithNormalUserRole_ShouldReturnForbidden()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var updateStatusDto = new BorrowingRequestStatusUpdateDto
        {
            RequestId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef"),
            Status = BorrowingRequestStatus.Rejected,
            ApproverId = 2
        };

        // Act
        var result = await _controller.UpdateBorrowingRequestStatus(updateStatusDto);

        // Assert
        // Kết quả có thể là ForbidResult hoặc UnauthorizedResult, tùy thuộc vào cài đặt bảo mật
        (result is ForbidResult || result is UnauthorizedResult).Should().BeTrue();
    }
    
    [Test]
    public async Task ReturnBook_WithValidData_ShouldReturnOk()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = _normalUserPrincipal;
        
        var detailId = Guid.Parse("51234567-89ab-cdef-0123-456789abcdef");
        var returnBookDto = new ReturnBookDto
        {
            DetailId = detailId
        };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ReturnBook(returnBookDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
        
        okResult?.Value.Should().NotBeNull();
    }
} 