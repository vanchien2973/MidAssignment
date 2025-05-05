using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Category
{
    [TestFixture]
    public class CreateCategoryCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<ILogger<CreateCategoryCommandHandler>> _mockLogger;
        private CreateCategoryCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Categories).Returns(_mockCategoryRepository.Object);
            
            _mockLogger = new Mock<ILogger<CreateCategoryCommandHandler>>();
            
            _handler = new CreateCategoryCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WithValidRequest_ShouldCreateCategoryAndReturnTrue()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                CategoryName = "Test Category",
                Description = "Test Description"
            };

            _mockCategoryRepository.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Category>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockCategoryRepository.Verify(repo => repo.CreateAsync(It.Is<Domain.Entities.Category>(c => 
                c.CategoryName == command.CategoryName && 
                c.Description == command.Description)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldRollbackTransactionAndReturnFalse()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                CategoryName = "Test Category",
                Description = "Test Description"
            };

            _mockCategoryRepository.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.Category>()))
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