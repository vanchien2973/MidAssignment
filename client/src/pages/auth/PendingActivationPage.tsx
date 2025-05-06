import { Button } from "../../components/ui/button";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "../../components/ui/card";
import { useNavigate } from "react-router-dom";
import { Clock } from "lucide-react";

export default function PendingActivationPage() {
  const navigate = useNavigate();
  
  return (
    <div className="flex items-center justify-center min-h-screen px-4 py-8 bg-gray-50 dark:bg-gray-900">
      <Card className="w-full max-w-md mx-auto shadow-lg">
        <CardHeader className="space-y-1">
          <div className="flex justify-center mb-4">
            <Clock className="h-12 w-12 text-amber-500" />
          </div>
          <CardTitle className="text-2xl font-bold text-center">Account Activation Pending</CardTitle>
          <CardDescription className="text-center">
            Your account has been successfully registered and is pending administrator activation.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4 text-center">
          <p className="text-muted-foreground">
            Please check your email regularly. We will notify you when your account has been activated.
          </p>
          <p className="text-muted-foreground">
            If you don't receive an email within 24 hours, please contact the administrator for assistance.
          </p>
        </CardContent>
        <CardFooter className="flex justify-center">
          <Button 
            variant="outline" 
            onClick={() => navigate("/login")}
          >
            Return to login page
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
} 