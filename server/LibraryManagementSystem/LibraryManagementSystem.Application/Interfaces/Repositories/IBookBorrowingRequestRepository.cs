using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface IBookBorrowingRequestRepository
{
    Task<BookBorrowingRequest> GetByIdAsync(Guid requestId);
    Task<BookBorrowingRequest> GetByIdAsync(int requestId);
    Task<IEnumerable<BookBorrowingRequest>> GetByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<IEnumerable<BookBorrowingRequest>> GetPendingRequestsAsync(int pageNumber, int pageSize);
    Task<IEnumerable<BookBorrowingRequest>> GetByStatusAsync(BorrowingRequestStatus status, int pageNumber, int pageSize);
    Task<IEnumerable<BookBorrowingRequest>> GetAllAsync(int pageNumber, int pageSize);
    Task<BookBorrowingRequest> CreateAsync(BookBorrowingRequest request);
    Task UpdateAsync(BookBorrowingRequest request);
    Task<int> GetRequestCountByUserInMonthAsync(int userId, DateTime monthStart, DateTime monthEnd);
    Task<int> CountAsync();
    Task<bool> HasUserActiveBookLoansAsync(int userId);
}