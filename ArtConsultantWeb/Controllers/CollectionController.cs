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
    public class CollectionController : ApiController
    {
        // GET api/collection
        [ActionName("DefaultAction")]
        public IEnumerable<Collection> Get(string keyword = "")
        {
            List<Collection> cs = new List<Collection>();
            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT c.*, u.*, COUNT(DISTINCT(p.PaintingId)) AS PaintingCount, COUNT(DISTINCT(f.FollowerId)) AS FollowerCount " +
                    "FROM Users AS u, Collections AS c " +
                    "LEFT OUTER JOIN CollectionPaintings AS p " +
                    "ON c.CollectionId = p.CollectionId " +
                    "LEFT OUTER JOIN Followers AS f " +
                    "ON c.UserId = f.FolloweeId " +
                    "WHERE c.UserId = u.UserId ";
                if (keyword != "")
                {
                    query = "AND (c.Name LIKE \"%" + keyword + "\" " +
                            "OR c.Name LIKE \"%" + keyword + "%\" " +
                            "OR c.Name LIKE \"" + keyword + "%\" " +
                            "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"%" + keyword + "\" " +
                            "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"%" + keyword + "%\" " +
                            "OR CONCAT(u.FirstName,\" \",u.LastName) LIKE \"" + keyword +" %\") ";
                }
                query += "GROUP BY c.CollectionId " +
                    "HAVING PaintingCount > 0";
                Collection c;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                while (reader.Read())
                {
                    c = new Collection();

                    c.CollectionId = DataUtils.getInt32(reader, "CollectionId");
                    c.UserId = DataUtils.getInt32(reader, "UserId");
                    c.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    c.Name = DataUtils.getString(reader, "Name");
                    c.CollectionPicture = DataUtils.getString(reader, "CollectionPicture");
                    c.PaintingCount = DataUtils.getInt32(reader, "PaintingCount");
                    c.User = new User();

                    c.User.Username = DataUtils.getString(reader, "Username");
                    c.User.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    c.User.FirstName = DataUtils.getString(reader, "FirstName");
                    c.User.LastName = DataUtils.getString(reader, "LastName");
                    c.User.City = DataUtils.getString(reader, "City");
                    c.User.State = DataUtils.getString(reader, "State");
                    c.User.Country = DataUtils.getString(reader, "Country");
                    c.User.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                    c.Status.Code = StatusCode.OK;
                    c.Status.Description = DataUtils.OK;

                    cs.Add(c);
                }
                DataUtils.closeConnection(connection);
            }

            return cs;
        }

        // GET api/collection/5
        [ActionName("DefaultAction")]
        public Collection Get(int id)
        {
            Collection c = new Collection();
            c.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * " + 
                    "FROM Collections " +
                    "WHERE CollectionId = " + id;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                if (reader.Read())
                {
                    c.CollectionId = DataUtils.getInt32(reader, "CollectionId");
                    c.UserId = DataUtils.getInt32(reader, "UserId");
                    c.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    c.Name = DataUtils.getString(reader, "Name");
                    c.CollectionPicture = DataUtils.getString(reader, "CollectionPicture");
                    c.Paintings = new List<Painting>();
                    c.User = new User();

                    query = "SELECT u.* " +
                        "FROM Users AS u, Collections AS c " +
                         "WHERE u.UserId = c.UserId AND c.CollectionId = " + id;
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                    if (reader.Read())
                    {
                        c.User.UserId = DataUtils.getInt32(reader, "UserId");
                        c.User.Username = DataUtils.getString(reader, "Username");
                        c.User.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                        c.User.FirstName = DataUtils.getString(reader, "FirstName");
                        c.User.LastName = DataUtils.getString(reader, "LastName");
                        c.User.City = DataUtils.getString(reader, "City");
                        c.User.State = DataUtils.getString(reader, "State");
                        c.User.Country = DataUtils.getString(reader, "Country");
                        c.User.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                        query = "SELECT p.*, a.*, u.*, COUNT(DISTINCT(pl.UserId)) AS LikeCount " +
                            "FROM (Collections AS c, CollectionPaintings AS cp, Paintings AS p, Users AS u, Artists AS a) LEFT JOIN PaintingLikes AS pl ON pl.PaintingId = p.PaintingId " +
                            "WHERE cp.CollectionId = c.CollectionId AND p.PaintingId = cp.PaintingId AND p.ArtistId = a.ArtistId AND a.UserId = u.UserId AND c.CollectionId = " + id + " " +
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

                            c.Paintings.Add(p);
                        }

                        c.Status.Code = StatusCode.OK;
                        c.Status.Description = DataUtils.OK;
                    }
                }
                else
                {
                    c.Status.Code = StatusCode.NotFound;
                    c.Status.Description = "Gallery not found!";
                }
                DataUtils.closeConnection(connection);
            }

            return c;
        }

        // POST api/collection/5/AddPainting
        [HttpPost]
        public BaseModel AddPainting(int id, Painting painting)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "SELECT * " + 
                    "FROM CollectionPaintings " +
                    "WHERE CollectionId=\"" + id + "\" AND PaintingId=\"" + painting.PaintingId + "\"";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.HasRows)
                {
                    bm.Status.Code = StatusCode.AlreadyExists;
                    bm.Status.Description = "Collection already contains that Painting.";
                }
                else
                {
                    query = "INSERT INTO CollectionPaintings " +
                        "VALUES (\"" + id + "\",\"" + painting.PaintingId + "\")";
                    reader.Close();
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    if (reader.RecordsAffected > 0)
                    {
                        bm.Status.Code = StatusCode.OK;
                        bm.Status.Description = "Success adding Painting.";
                    }
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/collection/5/RemovePainting
        [HttpPost]
        public BaseModel RemovePainting(int id, Painting painting)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null)
            {
                string query = "DELETE FROM CollectionPaintings " +
                    "WHERE CollectionId = " + id + " AND PaintingId = " + painting.PaintingId;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected > 0)
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Success removing Painting.";
                }
                else
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "No painting to remove.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/collection
        public void Post([FromBody]string value)
        {
        }

        // PUT api/collection/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/collection/5
        public void Delete(int id)
        {
        }
    }
}
