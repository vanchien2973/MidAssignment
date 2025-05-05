using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Interfaces.Repositories;
using LibraryManagementSystem.Application.Mappers;
using LibraryManagementSystem.Application.Queries.User;
using MediatR;

namespace LibraryManagementSystem.Application.Handlers.QueryHandlers.User;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedResponseDto<UserDto>>
{
    private readonly IUserRepository _userRepository;
    
    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<PaginatedResponseDto<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetUsersAsync(
            request.PageNumber, 
            request.PageSize, 
            request.SearchTerm);
        
        var totalCount = await _userRepository.CountBySearchTermAsync(request.SearchTerm);

        return new PaginatedResponseDto<UserDto>
        {
            Data = users.ToDtos(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
} 