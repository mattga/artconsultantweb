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
                string query = "CALL GetPaintingsWithSearchInfo(" 
                    + (keyword == "" ? "NULL" : "\""+keyword+"\"") + ","
                    + (minPrice == 0 ? "NULL" : "\"" + minPrice + "\"") + ","
                    + (maxPrice == int.MaxValue ? "NULL" : "\"" + maxPrice + "\"") + ","
                    + (medium == "" ? "NULL" : "\"" + medium + "\"") + ")";
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
                string query = "CALL IsPaintingLikedByUser(\"" + id + "\",\"" + UserId + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) < 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = "Painting liked.";
                    }
                    else
                    {
                        bm.Status.Code = StatusCode.NotFound;
                        bm.Status.Description = "No like found.";
                    }
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
                string query = "CALL LikePainting(\"" + id + "\",\"" + user.UserId + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = "Painting successfully liked.";
                    }
                    else
                    {
                        bm.Status.Code = StatusCode.AlreadyExists;
                        bm.Status.Description = "Painting already liked.";
                    }
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
                string query = "CALL UnlikePainting(\"" + id + "\",\"" + user.UserId + "\")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = "Painting successfully unliked.";
                    }
                    else
                    {
                        bm.Status.Code = StatusCode.NotFound;
                        bm.Status.Description = "No like found.";
                    }
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
                string query = "CALL GetGalleryForPainting(\"" + id + "\")";
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
