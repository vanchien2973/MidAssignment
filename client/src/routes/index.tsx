import { createBrowserRouter } from "react-router-dom";
import { lazy, Suspense } from "react";
import MainLayout from "../layouts/MainLayout";
import AuthLayout from "../layouts/AuthLayout";
import ProtectedRoute from "./ProtectedRoute";
import AdminProtectedRoute from "./AdminProtectedRoute";
import HomePage from "../pages/HomePage";
import { Dashboard } from "../pages/admin/Dashboard";
import UsersPage from "../pages/admin/users/UsersPage";
import { CategoriesPage } from "../pages/admin/categories/CategoriesPage";
import { AdminLayout } from "../layouts/AdminLayout";
import { BorrowingRequestsPage } from "../pages/admin/borrowing-requests/BorrowingRequestsPage";
import { BooksPage } from "../pages/admin/books/BooksPage";

// Lazy load các trang
const LoginPage = lazy(() => import("../pages/auth/LoginPage"));
const RegisterPage = lazy(() => import("../pages/auth/RegisterPage"));
const ProfilePage = lazy(() => import("../pages/user/ProfilePage"));
const NotFoundPage = lazy(() => import("../pages/NotFoundPage"));

// Lazy load 
const BorrowBooksPage = lazy(() => import("../pages/user/BorrowBooksPage"));
const MyBorrowingBooksPage = lazy(() => import("../pages/user/MyBorrowingBooksPage"));


// Loading fallback
const LoadingFallback = () => <div className="p-4 text-center">Loading...</div>;

const router = createBrowserRouter([
  {
    path: "/",
    element: <AuthLayout />,
    children: [
      {
        path: "login",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <LoginPage />
          </Suspense>
        ),
      },
      {
        path: "register",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <RegisterPage />
          </Suspense>
        ),
      },
    ],
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: "profile",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <ProfilePage />
          </Suspense>
        ),
      },
      {
        path: "borrow",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <BorrowBooksPage />
          </Suspense>
        ),
      },
      {
        path: "my-books",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <MyBorrowingBooksPage />
          </Suspense>
        ),
      },
    ],
  },
  {
    path: "/admin",
    element: (
      <AdminProtectedRoute>
        <AdminLayout />
      </AdminProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <Dashboard />,
      },
      {
        path: "users",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <UsersPage />
          </Suspense>
        ),
      },
      {
        path: "borrowing-requests",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <BorrowingRequestsPage />
          </Suspense>
        ),
      },
      {
        path: "categories",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <CategoriesPage />
          </Suspense>
        ),
      },
      {
        path: "books",
        element: (
          <Suspense fallback={<LoadingFallback />}>
            <BooksPage />
          </Suspense>
        ),
      },
    ],
  },
  // Route cho 404 Not Found
  {
    path: "*",
    element: (
      <Suspense fallback={<LoadingFallback />}>
        <NotFoundPage />
      </Suspense>
    ),
  },
]);

export default router;