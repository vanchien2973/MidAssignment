using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.UnitTests.Domain.Entities
{
    [TestFixture]
    public class CategoryTests
    {
        [Test]
        public void Category_PropertiesInitialization_PropertiesHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var category = new Category();
            
            // Assert
            Assert.That(category.CategoryId, Is.EqualTo(Guid.Empty));
            Assert.That(category.CategoryName, Is.Null);
            Assert.That(category.Description, Is.Null);
            Assert.That(category.CreatedDate, Is.EqualTo(default(DateTime)));
            Assert.That(category.Books, Is.Null);
        }
        
        [Test]
        public void Category_SetProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var category = new Category();
            var categoryId = Guid.NewGuid();
            var categoryName = "Programming";
            var description = "Books about computer programming";
            var createdDate = DateTime.UtcNow;
            
            // Act
            category.CategoryId = categoryId;
            category.CategoryName = categoryName;
            category.Description = description;
            category.CreatedDate = createdDate;
            
            // Assert
            Assert.That(category.CategoryId, Is.EqualTo(categoryId));
            Assert.That(category.CategoryName, Is.EqualTo(categoryName));
            Assert.That(category.Description, Is.EqualTo(description));
            Assert.That(category.CreatedDate, Is.EqualTo(createdDate));
        }
        
        [Test]
        public void Category_NavigationProperties_InitializedAsNull()
        {
            // Arrange & Act
            var category = new Category();
            
            // Assert
            Assert.That(category.Books, Is.Null);
        }
        
        [Test]
        public void Category_SetNavigationProperties_PropertiesAreSetCorrectly()
        {
            // Arrange
            var category = new Category();
            var books = new List<Book>
            {
                new Book { BookId = Guid.NewGuid(), Title = "Clean Code" },
                new Book { BookId = Guid.NewGuid(), Title = "The Pragmatic Programmer" }
            };
            
            // Act
            category.Books = books;
            
            // Assert
            Assert.That(category.Books, Is.SameAs(books));
            Assert.That(category.Books.Count, Is.EqualTo(2));
        }
    }
} 