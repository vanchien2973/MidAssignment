using LibraryManagementSystem.Application.DTOs.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.Book;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.Book
{
    public class GetAvailableBooksQueryHandler : IRequestHandler<GetAvailableBooksQuery, IEnumerable<BookListDto>>
    {
        private readonly IBookRepository _bookRepository;

        public GetAvailableBooksQueryHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<BookListDto>> Handle(GetAvailableBooksQuery request, CancellationToken cancellationToken)
        {
            var books = await _bookRepository.GetAvailableBooksAsync(
                request.PageNumber, 
                request.PageSize);
                
            return BookMapper.ToBookListDTOs(books);
        }
    }
} 