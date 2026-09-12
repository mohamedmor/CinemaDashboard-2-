using System.ComponentModel.DataAnnotations;

namespace CinemaDashboard.Models.Cart
{
    public class CheckoutViewModel
    {
        [Required]
        [MaxLength(150)]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(30)]
        public string Phone { get; set; }
    }
}
