﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using ArtConsultantWeb.Utils;
using MySql.Data.MySqlClient;

namespace ArtConsultantWeb.Models
{
    public enum UserType {
        Consumer,
        Artist,
        ArtConsultant
    }

    [Table("Users")]
    public class User : BaseModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ImageUrl { get; set; }
        public bool IsFaceBook { get; set; }

        [NotMapped]
        public int FollowerCount { get; set; }
        [NotMapped]
        public Collection MyCollection { get; set; }
        [NotMapped]
        [JsonIgnore]
        public UserType userType { get; set; }

        public bool ReadUser(MySqlDataReader reader)
        {
            if (reader.Read())
            {
                if (reader.HasRows)
                {
                    this.UserId = DataUtils.getInt32(reader, "UserId");
                    this.Username = DataUtils.getString(reader, "Username");
                    this.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    this.FirstName = DataUtils.getString(reader, "FirstName");
                    this.LastName = DataUtils.getString(reader, "LastName");
                    this.City = DataUtils.getString(reader, "City");
                    this.State = DataUtils.getString(reader, "State");
                    this.Country = DataUtils.getString(reader, "Country");
                    this.ImageUrl = DataUtils.getString(reader, "ImageUrl");
                    this.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                    if (reader.NextResult() && reader.Read())
                    {
                        this.MyCollection = new Collection();
                        this.MyCollection.Paintings = new List<Painting>();

                        this.MyCollection.CollectionId = DataUtils.getInt32(reader, "CollectionId");
                        this.MyCollection.CreatedDate = reader.GetDateTime("CreatedDate");
                        this.MyCollection.Name = DataUtils.getString(reader, "Name");
                        this.MyCollection.CollectionPicture = DataUtils.getString(reader, "CollectionPicture");

                        if (reader.NextResult())
                        {
                            Painting p;

                            while (reader.Read())
                            {
                                p = new Painting();
                                p.PaintingId = DataUtils.getInt32(reader, "PaintingId");
                                p.Name = DataUtils.getString(reader, "Name");
                                p.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                                p.Dimension = DataUtils.getString(reader, "Dimension");
                                p.Url = DataUtils.getString(reader, "Url");
                                p.Story = DataUtils.getString(reader, "Story");
                                p.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                                p.Price = DataUtils.getInt32(reader, "Price");
                                p.Medium = DataUtils.getString(reader, "Medium");
                                p.ArtistBio = DataUtils.getString(reader, "ArtistBio");
                                p.ArtistName = DataUtils.getString(reader, "ArtistName");
                                p.LikeCount = DataUtils.getInt32(reader, "LikeCount");

                                p.Artist = new User();
                                p.Artist.UserId = DataUtils.getInt32(reader, "UserId");
                                p.Artist.Username = DataUtils.getString(reader, "Username");
                                p.Artist.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                                p.Artist.FirstName = DataUtils.getString(reader, "FirstName");
                                p.Artist.LastName = DataUtils.getString(reader, "LastName");
                                p.Artist.City = DataUtils.getString(reader, "City");
                                p.Artist.State = DataUtils.getString(reader, "State");
                                p.Artist.Country = DataUtils.getString(reader, "Country");

                                this.MyCollection.Paintings.Add(p);
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public class UserDBContext : DbContext
    {
        public UserDBContext() : base("DefaultConnection")
        {

        }
    }
}