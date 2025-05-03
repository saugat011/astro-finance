import { useState } from 'react';
import { useToast } from '@/components/ui/use-toast';
import { getErrorMessage, formatApiData } from '@/lib/utils/api-utils';

interface UseFormSubmitOptions<T, R> {
  onSuccess?: (data: R) => void;
  onError?: (error: Error) => void;
  successMessage?: string;
  formatData?: (data: T) => any;
}

/**
 * Custom hook for handling form submissions with API integration
 */
export function useFormSubmit<T, R = any>(
  submitFn: (data: T) => Promise<R>,
  options: UseFormSubmitOptions<T, R> = {}
) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { toast } = useToast();
  
  const {
    onSuccess,
    onError,
    successMessage = 'Form submitted successfully',
    formatData = formatApiData,
  } = options;

  const handleSubmit = async (data: T) => {
    setIsSubmitting(true);
    
    try {
      // Format the data before submission if needed
      const formattedData = formatData(data);
      
      // Submit the data
      const result = await submitFn(formattedData);
      
      // Show success toast
      toast({
        title: 'Success',
        description: successMessage,
      });
      
      // Call onSuccess callback
      onSuccess?.(result);
      
      return result;
    } catch (error) {
      // Show error toast
      toast({
        title: 'Error',
        description: getErrorMessage(error),
        variant: 'destructive',
      });
      
      // Call onError callback
      if (error instanceof Error) {
        onError?.(error);
      } else {
        onError?.(new Error(getErrorMessage(error)));
      }
      
      throw error;
    } finally {
      setIsSubmitting(false);
    }
  };

  return {
    handleSubmit,
    isSubmitting,
  };
}