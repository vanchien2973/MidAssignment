import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { User } from "../../types/user";
import { useToast } from "../../hooks/use-toast";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "../ui/card";
import { Separator } from "../ui/separator";
import { Avatar, AvatarFallback } from "../ui/avatar";
import { Button } from "../ui/button";
import { Label } from "../ui/label";
import { Input } from "../ui/input";

// Schema for profile update form
const updateProfileSchema = z.object({
  fullName: z.string().min(2, "Full name must be at least 2 characters"),
  email: z.string().email("Invalid email format"),
});

type UpdateProfileFormData = z.infer<typeof updateProfileSchema>;

interface GeneralTabProps {
  userProfile: User;
  updateProfile: (data: { fullName?: string; email?: string }) => Promise<{ success: boolean; message?: string }>;
}

const GeneralTab = ({ userProfile, updateProfile }: GeneralTabProps) => {
  const [isEditing, setIsEditing] = useState(false);
  const { toast } = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<UpdateProfileFormData>({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      fullName: userProfile?.fullName || "",
      email: userProfile?.email || "",
    },
  });

  const onSubmit = async (data: UpdateProfileFormData) => {
    try {
      const response = await updateProfile({
        fullName: data.fullName,
        email: data.email
      });
      
      if (response.success) {
        toast({
          title: "Update successful",
          description: "Your information has been updated",
        });
      } else {
        toast({
          title: "Update failed",
          description: response.message || "An error occurred",
          variant: "destructive",
        });
      }
      setIsEditing(false);
    } catch (err) {
      console.error("Update profile error:", err);
      toast({
        title: "Update failed",
        description: "An error occurred, please try again later",
        variant: "destructive",
      });
    }
  };

  const handleCancel = () => {
    setIsEditing(false);
    // Reset form to original values
    if (userProfile) {
      reset({
        fullName: userProfile.fullName,
        email: userProfile.email,
      });
    }
  };

  // Get first letter of name to display in Avatar when no image
  const getInitials = (name: string = "") => {
    return name.charAt(0).toUpperCase();
  };

  return (
    <Card className="dark:border-gray-700">
      <CardHeader>
        <CardTitle className="dark:text-white">General Information</CardTitle>
        <CardDescription className="dark:text-gray-400">
          Your personal information and profile settings
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col md:flex-row gap-8">
          <div className="flex flex-col items-center space-y-4">
            <Avatar className="w-24 h-24">
              <AvatarFallback className="text-xl bg-blue-100 text-blue-600 dark:bg-blue-900 dark:text-blue-200">
                {getInitials(userProfile.fullName)}
              </AvatarFallback>
            </Avatar>
            {!isEditing && (
              <Button variant="outline" onClick={() => setIsEditing(true)}>
                Edit Information
              </Button>
            )}
          </div>

          <div className="flex-1">
            {isEditing ? (
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="fullName" className="dark:text-gray-300">Full Name</Label>
                  <Input
                    id="fullName"
                    {...register("fullName")}
                    className="dark:bg-gray-800 dark:border-gray-700"
                  />
                  {errors.fullName && (
                    <p className="text-sm text-red-500">{errors.fullName.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email" className="dark:text-gray-300">Email</Label>
                  <Input
                    id="email"
                    type="email"
                    {...register("email")}
                    className="dark:bg-gray-800 dark:border-gray-700"
                  />
                  {errors.email && (
                    <p className="text-sm text-red-500">{errors.email.message}</p>
                  )}
                </div>

                <div className="flex space-x-2 pt-4">
                  <Button type="submit">Save Changes</Button>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={handleCancel}
                  >
                    Cancel
                  </Button>
                </div>
              </form>
            ) : (
              <div className="space-y-4">
                <div>
                  <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400">Username</h3>
                  <p className="mt-1 dark:text-white">{userProfile.username}</p>
                </div>
                <Separator className="my-2" />
                <div>
                  <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400">Full Name</h3>
                  <p className="mt-1 dark:text-white">{userProfile.fullName}</p>
                </div>
                <Separator className="my-2" />
                <div>
                  <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400">Email</h3>
                  <p className="mt-1 dark:text-white">{userProfile.email}</p>
                </div>
                <Separator className="my-2" />
                <div>
                  <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400">Account Type</h3>
                  <p className="mt-1 dark:text-white">{userProfile.userType || "Normal User"}</p>
                </div>
                <Separator className="my-2" />
                <div>
                  <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400">Status</h3>
                  <div className="mt-1 flex items-center">
                    <span className={`inline-block w-2 h-2 rounded-full mr-2 ${userProfile.isActive ? "bg-green-500" : "bg-red-500"}`}></span>
                    <span className="dark:text-white">{userProfile.isActive ? "Active" : "Inactive"}</span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default GeneralTab; 