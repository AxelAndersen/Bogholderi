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

        public async Task<IActionResult> UploadFile(IFormFile reconciliationFile)
        {
            long size = reconciliationFile.Length;

            // full path to file in temp location
            var filePath = Path.GetTempPath();

            if (reconciliationFile.Length > 0)
            {
                using (var stream = new FileStream(filePath + reconciliationFile.FileName, FileMode.Create))
                {
                    await reconciliationFile.CopyToAsync(stream);
                }
            }

            StringBuilder sb = new StringBuilder();

            string sFileExtension = Path.GetExtension(reconciliationFile.FileName).ToLower();
            ISheet sheet;
            string fullPath = Path.Combine(filePath, reconciliationFile.FileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                reconciliationFile.CopyTo(stream);
                stream.Position = 0;
                if (sFileExtension == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                }
                else
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                    sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                }
                IRow headerRow = sheet.GetRow(0); //Get Header Row
                int cellCount = headerRow.LastCellNum;
                sb.Append("<table class='table'><tr>");
                for (int j = 0; j < cellCount; j++)
                {
                    NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);
                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                    sb.Append("<th>" + cell.ToString() + "</th>");
                }
                sb.Append("</tr>");
                sb.AppendLine("<tr>");
                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                    for (int j = row.FirstCellNum; j < cellCount; j++)
                    {
                        if (row.GetCell(j) != null)
                            sb.Append("<td>" + row.GetCell(j).ToString() + "</td>");
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(sb.ToString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
