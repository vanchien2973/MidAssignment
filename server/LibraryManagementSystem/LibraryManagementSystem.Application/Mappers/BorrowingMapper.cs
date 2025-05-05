using LibraryManagementSystem.Application.Commands.Borrowing;
using LibraryManagementSystem.Application.DTOs.Borrowing;
using LibraryManagementSystem.Application.Queries.Borrowing;
using LibraryManagementSystem.Domain.Entities;
using System;
using System.Linq;

namespace LibraryManagementSystem.Application.Mappers
{
    public static class BorrowingMapper
    {
        public static CreateBorrowingRequestCommand ToCommand(CreateBorrowingRequestDto dto)
        {
            if (dto == null) return null;

            return new CreateBorrowingRequestCommand
            {
                RequestorId = dto.RequestorId,
                Notes = dto.Notes,
                Books = dto.Books?.Select(b => new BorrowingBookItem
                {
                    BookId = b.BookId
                }).ToList()
            };
        }

        public static UpdateBorrowingRequestStatusCommand ToCommand(BorrowingRequestStatusUpdateDto dto)
        {
            if (dto == null) return null;

            return new UpdateBorrowingRequestStatusCommand
            {
                RequestId = dto.RequestId,
                ApproverId = dto.ApproverId,
                Status = dto.Status,
                Notes = dto.Notes,
                DueDays = dto.DueDays
            };
        }

        public static ReturnBookCommand ToCommand(ReturnBookDto dto)
        {
            if (dto == null) return null;

            return new ReturnBookCommand
            {
                DetailId = dto.DetailId,
                UserId = dto.UserId,
                Notes = dto.Notes
            };
        }

        public static ExtendBorrowingCommand ToCommand(ExtendBorrowingDto dto)
        {
            if (dto == null) return null;

            return new ExtendBorrowingCommand
            {
                DetailId = dto.DetailId,
                UserId = dto.UserId,
                NewDueDate = dto.NewDueDate,
                Notes = dto.Notes
            };
        }

        public static GetBorrowingRequestByIdQuery ToQuery(Guid requestId)
        {
            return new GetBorrowingRequestByIdQuery
            {
                RequestId = requestId
            };
        }

        public static GetUserBorrowingRequestsQuery ToQuery(int userId, int pageNumber = 1, int pageSize = 10)
        {
            return new GetUserBorrowingRequestsQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static GetPendingBorrowingRequestsQuery ToPendingQuery(int pageNumber = 1, int pageSize = 10, bool includeAllStatuses = false)
        {
            return new GetPendingBorrowingRequestsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                IncludeAllStatuses = includeAllStatuses
            };
        }

        public static GetOverdueBorrowingsQuery ToOverdueQuery(int pageNumber = 1, int pageSize = 10)
        {
            return new GetOverdueBorrowingsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static CountAllBorrowingRequestsQuery ToCountAllBorrowingRequestsQuery()
        {
            return new CountAllBorrowingRequestsQuery();
        }

        public static BookBorrowingRequestDto ToDto(BookBorrowingRequest entity)
        {
            if (entity == null) return null;

            return new BookBorrowingRequestDto
            {
                RequestId = entity.RequestId,
                RequestorId = entity.RequestorId,
                RequestorName = entity.Requestor?.FullName,
                RequestDate = entity.RequestDate,
                Status = entity.Status,
                ApproverId = entity.ApproverId,
                ApproverName = entity.Approver?.FullName,
                ApprovalDate = entity.ApprovalDate,
                Notes = entity.Notes,
                RequestDetails = entity.RequestDetails?.Select(d => ToDto(d)).ToList()
            };
        }

        public static BookBorrowingRequestDetailDto ToDto(BookBorrowingRequestDetail entity)
        {
            if (entity == null) return null;

            return new BookBorrowingRequestDetailDto
            {
                DetailId = entity.DetailId,
                RequestId = entity.RequestId,
                BookId = entity.BookId,
                BookTitle = entity.Book?.Title,
                BookAuthor = entity.Book?.Author,
                ISBN = entity.Book?.ISBN,
                DueDate = entity.DueDate,
                ReturnDate = entity.ReturnDate,
                Status = entity.Status,
                ExtensionDate = entity.ExtensionDate
            };
        }
    }
} 