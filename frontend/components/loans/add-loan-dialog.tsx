"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Loader2, CreditCard, TrendingDown, AlertCircle } from "lucide-react"
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { formatNepaliCurrency } from "@/lib/format"
import { CustomerSelector } from "./customer-selector"
import { useToast } from "@/components/ui/use-toast"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { Customer, CreateLoanRequest, Loan, LoanStatus, LoanType, PaymentSchedule } from "@/lib/api/types"
import { loanService } from "@/lib/api/services"
import { useFormSubmit } from "@/hooks/use-form-submit"

interface AddLoanDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onLoanAdded: (loan: Loan) => void
}

export function AddLoanDialog({ open, onOpenChange, onLoanAdded }: AddLoanDialogProps) {
  const [loanType, setLoanType] = useState<LoanType>(LoanType.Flat)
  const [selectedCustomerId, setSelectedCustomerId] = useState<string>("")
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | undefined>()
  const [calculationLoading, setCalculationLoading] = useState(false)
  const [calculationError, setCalculationError] = useState<string | null>(null)
  const [paymentSchedule, setPaymentSchedule] = useState<PaymentSchedule[]>([])
  const { toast } = useToast()

  const [loanData, setLoanData] = useState({
    amount: 100000,
    interestRate: 13.5,
    term: 12,
    startDate: new Date().toISOString().split("T")[0],
    status: LoanStatus.Pending,
    emi: 0,
    totalRepayment: 0,
    totalInterest: 0,
  })

  // Form submission handler
  const { handleSubmit, isSubmitting } = useFormSubmit<CreateLoanRequest, Loan>(
    async (data) => {
      const response = await loanService.createLoan(data)
      return response.data
    },
    {
      onSuccess: (loan) => {
        onLoanAdded(loan)
        resetForm()
        onOpenChange(false)
      },
      successMessage: "Loan created successfully",
    }
  )

  // Reset form when dialog is closed
  useEffect(() => {
    if (!open) {
      resetForm()
    }
  }, [open])

  // Calculate loan details whenever relevant fields change
  useEffect(() => {
    if (loanData.amount > 0 && loanData.interestRate > 0 && loanData.term > 0 && loanData.startDate) {
      calculateLoan()
    }
  }, [loanData.amount, loanData.interestRate, loanData.term, loanData.startDate, loanType])

  const resetForm = () => {
    setLoanType(LoanType.Flat)
    setSelectedCustomerId("")
    setSelectedCustomer(undefined)
    setLoanData({
      amount: 100000,
      interestRate: 13.5,
      term: 12,
      startDate: new Date().toISOString().split("T")[0],
      status: LoanStatus.Pending,
      emi: 0,
      totalRepayment: 0,
      totalInterest: 0,
    })
    setPaymentSchedule([])
    setCalculationError(null)
  }

  const calculateLoan = async () => {
    try {
      setCalculationLoading(true)
      setCalculationError(null)

      // Call API to calculate loan details
      const response = await loanService.calculateLoan({
        amount: loanData.amount,
        interestRate: loanData.interestRate,
        term: loanData.term,
        type: loanType,
        startDate: loanData.startDate,
      })

      if (response.success && response.data) {
        const { emi, totalRepayment, totalInterest, paymentSchedule } = response.data
        
        setLoanData({
          ...loanData,
          emi,
          totalRepayment,
          totalInterest,
        })
        
        setPaymentSchedule(paymentSchedule)
      }
    } catch (error) {
      console.error("Error calculating loan:", error)
      setCalculationError("Failed to calculate loan details. Please check your inputs.")
    } finally {
      setCalculationLoading(false)
    }
  }

  const handleCustomerSelect = (customerId: string, customer?: Customer) => {
    setSelectedCustomerId(customerId)
    setSelectedCustomer(customer)
  }

  const handleInputChange = (field: string, value: any) => {
    setLoanData({ ...loanData, [field]: value })
  }

  const handleLoanTypeChange = (value: string) => {
    setLoanType(value as LoanType)
  }

  const handleCreateLoan = () => {
    // Validate form
    if (!selectedCustomerId) {
      toast({
        title: "Validation Error",
        description: "Please select a customer",
        variant: "destructive",
      })
      return
    }

    if (loanData.amount <= 0) {
      toast({
        title: "Validation Error",
        description: "Loan amount must be greater than zero",
        variant: "destructive",
      })
      return
    }

    // Create loan request object
    const loanRequest: CreateLoanRequest = {
      customerId: selectedCustomerId,
      amount: loanData.amount,
      interestRate: loanData.interestRate,
      term: loanData.term,
      startDate: loanData.startDate,
      type: loanType,
    }

    // Submit the form
    handleSubmit(loanRequest)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Create New Loan</DialogTitle>
          <DialogDescription>Fill in the details to create a new loan.</DialogDescription>
        </DialogHeader>

        <Tabs value={loanType} onValueChange={handleLoanTypeChange}>
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value={LoanType.Flat} className="flex items-center gap-2">
              <CreditCard className="h-4 w-4" />
              Flat Rate Loan
            </TabsTrigger>
            <TabsTrigger value={LoanType.Diminishing} className="flex items-center gap-2">
              <TrendingDown className="h-4 w-4" />
              Diminishing Rate Loan
            </TabsTrigger>
          </TabsList>

          <div className="mt-4">
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="customer">Customer <span className="text-red-500">*</span></Label>
                <CustomerSelector 
                  value={selectedCustomerId} 
                  onChange={handleCustomerSelect} 
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="amount">Loan Amount <span className="text-red-500">*</span></Label>
                  <Input
                    id="amount"
                    type="number"
                    placeholder="100000"
                    value={loanData.amount}
                    onChange={(e) => handleInputChange("amount", Number(e.target.value))}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="interestRate">Interest Rate (%) <span className="text-red-500">*</span></Label>
                  <Input
                    id="interestRate"
                    type="number"
                    step="0.01"
                    placeholder="13.5"
                    value={loanData.interestRate}
                    onChange={(e) => handleInputChange("interestRate", Number(e.target.value))}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="term">Term (Months) <span className="text-red-500">*</span></Label>
                  <Select 
                    value={loanData.term.toString()} 
                    onValueChange={(value) => handleInputChange("term", Number(value))}
                  >
                    <SelectTrigger id="term">
                      <SelectValue placeholder="Select term" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="3">3 months</SelectItem>
                      <SelectItem value="6">6 months</SelectItem>
                      <SelectItem value="12">12 months</SelectItem>
                      <SelectItem value="24">24 months</SelectItem>
                      <SelectItem value="36">36 months</SelectItem>
                      <SelectItem value="48">48 months</SelectItem>
                      <SelectItem value="60">60 months</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="startDate">Start Date <span className="text-red-500">*</span></Label>
                  <Input
                    id="startDate"
                    type="date"
                    value={loanData.startDate}
                    onChange={(e) => handleInputChange("startDate", e.target.value)}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="status">Status</Label>
                  <Select 
                    value={loanData.status} 
                    onValueChange={(value) => handleInputChange("status", value as LoanStatus)}
                  >
                    <SelectTrigger id="status">
                      <SelectValue placeholder="Select status" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={LoanStatus.Pending}>Pending</SelectItem>
                      <SelectItem value={LoanStatus.Active}>Active</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>

            {calculationError && (
              <Alert variant="destructive" className="mt-4">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{calculationError}</AlertDescription>
              </Alert>
            )}

            <div className="mt-6 p-4 border rounded-md bg-slate-50 dark:bg-slate-800">
              <div className="flex justify-between items-center">
                <div>
                  <h3 className="text-sm font-medium">Calculated EMI</h3>
                  <p className="text-xs text-muted-foreground">Monthly installment amount</p>
                </div>
                <div className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
                  {calculationLoading ? (
                    <Loader2 className="h-5 w-5 animate-spin" />
                  ) : (
                    formatNepaliCurrency(loanData.emi)
                  )}
                </div>
              </div>
              <div className="mt-4 grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-muted-foreground">Total Repayment</p>
                  <p className="text-sm font-medium">
                    {calculationLoading ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      formatNepaliCurrency(loanData.totalRepayment)
                    )}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Total Interest</p>
                  <p className="text-sm font-medium">
                    {calculationLoading ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      formatNepaliCurrency(loanData.totalInterest)
                    )}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </Tabs>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleCreateLoan}
            disabled={isSubmitting || !selectedCustomerId || loanData.amount <= 0 || calculationLoading}
            className="bg-emerald-600 hover:bg-emerald-700"
          >
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Creating...
              </>
            ) : (
              "Create Loan"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
