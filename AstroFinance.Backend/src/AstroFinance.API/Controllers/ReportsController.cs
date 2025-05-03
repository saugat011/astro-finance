using AstroFinance.Application.Reports.Queries.GetLoanSummaryReport;
using AstroFinance.Application.Reports.Queries.GetOverdueLoansReport;
using AstroFinance.Application.Reports.Queries.GetTransactionSummaryReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AstroFinance.API.Controllers
{
    [Authorize]
    public class ReportsController : ApiControllerBase
    {
        [HttpGet("loan-summary")]
        public async Task<IActionResult> GetLoanSummary([FromQuery] GetLoanSummaryReportQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("overdue-loans")]
        public async Task<IActionResult> GetOverdueLoans([FromQuery] GetOverdueLoansReportQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("transaction-summary")]
        public async Task<IActionResult> GetTransactionSummary([FromQuery] GetTransactionSummaryReportQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }
    }
}