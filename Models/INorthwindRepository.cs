using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.Models
{
    public interface INorthwindRepository
    {
        IQueryable<Product> Products { get; }
        IQueryable<Category> Categories { get; }
        IQueryable<Discount> Discounts { get; }
        IQueryable<Customer> Customers { get; }
        void AddCustomer(Customer customer);
        void EditCustomer(Customer customer);
        CartItem AddToCart(CartItemJSON cartItemJSON);
    }
}
