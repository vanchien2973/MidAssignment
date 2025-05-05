import { useState, useEffect } from "react";
import { UserActivityLog, UserActivityLogSearchParams } from "../../types/user";
import { useToast } from "../../hooks/use-toast";
import UserService from "../../services/user.service";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "../ui/card";
import { Button } from "../ui/button";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "../ui/popover";
import { Filter, Search, Clock } from "lucide-react";

const ActivityTab = () => {
  const { toast } = useToast();
  const [activityLogs, setActivityLogs] = useState<UserActivityLog[]>([]);
  const [isLoadingLogs, setIsLoadingLogs] = useState(false);
  const [searchParams, setSearchParams] = useState<UserActivityLogSearchParams>({
    pageNumber: 1,
    pageSize: 10
  });
  const [showFilters, setShowFilters] = useState(false);
  const [startDate, setStartDate] = useState<Date | undefined>(undefined);
  const [endDate, setEndDate] = useState<Date | undefined>(undefined);
  const [searchActivity, setSearchActivity] = useState("");
  const [totalPages, setTotalPages] = useState(1);
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(10);
  const [activityTypeFilter, setActivityTypeFilter] = useState<string>("");

  // Apply filters
  const applyFilters = () => {
    const newParams: UserActivityLogSearchParams = {
      pageNumber: 1,
      pageSize: itemsPerPage
    };
    
    if (activityTypeFilter && activityTypeFilter.trim() !== "") {
      newParams.activityType = activityTypeFilter;
    }

    setCurrentPage(1);
    setSearchParams(newParams);
    fetchActivityLogs(newParams);
    setShowFilters(false);
  };
  
  // Reset filters
  const resetFilters = () => {
    setStartDate(undefined);
    setEndDate(undefined);
    setActivityTypeFilter("");
    setSearchActivity("");
    
    // Reset về trang đầu tiên
    const resetParams = {
      pageNumber: 1,
      pageSize: itemsPerPage
    };
    
    setCurrentPage(1);
    setSearchParams(resetParams);
    setShowFilters(false);
  
    fetchActivityLogs(resetParams);
  };
  
  // Fetch activity logs with search params
  const fetchActivityLogs = async (params?: UserActivityLogSearchParams) => {
    try {
      setIsLoadingLogs(true);
      
      // Use provided params or current searchParams
      const queryParams: UserActivityLogSearchParams = params || {
        ...searchParams,
        pageNumber: currentPage,
        pageSize: itemsPerPage
      };
    
      const response = await UserService.getUserActivityLogs(queryParams);
      
      // Check if response is empty
      if (!response || response.length === 0) {
        setActivityLogs([]);
        setTotalPages(1);
        setIsLoadingLogs(false);
        return;
      }
      
      // Apply filters to the received data
      let filteredLogs = [...response];
      
      // Filter by activity type if provided
      if (searchActivity && searchActivity.trim() !== "") {
        const searchTerm = searchActivity.toLowerCase();
        console.log('Filtering by search term:', searchTerm);
        
        filteredLogs = filteredLogs.filter(log => {
          const activityTypeMatch = log.activityType.toLowerCase().includes(searchTerm);
          const detailsMatch = log.details ? log.details.toLowerCase().includes(searchTerm) : false;
          const usernameMatch = log.username.toLowerCase().includes(searchTerm);
          
          return activityTypeMatch || detailsMatch || usernameMatch;
        });
      }
      
      // Filter by date range if provided
      if (startDate || endDate) {
        console.log('Filtering by date range:', { 
          startDate: startDate ? startDate.toISOString() : null, 
          endDate: endDate ? endDate.toISOString() : null 
        });
        
        filteredLogs = filteredLogs.filter(log => {
          const logDate = new Date(log.activityDate);
          
          const start = startDate ? new Date(startDate) : null;
          if (start) {
            start.setHours(0, 0, 0, 0);
          }
          
          const end = endDate ? new Date(endDate) : null;
          if (end) {
            end.setHours(23, 59, 59, 999);
          }
          
          const result = 
            (!start || logDate >= start) && 
            (!end || logDate <= end);
          
          return result;
        });
      }
      
      setActivityLogs(filteredLogs);
      updatePagination(filteredLogs, queryParams);
      
    } catch (error) {
      console.error("Error fetching activity logs:", error);
      toast({
        title: "Error",
        description: "Unable to load activity history",
        variant: "destructive",
      });
      setActivityLogs([]);
      setTotalPages(1);
    } finally {
      setIsLoadingLogs(false);
    }
  };
  
  // Helper function to update pagination based on filtered results
  const updatePagination = (filteredLogs: UserActivityLog[], queryParams: UserActivityLogSearchParams) => {
    const pageSize = queryParams.pageSize || 10;
    
    if (filteredLogs.length === 0) {
      setTotalPages(1);
    } else {
      // Check if any filters are applied
      const hasFilters = !!(startDate || endDate || searchActivity);
      
      if (hasFilters) {
        // When client-side filtering is applied
        setTotalPages(Math.max(1, Math.ceil(filteredLogs.length / pageSize)));
      } else if (filteredLogs.length < pageSize) {
        // When no client-side filters and not enough results
        setTotalPages(queryParams.pageNumber || 1);
      } else {
        // When there are no client-side filters and enough results, there may be a next page
        setTotalPages(Math.max((queryParams.pageNumber || 1) + 1, 2));
      }
    }
  };
  
  // Handle search input change
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchActivity(e.target.value);
  };
  
  // Apply search filter
  const applySearch = () => {
    // Thực hiện tìm kiếm mới
    setCurrentPage(1);
    
    const newParams: UserActivityLogSearchParams = {
      pageNumber: 1,
      pageSize: itemsPerPage,
      activityType: activityTypeFilter || undefined
    };
    setSearchParams(newParams);
    fetchActivityLogs(newParams);
  };

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    const newParams = {
      ...searchParams,
      pageNumber: page,
      pageSize: itemsPerPage
    };
    setSearchParams(newParams);
    fetchActivityLogs(newParams);
  };

  // Handle items per page change
  const handleItemsPerPageChange = (newSize: number) => {
    setItemsPerPage(newSize);
    const newParams = {
      ...searchParams,
      pageNumber: 1, // Reset to first page when changing items per page
      pageSize: newSize
    };
    setCurrentPage(1);
    setSearchParams(newParams);
    fetchActivityLogs(newParams);
  };

  // Format date for display
  const formatDate = (date: string | Date) => {
    return new Date(date).toLocaleString("en-US", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  // Load data on component mount
  useEffect(() => {
    fetchActivityLogs();
  }, []);

  return (
    <Card className="dark:border-gray-700">
      <CardHeader>
        <CardTitle className="dark:text-white">Activity History</CardTitle>
        <CardDescription className="dark:text-gray-400">
          View your recent activities in the system
        </CardDescription>
        
        <div className="mt-4 flex flex-col sm:flex-row gap-4">
          <div className="relative flex-1">
            <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search activities..."
              className="pl-8"
              value={searchActivity}
              onChange={handleSearchChange}
              onKeyDown={(e) => e.key === 'Enter' && applySearch()}
            />
          </div>
          
          <div className="flex gap-2">
            <Popover open={showFilters} onOpenChange={setShowFilters}>
              <PopoverTrigger asChild>
                <Button variant="outline" className="gap-2">
                  <Filter className="h-4 w-4" />
                  Filters {(startDate || endDate || activityTypeFilter) ? 
                    <span className="ml-1 bg-blue-500 text-white rounded-full w-4 h-4 flex items-center justify-center text-xs">!</span> : null}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-80">
                <div className="space-y-4">
                  <h4 className="font-medium">Filter Options</h4>
                  
                  <div className="space-y-2">
                    <Label htmlFor="startDate">Start Date</Label>
                    <Input
                      id="startDate"
                      type="date"
                      value={startDate ? new Date(startDate).toISOString().split('T')[0] : ''}
                      onChange={(e) => {
                        if (e.target.value) {
                          try {
                            // Tạo đối tượng Date mới từ giá trị đầu vào
                            const date = new Date(e.target.value);
                            // Kiểm tra xem đối tượng Date có hợp lệ không
                            if (!isNaN(date.getTime())) {
                              setStartDate(date);
                              console.log('Start date changed:', date.toISOString());
                            } else {
                              console.error('Invalid date format:', e.target.value);
                            }
                          } catch (error) {
                            console.error('Error parsing date:', error);
                          }
                        } else {
                          setStartDate(undefined);
                          console.log('Start date cleared');
                        }
                      }}
                      className="w-full"
                    />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="endDate">End Date</Label>
                    <Input
                      id="endDate"
                      type="date"
                      value={endDate ? new Date(endDate).toISOString().split('T')[0] : ''}
                      onChange={(e) => {
                        if (e.target.value) {
                          try {
                            // Tạo đối tượng Date mới từ giá trị đầu vào
                            const date = new Date(e.target.value);
                            // Kiểm tra xem đối tượng Date có hợp lệ không
                            if (!isNaN(date.getTime())) {
                              setEndDate(date);
                              console.log('End date changed:', date.toISOString());
                            } else {
                              console.error('Invalid date format:', e.target.value);
                            }
                          } catch (error) {
                            console.error('Error parsing date:', error);
                          }
                        } else {
                          setEndDate(undefined);
                          console.log('End date cleared');
                        }
                      }}
                      className="w-full"
                    />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="activityType">Activity Type</Label>
                    <Input
                      id="activityType"
                      placeholder="e.g., Login, Create, Update"
                      value={activityTypeFilter}
                      onChange={(e) => setActivityTypeFilter(e.target.value)}
                      className="w-full"
                    />
                  </div>
                  
                  <div className="flex justify-between">
                    <Button variant="outline" onClick={resetFilters}>
                      Reset
                    </Button>
                    <Button onClick={applyFilters}>
                      Apply Filters
                    </Button>
                  </div>
                </div>
              </PopoverContent>
            </Popover>
            
            <Button onClick={() => {
              fetchActivityLogs();
            }}>
              Refresh
            </Button>
          </div>
        </div>

        {/* Hiển thị trạng thái lọc hiện tại */}
        {(startDate || endDate || activityTypeFilter || searchActivity) && (
          <div className="mt-2 text-sm text-muted-foreground flex flex-wrap gap-2">
            <div>Active filters:</div>
            {startDate && (
              <div className="bg-blue-100 dark:bg-blue-900 px-2 py-0.5 rounded-full flex items-center">
                <span>From: {new Date(startDate).toLocaleDateString()}</span>
                <button 
                  className="ml-1 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200" 
                  onClick={() => {
                    setStartDate(undefined);
                    applyFilters();
                  }}
                >
                  ×
                </button>
              </div>
            )}
            {endDate && (
              <div className="bg-blue-100 dark:bg-blue-900 px-2 py-0.5 rounded-full flex items-center">
                <span>To: {new Date(endDate).toLocaleDateString()}</span>
                <button 
                  className="ml-1 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200" 
                  onClick={() => {
                    setEndDate(undefined);
                    applyFilters();
                  }}
                >
                  ×
                </button>
              </div>
            )}
            {activityTypeFilter && (
              <div className="bg-blue-100 dark:bg-blue-900 px-2 py-0.5 rounded-full flex items-center">
                <span>Type: {activityTypeFilter}</span>
                <button 
                  className="ml-1 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200" 
                  onClick={() => {
                    setActivityTypeFilter("");
                    applyFilters();
                  }}
                >
                  ×
                </button>
              </div>
            )}
            {searchActivity && (
              <div className="bg-blue-100 dark:bg-blue-900 px-2 py-0.5 rounded-full flex items-center">
                <span>Search: {searchActivity}</span>
                <button 
                  className="ml-1 text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200" 
                  onClick={() => {
                    setSearchActivity("");
                    applySearch();
                  }}
                >
                  ×
                </button>
              </div>
            )}
            {(startDate || endDate || activityTypeFilter || searchActivity) && (
              <button 
                className="text-blue-500 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300 underline"
                onClick={resetFilters}
              >
                Clear all
              </button>
            )}
          </div>
        )}
      </CardHeader>
      <CardContent>
        {isLoadingLogs ? (
          <div className="text-center py-4">Loading data...</div>
        ) : activityLogs.length === 0 ? (
          <div className="text-center py-4">No recent activities</div>
        ) : (
          <div className="space-y-4">
            {activityLogs.map((log) => (
              <div key={log.logId} className="p-4 border rounded-md dark:border-gray-700">
                <div className="flex items-center gap-2">
                  <Clock className="h-4 w-4 text-blue-500" />
                  <span className="font-medium dark:text-white">{log.activityType}</span>
                </div>
                <div className="mt-2 text-sm text-gray-500 dark:text-gray-400">
                  <p>Time: {formatDate(log.activityDate)}</p>
                  {log.details && <p className="mt-1">Details: {log.details}</p>}
                  <p className="mt-1 text-xs">User: {log.username}</p>
                  {log.ipAddress && <p className="mt-1 text-xs">IP: {log.ipAddress}</p>}
                </div>
              </div>
            ))}
            
            {/* Improved Pagination */}
            <div className="mt-6 space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <span className="text-sm text-muted-foreground">Items per page:</span>
                  <select 
                    className="h-8 rounded-md border border-input bg-background px-3 py-1 text-sm"
                    value={itemsPerPage}
                    onChange={(e) => handleItemsPerPageChange(parseInt(e.target.value))}
                  >
                    <option value="5">5</option>
                    <option value="10">10</option>
                    <option value="20">20</option>
                    <option value="50">50</option>
                  </select>
                </div>
                <div className="text-sm text-muted-foreground">
                  Showing page {currentPage} of {totalPages || 1}
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
                  
                  {/* Page Number Buttons */}
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
                    disabled={currentPage === totalPages || totalPages === 0}
                    onClick={() => handlePageChange(currentPage + 1)}
                  >
                    Next
                  </Button>
                  <Button 
                    variant="outline" 
                    size="sm"
                    disabled={currentPage === totalPages || totalPages === 0}
                    onClick={() => handlePageChange(totalPages)}
                  >
                    Last
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default ActivityTab; 