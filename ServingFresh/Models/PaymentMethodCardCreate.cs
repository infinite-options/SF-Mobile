using System;
namespace ServingFresh.Models
{
    public class PaymentMethodCardCreate
    {
        public string Card { get; set; }
        public string Number { get; set; }
        public string Cvc { get; set; }
        public long ExpMonth { get; set; }
        public long ExpYear { get; set; }
    }
}
