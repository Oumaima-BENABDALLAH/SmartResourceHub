using Microsoft.AspNetCore.Mvc;
using Payment.Application.Services;
using Payment.Domain.Models;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IdempotentPaymentService _paymentService;

        public PaymentController(IdempotentPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> PostPayment([FromBody] PaymentRequest request)
        {
            if (string.IsNullOrEmpty(request.TransactionId))
            {
                return BadRequest("Le TransactionId est obligatoire pour garantir l'idempotence.");
            }

            try
            {
                var result = await _paymentService.ProcessPaymentAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Une erreur interne est survenue lors du traitement du paiement." });
            }
        }
    }
}