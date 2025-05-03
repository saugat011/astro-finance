"use client"

import { useState, useEffect } from "react"
import { Check, ChevronsUpDown, Loader2, PlusCircle, Search, User } from "lucide-react"
import { Button } from "@/components/ui/button"
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "@/components/ui/command"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { cn } from "@/lib/utils"
import { Customer } from "@/lib/api/types"
import { customerService } from "@/lib/api/services"
import { useToast } from "@/components/ui/use-toast"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"

interface CustomerSelectorProps {
  value?: string
  onChange: (value: string, customer?: Customer) => void
}

export function CustomerSelector({ value, onChange }: CustomerSelectorProps) {
  const [open, setOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")
  const [customers, setCustomers] = useState<Customer[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | undefined>()
  const [addCustomerOpen, setAddCustomerOpen] = useState(false)
  const { toast } = useToast()

  // New customer form state
  const [newCustomer, setNewCustomer] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phoneNumber: "",
    address: "",
    identificationNumber: "",
    identificationType: "National ID",
  })
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Load selected customer details if value is provided
  useEffect(() => {
    if (value && !selectedCustomer) {
      fetchCustomerById(value)
    }
  }, [value])

  // Search customers when query changes
  useEffect(() => {
    if (searchQuery.length > 2) {
      searchCustomers(searchQuery)
    }
  }, [searchQuery])

  const fetchCustomerById = async (id: string) => {
    try {
      setIsLoading(true)
      const response = await customerService.getCustomerById(id)
      if (response.success && response.data) {
        setSelectedCustomer(response.data)
      }
    } catch (error) {
      console.error("Error fetching customer:", error)
    } finally {
      setIsLoading(false)
    }
  }

  const searchCustomers = async (query: string) => {
    try {
      setIsLoading(true)
      const response = await customerService.searchCustomers(query)
      if (response.success && response.data) {
        setCustomers(response.data)
      }
    } catch (error) {
      console.error("Error searching customers:", error)
    } finally {
      setIsLoading(false)
    }
  }

  const handleSelect = (customerId: string) => {
    const customer = customers.find(c => c.id === customerId)
    setSelectedCustomer(customer)
    onChange(customerId, customer)
    setOpen(false)
  }

  const handleAddCustomer = async () => {
    try {
      setIsSubmitting(true)
      
      // Validate form
      if (!newCustomer.firstName || !newCustomer.lastName || !newCustomer.phoneNumber || 
          !newCustomer.identificationNumber || !newCustomer.identificationType) {
        toast({
          title: "Validation Error",
          description: "Please fill in all required fields",
          variant: "destructive",
        })
        return
      }
      
      const response = await customerService.createCustomer(newCustomer)
      
      if (response.success && response.data) {
        toast({
          title: "Success",
          description: "Customer created successfully",
        })
        
        // Select the newly created customer
        setSelectedCustomer(response.data)
        onChange(response.data.id, response.data)
        setAddCustomerOpen(false)
        
        // Reset form
        setNewCustomer({
          firstName: "",
          lastName: "",
          email: "",
          phoneNumber: "",
          address: "",
          identificationNumber: "",
          identificationType: "National ID",
        })
      }
    } catch (error) {
      console.error("Error creating customer:", error)
      toast({
        title: "Error",
        description: "Failed to create customer. Please try again.",
        variant: "destructive",
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            role="combobox"
            aria-expanded={open}
            className="w-full justify-between"
          >
            {isLoading ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : value && selectedCustomer ? (
              <>
                <User className="mr-2 h-4 w-4" />
                {selectedCustomer.fullName || `${selectedCustomer.firstName} ${selectedCustomer.lastName}`}
              </>
            ) : (
              "Select customer..."
            )}
            <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-[400px] p-0">
          <Command>
            <CommandInput 
              placeholder="Search customers..." 
              value={searchQuery}
              onValueChange={setSearchQuery}
              startIcon={<Search className="h-4 w-4" />}
            />
            <CommandList>
              <CommandEmpty>
                {isLoading ? (
                  <div className="flex items-center justify-center p-4">
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Searching...
                  </div>
                ) : (
                  <div className="p-4 text-center">
                    <p>No customers found.</p>
                    <p className="text-sm text-muted-foreground">Try a different search or add a new customer.</p>
                  </div>
                )}
              </CommandEmpty>
              <CommandGroup heading="Customers">
                {customers.map((customer) => (
                  <CommandItem
                    key={customer.id}
                    value={customer.id}
                    onSelect={handleSelect}
                    className="flex items-center"
                  >
                    <User className="mr-2 h-4 w-4" />
                    <div className="flex-1">
                      <p>{customer.fullName || `${customer.firstName} ${customer.lastName}`}</p>
                      <p className="text-xs text-muted-foreground">{customer.phoneNumber}</p>
                    </div>
                    <Check
                      className={cn(
                        "h-4 w-4",
                        value === customer.id ? "opacity-100" : "opacity-0"
                      )}
                    />
                  </CommandItem>
                ))}
              </CommandGroup>
              <CommandSeparator />
              <CommandGroup>
                <CommandItem
                  onSelect={() => {
                    setOpen(false)
                    setAddCustomerOpen(true)
                  }}
                >
                  <PlusCircle className="mr-2 h-4 w-4" />
                  Add new customer
                </CommandItem>
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>

      {/* Add Customer Dialog */}
      <Dialog open={addCustomerOpen} onOpenChange={setAddCustomerOpen}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>Add New Customer</DialogTitle>
            <DialogDescription>
              Enter customer details to create a new record.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="firstName">First Name <span className="text-red-500">*</span></Label>
                <Input
                  id="firstName"
                  value={newCustomer.firstName}
                  onChange={(e) => setNewCustomer({ ...newCustomer, firstName: e.target.value })}
                  placeholder="John"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName">Last Name <span className="text-red-500">*</span></Label>
                <Input
                  id="lastName"
                  value={newCustomer.lastName}
                  onChange={(e) => setNewCustomer({ ...newCustomer, lastName: e.target.value })}
                  placeholder="Doe"
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={newCustomer.email}
                onChange={(e) => setNewCustomer({ ...newCustomer, email: e.target.value })}
                placeholder="john.doe@example.com"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="phoneNumber">Phone Number <span className="text-red-500">*</span></Label>
              <Input
                id="phoneNumber"
                value={newCustomer.phoneNumber}
                onChange={(e) => setNewCustomer({ ...newCustomer, phoneNumber: e.target.value })}
                placeholder="+977 9812345678"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="address">Address</Label>
              <Input
                id="address"
                value={newCustomer.address}
                onChange={(e) => setNewCustomer({ ...newCustomer, address: e.target.value })}
                placeholder="123 Main St, Kathmandu"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="identificationType">ID Type <span className="text-red-500">*</span></Label>
                <Select
                  value={newCustomer.identificationType}
                  onValueChange={(value) => setNewCustomer({ ...newCustomer, identificationType: value })}
                >
                  <SelectTrigger id="identificationType">
                    <SelectValue placeholder="Select ID type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="National ID">National ID</SelectItem>
                    <SelectItem value="Passport">Passport</SelectItem>
                    <SelectItem value="Driver License">Driver License</SelectItem>
                    <SelectItem value="Voter ID">Voter ID</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-2">
                <Label htmlFor="identificationNumber">ID Number <span className="text-red-500">*</span></Label>
                <Input
                  id="identificationNumber"
                  value={newCustomer.identificationNumber}
                  onChange={(e) => setNewCustomer({ ...newCustomer, identificationNumber: e.target.value })}
                  placeholder="123456789"
                />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAddCustomerOpen(false)}>
              Cancel
            </Button>
            <Button 
              onClick={handleAddCustomer} 
              disabled={isSubmitting}
              className="bg-emerald-600 hover:bg-emerald-700"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Creating...
                </>
              ) : (
                "Create Customer"
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}