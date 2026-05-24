using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using learnApp.Models;
using learnApp.ViewModels;

namespace learnApp.Controllers
{
    public class StockImportsController : Controller
    {
        private readonly VlxdContext _context;

        public StockImportsController(VlxdContext context)
        {
            _context = context;
        }

        // GET: StockImports
        public async Task<IActionResult> Index(string? searchbarinput, string? sortOrder)
        {
            IQueryable<StockImport> query = _context.StockImports
                .Include(x => x.Supplier)
                .Include(x => x.Employee);

            // SEARCH
            if (!string.IsNullOrWhiteSpace(searchbarinput))
            {
                query = query.Where(x =>
                    x.Supplier.SupplierName.Contains(searchbarinput));
            }

            // SORT
            query = sortOrder switch
            {
                "date_asc" =>
                    query.OrderBy(x => x.ImportDate),

                "amount_desc" =>
                    query.OrderByDescending(x => x.TotalAmount),

                "amount_asc" =>
                    query.OrderBy(x => x.TotalAmount),

                _ =>
                    query.OrderByDescending(x => x.ImportDate)
            };

            return View(await query.ToListAsync());
        }

        // GET: StockImports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImport = await _context.StockImports
                .Include(s => s.Employee)
                .Include(s => s.Supplier)
                .Include(s => s.StockImportDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.ImportId == id);
            if (stockImport == null)
            {
                return NotFound();
            }

            return View(stockImport);
        }

        // GET: StockImports/Create
        public IActionResult Create()
        {
            LoadViewData();
            return View();
        }

        // POST: StockImports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockImport stockImport)
        {
            foreach(var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"ModelState error in {state.Key}: {error.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                stockImport.StockImportDetails = stockImport.StockImportDetails
                    .GroupBy(d => d.ProductId)
                    .Select(g => new StockImportDetail
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(x => x.Quantity ?? 0),
                        ImportPrice = g.Last().ImportPrice
                    })
                    .ToList();

                decimal total = 0;
                foreach (var detail in stockImport.StockImportDetails)
                {
                    detail.ImportId = stockImport.ImportId;

                    var quantity = detail.Quantity;
                    var price = detail.ImportPrice;
                    total += (quantity ?? 0) * (price ?? 0);

                    var product = _context.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                        _context.Products.Update(product);
                    }
                }

                stockImport.TotalAmount = total;
                stockImport.DebtAmount = total;
                stockImport.PaidAmount = 0;
                _context.Add(stockImport);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadViewData();
            return View(stockImport);
        }

        // GET: StockImports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImport = await _context.StockImports
                .Include(x => x.StockImportDetails)
                .FirstOrDefaultAsync(x => x.ImportId == id);

            if (stockImport == null)
            {
                return NotFound();
            }

            LoadViewData();
            return View(stockImport);
        }

        // POST: StockImports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StockImport stockImport)
        {
            if (id != stockImport.ImportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var oldImport = await _context.StockImports
                    .Include(x => x.StockImportDetails)
                    .FirstOrDefaultAsync(x => x.ImportId == id);

                if (oldImport == null)
                {
                    return NotFound();
                }

                // rollback tồn kho cũ
                foreach (var oldDetail in oldImport.StockImportDetails)
                {
                    var product = await _context.Products
                        .FindAsync(oldDetail.ProductId);

                    if (product != null)
                    {
                        product.StockQuantity -= oldDetail.Quantity ?? 0;
                    }
                }

                // xóa detail cũ
                _context.StockImportDetails
                    .RemoveRange(oldImport.StockImportDetails);

                // gộp sản phẩm trùng
                stockImport.StockImportDetails = stockImport.StockImportDetails
                    .GroupBy(x => x.ProductId)
                    .Select(g => new StockImportDetail
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(x => x.Quantity ?? 0),
                        ImportPrice = g.Last().ImportPrice
                    })
                    .ToList();

                decimal total = 0;

                foreach (var detail in stockImport.StockImportDetails)
                {
                    detail.ImportId = stockImport.ImportId;

                    total +=
                        (detail.Quantity ?? 0)
                        * (detail.ImportPrice ?? 0);

                    var product = await _context.Products
                        .FindAsync(detail.ProductId);

                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity ?? 0;
                    }
                }

                oldImport.SupplierId = stockImport.SupplierId;
                oldImport.EmployeeId = stockImport.EmployeeId;
                oldImport.ImportDate = stockImport.ImportDate;

                oldImport.TotalAmount = total;
                oldImport.PaidAmount = stockImport?.PaidAmount ?? 0;
                oldImport.DebtAmount =
                    total - (stockImport?.PaidAmount ?? 0);

                oldImport.StockImportDetails =
                    stockImport.StockImportDetails;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadViewData();
            return View(stockImport);
        }

        // GET: StockImports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImport = await _context.StockImports
                .Include(s => s.Employee)
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(m => m.ImportId == id);
            if (stockImport == null)
            {
                return NotFound();
            }

            return View(stockImport);
        }

        // POST: StockImports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockImport = await _context.StockImports.FindAsync(id);
            if (stockImport != null)
            {
                foreach (var detail in _context.StockImportDetails.Where(d => d.ImportId == id))
                {
                    var product = _context.Products.Find(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= detail.Quantity;
                        _context.Products.Update(product);
                    }

                    _context.StockImportDetails.Remove(detail);
                }   

                _context.StockImports.Remove(stockImport);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }       

        private bool StockImportExists(int id)
        {
            return _context.StockImports.Any(e => e.ImportId == id);
        }

        private void LoadViewData()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");
            ViewBag.Products = _context.Products
                .Select(p => new { p.ProductId, p.ProductName })
                .ToList();
        }
    }
}
