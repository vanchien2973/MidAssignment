using LibraryManagementSystem.Application.DTOs.User;
using MediatR;

namespace LibraryManagementSystem.Application.Queries.User;

public class GetAllUsersQuery : IRequest<PaginatedResponseDto<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; } = string.Empty;
} 