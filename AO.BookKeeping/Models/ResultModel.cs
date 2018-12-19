using AO.BookKeeping.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AO.BookKeeping.Models
{
    public class ResultModel
    {
        public List<Invoice> NotPayedInvoices { get; set; }
        public int ReconsiledItemsCount { get; set; }
        public string ReconsiledItemsPresentation { get; set; }
        public string ResultHeader { get; set; }
    }
}
