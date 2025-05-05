using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Book
{
    [TestFixture]
    public class CreateBookCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<ILogger<CreateBookCommandHandler>> _mockLogger;
        private CreateBookCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockBookRepository = new Mock<IBookRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);
            
            _mockLogger = new Mock<ILogger<CreateBookCommandHandler>>();
            
            _handler = new CreateBookCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenValidRequest_ShouldCreateBookAndReturnTrue()
        {
            // Arrange
            var command = new CreateBookCommand
            {
                Title = "Test Book",
                Author = "Test Author",
                CategoryId = Guid.NewGuid(),
                ISBN = "1234567890",
                PublishedYear = 2023,
                Publisher = "Test Publisher",
                Description = "Test Description",
                TotalCopies = 10
            };

            _mockBookRepository.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Book>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockBookRepository.Verify(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Book>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldRollbackTransactionAndReturnFalse()
        {
            // Arrange
            var command = new CreateBookCommand
            {
                Title = "Test Book",
                Author = "Test Author",
                CategoryId = Guid.NewGuid(),
                ISBN = "1234567890",
                PublishedYear = 2023,
                Publisher = "Test Publisher",
                Description = "Test Description",
                TotalCopies = 10
            };

            _mockBookRepository.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Book>()))
                .ThrowsAsync(new Exception("Database error"));
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }
    }
} 