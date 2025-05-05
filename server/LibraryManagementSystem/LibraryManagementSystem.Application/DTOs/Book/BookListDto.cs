using LibraryManagementSystem.Application.DTOs.Category;

namespace LibraryManagementSystem.Application.DTOs.Book
{
    public class BookListDto
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int? PublishedYear { get; set; }
        public string Publisher { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
} 