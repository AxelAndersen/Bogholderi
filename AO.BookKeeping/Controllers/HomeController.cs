using AO.BookKeeping.Data;
using AO.BookKeeping.Data.Contexts;
using AO.BookKeeping.Models;
using AO.BookKeeping.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AO.BookKeeping.Controllers
{
    public class HomeController : Controller
    {
        private InvoiceContext _invoiceContext;

        public HomeController(InvoiceContext invoiceContext)
        {
            _invoiceContext = invoiceContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Reconciliation()
        {
            
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public async Task<IActionResult> DoReconciliation(IFormFile reconciliationFile)
        {
            try
            {
                if (reconciliationFile == null)
                {
                    ViewData["Message"] = "Ingen fil uploadet";
                    return View("Error");
                }

                ReconciliationService service = new ReconciliationService();
                DateTime fromDate = DateTime.MinValue, toDate = DateTime.MaxValue;
                List<ReconciliationItem> reconciliationItems = service.GetReconciliationItems(reconciliationFile, ref fromDate, ref toDate);
                if (reconciliationItems == null || reconciliationItems.Count == 0)
                {
                    ViewData["Message"] = "Ingen rækker fundet i korrekt format";
                    return View("Error");
                }


                var invoices = _invoiceContext.Invoices
                    .Select(i => new Invoice
                    {
                        DiscountAmount = i.DiscountAmount,
                        Id = i.Id,
                        InvoiceDate = i.InvoiceDate,
                        OrderId = i.OrderId,
                        PaymentDate = i.PaymentDate,
                        ShipmentCost = i.ShipmentCost,
                        TotalPrice = i.TotalPrice,
                        UserEmailAddress = i.UserEmailAddress,
                        UserPresentationName = i.UserPresentationName
                    })
                    .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                    .OrderByDescending(i => i.Id)
                    .ToList();

                List<Invoice> notPayedInvoices = service.Reconcilidate(reconciliationItems, invoices);


            }
            catch (Exception ex)
            {
                ErrorViewModel model = new ErrorViewModel();
                model.ErrorText = ex.Message;
                return View("Error", model);
            }

            return Ok("Ok");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {  });
        }
    }
}
