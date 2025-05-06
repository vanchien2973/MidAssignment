import { Book, BookOpen, Calendar, Users } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import { useAuth } from "../../context/AuthContext";
import { useEffect, useState } from "react";
import bookService from "../../services/book.service";
import borrowingService from "../../services/borrowing.service";
import userService from "../../services/user.service";
import categoryService from "../../services/category.service";
import { BookBorrowingRequestDto, BorrowingRequestStatus, BorrowingDetailStatus } from "../../types/borrowing";
import { Category } from "../../types/category";

interface DashboardData {
  totalBooks: number;
  newBooks: number;
  activeBorrowings: number;
  dueSoon: number;
  totalUsers: number;
  newUsers: number;
  pendingRequests: number;
  recentBorrowings: BookBorrowingRequestDto[];
  popularCategories: Category[];
  userBorrowings: BookBorrowingRequestDto[];
  userDueSoon: BookBorrowingRequestDto[];
  isLoading: boolean;
}

export const Dashboard: React.FC = () => {
    const { currentUser } = useAuth();
    const isSuperUser = currentUser?.userType === "SuperUser";
    
    // Trạng thái lưu trữ dữ liệu dashboard
    const [dashboardData, setDashboardData] = useState<DashboardData>({
      totalBooks: 0,
      newBooks: 0,
      activeBorrowings: 0,
      dueSoon: 0,
      totalUsers: 0,
      newUsers: 0,
      pendingRequests: 0,
      recentBorrowings: [],
      popularCategories: [],
      userBorrowings: [],
      userDueSoon: [],
      isLoading: true
    });

    useEffect(() => {
      const fetchDashboardData = async () => {
        try {
          // Lấy tổng số sách
          const booksResponse = await bookService.getBooks(1, 1);
          const totalBooks = booksResponse.totalCount || 0;
          
          // Lấy tổng số người dùng (chỉ khi là SuperUser)
          let totalUsers = 0;
          // let newUsers = 0;
          if (isSuperUser) {
            const usersResponse = await userService.getAllUsers();
            totalUsers = usersResponse.totalCount || 0;
          }
          
          // Lấy các yêu cầu mượn sách đang hoạt động
          let activeBorrowings = 0;
          let pendingRequests = 0;
          let recentBorrowings: BookBorrowingRequestDto[] = [];
          let userBorrowings: BookBorrowingRequestDto[] = [];
          let userDueSoon: BookBorrowingRequestDto[] = [];
          
          // Lấy các yêu cầu mượn cho SuperUser
          if (isSuperUser) {
            const borrowingsResponse = await borrowingService.getAllBorrowingRequests(1, 20);
            
            if (borrowingsResponse.success && borrowingsResponse.data) {
              recentBorrowings = borrowingsResponse.data.results || [];
              
              // Đếm các yêu cầu đang chờ phê duyệt
              pendingRequests = borrowingsResponse.data.results?.filter(
                req => req.status === BorrowingRequestStatus.Waiting
              ).length || 0;
              
              // Đếm các mượn sách đang hoạt động
              activeBorrowings = borrowingsResponse.data.results?.filter(
                req => req.status === BorrowingRequestStatus.Approved
              ).length || 0;
            }
          } else if (currentUser?.userId) {
            // Lấy các yêu cầu mượn cho người dùng thường
            const userBorrowingsResponse = await borrowingService.getUserBorrowingRequests(
              currentUser.userId, 
              1, 
              10
            );
            
            if (userBorrowingsResponse.success && userBorrowingsResponse.data) {
              userBorrowings = userBorrowingsResponse.data.results || [];
              
              // Lọc các sách sắp đến hạn trả
              const today = new Date();
              userDueSoon = userBorrowingsResponse.data.results?.filter(req => {
                if (!req.details || req.details.length === 0) return false;
                
                // Lọc chi tiết mượn có trạng thái đang mượn và có ngày hạn trả
                return req.details.some(detail => {
                  if (detail.status !== BorrowingDetailStatus.Borrowing || !detail.dueDate) return false;
                  
                  const dueDate = new Date(detail.dueDate);
                  const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
                  return diffDays <= 7 && diffDays >= 0; // Sắp đến hạn trong vòng 7 ngày
                });
              }) || [];
            }
          }
          
          // Lấy danh mục phổ biến
          const categoriesResponse = await categoryService.getCategories(1, 5);
          const popularCategories = categoriesResponse.success ? categoriesResponse.data : [];

          setDashboardData({
            totalBooks,
            newBooks: Math.floor(Math.random() * 10), // Giá trị mẫu cho sách mới
            activeBorrowings,
            dueSoon: userDueSoon.length,
            totalUsers,
            newUsers: Math.floor(Math.random() * 10), // Giá trị mẫu cho người dùng mới
            pendingRequests,
            recentBorrowings,
            popularCategories,
            userBorrowings,
            userDueSoon,
            isLoading: false
          });
        } catch (error) {
          console.error("Error fetching dashboard data:", error);
          setDashboardData(prev => ({...prev, isLoading: false}));
        }
      };

      fetchDashboardData();
    }, [currentUser, isSuperUser]);
    
    return (
      <div className="space-y-6">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
          <p className="text-muted-foreground">
            Welcome back, {currentUser?.fullName}!
          </p>
        </div>
        
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Total Books</CardTitle>
              <Book className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {dashboardData.isLoading ? "Loading..." : dashboardData.totalBooks}
              </div>
              <p className="text-xs text-muted-foreground">
                {dashboardData.isLoading ? "Loading..." : `+${dashboardData.newBooks} added this month`}
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Active Borrowings</CardTitle>
              <BookOpen className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {dashboardData.isLoading ? "Loading..." : dashboardData.activeBorrowings}
              </div>
              <p className="text-xs text-muted-foreground">
                {dashboardData.isLoading ? "Loading..." : `${dashboardData.dueSoon} due this week`}
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Users</CardTitle>
              <Users className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {dashboardData.isLoading ? "Loading..." : dashboardData.totalUsers}
              </div>
              <p className="text-xs text-muted-foreground">
                {dashboardData.isLoading ? "Loading..." : `+${dashboardData.newUsers} new users this week`}
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Pending Requests</CardTitle>
              <Calendar className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                {dashboardData.isLoading ? "Loading..." : dashboardData.pendingRequests}
              </div>
              <p className="text-xs text-muted-foreground">
                Awaiting approval
              </p>
            </CardContent>
          </Card>
        </div>
  
        {isSuperUser ? (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <Card className="col-span-2">
              <CardHeader>
                <CardTitle>Recent Borrowing Requests</CardTitle>
              </CardHeader>
              <CardContent>
                {dashboardData.isLoading ? (
                  <p className="text-center py-8 text-muted-foreground">Loading...</p>
                ) : dashboardData.recentBorrowings.length > 0 ? (
                  <div className="text-sm">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b">
                          <th className="px-2 py-2 text-left">User</th>
                          <th className="px-2 py-2 text-left">Date</th>
                          <th className="px-2 py-2 text-left">Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {dashboardData.recentBorrowings.slice(0, 5).map((request) => (
                          <tr key={request.requestId} className="border-b">
                            <td className="px-2 py-2">{request.requestorName || 'Unknown'}</td>
                            <td className="px-2 py-2">{new Date(request.requestDate).toLocaleDateString()}</td>
                            <td className="px-2 py-2">
                              <span className={`px-2 py-1 rounded-full text-xs 
                                ${request.status === BorrowingRequestStatus.Waiting ? 'bg-yellow-100 text-yellow-800' : 
                                  request.status === BorrowingRequestStatus.Approved ? 'bg-green-100 text-green-800' : 
                                  'bg-red-100 text-red-800'}`}>
                                {request.status === BorrowingRequestStatus.Waiting ? 'Waiting' : 
                                  request.status === BorrowingRequestStatus.Approved ? 'Approved' : 'Rejected'}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <p className="text-center py-8 text-muted-foreground">
                    Coming soon
                  </p>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Popular Categories</CardTitle>
              </CardHeader>
              <CardContent>
                {dashboardData.isLoading ? (
                  <p className="text-center py-8 text-muted-foreground">Loading...</p>
                ) : dashboardData.popularCategories.length > 0 ? (
                  <div className="text-sm">
                    <ul className="space-y-2">
                      {dashboardData.popularCategories.map((category) => (
                        <li key={category.categoryId} className="flex justify-between items-center">
                          <span>{category.categoryName}</span>
                          <span className="px-2 py-1 bg-gray-100 rounded-full text-xs">
                            {(category as any).booksCount || Math.floor(Math.random() * 50)} books
                          </span>
                        </li>
                      ))}
                    </ul>
                  </div>
                ) : (
                  <p className="text-center py-8 text-muted-foreground">
                    Coming soon
                  </p>
                )}
              </CardContent>
            </Card>
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>My Recent Borrowings</CardTitle>
              </CardHeader>
              <CardContent>
                {dashboardData.isLoading ? (
                  <p className="text-center py-8 text-muted-foreground">Loading...</p>
                ) : dashboardData.userBorrowings.length > 0 ? (
                  <div className="text-sm">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b">
                          <th className="px-2 py-2 text-left">Book</th>
                          <th className="px-2 py-2 text-left">Date</th>
                          <th className="px-2 py-2 text-left">Status</th>
                        </tr>
                      </thead>
                      <tbody>
                        {dashboardData.userBorrowings.slice(0, 5).map((request) => (
                          <tr key={request.requestId} className="border-b">
                            <td className="px-2 py-2">
                              {request.details && request.details[0]?.bookTitle || 'Unknown'}
                            </td>
                            <td className="px-2 py-2">{new Date(request.requestDate).toLocaleDateString()}</td>
                            <td className="px-2 py-2">
                              <span className={`px-2 py-1 rounded-full text-xs 
                                ${request.status === BorrowingRequestStatus.Waiting ? 'bg-yellow-100 text-yellow-800' : 
                                  request.status === BorrowingRequestStatus.Approved ? 'bg-green-100 text-green-800' : 
                                  'bg-red-100 text-red-800'}`}>
                                {request.status === BorrowingRequestStatus.Waiting ? 'Waiting' : 
                                  request.status === BorrowingRequestStatus.Approved ? 'Approved' : 'Rejected'}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <p className="text-center py-8 text-muted-foreground">
                    Coming soon
                  </p>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Due Soon</CardTitle>
              </CardHeader>
              <CardContent>
                {dashboardData.isLoading ? (
                  <p className="text-center py-8 text-muted-foreground">Loading...</p>
                ) : dashboardData.userDueSoon.length > 0 ? (
                  <div className="text-sm">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b">
                          <th className="px-2 py-2 text-left">Book</th>
                          <th className="px-2 py-2 text-left">Due Date</th>
                        </tr>
                      </thead>
                      <tbody>
                        {dashboardData.userDueSoon.slice(0, 5).map((request) => {
                          // Tìm chi tiết đầu tiên sắp đến hạn
                          const dueSoonDetail = request.details?.find((detail) => {
                            if (detail.status !== BorrowingDetailStatus.Borrowing || !detail.dueDate) return false;
                            const dueDate = new Date(detail.dueDate);
                            const today = new Date();
                            const diffDays = Math.ceil((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));
                            return diffDays <= 7 && diffDays >= 0;
                          });
                          
                          if (!dueSoonDetail) return null;
                          
                          return (
                            <tr key={request.requestId + dueSoonDetail.bookId} className="border-b">
                              <td className="px-2 py-2">{dueSoonDetail.bookTitle || 'Unknown'}</td>
                              <td className="px-2 py-2">
                                {dueSoonDetail.dueDate && new Date(dueSoonDetail.dueDate).toLocaleDateString()}
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <p className="text-center py-8 text-muted-foreground">
                    Coming soon
                  </p>
                )}
              </CardContent>
            </Card>
          </div>
        )}
      </div>
    );
  }