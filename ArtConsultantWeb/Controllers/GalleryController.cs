using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;
using ArtConsultantWeb.Models;
using ArtConsultantWeb.Utils;

namespace ArtConsultantWeb.Controllers
{
    public class GalleryController : ApiController
    {
        // GET api/gallery
        [ActionName("DefaultAction")]
        public IEnumerable<Gallery> Get(string keyword="")
        {
            List<Gallery> gs = new List<Gallery>();
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT g.*, u.*, COUNT(DISTINCT(p.PaintingId)) AS PaintingCount, COUNT(DISTINCT(f.FollowerId)) AS FollowerCount " +
                    "FROM Users AS u, ArtConsultants AS ac, Galleries AS g " +
                    "LEFT OUTER JOIN GalleryPaintings AS p " +
                    "ON g.GalleryId = p.GalleryId  " +
                    "LEFT OUTER JOIN Followers AS f " +
                    "ON g.UserId = f.FolloweeId " +
                    "WHERE g.UserId = u.UserId AND u.UserId = ac.UserId ";
                if (keyword != "")
                {
                    query = "AND (c.Name LIKE \"%" + keyword + "\" " +
                            "OR c.Name LIKE \"%" + keyword + "%\" " +
                            "OR c.Name LIKE \"" + keyword + "%\" " +
                            "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"%" + keyword + "\" " +
                            "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"%" + keyword + "%\" " +
                            "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"" + keyword + " %\") ";
                }
                query += "GROUP BY g.GalleryId " +
                    "HAVING PaintingCount > 0";
                Gallery g;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                while (reader.Read())
                {
                    g = new Gallery();

                    g.GalleryId = DataUtils.getInt32(reader, "GalleryId");
                    g.UserId = DataUtils.getInt32(reader, "UserId");
                    g.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    g.Name = DataUtils.getString(reader, "Name");
                    g.GalleryPicture = DataUtils.getString(reader, "GalleryPicture");
                    g.City = DataUtils.getString(reader, "City");
                    g.State = DataUtils.getString(reader, "State");
                    g.Country = DataUtils.getString(reader, "Country");
                    g.PaintingCount = DataUtils.getInt32(reader, "PaintingCount");
                    g.ArtConsultant = new User();
                    g.Paintings = new List<Painting>();

                    g.ArtConsultant.UserId = g.UserId;
                    g.ArtConsultant.ImageUrl = DataUtils.getString(reader, "ImageUrl");
                    g.ArtConsultant.Username = DataUtils.getString(reader, "Username");
                    g.ArtConsultant.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    g.ArtConsultant.FirstName = DataUtils.getString(reader, "FirstName");
                    g.ArtConsultant.LastName = DataUtils.getString(reader, "LastName");
                    g.ArtConsultant.City = DataUtils.getString(reader, "City");
                    g.ArtConsultant.State = DataUtils.getString(reader, "State");
                    g.ArtConsultant.Country = DataUtils.getString(reader, "Country");
                    g.ArtConsultant.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                    query = "SELECT p.Url " +
                            "FROM Galleries AS g, GalleryPaintings AS gp, Paintings AS p " +
                            "WHERE gp.GalleryId = g.GalleryId AND p.PaintingId = gp.PaintingId AND g.GalleryId = " + g.GalleryId;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                    Painting p;
                    while (reader.Read())
                    {
                        p = new Painting();
                        p.Url = DataUtils.getString(reader, "Url");

                        g.Paintings.Add(p);
                    }
                    g.Status.Code = StatusCode.OK;
                    g.Status.Description = DataUtils.OK;

                    gs.Add(g);
                }
                DataUtils.closeConnection(connection);
            }

            return gs;
        }

        // GET api/gallery/5
        [ActionName("DefaultAction")]
        public Gallery Get(int id)
        {
            Gallery g = new Gallery();
            g.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * " +
                    "FROM Galleries " +
                    "WHERE GalleryId = " + id;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                if (reader.Read())
                {
                    g.GalleryId = DataUtils.getInt32(reader, "GalleryId");
                    g.UserId = DataUtils.getInt32(reader, "UserId");
                    g.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    g.Name = DataUtils.getString(reader, "Name");
                    g.GalleryPicture = DataUtils.getString(reader, "GalleryPicture");
                    g.City = DataUtils.getString(reader, "City");
                    g.State = DataUtils.getString(reader, "State");
                    g.Country = DataUtils.getString(reader, "Country");
                    g.Paintings = new List<Painting>();
                    g.ArtConsultant = new User();

                    query = "SELECT u.*, ac.* " +
                        "FROM Users AS u, Galleries AS g, ArtConsultants AS ac " +
                        "WHERE u.UserId = g.UserId AND u.UserId = ac.UserId AND g.GalleryId = " + id;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                    if (reader.Read())
                    {
                        g.ArtConsultant.UserId = DataUtils.getInt32(reader, "UserId");
                        g.ArtConsultant.Username = DataUtils.getString(reader, "Username");
                        g.ArtConsultant.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                        g.ArtConsultant.FirstName = DataUtils.getString(reader, "FirstName");
                        g.ArtConsultant.LastName = DataUtils.getString(reader, "LastName");
                        g.ArtConsultant.City = DataUtils.getString(reader, "City");
                        g.ArtConsultant.State = DataUtils.getString(reader, "State");
                        g.ArtConsultant.Country = DataUtils.getString(reader, "Country");
                        g.ArtConsultant.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                        query = "SELECT p.*, a.*, u.*, COUNT(DISTINCT(pl.UserId)) AS LikeCount " +
                            "FROM (Galleries AS g, GalleryPaintings AS gp, Paintings AS p, Users AS u, Artists AS a) LEFT JOIN PaintingLikes AS pl ON pl.PaintingId = p.PaintingId " +
                            "WHERE gp.GalleryId = g.GalleryId AND p.PaintingId = gp.PaintingId AND p.ArtistId = a.ArtistId AND a.UserId = u.UserId AND g.GalleryId = " + id + " " +
                            "GROUP BY p.PaintingId";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                        Painting p;
                        while (reader.Read())
                        {
                            p = new Painting();
                            p.PaintingId = DataUtils.getInt32(reader, "PaintingId");
                            p.Name = DataUtils.getString(reader, "Name");
                            p.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
                            p.Dimension = DataUtils.getString(reader, "Dimension");
                            p.Url = DataUtils.getString(reader, "Url");
                            p.ZoomUrl = DataUtils.getString(reader, "ZoomUrl");
                            p.RoomUrl = DataUtils.getString(reader, "RoomUrl");
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

                            g.Paintings.Add(p);
                        }

                        g.Status.Code = StatusCode.OK;
                        g.Status.Description = DataUtils.OK;
                    }
                }
                else
                {
                    g.Status.Code = StatusCode.NotFound;
                    g.Status.Description = "Gallery not found!";
                }
                DataUtils.closeConnection(connection);
            }

            return g;
        }

        // POST api/gallery
        [ActionName("DefaultAction")]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/gallery/5
        [ActionName("DefaultAction")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/gallery/5
        [ActionName("DefaultAction")]
        public void Delete(int id)
        {
        }
    }
}
