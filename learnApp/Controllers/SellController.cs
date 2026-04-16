using learnApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace learnApp.Controllers
{
    public class SellController : Controller
    {
        private readonly VlxdContext _context;

        public SellController(VlxdContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }
    }
}
