import { useState, useEffect, useCallback } from 'react';
import { getErrorMessage } from '@/lib/utils/api-utils';
import { useToast } from '@/components/ui/use-toast';

interface UseApiOptions<T> {
  onSuccess?: (data: T) => void;
  onError?: (error: Error) => void;
  showSuccessToast?: boolean;
  showErrorToast?: boolean;
  successMessage?: string;
}

/**
 * Custom hook for API data fetching with loading, error, and success states
 */
export function useApi<T>(
  apiFunction: () => Promise<T>,
  options: UseApiOptions<T> = {}
) {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const { toast } = useToast();

  const {
    onSuccess,
    onError,
    showSuccessToast = false,
    showErrorToast = true,
    successMessage = 'Operation completed successfully',
  } = options;

  const execute = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await apiFunction();
      setData(result);
      
      if (showSuccessToast) {
        toast({
          title: 'Success',
          description: successMessage,
        });
      }
      
      onSuccess?.(result);
      return result;
    } catch (err) {
      const error = err instanceof Error ? err : new Error('An unknown error occurred');
      setError(error);
      
      if (showErrorToast) {
        toast({
          title: 'Error',
          description: getErrorMessage(err),
          variant: 'destructive',
        });
      }
      
      onError?.(error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, [apiFunction, onSuccess, onError, showSuccessToast, showErrorToast, successMessage, toast]);

  return {
    data,
    isLoading,
    error,
    execute,
  };
}

/**
 * Custom hook for API data fetching that executes on mount
 */
export function useApiEffect<T>(
  apiFunction: () => Promise<T>,
  dependencies: any[] = [],
  options: UseApiOptions<T> = {}
) {
  const api = useApi(apiFunction, options);

  useEffect(() => {
    api.execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, dependencies);

  return api;
}