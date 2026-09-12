using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using CinemaDashboard.Models.Enums;

namespace CinemaDashboard.Models
{
    // Movie (Id, Name, Des, Price, Status, DateTime, MainImg, SubImages, List<Actor>, CategoryId, CinemaId)
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(2000)]
        [Column("Des")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public MovieStatus Status { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        // main/cover image - stored file name (wwwroot/uploads/movies)
        public string MainImg { get; set; }

        [NotMapped]
        public IFormFile MainImgFile { get; set; }

        // sub / gallery images
        public virtual ICollection<MovieImage> SubImages { get; set; } = new List<MovieImage>();

        [NotMapped]
        public List<IFormFile> SubImageFiles { get; set; } = new List<IFormFile>();

        // cast
        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

        [NotMapped]
        public List<int> SelectedActorIds { get; set; } = new List<int>();

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [ForeignKey(nameof(CinemaId))]
        public virtual Cinema Cinema { get; set; }
    }
}
