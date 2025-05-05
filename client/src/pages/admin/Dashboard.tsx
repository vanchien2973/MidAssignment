import { Book, BookOpen, Calendar, Users } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";
import { useAuth } from "../../context/AuthContext";

export const Dashboard: React.FC = () => {
    const { currentUser } = useAuth();
    const isSuperUser = currentUser?.userType === "SuperUser";
    
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
              <div className="text-2xl font-bold">254</div>
              <p className="text-xs text-muted-foreground">
                +4 added this month
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Active Borrowings</CardTitle>
              <BookOpen className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">45</div>
              <p className="text-xs text-muted-foreground">
                12 due this week
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Users</CardTitle>
              <Users className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">121</div>
              <p className="text-xs text-muted-foreground">
                +5 new users this week
              </p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
              <CardTitle className="text-sm font-medium">Pending Requests</CardTitle>
              <Calendar className="w-4 h-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">8</div>
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
                <div className="text-sm">
                  <p className="text-center py-8 text-muted-foreground">
                    Recent borrowing requests will be displayed here.
                  </p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Popular Categories</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-sm">
                  <p className="text-center py-8 text-muted-foreground">
                    Top book categories will be displayed here.
                  </p>
                </div>
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
                <div className="text-sm">
                  <p className="text-center py-8 text-muted-foreground">
                    Your recent borrowings will be displayed here.
                  </p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle>Due Soon</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-sm">
                  <p className="text-center py-8 text-muted-foreground">
                    Books due for return soon will be displayed here.
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>
        )}
      </div>
    );
  }