import { 
    BookBorrowingRequestDto, 
    BorrowingRequestStatus, 
} from "../../../types/borrowing";
import { Button } from "../../../components/ui/button";
import { Badge } from "../../../components/ui/badge";
import {
    Collapsible,
    CollapsibleContent,
    CollapsibleTrigger,
} from "../../../components/ui/collapsible";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "../../../components/ui/table";
import { ChevronDown, ChevronUp, CheckCircle, XCircle } from "lucide-react";
import { format } from "date-fns";
import { getBadgeVariant, getBorrowingRequestStatusName, getBorrowingDetailStatusName } from "../../../utils/enumUtils";

interface BorrowingRequestItemProps {
    request: BookBorrowingRequestDto;
    isExpanded: boolean;
    toggleExpand: (requestId: string) => void;
    onApprove: (request: BookBorrowingRequestDto) => void;
    onReject: (request: BookBorrowingRequestDto) => void;
}

export const BorrowingRequestItem =({
    request,
    isExpanded,
    toggleExpand,
    onApprove,
    onReject
}: BorrowingRequestItemProps) => {
    // Format date
    const formatDate = (dateString: string | null) => {
        if (!dateString) return "-";
        return format(new Date(dateString), "MMM dd, yyyy h:mm a");
    };

    return (
        <Collapsible
            key={request.requestId}
            open={isExpanded}
            onOpenChange={() => toggleExpand(request.requestId)}
            className="border rounded-md"
        >
            <div className="flex flex-wrap items-center justify-between p-4">
                <div className="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-6 mb-2 sm:mb-0">
                    <div>
                        <h4 className="text-sm font-medium">
                            Request #{request.requestId.substring(0, 8)} by {request.requestorName}
                        </h4>
                        <p className="text-xs text-muted-foreground">
                            {formatDate(request.requestDate)}
                        </p>
                    </div>

                    <div className="flex items-center gap-2">
                        <Badge variant={getBadgeVariant(request.status)}>
                            {getBorrowingRequestStatusName(request.status)}
                        </Badge>
                        <div className="text-sm">
                            {request.details.length} {request.details.length === 1 ? "book" : "books"}
                        </div>
                    </div>
                </div>

                <div className="flex items-center gap-2">
                    {request.status === BorrowingRequestStatus.Waiting && (
                        <>
                            <Button
                                size="sm"
                                className="flex gap-1 items-center"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    onApprove(request);
                                }}
                            >
                                <CheckCircle className="h-4 w-4" />
                                <span>Approve</span>
                            </Button>
                            <Button
                                size="sm"
                                variant="secondary"
                                className="flex gap-1 items-center"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    onReject(request);
                                }}
                            >
                                <XCircle className="h-4 w-4" />
                                <span>Reject</span>
                            </Button>
                        </>
                    )}

                    <CollapsibleTrigger asChild>
                        <Button variant="ghost" size="icon">
                            {isExpanded ? (
                                <ChevronUp className="h-4 w-4" />
                            ) : (
                                <ChevronDown className="h-4 w-4" />
                            )}
                        </Button>
                    </CollapsibleTrigger>
                </div>
            </div>

            <CollapsibleContent className="border-t">
                <div className="p-4">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Book</TableHead>
                                <TableHead>Status</TableHead>
                                <TableHead>Due Date</TableHead>
                                <TableHead>Return Date</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {request.details.map(detail => (
                                <TableRow key={detail.detailId}>
                                    <TableCell>{detail.bookTitle}</TableCell>
                                    <TableCell>
                                        <Badge variant={getBadgeVariant(detail.status)}>
                                            {getBorrowingDetailStatusName(detail.status)}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        {formatDate(detail.dueDate)}
                                    </TableCell>
                                    <TableCell>
                                        {formatDate(detail.returnDate)}
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>

                    {request.status !== BorrowingRequestStatus.Waiting && request.approverName && (
                        <div className="mt-4 text-sm">
                            <p><span className="font-medium">Processed by:</span> {request.approverName}</p>
                            <p><span className="font-medium">Processed on:</span> {formatDate(request.approvalDate)}</p>
                        </div>
                    )}

                    {request.notes && (
                        <div className="mt-4 p-3 bg-muted rounded-md">
                            <p className="text-sm font-medium">Notes</p>
                            <p className="text-sm">{request.notes}</p>
                        </div>
                    )}
                </div>
            </CollapsibleContent>
        </Collapsible>
    );
} 