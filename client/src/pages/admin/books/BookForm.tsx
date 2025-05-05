import { Input } from "../../../components/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../../components/ui/select";
import { Category } from "../../../types/category";

interface BookFormProps {
  title: string;
  author: string;
  categoryId: string | null;
  isbn: string;
  publishedYear: number | null;
  publisher: string;
  description: string;
  totalCopies: number;
  categories: Category[];
  onTitleChange: (value: string) => void;
  onAuthorChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onIsbnChange: (value: string) => void;
  onPublishedYearChange: (value: number | null) => void;
  onPublisherChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onTotalCopiesChange: (value: number) => void;
}

export const BookForm =({
  title,
  author,
  categoryId,
  isbn,
  publishedYear,
  publisher,
  description,
  totalCopies,
  categories,
  onTitleChange,
  onAuthorChange,
  onCategoryChange,
  onIsbnChange,
  onPublishedYearChange,
  onPublisherChange,
  onDescriptionChange,
  onTotalCopiesChange
}: BookFormProps) => {
  return (
    <div className="grid gap-4 py-4">
      <div className="grid grid-cols-2 gap-4">
        <div className="grid gap-2">
          <label htmlFor="title" className="text-sm font-medium">
            Title *
          </label>
          <Input
            id="title"
            value={title}
            onChange={(e) => onTitleChange(e.target.value)}
            placeholder="Enter book title"
          />
        </div>
        <div className="grid gap-2">
          <label htmlFor="author" className="text-sm font-medium">
            Author *
          </label>
          <Input
            id="author"
            value={author}
            onChange={(e) => onAuthorChange(e.target.value)}
            placeholder="Enter author name"
          />
        </div>
      </div>
      
      <div className="grid grid-cols-2 gap-4">
        <div className="grid gap-2">
          <label htmlFor="category" className="text-sm font-medium">
            Category *
          </label>
          <Select 
            value={categoryId || ""} 
            onValueChange={(value) => onCategoryChange(value)}
          >
            <SelectTrigger>
              <SelectValue placeholder="Select a category" />
            </SelectTrigger>
            <SelectContent>
              <SelectGroup>
                {categories.map((category) => (
                  <SelectItem 
                    key={category.categoryId} 
                    value={category.categoryId.toString()}
                  >
                    {category.categoryName}
                  </SelectItem>
                ))}
              </SelectGroup>
            </SelectContent>
          </Select>
        </div>
        <div className="grid gap-2">
          <label htmlFor="isbn" className="text-sm font-medium">
            ISBN *
          </label>
          <Input
            id="isbn"
            value={isbn}
            onChange={(e) => onIsbnChange(e.target.value)}
            placeholder="Enter ISBN"
          />
        </div>
      </div>
      
      <div className="grid grid-cols-2 gap-4">
        <div className="grid gap-2">
          <label htmlFor="publishedYear" className="text-sm font-medium">
            Published Year
          </label>
          <Input
            id="publishedYear"
            type="number"
            value={publishedYear || ""}
            onChange={(e) => onPublishedYearChange(e.target.value ? Number(e.target.value) : null)}
            placeholder="Enter published year"
          />
        </div>
        <div className="grid gap-2">
          <label htmlFor="publisher" className="text-sm font-medium">
            Publisher
          </label>
          <Input
            id="publisher"
            value={publisher}
            onChange={(e) => onPublisherChange(e.target.value)}
            placeholder="Enter publisher name"
          />
        </div>
      </div>
      
      <div className="grid gap-2">
        <label htmlFor="description" className="text-sm font-medium">
          Description
        </label>
        <Input
          id="description"
          value={description}
          onChange={(e) => onDescriptionChange(e.target.value)}
          placeholder="Enter book description"
        />
      </div>
      
      <div className="grid gap-2">
        <label htmlFor="totalCopies" className="text-sm font-medium">
          Number of Copies *
        </label>
        <Input
          id="totalCopies"
          type="number"
          min="1"
          value={totalCopies}
          onChange={(e) => onTotalCopiesChange(Number(e.target.value))}
        />
      </div>
    </div>
  );
}