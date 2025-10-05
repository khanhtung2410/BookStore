using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.EntityFrameworkCore
{
    public static class BookstoreDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<BookstoreDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<BookstoreDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
