using ChatApi.Models;
using System.Collections.Generic;

namespace ChatApi.Helpers
{
    public interface IUserHelper
    {
        void AddUserConnection(string userName, string connectionId, out bool isNewUser);
        void RemoveUserConnection(string userName, string connectionId, out bool isLastConnection);
        User GetUserByName(string userName);
    }
}
