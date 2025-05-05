import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Formik, Form, FormikHelpers } from "formik";
import * as Yup from "yup";
import { useAuth } from "../../context/AuthContext";
import { Button } from "../ui/button";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { useToast } from "../../hooks/use-toast";
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "../ui/card";
import { EyeIcon, EyeOffIcon, AlertCircle } from "lucide-react";
import { LoginRequest } from "../../types/auth";

// Định nghĩa schema xác thực với Yup
const LoginSchema = Yup.object().shape({
  username: Yup.string().required("Username is required"),
  password: Yup.string().required("Password is required"),
});

// Giá trị khởi tạo
const initialValues: LoginRequest = {
  username: "",
  password: "",
};

const LoginForm = () => {
  const { login, isLoginLoading } = useAuth();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (
    values: LoginRequest,
    { setSubmitting }: FormikHelpers<LoginRequest>
  ) => {
    if (isLoginLoading) return;
    
    try {
      setError(null);
      
      const response = await login(values);
      
      if (!response.success) {
        setError(response.message || "Login failed. Please try again!");
        toast({
          title: "Error",
          description: response.message || "Login failed. Please try again!",
          variant: "destructive",
        });
      } else {
        toast({
          title: "Success",
          description: "Login successfully!",
        });
        navigate("/");
      }
    } catch (error: unknown) {
      const errorMessage = "An unexpected error occurred. Please try again later.";
      setError(errorMessage);
      toast({
        title: "Error",
        description: errorMessage,
        variant: "destructive",
      });
      console.error(error);
    } finally {
      setSubmitting(false);
    }
  };

  // Handle navigation to register page
  const handleRegisterClick = (e: React.MouseEvent<HTMLAnchorElement>) => {
    e.preventDefault();
    navigate("/register");
  };

  return (
    <Card className="w-full max-w-md mx-auto shadow-lg">
      <CardHeader className="space-y-1">
        <CardTitle className="text-2xl font-bold text-center">Login</CardTitle>
        <CardDescription className="text-center">
          Please login to continue
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Formik
          initialValues={initialValues}
          validationSchema={LoginSchema}
          onSubmit={handleSubmit}
        >
          {({ errors, touched, isSubmitting, values, handleChange, handleBlur, handleSubmit: formikHandleSubmit }) => (
            <Form onSubmit={formikHandleSubmit} className="space-y-4">
              {error && (
                <div className="bg-destructive/20 text-destructive p-3 rounded-md flex items-start space-x-2">
                  <AlertCircle className="h-5 w-5 mt-0.5" />
                  <div>
                    <div className="font-medium">Error</div>
                    <div className="text-sm">{error}</div>
                  </div>
                </div>
              )}
              
              <div className="grid gap-2">
                <Label htmlFor="username">Username</Label>
                <div className="relative">
                  <Input
                    id="username"
                    name="username"
                    type="text"
                    placeholder="Enter your username"
                    value={values.username}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    className={errors.username && touched.username ? "border-red-500" : ""}
                    autoComplete="username"
                  />
                </div>
                {errors.username && touched.username && (
                  <p className="text-sm text-red-500">{errors.username}</p>
                )}
              </div>

              <div className="grid gap-2">
                <Label htmlFor="password">Password</Label>
                <div className="relative">
                  <Input
                    id="password"
                    name="password"
                    type={showPassword ? "text" : "password"}
                    placeholder="Enter your password"
                    value={values.password}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    className={errors.password && touched.password ? "border-red-500 pr-10" : "pr-10"}
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? (
                      <EyeOffIcon className="h-4 w-4" />
                    ) : (
                      <EyeIcon className="h-4 w-4" />
                    )}
                  </button>
                </div>
                {errors.password && touched.password && (
                  <p className="text-sm text-red-500">{errors.password}</p>
                )}
              </div>

              <div className="flex justify-end">
                <a
                  href="#"
                  className="text-sm text-blue-600 hover:text-blue-500 hover:underline"
                  onClick={(e) => e.preventDefault()}
                >
                  Forgot password?
                </a>
              </div>

              <Button
                type="submit"
                className="w-full"
                disabled={isSubmitting || isLoginLoading}
              >
                {isLoginLoading || isSubmitting ? "Logging in..." : "Login"}
              </Button>
            </Form>
          )}
        </Formik>
      </CardContent>
      <CardFooter className="flex justify-center">
        <p className="text-sm text-gray-600">
          Don't have an account?{" "}
          <a
            href="/register"
            className="font-medium text-blue-600 hover:text-blue-500 hover:underline"
            onClick={handleRegisterClick}
          >
            Register
          </a>
        </p>
      </CardFooter>
    </Card>
  );
};

export default LoginForm;
