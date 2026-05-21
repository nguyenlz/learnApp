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
    public class SupplierPaymentsController : Controller
    {
        private readonly VlxdContext _context;

        public SupplierPaymentsController(VlxdContext context)
        {
            _context = context;
        }

        // GET: SupplierPayments
        public async Task<IActionResult> Index()
        {
            var vlxdContext = _context.SupplierPayments.Include(s => s.Supplier);
            return View(await vlxdContext.ToListAsync());
        }

        // GET: SupplierPayments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplierPayment = await _context.SupplierPayments
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(m => m.SupplierPaymentId == id);
            if (supplierPayment == null)
            {
                return NotFound();
            }

            return View(supplierPayment);
        }

        // GET: SupplierPayments/Create
        public IActionResult Create(int supplierId, decimal debtAmount)
        {
            var supplier = _context.Suppliers.Find(supplierId);

            var imports = _context.StockImports.Where(i => i.SupplierId == supplierId).ToList();
            if (supplier == null) return NotFound();

            var vm = new SupplierPaymentCreateViewModel
            {
                SupplierId = supplierId,
                Amount = debtAmount,
                Supplier = supplier,
            };

            return View(vm);
        }

        // POST: SupplierPayments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierPaymentCreateViewModel vm)
        {



            if (ModelState.IsValid)
            {
                var supplierPayment = new SupplierPayment
                {
                    SupplierId = vm.SupplierId,
                    Amount = vm.Amount,
                    PaymentDate = DateTime.Now
                };
                await _context.SupplierPayments.AddAsync(supplierPayment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: SupplierPayments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplierPayment = await _context.SupplierPayments.FindAsync(id);
            if (supplierPayment == null)
            {
                return NotFound();
            }
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", supplierPayment.SupplierId);
            return View(supplierPayment);
        }

        // POST: SupplierPayments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SupplierPaymentId,SupplierId,Amount,PaymentDate")] SupplierPayment supplierPayment)
        {
            if (id != supplierPayment.SupplierPaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplierPayment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierPaymentExists(supplierPayment.SupplierPaymentId))
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
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", supplierPayment.SupplierId);
            return View(supplierPayment);
        }

        // GET: SupplierPayments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplierPayment = await _context.SupplierPayments
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(m => m.SupplierPaymentId == id);
            if (supplierPayment == null)
            {
                return NotFound();
            }

            return View(supplierPayment);
        }

        // POST: SupplierPayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplierPayment = await _context.SupplierPayments.FindAsync(id);
            if (supplierPayment != null)
            {
                _context.SupplierPayments.Remove(supplierPayment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupplierPaymentExists(int id)
        {
            return _context.SupplierPayments.Any(e => e.SupplierPaymentId == id);
        }
    }
}
