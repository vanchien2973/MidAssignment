import { BookBorrowingRequestDto } from "../../../types/borrowing";
import { Button } from "../../../components/ui/button";
import { Input } from "../../../components/ui/input";
import { Textarea } from "../../../components/ui/textarea";
import { CheckCircle, Loader2 } from "lucide-react";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "../../../components/ui/dialog";

interface ApproveDialogProps {
    isOpen: boolean;
    onClose: () => void;
    currentRequest: BookBorrowingRequestDto | null;
    notes: string;
    dueDays: string;
    onNotesChange: (value: string) => void;
    onDueDaysChange: (value: string) => void;
    onApprove: () => void;
    isSubmitting: boolean;
}

export const ApproveDialog =({
    isOpen,
    onClose,
    currentRequest,
    notes,
    dueDays,
    onNotesChange,
    onDueDaysChange,
    onApprove,
    isSubmitting
}: ApproveDialogProps) => {
    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Approve Borrowing Request</DialogTitle>
                    <DialogDescription>
                        You are approving request #{currentRequest?.requestId.substring(0, 8)} from {currentRequest?.requestorName}.
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
                        <label htmlFor="due-days" className="text-sm font-medium">
                            Borrowing Period (days):
                        </label>
                        <Input
                            id="due-days"
                            type="number"
                            min="1"
                            value={dueDays}
                            onChange={(e) => onDueDaysChange(e.target.value)}
                            disabled={isSubmitting}
                        />
                    </div>

                    <div className="grid gap-2">
                        <label htmlFor="notes" className="text-sm font-medium">
                            Notes (optional):
                        </label>
                        <Textarea
                            id="notes"
                            value={notes}
                            onChange={(e) => onNotesChange(e.target.value)}
                            placeholder="Add any notes about this approval"
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
                        onClick={onApprove}
                        className="flex gap-1 items-center"
                        disabled={Number(dueDays) < 1 || isSubmitting}
                    >
                        {isSubmitting ? (
                            <>
                                <Loader2 className="h-4 w-4 animate-spin" />
                                <span>Processing...</span>
                            </>
                        ) : (
                            <>
                                <CheckCircle className="h-4 w-4" />
                                <span>Approve</span>
                            </>
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
} 