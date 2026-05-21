using learnApp.Enums;
using learnApp.Models;
using learnApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace learnApp.Controllers
{
    public class CustomerPaymentsController : Controller
    {
        private readonly VlxdContext _context;

        public CustomerPaymentsController(VlxdContext context)
        {
            _context = context;
        }

        // GET: CustomerPayments
        public async Task<IActionResult> Index()
        {
            var vlxdContext = _context.CustomerPayment.Include(c => c.Customer).Include(c => c.Site);
            return View(await vlxdContext.ToListAsync());
        }

        // GET: CustomerPayments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerPayment = await _context.CustomerPayment
                .Include(c => c.Customer)
                .Include(c => c.Site)
                .FirstOrDefaultAsync(m => m.CustomerPaymentId == id);
            if (customerPayment == null)
            {
                return NotFound();
            }

            return View(customerPayment);
        }

        // GET: CustomerPayments/Create
        public IActionResult Create(int? customerId, int? siteId)
        {
            if (customerId == null && siteId == null)
            {                
                return View();
            }

            Customer? customer = null;
            Site? site = null;            

            if (siteId.HasValue)
            {
                site = _context.Sites
                    .Include(s => s.Customer)
                    .FirstOrDefault(s => s.SiteId == siteId.Value);

                if (site == null)
                    return NotFound();

                customer = site.Customer;
            }

            if (customerId.HasValue)
            {
                customer = _context.Customers
                    .FirstOrDefault(c => c.CustomerId == customerId.Value);

                if (customer == null)
                    return NotFound();
            }

            var vm = new PaymentCreateViewModel
            {
                CustomerId = customer.CustomerId,
                CustomerName = customer.CustomerName,

                SiteId = site?.SiteId,
                SiteName = site?.Name,

                Amount = GetDebt(customerId, siteId)
            };

            return View(vm);
        }

        // POST: CustomerPayments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var customerPayment = new CustomerPayment
                {
                    CustomerId = vm.CustomerId,
                    SiteId = vm?.SiteId,
                    Amount = vm.Amount,
                    PaymentDate = DateTime.Now,
                    Note = vm.Note
                };

                _context.Add(customerPayment);

                var orders = await _context.Orders
                    .Where(o => o.CustomerId == vm.CustomerId && o.PaidAmount < o.TotalAmount)
                    .OrderBy(o => o.OrderDate)
                    .ToListAsync();

                decimal remain = vm.Amount;
                foreach (var order in orders)
                {
                    if (remain <= 0)
                        break;

                    var debt = order.TotalAmount - order.PaidAmount;

                    if (remain >= debt)
                    {
                        order.PaidAmount += debt;
                        remain -= debt;
                    }
                    else
                    {
                        order.PaidAmount += remain;
                        remain = 0;
                    }

                    // update status
                    order.UpdatePaymentStatus();
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: CustomerPayments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerPayment = await _context.CustomerPayment.FindAsync(id);
            if (customerPayment == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", customerPayment.CustomerId);
            ViewData["SiteId"] = new SelectList(_context.Sites, "SiteId", "SiteId", customerPayment.SiteId);
            return View(customerPayment);
        }

        // POST: CustomerPayments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerPaymentId,CustomerId,SiteId,Amount,PaymentDate,Note")] CustomerPayment customerPayment)
        {
            if (id != customerPayment.CustomerPaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customerPayment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerPaymentExists(customerPayment.CustomerPaymentId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", customerPayment.CustomerId);
            ViewData["SiteId"] = new SelectList(_context.Sites, "SiteId", "SiteId", customerPayment.SiteId);
            return View(customerPayment);
        }

        // GET: CustomerPayments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerPayment = await _context.CustomerPayment
                .Include(c => c.Customer)
                .Include(c => c.Site)
                .FirstOrDefaultAsync(m => m.CustomerPaymentId == id);
            if (customerPayment == null)
            {
                return NotFound();
            }

            return View(customerPayment);
        }

        // POST: CustomerPayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.CustomerPayment
                .FirstOrDefaultAsync(m => m.CustomerPaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            decimal remain = payment.Amount;
            var ordersQuery = _context.Orders
                .Where(o => o.CustomerId == payment.CustomerId && o.PaidAmount > 0);

            if(payment.SiteId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.SiteId == payment.SiteId.Value);
            }else
            {
                ordersQuery = ordersQuery.Where(o => o.SiteId == null);
            }
            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            foreach (var order in orders)
            {
                if (remain <= 0)
                    break;

                if (order.PaidAmount >= remain)
                {
                    order.PaidAmount -= remain;
                    remain = 0;
                }
                else
                {
                    remain -= order.PaidAmount;
                    order.PaidAmount = 0;
                }

                order.UpdatePaymentStatus();
            }

            _context.CustomerPayments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerPaymentExists(int id)
        {
            return _context.CustomerPayment.Any(e => e.CustomerPaymentId == id);
        }
        private decimal GetDebt(int? customerId, int? siteId)
        {
            decimal totalAmount = 0;
            decimal totalPayment = 0;
            if (customerId.HasValue)
            {
                totalAmount = _context.Orders
                    .Where(o => o.CustomerId == customerId.Value)
                    .Sum(o => o.TotalAmount);
                totalPayment = _context.CustomerPayment
                    .Where(p => p.CustomerId == customerId.Value)
                    .Sum(p => p.Amount);
            } else if (siteId.HasValue)
            {
                totalAmount = _context.Orders
                    .Where(o => o.SiteId == siteId.Value)
                    .Sum(o => o.TotalAmount);
                totalPayment = _context.CustomerPayment
                    .Where(p => p.SiteId == siteId.Value)
                    .Sum(p => p.Amount);
            }

            return totalAmount - totalPayment;
        }
    }
}
