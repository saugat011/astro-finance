using AstroFinance.Application.Users.Commands.DeleteUser;
using AstroFinance.Application.Users.Commands.UpdateUser;
using AstroFinance.Application.Users.Queries.GetUserById;
using AstroFinance.Application.Users.Queries.GetUsersList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AstroFinance.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetUsersListQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await Mediator.Send(new GetUserByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserCommand command)
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
            await Mediator.Send(new DeleteUserCommand { Id = id });
            return NoContent();
        }
    }
}