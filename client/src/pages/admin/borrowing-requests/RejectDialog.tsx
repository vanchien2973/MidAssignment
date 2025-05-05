import { BookBorrowingRequestDto } from "../../../types/borrowing";
import { Button } from "../../../components/ui/button";
import { Textarea } from "../../../components/ui/textarea";
import { XCircle, Loader2 } from "lucide-react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "../../../components/ui/dialog";

interface RejectDialogProps {
    isOpen: boolean;
    onClose: () => void;
    currentRequest: BookBorrowingRequestDto | null;
    notes: string;
    onNotesChange: (value: string) => void;
    onReject: () => void;
    isSubmitting: boolean;
}

export const RejectDialog = ({
    isOpen,
    onClose,
    currentRequest,
    notes,
    onNotesChange,
    onReject,
    isSubmitting
}: RejectDialogProps) => {
    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Reject Borrowing Request</DialogTitle>
                    <DialogDescription>
                        You are rejecting request #{currentRequest?.requestId.substring(0, 8)} from {currentRequest?.requestorName}.
                    </DialogDescription>
                </DialogHeader>

                <div className="grid gap-4 py-4">
                    <div>
                        <h4 className="text-sm font-medium mb-1">Books Requested:</h4>
                        <ul className="text-sm space-y-1">
                            {currentRequest?.details.map(detail => (
                                <li key={detail.detailId}>• {detail.bookTitle}</li>
                            ))}
                        </ul>
                    </div>

                    <div className="grid gap-2">
                        <label htmlFor="reject-notes" className="text-sm font-medium">
                            Reason for rejection:
                        </label>
                        <Textarea
                            id="reject-notes"
                            value={notes}
                            onChange={(e) => onNotesChange(e.target.value)}
                            placeholder="Provide a reason for rejecting this request"
                            className="resize-none"
                            disabled={isSubmitting}
                        />
                    </div>
                </div>

                <DialogFooter>
                    <Button 
                        variant="outline" 
                        onClick={onClose}
                        disabled={isSubmitting}
                    >
                        Cancel
                    </Button>
                    <Button
                        onClick={onReject}
                        className="flex gap-1 items-center"
                        variant="destructive"
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? (
                            <>
                                <Loader2 className="h-4 w-4 animate-spin" />
                                <span>Processing...</span>
                            </>
                        ) : (
                            <>
                                <XCircle className="h-4 w-4" />
                                <span>Reject</span>
                            </>
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
} 