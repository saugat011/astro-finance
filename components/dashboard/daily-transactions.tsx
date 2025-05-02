"use client"

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { formatNepaliCurrency } from "@/lib/format"
import { ArrowDownLeft, ArrowUpRight, ChevronRight } from "lucide-react"

// Sample transaction data
const transactionsData = [
  {
    id: "1",
    clientName: "Rajesh Kumar",
    type: "deposit",
    amount: 15000,
    accountType: "SB",
    time: "09:15 AM",
    date: "Today",
  },
  {
    id: "2",
    clientName: "Priya Sharma",
    type: "withdrawal",
    amount: 8000,
    accountType: "BB",
    time: "10:30 AM",
    date: "Today",
  },
  {
    id: "3",
    clientName: "Amit Singh",
    type: "deposit",
    amount: 25000,
    accountType: "MB",
    time: "11:45 AM",
    date: "Today",
  },
  {
    id: "4",
    clientName: "Sunita Patel",
    type: "withdrawal",
    amount: 12500,
    accountType: "SB",
    time: "01:20 PM",
    date: "Today",
  },
  {
    id: "5",
    clientName: "Rahul Verma",
    type: "deposit",
    amount: 5000,
    accountType: "BB",
    time: "03:45 PM",
    date: "Yesterday",
  },
  {
    id: "6",
    clientName: "Deepika Reddy",
    type: "deposit",
    amount: 18000,
    accountType: "SB",
    time: "04:30 PM",
    date: "Yesterday",
  },
]

export function DailyTransactions() {
  // Calculate totals
  const todayDeposits = transactionsData
    .filter((t) => t.date === "Today" && t.type === "deposit")
    .reduce((sum, t) => sum + t.amount, 0)

  const todayWithdrawals = transactionsData
    .filter((t) => t.date === "Today" && t.type === "withdrawal")
    .reduce((sum, t) => sum + t.amount, 0)

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Daily Transactions</CardTitle>
            <CardDescription>Client deposits and withdrawals</CardDescription>
          </div>
          <div className="flex gap-4 text-sm">
            <div className="flex flex-col items-end">
              <span className="text-muted-foreground">Today's Deposits</span>
              <span className="font-medium text-emerald-600 dark:text-emerald-400">
                {formatNepaliCurrency(todayDeposits)}
              </span>
            </div>
            <div className="flex flex-col items-end">
              <span className="text-muted-foreground">Today's Withdrawals</span>
              <span className="font-medium text-red-600 dark:text-red-400">
                {formatNepaliCurrency(todayWithdrawals)}
              </span>
            </div>
          </div>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        <div className="space-y-1">
          {transactionsData.map((transaction, index) => (
            <div
              key={transaction.id}
              className={`flex items-center justify-between p-3 hover:bg-slate-50 dark:hover:bg-slate-800/50 ${
                index !== 0 ? "border-t border-slate-200 dark:border-slate-700" : ""
              }`}
            >
              <div className="flex items-center gap-3">
                <Avatar className="h-9 w-9">
                  <AvatarImage
                    src={`/placeholder.svg?height=36&width=36&text=${transaction.clientName.charAt(0)}`}
                    alt={transaction.clientName}
                  />
                  <AvatarFallback>{transaction.clientName.charAt(0)}</AvatarFallback>
                </Avatar>
                <div>
                  <p className="text-sm font-medium">{transaction.clientName}</p>
                  <div className="flex items-center gap-2">
                    <Badge variant="outline" className="text-xs font-normal">
                      {transaction.accountType === "SB"
                        ? "Sadaran Bachat"
                        : transaction.accountType === "BB"
                          ? "Baal Bachat"
                          : "Masik Bachat"}
                    </Badge>
                    <span className="text-xs text-muted-foreground">
                      {transaction.time} • {transaction.date}
                    </span>
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="text-right">
                  <p
                    className={`text-sm font-medium ${
                      transaction.type === "deposit"
                        ? "text-emerald-600 dark:text-emerald-400"
                        : "text-red-600 dark:text-red-400"
                    }`}
                  >
                    {transaction.type === "deposit" ? "+" : "-"}
                    {formatNepaliCurrency(transaction.amount)}
                  </p>
                  <p className="text-xs text-muted-foreground capitalize">{transaction.type}</p>
                </div>
                <div
                  className={`flex h-8 w-8 items-center justify-center rounded-full ${
                    transaction.type === "deposit"
                      ? "bg-emerald-100 dark:bg-emerald-900/20"
                      : "bg-red-100 dark:bg-red-900/20"
                  }`}
                >
                  {transaction.type === "deposit" ? (
                    <ArrowUpRight className={`h-4 w-4 text-emerald-600 dark:text-emerald-400`} />
                  ) : (
                    <ArrowDownLeft className={`h-4 w-4 text-red-600 dark:text-red-400`} />
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
        <div className="flex items-center justify-center p-3 border-t border-slate-200 dark:border-slate-700">
          <Button variant="ghost" size="sm" className="gap-1">
            View All Transactions <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
