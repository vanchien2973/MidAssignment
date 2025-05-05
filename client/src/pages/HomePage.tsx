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

export default function HomePage() {
  const { currentUser } = useAuth();
  const navigate = useNavigate();
  
  // Stats for cards - would be from API in real app
  const stats = {
    requestsThisMonth: 1,
    maxRequestsPerMonth: 3,
    activeBorrowings: 2,
    pendingRequests: 1,
    availableBooks: 254
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight dark:text-white">Welcome</h2>
        <p className="text-muted-foreground dark:text-gray-400">
          Hello, {currentUser?.fullName}! Here's an overview of your library activity.
        </p>
      </div>
      
      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium dark:text-white">Borrowing Requests</CardTitle>
            <Calendar className="w-4 h-4 text-muted-foreground dark:text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold dark:text-white">{stats.requestsThisMonth}/{stats.maxRequestsPerMonth}</div>
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
            <div className="text-2xl font-bold dark:text-white">{stats.activeBorrowings}</div>
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
            <CardTitle className="text-sm font-medium dark:text-white">Pending Requests</CardTitle>
            <Clock className="w-4 h-4 text-muted-foreground dark:text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold dark:text-white">{stats.pendingRequests}</div>
            <p className="text-xs text-muted-foreground dark:text-gray-400">
              Awaiting approval
            </p>
            <Button 
              onClick={() => navigate("/history")} 
              className="w-full mt-4"
              variant="outline"
              size="sm"
            >
              View History
            </Button>
          </CardContent>
        </Card>
        
        <Card className="dark:border-gray-700">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium dark:text-white">Available Books</CardTitle>
            <Book className="w-4 h-4 text-muted-foreground dark:text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold dark:text-white">{stats.availableBooks}</div>
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
              {stats.activeBorrowings > 0 ? (
                <>
                  <div className="flex items-start space-x-3">
                    <BookOpen className="h-5 w-5 mt-0.5 text-muted-foreground dark:text-gray-400" />
                    <div>
                      <div className="font-medium dark:text-white">To Kill a Mockingbird</div>
                      <div className="text-sm text-muted-foreground dark:text-gray-400">Due: May 15, 2023</div>
                    </div>
                  </div>
                  <div className="flex items-start space-x-3">
                    <BookOpen className="h-5 w-5 mt-0.5 text-muted-foreground dark:text-gray-400" />
                    <div>
                      <div className="font-medium dark:text-white">1984</div>
                      <div className="text-sm text-muted-foreground dark:text-gray-400">Due: May 15, 2023</div>
                    </div>
                  </div>
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
              {stats.activeBorrowings > 0 ? (
                <>
                  <div className="flex items-start space-x-3">
                    <Clock className="h-5 w-5 mt-0.5 text-orange-500 dark:text-orange-400" />
                    <div>
                      <div className="font-medium dark:text-white">To Kill a Mockingbird</div>
                      <div className="text-sm text-orange-500 dark:text-orange-400">Due in 7 days</div>
                    </div>
                  </div>
                  <div className="flex items-start space-x-3">
                    <Clock className="h-5 w-5 mt-0.5 text-orange-500 dark:text-orange-400" />
                    <div>
                      <div className="font-medium dark:text-white">1984</div>
                      <div className="text-sm text-orange-500 dark:text-orange-400">Due in 7 days</div>
                    </div>
                  </div>
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