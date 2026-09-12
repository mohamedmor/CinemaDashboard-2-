using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CinemaDashboard.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        // stored image file name (wwwroot/uploads/actors)
        public string Img { get; set; }

        [NotMapped]
        public IFormFile ImgFile { get; set; }

        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
