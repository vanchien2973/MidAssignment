import { useEffect, useState } from "react";
import { Category } from "../../../types/category";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "../../../components/ui/dialog";
import { Input } from "../../../components/ui/input";
import { Button } from "../../../components/ui/button";
import { useToast } from "../../../hooks/use-toast";

interface EditCategoryDialogProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    onEdit: (name: string, description: string) => Promise<{success: boolean, message: string}>;
    category: Category | null;
}
  
export const EditCategoryDialog = ({ 
  isOpen, onOpenChange, onEdit, category 
}: EditCategoryDialogProps) => {
  const [categoryName, setCategoryName] = useState("");
  const [categoryDescription, setCategoryDescription] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { toast } = useToast();

  useEffect(() => {
    if (category) {
      setCategoryName(category.categoryName);
      setCategoryDescription(category.description || "");
    }
  }, [category]);

  const handleSubmit = async () => {
    // Simple validation
    if (categoryName.trim().length < 3) {
      toast({
        title: "Error",
        description: "Category name must be at least 3 characters long",
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
      const result = await onEdit(categoryName, categoryDescription);
      if (result.success) {
        toast({
          title: "Success",
          description: result.message || "Category updated successfully",
        });
        onOpenChange(false);
      } else {
        toast({
          title: "Error",
          description: result.message || "Failed to update category",
          variant: "destructive",
        });
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "An unexpected error occurred while updating the category",
        variant: "destructive",
      });
      console.error(error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => {
      onOpenChange(open);
    }}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Edit Category</DialogTitle>
          <DialogDescription>
            Update the category details.
          </DialogDescription>
        </DialogHeader>
        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <label htmlFor="edit-name" className="text-sm font-medium">
              Category Name
            </label>
            <Input
              id="edit-name"
              value={categoryName}
              onChange={(e) => setCategoryName(e.target.value)}
            />
          </div>
          <div className="grid gap-2">
            <label htmlFor="edit-description" className="text-sm font-medium">
              Description
            </label>
            <Input
              id="edit-description"
              value={categoryDescription}
              onChange={(e) => setCategoryDescription(e.target.value)}
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => {
            onOpenChange(false);
          }} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!categoryName.trim() || isSubmitting}>
            {isSubmitting ? "Updating..." : "Update Category"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}