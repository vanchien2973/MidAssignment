import { 
  BookBorrowingRequestDto, 
  BorrowingRequestStatus 
} from "../../types/borrowing";
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from "../../components/ui/table";
import { Badge } from "../../components/ui/badge";
import { Clock, Loader2 } from "lucide-react";

interface BorrowingRequestsListProps {
  requests: BookBorrowingRequestDto[];
  isLoading: boolean;
}

export const BorrowingRequestsList = ({
  requests,
  isLoading
}: BorrowingRequestsListProps) => {
  if (isLoading) {
    return (
      <div className="flex justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }
  
  if (requests.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <Clock className="h-12 w-12 text-muted-foreground mb-4" />
        <h3 className="text-lg font-medium mb-1 dark:text-white">No borrowing requests yet</h3>
        <p className="text-sm text-muted-foreground dark:text-gray-400">
          Browse books and create your first borrowing request
        </p>
      </div>
    );
  }
  
  return (
    <div className="space-y-6">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Request Date</TableHead>
            <TableHead>Books</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Notes</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {requests.map((request) => (
            <TableRow key={request.requestId}>
              <TableCell>
                {request.requestDate ? new Date(request.requestDate).toLocaleDateString() : 'N/A'}
              </TableCell>
              <TableCell>
                {request.details && Array.isArray(request.details)
                  ? request.details.map(detail => detail.bookTitle).join(", ")
                  : 'No books'}
              </TableCell>
              <TableCell>
                <Badge 
                  variant={
                    request.status === BorrowingRequestStatus.Approved 
                      ? "success" 
                      : request.status === BorrowingRequestStatus.Rejected 
                        ? "destructive" 
                        : "secondary"
                  }
                  className={
                    request.status === BorrowingRequestStatus.Approved 
                      ? "bg-green-100 text-green-800 hover:bg-green-100" 
                      : request.status === BorrowingRequestStatus.Rejected 
                        ? "bg-red-100 text-red-800 hover:bg-red-100" 
                        : "bg-yellow-100 text-yellow-800 hover:bg-yellow-100"
                  }
                >
                  {request.status === BorrowingRequestStatus.Approved 
                    ? "Approved" 
                    : request.status === BorrowingRequestStatus.Rejected 
                      ? "Rejected" 
                      : "Waiting"}
                </Badge>
              </TableCell>
              <TableCell className="max-w-xs truncate">
                {request.notes || 'No notes'}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}; 