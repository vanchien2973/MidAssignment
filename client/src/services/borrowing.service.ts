import api from '../config/axios';
import { getDetailedErrorMessage } from '../utils/errorUtils';
import {
  BookBorrowingRequestDto,
  BorrowingResults,
  CreateBorrowingRequestDto,
  BorrowingRequestStatusUpdateDto,
  ReturnBookDto,
  ExtendBorrowingDto,
  ApiResponse,
  BorrowingRequestStatus,
  BorrowingDetailStatus,
} from '../types/borrowing';
import { MutationResponse } from '../types/api';

interface MutationResponseWithId extends MutationResponse {
  id?: string;
}

interface RawBorrowingDetail {
  status?: string | number;
  statusName?: string;
  [key: string]: any;
}

interface RawBorrowingRequest {
  status?: string | number;
  statusName?: string;
  requestDetails?: RawBorrowingDetail[];
  details?: RawBorrowingDetail[];
  [key: string]: any;
}

class BorrowingService {
  // Function to normalize API data to match client model
  private normalizeBookBorrowingData = (data: any): any => {
    // If array, normalize each item
    if (Array.isArray(data)) {
      return data.map(item => this.normalizeBookBorrowingData(item));
    }

    // If object, process a borrowing request
    if (data && typeof data === 'object') {
      const rawData = data as RawBorrowingRequest;
      
      // Process request status
      let status = rawData.status as number;
      if (typeof rawData.status === 'string') {
        // Convert from string to number
        const statusString = rawData.status.toLowerCase();
        if (statusString === 'waiting') status = BorrowingRequestStatus.Waiting;
        else if (statusString === 'approved') status = BorrowingRequestStatus.Approved;
        else if (statusString === 'rejected') status = BorrowingRequestStatus.Rejected;
      } else if (typeof status !== 'number' && rawData.statusName) {
        // Use statusName if available
        const statusNameString = rawData.statusName.toLowerCase();
        if (statusNameString === 'waiting') status = BorrowingRequestStatus.Waiting;
        else if (statusNameString === 'approved') status = BorrowingRequestStatus.Approved;
        else if (statusNameString === 'rejected') status = BorrowingRequestStatus.Rejected;
      }

      // Normalize details or requestDetails
      let details: any[] = [];
      if (rawData.requestDetails && Array.isArray(rawData.requestDetails)) {
        details = rawData.requestDetails.map((detail: RawBorrowingDetail) => {
          let detailStatus = detail.status as number;
          
          if (typeof detail.status === 'string') {
            // Convert from string to number
            const detailStatusString = detail.status.toLowerCase();
            if (detailStatusString === 'borrowing') detailStatus = BorrowingDetailStatus.Borrowing;
            else if (detailStatusString === 'returned') detailStatus = BorrowingDetailStatus.Returned;
            else if (detailStatusString === 'extended') detailStatus = BorrowingDetailStatus.Extended;
          } else if (typeof detailStatus !== 'number' && detail.statusName) {
            // Use statusName if available
            const detailStatusNameString = detail.statusName.toLowerCase();
            if (detailStatusNameString === 'borrowing') detailStatus = BorrowingDetailStatus.Borrowing;
            else if (detailStatusNameString === 'returned') detailStatus = BorrowingDetailStatus.Returned;
            else if (detailStatusNameString === 'extended') detailStatus = BorrowingDetailStatus.Extended;
          }

          return {
            ...detail,
            status: detailStatus
          };
        });
      } else if (rawData.details && Array.isArray(rawData.details)) {
        details = this.normalizeBookBorrowingData(rawData.details) as any[];
      }

      // Return normalized data
      return {
        ...rawData,
        status,
        details,
        requestDetails: undefined
      };
    }

    // If not object or array, return original value
    return data;
  };

