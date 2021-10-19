using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using PaymentAPI.Data;
using PaymentAPI.Models;
using PaymentAPI.Services;

namespace PaymentAPI.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class PaymentDetailController : ControllerBase
    {
        private readonly IPaymentDetailService _paymentDetailService;

        public PaymentDetailController(IPaymentDetailService paymentDetailService)
        {
            _paymentDetailService = paymentDetailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaymentDetails()
        {
            var itemPaymentDetails = await _paymentDetailService.GetPaymentDetails();
            return Ok(itemPaymentDetails);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentDetail(PaymentDetail data)
        {
            if (ModelState.IsValid)
            {
                data = await _paymentDetailService.CreatePaymentDetail(data);
                return CreatedAtAction("GetPaymentDetail", new {id = data.paymentDetailId}, data);
            }

            return new JsonResult("Something went wrong") {StatusCode = 500};
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetail(int id)
        {
            var itemPaymentDetail = await _paymentDetailService.GetPaymentDetailById(id);

            if (itemPaymentDetail == null) return NotFound();

            return Ok(itemPaymentDetail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentDetail(int id, PaymentDetail data)
        {
            if (!ModelState.IsValid) return new JsonResult("Something went wrong") {StatusCode = 500};

            var itemPaymentDetail = await _paymentDetailService.UpdatePaymentDetail(id, data);

            if (itemPaymentDetail == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentDetail(int id)
        {
            var itemPaymentDetail = await _paymentDetailService.DeletePaymentDetail(id);

            if (itemPaymentDetail == null) return NotFound();
            
            return Ok(itemPaymentDetail);
        }
        


        
    }
}