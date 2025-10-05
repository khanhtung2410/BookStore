using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Entities
{
    public static class BookConsts
    {
        public const int MaxTitleLength = 200;
        public const int MaxAuthorLength = 50;
        public const int MaxDescriptionLength = 1000;
        public enum Genre
        {
            Fiction,
            NonFiction,
            Mystery,
            Thriller,
            Romance,
            Fantasy,
            ScienceFiction,
            Biography,
            History,
            Adventure,
            Children,
            SelfHelp,
            Classic,
            Travel,
            Cooking,
            Horror,
            GraphicNovel
        }
    }

    [Table("Books")]
    public class Book : FullAuditedEntity<int>, IValidatableObject
    {
        [Required]
        [StringLength(BookConsts.MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        [StringLength(BookConsts.MaxAuthorLength)]
        public string Author { get; set; }

        [Required]
        public BookConsts.Genre Genre { get; set; }

        [StringLength(BookConsts.MaxDescriptionLength)]
        public string Description { get; set; }

        public DateTime? PublishedDate { get; set; }

        public Book(string title, string author, BookConsts.Genre genre, string description, DateTime? publishedDate)
        {
            Title = title;
            Author = author;
            Genre = genre;
            Description = description;
            PublishedDate = publishedDate;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(typeof(BookConsts.Genre), Genre))
            {
                yield return new ValidationResult("Genre is required and must be a valid value.", new[] { nameof(Genre) });
            }
            if (PublishedDate.HasValue && PublishedDate.Value > DateTime.Now.AddYears(1))
            {
                yield return new ValidationResult("Published date cannot be more than 1 year in the future.", new[] { nameof(PublishedDate) });
            }
        }
    }

    [Table("BookInventories")]
    public class BookInventory : FullAuditedEntity<int>
    {
        [ForeignKey(nameof(Book))]
        public int BookId { get; set; } 

        [Required]
        public long Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BuyPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellPrice { get; set; }

        public BookInventory(int bookId, long amount, decimal buyPrice, decimal sellPrice)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative", nameof(amount));
            if (buyPrice < 0) throw new ArgumentException("Buy price cannot be negative", nameof(buyPrice));
            if (sellPrice < 0) throw new ArgumentException("Sell price cannot be negative", nameof(sellPrice));
            BookId = bookId;
            Amount = amount;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
        }
    }
}