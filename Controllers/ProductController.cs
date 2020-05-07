using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Northwind.Models;

namespace Northwind.Controllers
{
    public class ProductController : Controller
    {
        // this controller depends on the NorthwindRepository
        private INorthwindRepository repository;
        public ProductController(INorthwindRepository repo) => repository = repo;

        public IActionResult Category() => View(repository.Categories.OrderBy(c => c.CategoryName));
        public IActionResult Index(int id)
        {
            ViewBag.id = id;
            return View(repository.Categories.OrderBy(c => c.CategoryName));
        }
        public IActionResult Discounts() => View(repository.Discounts.Where(d => d.StartTime <= DateTime.Now && d.EndTime > DateTime.Now));
    }
}