  // Get borrowing requests for a specific user
  getUserBorrowingRequests = async (
    userId: number,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<ApiResponse<BorrowingResults>> => {
    try {
      const limitedPageSize = Math.min(pageSize, 50);
      
      const url = `/Borrowing/user/${userId}?pageNumber=${pageNumber}&pageSize=${limitedPageSize}`;
      
      const response = await api.get(url);
      
      // Normalize data
      let normalizedData: BorrowingResults;
      
      if (response.data && typeof response.data === 'object' && response.data.results) {
        normalizedData = {
          ...response.data,
          results: this.normalizeBookBorrowingData(response.data.results) as BookBorrowingRequestDto[]
        };
      } else if (Array.isArray(response.data)) {
        normalizedData = {
          totalCount: response.data.length,
          pageNumber: pageNumber,
          pageSize: limitedPageSize,
          results: this.normalizeBookBorrowingData(response.data) as BookBorrowingRequestDto[]
        };
      } else {
        normalizedData = {
          totalCount: 0,
          pageNumber: pageNumber,
          pageSize: limitedPageSize,
          results: []
        };
      }
      
      return {
        success: true,
        data: normalizedData,
        totalCount: normalizedData.totalCount,
        pageNumber: normalizedData.pageNumber,
        pageSize: normalizedData.pageSize
      };
    } catch (error: unknown) {
      console.error(`Error fetching borrowing requests for user ${userId}:`, error);
      return {
        success: false,
        data: {
          totalCount: 0,
          pageNumber: pageNumber,
          pageSize: pageSize,
          results: []
        },
        message: getDetailedErrorMessage(error, 'Failed to fetch user borrowing requests')
      };
    }
  };

  // Get all borrowing requests (admin only)
  getAllBorrowingRequests = async (
    pageNumber: number = 1,
    pageSize: number = 20
  ): Promise<ApiResponse<BorrowingResults>> => {
    try {
      const url = `/Borrowing/all?pageNumber=${pageNumber}&pageSize=${pageSize}`;
      const response = await api.get(url);

      // Normalize data
      let normalizedData: BorrowingResults;
      
      if (response.data && typeof response.data === 'object' && response.data.results) {
        normalizedData = {
          ...response.data,
          results: this.normalizeBookBorrowingData(response.data.results) as BookBorrowingRequestDto[],
          totalCount: response.data.totalCount || 0
        };
      } else if (Array.isArray(response.data)) {
        normalizedData = {
          totalCount: response.data.length,
          pageNumber: pageNumber,
          pageSize: pageSize,
          results: this.normalizeBookBorrowingData(response.data) as BookBorrowingRequestDto[]
        };
      } else {
        normalizedData = {
          totalCount: 0,
          pageNumber: pageNumber,
          pageSize: pageSize,
          results: []
        };
      }
      
      return {
        success: true,
        data: normalizedData,
        totalCount: normalizedData.totalCount
      };
    } catch (error: unknown) {
      console.error('Error fetching all borrowing requests:', error);
      return {
        success: false,
        data: {
          totalCount: 0,
          pageNumber: pageNumber,
          pageSize: pageSize,
          results: []
        },
        message: getDetailedErrorMessage(error, 'Failed to fetch all borrowing requests')
      };
    }
  };

  createBorrowingRequest = async (requestData: CreateBorrowingRequestDto): Promise<MutationResponseWithId> => {
    try {
      const response = await api.post('/Borrowing', requestData);
      return {
        success: true,
        message: response.data.message || 'Borrowing request created successfully',
        id: response.data.requestId
      };
    } catch (error: unknown) {
      console.error('Error creating borrowing request:', error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to create borrowing request')
      };
    }
  };

  // Update borrowing request (admin only)
  updateBorrowingRequestStatus = async (statusData: BorrowingRequestStatusUpdateDto): Promise<MutationResponse> => {
    try {
      const response = await api.put('/Borrowing/status', statusData);
      return {
        success: true,
        message: response.data.message || 'Borrowing request status updated successfully'
      };
    } catch (error: unknown) {
      console.error(`Error updating borrowing request status ${statusData.requestId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to update borrowing request status')
      };
    }
  };

  // Return book
  returnBook = async (returnData: ReturnBookDto): Promise<MutationResponse> => {
    try {
      // Ensure notes are not undefined
      const dataToSend = {
        ...returnData,
        notes: returnData.notes || ""
      };
      
      const response = await api.put('/Borrowing/return', dataToSend);
      return {
        success: true,
        message: response.data.message || 'Book returned successfully'
      };
    } catch (error: unknown) {
      console.error(`Error returning book ${returnData.detailId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to return book')
      };
    }
  };

  // Extend borrowing period
  extendBorrowing = async (extendData: ExtendBorrowingDto): Promise<MutationResponse> => {
    try {
      const dataToSend = {
        ...extendData,
        notes: extendData.notes || ""
      };
      
      const response = await api.put('/Borrowing/extend', dataToSend);
      return {
        success: true,
        message: response.data.message || 'Borrowing period extended successfully'
      };
    } catch (error: unknown) {
      console.error(`Error extending borrowing period ${extendData.detailId}:`, error);
      return {
        success: false,
        message: getDetailedErrorMessage(error, 'Failed to extend borrowing period')
      };
    }
  };
}

export default new BorrowingService();
