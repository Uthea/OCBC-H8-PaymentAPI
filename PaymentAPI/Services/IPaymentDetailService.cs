using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentAPI.Models;

namespace PaymentAPI.Services
{
    public interface IPaymentDetailService
    {
        Task<List<PaymentDetail>> GetPaymentDetails();
        Task<PaymentDetail> CreatePaymentDetail(PaymentDetail data);
        Task<PaymentDetail> GetPaymentDetailById(int id);
        Task<PaymentDetail> UpdatePaymentDetail(int id, PaymentDetail data);
        Task<PaymentDetail> DeletePaymentDetail(int id);
    }
}