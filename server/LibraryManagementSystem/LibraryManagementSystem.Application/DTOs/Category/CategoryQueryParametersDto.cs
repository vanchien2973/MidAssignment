namespace LibraryManagementSystem.Application.DTOs.Category
{
    public class CategoryQueryParametersDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string SearchTerm { get; set; }
    }
} 