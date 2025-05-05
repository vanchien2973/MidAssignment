import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useToast } from "../../hooks/use-toast";
import UserService from "../../services/user.service";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "../ui/card";
import { Button } from "../ui/button";
import { Label } from "../ui/label";
import { Input } from "../ui/input";

const updatePasswordSchema = z.object({
  currentPassword: z.string().min(6, "Current password is required"),
  newPassword: z.string().min(6, "New password must be at least 6 characters"),
  confirmPassword: z.string().min(6, "Confirm password is required")
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: "Passwords do not match",
  path: ["confirmPassword"]
});

type UpdatePasswordFormData = z.infer<typeof updatePasswordSchema>;

const SecurityTab = () => {
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const { toast } = useToast();

  const passwordForm = useForm<UpdatePasswordFormData>({
    resolver: zodResolver(updatePasswordSchema),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: ""
    }
  });

  const handlePasswordSubmit = async (data: UpdatePasswordFormData) => {
    try {
      const response = await UserService.updatePassword({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword
      });
      
      if (response.success) {
        toast({
          title: "Update successful",
          description: "Your password has been updated",
        });
        setShowPasswordForm(false);
        passwordForm.reset();
      } else {
        toast({
          title: "Update failed",
          description: response.message || "An error occurred",
          variant: "destructive",
        });
      }
    } catch (err) {
      console.error("Update password error:", err);
      toast({
        title: "Update failed",
        description: "An error occurred, please try again later",
        variant: "destructive",
      });
    }
  };

  return (
    <Card className="dark:border-gray-700">
      <CardHeader>
        <CardTitle className="dark:text-white">Security Settings</CardTitle>
        <CardDescription className="dark:text-gray-400">
          Manage your password and security preferences
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div>
            <h3 className="text-lg font-medium dark:text-white">Change Password</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              Update your password to keep your account secure
            </p>
            {!showPasswordForm ? (
              <Button variant="outline" className="mt-4" onClick={() => setShowPasswordForm(true)}>
                Change Password
              </Button>
            ) : (
              <form onSubmit={passwordForm.handleSubmit(handlePasswordSubmit)} className="space-y-4 mt-4">
                <div className="space-y-2">
                  <Label htmlFor="currentPassword" className="dark:text-gray-300">Current Password</Label>
                  <Input
                    id="currentPassword"
                    type="password"
                    {...passwordForm.register("currentPassword")}
                    className="dark:bg-gray-800 dark:border-gray-700"
                  />
                  {passwordForm.formState.errors.currentPassword && (
                    <p className="text-sm text-red-500">{passwordForm.formState.errors.currentPassword.message}</p>
                  )}
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="newPassword" className="dark:text-gray-300">New Password</Label>
                  <Input
                    id="newPassword"
                    type="password"
                    {...passwordForm.register("newPassword")}
                    className="dark:bg-gray-800 dark:border-gray-700"
                  />
                  {passwordForm.formState.errors.newPassword && (
                    <p className="text-sm text-red-500">{passwordForm.formState.errors.newPassword.message}</p>
                  )}
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="confirmPassword" className="dark:text-gray-300">Confirm New Password</Label>
                  <Input
                    id="confirmPassword"
                    type="password"
                    {...passwordForm.register("confirmPassword")}
                    className="dark:bg-gray-800 dark:border-gray-700"
                  />
                  {passwordForm.formState.errors.confirmPassword && (
                    <p className="text-sm text-red-500">{passwordForm.formState.errors.confirmPassword.message}</p>
                  )}
                </div>
                
                <div className="flex space-x-2 pt-4">
                  <Button type="submit">Save Password</Button>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => {
                      setShowPasswordForm(false);
                      passwordForm.reset();
                    }}
                  >
                    Cancel
                  </Button>
                </div>
              </form>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default SecurityTab; 