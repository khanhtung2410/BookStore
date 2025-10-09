using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Books.Dto
{
    public class DeleteBookDto
    {
        [Required]
        public int Id { get; set; }
    }
}

