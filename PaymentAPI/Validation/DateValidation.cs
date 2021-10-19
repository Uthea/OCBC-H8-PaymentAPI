using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PaymentAPI.Validation
{
    public class DateValidation : ValidationAttribute
    {
        public override bool IsValid( object value)
        {
            DateTime dt;
            var isValid = DateTime.TryParseExact(
                Convert.ToString(value),
                "MM/dd/yyyy",
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out dt);

            return isValid;
        }
    }
}