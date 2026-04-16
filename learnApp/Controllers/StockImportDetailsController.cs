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
    public class StockImportDetailsController : Controller
    {
        private readonly VlxdContext _context;

        public StockImportDetailsController(VlxdContext context)
        {
            _context = context;
        }

        // GET: StockImportDetails
        public async Task<IActionResult> Index()
        {
            var vlxdContext = _context.StockImportDetails.Include(s => s.Import).Include(s => s.Product);
            return View(await vlxdContext.ToListAsync());
        }

        // GET: StockImportDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImportDetail = await _context.StockImportDetails
                .Include(s => s.Import)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.ImportId == id);
            if (stockImportDetail == null)
            {
                return NotFound();
            }

            return View(stockImportDetail);
        }

        // GET: StockImportDetails/Create
        public IActionResult Create()
        {
            ViewData["ImportId"] = new SelectList(_context.StockImports, "ImportId", "ImportId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            return View();
        }

        // POST: StockImportDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ImportId,ProductId,Quantity,ImportPrice")] StockImportDetail stockImportDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stockImportDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ImportId"] = new SelectList(_context.StockImports, "ImportId", "ImportId", stockImportDetail.ImportId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", stockImportDetail.ProductId);
            return View(stockImportDetail);
        }

        // GET: StockImportDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImportDetail = await _context.StockImportDetails.FindAsync(id);
            if (stockImportDetail == null)
            {
                return NotFound();
            }
            ViewData["ImportId"] = new SelectList(_context.StockImports, "ImportId", "ImportId", stockImportDetail.ImportId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", stockImportDetail.ProductId);
            return View(stockImportDetail);
        }

        // POST: StockImportDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ImportId,ProductId,Quantity,ImportPrice")] StockImportDetail stockImportDetail)
        {
            if (id != stockImportDetail.ImportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockImportDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockImportDetailExists(stockImportDetail.ImportId))
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
            ViewData["ImportId"] = new SelectList(_context.StockImports, "ImportId", "ImportId", stockImportDetail.ImportId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", stockImportDetail.ProductId);
            return View(stockImportDetail);
        }

        // GET: StockImportDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockImportDetail = await _context.StockImportDetails
                .Include(s => s.Import)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.ImportId == id);
            if (stockImportDetail == null)
            {
                return NotFound();
            }

            return View(stockImportDetail);
        }

        // POST: StockImportDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockImportDetail = await _context.StockImportDetails.FindAsync(id);
            if (stockImportDetail != null)
            {
                _context.StockImportDetails.Remove(stockImportDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockImportDetailExists(int id)
        {
            return _context.StockImportDetails.Any(e => e.ImportId == id);
        }
    }
}
