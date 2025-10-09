using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class BookImageDto
    {
        [Required]
        public int BookId { get; set; }
        public string ImagePath { get; set; }   
        public string Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsCover { get; set; }
    }
    public class BundleImageDto 
    {
        public int BundleId { get; set; }
        public string ImagePath { get; set; }
        public string Caption { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsCover { get; set; }
    }
}
