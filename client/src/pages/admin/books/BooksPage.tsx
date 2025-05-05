import { useState, useEffect } from "react";
import { Input } from "../../../components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../../components/ui/select";
import { Category } from "../../../types/category";
import { useToast } from "../../../hooks/use-toast";
import { BooksTable } from "./BooksTable";
import { AddBookDialog } from "./AddBookDialog";
import { EditBookDialog } from "./EditBookDialog";
import { DeleteBookDialog } from "./DeleteBookDialog";
import BookService from "../../../services/book.service";
import CategoryService from "../../../services/category.service";
import { Book as BookType, BookCreateDto, BookUpdateDto } from "../../../types/book";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../components/ui/card";
import { Loader2, Plus, BookIcon } from "lucide-react";
import { Button } from "../../../components/ui/button";

export const BooksPage = () => {
  const [books, setBooks] = useState<BookType[]>([]);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [currentBook, setCurrentBook] = useState<BookType | null>(null);
  const [searchTerm, setSearchTerm] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [totalBooks, setTotalBooks] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  
  // Form state
  const [bookTitle, setBookTitle] = useState("");
  const [bookAuthor, setBookAuthor] = useState("");
  const [bookCategoryId, setBookCategoryId] = useState<string | null>(null);
  const [bookIsbn, setBookIsbn] = useState("");
  const [bookPublishedYear, setBookPublishedYear] = useState<number | null>(null);
  const [bookPublisher, setBookPublisher] = useState("");
  const [bookDescription, setBookDescription] = useState("");
  const [bookTotalCopies, setBookTotalCopies] = useState<number>(1);
  
  const { toast } = useToast();
  
  useEffect(() => {
    fetchCategories();
  }, []);

  useEffect(() => {
    fetchBooks();
  }, [currentPage, pageSize, categoryFilter]);

  const fetchCategories = async () => {
    try {
      const response = await CategoryService.getCategories(1, 100);
      if (response.success) {
        setCategories(response.data);
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to fetch categories",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error fetching categories:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while fetching categories",
        variant: "destructive",
      });
    }
  };

  const fetchBooks = async () => {
    setIsLoading(true);
    try {
      let response;
      
      if (categoryFilter !== "all") {
        response = await BookService.getBooksByCategory(categoryFilter, currentPage, pageSize);
      } else {
        response = await BookService.getBooks(currentPage, pageSize);
      }
      
      if (response.success) {
        setBooks(response.data);
        if (response.totalCount) {
          setTotalBooks(response.totalCount);
        }
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to fetch books",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error fetching books:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while fetching books",
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };
  
  // Search books with debounce
  useEffect(() => {
    if (searchTerm === "") {
      fetchBooks();
      return;
    }
    
    const debounceTimer = setTimeout(() => {
      setBooks(books.filter(book => {
        const searchTermLower = searchTerm.toLowerCase();
        return book.title.toLowerCase().includes(searchTermLower) || 
          book.author.toLowerCase().includes(searchTermLower) ||
          book.isbn.toLowerCase().includes(searchTermLower);
      }));
    }, 300);
    
    return () => clearTimeout(debounceTimer);
  }, [searchTerm]);
  
  const handleAddBook = async () => {
    if (!bookCategoryId) return;
    
    try {
      const categoryResponse = await CategoryService.getCategory(bookCategoryId.toString());
      
      if (!categoryResponse.success) {
        toast({
          title: "Error",
          description: "Could not retrieve category information",
          variant: "destructive",
        });
        return;
      }
      
      const bookData: BookCreateDto = {
        title: bookTitle,
        author: bookAuthor,
        categoryId: categoryResponse.data?.categoryId.toString() || bookCategoryId.toString(),
        isbn: bookIsbn,
        publishedYear: bookPublishedYear,
        publisher: bookPublisher || "",
        description: bookDescription || "",
        totalCopies: bookTotalCopies
      };
      
      const response = await BookService.createBook(bookData);
      
      if (response.success) {
        toast({
          title: "Success",
          description: response.message || "Book added successfully",
        });
        fetchBooks();
        setIsAddDialogOpen(false);
        resetForm();
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to add book",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error adding book:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while adding the book",
        variant: "destructive",
      });
    }
  };
  
  const handleEditBook = async () => {
    if (!currentBook || !bookCategoryId) return;
    
    try {
      const categoryResponse = await CategoryService.getCategory(bookCategoryId.toString());
      
      if (!categoryResponse.success) {
        toast({
          title: "Error",
          description: "Could not retrieve category information",
          variant: "destructive",
        });
        return;
      }
      
      const bookData: BookUpdateDto = {
        bookId: currentBook.bookId.toString(),
        title: bookTitle,
        author: bookAuthor,
        categoryId: categoryResponse.data?.categoryId.toString() || bookCategoryId.toString(),
        isbn: bookIsbn,
        publishedYear: bookPublishedYear,
        publisher: bookPublisher || "",
        description: bookDescription || "",
        totalCopies: bookTotalCopies
      };
      
      const response = await BookService.updateBook(bookData);
      
      if (response.success) {
        toast({
          title: "Success",
          description: response.message || "Book updated successfully",
        });
        fetchBooks();
        setIsEditDialogOpen(false);
        resetForm();
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to update book",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error updating book:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while updating the book",
        variant: "destructive",
      });
    }
  };
  
  const handleDeleteBook = async () => {
    if (!currentBook) return;
    
    try {
      const response = await BookService.deleteBook(currentBook.bookId.toString());
      
      if (response.success) {
        toast({
          title: "Success",
          description: response.message || "Book deleted successfully",
        });
        // Refresh the book list and maintain the current page unless this was the last book on the page
        const isLastItemOnPage = books.length === 1 && currentPage > 1;
        const pageToFetch = isLastItemOnPage ? currentPage - 1 : currentPage;
        setCurrentPage(pageToFetch);
        fetchBooks();
        setIsDeleteDialogOpen(false);
        setCurrentBook(null);
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to delete book",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error deleting book:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while deleting the book",
        variant: "destructive",
      });
    }
  };
  
  const openEditDialog = (book: BookType) => {
    setCurrentBook(book);
    setBookTitle(book.title);
    setBookAuthor(book.author);
    setBookCategoryId(book.categoryId.toString());
    setBookIsbn(book.isbn);
    setBookPublishedYear(book.publishedYear);
    setBookPublisher(book.publisher || "");
    setBookDescription(book.description || "");
    setBookTotalCopies(book.totalCopies);
    setIsEditDialogOpen(true);
  };
  
  const openDeleteDialog = (book: BookType) => {
    setCurrentBook(book);
    setIsDeleteDialogOpen(true);
  };
  
  const resetForm = () => {
    setBookTitle("");
    setBookAuthor("");
    setBookCategoryId(null);
    setBookIsbn("");
    setBookPublishedYear(null);
    setBookPublisher("");
    setBookDescription("");
    setBookTotalCopies(1);
    setCurrentBook(null);
  };

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
  };

  const handleCategoryFilterChange = (value: string) => {
    setCategoryFilter(value);
    setCurrentPage(1); // Reset to first page when filter changes
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle page size change
  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1); // Reset to first page when page size changes
  };

  const bookFormData = {
    title: bookTitle,
    author: bookAuthor,
    categoryId: bookCategoryId,
    isbn: bookIsbn,
    publishedYear: bookPublishedYear,
    publisher: bookPublisher,
    description: bookDescription,
    totalCopies: bookTotalCopies
  };

  // Calculate total pages
  const totalPages = Math.ceil(totalBooks / pageSize);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Book Management</h2>
          <p className="text-muted-foreground">
            Manage books in the library
          </p>
        </div>
        <Button onClick={() => setIsAddDialogOpen(true)} className="flex items-center gap-2">
          <Plus size={16} />
          Add Book
        </Button>
      </div>
      
      <div className="flex flex-col sm:flex-row gap-4 items-center py-4">
        <Input
          placeholder="Search books..."
          value={searchTerm}
          onChange={handleSearch}
          className="sm:max-w-sm"
        />
        <Select value={categoryFilter} onValueChange={handleCategoryFilterChange}>
          <SelectTrigger className="sm:max-w-[200px]">
            <SelectValue placeholder="Filter by category" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Categories</SelectItem>
            {categories.map((category) => (
              <SelectItem key={category.categoryId} value={category.categoryId.toString()}>
                {category.categoryName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      
      {isLoading ? (
        <Card>
          <CardContent className="p-10 flex justify-center items-center">
            <Loader2 className="h-12 w-12 animate-spin text-primary" />
          </CardContent>
        </Card>
      ) : (
        <>
          <Card>
            <CardHeader className="flex flex-row items-center gap-4">
              <div className="bg-primary p-3 rounded-lg">
                <BookIcon className="h-6 w-6 text-primary-foreground" />
              </div>
              <div>
                <CardTitle>Book List</CardTitle>
                <CardDescription>
                  Manage and organize library books
                </CardDescription>
              </div>
            </CardHeader>
            <CardContent>
              <BooksTable
                books={books}
                onEditBook={openEditDialog}
                onDeleteBook={openDeleteDialog}
                currentPage={currentPage}
                pageSize={pageSize}
              />
            </CardContent>
          </Card>
          
          {/* Pagination controls */}
          <div className="mt-6 space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <span className="text-sm text-muted-foreground">Items per page:</span>
                <select
                  className="h-8 rounded-md border border-input bg-background px-3 py-1 text-sm"
                  value={pageSize}
                  onChange={(e) => handlePageSizeChange(parseInt(e.target.value))}
                >
                  <option value="5">5</option>
                  <option value="10">10</option>
                  <option value="20">20</option>
                  <option value="50">50</option>
                </select>
              </div>
              <div className="text-sm text-muted-foreground">
                Page {currentPage} of {totalPages || 1} ({totalBooks} items)
              </div>
            </div>
            
            <div className="flex justify-center">
              <div className="flex space-x-1">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage === 1}
                  onClick={() => handlePageChange(1)}
                >
                  First
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage === 1}
                  onClick={() => handlePageChange(currentPage - 1)}
                >
                  Previous
                </Button>
                
                {/* Page number buttons */}
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  // Calculate page numbers to show around current page
                  let pageNum;
                  if (totalPages <= 5) {
                    pageNum = i + 1;
                  } else if (currentPage <= 3) {
                    pageNum = i + 1;
                  } else if (currentPage >= totalPages - 2) {
                    pageNum = totalPages - 4 + i;
                  } else {
                    pageNum = currentPage - 2 + i;
                  }
                  
                  // Only render if page is in range
                  if (pageNum > 0 && pageNum <= totalPages) {
                    return (
                      <Button
                        key={pageNum}
                        variant={currentPage === pageNum ? "default" : "outline"}
                        size="sm"
                        onClick={() => handlePageChange(pageNum)}
                      >
                        {pageNum}
                      </Button>
                    );
                  }
                  return null;
                })}
                
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage >= totalPages}
                  onClick={() => handlePageChange(currentPage + 1)}
                >
                  Next
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage >= totalPages}
                  onClick={() => handlePageChange(totalPages)}
                >
                  Last
                </Button>
              </div>
            </div>
          </div>
        </>
      )}
      
      <AddBookDialog
        isOpen={isAddDialogOpen}
        onOpenChange={setIsAddDialogOpen}
        bookData={bookFormData}
        categories={categories}
        onTitleChange={setBookTitle}
        onAuthorChange={setBookAuthor}
        onCategoryChange={setBookCategoryId}
        onIsbnChange={setBookIsbn}
        onPublishedYearChange={setBookPublishedYear}
        onPublisherChange={setBookPublisher}
        onDescriptionChange={setBookDescription}
        onTotalCopiesChange={setBookTotalCopies}
        onAddBook={handleAddBook}
        onCancel={() => {
          resetForm();
          setIsAddDialogOpen(false);
        }}
      />
      
      <EditBookDialog
        isOpen={isEditDialogOpen}
        onOpenChange={(open) => {
          if (!open) resetForm();
          setIsEditDialogOpen(open);
        }}
        bookData={bookFormData}
        categories={categories}
        onTitleChange={setBookTitle}
        onAuthorChange={setBookAuthor}
        onCategoryChange={setBookCategoryId}
        onIsbnChange={setBookIsbn}
        onPublishedYearChange={setBookPublishedYear}
        onPublisherChange={setBookPublisher}
        onDescriptionChange={setBookDescription}
        onTotalCopiesChange={setBookTotalCopies}
        onEditBook={handleEditBook}
        onCancel={() => {
          resetForm();
          setIsEditDialogOpen(false);
        }}
      />
      
      <DeleteBookDialog
        isOpen={isDeleteDialogOpen}
        onOpenChange={setIsDeleteDialogOpen}
        book={currentBook}
        onDeleteBook={handleDeleteBook}
        onCancel={() => {
          setCurrentBook(null);
          setIsDeleteDialogOpen(false);
        }}
      />
    </div>
  );
}