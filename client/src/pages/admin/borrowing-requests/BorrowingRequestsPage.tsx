import { useState, useEffect } from "react";
import { useToast } from "../../../hooks/use-toast";
import {
    Tabs,
    TabsContent,
    TabsList,
    TabsTrigger,
} from "../../../components/ui/tabs";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "../../../components/ui/card";

import { BorrowingRequestList } from "./BorrowingRequestList";
import { ApproveDialog } from "./ApproveDialog";
import { RejectDialog } from "./RejectDialog";
import { 
    BookBorrowingRequestDto, 
    BorrowingRequestStatus,
    BorrowingRequestStatusUpdateDto 
} from "../../../types/borrowing";
import BorrowingService from "../../../services/borrowing.service";
import { Alert, AlertDescription } from "../../../components/ui/alert";
import { AlertCircle, Loader2 } from "lucide-react";
import { useAuth } from "../../../context/AuthContext";

export const BorrowingRequestsPage = () => {
    const [requests, setRequests] = useState<BookBorrowingRequestDto[]>([]);
    const [expandedRequests, setExpandedRequests] = useState<string[]>([]);
    const [activeStatus, setActiveStatus] = useState<string>("all");
    const [isApproveDialogOpen, setIsApproveDialogOpen] = useState(false);
    const [isRejectDialogOpen, setIsRejectDialogOpen] = useState(false);
    const [currentRequest, setCurrentRequest] = useState<BookBorrowingRequestDto | null>(null);
    const [notes, setNotes] = useState("");
    const [dueDays, setDueDays] = useState("30");
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalItems, setTotalItems] = useState(0);

    const { toast } = useToast();
    const { currentUser } = useAuth();

    useEffect(() => {
        fetchRequests();
    }, [currentPage, pageSize, activeStatus]);

    // Fetch borrowing requests from the server
    const fetchRequests = async () => {
        setIsLoading(true);
        setError(null);
        
        try {
            const response = await BorrowingService.getAllBorrowingRequests(currentPage, pageSize);
            
            if (response.success) {
                if (response.data && response.data.results) {
                    setRequests(response.data.results);
                    setTotalItems(response.data.totalCount || response.totalCount || 0);
                    console.log("Received total items:", response.data.totalCount || response.totalCount || 0);
                } else if (Array.isArray(response.data)) {
                    // If API returns an array directly
                    setRequests(response.data);
                    setTotalItems(response.totalCount || response.data.length);
                    console.log("Received array items:", response.totalCount || response.data.length);
                } else {
                    setRequests([]);
                    console.error("Unexpected data format:", response.data);
                    setError("Unexpected data format received from server");
                    toast({
                        variant: "destructive",
                        title: "Error",
                        description: "Unexpected data format received from server",
                    });
                }
            } else {
                setRequests([]);
                const errorMsg = response.message || "Failed to load borrowing requests";
                console.error("Error loading requests:", errorMsg);
                setError(errorMsg);
                toast({
                    variant: "destructive",
                    title: "Error",
                    description: errorMsg,
                });
            }
        } catch (err: unknown) {
            console.error("Exception when loading requests:", err);
            setRequests([]);
            const errorMsg = (err as Error).message || "An unexpected error occurred while loading borrowing requests";
            setError(errorMsg);
            toast({
                variant: "destructive",
                title: "Error",
                description: errorMsg,
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
        setCurrentPage(1); // Reset to first page when changing page size
    };

    // Filter requests based on tab selection
    const filteredRequests = (() => {
        // Đảm bảo mỗi request đều có thuộc tính details là mảng để tránh lỗi undefined.length
        const safeRequests = requests.map(req => ({
            ...req,
            details: req.details || []
        }));
        
        switch (activeStatus) {
            case "all":
                return safeRequests;
            case "waiting":
                return safeRequests.filter(req => req.status === BorrowingRequestStatus.Waiting);
            case "approved":
                return safeRequests.filter(req => req.status === BorrowingRequestStatus.Approved);
            case "rejected":
                return safeRequests.filter(req => req.status === BorrowingRequestStatus.Rejected);
            default:
                return safeRequests;
        }
    })();

    // Group requests by status for the counts
    const statusCounts = {
        all: requests.length,
        waiting: requests.filter(req => req.status === BorrowingRequestStatus.Waiting).length,
        approved: requests.filter(req => req.status === BorrowingRequestStatus.Approved).length,
        rejected: requests.filter(req => req.status === BorrowingRequestStatus.Rejected).length,
    };

    const toggleExpand = (requestId: string) => {
        setExpandedRequests(prev =>
            prev.includes(requestId)
                ? prev.filter(id => id !== requestId)
                : [...prev, requestId]
        );
    };

    const openApproveDialog = (request: BookBorrowingRequestDto) => {
        setCurrentRequest(request);
        setNotes("");
        setDueDays("30");
        setIsApproveDialogOpen(true);
    };

    const openRejectDialog = (request: BookBorrowingRequestDto) => {
        setCurrentRequest(request);
        setNotes("");
        setIsRejectDialogOpen(true);
    };

    const handleApproveRequest = async () => {
        if (!currentRequest || !currentUser) return;
        
        setIsSubmitting(true);
        
        try {
            const statusUpdateData: BorrowingRequestStatusUpdateDto = {
                requestId: currentRequest.requestId,
                approverId: currentUser.userId,
                status: BorrowingRequestStatus.Approved,
                notes: notes,
                dueDays: parseInt(dueDays, 10)
            };
            
            const response = await BorrowingService.updateBorrowingRequestStatus(statusUpdateData);
            
            if (response.success) {
                // Update local state to reflect the change
                const updatedRequests = requests.map(request => {
                    if (request.requestId === currentRequest.requestId) {
                        return {
                            ...request,
                            status: BorrowingRequestStatus.Approved,
                        };
                    }
                    return request;
                });
                
                setRequests(updatedRequests);
                toast({
                    title: "Request Approved",
                    description: `Borrowing request #${currentRequest.requestId.substring(0, 8)} has been approved.`,
                });
                
                // Refresh the list to get updated data from server
                fetchRequests();
            } else {
                toast({
                    variant: "destructive",
                    title: "Error",
                    description: response.message || "Failed to approve request",
                });
            }
        } catch (err) {
            console.error("Error approving request:", err);
            toast({
                variant: "destructive",
                title: "Error",
                description: "An unexpected error occurred while approving the request",
            });
        } finally {
            setIsSubmitting(false);
            setIsApproveDialogOpen(false);
            setCurrentRequest(null);
        }
    };

    const handleRejectRequest = async () => {
        if (!currentRequest || !currentUser) return;
        
        setIsSubmitting(true);
        
        try {
            const statusUpdateData: BorrowingRequestStatusUpdateDto = {
                requestId: currentRequest.requestId,
                approverId: currentUser.userId,
                status: BorrowingRequestStatus.Rejected,
                notes: notes
            };
            
            const response = await BorrowingService.updateBorrowingRequestStatus(statusUpdateData);
            
            if (response.success) {
                // Update local state to reflect the change
                const updatedRequests = requests.map(request => {
                    if (request.requestId === currentRequest.requestId) {
                        return {
                            ...request,
                            status: BorrowingRequestStatus.Rejected,
                        };
                    }
                    return request;
                });
                
                setRequests(updatedRequests);
                toast({
                    title: "Request Rejected",
                    description: `Borrowing request #${currentRequest.requestId.substring(0, 8)} has been rejected.`,
                });
                
                // Refresh the list to get updated data from server
                fetchRequests();
            } else {
                toast({
                    variant: "destructive",
                    title: "Error",
                    description: response.message || "Failed to reject request",
                });
            }
        } catch (err) {
            console.error("Error rejecting request:", err);
            toast({
                variant: "destructive",
                title: "Error",
                description: "An unexpected error occurred while rejecting the request",
            });
        } finally {
            setIsSubmitting(false);
            setIsRejectDialogOpen(false);
            setCurrentRequest(null);
        }
    };

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">Borrowing Requests</h2>
                <p className="text-muted-foreground">
                    Manage book borrowing requests from users
                </p>
            </div>

            <Tabs defaultValue="all" onValueChange={setActiveStatus}>
                <TabsList>
                    <TabsTrigger value="all">
                        All Requests ({statusCounts.all})
                    </TabsTrigger>
                    <TabsTrigger value="waiting">
                        Waiting ({statusCounts.waiting})
                    </TabsTrigger>
                    <TabsTrigger value="approved">
                        Approved ({statusCounts.approved})
                    </TabsTrigger>
                    <TabsTrigger value="rejected">
                        Rejected ({statusCounts.rejected})
                    </TabsTrigger>
                </TabsList>

                <TabsContent value={activeStatus} className="mt-6">
                    <Card>
                        <CardHeader>
                            <CardTitle>
                                {activeStatus === "all" ? "All Borrowing Requests" :
                                    activeStatus === "waiting" ? "Pending Requests" :
                                        activeStatus === "approved" ? "Approved Requests" : "Rejected Requests"}
                            </CardTitle>
                            <CardDescription>
                                {activeStatus === "waiting" ? "Review and manage pending borrowing requests" :
                                    activeStatus === "approved" ? "Requests that have been approved" :
                                        activeStatus === "rejected" ? "Requests that have been rejected" :
                                            "All borrowing requests in the system"}
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            {isLoading ? (
                                <div className="flex justify-center items-center py-8">
                                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                                </div>
                            ) : error ? (
                                <Alert variant="destructive">
                                    <AlertCircle className="h-4 w-4" />
                                    <AlertDescription>{error}</AlertDescription>
                                </Alert>
                            ) : (
                                <BorrowingRequestList
                                    requests={filteredRequests}
                                    expandedRequests={expandedRequests}
                                    toggleExpand={toggleExpand}
                                    onApprove={openApproveDialog}
                                    onReject={openRejectDialog}
                                    currentPage={currentPage}
                                    pageSize={pageSize}
                                    totalItems={totalItems}
                                    onPageChange={handlePageChange}
                                    onPageSizeChange={handlePageSizeChange}
                                />
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>
            </Tabs>

            {/* Approve Dialog */}
            <ApproveDialog
                isOpen={isApproveDialogOpen}
                onClose={() => setIsApproveDialogOpen(false)}
                currentRequest={currentRequest}
                notes={notes}
                dueDays={dueDays}
                onNotesChange={setNotes}
                onDueDaysChange={setDueDays}
                onApprove={handleApproveRequest}
                isSubmitting={isSubmitting}
            />

            {/* Reject Dialog */}
            <RejectDialog
                isOpen={isRejectDialogOpen}
                onClose={() => setIsRejectDialogOpen(false)}
                currentRequest={currentRequest}
                notes={notes}
                onNotesChange={setNotes}
                onReject={handleRejectRequest}
                isSubmitting={isSubmitting}
            />
        </div>
    );
}