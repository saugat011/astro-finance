using AstroFinance.Application.Sms.Commands.CreateSmsTemplate;
using AstroFinance.Application.Sms.Commands.DeleteSmsTemplate;
using AstroFinance.Application.Sms.Commands.SendSms;
using AstroFinance.Application.Sms.Commands.UpdateSmsTemplate;
using AstroFinance.Application.Sms.Queries.GetSmsHistoryList;
using AstroFinance.Application.Sms.Queries.GetSmsTemplateById;
using AstroFinance.Application.Sms.Queries.GetSmsTemplatesList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AstroFinance.API.Controllers
{
    [Authorize]
    public class SmsController : ApiControllerBase
    {
        [HttpGet("templates")]
        public async Task<IActionResult> GetAllTemplates([FromQuery] GetSmsTemplatesListQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplateById(Guid id)
        {
            var result = await Mediator.Send(new GetSmsTemplateByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate(CreateSmsTemplateCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, result);
        }

        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, UpdateSmsTemplateCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(Guid id)
        {
            await Mediator.Send(new DeleteSmsTemplateCommand { Id = id });
            return NoContent();
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetSmsHistory([FromQuery] GetSmsHistoryListQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSms(SendSmsCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }
    }
}