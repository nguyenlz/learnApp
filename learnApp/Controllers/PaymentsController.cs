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
    public class PaymentsController : Controller
    {
        private readonly VlxdContext _context;

        public PaymentsController(VlxdContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var vlxdContext = _context.Payments.Include(p => p.Order);
            return View(await vlxdContext.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            var vm = new PaymentCreateViewModel
            {
                OrderId = orderId,
                Order = order,
                Amount = order.TotalAmount - order.Payments.Sum(p => p.Amount) // gợi ý auto fill
            };

            return View(vm);
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel vm)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == vm.OrderId);

            if (order == null) return NotFound();

            var totalPaid = order.Payments.Sum(p => p.Amount);

            if (vm.Amount <= 0)
            {
                ModelState.AddModelError("", "Số tiền phải lớn hơn 0");
            }

            if (totalPaid + vm.Amount > order.TotalAmount)
            {
                ModelState.AddModelError("", "Thanh toán vượt quá số tiền");
            }

            if (ModelState.IsValid)
            {
                var payment = new Payment
                {
                    OrderId = vm.OrderId,
                    Amount = vm.Amount,
                    PaymentDate = DateTime.Now
                };

                _context.Payments.Add(payment);
                order.Payments.Add(payment);

                order.UpdatePaymentStatus();

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Orders", new { id = vm.OrderId });
            }

            // load lại data nếu lỗi
            vm.Order = order;
            return View(vm);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", payment.OrderId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentId,OrderId,Amount,PaymentDate")] Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentId))
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", payment.OrderId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order).ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
            if (payment != null)
            {                
                var order = payment.Order;
                _context.Payments.Remove(payment);

                if (order != null)
                {
                    order.Payments.Remove(payment);
                    order.UpdatePaymentStatus();
                }

            }

            
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}
