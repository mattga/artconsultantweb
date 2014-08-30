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
    [Table("Collections")]
    public class Collection : BaseModel
    {
        public int CollectionId { get; set; }
        [JsonIgnore]
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string CollectionPicture { get; set; }
        public DateTime CreatedDate { get; set; }
        public int PaintingCount { get; set; }

        [NotMapped]
        public User User { get; set; }
        [NotMapped]
        public ICollection<Painting> Paintings { get; set; }
    }

    public class CollectionDBContext : DbContext
    {
        public CollectionDBContext()
            : base("DefaultConnection")
        {

        }
    }
}