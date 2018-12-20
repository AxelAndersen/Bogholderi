using AO.BookKeeping.Data;
using System.Collections.Generic;

namespace AO.BookKeeping.Models
{
    public class ResultModel
    {
        public List<Invoice> NotPayedInvoices { get; set; }
        public int ReconsiledItemsCount { get; set; }
        public string ReconsiledItemsPresentation { get; set; }
        public string ResultHeader { get; set; }

        public string CompleteFileName { get; set; }
    }
}
