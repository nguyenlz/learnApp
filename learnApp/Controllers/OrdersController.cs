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
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? searchbarinput, string? sortOrder)
        {

            IQueryable<Order> query = _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.Employee)
                        .Include(o => o.Site)
                        .AsQueryable();

            // SEARCH
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(searchbarinput))
            {
                query = query.Where(o =>
                    (o.Customer.CustomerName != null && o.Customer.CustomerName.Contains(searchbarinput)) ||
                    (o.Employee.EmployeeName != null && o.Employee.EmployeeName.Contains(searchbarinput)) ||
                    (o.Site != null && o.Site.Name != null && o.Site.Name.Contains(searchbarinput))
                );
            }

            // SORT
            query = sortOrder switch
            {
                "date_asc" =>
                    query.OrderBy(o => o.OrderDate),
                "amount_desc" =>
                    query.OrderByDescending(o => o.TotalAmount),
                "amount_asc" =>
                    query.OrderBy(o => o.TotalAmount),
                _ =>
                    query.OrderByDescending(o => o.OrderDate)
            };

            return View(await query.ToListAsync());
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
                .Include(o => o.Site)
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
            LoadViewData();
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
                if(model.Order.CustomerId <= 0)
                {
                    ModelState.AddModelError("Order.CustomerId", "Vui lòng chọn khách hàng.");
                }

                LoadViewData();
                return View(model);
            }

            var error = await ValidateOrderDetails(model.OrderDetails);

            if (error != null)
            {
                ModelState.AddModelError("", error);

                LoadViewData();

                return View(model);
            }

            _context.Orders.Add(model.Order);

            await _context.SaveChangesAsync();

            foreach (var item in model.OrderDetails)
            {
                item.OrderId = model.Order.OrderId;

                _context.OrderDetails.Add(item);
            }

            await DeductStock(model.OrderDetails);

            model.Order.TotalAmount =
                CalculateTotal(model.OrderDetails);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details),
                new { id = model.Order.OrderId });
        }

        public async Task<IActionResult> CreateDraft(OrderCreateViewModel model)
        {
            // =========================
            // VALIDATE HEADER
            // =========================

            if (model.Order.CustomerId <= 0)
            {
                ModelState.AddModelError("Order.CustomerId",
                    "Vui lòng chọn khách hàng.");
            }

            if (model.Order.EmployeeId <= 0)
            {
                ModelState.AddModelError("Order.EmployeeId",
                    "Vui lòng chọn nhân viên.");
            }

            if (model.Order.OrderDate == default)
            {
                ModelState.AddModelError("Order.OrderDate",
                    "Vui lòng chọn ngày tạo đơn.");
            }

            // =========================
            // VALIDATE DETAILS
            // =========================

            if (model.OrderDetails == null || !model.OrderDetails.Any())
            {
                ModelState.AddModelError("",
                    "Vui lòng thêm ít nhất 1 sản phẩm.");
            }
            else
            {
                foreach (var item in model.OrderDetails)
                {
                    if (item.ProductId <= 0)
                    {
                        ModelState.AddModelError("",
                            "Vui lòng chọn sản phẩm.");
                    }

                    if (item.Quantity <= 0)
                    {
                        ModelState.AddModelError("",
                            "Số lượng phải lớn hơn 0.");
                    }

                    if (item.UnitPrice <= 0)
                    {
                        ModelState.AddModelError("",
                            "Đơn giá phải lớn hơn 0.");
                    }
                }
            }

            // Nếu validate fail
            if (!ModelState.IsValid)
            {
                LoadViewData();
                return View(model);
            }

            // =========================
            // GROUP PRODUCT
            // =========================

            var groupedProducts = model.OrderDetails
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            // =========================
            // LOAD PRODUCTS 1 LẦN
            // =========================

            var productIds = groupedProducts
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            // =========================
            // VALIDATE STOCK
            // =========================

            foreach (var item in groupedProducts)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    ModelState.AddModelError("",
                        "Sản phẩm không tồn tại.");

                    LoadViewData();
                    return View(model);
                }

                decimal stock = product.StockQuantity ?? 0;

                if (stock < item.TotalQuantity)
                {
                    ModelState.AddModelError("",
                        $"'{product.ProductName}' chỉ còn {stock} {product.Unit}.");

                    LoadViewData();
                    return View(model);
                }
            }

            // =========================
            // CREATE ORDER
            // =========================

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                decimal total = 0;

                foreach (var item in model.OrderDetails)
                {
                    total += item.Quantity * item.UnitPrice;

                    // trừ tồn
                    products[item.ProductId].StockQuantity =
                        (products[item.ProductId].StockQuantity ?? 0)
                        - item.Quantity;
                }

                model.Order.TotalAmount = total;

                // add details vào order
                model.Order.OrderDetails = model.OrderDetails;

                _context.Orders.Add(model.Order);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details),
                    new { id = model.Order.OrderId });
            }
            catch
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError("",
                    "Có lỗi xảy ra khi tạo đơn hàng.");

                LoadViewData();

                return View(model);
            }
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

        public IActionResult SearchCustomers(string q)
        {
            var data = _context.Customers
                .Where(x => x.CustomerName.Contains(q))
                .Select(x => new
                {
                    id = x.CustomerId,
                    name = x.CustomerName
                })
                .Take(20)
                .ToList();

            return Json(data);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            LoadViewData();

            var vm = new OrderCreateViewModel
            {
                Order = order,
                OrderDetails = order.OrderDetails.ToList()
            };

            return View(vm);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderCreateViewModel model)
        {
            if (id != model.Order.OrderId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                LoadViewData();
                return View(model);
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // hoàn kho cũ trước khi validate
            await RestoreStock(order.OrderDetails);

            var error = await ValidateOrderDetails(model.OrderDetails);

            if (error != null)
            {
                // rollback kho cũ
                await DeductStock(order.OrderDetails);

                ModelState.AddModelError("", error);

                LoadViewData();

                return View(model);
            }

            // update order
            order.CustomerId = model.Order.CustomerId;
            order.EmployeeId = model.Order.EmployeeId;
            order.SiteId = model.Order.SiteId;
            order.OrderDate = model.Order.OrderDate;

            // xoá detail cũ
            _context.OrderDetails.RemoveRange(order.OrderDetails);

            // add detail mới
            foreach (var item in model.OrderDetails)
            {
                item.OrderId = order.OrderId;

                _context.OrderDetails.Add(item);
            }

            // trừ kho mới
            await DeductStock(model.OrderDetails);

            // cập nhật tổng tiền
            order.TotalAmount =
                CalculateTotal(model.OrderDetails);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details),
                new { id = order.OrderId });
        }
        public async Task<IActionResult> EditDraft(int id, OrderCreateViewModel model)
        {
            var error = await ValidateOrderDetails(model.OrderDetails);

            if (error != null)
            {
                ModelState.AddModelError("", error);

                LoadViewData();

                return View(model);
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // =========================
            // HOÀN LẠI TỒN KHO CŨ
            // =========================

            foreach (var oldDetail in order.OrderDetails)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == oldDetail.ProductId);

                if (product != null)
                {
                    product.StockQuantity =
                        (product.StockQuantity ?? 0) + oldDetail.Quantity;
                }
            }

            // =========================
            // GOM SP MỚI ĐỂ CHECK KHO
            // =========================

            var groupedProducts = model.OrderDetails
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToList();

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

                if (item.TotalQuantity <= 0)
                {
                    ModelState.AddModelError("",
                        "Số lượng phải lớn hơn 0.");

                    LoadViewData();

                    return View(model);
                }

                decimal stock = product.StockQuantity ?? 0;

                if (stock < item.TotalQuantity)
                {
                    TempData["Error"] =
                        $"Sản phẩm '{product.ProductName}' chỉ còn {stock} {product.Unit}.";

                    ModelState.AddModelError("",
                        $"Sản phẩm '{product.ProductName}' không đủ tồn kho.");

                    // rollback stock cũ nếu fail
                    foreach (var rollback in order.OrderDetails)
                    {
                        var rollbackProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == rollback.ProductId);

                        if (rollbackProduct != null)
                        {
                            rollbackProduct.StockQuantity =
                                (rollbackProduct.StockQuantity ?? 0) - rollback.Quantity;
                        }
                    }

                    LoadViewData();
                    return View(model);
                }
            }

            // =========================
            // UPDATE ORDER
            // =========================

            order.CustomerId = model.Order.CustomerId;
            order.EmployeeId = model.Order.EmployeeId;
            order.SiteId = model.Order.SiteId;
            order.OrderDate = model.Order.OrderDate;

            // =========================
            // XÓA DETAIL CŨ
            // =========================

            _context.OrderDetails.RemoveRange(order.OrderDetails);

            decimal total = 0;

            // =========================
            // ADD DETAIL MỚI
            // =========================

            foreach (var item in model.OrderDetails)
            {
                item.OrderId = order.OrderId;

                total += item.Quantity * item.UnitPrice;

                _context.OrderDetails.Add(item);

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product != null)
                {
                    product.StockQuantity =
                        (product.StockQuantity ?? 0) - item.Quantity;
                }
            }

            order.TotalAmount = total;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = order.OrderId });
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
                .OrderBy(o => o.OrderDate)
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

        private async Task<string?> ValidateOrderDetails(
    IEnumerable<OrderDetail> details)
        {
            if (details == null || !details.Any())
            {
                return "Đơn hàng chưa có sản phẩm.";
            }

            var groupedProducts = details
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            foreach (var item in groupedProducts)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product == null)
                {
                    return "Sản phẩm không tồn tại.";
                }

                if (item.TotalQuantity <= 0)
                {
                    return "Số lượng phải lớn hơn 0.";
                }

                decimal stock = product.StockQuantity ?? 0;

                if (stock < item.TotalQuantity)
                {
                    return $"Sản phẩm '{product.ProductName}' chỉ còn {stock} {product.Unit}.";
                }
            }

            return null;
        }

        private async Task DeductStock(
            IEnumerable<OrderDetail> details)
        {
            foreach (var item in details)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product != null)
                {
                    product.StockQuantity =
                        (product.StockQuantity ?? 0) - item.Quantity;
                }
            }
        }

        private async Task RestoreStock(
            IEnumerable<OrderDetail> details)
        {
            foreach (var item in details)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                if (product != null)
                {
                    product.StockQuantity =
                        (product.StockQuantity ?? 0) + item.Quantity;
                }
            }
        }

        private decimal CalculateTotal(
            IEnumerable<OrderDetail> details)
        {
            return details.Sum(x => x.Quantity * x.UnitPrice);
        }
    }
}