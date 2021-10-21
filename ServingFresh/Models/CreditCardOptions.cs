using System;
namespace ServingFresh.Models
{
    public class CreditCardOptions
    {
        public string Number { get; set; }
        public int ExpYear { get; set; }
        public int ExpMonth { get; set; }
        public string CardCvv { get; set; }
    }
}
