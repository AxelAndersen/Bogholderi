using AO.BookKeeping.Models;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AO.BookKeeping.Services
{
    public class ReconciliationService
    {
        public async Task<List<ReconciliationItem>> GetReconciliationItems(IFormFile reconciliationFile)
        {
            List<ReconciliationItem> reconciliationItems = new List<ReconciliationItem>();

            var filePath = Path.GetTempPath();
            await SaveFile(reconciliationFile, filePath);            
            AddToList(reconciliationFile, reconciliationItems, filePath);

            return reconciliationItems;
        }

        private static void AddToList(IFormFile reconciliationFile, List<ReconciliationItem> reconciliationItems, string filePath)
        {
            string errorMessage = string.Empty;

            FileInfo file = new FileInfo(Path.Combine(filePath, reconciliationFile.FileName));
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                for (int row = 1; row <= rowCount; row++)
                {
                    errorMessage = ValidateCells(worksheet.Cells, row);

                    if (string.IsNullOrEmpty(errorMessage) == false)
                    {
                        throw new Exception("Fejl i excel arket: " + errorMessage);
                    }

                    ReconciliationItem item = new ReconciliationItem()
                    {
                        BankDay = Convert.ToDateTime(worksheet.Cells[row, 1].Value.ToString()),
                        ItemInfo = worksheet.Cells[row, 2].Value.ToString(),
                        Amount = Convert.ToDecimal(worksheet.Cells[row, 3].Value.ToString())
                    };


                    reconciliationItems.Add(item);
                }
            }
        }

        private static string ValidateCells(ExcelRange cells, int row)
        {
            DateTime dt;
            bool allWell = DateTime.TryParse(cells[row, 1].Value.ToString(), out dt);
            if (allWell == false)
            {
                return "Første celle i række " + row + " indeholder ikke en rigtig dato";
            }

            if (cells[row, 2] == null || cells[row, 2].Value == null || string.IsNullOrWhiteSpace(cells[row, 2].Value.ToString()))
            {
                return "2. celle i række " + row + " er tom";
            }

            decimal d;
            allWell = Decimal.TryParse(cells[row, 3].Value.ToString(), out d);
            if (allWell == false)
            {
                return "3. celle i række " + row + " indeholder ikke et rigtigt beløb";
            }

            return string.Empty;
        }

        private static async Task SaveFile(IFormFile reconciliationFile, string filePath)
        {
            if (reconciliationFile.Length > 0)
            {
                using (var stream = new FileStream(filePath + reconciliationFile.FileName, FileMode.Create))
                {
                    await reconciliationFile.CopyToAsync(stream);
                }
            }
        }
    }
}