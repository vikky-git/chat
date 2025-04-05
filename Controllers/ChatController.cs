using System.Linq;
using System.Web.Mvc;
using ChatApp.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System;

namespace ChatApp.Controllers
{
    public class ChatController : Controller
    {
        private string conStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ActionResult Index(int id)
        {
            int currentUserId = id; // simulate logged-in user
            ViewBag.CurrentUserId = currentUserId;

            var users = new List<User>();
            using (var con = new SqlConnection(conStr))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * FROM Users WHERE UserId <> @id", con);
                cmd.Parameters.AddWithValue("@id", currentUserId);
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    users.Add(new User
                    {
                        UserId = (int)rdr["UserId"],
                        UserName = rdr["UserName"].ToString()
                    });
                }
            }

            return View(users);
        }

        [HttpGet]
        public JsonResult GetChatHistory(int currentUserId, int selectedUserId)
        {
            var messages = new List<object>();

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                con.Open();
                string query = @"
            SELECT SenderId, ReceiverId, MessageText, Timestamp
            FROM Messages
            WHERE (SenderId = @user1 AND ReceiverId = @user2)
               OR (SenderId = @user2 AND ReceiverId = @user1)
            ORDER BY Timestamp";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@user1", currentUserId);
                cmd.Parameters.AddWithValue("@user2", selectedUserId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    messages.Add(new
                    {
                        SenderId = (int)reader["SenderId"],
                        Message = reader["MessageText"].ToString(),
                        CreatedAt = ((DateTime)reader["Timestamp"]).ToString("g")
                    });
                }
            }

            return Json(messages, JsonRequestBehavior.AllowGet);
        }

    }
}