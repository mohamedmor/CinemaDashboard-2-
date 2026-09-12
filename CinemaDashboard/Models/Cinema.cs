using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CinemaDashboard.Models
{
    public class Cinema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string Address { get; set; }

        // stored image file name (wwwroot/uploads/cinemas)
        public string Img { get; set; }

        [NotMapped]
        public IFormFile ImgFile { get; set; }

        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
