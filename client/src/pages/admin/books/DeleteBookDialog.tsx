import {
    AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
    AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle
} from "../../../components/ui/alert-dialog";
import { Book as BookType } from "../../../types/book";

interface DeleteBookDialogProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    book: BookType | null;
    onDeleteBook: () => void;
    onCancel: () => void;
}

export const DeleteBookDialog = ({
    isOpen,
    onOpenChange,
    book,
    onDeleteBook,
    onCancel
}: DeleteBookDialogProps) => {
    return (
        <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Are you sure?</AlertDialogTitle>
                    <AlertDialogDescription>
                        This will permanently delete the book "{book?.title}".
                        This action cannot be undone.
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel onClick={onCancel}>
                        Cancel
                    </AlertDialogCancel>
                    <AlertDialogAction onClick={onDeleteBook}>
                        Delete
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}