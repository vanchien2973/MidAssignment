using LibraryManagementSystem.Application.Commands.User;
using LibraryManagementSystem.Application.DTOs.User;
using LibraryManagementSystem.Application.Queries.User;
using LibraryManagementSystem.Domain.Entities;
using System.Collections.Generic;

namespace LibraryManagementSystem.Application.Mappers
{
    public static class UserMapper
    {
        // Command mappers
        public static ActivateUserCommand ToCommand(this ActivateUserDto dto)
        {
            return new ActivateUserCommand(dto.UserId);
        }

        public static DeactivateUserCommand ToCommand(this DeactivateUserDto dto)
        {
            return new DeactivateUserCommand(dto.UserId);
        }

        public static DeleteUserCommand ToCommand(this DeleteUserDto dto)
        {
            return new DeleteUserCommand(dto.UserId);
        }
        public static UpdateUserProfileCommand ToCommand(this UpdateUserProfileDto dto)
        {
            return new UpdateUserProfileCommand
            {
                UserId = dto.UserId,
                Email = dto.Email ?? string.Empty,
                FullName = dto.FullName ?? string.Empty
            };
        }
        
        public static UpdatePasswordCommand ToCommand(this UpdatePasswordDto dto)
        {
            return new UpdatePasswordCommand
            {
                UserId = dto.UserId,
                CurrentPassword = dto.CurrentPassword,
                NewPassword = dto.NewPassword
            };
        }

        public static UpdateUserRoleCommand ToCommand(this UpdateUserRoleDto dto)
        {
            return new UpdateUserRoleCommand
            {
                UserId = dto.UserId,
                UserType = dto.UserType
            };
        }

        // Query mappers
        public static GetUserByIdQuery ToQuery(this int userId)
        {
            return new GetUserByIdQuery(userId);
        }

        public static GetAllUsersQuery ToQuery(this UserSearchDto searchDto)
        {
            return new GetAllUsersQuery
            {
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                SearchTerm = searchDto.SearchTerm ?? string.Empty
            };
        }

        public static GetUserActivityLogsQuery ToQuery(this UserActivityLogSearchDto searchDto)
        {
            return new GetUserActivityLogsQuery
            {
                UserId = searchDto.UserId,
                ActivityType = searchDto.ActivityType,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };
        }

        // DTO mappings
        public static UserDto ToDto(this User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                UserType = user.UserType,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };
        }

        public static UserActivityLogDto ToDto(this UserActivityLog log, string username = null)
        {
            if (log == null) return null;

            return new UserActivityLogDto
            {
                LogId = log.LogId,
                UserId = log.UserId,
                Username = username ?? "Unknown",
                ActivityType = log.ActivityType,
                ActivityDate = log.ActivityDate,
                Details = log.Details,
                IpAddress = log.IpAddress
            };
        }

        // Collection mappers
        public static IEnumerable<UserDto> ToDtos(this IEnumerable<User> users)
        {
            return users?.Select(u => u.ToDto()) ?? Enumerable.Empty<UserDto>();
        }

        public static IEnumerable<UserActivityLogDto> ToDtos(this IEnumerable<UserActivityLog> logs, Dictionary<int, string> usernames)
        {
            return logs?.Select(l => l.ToDto(usernames.GetValueOrDefault(l.UserId, "Unknown"))) ?? Enumerable.Empty<UserActivityLogDto>();
        }

        // Response mappers
        public static UserResponseDto ToResponseDto(this bool success, string message = null)
        {
            return new UserResponseDto
            {
                Success = success,
                Message = message ?? (success ? "Operation completed successfully" : "Operation failed")
            };
        }
        
        public static UserResponseDto ToResponseDto(this UserDto dto, bool success = true, string message = null)
        {
            return new UserResponseDto
            {
                Success = success,
                Message = message ?? "User information retrieved successfully",
                User = dto != null ? new UserResponseDto.UserDto
                {
                    UserId = dto.UserId,
                    Username = dto.Username,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    UserType = dto.UserType.ToString(),
                    IsActive = dto.IsActive,
                    CreatedDate = dto.CreatedDate,
                    LastLoginDate = dto.LastLoginDate
                } : null
            };
        }
    }
} 