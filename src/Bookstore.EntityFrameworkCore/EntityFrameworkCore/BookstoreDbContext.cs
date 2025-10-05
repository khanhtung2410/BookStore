using Abp.Zero.EntityFrameworkCore;
using Bookstore.Authorization.Roles;
using Bookstore.Authorization.Users;
using Bookstore.Entities;
using Bookstore.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.EntityFrameworkCore
{
    public class BookstoreDbContext : AbpZeroDbContext<Tenant, Role, User, BookstoreDbContext>
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<BookInventory> BookInventories { get; set; }

        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
            : base(options)
        {
        }
    }
}
