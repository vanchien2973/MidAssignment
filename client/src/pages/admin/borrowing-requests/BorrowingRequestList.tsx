import { 
    BookBorrowingRequestDto, 
} from "../../../types/borrowing";
import { BorrowingRequestItem } from "./BorrowingRequestItem";
import { Button } from "../../../components/ui/button";

interface BorrowingRequestListProps {
    requests: BookBorrowingRequestDto[];
    expandedRequests: string[];
    toggleExpand: (requestId: string) => void;
    onApprove: (request: BookBorrowingRequestDto) => void;
    onReject: (request: BookBorrowingRequestDto) => void;
    currentPage?: number;
    pageSize?: number;
    totalItems?: number;
    onPageChange?: (page: number) => void;
    onPageSizeChange?: (size: number) => void;
}

export const BorrowingRequestList = ({
    requests,
    expandedRequests,
    toggleExpand,
    onApprove,
    onReject,
    currentPage = 1,
    pageSize = 10,
    totalItems = 0,
    onPageChange = () => {},
    onPageSizeChange = () => {}
}: BorrowingRequestListProps) => {
    const borrowingRequests = requests || [];
    const calculatedTotalPages = Math.max(1, totalItems > 0 ? Math.ceil(totalItems / pageSize) : 1);
    
    // Define pagination handlers
    const handleFirstPage = () => {
        if (currentPage !== 1) {
            onPageChange(1);
        }
    };
    
    const handlePreviousPage = () => {
        if (currentPage > 1) {
            onPageChange(currentPage - 1);
        }
    };
    
    const handleNextPage = () => {
        if (currentPage < calculatedTotalPages) {
            onPageChange(currentPage + 1);
        }
    };
    
    const handleLastPage = () => {
        if (currentPage !== calculatedTotalPages && calculatedTotalPages > 0) {
            onPageChange(calculatedTotalPages);
        }
    };
    
    const handlePageClick = (page: number) => {
        if (page !== currentPage) {
            onPageChange(page);
        }
    };
    
    return (
        <div className="space-y-6">
            {borrowingRequests.length > 0 ? (
                <>
                    <div className="space-y-6">
                        {borrowingRequests.map(request => (
                            <BorrowingRequestItem
                                key={request.requestId}
                                request={request}
                                isExpanded={expandedRequests.includes(request.requestId)}
                                toggleExpand={toggleExpand}
                                onApprove={onApprove}
                                onReject={onReject}
                            />
                        ))}
                    </div>
                    
                    {/* Pagination section */}
                    <div className="mt-6 space-y-4">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center space-x-2">
                                <span className="text-sm text-muted-foreground">Show:</span>
                                <select 
                                    className="h-8 rounded-md border border-input bg-background px-3 py-1 text-sm"
                                    value={pageSize}
                                    onChange={(e) => {
                                        const newSize = parseInt(e.target.value);
                                        onPageSizeChange(newSize);
                                    }}
                                >
                                    <option value="5">5</option>
                                    <option value="10">10</option>
                                    <option value="20">20</option>
                                    <option value="50">50</option>
                                </select>
                            </div>
                            <div className="text-sm text-muted-foreground">
                                Page {currentPage} of {calculatedTotalPages} ({totalItems} requests)
                            </div>
                        </div>
                        
                        <div className="flex justify-center">
                            <div className="flex space-x-1">
                                <Button 
                                    variant="outline" 
                                    size="sm"
                                    disabled={currentPage === 1}
                                    onClick={handleFirstPage}
                                >
                                    First
                                </Button>
                                <Button 
                                    variant="outline" 
                                    size="sm"
                                    disabled={currentPage === 1}
                                    onClick={handlePreviousPage}
                                >
                                    Previous
                                </Button>
                                
                                {/* Page number buttons */}
                                {Array.from({ length: Math.min(5, calculatedTotalPages) }, (_, i) => {
                                    // Calculate page numbers to show around current page
                                    let pageNum;
                                    if (calculatedTotalPages <= 5) {
                                        pageNum = i + 1;
                                    } else if (currentPage <= 3) {
                                        pageNum = i + 1;
                                    } else if (currentPage >= calculatedTotalPages - 2) {
                                        pageNum = calculatedTotalPages - 4 + i;
                                    } else {
                                        pageNum = currentPage - 2 + i;
                                    }
                                    
                                    // Only render if page is in range
                                    if (pageNum > 0 && pageNum <= calculatedTotalPages) {
                                        return (
                                            <Button
                                                key={pageNum}
                                                variant={currentPage === pageNum ? "default" : "outline"}
                                                size="sm"
                                                onClick={() => handlePageClick(pageNum)}
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
                                    disabled={currentPage >= calculatedTotalPages}
                                    onClick={handleNextPage}
                                >
                                    Next
                                </Button>
                                <Button 
                                    variant="outline" 
                                    size="sm"
                                    disabled={currentPage >= calculatedTotalPages}
                                    onClick={handleLastPage}
                                >
                                    Last
                                </Button>
                            </div>
                        </div>
                    </div>
                </>
            ) : (
                <div className="py-8 text-center">
                    <h3 className="text-lg font-medium">No requests found</h3>
                    <p className="text-sm text-muted-foreground mt-1">
                        There are no borrowing requests in this category
                    </p>
                </div>
            )}
        </div>
    );
} 