using Abp.Zero.EntityFrameworkCore;
using Bookstore.Authorization.Roles;
using Bookstore.Authorization.Users;
using Bookstore.Entities.Books;
using Bookstore.Entities.Carts;
using Bookstore.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.EntityFrameworkCore
{
    public class BookstoreDbContext : AbpZeroDbContext<Tenant, Role, User, BookstoreDbContext>
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<BookInventory> BookInventories { get; set; }
        public DbSet<BookImage> BookImages { get; set; }
        public DbSet<BookEdition> BookEditions { get; set; }
        public DbSet<BookBundle> BookBundles { get; set; }
        public DbSet<BookBundleItem> BookBundleItems { get; set; }
        public DbSet<BookBundleImage> BookBundleImages { get; set; }
        public DbSet<BookDiscount> BookDiscounts { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookEdition>(entity =>
            {
                entity.HasIndex(e => e.ISBN).IsUnique();
            });
            modelBuilder.Entity<CartItem>(b =>
            {
                b.HasOne(i => i.BookEdition)
                 .WithMany()
                 .HasForeignKey(i => i.BookEditionId)
                 .OnDelete(DeleteBehavior.SetNull); // leave CartItem when edition deleted
            });
        }

    }
}
