import { useState, useEffect } from "react";
import { useToast } from "../../hooks/use-toast";
import { useAuth } from "../../context/AuthContext";
import { 
  Card, CardContent, CardDescription, CardHeader, CardTitle 
} from "../../components/ui/card";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "../../components/ui/tabs";
import CategoryService from "../../services/category.service";
import BookService from "../../services/book.service";
import BorrowingService from "../../services/borrowing.service";
import { Book } from "../../types/book";
import { 
  BookBorrowingRequestDto, 
  CreateBorrowingRequestDto
} from "../../types/borrowing";
import { BookCatalog } from "./BookCatalog";
import { BorrowingRequestForm } from "./BorrowingRequestForm";
import { BorrowingRules } from "./BorrowingRules";
import { BorrowingRequestsList } from "./BorrowingRequestsList";

const BorrowBooksPage = () => {
  const { currentUser } = useAuth();
  const { toast } = useToast();
  
  const [books, setBooks] = useState<Book[]>([]);
  const [filteredBooks, setFilteredBooks] = useState<Book[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");
  const [selectedBooks, setSelectedBooks] = useState<Book[]>([]);
  const [borrowingRequests, setBorrowingRequests] = useState<BookBorrowingRequestDto[]>([]);
  const [requestsThisMonth, setRequestsThisMonth] = useState(0);
  const [categories, setCategories] = useState<{id: number, name: string}[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [notes, setNotes] = useState("");
  
  // Pagination states
  const [totalBooks, setTotalBooks] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(6);
  
  const MAX_BOOKS_PER_REQUEST = 5;
  const MAX_REQUESTS_PER_MONTH = 3;

  // Load initial data
  useEffect(() => {
    fetchCategories();
    fetchBooks(categoryFilter, currentPage, pageSize);
    if (currentUser) {
      fetchUserBorrowingRequests();
    }
  }, [categoryFilter, currentUser, currentPage, pageSize]);

  // Count requests for current month
  useEffect(() => {
    if (currentUser && borrowingRequests.length > 0) {
      const currentMonth = new Date().getMonth();
      const currentYear = new Date().getFullYear();
      
      const requestsCount = borrowingRequests.filter(request => {
        if (!request.requestDate) return false;
        const requestDate = new Date(request.requestDate);
        return request.requestorId === currentUser.userId && 
               requestDate.getMonth() === currentMonth &&
               requestDate.getFullYear() === currentYear;
      }).length;
      
      setRequestsThisMonth(requestsCount);
    }
  }, [currentUser, borrowingRequests]);
  
  // Update filtered books when search term changes
  useEffect(() => {
    if (searchTerm.trim() === "") {
      setFilteredBooks(books);
    } else {
      const filtered = books.filter(book => 
        book.title.toLowerCase().includes(searchTerm.toLowerCase()) || 
        book.author.toLowerCase().includes(searchTerm.toLowerCase())
      );
      setFilteredBooks(filtered);
    }
  }, [books, searchTerm]);
  
  const fetchUserBorrowingRequests = async () => {
    if (!currentUser) return;
    
    setIsLoading(true);
    try {
      const response = await BorrowingService.getUserBorrowingRequests(
        currentUser.userId, 
        1,  
        100 
      );
      
      if (response.success && response.data) {
        const results = Array.isArray(response.data.results) 
          ? response.data.results 
          : Array.isArray(response.data) 
            ? response.data 
            : [];
        
        setBorrowingRequests(results);
      }
    } catch (error) {
      console.error("Error fetching user borrowing requests:", error);
    } finally {
      setIsLoading(false);
    }
  };
  
  const fetchCategories = async () => {
    try {
      const response = await CategoryService.getCategories(1, 100);
      
      if (response.success && response.data.length > 0) {
        const formattedCategories = response.data.map(cat => ({
          id: cat.categoryId,
          name: cat.categoryName
        }));
        setCategories(formattedCategories);
      }
    } catch (error) {
      console.error("Error fetching categories:", error);
      toast({
        title: "Error",
        description: "Could not load categories",
        variant: "destructive",
      });
    }
  };

  const fetchBooks = async (
    categoryId: string = categoryFilter, 
    pageNum: number = currentPage, 
    pageSizeParam: number = pageSize,
  ) => {
    setIsLoading(true);
    try {
      let response;
      
      if (categoryId !== "all") {
        // Fetch books by category
        response = await BookService.getBooksByCategory(
          categoryId, 
          pageNum, 
          pageSizeParam
        );
      } else {
        // Get available books
        response = await BookService.getAvailableBooks(pageNum, pageSizeParam);
      }
      
      if (response.success) {
        const bookData = response.data || [];
        setBooks(bookData);
        
        // Update filtered books
        if (response.totalCount !== undefined) {
          console.log('Setting total books to:', response.totalCount);
          setTotalBooks(response.totalCount);
        } else {
          // Fallback if totalCount is not available
          setTotalBooks(bookData.length);
        }
      } else {
        toast({
          title: "Error",
          description: response.message || "Could not load books",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error fetching books:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while loading books",
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };
  
  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle page size change
  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };
  
  const handleAddBook = (book: Book) => {
    if (selectedBooks.length >= MAX_BOOKS_PER_REQUEST) {
      toast({
        title: "Limit Reached",
        description: `You can only borrow up to ${MAX_BOOKS_PER_REQUEST} books per request.`,
        variant: "destructive",
      });
      return;
    }
    
    if (selectedBooks.some(selectedBook => selectedBook.bookId === book.bookId)) {
      toast({
        title: "Book Already Selected",
        description: "You've already added this book to your request.",
        variant: "destructive",
      });
      return;
    }
    
    setSelectedBooks([...selectedBooks, book]);
    toast({
      title: "Book Added",
      description: `${book.title} added to your request.`,
    });
  };
  
  const handleRemoveBook = (bookId: number) => {
    setSelectedBooks(selectedBooks.filter(book => book.bookId !== bookId));
  };
  
  const handleSubmitRequest = async () => {
    if (!currentUser) {
      toast({
        title: "Authentication Required",
        description: "Please log in to submit a borrowing request.",
        variant: "destructive",
      });
      return;
    }
    
    if (selectedBooks.length === 0) {
      toast({
        title: "No Books Selected",
        description: "Please select at least one book to borrow.",
        variant: "destructive",
      });
      return;
    }

    if (!notes.trim()) {
      toast({
        title: "Notes Required",
        description: "Please provide a reason for borrowing these books.",
        variant: "destructive",
      });
      return;
    }
    
    if (requestsThisMonth >= MAX_REQUESTS_PER_MONTH) {
      toast({
        title: "Monthly Limit Reached",
        description: `You can only make ${MAX_REQUESTS_PER_MONTH} borrowing requests per month.`,
        variant: "destructive",
      });
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      const requestData: CreateBorrowingRequestDto = {
        requestorId: currentUser.userId,
        books: selectedBooks.map(book => ({ bookId: book.bookId })),
        notes: notes.trim()
      };
      
      const response = await BorrowingService.createBorrowingRequest(requestData);
      
      if (response.success) {
        toast({
          title: "Request Submitted",
          description: "Your borrowing request has been submitted successfully.",
        });
        
        await fetchUserBorrowingRequests();
        
        // Clear selected books and notes
        setSelectedBooks([]);
        setNotes("");
        
        // Refresh available books
        fetchBooks();
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to submit borrowing request.",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error submitting borrowing request:", error);
      toast({
        title: "Error",
        description: "An unexpected error occurred while submitting your request.",
        variant: "destructive",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight dark:text-white">Borrow Books</h2>
        <p className="text-muted-foreground dark:text-gray-400">
          Select books you want to borrow from the library
        </p>
      </div>
      
      <Tabs defaultValue="browse">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="browse">Browse Books</TabsTrigger>
          <TabsTrigger value="requests">My Requests</TabsTrigger>
        </TabsList>
        
        <TabsContent value="browse" className="mt-4">
          <div className="grid gap-6 md:grid-cols-[1fr_350px]">
            <div>
              <BookCatalog 
                books={books}
                categories={categories}
                onAddBook={handleAddBook}
                onFetchBooks={fetchBooks}
                isLoading={isLoading}
                selectedBooks={selectedBooks}
                maxBooksPerRequest={MAX_BOOKS_PER_REQUEST}
                currentPage={currentPage}
                totalBooks={totalBooks}
                pageSize={pageSize}
                onPageChange={handlePageChange}
                onPageSizeChange={handlePageSizeChange}
              />
            </div>
            
            <div className="space-y-6">
              <BorrowingRequestForm
                selectedBooks={selectedBooks}
                onRemoveBook={handleRemoveBook}
                notes={notes}
                setNotes={setNotes}
                onSubmit={handleSubmitRequest}
                isSubmitting={isSubmitting}
                requestsThisMonth={requestsThisMonth}
                maxBooksPerRequest={MAX_BOOKS_PER_REQUEST}
                maxRequestsPerMonth={MAX_REQUESTS_PER_MONTH}
              />
              
              <BorrowingRules
                maxBooksPerRequest={MAX_BOOKS_PER_REQUEST}
                maxRequestsPerMonth={MAX_REQUESTS_PER_MONTH}
              />
            </div>
          </div>
        </TabsContent>
        
        <TabsContent value="requests" className="mt-4">
          <Card className="dark:border-gray-700">
            <CardHeader>
              <CardTitle className="dark:text-white">My Borrowing Requests</CardTitle>
              <CardDescription className="dark:text-gray-400">
                Track the status of your borrowing requests
              </CardDescription>
            </CardHeader>
            <CardContent>
              <BorrowingRequestsList
                requests={borrowingRequests}
                isLoading={isLoading}
              />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default BorrowBooksPage;
