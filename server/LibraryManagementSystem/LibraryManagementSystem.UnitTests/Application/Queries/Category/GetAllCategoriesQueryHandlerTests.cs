using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetAllCategoriesQueryHandlerTests
    {
        private Mock<ICategoryRepository> _mockCategoryRepository;
        private GetAllCategoriesQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new GetAllCategoriesQueryHandler(_mockCategoryRepository.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnCategoryListDtos()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var sortBy = "CategoryName";
            var sortOrder = "asc";
            var searchTerm = "test";
            
            var categories = new List<Domain.Entities.Category>
            {
                new Domain.Entities.Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Test Category 1",
                    Description = "Test Description 1"
                },
                new Domain.Entities.Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Test Category 2",
                    Description = "Test Description 2"
                },
                new Domain.Entities.Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Test Category 3",
                    Description = "Test Description 3"
                }
            };
            
            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                    pageNumber, 
                    pageSize, 
                    sortBy, 
                    sortOrder, 
                    searchTerm))
                .ReturnsAsync(categories);
            
            var query = new GetAllCategoriesQuery(pageNumber, pageSize, sortBy, sortOrder, searchTerm);
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(3));
            
            var resultList = result.ToList();
            for (int i = 0; i < categories.Count; i++)
            {
                Assert.That(resultList[i].CategoryId, Is.EqualTo(categories[i].CategoryId));
                Assert.That(resultList[i].CategoryName, Is.EqualTo(categories[i].CategoryName));
            }
        }

        [Test]
        public async Task Handle_WithDefaultParameters_ShouldUseDefaultValues()
        {
            // Arrange
            var defaultPageNumber = 1;
            var defaultPageSize = 10;
            
            var categories = new List<Domain.Entities.Category>
            {
                new Domain.Entities.Category
                {
                    CategoryId = Guid.NewGuid(),
                    CategoryName = "Test Category",
                    Description = "Test Description"
                }
            };
            
            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                    defaultPageNumber, 
                    defaultPageSize, 
                    null, 
                    null, 
                    null))
                .ReturnsAsync(categories);
            
            var query = new GetAllCategoriesQuery(); // Using default values
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            
            _mockCategoryRepository.Verify(repo => repo.GetAllAsync(
                defaultPageNumber, 
                defaultPageSize, 
                null, 
                null, 
                null), Times.Once);
        }

        [Test]
        public async Task Handle_WithEmptyCategories_ShouldReturnEmptyList()
        {
            // Arrange
            var categories = new List<Domain.Entities.Category>();
            
            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(
                    It.IsAny<int>(), 
                    It.IsAny<int>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>()))
                .ReturnsAsync(categories);
            
            var query = new GetAllCategoriesQuery();
            
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Any(), Is.False);
        }
    }
} 