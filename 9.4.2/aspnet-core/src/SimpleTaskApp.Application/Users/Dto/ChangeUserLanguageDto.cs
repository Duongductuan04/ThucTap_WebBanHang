using System.ComponentModel.DataAnnotations;

namespace SimpleTaskApp.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}