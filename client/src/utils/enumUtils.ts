import { BorrowingDetailStatus, BorrowingRequestStatus } from "../types/borrowing";


// Function to convert enum to display name
export function getBorrowingRequestStatusName(status: BorrowingRequestStatus): string {
  switch (status) {
    case BorrowingRequestStatus.Waiting:
      return "Waiting";
    case BorrowingRequestStatus.Approved:
      return "Approved";
    case BorrowingRequestStatus.Rejected:
      return "Rejected";
    default:
      return "Unknown";
  }
}

export function getBorrowingDetailStatusName(status: BorrowingDetailStatus): string {
  switch (status) {
    case BorrowingDetailStatus.Borrowing:
      return "Borrowing";
    case BorrowingDetailStatus.Returned:
      return "Returned";
    case BorrowingDetailStatus.Extended:
      return "Extended";
    default:
      return "Unknown";
  }
}

// Function to convert status to badge variant
export function getBadgeVariant(status: BorrowingRequestStatus | BorrowingDetailStatus) {
  // Request statuses
  if (status === BorrowingRequestStatus.Waiting) {
    return "warning" as const;
  }
  if (status === BorrowingRequestStatus.Approved) {
    return "success" as const;
  }
  if (status === BorrowingRequestStatus.Rejected) {
    return "destructive" as const;
  }
  
  // Detail statuses
  if (status === BorrowingDetailStatus.Borrowing) {
    return "secondary" as const;
  }
  if (status === BorrowingDetailStatus.Returned) {
    return "success" as const;
  }
  if (status === BorrowingDetailStatus.Extended) {
    return "outline" as const;
  }
  
  return "default" as const;
}