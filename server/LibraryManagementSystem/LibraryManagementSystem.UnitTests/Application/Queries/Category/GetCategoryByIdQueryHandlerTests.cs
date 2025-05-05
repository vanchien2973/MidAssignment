using System;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Application.DTOs.Category;
using LibraryManagementSystem.Application.Handlers.QueryHandlers.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Category;
using LibraryManagementSystem.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace LibraryManagementSystem.UnitTests.Application.Queries.Category
{
    [TestFixture]
    public class GetCategoryByIdQueryHandlerTests
    {
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private GetCategoryByIdQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new GetCategoryByIdQueryHandler(_mockCategoryRepository.Object);
        }

        [Test]
        public async Task Handle_WhenCategoryExists_ShouldReturnCategoryDetailsDto()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var categoryName = "Test Category";
            var description = "Test Description";
            
            var category = new Domain.Entities.Category
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                Description = description
            };
            
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);
            
            var query = new GetCategoryByIdQuery(categoryId);
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CategoryId, Is.EqualTo(categoryId));
            Assert.That(result.CategoryName, Is.EqualTo(categoryName));
            Assert.That(result.Description, Is.EqualTo(description));
        }

        [Test]
        public async Task Handle_WhenCategoryDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Domain.Entities.Category)null);
            
            var query = new GetCategoryByIdQuery(categoryId);
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Null);
        }
    }
} 