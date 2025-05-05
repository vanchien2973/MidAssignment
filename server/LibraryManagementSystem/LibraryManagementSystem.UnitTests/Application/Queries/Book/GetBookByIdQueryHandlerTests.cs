using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Book;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Book
{
    [TestFixture]
    public class GetBookByIdQueryHandlerTests
    {
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<ILogger<GetBookByIdQueryHandler>> _mockLogger;
        private GetBookByIdQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            _mockLogger = new Mock<ILogger<GetBookByIdQueryHandler>>();
            
            _handler = new GetBookByIdQueryHandler(_mockBookRepository.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenBookExists_ShouldReturnBookDetailsDto()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            
            var book = new Domain.Entities.Book
            {
                BookId = bookId,
                Title = "Test Book",
                Author = "Test Author",
                CategoryId = categoryId,
                ISBN = "1234567890",
                PublishedYear = 2023,
                Publisher = "Test Publisher",
                Description = "Test Description",
                TotalCopies = 10,
                AvailableCopies = 5,
                IsActive = true
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync(book);

            var query = new GetBookByIdQuery(bookId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BookId, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo(book.Title));
            Assert.That(result.Author, Is.EqualTo(book.Author));
            Assert.That(result.CategoryId, Is.EqualTo(book.CategoryId));
            Assert.That(result.ISBN, Is.EqualTo(book.ISBN));
            Assert.That(result.PublishedYear, Is.EqualTo(book.PublishedYear));
            Assert.That(result.Publisher, Is.EqualTo(book.Publisher));
            Assert.That(result.Description, Is.EqualTo(book.Description));
            Assert.That(result.TotalCopies, Is.EqualTo(book.TotalCopies));
            Assert.That(result.AvailableCopies, Is.EqualTo(book.AvailableCopies));
        }

        [Test]
        public async Task Handle_WhenBookDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ReturnsAsync((Domain.Entities.Book)null);

            var query = new GetBookByIdQuery(bookId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldLogErrorAndReturnNull()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetBookByIdQuery(bookId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
} 