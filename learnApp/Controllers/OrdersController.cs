using learnApp.Models;
using learnApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string searchbarinput = "")
        {
            
            var vlxdContext = _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.Employee)
                        .Include(o => o.Site)
                        .AsQueryable();

            if (fromDate.HasValue)
            {
                vlxdContext = vlxdContext.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                vlxdContext = vlxdContext.Where(o => o.OrderDate <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(searchbarinput))
            {
                vlxdContext = vlxdContext.Where(o =>
                    (o.Customer.CustomerName != null && o.Customer.CustomerName.Contains(searchbarinput)) ||
                    (o.Employee.EmployeeName != null && o.Employee.EmployeeName.Contains(searchbarinput)) ||
                    (o.Site != null && o.Site.Name != null && o.Site.Name.Contains(searchbarinput))
                );
            }

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
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .Include(o => o.Payments)
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
            ViewData["SiteId"] = new SelectList(_context.Sites, "SiteId", "Name");

            ViewBag.ProductOptions = "<option value='' selected disabled>-- Chọn sản phẩm --</option>" +
                string.Join("", _context.Products.Select(p =>
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

        [HttpGet]
        public IActionResult GetSitesByCustomer(int customerId)
        {
            var customer = _context.Customers.Find(customerId);

            if (customer == null)
                return NotFound();

            // chỉ load nếu là nhà thầu
            if (customer.Type != Enums.CustomerType.Contractor)
            {
                return Json(new List<object>());
            }

            var sites = _context.Sites
                .Where(s => s.CustomerId == customerId)
                .Select(s => new {
                    siteId = s.SiteId,
                    name = s.Name
                })
                .ToList();

            return Json(sites);
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

        public async Task<IActionResult> Summary(int? customerId, int? siteId)
        {
            if (customerId == null && siteId == null)
                return BadRequest();

            //var from = fromDate ?? DateTime.Today.AddDays(-30);
            //var to = toDate ?? DateTime.Today;

            var query = _context.Orders
                //.Where(o => o.CustomerId == customerId && o.SiteId == siteId)
                    //&& o.OrderDate >= from
                    //&& o.OrderDate <= to)
                .Where(o => o.PaymentStatus != Enums.PaymentStatus.Paid)
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId.Value);

            if (siteId.HasValue)
                query = query.Where(o => o.SiteId == siteId.Value);

            var orders = await query.ToListAsync();

            var customerName = orders.FirstOrDefault()?.Customer?.CustomerName ?? "";

            var total = orders
                .SelectMany(o => o.OrderDetails)
                .Sum(od => od.Quantity * od.UnitPrice);

            var vm = new OrderSummaryViewModel
            {
                CustomerId = customerId ?? 0,
                CustomerName = customerName,
                //FromDate = from,
                //ToDate = to,
                SiteId = siteId ?? 0,
                Orders = orders,
                TotalAmount = total
            };  

            return View(vm);
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
