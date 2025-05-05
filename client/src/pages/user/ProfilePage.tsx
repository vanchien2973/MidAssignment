import { useUser } from "../../context/UserContext";
import { useAuth } from "../../context/AuthContext";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "../../components/ui/tabs";
import { useEffect } from "react";
import { User, Lock, Clock } from "lucide-react";
import GeneralTab from "../../components/profile/GeneralTab";
import SecurityTab from "../../components/profile/SecurityTab";
import ActivityTab from "../../components/profile/ActivityTab";

const ProfilePage = () => {
  const { userProfile, isLoading, error, updateProfile } = useUser();
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      window.location.href = '/login';
    }
  }, [isAuthenticated, isLoading]);

  if (!isAuthenticated) {
    return <div className="text-center py-8">Please login to view your profile. Redirecting to login page...</div>;
  }

  if (isLoading) {
    return <div className="text-center py-8">Loading user information...</div>;
  }

  if (error && !userProfile) {
    return (
      <div className="text-center py-8 space-y-4">
        <div className="text-red-500">{error}</div>
        <button 
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          onClick={() => window.location.reload()}
        >
          Retry
        </button>
      </div>
    );
  }

  if (!userProfile) {
    return (
      <div className="text-center py-8 space-y-4">
        <div>User information not found.</div>
        <button 
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
          onClick={() => window.location.reload()}
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight dark:text-white">Profile</h2>
        <p className="text-muted-foreground dark:text-gray-400">
          Manage your account settings and preferences
        </p>
      </div>
      
      <Tabs defaultValue="general" className="space-y-4">
        <TabsList className="grid w-full grid-cols-3 md:w-auto">
          <TabsTrigger value="general" className="flex items-center gap-2">
            <User className="h-4 w-4" /> General
          </TabsTrigger>
          <TabsTrigger value="security" className="flex items-center gap-2">
            <Lock className="h-4 w-4" /> Security
          </TabsTrigger>
          <TabsTrigger value="activity" className="flex items-center gap-2">
            <Clock className="h-4 w-4" /> Activity
          </TabsTrigger>
        </TabsList>
        
        <TabsContent value="general">
          <GeneralTab userProfile={userProfile} updateProfile={updateProfile} />
        </TabsContent>
        
        <TabsContent value="security">
          <SecurityTab />
        </TabsContent>
        
        <TabsContent value="activity">
          <ActivityTab />
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default ProfilePage;