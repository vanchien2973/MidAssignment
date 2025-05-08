import { useState } from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";
import {
    FileText, LogOut, Menu,
    ChevronLeft, ChevronRight,
    Bookmark,
    Book,
    Users,
    BookOpen
} from "lucide-react";
import { Button } from "../../components/ui/button";
import { Sheet, SheetContent } from "../../components/ui/sheet";
import { cn } from "../../lib/utils";
import { useAuth } from "../../context/AuthContext";

export const AdminSidebar = () => {
    const [collapsed, setCollapsed] = useState(false);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const { logout } = useAuth();
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    const navItems = [
        { name: "Dashboard", path: "/admin", icon: BookOpen },
        { name: "Manage Users", path: "/admin/users", icon: Users },
        { name: "Manage Categories", path: "/admin/categories", icon: FileText },
        { name: "Manage Books", path: "/admin/books", icon: Book },
        { name: "Borrowing Requests", path: "/admin/borrowing-requests", icon: Bookmark },
    ];

    const SidebarContent = () => (
        <div className={cn(
            "flex flex-col h-full bg-sidebar text-sidebar-foreground border-r border-gray-200 transition-all",
            collapsed ? "w-16" : "w-64"
        )}>
            <div className="p-4 border-b border-gray-200 flex items-center justify-between">
                {!collapsed && (
                    <Link to="/admin" className="flex items-center gap-2">
                        <div className="bg-primary w-8 h-8 rounded-md flex items-center justify-center">
                            <span className="text-white font-bold">L</span>
                        </div>
                        <span className="font-bold text-lg">LMS</span>
                    </Link>
                )}
                <Button
                    variant="ghost"
                    size="icon"
                    className="ml-auto"
                    onClick={() => setCollapsed(prev => !prev)}
                >
                    {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
                </Button>
            </div>

            <div className="flex flex-col pt-4 flex-1 overflow-y-auto">
                <div className="px-3 py-2">
                    {!collapsed && <p className="text-xs font-semibold text-gray-500 uppercase mb-2 px-3">MAIN</p>}
                    <div className="space-y-1">
                        {navItems.map((item) => (
                            <NavLink
                                key={item.path}
                                to={item.path}
                                end={item.path === "/admin"}
                                className={({ isActive }) => cn(
                                    "flex items-center gap-3 px-3 py-2 rounded-md transition-colors",
                                    isActive
                                        ? "bg-gray-100 dark:bg-gray-800"
                                        : "hover:bg-gray-100 dark:hover:bg-gray-800",
                                    collapsed && "justify-center"
                                )}
                            >
                                <item.icon className={cn("w-5 h-5 text-gray-500 dark:text-gray-400", collapsed ? "mx-0" : "mr-3")} />
                                {!collapsed && <span className="text-sm font-medium">{item.name}</span>}
                            </NavLink>
                        ))}
                    </div>
                </div>
            </div>

            <div className="p-4 mt-auto border-t border-gray-200">
                <Button 
                    variant="outline" 
                    className="w-full justify-start gap-2"
                    onClick={handleLogout}
                >
                    <LogOut className="w-4 h-4" />
                    {!collapsed && <span>Log out</span>}
                </Button>
            </div>
        </div>
    );

    return (
        <>
            {/* Desktop Sidebar */}
            <div className="hidden lg:block">
                <SidebarContent />
            </div>

            {/* Mobile Menu Button */}
            <Button
                variant="outline"
                size="icon"
                className="lg:hidden fixed left-4 top-4 z-20"
                onClick={() => setIsMobileMenuOpen(true)}
            >
                <Menu className="h-5 w-5" />
            </Button>

            {/* Mobile Sidebar */}
            <Sheet open={isMobileMenuOpen} onOpenChange={setIsMobileMenuOpen}>
                <SheetContent side="left" className="p-0">
                    <SidebarContent />
                </SheetContent>
            </Sheet>
        </>
    );
}