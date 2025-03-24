using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace _2280600691_NguyenDinhDo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public string? Sex { get; set; }
    }
}
