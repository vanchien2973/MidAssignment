import { Card, CardContent, CardHeader, CardTitle } from "../../components/ui/card";

interface BorrowingRulesProps {
  maxBooksPerRequest: number;
  maxRequestsPerMonth: number;
}

export const BorrowingRules = ({
  maxBooksPerRequest,
  maxRequestsPerMonth
}: BorrowingRulesProps) => {
  return (
    <Card className="dark:border-gray-700">
      <CardHeader>
        <CardTitle className="dark:text-white">Information</CardTitle>
      </CardHeader>
      <CardContent className="text-sm space-y-2 dark:text-gray-300">
        <p>
          <span className="font-medium">Monthly limit:</span> {maxRequestsPerMonth} borrowing requests
        </p>
        <p>
          <span className="font-medium">Per request:</span> Up to {maxBooksPerRequest} books
        </p>
        <p>
          <span className="font-medium">Loan period:</span> 30 days
        </p>
        <p>
          <span className="font-medium">Late fees:</span> Apply after due date
        </p>
      </CardContent>
    </Card>
  );
}; 