using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Category
{
    [TestFixture]
    public class DeleteCategoryCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<IBookRepository> _mockBookRepository;
        private Mock<ILogger<DeleteCategoryCommandHandler>> _mockLogger;
        private DeleteCategoryCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockBookRepository = new Mock<IBookRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Categories).Returns(_mockCategoryRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Books).Returns(_mockBookRepository.Object);
            
            _mockLogger = new Mock<ILogger<DeleteCategoryCommandHandler>>();
            
            _handler = new DeleteCategoryCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenCategoryExistsAndHasNoBooks_ShouldDeleteCategoryAndReturnTrue()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand
            {
                CategoryId = categoryId
            };

            var existingCategory = new Domain.Entities.Category
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Description = "Test Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);
            
            _mockBookRepository.Setup(repo => repo.HasBooksInCategoryAsync(categoryId))
                .ReturnsAsync(false);
            
            _mockCategoryRepository.Setup(repo => repo.DeleteAsync(categoryId))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockBookRepository.Verify(repo => repo.HasBooksInCategoryAsync(categoryId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(categoryId), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenCategoryDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand
            {
                CategoryId = categoryId
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Domain.Entities.Category)null);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockBookRepository.Verify(repo => repo.HasBooksInCategoryAsync(categoryId), Times.Never);
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(categoryId), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenCategoryHasBooks_ShouldReturnFalse()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand
            {
                CategoryId = categoryId
            };

            var existingCategory = new Domain.Entities.Category
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Description = "Test Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);
            
            _mockBookRepository.Setup(repo => repo.HasBooksInCategoryAsync(categoryId))
                .ReturnsAsync(true);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
            _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockBookRepository.Verify(repo => repo.HasBooksInCategoryAsync(categoryId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(categoryId), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldRollbackTransactionAndReturnFalse()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand
            {
                CategoryId = categoryId
            };

            var existingCategory = new Domain.Entities.Category
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Description = "Test Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);
            
            _mockBookRepository.Setup(repo => repo.HasBooksInCategoryAsync(categoryId))
                .ReturnsAsync(false);
            
            _mockCategoryRepository.Setup(repo => repo.DeleteAsync(categoryId))
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