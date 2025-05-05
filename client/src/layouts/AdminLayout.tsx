import { Outlet } from "react-router-dom";
import { AdminHeader } from "../pages/admin/Header";
import { AdminSidebar } from "../pages/admin/Sidebar";
import { useAuth } from "../context/AuthContext";

export const AdminLayout = () => {
    const { isInitializing } = useAuth();

    if (isInitializing) {
        return <div className="flex items-center justify-center h-screen">Loading admin dashboard...</div>;
    }

    return (
        <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
            <AdminSidebar />
            <div className="flex flex-col flex-1">
                <AdminHeader />
                <main className="flex-1 p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};