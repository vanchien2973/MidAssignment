using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.Book
{
    public class GetBooksByCategoryQueryHandler : IRequestHandler<GetBooksByCategoryQuery, IEnumerable<BookListDto>>
    {
        private readonly IBookRepository _bookRepository;

        public GetBooksByCategoryQueryHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<BookListDto>> Handle(GetBooksByCategoryQuery request, CancellationToken cancellationToken)
        {
            var books = await _bookRepository.GetByCategoryIdAsync(
                request.CategoryId, 
                request.PageNumber, 
                request.PageSize);
                
            return BookMapper.ToBookListDTOs(books);
        }
    }
} 