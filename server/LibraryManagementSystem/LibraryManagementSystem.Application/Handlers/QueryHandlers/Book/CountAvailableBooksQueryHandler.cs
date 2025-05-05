using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.Book
{
    public class CountAvailableBooksQueryHandler : IRequestHandler<CountAvailableBooksQuery, int>
    {
        private readonly IBookRepository _bookRepository;

        public CountAvailableBooksQueryHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<int> Handle(CountAvailableBooksQuery request, CancellationToken cancellationToken)
        {
            return await _bookRepository.CountAvailableBooksAsync();
        }
    }
} 