using AstroFinance.Application.Transactions.Commands.CreateTransaction;
using AstroFinance.Application.Transactions.Commands.DeleteTransaction;
using AstroFinance.Application.Transactions.Queries.GetTransactionById;
using AstroFinance.Application.Transactions.Queries.GetTransactionsList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AstroFinance.API.Controllers
{
    [Authorize]
    public class TransactionsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetTransactionsListQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await Mediator.Send(new GetTransactionByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteTransactionCommand { Id = id });
            return NoContent();
        }
    }
}