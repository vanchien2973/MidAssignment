import { User, UserType, UserSearchParams } from "../../../types/user";
import { useState, useEffect } from "react";
import { Badge } from "../../../components/ui/badge";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "../../../components/ui/table";
import { Button } from "../../../components/ui/button";
import { Edit, UserCog, CheckCircle2, XCircle, Loader2, Trash2 } from "lucide-react";
import { Input } from "../../../components/ui/input";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "../../../components/ui/alert-dialog";
import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "../../../components/ui/pagination";

interface UsersTableProps {
    users: User[];
    onEditUser: (user: User) => void;
    onDeleteUser?: (userId: number, username: string) => void;
    loading?: boolean;
    searchParams?: UserSearchParams;
    onSearchParamsChange?: (params: Partial<UserSearchParams>) => void;
    totalCount?: number;
}

export function UsersTable({
    users,
    onEditUser,
    onDeleteUser,
    loading = false,
    searchParams = { pageNumber: 1, pageSize: 5, searchTerm: '' },
    onSearchParamsChange,
    totalCount = 0
}: UsersTableProps) {
    const [searchTerm, setSearchTerm] = useState(searchParams.searchTerm || '');
    const [userToDelete, setUserToDelete] = useState<User | null>(null);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);

    // Sync internal state with searchParams from props
    useEffect(() => {
        if (searchParams) {
            setSearchTerm(searchParams.searchTerm || '');
        }
    }, [searchParams]);

    // Sync internal filters with searchParams from props
    useEffect(() => {
        if (onSearchParamsChange) {
            // Send search parameters when user changes filters
            const debounce = setTimeout(() => {
                const params: Partial<UserSearchParams> = {
                    searchTerm: searchTerm ? searchTerm : undefined,
                };
                
                onSearchParamsChange(params);
            }, 500);

            return () => clearTimeout(debounce);
        }
    }, [searchTerm, onSearchParamsChange]);

    // If no onSearchParamsChange, use local filtering
    const filteredUsers = !onSearchParamsChange ? users.filter(user => {
        const searchTermToUse = searchTerm.trim();
        
        const matchesSearch = searchTermToUse === '' || 
            user.fullName.toLowerCase().includes(searchTermToUse.toLowerCase()) ||
            user.username.toLowerCase().includes(searchTermToUse.toLowerCase()) ||
            user.email.toLowerCase().includes(searchTermToUse.toLowerCase());
        
        return matchesSearch;
    }) : users;

    // Tính toán số trang
    const totalPages = Math.ceil((totalCount || filteredUsers.length) / (searchParams.pageSize || 5));
    const currentPage = searchParams.pageNumber || 1;

    // Hàm xử lý chuyển trang
    const handlePageChange = (page: number) => {
        console.log("Changing to page:", page, "Current page:", currentPage);
        if (onSearchParamsChange && page !== currentPage) {
            console.log("Sending page change request:", page);
            onSearchParamsChange({ 
                pageNumber: page,
                pageSize: searchParams?.pageSize || 5
            });
        }
    };

    // Format date and time
    const formatDateTime = (dateString: string | null | undefined): string => {
        if (!dateString) return "Never logged in";
        
        try {
            const date = new Date(dateString);
            return date.toLocaleString('en-US', { 
                day: '2-digit',
                month: '2-digit', 
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit'
            });
        } catch (error) {
            return "Invalid format";
        }
    };

    const handleDeleteConfirm = (user: User) => {
        setUserToDelete(user);
        setIsDeleteDialogOpen(true);
    };

    const confirmDelete = () => {
        if (userToDelete && onDeleteUser) {
            onDeleteUser(userToDelete.userId, userToDelete.username);
        }
        setIsDeleteDialogOpen(false);
        setUserToDelete(null);
    };

    // Tạo mảng số trang để hiển thị
    const getPageNumbers = () => {
        const pageNumbers = [];
        const maxPagesToShow = 5;
        const maxPagesOneSide = Math.floor(maxPagesToShow / 2);
        
        let startPage = Math.max(1, currentPage - maxPagesOneSide);
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);
        
        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }
        
        return Array.from({ length: endPage - startPage + 1 }, (_, i) => startPage + i);
    };

    return (
        <div className="space-y-4">
            <div className="flex flex-wrap gap-4">
                <div className="flex-1 min-w-[250px]">
                    <Input
                        placeholder="Search users..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        className="max-w-sm"
                    />
                </div>
            </div>

            <div className="rounded-md border">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead>No.</TableHead>
                            <TableHead>Username</TableHead>
                            <TableHead>Full Name</TableHead>
                            <TableHead>Email</TableHead>
                            <TableHead>Role</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Last Login</TableHead>
                            <TableHead>Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {loading ? (
                            <TableRow>
                                <TableCell colSpan={8} className="h-24 text-center">
                                    <div className="flex justify-center items-center">
                                        <Loader2 className="h-6 w-6 animate-spin mr-2" />
                                        <span>Loading...</span>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ) : filteredUsers.length > 0 ? (
                            filteredUsers.map((user, index) => (
                                <TableRow key={user.userId}>
                                    <TableCell>
                                        {((searchParams.pageNumber || 1) - 1) * (searchParams.pageSize || 5) + index + 1}
                                    </TableCell>
                                    <TableCell>{user.username}</TableCell>
                                    <TableCell>{user.fullName}</TableCell>
                                    <TableCell>{user.email}</TableCell>
                                    <TableCell>
                                        <Badge 
                                            variant={user.userType === "SuperUser" ? "destructive" : "default"} 
                                            className="flex w-fit items-center gap-1"
                                        >
                                            <UserCog className="h-3 w-3" />
                                            <span>
                                                {user.userType === "SuperUser" ? "Administrator" : "Regular User"}
                                            </span>
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        <Badge
                                            variant={user.isActive ? "success" : "secondary"}
                                            className="flex w-fit items-center gap-1"
                                        >
                                            {user.isActive ? (
                                                <>
                                                    <CheckCircle2 className="h-3 w-3" />
                                                    <span>Active</span>
                                                </>
                                            ) : (
                                                <>
                                                    <XCircle className="h-3 w-3" />
                                                    <span>Inactive</span>
                                                </>
                                            )}
                                        </Badge>
                                    </TableCell>
                                    <TableCell>
                                        {formatDateTime(user.lastLoginDate)}
                                    </TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            <Button
                                                size="sm"
                                                variant="outline"
                                                className="flex items-center gap-1"
                                                onClick={() => onEditUser(user)}
                                            >
                                                <Edit className="h-3.5 w-3.5" />
                                                <span>Edit</span>
                                            </Button>
                                            
                                            {onDeleteUser && (
                                                <Button
                                                    size="sm"
                                                    variant="destructive"
                                                    className="flex items-center gap-1"
                                                    onClick={() => handleDeleteConfirm(user)}
                                                >
                                                    <Trash2 className="h-3.5 w-3.5" />
                                                    <span>Delete</span>
                                                </Button>
                                            )}
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))
                        ) : (
                            <TableRow>
                                <TableCell colSpan={8} className="h-24 text-center">
                                    No users found.
                                </TableCell>
                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>

            {/* Phân trang */}
            {(totalCount > 0 && totalPages > 1) && (
                <Pagination>
                    <PaginationContent>
                        <PaginationItem>
                            <PaginationPrevious 
                                onClick={() => handlePageChange(Math.max(1, currentPage - 1))}
                                className={currentPage === 1 ? "pointer-events-none opacity-50" : ""}
                            />
                        </PaginationItem>
                        
                        {getPageNumbers().map(page => (
                            <PaginationItem key={page}>
                                <PaginationLink 
                                    isActive={page === currentPage}
                                    onClick={() => handlePageChange(page)}
                                >
                                    {page}
                                </PaginationLink>
                            </PaginationItem>
                        ))}
                        
                        <PaginationItem>
                            <PaginationNext 
                                onClick={() => handlePageChange(Math.min(totalPages, currentPage + 1))}
                                className={currentPage === totalPages ? "pointer-events-none opacity-50" : ""}
                            />
                        </PaginationItem>
                    </PaginationContent>
                </Pagination>
            )}

            <AlertDialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Confirm User Deletion</AlertDialogTitle>
                        <AlertDialogDescription>
                            Are you sure you want to delete the user {userToDelete?.username}?
                            This action cannot be undone.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction 
                            onClick={confirmDelete}
                            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                        >
                            Delete
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </div>
    );
}