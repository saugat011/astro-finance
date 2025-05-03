using AstroFinance.API.Controllers;
using AstroFinance.Application.Customers.Commands.CreateCustomer;
using AstroFinance.Application.Customers.Commands.DeleteCustomer;
using AstroFinance.Application.Customers.Commands.UpdateCustomer;
using AstroFinance.Application.Customers.Queries.GetCustomerById;
using AstroFinance.Application.Customers.Queries.GetCustomersList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// This is a temporary file to test if we can build just the CustomersController
// It's not meant to be used in production
namespace AstroFinance.API
{
    public class TempProject
    {
        public static void Main()
        {
            Console.WriteLine("This is a temporary project to test if we can build just the CustomersController");
        }
    }
}