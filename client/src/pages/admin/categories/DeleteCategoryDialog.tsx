import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "../../../components/ui/alert-dialog";
import { Category } from "../../../types/category";

interface DeleteCategoryDialogProps {
    isOpen: boolean;
    onOpenChange: (open: boolean) => void;
    onDelete: () => void;
    category: Category | null;
  }
  
  export const DeleteCategoryDialog =({ 
    isOpen, onOpenChange, onDelete, category 
  }: DeleteCategoryDialogProps) => {
    return (
      <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This will permanently delete the category "{category?.categoryName}".
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => onOpenChange(false)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction onClick={onDelete}>
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    );
  }