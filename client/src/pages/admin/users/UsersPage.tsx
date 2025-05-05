import { useState, useEffect } from "react";
import { User, UserSearchParams, UserType } from "../../../types/user";
import { useToast } from "../../../hooks/use-toast";
import { Users } from "lucide-react";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "../../../components/ui/card";

import { UsersTable } from "./UsersTable";
import { UserEditDialog } from "./UserEditDialog";
import UserService from "../../../services/user.service";

export default function UsersPage() {
    const [users, setUsers] = useState<User[]>([]);
    const [editingUser, setEditingUser] = useState<User | null>(null);
    const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [searchParams, setSearchParams] = useState<UserSearchParams>({
        pageNumber: 1,
        pageSize: 5,
        searchTerm: ''
    });
    const [totalCount, setTotalCount] = useState(0);
    
    const { toast } = useToast();

    const fetchUsers = async () => {
        try {
            const response = await UserService.getAllUsers(searchParams);
            if (response.success) {
                setUsers(response.data);
                if (response.totalCount !== undefined) {
                    setTotalCount(response.totalCount);
                }
            } else {
                toast({
                    title: "Error",
                    description: response.message || "Unable to load user list",
                    variant: "destructive"
                });
            }
        } catch (error) {
            toast({
                title: "Error",
                description: "An error occurred while loading user list",
                variant: "destructive"
            });
            console.error("Error fetching users:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, [searchParams]);

    const handleEditUser = (user: User) => {
        setEditingUser(user);
        setIsEditDialogOpen(true);
    };

    const handleDeleteUser = async (userId: number, username: string) => {
        try {
            const response = await UserService.deleteUser(userId);
            if (response.success) {
                toast({
                    title: "User deleted",
                    description: `User ${username} has been deleted successfully.`
                });
                fetchUsers();
            } else {
                toast({
                    title: "Error",
                    description: response.message || "Unable to delete user",
                    variant: "destructive"
                });
            }
        } catch (error) {
            toast({
                title: "Error",
                description: "An error occurred while deleting user",
                variant: "destructive"
            });
            console.error("Error deleting user:", error);
        }
    };

    const handleSaveUser = async (updatedUser: User) => {
        try {
            let success = false;
            let message = "";

            // Handle role change if userType changed
            if (editingUser && editingUser.userType !== updatedUser.userType) {
                const roleResponse = await UserService.updateUserRole(
                    updatedUser.userId, 
                    updatedUser.userType as UserType
                );
                success = roleResponse.success;
                message = roleResponse.message || "";
            }

            // Handle active status change
            if (editingUser && editingUser.isActive !== updatedUser.isActive) {
                const statusResponse = updatedUser.isActive 
                    ? await UserService.activateUser(updatedUser.userId)
                    : await UserService.deactivateUser(updatedUser.userId);
                
                success = statusResponse.success;
                message = statusResponse.message || "";
            }

            if (success) {
                fetchUsers();
                setIsEditDialogOpen(false);
                setEditingUser(null);
                
                toast({
                    title: "Update successful",
                    description: `User ${updatedUser.fullName} has been updated.`,
                });
            } else {
                toast({
                    title: "Error",
                    description: message || "Unable to update user",
                    variant: "destructive"
                });
            }
        } catch (error) {
            toast({
                title: "Error",
                description: "An error occurred while updating user",
                variant: "destructive"
            });
            console.error("Error saving user:", error);
        }
    };

    const handleSearchParamsChange = (newParams: Partial<UserSearchParams>) => {
        setSearchParams(prev => {
            const updated = {
                ...prev,
                ...newParams
            };
            return updated;
        });
    };

    return (
        <div className="space-y-6">
            <div>
                <h2 className="text-3xl font-bold tracking-tight">User Management</h2>
                <p className="text-muted-foreground">
                    Manage user information and permissions in the system
                </p>
            </div>

            <div className="grid gap-6">
                <Card>
                    <CardHeader className="flex flex-row items-center gap-4">
                        <div className="bg-primary p-3 rounded-lg">
                            <Users className="h-6 w-6 text-primary-foreground" />
                        </div>
                        <div>
                            <CardTitle>User List</CardTitle>
                            <CardDescription>
                                Manage and set permissions for user accounts
                            </CardDescription>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <UsersTable
                            users={users}
                            onEditUser={handleEditUser}
                            onDeleteUser={handleDeleteUser}
                            loading={loading}
                            searchParams={searchParams}
                            onSearchParamsChange={handleSearchParamsChange}
                            totalCount={totalCount}
                        />
                    </CardContent>
                </Card>
            </div>

            <UserEditDialog
                isOpen={isEditDialogOpen}
                onClose={() => setIsEditDialogOpen(false)}
                user={editingUser}
                onSave={handleSaveUser}
            />
        </div>
    );
}