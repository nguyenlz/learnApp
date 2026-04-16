using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using learnApp.Models;

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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId");
            return View();
        }

        // POST: StockImports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImportId,SupplierId,ImportDate,EmployeeId")] StockImport stockImport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stockImport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", stockImport.EmployeeId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", stockImport.SupplierId);
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", stockImport.EmployeeId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", stockImport.SupplierId);
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
