using System.ComponentModel.DataAnnotations;

namespace Bookstore.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}