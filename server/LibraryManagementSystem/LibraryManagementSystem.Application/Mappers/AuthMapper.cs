using LibraryManagementSystem.Application.Commands.Auth;
using LibraryManagementSystem.Application.DTOs.Auth;
using LibraryManagementSystem.Application.Queries.Auth;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Mappers
{
    public static class AuthMapper
    {
        public static LoginCommand ToCommand(this LoginDto dto)
        {
            return new LoginCommand
            {
                Username = dto.Username,
                Password = dto.Password
            };
        }

        public static RegisterCommand ToCommand(this RegisterDto dto)
        {
            return new RegisterCommand
            {
                Username = dto.Username,
                Password = dto.Password,
                Email = dto.Email,
                FullName = dto.FullName
            };
        }

        public static RefreshTokenCommand ToCommand(this RefreshTokenDto dto)
        {
            return new RefreshTokenCommand
            {
                Token = dto.Token,
                RefreshToken = dto.RefreshToken
            };
        }

        public static GetCurrentUserQuery ToQuery(this string userId)
        {
            return new GetCurrentUserQuery
            {
                UserId = userId
            };
        }

        public static AuthResponseDto ToAuthResponseDto(this LoginResponse response)
        {
            return new AuthResponseDto
            {
                Success = response.Success,
                Message = response.Success ? "Login successful" : "Invalid username or password",
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                User = response.User != null ? ToUserInfoDto(response.User) : null
            };
        }

        public static AuthResponseDto ToAuthResponseDto(this RegisterResponse response)
        {
            return new AuthResponseDto
            {
                Success = response.Success,
                Message = response.Success 
                    ? "Registration successful. Please wait for admin to activate your account to login!" 
                    : response.Message
            };
        }

        public static AuthResponseDto ToAuthResponseDto(this RefreshTokenResponse response)
        {
            return new AuthResponseDto
            {
                Success = response.Success,
                Message = response.Success ? "Token refreshed successfully" : "Invalid or expired token",
                Token = response.Token,
                RefreshToken = response.RefreshToken
            };
        }

        public static CurrentUserDto ToCurrentUserDto(this CurrentUserResponse response)
        {
            return new CurrentUserDto
            {
                Success = response.Success,
                Message = response.Success ? "User data retrieved successfully" : "Failed to retrieve user data",
                Data = response.Success ? new CurrentUserDto.UserData
                {
                    UserId = response.UserId,
                    Username = response.Username,
                    Email = response.Email,
                    FullName = response.FullName,
                    UserType = response.UserType
                } : null
            };
        }

        private static AuthResponseDto.UserInfoDto ToUserInfoDto(User user)
        {
            return new AuthResponseDto.UserInfoDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                UserType = user.UserType.ToString()
            };
        }

        public static CurrentUserResponse ToResponse(this User user, bool success = true)
        {
            if (user == null)
            {
                return new CurrentUserResponse { Success = false };
            }

            return new CurrentUserResponse
            {
                Success = success,
                UserId = user.UserId.ToString(),
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                UserType = user.UserType.ToString()
            };
        }
    }
} 