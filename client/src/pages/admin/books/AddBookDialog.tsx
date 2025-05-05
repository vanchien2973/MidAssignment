import { Button } from "../../../components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "../../../components/ui/dialog";
import { Category } from "../../../types/category";
import { BookForm } from "./BookForm";

interface AddBookDialogProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    bookData: {
        title: string;
        author: string;
        categoryId: string | null;
        isbn: string;
        publishedYear: number | null;
        publisher: string;
        description: string;
        totalCopies: number;
    };
    categories: Category[];
    onTitleChange: (value: string) => void;
    onAuthorChange: (value: string) => void;
    onCategoryChange: (value: string) => void;
    onIsbnChange: (value: string) => void;
    onPublishedYearChange: (value: number | null) => void;
    onPublisherChange: (value: string) => void;
    onDescriptionChange: (value: string) => void;
    onTotalCopiesChange: (value: number) => void;
    onAddBook: () => void;
    onCancel: () => void;
}

export const AddBookDialog = ({
    isOpen,
    onOpenChange,
    bookData,
    categories,
    onTitleChange,
    onAuthorChange,
    onCategoryChange,
    onIsbnChange,
    onPublishedYearChange,
    onPublisherChange,
    onDescriptionChange,
    onTotalCopiesChange,
    onAddBook,
    onCancel
}: AddBookDialogProps) => {
    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[600px]">
                <DialogHeader>
                    <DialogTitle>Add New Book</DialogTitle>
                    <DialogDescription>
                        Add a new book to the library catalog.
                    </DialogDescription>
                </DialogHeader>
                <BookForm
                    title={bookData.title}
                    author={bookData.author}
                    categoryId={bookData.categoryId}
                    isbn={bookData.isbn}
                    publishedYear={bookData.publishedYear}
                    publisher={bookData.publisher}
                    description={bookData.description}
                    totalCopies={bookData.totalCopies}
                    categories={categories}
                    onTitleChange={onTitleChange}
                    onAuthorChange={onAuthorChange}
                    onCategoryChange={onCategoryChange}
                    onIsbnChange={onIsbnChange}
                    onPublishedYearChange={onPublishedYearChange}
                    onPublisherChange={onPublisherChange}
                    onDescriptionChange={onDescriptionChange}
                    onTotalCopiesChange={onTotalCopiesChange}
                />
                <DialogFooter>
                    <Button variant="outline" onClick={onCancel}>
                        Cancel
                    </Button>
                    <Button
                        onClick={onAddBook}
                        disabled={!bookData.title || !bookData.author || !bookData.categoryId || !bookData.isbn || bookData.totalCopies < 1}
                    >
                        Add Book
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}