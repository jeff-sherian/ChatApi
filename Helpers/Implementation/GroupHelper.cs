using ChatApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace ChatApi.Helpers
{
    public class GroupHelper : IGroupHelper
    {
        public List<Group> GroupList { get; set; }

        public GroupHelper()
        {
            // These would eventually be managed in a db versus in-memory
            this.GroupList = new List<Group>();
        }

        public List<string> GetUserExistingGroups(User user)
        {
            List<string> groupNames = new List<string>();

            foreach (Group group in this.GroupList)
            {
                if (group.Connections.Any(i => user.Connections.Contains(i)))
                {
                    groupNames.Add(group.Name);
                }
            }

            return groupNames;
        }

        public void AddGroupConnection(string groupName, string connectionId, out bool isNewGroup)
        {
            Group group = this.GroupList.Find(i => i.Name == groupName);

            if (group == null)
            {
                isNewGroup = true;

                this.GroupList.Add(
                    new Group
                    {
                        Name = groupName,
                        Connections = new List<string>() { connectionId }
                    }
                );
            }
            else
            {
                isNewGroup = false;

                if (!group.Connections.Contains(connectionId))
                {
                    group.Connections.Add(connectionId);
                }
            }
        }

        public void RemoveGroupConnection(string groupName, string connectionId)
        {
            Group group = this.GroupList.Find(i => i.Name == groupName);

            if (group != null)
            {
                group.Connections.Remove(connectionId);

                if (group.Connections.Count == 0)
                {
                    this.GroupList.RemoveAll(i => i.Name == groupName);
                }
            }
        }

        public void RemoveConnectionFromGroups(string connectionId, out List<string> removedGroups)
        {
            removedGroups = new List<string>();

            foreach (Group group in this.GroupList)
            {
                group.Connections.Remove(connectionId);
                if (group.Connections.Count == 0) removedGroups.Add(group.Name);
            }

            foreach (string groupName in removedGroups)
            {
                this.GroupList.RemoveAll(i => i.Name == groupName);
            }
        }

        public bool GroupExists(string groupName)
        {
            return this.GroupList.Exists(i => i.Name == groupName);
        }
    }
}
