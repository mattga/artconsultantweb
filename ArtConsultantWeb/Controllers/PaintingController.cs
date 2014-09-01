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
    public class PaintingController : ApiController
    {
        // GET api/painting
        [ActionName("DefaultAction")]
        public IEnumerable<Painting> Get(string keyword = "", int minPrice = 0, int maxPrice = int.MaxValue, string medium = "")
        {
            List<Painting> ps = new List<Painting>();
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "  SELECT p.*, a.*, u.*, COUNT(pl.UserId) AS LikeCount " +
                    "FROM Users AS u, Artists AS a, Paintings AS p LEFT JOIN PaintingLikes AS pl " +
                    "ON p.PaintingId = pl.PaintingId " +
                    "WHERE p.ArtistId = a.ArtistId AND u.UserId = a.UserId ";
                if (keyword != "")
                {
                  query += "AND (p.Name LIKE \"% " + keyword + "\" " +
                    "OR p.Name LIKE \"%" + keyword + "%\" " +
                    "OR p.Name LIKE \"" + keyword + "%\" " +
                    "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"%" + keyword + "\" " +
                    "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"%" + keyword + "%\" " +
                    "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"" + keyword + "%\") ";
                }
                if (minPrice > 0)
                {
                    query += "AND p.Price >= " + minPrice + " ";
                }
                if (maxPrice < int.MaxValue)
                {
                    query += "AND p.Price <= " + maxPrice + " ";
                }
                if (medium != "")
                {
                    query += "AND medium = \"" + medium + "\" ";
                }
                query += "GROUP BY p.PaintingId ";
                query += "ORDER BY COUNT(pl.UserId) DESC";
                Painting p;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

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

                    ps.Add(p);
                }
                DataUtils.closeConnection(connection);
            }

            return ps;
        }

        // GET api/painting/5/IsLiked
        [HttpGet]
        public BaseModel IsLiked(string id, string UserId)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * FROM PaintingLikes WHERE PaintingId=" + id + " AND UserId=" + UserId;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Painting liked.";
                }
                else
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "No like found.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/painting/5/like
        [HttpPost]
        public BaseModel Like(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * FROM PaintingLikes WHERE PaintingId=" + id + " AND UserId=" + user.UserId;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    bm.Status.Code = StatusCode.AlreadyExists;
                    bm.Status.Description = "Painting already liked.";
                }
                else
                {
                    query = "INSERT INTO PaintingLikes (UserId, PaintingId)" +
                        "VALUES (" + user.UserId + "," + id + ")";
                    DataUtils.executeQuery(connection, query);
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Painting successfully liked.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/painting/5/unlike
        [HttpPost]
        public BaseModel Unlike(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * FROM PaintingLikes WHERE PaintingId=" + id + " AND UserId=" + user.UserId;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    query = "DELETE FROM PaintingLikes " +
                        "WHERE UserId = " + user.UserId + " AND PaintingId = " + id + ")";
                    DataUtils.executeQuery(connection, query);
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Painting successfully liked.";
                }
                else
                {
                    bm.Status.Code = StatusCode.AlreadyExists;
                    bm.Status.Description = "Painting not liked.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // GET api/painting/5/gallery
        [HttpGet]
        public Gallery Gallery(string id)
        {
            Gallery g = new Gallery();
            g.Status.Code = StatusCode.Error;
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "  SELECT g.*, u.*, COUNT(DISTINCT(p.PaintingId)) AS PaintingCount " +
                    "FROM Users AS u, Galleries AS g " +
                    "LEFT OUTER JOIN GalleryPaintings AS p " +
                    "ON p.GalleryId = p.GalleryId " +
                    "WHERE g.UserId = u.UserId AND g.GalleryId IN  " +
                      "(SELECT GalleryId " +
                      "FROM GalleryPaintings " +
                      "WHERE PaintingId = " + id + ")";
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
                    g.PaintingCount = DataUtils.getInt32(reader, "PaintingCount");
                    g.ArtConsultant = new User();

                    g.ArtConsultant.Username = DataUtils.getString(reader, "Username");
                    g.ArtConsultant.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    g.ArtConsultant.FirstName = DataUtils.getString(reader, "FirstName");
                    g.ArtConsultant.LastName = DataUtils.getString(reader, "LastName");
                    g.ArtConsultant.City = DataUtils.getString(reader, "City");
                    g.ArtConsultant.State = DataUtils.getString(reader, "State");
                    g.ArtConsultant.Country = DataUtils.getString(reader, "Country");

                    g.Status.Code = StatusCode.OK;

                }
                DataUtils.closeConnection(connection);
            }

            return g;
        }

        // POST api/painting
        public void Post([FromBody]string value)
        {
        }

        // PUT api/painting/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/painting/5
        public void Delete(int id)
        {
        }
    }
}
