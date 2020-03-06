using ChatApi.Models;
using System.Collections.Generic;

namespace ChatApi.Helpers
{
    public class UserHelper : IUserHelper
    {
        public List<User> UserList { get; set; }

        public UserHelper()
        {
            // These would eventually be managed in a db versus in-memory
            this.UserList = new List<User>();
        }

        public void AddUserConnection(string userName, string connectionId, out bool isNewUser)
        {
            User user = this.UserList.Find(i => i.Name == userName);

            if (user == null)
            {
                isNewUser = true;

                this.UserList.Add(
                    new User
                    {
                        Name = userName,
                        Connections = new List<string>() { connectionId }
                    }
                );
            }
            else
            {
                isNewUser = false;

                if (!user.Connections.Contains(connectionId))
                {
                    user.Connections.Add(connectionId);
                }
            }
        }

        public void RemoveUserConnection(string userName, string connectionId, out bool isLastConnection)
        {
            isLastConnection = false;
            User user = this.UserList.Find(i => i.Name == userName);

            if (user != null)
            {
                user.Connections.Remove(connectionId);

                if (user.Connections.Count == 0)
                {
                    isLastConnection = true;
                    this.UserList.RemoveAll(i => i.Name == userName);
                }
            }
        }

        public User GetUserByName(string userName)
        {
            return this.UserList.Find(i => i.Name == userName);
        }
    }
}
