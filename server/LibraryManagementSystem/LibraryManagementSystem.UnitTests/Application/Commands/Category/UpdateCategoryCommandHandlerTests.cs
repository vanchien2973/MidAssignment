using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Handlers.CommandHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Commands.Category
{
    [TestFixture]
    public class UpdateCategoryCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private Mock<ILogger<UpdateCategoryCommandHandler>> _mockLogger;
        private UpdateCategoryCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Categories).Returns(_mockCategoryRepository.Object);
            
            _mockLogger = new Mock<ILogger<UpdateCategoryCommandHandler>>();
            
            _handler = new UpdateCategoryCommandHandler(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_WhenCategoryExists_ShouldUpdateCategoryAndReturnTrue()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new UpdateCategoryCommand
            {
                CategoryId = categoryId,
                CategoryName = "Updated Category",
                Description = "Updated Description"
            };

            var existingCategory = new Domain.Entities.Category
            {
                CategoryId = categoryId,
                CategoryName = "Original Category",
                Description = "Original Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);
            
            _mockCategoryRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Category>()))
                .Returns(Task.CompletedTask);
            
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CommitTransactionAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
            _mockCategoryRepository.Verify(repo => repo.GetByIdAsync(categoryId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.UpdateAsync(It.Is<Domain.Entities.Category>(c => 
                c.CategoryId == categoryId && 
                c.CategoryName == command.CategoryName && 
                c.Description == command.Description)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Never);
        }

        [Test]
        public async Task Handle_WhenCategoryDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new UpdateCategoryCommand
            {
                CategoryId = categoryId,
                CategoryName = "Updated Category",
                Description = "Updated Description"
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
            _mockCategoryRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Category>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WhenExceptionThrown_ShouldRollbackTransactionAndReturnFalse()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new UpdateCategoryCommand
            {
                CategoryId = categoryId,
                CategoryName = "Updated Category",
                Description = "Updated Description"
            };

            var existingCategory = new Domain.Entities.Category
            {
                CategoryId = categoryId,
                CategoryName = "Original Category",
                Description = "Original Description"
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);
            
            _mockCategoryRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Category>()))
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