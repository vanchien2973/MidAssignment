import { User, UserType } from "../../../types/user";
import { Button } from "../../../components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
} from "../../../components/ui/dialog";
import { Input } from "../../../components/ui/input";
import { Label } from "../../../components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "../../../components/ui/select";
import { Switch } from "../../../components/ui/switch";
import { useState, useEffect } from "react";
import { AlertCircle } from "lucide-react";
import { Alert, AlertDescription } from "../../../components/ui/alert";

interface UserEditDialogProps {
    isOpen: boolean;
    onClose: () => void;
    user: User | null;
    onSave: (user: User) => void;
}

export function UserEditDialog({
    isOpen,
    onClose,
    user,
    onSave
}: UserEditDialogProps) {
    const [userForm, setUserForm] = useState<User | null>(null);
    const [errors, setErrors] = useState<Record<string, string>>({});

    useEffect(() => {
        if (user) {
            setUserForm({ ...user });
            setErrors({});
        }
    }, [user]);

    if (!userForm) return null;

    const handleChange = (field: keyof User, value: string | boolean) => {
        setUserForm(prev => {
            if (!prev) return prev;
            return { ...prev, [field]: value };
        });

        // Clear error for the changing field
        if (errors[field]) {
            setErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[field];
                return newErrors;
            });
        }
    };

    const validateForm = (): boolean => {
        const newErrors: Record<string, string> = {};
        
        if (!userForm.fullName || userForm.fullName.trim() === '') {
            newErrors.fullName = 'Full name cannot be empty';
        }
        
        if (!userForm.email || userForm.email.trim() === '') {
            newErrors.email = 'Email cannot be empty';
        } else if (!/^\S+@\S+\.\S+$/.test(userForm.email)) {
            newErrors.email = 'Invalid email format';
        }
        
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = () => {
        if (validateForm() && userForm) {
            onSave(userForm);
        }
    };

    const isSuperUser = userForm.userType === 'SuperUser';

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-[500px]">
                <DialogHeader>
                    <DialogTitle>Edit User</DialogTitle>
                    <DialogDescription>
                        Edit information and permissions for user {userForm.fullName}.
                    </DialogDescription>
                </DialogHeader>

                <div className="grid gap-4 py-4">
                    <div className="grid gap-2">
                        <Label htmlFor="fullName">Full Name</Label>
                        <Input
                            id="fullName"
                            value={userForm.fullName}
                            onChange={(e) => handleChange("fullName", e.target.value)}
                        />
                        {errors.fullName && (
                            <p className="text-sm text-destructive">{errors.fullName}</p>
                        )}
                    </div>

                    <div className="grid gap-2">
                        <Label htmlFor="email">Email</Label>
                        <Input
                            id="email"
                            type="email"
                            value={userForm.email}
                            onChange={(e) => handleChange("email", e.target.value)}
                        />
                        {errors.email && (
                            <p className="text-sm text-destructive">{errors.email}</p>
                        )}
                    </div>

                    <div className="grid gap-2">
                        <Label htmlFor="userType">Role</Label>
                        <Select
                            value={userForm.userType}
                            onValueChange={(value) => handleChange("userType", value as UserType)}
                        >
                            <SelectTrigger id="userType">
                                <SelectValue placeholder="Select role" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="SuperUser">Administrator</SelectItem>
                                <SelectItem value="NormalUser">Regular User</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="flex items-center gap-2">
                        <Switch
                            id="isActive"
                            checked={userForm.isActive}
                            onCheckedChange={(checked) => handleChange("isActive", checked)}
                            disabled={isSuperUser} // Prevent disabling SuperUser accounts
                        />
                        <Label htmlFor="isActive">Activate account</Label>
                    </div>

                    {isSuperUser && !userForm.isActive && (
                        <Alert>
                            <AlertCircle className="h-4 w-4" />
                            <AlertDescription>
                                Administrator accounts cannot be deactivated.
                            </AlertDescription>
                        </Alert>
                    )}
                </div>

                <DialogFooter>
                    <Button variant="outline" onClick={onClose}>
                        Cancel
                    </Button>
                    <Button onClick={handleSubmit}>
                        Save Changes
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
