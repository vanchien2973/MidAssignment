using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.Book
{
    public class CountBooksQueryHandler : IRequestHandler<CountBooksQuery, int>
    {
        private readonly IBookRepository _bookRepository;

        public CountBooksQueryHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<int> Handle(CountBooksQuery request, CancellationToken cancellationToken)
        {
            return await _bookRepository.CountAsync();
        }
    }
} 