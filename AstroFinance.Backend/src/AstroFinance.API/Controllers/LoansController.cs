using AstroFinance.Application.Loans.Commands.ApproveLoan;
using AstroFinance.Application.Loans.Commands.CreateLoan;
using AstroFinance.Application.Loans.Commands.DeleteLoan;
using AstroFinance.Application.Loans.Commands.DisburseLoan;
using AstroFinance.Application.Loans.Commands.UpdateLoan;
using AstroFinance.Application.Loans.Queries.GetLoanById;
using AstroFinance.Application.Loans.Queries.GetLoansList;
using AstroFinance.Application.Loans.Queries.GetPaymentSchedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AstroFinance.API.Controllers
{
    [Authorize]
    public class LoansController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetLoansListQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await Mediator.Send(new GetLoanByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpGet("{id}/schedule")]
        public async Task<IActionResult> GetPaymentSchedule(Guid id)
        {
            var result = await Mediator.Send(new GetPaymentScheduleQuery { LoanId = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateLoanCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteLoanCommand { Id = id });
            return NoContent();
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            await Mediator.Send(new ApproveLoanCommand { Id = id });
            return NoContent();
        }

        [HttpPost("{id}/disburse")]
        public async Task<IActionResult> Disburse(Guid id, DisburseCommand command)
        {
            if (id != command.LoanId)
            {
                return BadRequest();
            }

            await Mediator.Send(command);
            return NoContent();
        }
    }
}