using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Interfaces.Repositories;

public interface IBookBorrowingRequestDetailRepository
{
    Task<BookBorrowingRequestDetail> GetByIdAsync(Guid detailId);
    Task<BookBorrowingRequestDetail> CreateAsync(BookBorrowingRequestDetail detail);
    Task UpdateAsync(BookBorrowingRequestDetail detail);
    Task<IEnumerable<BookBorrowingRequestDetail>> GetOverdueItemsAsync(int pageNumber, int pageSize);
    Task<bool> HasActiveBorrowingsForBookAsync(Guid bookId);
}