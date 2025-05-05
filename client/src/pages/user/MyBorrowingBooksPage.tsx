import { useState, useEffect } from "react";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../hooks/use-toast";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "../../components/ui/card";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger
} from "../../components/ui/tabs";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "../../components/ui/table";
import { Button } from "../../components/ui/button";
import { Badge } from "../../components/ui/badge";
import { BookOpen, Clock, CalendarCheck, X, Loader2 } from "lucide-react";
import { 
  BookBorrowingRequestDto, 
  BorrowingRequestStatus, 
  BorrowingDetailStatus,
  ExtendBorrowingDto,
  ReturnBookDto,
  // BookBorrowingRequestDetailDto
} from "../../types/borrowing";
import BorrowingService from "../../services/borrowing.service";
import { format, isAfter, addDays } from "date-fns";
import { getBorrowingRequestStatusName, getBorrowingDetailStatusName, getBadgeVariant } from "../../utils/enumUtils";

const MyBorrowingBooksPage = () => {
  const { currentUser } = useAuth();
  const { toast } = useToast();
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [activeTab, setActiveTab] = useState<string>("current");
  const [currentBorrowings, setCurrentBorrowings] = useState<BookBorrowingRequestDto[]>([]);
  const [pendingRequests, setPendingRequests] = useState<BookBorrowingRequestDto[]>([]);
  const [historyBorrowings, setHistoryBorrowings] = useState<BookBorrowingRequestDto[]>([]);

  useEffect(() => {
    if (currentUser?.userId) {
      loadUserBorrowingData();
    }
  }, [currentUser]);

  const loadUserBorrowingData = async () => {
    if (!currentUser?.userId) return;
    
    setIsLoading(true);
    try {
      const response = await BorrowingService.getUserBorrowingRequests(currentUser.userId, 1, 100);
      let allRequests: BookBorrowingRequestDto[] = [];
      
      if (response.success) {
        if (response.data?.results && Array.isArray(response.data.results)) {
          allRequests = response.data.results;
        } else if (Array.isArray(response.data)) {
          allRequests = response.data;
        } else if (response.data) {
          const dataObj = response.data;
          if (dataObj.results && Array.isArray(dataObj.results)) {
            allRequests = dataObj.results;
          } else {
            console.warn("Unexpected data format:", response.data);
            allRequests = [];
          }
        }
        
        
        // // Kiểm tra tính hợp lệ của dữ liệu và biến đổi nếu cần
        // allRequests = allRequests.filter(req => 
        //   req && typeof req === 'object'
        // ).map(req => {
        //   // Đảm bảo thuộc tính details tồn tại và là mảng
        //   if (!req.details || !Array.isArray(req.details)) {
        //     // Truy cập các thuộc tính một cách an toàn - dùng ép kiểu để tránh lỗi linter
        //     const rawData = req as any;
        //     req.details = Array.isArray(rawData.requestDetails) 
        //       ? rawData.requestDetails 
        //       : [];
        //   }
        //   return req;
        // });
        
        // Pending requests
        const pendingReqs = allRequests.filter(req => 
          req.status === BorrowingRequestStatus.Waiting
        );
        setPendingRequests(pendingReqs);
        
        // Approved requests with active borrowings
        const activeReqs = allRequests.filter(req => 
          req.status === BorrowingRequestStatus.Approved && 
          req.details.some(detail => detail.returnDate === null)
        );
        setCurrentBorrowings(activeReqs);
        
        // History - returned books
        const historyReqs = allRequests.filter(req => 
          req.status === BorrowingRequestStatus.Approved && 
          req.details.every(detail => detail.returnDate !== null)
        );
        setHistoryBorrowings(historyReqs);
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to load borrowing data",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error loading borrowing data:", error);
      toast({
        title: "Error",
        description: "Failed to load borrowing data",
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateString: string | null | undefined): string => {
    if (!dateString) return "N/A";
    try {
      return format(new Date(dateString), "dd/MM/yyyy");
    } catch (error) {
      console.error("Date format error:", error);
      return "Invalid date";
    }
  };

  const isOverdue = (dueDate: string | null | undefined): boolean => {
    if (!dueDate) return false;
    try {
      return isAfter(new Date(), new Date(dueDate));
    } catch (error) {
      console.error("Date comparison error:", error);
      return false;
    }
  };

  const handleExtensionRequest = async (detailId: string) => {
    if (!currentUser?.userId) return;
    
    setIsSubmitting(true);
    try {
      // Calculate new due date (7 days from now)
      const newDueDate = format(addDays(new Date(), 7), "yyyy-MM-dd'T'HH:mm:ss");
      
      const extensionData: ExtendBorrowingDto = {
        detailId: detailId,
        userId: currentUser.userId,
        newDueDate: newDueDate
      };
      
      const response = await BorrowingService.extendBorrowing(extensionData);
      
      if (response.success) {
        toast({
          title: "Success",
          description: "Borrowing period extended successfully",
        });
        await loadUserBorrowingData();
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to extend borrowing period",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error extending borrowing:", error);
      toast({
        title: "Error",
        description: "Failed to extend borrowing period",
        variant: "destructive",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleReturnBook = async (detailId: string) => {
    if (!currentUser?.userId) return;
    
    setIsSubmitting(true);
    try {
      const returnData: ReturnBookDto = {
        detailId: detailId,
        userId: currentUser.userId
      };
      
      const response = await BorrowingService.returnBook(returnData);
      
      if (response.success) {
        toast({
          title: "Success",
          description: "Book returned successfully",
        });
        await loadUserBorrowingData();
      } else {
        toast({
          title: "Error",
          description: response.message || "Failed to return book",
          variant: "destructive",
        });
      }
    } catch (error) {
      console.error("Error returning book:", error);
      toast({
        title: "Error",
        description: "Failed to return book",
        variant: "destructive",
      });
    } finally {
      setIsSubmitting(false);
    }
  };
  
  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex flex-col gap-2">
        <h1 className="text-3xl font-bold tracking-tight">My Borrowed Books</h1>
        <p className="text-muted-foreground">
          Manage your borrowed books and track your borrowing history
        </p>
      </div>

      {isLoading ? (
        <div className="flex justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : (
        <Tabs 
          defaultValue="current" 
          value={activeTab} 
          onValueChange={setActiveTab}
          className="w-full"
        >
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="current">Currently Borrowed</TabsTrigger>
            <TabsTrigger value="pending">Pending Approval</TabsTrigger>
            <TabsTrigger value="history">History</TabsTrigger>
          </TabsList>

          <TabsContent value="current" className="space-y-4 mt-4">
            <Card>
              <CardHeader>
                <CardTitle>Currently Borrowed Books</CardTitle>
                <CardDescription>
                  List of books you are currently borrowing and their due dates
                </CardDescription>
              </CardHeader>
              <CardContent>
                {currentBorrowings.length > 0 ? (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Book Title</TableHead>
                        <TableHead>Borrow Date</TableHead>
                        <TableHead>Due Date</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead>Actions</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {currentBorrowings.flatMap(request => 
                        Array.isArray(request.details) ? 
                          request.details
                            .filter(detail => detail && detail.returnDate === null)
                            .map(detail => (
                              <TableRow key={detail.detailId}>
                                <TableCell className="font-medium">{detail.bookTitle}</TableCell>
                                <TableCell>{formatDate(request.approvalDate)}</TableCell>
                                <TableCell>
                                  <span className={isOverdue(detail.dueDate) ? "text-red-500 font-bold" : ""}>
                                    {formatDate(detail.dueDate)}
                                    {isOverdue(detail.dueDate) && " (Overdue)"}
                                  </span>
                                </TableCell>
                                <TableCell>
                                  <Badge variant={getBadgeVariant(detail.status)}>
                                    {getBorrowingDetailStatusName(detail.status)}
                                  </Badge>
                                </TableCell>
                                <TableCell className="space-x-2">
                                  <Button 
                                    variant="outline" 
                                    size="sm"
                                    onClick={() => handleExtensionRequest(detail.detailId)}
                                    disabled={
                                      detail.status === BorrowingDetailStatus.Extended || 
                                      isSubmitting
                                    }
                                  >
                                    <Clock className="h-4 w-4 mr-1" />
                                    Extend
                                  </Button>
                                  <Button 
                                    variant="outline" 
                                    size="sm"
                                    onClick={() => handleReturnBook(detail.detailId)}
                                    disabled={isSubmitting}
                                  >
                                    <BookOpen className="h-4 w-4 mr-1" />
                                    Return
                                  </Button>
                                </TableCell>
                              </TableRow>
                            ))
                          : []
                      )}
                    </TableBody>
                  </Table>
                ) : (
                  <div className="text-center py-6 text-muted-foreground">
                    <BookOpen className="h-10 w-10 mx-auto mb-2 opacity-50" />
                    <p>You haven't borrowed any books yet.</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="pending" className="space-y-4 mt-4">
            <Card>
              <CardHeader>
                <CardTitle>Borrowing Requests</CardTitle>
                <CardDescription>
                  Pending book borrowing requests
                </CardDescription>
              </CardHeader>
              <CardContent>
                {pendingRequests.length > 0 ? (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Request ID</TableHead>
                        <TableHead>Request Date</TableHead>
                        <TableHead>Requested Books</TableHead>
                        <TableHead>Status</TableHead>
                        <TableHead>Action</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {pendingRequests.map(request => (
                        <TableRow key={request.requestId}>
                          <TableCell className="font-medium">#{request.requestId?.substring(0, 8) || 'N/A'}</TableCell>
                          <TableCell>{formatDate(request.requestDate)}</TableCell>
                          <TableCell>
                            {Array.isArray(request.details) ? 
                              request.details.map(detail => detail.bookTitle).join(", ") :
                              'Unknown books'
                            }
                          </TableCell>
                          <TableCell>
                            <Badge variant={getBadgeVariant(request.status)}>
                              {getBorrowingRequestStatusName(request.status)}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <Button 
                              variant="outline" 
                              size="sm"
                              onClick={() => {
                                // Handle cancel request (not implemented in API yet)
                                toast({
                                  title: "Not Implemented",
                                  description: "Request cancellation is not yet implemented.",
                                  variant: "destructive",
                                });
                              }}
                            >
                              <X className="h-4 w-4 mr-1" />
                              Cancel
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                ) : (
                  <div className="text-center py-6 text-muted-foreground">
                    <Clock className="h-10 w-10 mx-auto mb-2 opacity-50" />
                    <p>No pending requests.</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="history" className="space-y-4 mt-4">
            <Card>
              <CardHeader>
                <CardTitle>Borrowing History</CardTitle>
                <CardDescription>
                  List of books you have borrowed and returned
                </CardDescription>
              </CardHeader>
              <CardContent>
                {historyBorrowings.length > 0 ? (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Book Title</TableHead>
                        <TableHead>Borrow Date</TableHead>
                        <TableHead>Return Date</TableHead>
                        <TableHead>Status</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {historyBorrowings.flatMap(request => 
                        Array.isArray(request.details) ?
                          request.details
                            .filter(detail => detail && detail.returnDate !== null)
                            .map(detail => (
                              <TableRow key={detail.detailId}>
                                <TableCell className="font-medium">{detail.bookTitle}</TableCell>
                                <TableCell>{formatDate(request.approvalDate)}</TableCell>
                                <TableCell>{formatDate(detail.returnDate)}</TableCell>
                                <TableCell>
                                  <Badge variant={getBadgeVariant(detail.status)}>
                                    {getBorrowingDetailStatusName(detail.status)}
                                  </Badge>
                                </TableCell>
                              </TableRow>
                            ))
                          : []
                      )}
                    </TableBody>
                  </Table>
                ) : (
                  <div className="text-center py-6 text-muted-foreground">
                    <CalendarCheck className="h-10 w-10 mx-auto mb-2 opacity-50" />
                    <p>You don't have any borrowing history yet.</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      )}
    </div>
  );
};

export default MyBorrowingBooksPage;
