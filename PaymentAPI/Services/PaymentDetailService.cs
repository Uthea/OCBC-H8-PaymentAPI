using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Data;
using PaymentAPI.Models;

namespace PaymentAPI.Services
{
    public class PaymentDetailService : IPaymentDetailService
    {
        private readonly AppDbContext _context;

        public PaymentDetailService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentDetail>> GetPaymentDetails()
        {
            return await _context.PaymentDetails.ToListAsync();
        }

        public async Task<PaymentDetail> CreatePaymentDetail(PaymentDetail data)
        {
                await _context.PaymentDetails.AddAsync(data);
                await _context.SaveChangesAsync();

                return data;
                
        }

        public async Task<PaymentDetail> GetPaymentDetailById(int id)
        {
            var paymentDetail = await _context.PaymentDetails.FirstOrDefaultAsync(
                            x => x.paymentDetailId == id);

            return paymentDetail;
        }

        public async Task<PaymentDetail> UpdatePaymentDetail(int id, PaymentDetail data)
        {
             var itemPaymentDetail = await _context.PaymentDetails.FirstOrDefaultAsync(
             x => x.paymentDetailId == id);
 
             if (itemPaymentDetail == null) return null;
 
             itemPaymentDetail.cardOwnerName = data.cardOwnerName;
             itemPaymentDetail.expirationDate = data.expirationDate;
             itemPaymentDetail.securityCode = data.securityCode; 
             
             await _context.SaveChangesAsync();

             return itemPaymentDetail;
        }

        public async Task<PaymentDetail> DeletePaymentDetail(int id)
        {
            var itemPaymentDetail = await _context.PaymentDetails.FirstOrDefaultAsync(
                x => x.paymentDetailId == id);
            
            if (itemPaymentDetail == null) return null;
            
            _context.Remove(itemPaymentDetail);
            await _context.SaveChangesAsync();

            return itemPaymentDetail;
        }
    }
}