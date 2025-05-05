namespace LibraryManagementSystem.Application.DTOs.Book
{
    public class BookQueryParametersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? AvailableOnly { get; set; }
    }
} 