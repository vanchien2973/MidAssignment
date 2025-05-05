import { FileText, Edit, Trash2 } from "lucide-react";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "../../../components/ui/table";
import { Category } from "../../../types/category";
import { Button } from "../../../components/ui/button";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "../../../components/ui/tooltip";

interface CategoryTableProps {
    categories: Category[];
    onEdit: (category: Category) => void;
    onDelete: (category: Category) => void;
    currentPage?: number;
    pageSize?: number;
    totalItems?: number;
    onPageChange?: (page: number) => void;
    onPageSizeChange?: (size: number) => void;
}

export const CategoryTable = ({ 
    categories, 
    onEdit, 
    onDelete,
    currentPage = 1,
    pageSize = 10,
    totalItems = 0,
    onPageChange = () => {},
    onPageSizeChange = () => {}
}: CategoryTableProps) => {
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
        <>
            <div className="rounded-md border mb-4">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead className="w-[60px]">No.</TableHead>
                            <TableHead>Name</TableHead>
                            <TableHead>Description</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {categories.length > 0 ? (
                            categories.map((category, index) => (
                                <TableRow key={category.categoryId}>
                                    <TableCell className="font-medium">
                                        {((currentPage - 1) * pageSize) + index + 1}
                                    </TableCell>
                                    <TableCell className="flex items-center gap-2">
                                        <FileText size={16} /> {category.categoryName}
                                    </TableCell>
                                    <TableCell>{category.description}</TableCell>
                                    <TableCell className="text-right space-x-2">
                                        <TooltipProvider>
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        onClick={() => onEdit(category)}
                                                    >
                                                        <Edit size={16} />
                                                    </Button>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    <p>Update</p>
                                                </TooltipContent>
                                            </Tooltip>
                                        </TooltipProvider>
                                        
                                        <TooltipProvider>
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        onClick={() => onDelete(category)}
                                                    >
                                                        <Trash2 size={16} />
                                                    </Button>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    <p>Delete</p>
                                                </TooltipContent>
                                            </Tooltip>
                                        </TooltipProvider>
                                    </TableCell>
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={4} className="h-24 text-center">
                                    No categories found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>

            {/* Pagination section - always showing */}
            <div className="mt-6 space-y-4">
                <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                        <span className="text-sm text-muted-foreground">Items per page:</span>
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
                        Page {currentPage} of {calculatedTotalPages || 1} ({totalItems} items)
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
    );
};