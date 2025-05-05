import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";

const NotFoundPage = () => {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 text-center">
      <h1 className="text-6xl font-bold text-gray-900 mb-4">404</h1>
      <h2 className="text-2xl font-medium text-gray-700 mb-8">
        Page not found
      </h2>
      <p className="text-gray-600 mb-8 max-w-md">
        The page you are looking for may have been removed, renamed, or is temporarily unavailable.
      </p>
      <Button asChild>
        <Link to="/">Return to home page</Link>
      </Button>
    </div>
  );
};

export default NotFoundPage; 