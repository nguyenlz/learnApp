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
        public async Task<IActionResult> Index()
        {
            var vlxdContext = _context.StockImports.Include(s => s.Employee).Include(s => s.Supplier);
            return View(await vlxdContext.ToListAsync());
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");
            ViewBag.Products = _context.Products
                .Select(p => new { p.ProductId, p.ProductName })
                .ToList();
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
                decimal total = 0;
                foreach (var detail in stockImport.StockImportDetails)
                {
                    detail.ImportId = stockImport.ImportId;

                    var quantity = detail.Quantity;
                    var price = detail.ImportPrice;
                    total += (quantity ?? 0) * (price ?? 0);

                    //_context.StockImportDetails.Add(detail);

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

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName", stockImport.EmployeeId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", stockImport.SupplierId);
            return View(stockImport);
        }

        // GET: StockImports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImport = await _context.StockImports.FindAsync(id);
            if (stockImport == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName", stockImport.EmployeeId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName", stockImport.SupplierId);
            return View(stockImport);
        }

        // POST: StockImports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ImportId,SupplierId,ImportDate,EmployeeId")] StockImport stockImport)
        {
            if (id != stockImport.ImportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockImport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockImportExists(stockImport.ImportId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", stockImport.EmployeeId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", stockImport.SupplierId);
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
    }
}
