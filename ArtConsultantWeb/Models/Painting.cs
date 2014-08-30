using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ArtConsultantWeb.Models
{
    [Table("Paintings")]
    public class Painting : BaseModel
    {
        public int PaintingId { get; set; }
        public string Url { get; set; }
        public string ZoomUrl { get; set; }
        public string RoomUrl { get; set; }
        [JsonIgnore]
        [ForeignKey("Artists")]
        public int ArtistId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Dimension { get; set; }
        public string Story { get; set; }
        public int Price { get; set; }
        public string Medium { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LikeCount { get; set; }

        [NotMapped]
        public IEnumerable<User> PaintingLikes;
        [NotMapped]
        public User Artist { get; set; }

        // From Artist table in case a User does not exist for the Artist 
        // (i.e. when painting uploaded by ArtConsultant)
        [NotMapped]
        public string ArtistBio { get; set; }
        [NotMapped]
        public string ArtistName { get; set; }
    }

    public class PaintingDbContext : DbContext
    {
        public PaintingDbContext() : base("DefaultConnection")
        {

        }
    }
}