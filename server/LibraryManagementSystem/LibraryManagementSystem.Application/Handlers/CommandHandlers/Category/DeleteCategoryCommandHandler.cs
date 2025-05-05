using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;

        public DeleteCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {   
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
                
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for deletion", request.CategoryId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                // Check if category is used by any books
                var hasBooks = await _unitOfWork.Books.HasBooksInCategoryAsync(request.CategoryId);
                if (hasBooks)
                {
                    _logger.LogWarning("Cannot delete category {CategoryId} as it has associated books", request.CategoryId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                await _unitOfWork.Categories.DeleteAsync(request.CategoryId);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Category {CategoryId} deleted", request.CategoryId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", request.CategoryId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 