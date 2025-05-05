using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.Book
{
    public class CountBooksByCategoryQueryHandler : IRequestHandler<CountBooksByCategoryQuery, int>
    {
        private readonly IBookRepository _bookRepository;

        public CountBooksByCategoryQueryHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<int> Handle(CountBooksByCategoryQuery request, CancellationToken cancellationToken)
        {
            return await _bookRepository.CountByCategoryAsync(request.CategoryId);
        }
    }
} 