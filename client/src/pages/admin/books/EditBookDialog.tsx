import { Button } from "../../../components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "../../../components/ui/dialog";
import { Category } from "../../../types/category";
import { BookForm } from "./BookForm";

interface EditBookDialogProps {
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
    onEditBook: () => void;
    onCancel: () => void;
}

export const EditBookDialog =({
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
    onEditBook,
    onCancel
}: EditBookDialogProps) => {
    return (
        <Dialog open={isOpen} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-[600px]">
                <DialogHeader>
                    <DialogTitle>Edit Book</DialogTitle>
                    <DialogDescription>
                        Update the book details.
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
                        onClick={onEditBook}
                        disabled={!bookData.title || !bookData.author || !bookData.categoryId || !bookData.isbn || bookData.totalCopies < 1}
                    >
                        Update Book
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}