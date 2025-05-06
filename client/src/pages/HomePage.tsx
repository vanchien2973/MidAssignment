import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { 
  Card, 
  CardContent, 
  CardHeader, 
  CardTitle, 
  CardDescription 
} from "../components/ui/card";
import { Button } from "../components/ui/button";
import { 
  BookOpen, 
  Book, 
  Calendar, 
  Clock, 
  Search,
  Users
} from "lucide-react";
import { useEffect, useState } from "react";
import bookService from "../services/book.service";
import borrowingService from "../services/borrowing.service";
import { BookBorrowingRequestDto, BorrowingDetailStatus, BorrowingRequestStatus } from "../types/borrowing";

interface HomePageData {
  maxRequestsPerMonth: number;
  requestsThisMonth: number;
  activeBorrowings: number;
  availableBooks: number;
  userBorrowings: BookBorrowingRequestDto[];
  userDueSoon: BookBorrowingRequestDto[];
  isLoading: boolean;
}

export default function HomePage() {
  const { currentUser } = useAuth();
  const navigate = useNavigate();
  
  const [homeData, setHomeData] = useState<HomePageData>({
    maxRequestsPerMonth: 3,
    requestsThisMonth: 0,
    activeBorrowings: 0,
    pendingRequests: 0,
    availableBooks: 0,
    userBorrowings: [],
    userDueSoon: [],
    isLoading: true
  });

  useEffect(() => {
    const fetchHomeData = async () => {
      try {
        if (!currentUser || !currentUser.userId) {
          setHomeData(prev => ({ ...prev, isLoading: false }));
          return;
        }

        // Lấy số lượng sách có sẵn
        const booksResponse = await bookService.getAvailableBooks(1, 1);
        const availableBooks = booksResponse.totalCount || 0;
        
        // Lấy các yêu cầu mượn sách của người dùng
        let userBorrowings: BookBorrowingRequestDto[] = [];
        let activeBorrowings = 0;
        let userDueSoon: BookBorrowingRequestDto[] = [];
        
        const userBorrowingsResponse = await borrowingService.getUserBorrowingRequests(
          currentUser.userId, 
          1, 
          20
        );
        
        if (userBorrowingsResponse.success && userBorrowingsResponse.data) {
          userBorrowings = userBorrowingsResponse.data.results || [];
          
          // Đếm số yêu cầu mượn trong tháng này
          const today = new Date();
          const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
          const requestsThisMonth = userBorrowings.filter(req => {
            const requestDate = new Date(req.requestDate);
            return requestDate >= startOfMonth;
          }).length;
          
          // Đếm số mượn sách đang hoạt động
          activeBorrowings = userBorrowings.filter(req => 
            req.status === BorrowingRequestStatus.Approved && req.details?.some(
              detail => detail.status === BorrowingDetailStatus.Borrowing
            )
          ).length;
          
          // Lọc các sách sắp đến hạn trả
          userDueSoon = userBorrowings.filter(req => {
            if (!req.details || req.details.length === 0) return false;
            
            return req.details.some(detail => {
              if (detail.status !== BorrowingDetailStatus.Borrowing || !detail.dueDate) return false;
              
              const dueDate = new Date(detail.dueDate);
              const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
              return diffDays <= 7 && diffDays >= 0; // Sắp đến hạn trong vòng 7 ngày
            });
          });
          
          setHomeData({
            maxRequestsPerMonth: 3,
            requestsThisMonth,
            activeBorrowings,
            availableBooks,
            userBorrowings: userBorrowings.slice(0, 5),
            userDueSoon,
            isLoading: false
          });
        }
      } catch (error) {
        console.error("Error fetching home data:", error);
        setHomeData(prev => ({ ...prev, isLoading: false }));
      }
    };

    fetchHomeData();
  }, [currentUser]);

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight dark:text-white">Welcome</h2>
        <p className="text-muted-foreground dark:text-gray-400">
          Hello, {currentUser?.fullName}! Here's an overview of your library activity.
        </p>
      </div>
      
      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card className="dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium dark:text-white">Borrowing Requests</CardTitle>
            <Calendar className="w-4 h-4 text-muted-foreground dark:text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold dark:text-white">
              {homeData.isLoading ? "Loading..." : `${homeData.requestsThisMonth}/${homeData.maxRequestsPerMonth}`}
            </div>
            <p className="text-xs text-muted-foreground dark:text-gray-400">
              This month
            </p>
            <Button 
              onClick={() => navigate("/borrow")} 
              className="w-full mt-4"
              variant="outline"
              size="sm"
            >
              Borrow Books
            </Button>
          </CardContent>
        </Card>
        
        <Card className="dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium dark:text-white">Active Borrowings</CardTitle>
            <BookOpen className="w-4 h-4 text-muted-foreground dark:text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold dark:text-white">
              {homeData.isLoading ? "Loading..." : homeData.activeBorrowings}
            </div>
            <p className="text-xs text-muted-foreground dark:text-gray-400">
              Currently borrowed
            </p>
            <Button 
              onClick={() => navigate("/my-books")} 
              className="w-full mt-4"
              variant="outline"
              size="sm"
            >
              View My Borrowing Books
            </Button>
          </CardContent>
        </Card>
        
        <Card className="dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium dark:text-white">Available Books</CardTitle>
            <Book className="w-4 h-4 text-muted-foreground dark:text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold dark:text-white">
              {homeData.isLoading ? "Loading..." : homeData.availableBooks}
            </div>
            <p className="text-xs text-muted-foreground dark:text-gray-400">
              Ready to borrow
            </p>
            <Button 
              onClick={() => navigate("/borrow")} 
              className="w-full mt-4"
              variant="outline"
              size="sm"
            >
              Browse Catalog
            </Button>
          </CardContent>
        </Card>
      </div>
      
      {/* User Content */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card className="dark:border-gray-700">
          <CardHeader>
            <CardTitle className="dark:text-white">My Recent Borrowings</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {homeData.isLoading ? (
                <div className="flex flex-col items-center justify-center py-6">
                  <p className="text-center text-muted-foreground dark:text-gray-400">
                    Loading...
                  </p>
                </div>
              ) : homeData.userBorrowings.length > 0 ? (
                <>
                  {homeData.userBorrowings.map(borrowing => {
                    // Lấy chi tiết đầu tiên
                    const detail = borrowing.details && borrowing.details.length > 0 
                      ? borrowing.details[0] 
                      : null;
                    
                    if (!detail) return null;
                    
                    return (
                      <div key={borrowing.requestId} className="flex items-start space-x-3">
                        <BookOpen className="h-5 w-5 mt-0.5 text-muted-foreground dark:text-gray-400" />
                        <div>
                          <div className="font-medium dark:text-white">{detail.bookTitle || 'Unknown book'}</div>
                          <div className="text-sm text-muted-foreground dark:text-gray-400">
                            Due: {detail.dueDate ? new Date(detail.dueDate).toLocaleDateString() : 'N/A'}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center py-6">
                  <Search className="h-10 w-10 text-muted-foreground dark:text-gray-400 mb-2" />
                  <p className="text-center text-muted-foreground dark:text-gray-400">
                    You don't have any active borrowings
                  </p>
                </div>
              )}
              
              <Button 
                onClick={() => navigate("/my-books")} 
                className="w-full mt-2"
                variant="outline"
              >
                View All Borrowings
              </Button>
            </div>
          </CardContent>
        </Card>
        
        <Card className="dark:border-gray-700">
          <CardHeader>
            <CardTitle className="dark:text-white">Books Due Soon</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {homeData.isLoading ? (
                <div className="flex flex-col items-center justify-center py-6">
                  <p className="text-center text-muted-foreground dark:text-gray-400">
                    Loading...
                  </p>
                </div>
              ) : homeData.userDueSoon.length > 0 ? (
                <>
                  {homeData.userDueSoon.map(borrowing => {
                    const dueSoonDetail = borrowing.details?.find(detail => {
                      if (detail.status !== BorrowingDetailStatus.Borrowing || !detail.dueDate) return false;
                      
                      const dueDate = new Date(detail.dueDate);
                      const today = new Date();
                      const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
                      return diffDays <= 7 && diffDays >= 0;
                    });
                    
                    if (!dueSoonDetail) return null;
                    
                    const dueDate = new Date(dueSoonDetail.dueDate || '');
                    const today = new Date();
                    const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
                    
                    return (
                      <div key={borrowing.requestId + dueSoonDetail.bookId} className="flex items-start space-x-3">
                        <Clock className="h-5 w-5 mt-0.5 text-orange-500 dark:text-orange-400" />
                        <div>
                          <div className="font-medium dark:text-white">{dueSoonDetail.bookTitle || 'Unknown'}</div>
                          <div className="text-sm text-orange-500 dark:text-orange-400">
                            {diffDays === 0 ? 'Due today' : `Due in ${diffDays} day${diffDays > 1 ? 's' : ''}`}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center py-6">
                  <Calendar className="h-10 w-10 text-muted-foreground dark:text-gray-400 mb-2" />
                  <p className="text-center text-muted-foreground dark:text-gray-400">
                    No books due soon
                  </p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
      
      {/* Featured Section */}
      <Card className="dark:border-gray-700">
        <CardHeader>
          <CardTitle className="dark:text-white">Library Information</CardTitle>
          <CardDescription className="dark:text-gray-400">
            Important information about your library services
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="flex flex-col items-center justify-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <Users className="h-8 w-8 text-muted-foreground dark:text-gray-400 mb-2" />
              <h3 className="font-medium text-center dark:text-white">Book Limit</h3>
              <p className="text-sm text-center text-muted-foreground dark:text-gray-400">
                Borrow up to 5 books at once
              </p>
            </div>
            
            <div className="flex flex-col items-center justify-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <Calendar className="h-8 w-8 text-muted-foreground dark:text-gray-400 mb-2" />
              <h3 className="font-medium text-center dark:text-white">Request Limit</h3>
              <p className="text-sm text-center text-muted-foreground dark:text-gray-400">
                3 borrowing requests per month
              </p>
            </div>
            
            <div className="flex flex-col items-center justify-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
              <Clock className="h-8 w-8 text-muted-foreground dark:text-gray-400 mb-2" />
              <h3 className="font-medium text-center dark:text-white">Loan Period</h3>
              <p className="text-sm text-center text-muted-foreground dark:text-gray-400">
                Standard loan period is 30 days
              </p>
            </div>
          </div>
          
          <div className="pt-2">
            <Button onClick={() => navigate("/borrow")} className="w-full sm:w-auto">
              Browse Books
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}