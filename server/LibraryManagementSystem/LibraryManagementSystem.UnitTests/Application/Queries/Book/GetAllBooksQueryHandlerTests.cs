using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Book;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book
{
    [TestFixture]
    public class GetAllBooksQueryHandlerTests
    {
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<ILogger<GetAllBooksQueryHandler>> _mockLogger;
        private GetAllBooksQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            _mockLogger = new Mock<ILogger<GetAllBooksQueryHandler>>();
            
            _handler = new GetAllBooksQueryHandler(_mockBookRepository.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnBookListDtos()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var sortBy = "Title";
            var sortOrder = "asc";
            
            var books = new List<Domain.Entities.Book>
            {
                new Domain.Entities.Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Test Book 1",
                    Author = "Test Author 1",
                    ISBN = "1234567890",
                    PublishedYear = 2023,
                    TotalCopies = 10,
                    AvailableCopies = 5
                },
                new Domain.Entities.Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Test Book 2",
                    Author = "Test Author 2",
                    ISBN = "0987654321",
                    PublishedYear = 2022,
                    TotalCopies = 8,
                    AvailableCopies = 3
                }
            };
            
            _mockBookRepository.Setup(repo => repo.GetAllAsync(
                    pageNumber, 
                    pageSize, 
                    sortBy, 
                    sortOrder))
                .ReturnsAsync(books);
            
            var query = new GetAllBooksQuery(pageNumber, pageSize, sortBy, sortOrder);
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            
            var resultList = result.ToList();
            for (int i = 0; i < books.Count; i++)
            {
                Assert.That(resultList[i].BookId, Is.EqualTo(books[i].BookId));
                Assert.That(resultList[i].Title, Is.EqualTo(books[i].Title));
                Assert.That(resultList[i].Author, Is.EqualTo(books[i].Author));
                Assert.That(resultList[i].ISBN, Is.EqualTo(books[i].ISBN));
                Assert.That(resultList[i].PublishedYear, Is.EqualTo(books[i].PublishedYear));
            }
        }

        [Test]
        public async Task Handle_WithDefaultParameters_ShouldUseDefaultValues()
        {
            // Arrange
            var defaultPageNumber = 1;
            var defaultPageSize = 10;
            
            var books = new List<Domain.Entities.Book>
            {
                new Domain.Entities.Book
                {
                    BookId = Guid.NewGuid(),
                    Title = "Test Book",
                    Author = "Test Author"
                }
            };
            
            _mockBookRepository.Setup(repo => repo.GetAllAsync(
                    defaultPageNumber, 
                    defaultPageSize, 
                    null, 
                    null))
                .ReturnsAsync(books);
            
            var query = new GetAllBooksQuery(); // Using default values
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            
            _mockBookRepository.Verify(repo => repo.GetAllAsync(
                defaultPageNumber, 
                defaultPageSize, 
                null, 
                null), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetAllAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            
            var query = new GetAllBooksQuery();
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.False);
            
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
} 