using AO.BookKeeping.Data;
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
        private DateTime _fromDate = DateTime.MinValue, _toDate = DateTime.MaxValue;

        public List<ReconciliationItem> GetReconciliationItems(IFormFile reconciliationFile, ref DateTime fromDate, ref DateTime toDate, ref string completeFileName)
        {
            List<ReconciliationItem> reconciliationItems = new List<ReconciliationItem>();

            var filePath = Path.GetTempPath();
            completeFileName = filePath + "_" + DateTime.Now.Ticks + "-" + reconciliationFile.FileName;
            SaveFile(reconciliationFile, completeFileName);
            AddToList(completeFileName, reconciliationItems);            

            fromDate = _fromDate;
            toDate = _toDate;

            return reconciliationItems;
        }

        public List<ReconciliationItem> GetReconciliationItemsForPrint(ref DateTime fromDate, ref DateTime toDate, string completeFileName)
        {
            List<ReconciliationItem> reconciliationItems = new List<ReconciliationItem>();

            AddToList(completeFileName, reconciliationItems);

            fromDate = _fromDate;
            toDate = _toDate;

            return reconciliationItems;
        }

        private void AddToList(string completeFileName, List<ReconciliationItem> reconciliationItems)
        {
            string errorMessage = string.Empty;

            FileInfo file = new FileInfo(completeFileName);
            using (ExcelPackage package = new ExcelPackage(file))
            {
                if(package.Workbook.Worksheets.Count == 0)
                {
                    throw new Exception("Fejl ved indlæsning af excel ark. Prøv igen forfra");
                }

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

                    if (row == 1)
                    {
                        _fromDate = item.BankDay;
                    }
                    if(row == rowCount)
                    {
                        _toDate = item.BankDay;
                    }
                    reconciliationItems.Add(item);
                }
            }            
        }

        private string ValidateCells(ExcelRange cells, int row)
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

        private void SaveFile(IFormFile reconciliationFile, string fullfileNamePath)
        {
            if (reconciliationFile.Length > 0)
            {
                using (var stream = new FileStream(fullfileNamePath, FileMode.Create))
                {
                    reconciliationFile.CopyTo(stream);
                }
            }
        }

        internal ResultModel Reconcilidate(List<ReconciliationItem> reconciliationItems, List<Invoice> invoices)
        {
            ResultModel model = new ResultModel();            
            model.NotPayedInvoices = new List<Invoice>();

            foreach (Invoice invoice in invoices)
            {
                if(IsPayed(invoice, reconciliationItems) == false)
                {
                    model.NotPayedInvoices.Add(invoice);
                }
                else
                {
                    model.ReconsiledItemsCount++;
                }
            }

            return model;
        }

        private bool IsPayed(Invoice invoice, List<ReconciliationItem> reconciliationItems)
        {
            foreach(ReconciliationItem item in reconciliationItems)
            {
                int orderId = GetOrderId(item.ItemInfo);
                if(orderId <= 0)
                {                    
                    continue;
                }

                if(orderId == invoice.OrderId)
                {
                    // Found a match, lets see if the amounts are the same
                    if(invoice.TotalPrice == item.Amount)
                    {
                        return true;
                    }
                    else
                    {
                        invoice.ErrorText = "Beløbene er ikke ens!<br />Fakturabeløb: " + invoice.TotalPrice + "<br />Betalt beløb: " + item.Amount + "<br />" + item.ItemInfo;
                    }
                }                
            }

            return false;
        }

        private int GetOrderId(string itemInfo)
        {            
            if(itemInfo.StartsWith("DK-IND") == false)
            {
                // we only do this for these specifics
                return 0;
            }

            if (itemInfo.Contains("Reference") == false)
            {
                // we only do this for these specifics
                return 0;
            }

            string orderId = itemInfo.Substring(itemInfo.IndexOf("Reference") + 9).Trim();
            orderId = orderId.Substring(7);
            return Convert.ToInt32(orderId);        
        }
    }
}