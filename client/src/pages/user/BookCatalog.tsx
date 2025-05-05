import { useState, useEffect } from "react";
import { Book } from "../../types/book";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "../../components/ui/card";
import { Input } from "../../components/ui/input";
import { Button } from "../../components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "../../components/ui/select";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "../../components/ui/tooltip";
import { Search, Plus, Loader2 } from "lucide-react";

interface BookCatalogProps {
  books: Book[];
  categories: { id: number; name: string }[];
  onAddBook: (book: Book) => void;
  onFetchBooks: (categoryId: string, pageNum: number, pageSize: number, searchQuery: string) => Promise<void>;
  isLoading: boolean;
  selectedBooks: Book[];
  maxBooksPerRequest: number;
  currentPage: number;
  totalBooks: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
}

export const BookCatalog = ({
  books,
  categories,
  onAddBook,
  onFetchBooks,
  isLoading,
  selectedBooks,
  maxBooksPerRequest,
  currentPage,
  totalBooks,
  pageSize,
  onPageChange,
  onPageSizeChange
}: BookCatalogProps) => {
  const [searchTerm, setSearchTerm] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");
  const [filteredBooks, setFilteredBooks] = useState<Book[]>(books);
  const [displayBooks, setDisplayBooks] = useState<Book[]>([]);

  // Ensure we display the correct books based on search and filter
  useEffect(() => {
    let booksToDisplay = searchTerm.trim() ? filteredBooks : books;
    setDisplayBooks(booksToDisplay);
  }, [books, filteredBooks, searchTerm]);

  // Update books list when books prop changes
  useEffect(() => {
    if (searchTerm.trim() === "") {
      setFilteredBooks(books);
    } else {
      filterBooks();
    }
  }, [books]);

  // Update filtered books when search term changes
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    if (e.target.value.trim() === "") {
      setFilteredBooks(books);
    } else {
      filterBooks();
    }
  };
  
  const filterBooks = () => {
    const filtered = books.filter(book => 
      book.title.toLowerCase().includes(searchTerm.toLowerCase()) || 
      book.author.toLowerCase().includes(searchTerm.toLowerCase())
    );
    setFilteredBooks(filtered);
  };
  
  const handleCategoryChange = (value: string) => {
    setCategoryFilter(value);
    onFetchBooks(value, 1, pageSize, searchTerm);
  };
  
  const handleSearch = () => {
    onFetchBooks(categoryFilter, 1, pageSize, searchTerm);
  };

  // Calculate total pages
  const totalPages = Math.ceil(totalBooks / pageSize);

  return (
    <TooltipProvider>
      <Card className="dark:border-gray-700">
        <CardHeader>
          <CardTitle className="dark:text-white">Available Books</CardTitle>
          <CardDescription className="dark:text-gray-400">
            Browse and select books to add to your borrowing request
          </CardDescription>
          <div className="flex flex-col sm:flex-row gap-4 pt-4">
            <div className="relative flex-grow sm:max-w-sm">
              <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-gray-500 dark:text-gray-400" />
              <Input
                placeholder="Search books..."
                value={searchTerm}
                onChange={handleSearchChange}
                className="pl-8 dark:bg-gray-800 dark:border-gray-700"
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              />
            </div>
            <div className="flex space-x-2">
              <Select value={categoryFilter} onValueChange={handleCategoryChange}>
                <SelectTrigger className="sm:w-[180px] dark:bg-gray-800 dark:border-gray-700">
                  <SelectValue placeholder="All Categories" />
                </SelectTrigger>
                <SelectContent className="dark:bg-gray-800 dark:border-gray-700">
                  <SelectItem value="all">All Categories</SelectItem>
                  {categories.map((category) => (
                    <SelectItem 
                      key={category.id} 
                      value={category.id.toString()}
                    >
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button variant="outline" onClick={handleSearch}>
                <Search className="h-4 w-4 mr-2" /> Search
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex justify-center py-8">
              <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
          ) : displayBooks.length > 0 ? (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {displayBooks
                .filter(book => book.availableCopies > 0)
                .map(book => (
                <Card key={book.bookId} className="overflow-hidden dark:border-gray-700 flex flex-col h-full">
                  <CardHeader className="pb-2 pt-4">
                    <div className="flex items-start justify-between">
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <CardTitle className="text-xl font-bold line-clamp-1 dark:text-white">{book.title || 'Untitled Book'}</CardTitle>
                        </TooltipTrigger>
                        <TooltipContent className="bg-black/80 text-white dark:bg-white/90 dark:text-gray-900 max-w-sm">
                          <div className="font-medium">{book.title || 'Untitled Book'}</div>
                          <div className="text-xs mt-1 opacity-90">{book.description || 'No description available'}</div>
                        </TooltipContent>
                      </Tooltip>
                      <div className="px-2 py-1 rounded-md bg-green-100 text-green-800 text-xs dark:bg-green-900 dark:text-green-100">
                        {book.availableCopies || 0} available
                      </div>
                    </div>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <CardDescription className="text-base dark:text-gray-400">{book.author || 'Unknown Author'}</CardDescription>
                      </TooltipTrigger>
                      <TooltipContent side="bottom" className="bg-black/80 text-white dark:bg-white/90 dark:text-gray-900">
                        <div className="max-w-xs">
                          <div className="font-medium">{book.author || 'Unknown Author'}</div>
                        </div>
                      </TooltipContent>
                    </Tooltip>
                  </CardHeader>
                  <CardContent className="pb-2 pt-0 flex-grow">
                    <div className="flex flex-col space-y-1 text-sm dark:text-gray-300">
                      <div><span className="font-medium">Category:</span> {book.categoryName || 'Uncategorized'}</div>
                      <div><span className="font-medium">Year:</span> {book.publishedYear || 'Unknown'}</div>
                      <div><span className="font-medium">Publisher:</span> {book.publisher || 'Unknown'}</div>
                    </div>
                  </CardContent>
                  <CardFooter className="pt-2">
                    <Button 
                      className="w-full flex gap-2 justify-center" 
                      variant="default"
                      style={{ backgroundColor: '#4f46e5' }}
                      onClick={() => onAddBook(book)}
                      disabled={
                        selectedBooks.length >= maxBooksPerRequest ||
                        selectedBooks.some(selectedBook => selectedBook.bookId === book.bookId)
                      }
                    >
                      <Plus size={16} /> Add to Request
                    </Button>
                  </CardFooter>
                </Card>
              ))}
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center py-8">
              <Search className="h-10 w-10 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium dark:text-white">No books found</h3>
              <p className="text-sm text-muted-foreground text-center mt-1 dark:text-gray-400">
                Try adjusting your search or filter to find what you're looking for
              </p>
            </div>
          )}
          
          {/* Pagination controls */}
          {totalBooks > 0 && (
            <div className="mt-6 space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <span className="text-sm text-muted-foreground dark:text-gray-400">Books per page:</span>
                  <select
                    className="h-8 rounded-md border border-input bg-background px-3 py-1 text-sm dark:bg-gray-800 dark:border-gray-700 dark:text-gray-200"
                    value={pageSize}
                    onChange={(e) => onPageSizeChange(parseInt(e.target.value))}
                  >
                    <option value="6">6</option>
                    <option value="9">9</option>
                    <option value="12">12</option>
                    <option value="15">15</option>
                    <option value="24">24</option>
                  </select>
                </div>
                <div className="text-sm text-muted-foreground dark:text-gray-400">
                  Page {currentPage} / {totalPages || 1} ({totalBooks} books)
                </div>
              </div>
              
              <div className="flex justify-center">
                <div className="flex space-x-1">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={currentPage === 1}
                    onClick={() => onPageChange(1)}
                    className="dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"
                  >
                    First
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={currentPage === 1}
                    onClick={() => onPageChange(currentPage - 1)}
                    className="dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"
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
                          onClick={() => onPageChange(pageNum)}
                          className={currentPage === pageNum ? 
                            "bg-indigo-600 hover:bg-indigo-700" : 
                            "dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"}
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
                    onClick={() => onPageChange(currentPage + 1)}
                    className="dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"
                  >
                    Next
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={currentPage >= totalPages}
                    onClick={() => onPageChange(totalPages)}
                    className="dark:border-gray-700 dark:bg-gray-800 dark:text-gray-200"
                  >
                    Last
                  </Button>
                </div>
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </TooltipProvider>
  );
}; 