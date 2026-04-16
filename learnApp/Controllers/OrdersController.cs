using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using learnApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace learnApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly VlxdContext _context;

        public OrdersController(VlxdContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var vlxdContext = _context.Orders.
                Include(o => o.Customer).
                Include(o => o.Employee);
            return View(await vlxdContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Print(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            //return new ViewAsPdf("Print", order)
            //{
            //    FileName = $"HoaDon_{id}.pdf"
            //};

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeName");

            ViewBag.ProductOptions = string.Join("", _context.Products.Select(p =>
                $"<option value='{p.ProductId}' data-price='{p.Price}'>{p.ProductName}</option>"
            ));

            return View(new OrderCreateViewModel());
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            Console.WriteLine("Received Order:");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Field: {state.Key} - Error: {error.ErrorMessage}");
                }
            }
            if (ModelState.IsValid)
            {
                // 1. Lưu Order
                Console.WriteLine($"Order: CustomerId={model.Order.CustomerId}, EmployeeId={model.Order.EmployeeId}, OrderDate={model.Order.OrderDate}");
                _context.Orders.Add(model.Order);
                await _context.SaveChangesAsync();

                decimal total = 0;

                // 2. Lưu OrderDetail
                Console.WriteLine("Order Details:");
                foreach (var item in model.OrderDetails)
                {
                    item.OrderId = model.Order.OrderId;
                    total += item.Quantity * item.UnitPrice;
                    _context.OrderDetails.Add(item);
                }

                // 3. Update TotalAmount
                model.Order.TotalAmount = total;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", order.EmployeeId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,EmployeeId,OrderDate,TotalAmount")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "EmployeeId", order.EmployeeId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                _context.OrderDetails.RemoveRange(order.OrderDetails); // xoá con
                _context.Orders.Remove(order); // xoá cha
                await _context.SaveChangesAsync();
            }
            //var order = await _context.Orders.FindAsync(id);
            //if (order != null)
            //{
            //    _context.Orders.Remove(order);
            //}

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
