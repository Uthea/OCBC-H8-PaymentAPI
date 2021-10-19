using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;
using PaymentAPI.Validation;

namespace PaymentAPI.Models
{
    public class PaymentDetail
    {
        public int paymentDetailId { get; set; }
        
        [Required]
        public string cardOwnerName { get; set; }
            
        [DateValidation]
        public string expirationDate { get; set; }
        
        [Required]
        [MaxLength(4)]
        [MinLength(3)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Must be numeric")]
        public string securityCode { get; set; }
    }
}