// Define enums for borrowing statuses
export enum BorrowingRequestStatus {
  Waiting = 1,
  Approved = 2,
  Rejected = 3
}

export enum BorrowingDetailStatus {
  Borrowing = 1,
  Returned = 2,
  Extended = 3
}

// DTOs for sending requests
export interface CreateBorrowingRequestDto {
  requestorId: number;
  books: {
    bookId: number;
  }[];
  notes?: string;
}

export interface ReturnBookDto {
  detailId: string; // GUID
  userId: number;
  notes?: string;
}

export interface ExtendBorrowingDto {
  detailId: string; // GUID
  userId: number;
  newDueDate: string; // ISO date string
  notes?: string;
}

export interface BorrowingRequestStatusUpdateDto {
  requestId: string; // GUID
  approverId: number;
  status: BorrowingRequestStatus;
  notes?: string;
  dueDays?: number;
}

// DTOs for receiving data
export interface BookBorrowingRequestDetailDto {
  detailId: string; // GUID
  requestId: string; // GUID
  bookId: number;
  bookTitle: string;
  dueDate: string | null; // ISO date string
  returnDate: string | null; // ISO date string
  status: BorrowingDetailStatus;
  extensionDate: string | null; // ISO date string
}

export interface BookBorrowingRequestDto {
  requestId: string; // GUID
  requestorId: number;
  requestorName: string;
  requestDate: string; // ISO date string
  status: BorrowingRequestStatus;
  approverId: number | null;
  approverName: string | null;
  approvalDate: string | null; // ISO date string
  notes: string | null;
  details: BookBorrowingRequestDetailDto[];
}

// Parameters for queries
export interface GetBorrowingRequestsParams {
  userId?: number;
  pageNumber?: number;
  pageSize?: number;
}

// Response types
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  totalCount?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface BorrowingResults {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  results: BookBorrowingRequestDto[];
}

export interface OverdueResults {
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  results: BookBorrowingRequestDetailDto[];
} 