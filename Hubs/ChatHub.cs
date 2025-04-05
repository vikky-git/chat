using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private static Dictionary<int, string> userConnections = new Dictionary<int, string>();

        public void Connect(int userId)
        {
            userConnections[userId] = Context.ConnectionId;
            System.Diagnostics.Debug.WriteLine($"User {userId} connected with ID {Context.ConnectionId}");

        }

        public void SendMessage(int senderId, int receiverId, string message)
        {
            SaveMessage(senderId, receiverId, message);

            // Send to receiver
            if (userConnections.ContainsKey(receiverId))
            {
                var receiverConn = userConnections[receiverId];
                Clients.Client(receiverConn).receiveMessage(senderId, message);
            }

            //// Also send back to sender (echo)
            //if (userConnections.ContainsKey(senderId))
            //{
            //    var senderConn = userConnections[senderId];
            //    Clients.Client(senderConn).receiveMessage(senderId, message);
            //}
        }

        private void SaveMessage(int senderId, int receiverId, string message)
        {
            var conStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                var cmd = new SqlCommand("INSERT INTO Messages (SenderId, ReceiverId, MessageText) VALUES (@s, @r, @m)", con);
                cmd.Parameters.AddWithValue("@s", senderId);
                cmd.Parameters.AddWithValue("@r", receiverId);
                cmd.Parameters.AddWithValue("@m", message);
                cmd.ExecuteNonQuery();
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var user = userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!user.Equals(default(KeyValuePair<int, string>)))
            {
                userConnections.Remove(user.Key);
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}