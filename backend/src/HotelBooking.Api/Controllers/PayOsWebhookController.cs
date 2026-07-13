using HotelBooking.Api.Contracts;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HotelBooking.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/payments/payos")]
public sealed class PayOsWebhookController(IHotelBookingService hotelBookingService) : ControllerBase
{
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook(
        PayOsWebhookRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Data.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return BadRequest(new { error = "Missing payOS webhook data." });
        }

        var result = await hotelBookingService.ProcessPayOsWebhookAsync(
            request.Data,
            request.Signature,
            cancellationToken);

        if (!result.Accepted)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { code = "00", desc = "success" });
    }
}
