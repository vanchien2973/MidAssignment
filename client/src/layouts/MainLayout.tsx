import { useState } from "react";
import { Outlet, Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { Button } from "../components/ui/button";
import { Avatar, AvatarFallback } from "../components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "../components/ui/dropdown-menu";
import { 
  Home, 
  User, 
  LogOut, 
  Menu, 
  X, 
  BookOpen,
  Library,
  Settings
} from "lucide-react";
import { ThemeToggle } from "../components/ThemeToggle";

const MainLayout = () => {
  const { currentUser, logout, isAdmin } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Handle logout
  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  // Get first letter of name to display in Avatar when no image
  const getInitials = (name: string) => {
    return name?.charAt(0).toUpperCase() || "U";
  };

  // Menu items for navigation
  const navigationItems = [
    { path: "/", label: "Home", icon: <Home className="w-5 h-5" /> },
    { path: "/borrow", label: "Borrow Books", icon: <BookOpen className="w-5 h-5" /> },
    { path: "/my-books", label: "My Borrowing Books", icon: <Library className="w-5 h-5" /> },
  ];

  // Admin menu item - only shown to superusers
  const adminMenuItem = { 
    path: "/admin", 
    label: "Admin Dashboard", 
    icon: <Settings className="w-5 h-5" /> 
  };

  // Check if menu item is active
  const isActive = (path: string) => {
    return location.pathname === path || 
           (path === "/" && location.pathname === "") || 
           (path === "/admin" && location.pathname.startsWith("/admin"));
  };

  // Toggle mobile menu
  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col dark:bg-gray-900">
      {/* Header */}
      <header className="bg-white shadow dark:bg-gray-800 dark:text-gray-100">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <Library className="h-8 w-8 text-blue-600 dark:text-blue-400" />
              <h1 className="text-xl font-bold text-gray-900 dark:text-white">Library Management System</h1>
            </div>
            
            {/* Desktop Navigation */}
            <nav className="hidden md:flex items-center space-x-6">
              {navigationItems.map((item) => (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`text-sm font-medium ${
                    isActive(item.path) 
                      ? "text-blue-600 dark:text-blue-400" 
                      : "text-gray-600 hover:text-blue-600 dark:text-gray-300 dark:hover:text-blue-400"
                  }`}
                >
                  {item.label}
                </Link>
              ))}
            </nav>
            
            <div className="flex items-center gap-3">
              {/* Theme Toggle */}
              <ThemeToggle />
              
              {/* Mobile menu button */}
              <button
                onClick={toggleMobileMenu}
                className="md:hidden p-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
              >
                {isMobileMenuOpen ? (
                  <X className="w-6 h-6" />
                ) : (
                  <Menu className="w-6 h-6" />
                )}
              </button>
              
              {/* User menu - shown on desktop and mobile */}
              {currentUser && (
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium hidden md:inline dark:text-gray-200">
                    {currentUser.fullName}
                  </span>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" className="rounded-full p-0 w-10 h-10">
                        <Avatar>
                          <AvatarFallback>{getInitials(currentUser.fullName)}</AvatarFallback>
                        </Avatar>
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem asChild>
                        <Link to="/profile" className="cursor-pointer">
                          <User className="w-4 h-4 mr-2" />
                          Profile
                        </Link>
                      </DropdownMenuItem>
                      {isAdmin && (
                        <DropdownMenuItem asChild>
                          <Link to="/admin" className="cursor-pointer">
                            <Settings className="w-4 h-4 mr-2" />
                            Admin Dashboard
                          </Link>
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuItem onClick={handleLogout} className="cursor-pointer">
                        <LogOut className="w-4 h-4 mr-2" />
                        Logout
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Mobile navigation menu */}
      {isMobileMenuOpen && (
        <div className="md:hidden bg-white shadow-md dark:bg-gray-800">
          <div className="container mx-auto px-4 py-2">
            <nav className="space-y-1">
              {navigationItems.map((item) => (
                <Link
                  key={item.path}
                  to={item.path}
                  className={`flex items-center p-3 rounded-md ${
                    isActive(item.path) 
                      ? "bg-blue-50 text-blue-600 font-medium dark:bg-blue-900/20 dark:text-blue-400" 
                      : "hover:bg-gray-50 dark:hover:bg-gray-700"
                  }`}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <span className="mr-3">{item.icon}</span>
                  {item.label}
                </Link>
              ))}

              {/* Admin Dashboard Link in Mobile Menu - Only visible to admin users */}
              {isAdmin && (
                <Link
                  to={adminMenuItem.path}
                  className={`flex items-center p-3 rounded-md ${
                    isActive(adminMenuItem.path) 
                      ? "bg-blue-50 text-blue-600 font-medium dark:bg-blue-900/20 dark:text-blue-400" 
                      : "hover:bg-gray-50 dark:hover:bg-gray-700"
                  }`}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <span className="mr-3">{adminMenuItem.icon}</span>
                  {adminMenuItem.label}
                </Link>
              )}
            </nav>
          </div>
        </div>
      )}

      {/* Main content */}
      <main className="flex-1 dark:bg-gray-900 dark:text-gray-100">
        <div className="container mx-auto px-4 py-6">
          <Outlet />
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-white shadow-sm mt-auto dark:bg-gray-800 dark:text-gray-400">
        <div className="container mx-auto px-4 py-4 text-center text-gray-600 text-sm dark:text-gray-400">
          &copy; {new Date().getFullYear()} Library Management System
        </div>
      </footer>
    </div>
  );
};

export default MainLayout;