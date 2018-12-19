using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AO.BookKeeping.Data
{
    public class Invoice
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string UserPresentationName { get; set; }
        public string UserEmailAddress { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShipmentCost { get; set; }
        public decimal Vat { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
