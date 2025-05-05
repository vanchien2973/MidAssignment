import { Edit, Trash2 } from "lucide-react";
import { Button } from "../../../components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "../../../components/ui/table";
import { Book } from "../../../types/book";

interface BooksTableProps {
    books: Book[];
    onEditBook: (book: Book) => void;
    onDeleteBook: (book: Book) => void;
    currentPage: number;
    pageSize: number;
}

export const BooksTable = ({ books, onEditBook, onDeleteBook, currentPage, pageSize }: BooksTableProps) => {
    return (
        <div className="rounded-md border overflow-x-auto">
            <Table>
                <TableHeader>
                    <TableRow>
                        <TableHead>No.</TableHead>
                        <TableHead>Title</TableHead>
                        <TableHead>Author</TableHead>
                        <TableHead>Category</TableHead>
                        <TableHead>ISBN</TableHead>
                        <TableHead className="text-center">Total Copies</TableHead>
                        <TableHead className="text-center">Available</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    {books.length > 0 ? (
                        books.map((book, index) => (
                            <TableRow key={book.bookId}>
                                <TableCell>{(currentPage - 1) * pageSize + index + 1}</TableCell>
                                <TableCell className="font-medium">{book.title}</TableCell>
                                <TableCell>{book.author}</TableCell>
                                <TableCell>{book.categoryName}</TableCell>
                                <TableCell>{book.isbn}</TableCell>
                                <TableCell className="text-center">{book.totalCopies}</TableCell>
                                <TableCell className="text-center">
                                    <span className={`px-2 py-1 rounded-full text-xs ${book.availableCopies === 0
                                            ? 'bg-red-100 text-red-800'
                                            : book.availableCopies < book.totalCopies
                                                ? 'bg-yellow-100 text-yellow-800'
                                                : 'bg-green-100 text-green-800'
                                        }`}>
                                        {book.availableCopies}
                                    </span>
                                </TableCell>
                                <TableCell className="text-right space-x-2">
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        onClick={() => onEditBook(book)}
                                    >
                                        <Edit size={16} />
                                    </Button>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        onClick={() => onDeleteBook(book)}
                                    >
                                        <Trash2 size={16} />
                                    </Button>
                                </TableCell>
                            </TableRow>
                        ))
                    ) : (
                        <TableRow>
                            <TableCell colSpan={8} className="h-24 text-center">
                                No books found.
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
        </div>
    );
}