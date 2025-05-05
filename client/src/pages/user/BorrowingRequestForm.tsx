import { Book } from "../../types/book";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { Textarea } from "../../components/ui/textarea";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "../../components/ui/tooltip";
import { BookOpen as BookIcon, X, Loader2 } from "lucide-react";

interface BorrowingRequestFormProps {
  selectedBooks: Book[];
  onRemoveBook: (bookId: number) => void;
  notes: string;
  setNotes: (notes: string) => void;
  onSubmit: () => void;
  isSubmitting: boolean;
  requestsThisMonth: number;
  maxBooksPerRequest: number;
  maxRequestsPerMonth: number;
}

export const BorrowingRequestForm = ({
  selectedBooks,
  onRemoveBook,
  notes,
  setNotes,
  onSubmit,
  isSubmitting,
  requestsThisMonth,
  maxBooksPerRequest,
  maxRequestsPerMonth
}: BorrowingRequestFormProps) => {
  return (
    <TooltipProvider>
      <Card className="dark:border-gray-700">
        <CardHeader>
          <CardTitle className="dark:text-white">Your Request</CardTitle>
          <CardDescription className="dark:text-gray-400">
            You can borrow up to {maxBooksPerRequest} books per request
          </CardDescription>
        </CardHeader>
        <CardContent>
          {selectedBooks.length > 0 ? (
            <ul className="space-y-3">
              {selectedBooks.map(book => (
                <li key={book.bookId} className="flex items-center justify-between gap-2 border-b dark:border-gray-700 pb-2">
                  <div className="flex items-center gap-2">
                    <BookIcon className="h-5 w-5 text-blue-500 shrink-0 dark:text-blue-400" />
                    <div className="text-sm">
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <div className="font-medium line-clamp-1 dark:text-white">{book.title || 'Untitled Book'}</div>
                        </TooltipTrigger>
                        <TooltipContent side="bottom" className="bg-black/80 text-white dark:bg-white/90 dark:text-gray-900">
                          {book.title || 'Untitled Book'}
                        </TooltipContent>
                      </Tooltip>
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <div className="text-muted-foreground line-clamp-1 dark:text-gray-400">{book.author || 'Unknown Author'}</div>
                        </TooltipTrigger>
                        <TooltipContent side="bottom" className="bg-black/80 text-white dark:bg-white/90 dark:text-gray-900">
                          {book.author || 'Unknown Author'}
                        </TooltipContent>
                      </Tooltip>
                    </div>
                  </div>
                  <Button 
                    variant="ghost" 
                    size="icon" 
                    className="h-8 w-8 text-gray-500 hover:text-red-500 dark:text-gray-400 dark:hover:text-red-400"
                    onClick={() => onRemoveBook(book.bookId)}
                  >
                    <X size={16} />
                  </Button>
                </li>
              ))}
            </ul>
          ) : (
            <div className="flex flex-col items-center justify-center py-6 text-center">
              <BookIcon className="h-10 w-10 text-muted-foreground mb-2" />
              <p className="text-sm text-muted-foreground dark:text-gray-400">
                No books selected yet
              </p>
            </div>
          )}

          <div className="mt-4">
            <label htmlFor="notes" className="block text-sm font-medium mb-2 dark:text-white">
              Reason for Borrowing <span className="text-red-500">*</span>
            </label>
            <Textarea
              id="notes"
              placeholder="Please provide a reason for your borrowing request..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="w-full dark:bg-gray-800 dark:border-gray-700"
              rows={3}
            />
          </div>
        </CardContent>
        <CardFooter className="flex flex-col items-stretch space-y-3">
          <div className="text-sm text-muted-foreground dark:text-gray-400">
            {selectedBooks.length} of {maxBooksPerRequest} books selected
          </div>
          <div className="text-sm text-muted-foreground dark:text-gray-400">
            {requestsThisMonth} of {maxRequestsPerMonth} monthly requests used
          </div>
          <Button 
            onClick={onSubmit}
            disabled={
              selectedBooks.length === 0 ||
              !notes.trim() ||
              requestsThisMonth >= maxRequestsPerMonth ||
              isSubmitting
            }
            className="w-full"
            style={{ backgroundColor: selectedBooks.length > 0 ? '#4f46e5' : undefined }}
          >
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Submitting...
              </>
            ) : (
              "Submit Request"
            )}
          </Button>
        </CardFooter>
      </Card>
    </TooltipProvider>
  );
}; 