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
    [Table("Galleries")]
    public class Gallery : BaseModel
    {
        public int GalleryId { get; set; }
        [JsonIgnore]
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string GalleryPicture { get; set; }
        public DateTime CreatedDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int PaintingCount { get; set; }

        [NotMapped]
        public User ArtConsultant { get; set; }
        [NotMapped]
        public ICollection<Painting> Paintings { get; set; }
    }

    public class GalleryDBContext : DbContext
    {
        public GalleryDBContext() : base("DefaultConnection")
        {

        }
    }
}