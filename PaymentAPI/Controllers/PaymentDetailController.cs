using System;
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
                await _paymentDetailService.CreatePaymentDetail(data);
                //data = await _paymentDetailService.CreatePaymentDetail(data);
                //return CreatedAtAction("GetPaymentDetail", new {id = data.paymentDetailId}, data);
                return new JsonResult("Successfully inserted new data") {StatusCode = 200};
            }

            return new JsonResult("Something went wrong") {StatusCode = 500};
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentDetail(int id)
        {
            var itemPaymentDetail = await _paymentDetailService.GetPaymentDetailById(id);
            if (itemPaymentDetail == null)
                return new JsonResult(
                    String.Format("Payment Detail with id : {0} doesn't exist in database", id)
                ) {StatusCode = 404};

            return Ok(itemPaymentDetail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentDetail(int id, PaymentDetail data)
        {
            if (!ModelState.IsValid) return new JsonResult("Something went wrong") {StatusCode = 500};

            var itemPaymentDetail = await _paymentDetailService.UpdatePaymentDetail(id, data);

            if (itemPaymentDetail == null)
            {
                return new JsonResult(
                    String.Format("Payment Detail with id : {0} doesn't exist in database", id)
                ) {StatusCode = 404};
            }

            return new JsonResult("Data is successfully updated") {StatusCode = 200};
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentDetail(int id)
        {
            var itemPaymentDetail = await _paymentDetailService.DeletePaymentDetail(id);

            if (itemPaymentDetail == null)
                return new JsonResult(
                    String.Format("Payment Detail with id : {0} doesn't exist in database", id)
                ) {StatusCode = 404};

            return new JsonResult("Data is successfully deleted") {StatusCode = 200};
        }
    }
}