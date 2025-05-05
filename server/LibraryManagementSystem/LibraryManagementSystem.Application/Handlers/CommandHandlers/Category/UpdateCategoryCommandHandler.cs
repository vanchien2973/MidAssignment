using LibraryManagementSystem.Application.Commands.Category;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Application.Handlers.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCategoryCommandHandler> _logger;

        public UpdateCategoryCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
                
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found for update", request.CategoryId);
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                CategoryMapper.UpdateFrom(category, request);

                await _unitOfWork.Categories.UpdateAsync(category);
                
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Category {CategoryId} updated", category.CategoryId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", request.CategoryId);
                
                await _unitOfWork.RollbackTransactionAsync();
                
                return false;
            }
        }
    }
} 