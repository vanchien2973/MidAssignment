using LibraryManagementSystem.Application.Commands.Book;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Book
{
    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateBookCommandHandler> _logger;

        public CreateBookCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<CreateBookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(CreateBookCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var book = BookMapper.ToEntity(request);

                await _unitOfWork.Books.CreateAsync(book);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Book {BookTitle} created with ID {BookId}", 
                    book.Title, book.BookId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book {BookTitle}", request.Title);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 