using System;

namespace ChatApi.Models
{
    public class Broadcast
    {
        public Broadcast(string sender)
        {
            this.UtcTimestamp = DateTime.UtcNow;
            this.Sender = sender;
        }

        public DateTime UtcTimestamp { get; set; }
        public string Sender { get; set; }
    }

    public class GroupBroadcast : Broadcast
    {
        public GroupBroadcast(string sender, string groupName) : base (sender)
        {
            this.GroupName = groupName;
        }

        public string GroupName { get; set; }
    }

    public class MessageBroadcast : Broadcast
    {
        public MessageBroadcast(string sender, string content) : base(sender)
        {
            this.Content = content;
        }

        public string Content { get; set; }
    }

    public class UserAddedBroadcast : Broadcast
    {
        public UserAddedBroadcast(string sender) : base(sender)
        {
        }
    }

    public class UserRemovedBroadcast : Broadcast
    {
        public UserRemovedBroadcast(string sender) : base(sender)
        {
        }
    }

    public class GroupAddedBroadcast : GroupBroadcast
    {
        public GroupAddedBroadcast(string sender, string groupName) : base(sender, groupName)
        {
        }
    }

    public class GroupUserAddedBroadcast : GroupBroadcast
    {
        public GroupUserAddedBroadcast(string sender, string groupName) : base(sender, groupName)
        {
        }
    }

    public class GroupUserRemovedBroadcast : GroupBroadcast
    {
        public GroupUserRemovedBroadcast(string sender, string groupName) : base(sender, groupName)
        {
        }
    }

    public class GroupRemovedBroadcast : GroupBroadcast
    {
        public GroupRemovedBroadcast(string sender, string groupName) : base(sender, groupName)
        {
        }
    }

    public class PublicMessageReceivedBroadcast : MessageBroadcast
    {
        public PublicMessageReceivedBroadcast(string sender, string content) : base(sender, content)
        {
        }
    }

    public class GroupMessageReceivedBroadcast : MessageBroadcast
    {
        public GroupMessageReceivedBroadcast(string sender, string groupName, string content) : base(sender, content)
        {
            this.GroupName = groupName;
        }

        public string GroupName { get; set; }
    }

    public class PrivateMessageReceivedBroadcast : MessageBroadcast
    {
        public PrivateMessageReceivedBroadcast(string sender, string recipient, string content) : base(sender, content)
        {
            this.Recipient = recipient;
        }

        public string Recipient { get; set; }
    }
}
