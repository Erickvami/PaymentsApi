using Microsoft.AspNetCore.Mvc;
using SparkTech.Data.Models;
using SparkTech.Services;
using Microsoft.EntityFrameworkCore;

namespace ClearMechanic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class paymentsController(
        IPaymentService service
    ) : ControllerBase
    {
        // GET: api/payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> Get()
        {
            return Ok(await service.GetAll());
        }

        // GET: api/payments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetById(int id)
        {
            var payment = await service.GetById(id);
            if (payment == null) return NotFound();

            return Ok(payment);
        }

        // GET: api/payments/ref/{reference}
        [HttpGet("ref/{reference}")]
        public async Task<ActionResult<Payment>> GetByReference(string reference)
        {
            var payment = await service.GetByReference(reference);
            if (payment == null) return NotFound();

            return Ok(payment);
        }

        // POST: api/payments
        [HttpPost]
        public async Task<ActionResult<Payment>> Post(Payment payment)
        {
            if (payment == null) return BadRequest("Payment cannot be null");

            payment = await service.CreateAsync(payment);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        // PUT: api/payments
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] Payment updatedPayment)
        {
            if (id != updatedPayment.Id)
            {
                return BadRequest("The ID in the URL does not match the ID in the payment object.");
            }

            // Check if the payment exists in the database
            var existingPayment = await service.GetById(id);
            if (existingPayment == null)
            {
                return NotFound();
            }

            // Update the payment details
            try
            {
                await service.UpdateAsync(updatedPayment);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(updatedPayment);
        }

        // DELETE: api/payments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id, isSoft: true);
            return Ok();
        }
    }
}