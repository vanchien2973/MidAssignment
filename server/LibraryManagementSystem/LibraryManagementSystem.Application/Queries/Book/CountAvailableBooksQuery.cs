using MediatR;

namespace LibraryManagementSystem.Application.Queries.Book
{
    public class CountAvailableBooksQuery : IRequest<int>
    {
    }
} 