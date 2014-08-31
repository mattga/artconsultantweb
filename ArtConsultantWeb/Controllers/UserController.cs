using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;
using ArtConsultantWeb.Models;
using ArtConsultantWeb.Utils;

namespace ArtConsultantWeb.Controllers
{
    public class UserController : ApiController
    {
        // GET api/user
        [ActionName("DefaultAction")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/user/5
        [ActionName("DefaultAction")]
        public User Get(int id)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && u != null && u.Username != "")
            {
                u.UserId = id;

                if (u.ReadUser(connection))
                {
                    u.Status.Code = StatusCode.OK;
                    u.Status.Description = DataUtils.OK;
                }
                else
                {
                    u.Status.Code = StatusCode.NotFound;
                    u.Status.Description = "Incorrect username or password";
                }
                DataUtils.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/id/authenticate
        [HttpPost]
        public User Authenticate(string id, User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "SELECT * FROM Users WHERE Username=\"" + user.Username + "\" AND Password=\"" + user.Password + "\"";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        u.UserId = DataUtils.getInt32(reader, "UserId");
                        u.Username = DataUtils.getString(reader, "Username");
                        u.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                        u.FirstName = DataUtils.getString(reader, "FirstName");
                        u.LastName = DataUtils.getString(reader, "LastName");
                        u.City = DataUtils.getString(reader, "City");
                        u.State = DataUtils.getString(reader, "State");
                        u.Country = DataUtils.getString(reader, "Country");
                        u.ImageUrl = DataUtils.getString(reader, "ImageUrl");
                        u.IsFaceBook = (DataUtils.getInt32(reader, "IsFaceBook") == 0 ? false : true);

                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = DataUtils.OK;

                        query = "SELECT c.*" +
                            "FROM Users AS u, Collections AS c" +
                            "WHERE c.UserId = u.UserId AND IsCustom = 0 AND u.Username = \"" + u.Username + "\" AND u.Password = \"" + u.Password + "\"";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);
                        if (reader.Read())
                        {
                            u.MyCollection = new Collection();
                            u.MyCollection.Paintings = new List<Painting>();

                            u.MyCollection.CollectionId = DataUtils.getInt32(reader, "CollectionId");
                            u.MyCollection.CreatedDate = reader.GetDateTime("CreatedDate");
                            u.MyCollection.Name = DataUtils.getString(reader, "Name");
                            u.MyCollection.CollectionPicture = DataUtils.getString(reader, "CollectionPicture");

                            query = "SELECT p.*, a.*, u1.*, COUNT(pl.UserId) AS LikeCount" +
                                "FROM (Collections AS c, CollectionPaintings AS cp, Paintings AS p, Users AS u1, Users AS u2, Artists AS a) LEFT JOIN PaintingLikes AS pl ON pl.PaintingId = p.PaintingId" +
                                "WHERE cp.CollectionId = c.CollectionId AND p.PaintingId = cp.PaintingId AND p.ArtistId = a.ArtistId AND a.UserId = u1.UserId AND c.UserId = u2.UserId AND u2.Username = \"" + u.Username + "\" AND u2.Password = \"" + u.Password + "\"" +
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

                                u.MyCollection.Paintings.Add(p);
                            }

                            u.Status.Code = StatusCode.OK;
                            u.Status.Description = DataUtils.OK;
                        }
                    }
                }
                else
                {
                    u.Status.Code = StatusCode.NotFound;
                    u.Status.Description = "Incorrect username or password";
                }
                DataUtils.closeConnection(connection);
            }

            return u;
        }

        // POST api/user
        [ActionName("DefaultAction")]
        public BaseModel Post(User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "";
                if (user.userType == UserType.Consumer)
                {
                    query = "SELECT UserId FROM Users WHERE Username=\"" + user.Username + "\" AND IsFaceBook=\"" + user.IsFaceBook + "\"";
                    MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    if (reader.HasRows)
                    {
                        int isFbInt = (user.IsFaceBook ? 1 : 0);
                        reader.Close();
                        query = "INSERT INTO Users (Username, Password, FirstName, LastName, ImageUrl, IsFaceBook) " +
                            "VALUES (\"" + user.Username + "\",\"" + user.Password + "\",\"" + user.FirstName + "\",\"" + user.LastName + "\""
                                + (user.ImageUrl == null ? "NULL" : "\"" + user.ImageUrl + "\"") + ",\"" + isFbInt + "\")";
                        reader.Close();
                        reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                        if (reader.RecordsAffected > 0)
                        {
                            query = "SET @UserId = LAST_INSERT_ID()";
                            DataUtils.executeQuery(connection, query);

                            query = "INSERT INTO Consumers (UserId)" +
                                "VALUES (@UserId)";
                            DataUtils.executeQuery(connection, query);

                            query = "INSERT INTO Collections (UserId, Name, IsCustom)" +
                                "VALUES (@UserId, \"" + user.FirstName + " " + user.LastName + "'s Wall\", 0)";
                            DataUtils.executeQuery(connection, query);

                            bm.Status.Code = StatusCode.OK;
                            bm.Status.Description = "Success creating user.";
                        }
                        else
                        {
                            bm.Status.Code = StatusCode.AlreadyExists;
                            bm.Status.Description = "User already exists.";
                        }
                    }
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // PUT api/user
        [ActionName("DefaultAction")]
        public User Put(User user)
        {
            User u = new User();
            u.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "";
                if (user.userType == UserType.Consumer)
                {
                    int isFbInt = (user.IsFaceBook ? 1 : 0);
                    query = "SELECT UserId FROM Users WHERE Username=\"" + user.Username + "\" AND IsFaceBook=" + user.IsFaceBook;
                }
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.Read())
                {
                    query = "UPDATE Users " +
                        "SET Password=\"" + user.Password + "\",Username=\"" + user.Username + "\",FirstName=\"" + user.FirstName + "\",LastName=\"" + user.LastName +
                            "\",ImageUrl=\"" + user.ImageUrl + "\",IsFaceBook=" + user.IsFaceBook + " " +
                        "WHERE Username = _Username AND Password = _Password";
                    DataUtils.executeQuery(connection, query);
                    
                    query = "SELECT UserId " +
                        "FROM Users " +
                        "WHERE Username = \"" + user.Username + "\" AND Password = \"" + user.Password + "\"";
                    reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                    u.UserId = reader.GetInt32(0);
                    if (u.ReadUser(connection))
                    {
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success updating user.";
                    }
                }
                else
                {
                    query = " INSERT INTO Users (Username, Password, FirstName, LastName, ImageUrl, IsFaceBook) " +
                        "VALUES (\"" + user.Username + "\", \"" + user.Password + "\", \"" + user.FirstName + "\", \"" + user.LastName + "\", " +
                        (user.ImageUrl == null ? "NULL" : "\"" + user.ImageUrl + "\"") + ", " + user.IsFaceBook + ")";
                    DataUtils.executeQuery(connection, query);

                    reader.Close();
                    u.UserId = ((MySqlDataReader)DataUtils.executeQuery(connection, "SELECT LAST_INSERT_ID()")).GetInt32(0);
                    if (u.ReadUser(connection))
                    {
                        u.Status.Code = StatusCode.OK;
                        u.Status.Description = "Success updating user.";
                    }
                }
                DataUtils.closeConnection(connection);
            }

            return u;
        }

        // POST api/user/5/follow
        [ActionName("DefaultAction")]
        [HttpPost]
        public BaseModel Follow(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "INSERT INTO Followers (FollowerId, FolloweeId) " +
                    "VALUES (" + id + "," + user.UserId + ")";
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected == 0)
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "No user found.";
                }
                else
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Success creating user.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }

        // POST api/user/5/unfollow
        [ActionName("DefaultAction")]
        [HttpPost]
        public BaseModel Unfollow(string id, User user)
        {
            BaseModel bm = new BaseModel();
            bm.Status.Code = StatusCode.Error;

            MySqlConnection connection = DataUtils.getConnection();

            if (connection != null && user != null && user.Username != "")
            {
                string query = "DELETE FROM Followers " +
                    "WHERE FollowerId = " + id + " AND FolloweeId = " + user.UserId;
                MySqlDataReader reader = (MySqlDataReader)DataUtils.executeQuery(connection, query);

                if (reader.RecordsAffected == 0)
                {
                    bm.Status.Code = StatusCode.NotFound;
                    bm.Status.Description = "No user found.";
                }
                else
                {
                    bm.Status.Code = StatusCode.OK;
                    bm.Status.Description = "Success unfollowing user.";
                }
                DataUtils.closeConnection(connection);
            }

            return bm;
        }
    }
}
