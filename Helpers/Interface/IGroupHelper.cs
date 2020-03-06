using ChatApi.Models;
using System.Collections.Generic;

namespace ChatApi.Helpers
{
    public interface IGroupHelper
    {
        List<string> GetUserExistingGroups(User user);
        void AddGroupConnection(string groupName, string connectionId, out bool isNewGroup);
        void RemoveGroupConnection(string groupName, string connectionId);
        void RemoveConnectionFromGroups(string connectionId, out List<string> removedGroups);
        bool GroupExists(string groupName);
    }
}
