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
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> Print(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            //return new ViewAsPdf("Print", order)
            //{
            //    FileName = $"HoaDon_{orderId}.pdf"
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
                    $"<option value='{p.ProductId}' data-price='{p.Price}'>{p.ProductName} (Còn: {p.StockQuantity})</option>"
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
            if (!ModelState.IsValid)
            {
                LoadViewData();
                return View(model);
            }

            // Gom sản phẩm trùng nhau trong cùng đơn
            var groupedProducts = model.OrderDetails
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            // Kiểm tra tồn kho
            foreach (var item in groupedProducts)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product == null)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                    LoadViewData();
                    return View(model);
                }

                decimal stock = product.StockQuantity ?? 0;

                if (stock < item.TotalQuantity)
                {
                    ModelState.AddModelError("",
                        $"Sản phẩm '{product.ProductName}' chỉ còn {stock} {product.Unit}.");

                    TempData["Error"] =
                        $"Sản phẩm '{product.ProductName}' chỉ còn {stock} {product.Unit}.";

                    LoadViewData();
                    return View(model);
                }
            }

            // Tạo order
            _context.Orders.Add(model.Order);
            await _context.SaveChangesAsync();

            decimal total = 0;

            // Lưu detail + trừ kho
            foreach (var item in model.OrderDetails)
            {
                item.OrderId = model.Order.OrderId;

                total += item.Quantity * item.UnitPrice;

                _context.OrderDetails.Add(item);

                // trừ tồn kho
                var product = await _context.Products
                    .FirstAsync(p => p.ProductId == item.ProductId);

                product.StockQuantity =
                    (product.StockQuantity ?? 0) - item.Quantity;
            }

            model.Order.TotalAmount = total;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.Order.OrderId });
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
                .Select(s => new
                {
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
                foreach (var item in order.OrderDetails)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product != null)
                    {
                        product.StockQuantity =
                            (product.StockQuantity ?? 0) + item.Quantity;
                    }
                }

                _context.OrderDetails.RemoveRange(order.OrderDetails); // xoá con
                _context.Orders.Remove(order); // xoá cha
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Summary(int? customerId, int? siteId, string? type)
        {
            if (customerId == null && siteId == null)
                return BadRequest();

            var vm = await GetSummaryData(customerId, siteId, type);

            return View(vm);
        }
        public async Task<IActionResult> PrintSummary(int? customerId, int? siteId, string? type)
        {
            if (customerId == null && siteId == null)
                return BadRequest();

            var vm = await GetSummaryData(customerId, siteId, type);

            //return new ViewAsPdf("PrintSummary", vm)
            //{
            //    FileName = $"HoaDonTong_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            //};

            return View(vm);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
        private async Task<OrderSummaryViewModel> GetSummaryData(int? customerId, int? siteId, string? type)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Site)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            var paid = _context.CustomerPayments.AsQueryable();

            if (!string.IsNullOrEmpty(type))
            {
                if (type == "unpaid")
                    query = query.Where(o => o.PaymentStatus != Enums.PaymentStatus.Paid);
            }

            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId);
                paid = paid.Where(cp => cp.CustomerId == customerId);
            }


            if (siteId.HasValue)
            {
                query = query.Where(o => o.SiteId == siteId);
                paid = paid.Where(cp => cp.SiteId == siteId);
            }

            var orders = await query.ToListAsync();

            var customerName = orders.FirstOrDefault()?.Customer?.CustomerName ?? "";
            var siteName = orders.FirstOrDefault()?.Site?.Name ?? "";

            var total = orders.Sum(o => o.TotalAmount);

            var paidAmount = orders.Sum(o => o.PaidAmount);

            return new OrderSummaryViewModel
            {
                CustomerId = customerId ?? 0,
                SiteId = siteId ?? 0,
                CustomerName = customerName,
                SiteName = siteName,
                Orders = orders,
                TotalAmount = total,
                PaidAmount = paidAmount
            };
        }
        private void LoadViewData()
        {
            ViewData["CustomerId"] =
                new SelectList(_context.Customers, "CustomerId", "CustomerName");

            ViewData["EmployeeId"] =
                new SelectList(_context.Employees, "EmployeeId", "EmployeeName");

            ViewData["SiteId"] =
                new SelectList(_context.Sites, "SiteId", "Name");

            ViewBag.ProductOptions =
                "<option value='' selected disabled>-- Chọn sản phẩm --</option>" +
                string.Join("", _context.Products.Select(p =>
                    $"<option value='{p.ProductId}' data-price='{p.Price}'>" +
                    $"{p.ProductName} (Còn: {p.StockQuantity})</option>"
                ));
        }
    }
}