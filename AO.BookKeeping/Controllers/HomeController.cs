using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AO.BookKeeping.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using AO.BookKeeping.Services;

namespace AO.BookKeeping.Controllers
{
    public class HomeController : Controller
    {
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

                List<ReconciliationItem> reconciliationItems = await service.GetReconciliationItems(reconciliationFile);
                if (reconciliationItems == null || reconciliationItems.Count == 0)
                {
                    ViewData["Message"] = "Ingen rækker fundet i korrekt format";
                    return View("Error");
                }

            }
            catch(Exception ex)
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
