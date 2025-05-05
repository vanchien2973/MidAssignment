import { useState, useEffect } from "react";
import { Category } from "../../../types/category";
import { useToast } from "../../../hooks/use-toast";
import { AddCategoryDialog } from "./AddCategoryDialog";
import { Input } from "../../../components/ui/input";
import { CategoryTable } from "./CategoryTable";
import { EditCategoryDialog } from "./EditCategoryDialog";
import { DeleteCategoryDialog } from "./DeleteCategoryDialog";
import CategoryService from "../../../services/category.service";
import { Loader2, File } from "lucide-react";
import { Button } from '../../../components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../components/ui/card";

export const CategoriesPage = () => {
    const [categories, setCategories] = useState<Category[]>([]);
    const [filteredCategories, setFilteredCategories] = useState<Category[]>([]);
    const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
    const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
    const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
    const [currentCategory, setCurrentCategory] = useState<Category | null>(null);
    const [searchTerm, setSearchTerm] = useState("");
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    
    // Pagination
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [totalItems, setTotalItems] = useState(0);
    const [sortBy] = useState<string | undefined>(undefined);
    const [sortOrder] = useState<string | undefined>(undefined);
    
    const { toast } = useToast();
    
    // Fetch categories from API
    const fetchCategories = async (page = 1, size = pageSize, search = searchTerm) => {
      setIsLoading(true);
      setError(null);
      
      try {
        const response = await CategoryService.getCategories(
          page,
          size,
          sortBy,
          sortOrder,
          search
        );
        
        if (response.success) {
          setCategories(response.data);
          setFilteredCategories(response.data);
          if (response.totalCount) {
            setTotalItems(response.totalCount);
          }
          setCurrentPage(page);
        } else {
          setError(response.message || 'Failed to fetch categories');
          toast({
            title: "Error",
            description: response.message || 'Failed to fetch categories',
            variant: "destructive",
          });
        }
      } catch (err) {
        console.error('Error loading categories:', err);
        setError('An unexpected error occurred');
        toast({
          title: "Error",
          description: 'An unexpected error occurred while fetching categories',
          variant: "destructive",
        });
      } finally {
        setIsLoading(false);
      }
    };
    
    // Load categories on component mount
    useEffect(() => {
      fetchCategories(currentPage, pageSize, '');
    }, []); 

    // Load categories when search term changes, with debounce
    useEffect(() => {
      const debounceTimer = setTimeout(() => {
        if (searchTerm !== undefined) {
          // When searching, always start from page 1
          fetchCategories(1, pageSize, searchTerm);
        }
      }, 500); // 500ms debounce
      
      return () => clearTimeout(debounceTimer);
    }, [searchTerm]);
    
    const handleAddCategory = async (name: string, description: string) => {
      const categoryData = {
        categoryName: name,
        description: description
      };
      
      try {
        const response = await CategoryService.createCategory(categoryData);
        
        if (response.success) {
          // Refresh categories list
          fetchCategories(currentPage, pageSize);
          
          toast({
            title: "Category Added",
            description: response.message,
          });
        }
        
        return response;
      } catch (err) {
        console.log(err);
        const errorMessage = "An unexpected error occurred while adding category";
        
        toast({
          title: "Error",
          description: errorMessage,
          variant: "destructive",
        });
        
        return {
          success: false,
          message: errorMessage
        };
      }
    };
    
    const handleEditCategory = async (name: string, description: string) => {
      if (!currentCategory) return {
        success: false,
        message: "No category selected"
      };
      
      const categoryData = {
        categoryId: currentCategory.categoryId.toString(),
        categoryName: name,
        description: description
      };
      
      try {
        const response = await CategoryService.updateCategory(categoryData);
        
        if (response.success) {
          // Refresh categories list
          fetchCategories(currentPage, pageSize);
          setCurrentCategory(null);
          
          toast({
            title: "Category Updated",
            description: response.message,
          });
        }
        
        return response;
      } catch (err) {
        console.log(err);
        const errorMessage = "An unexpected error occurred while updating category";
        
        toast({
          title: "Error",
          description: errorMessage,
          variant: "destructive",
        });
        
        return {
          success: false,
          message: errorMessage
        };
      }
    };
    
    const handleDeleteCategory = async () => {
      if (!currentCategory) return;
      
      try {
        const response = await CategoryService.deleteCategory(currentCategory.categoryId.toString());
        
        if (response.success) {
          // Refresh categories list and maintain current page unless this was the last item on the page
          const isLastItemOnPage = categories.length === 1 && currentPage > 1;
          const pageToFetch = isLastItemOnPage ? currentPage - 1 : currentPage;
          
          fetchCategories(pageToFetch, pageSize);
          setIsDeleteDialogOpen(false);
          setCurrentCategory(null);
          
          toast({
            title: "Category Deleted",
            description: response.message,
          });
        } else {
          toast({
            title: "Error",
            description: response.message,
            variant: "destructive",
          });
        }
      } catch (err) {
        console.log(err)
        toast({
          title: "Error",
          description: "An unexpected error occurred while deleting category",
          variant: "destructive",
        });
      }
    };
    
    const openEditDialog = (category: Category) => {
      setCurrentCategory(category);
      setIsEditDialogOpen(true);
    };
    
    const openDeleteDialog = (category: Category) => {
      setCurrentCategory(category);
      setIsDeleteDialogOpen(true);
    };
    
    // Handle page change
    const handlePageChange = (page: number) => {
      console.log("Changing to page:", page);
      setCurrentPage(page);
      fetchCategories(page, pageSize, searchTerm);
    };

    // Handle page size change
    const handlePageSizeChange = (size: number) => {
      setPageSize(size);
      setCurrentPage(1);
      fetchCategories(1, size, searchTerm);
    };
    
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">Category Management</h2>
            <p className="text-muted-foreground">
              Manage book categories in the library
            </p>
          </div>
          <Button className="flex items-center gap-1">
            <AddCategoryDialog 
              isOpen={isAddDialogOpen}
              onOpenChange={setIsAddDialogOpen}
              onAdd={handleAddCategory}
            />
          </Button>
        </div>
        
        <div className="flex items-center py-4">
          <Input
            placeholder="Search categories..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="max-w-sm"
          />
        </div>
        
        {isLoading ? (
          <div className="flex justify-center items-center py-8">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
          </div>
        ) : error ? (
          <div className="py-8 text-center">
            <p className="text-destructive">{error}</p>
          </div>
        ) : (
          <>
            <Card>
              <CardHeader className="flex flex-row items-center gap-4">
                <div className="bg-primary p-3 rounded-lg">
                  <File className="h-6 w-6 text-primary-foreground" />
                </div>
                <div>
                  <CardTitle>Category List</CardTitle>
                  <CardDescription>
                    Manage and organize book categories
                  </CardDescription>
                </div>
              </CardHeader>
              <CardContent>
                <CategoryTable 
                  categories={filteredCategories} 
                  onEdit={openEditDialog}
                  onDelete={openDeleteDialog}
                  currentPage={currentPage}
                  pageSize={pageSize}
                  totalItems={totalItems}
                  onPageChange={handlePageChange}
                  onPageSizeChange={handlePageSizeChange}
                />
              </CardContent>
            </Card>
          </>
        )}
        
        <EditCategoryDialog 
          isOpen={isEditDialogOpen}
          onOpenChange={setIsEditDialogOpen}
          onEdit={handleEditCategory}
          category={currentCategory}
        />
        
        <DeleteCategoryDialog 
          isOpen={isDeleteDialogOpen}
          onOpenChange={setIsDeleteDialogOpen}
          onDelete={handleDeleteCategory}
          category={currentCategory}
        />
      </div>
    );
  }