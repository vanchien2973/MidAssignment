import { useState } from "react";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "../../../components/ui/dialog";
import { Button } from "../../../components/ui/button";
import { Plus } from "lucide-react";
import { Input } from "../../../components/ui/input";
import { useToast } from "../../../hooks/use-toast";

interface AddCategoryDialogProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    onAdd: (name: string, description: string) => Promise<{success: boolean, message: string}>;
}

export const AddCategoryDialog = ({ isOpen, onOpenChange, onAdd }: AddCategoryDialogProps) => {
    const [categoryName, setCategoryName] = useState("");
    const [categoryDescription, setCategoryDescription] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { toast } = useToast();

    const resetForm = () => {
        setCategoryName("");
        setCategoryDescription("");
        setIsSubmitting(false);
    };

    const handleSubmit = async () => {
        // Simple validation
        if (categoryName.trim().length < 3) {
            toast({
                title: "Error",
                description: "Category name must be at least 3 characters",
                variant: "destructive",
            });
            return;
        }

        if (categoryName.trim().length > 100) {
            toast({
                title: "Error",
                description: "Category name cannot exceed 100 characters",
                variant: "destructive",
            });
            return;
        }

        if (categoryDescription.trim().length > 500) {
            toast({
                title: "Error",
                description: "Description cannot exceed 500 characters",
                variant: "destructive",
            });
            return;
        }

        setIsSubmitting(true);
        
        try {
            const result = await onAdd(categoryName, categoryDescription);
            if (result.success) {
                toast({
                    title: "Success",
                    description: result.message || "Category added successfully",
                });
                resetForm();
                onOpenChange(false);
            } else {
                toast({
                    title: "Error",
                    description: result.message || "Failed to add category",
                    variant: "destructive",
                });
            }
        } catch (error) {
            toast({
                title: "Error",
                description: "An unexpected error occurred while adding the category",
                variant: "destructive",
            });
            console.error(error);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={(open) => {
            if (!open) resetForm();
            onOpenChange(open);
        }}>
            <DialogTrigger asChild>
                <Button className="flex items-center gap-2">
                    <Plus size={16} /> Add Category
                </Button>
            </DialogTrigger>
            <DialogContent>
                <DialogHeader>
                    <DialogTitle>Add New Category</DialogTitle>
                    <DialogDescription>
                        Create a new category for organizing books in the library.
                    </DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                    <div className="grid gap-2">
                        <label htmlFor="name" className="text-sm font-medium">
                            Category Name
                        </label>
                        <Input
                            id="name"
                            value={categoryName}
                            onChange={(e) => setCategoryName(e.target.value)}
                            placeholder="Enter category name"
                        />
                    </div>
                    <div className="grid gap-2">
                        <label htmlFor="description" className="text-sm font-medium">
                            Description
                        </label>
                        <Input
                            id="description"
                            value={categoryDescription}
                            onChange={(e) => setCategoryDescription(e.target.value)}
                            placeholder="Enter category description"
                        />
                    </div>
                </div>
                <DialogFooter>
                    <Button variant="outline" onClick={() => {
                        resetForm();
                        onOpenChange(false);
                    }} disabled={isSubmitting}>
                        Cancel
                    </Button>
                    <Button onClick={handleSubmit} disabled={!categoryName.trim() || isSubmitting}>
                        {isSubmitting ? "Saving..." : "Save Category"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
