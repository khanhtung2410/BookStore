using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bookstore.Entities.Books.BookConsts;

namespace Bookstore.Entities.Books
{
    public static class BookConsts
    {
        public const int MaxTitleLength = 200;
        public const int MaxAuthorLength = 50;
        public const int MaxDescriptionLength = 5000;
        public const int MinISBNLength = 13;
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
        public enum Format
        {
            Hardcover,
            Paperback
        }
    }

    [Table("Books")]
    public class Book : FullAuditedEntity<int>, IValidatableObject
    {
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        [StringLength(MaxAuthorLength)]
        public string Author { get; set; }

        [Required]
        public Genre Genre { get; set; }

        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        // Relationships
        public virtual ICollection<BookEdition> Editions { get; set; } = new List<BookEdition>();
        public virtual ICollection<BookImage> Images { get; set; } = new List<BookImage>();

        public Book() { }

        public Book(string title, string author, Genre genre, string description)
        {
            Title = title;
            Author = author;
            Genre = genre;
            Description = description;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Title))
                yield return new ValidationResult("Title is required.", new[] { nameof(Title) });

            if (string.IsNullOrWhiteSpace(Author))
                yield return new ValidationResult("Author is required.", new[] { nameof(Author) });

            if (!Enum.IsDefined(typeof(Genre), Genre))
                yield return new ValidationResult("Genre is invalid.", new[] { nameof(Genre) });
        }
    }
}
