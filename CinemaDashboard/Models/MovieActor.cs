using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaDashboard.Models
{
    // Mapping entity between Movie and Actor (List<Actor> in the spec)
    public class MovieActor
    {
        [Required]
        public int MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public virtual Movie Movie { get; set; }

        [Required]
        public int ActorId { get; set; }

        [ForeignKey(nameof(ActorId))]
        public virtual Actor Actor { get; set; }
    }
}
