using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AO.BookKeeping.Models
{
    public class ReconciliationItem
    {
        public DateTime BankDay { get; set; }
        public string  ItemInfo { get; set; }
        public decimal Amount { get; set; }
    }
}
