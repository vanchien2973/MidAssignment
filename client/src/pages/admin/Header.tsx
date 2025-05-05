import { Bell, ChevronDown, Search } from "lucide-react";
import { Input } from "../../components/ui/input";
import { ThemeToggle } from "../../components/ThemeToggle";
import { Button } from "../../components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "../../components/ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "../../components/ui/avatar";

export const AdminHeader: React.FC = () => {
    return (
        <header className="sticky top-0 z-10 flex items-center justify-between h-16 px-6 border-b bg-white dark:bg-gray-950 border-gray-200 dark:border-gray-800">
            <div className="lg:hidden w-6"></div>

            <div className="flex items-center gap-4 md:gap-8 lg:ml-64">
                <div className="relative hidden md:block">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-gray-500 dark:text-gray-400" />
                    <Input
                        type="search"
                        placeholder="Search..."
                        className="w-64 pl-8 bg-gray-50 dark:bg-gray-900 border-gray-200 dark:border-gray-800"
                    />
                </div>
            </div>

            <div className="flex items-center gap-3">
                <ThemeToggle />

                <Button variant="outline" size="icon" className="relative">
                    <Bell className="h-5 w-5" />
                    <span className="absolute top-0 right-0 w-2 h-2 bg-red-500 rounded-full"></span>
                </Button>

                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="gap-2 flex items-center">
                            <Avatar className="h-8 w-8">
                                <AvatarImage src="/api/placeholder/30/30" alt="User" />
                                <AvatarFallback>AD</AvatarFallback>
                            </Avatar>
                            <div className="hidden md:block text-sm font-medium text-left">
                                Admin User
                            </div>
                            <ChevronDown className="h-4 w-4 text-gray-500 dark:text-gray-400" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" className="w-56">
                        <DropdownMenuLabel>My Account</DropdownMenuLabel>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem>Profile</DropdownMenuItem>
                        <DropdownMenuItem>Settings</DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem>Log out</DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </header>
    );
};