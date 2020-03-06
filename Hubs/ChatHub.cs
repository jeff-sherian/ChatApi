using ChatApi.Helpers;
using ChatApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        private IGroupHelper GroupHelper { get; set; }
        private IUserHelper UserHelper { get; set; }
        private Identity Identity { get; set; }

        public ChatHub(IHttpContextAccessor httpContextAccessor, IGroupHelper groupHelper, IUserHelper userHelper)
        {
            this.GroupHelper = groupHelper;
            this.UserHelper = userHelper;

            // The following would eventually get replaced with built-in JWT Authentication in Startup.cs
            object identity;
            httpContextAccessor.HttpContext.Items.TryGetValue("Identity", out identity);

            this.Identity = JsonSerializer.Deserialize<Identity>(JsonSerializer.Serialize(identity));
        }

        public async override Task OnConnectedAsync()
        {
            // Add the connection to all existing groups associated with the user
            User user = this.UserHelper.GetUserByName(this.Identity.UserName);
            if (user != null)
            {
                foreach (string groupName in this.GroupHelper.GetUserExistingGroups(user))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                }
            }

            // Add the connection to the current user.  This will also add the user if they do not already exist
            bool isNewUser;
            this.UserHelper.AddUserConnection(this.Identity.UserName, Context.ConnectionId, out isNewUser);

            // Only send a broadcast if the user was not already connected
            if (isNewUser)
            {
                await Clients.All.SendAsync(
                    "UserAdded",
                    JsonSerializer.Serialize(
                        new UserAddedBroadcast(this.Identity.UserName)
                    )
                );
            }

            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            // Remove the connection from the current user.  This will also remove the user if there are no more connections
            bool isLastConnection;
            this.UserHelper.RemoveUserConnection(this.Identity.UserName, Context.ConnectionId, out isLastConnection);

            // Remove the connection from all associated groups.  This will also remove the group if there are no more connections
            List<string> removedGroups;
            this.GroupHelper.RemoveConnectionFromGroups(Context.ConnectionId, out removedGroups);

            // Send out a broadcast for any removed groups
            foreach (string groupName in removedGroups)
            {
                await BroadcastRemovedGroup(groupName);
            }

            // Only send a broadcast if all user connections have been removed
            if (isLastConnection)
            {
                await Clients.All.SendAsync(
                    "UserRemoved",
                    JsonSerializer.Serialize(
                        new UserRemovedBroadcast(this.Identity.UserName)
                    )
                );
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task BroadcastRemovedGroup(string groupName)
        {
            await Clients.All.SendAsync(
                "GroupRemoved",
                JsonSerializer.Serialize(
                    new GroupRemovedBroadcast(this.Identity.UserName, groupName)
                )
            );
        }

        public async Task AddToGroup(string groupName)
        {
            // Add the connection to the specified group
            // Send an appropriate broadcast based on whether it is a new group

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            bool isNewGroup;
            this.GroupHelper.AddGroupConnection(groupName, Context.ConnectionId, out isNewGroup);

            if (isNewGroup)
            {
                await Clients.All.SendAsync(
                    "GroupAdded",
                    JsonSerializer.Serialize(
                        new GroupAddedBroadcast(this.Identity.UserName, groupName)
                    )
                );
            }
            else
            {
                await Clients.Group(groupName).SendAsync(
                    "GroupUserAdded",
                    JsonSerializer.Serialize(
                        new GroupUserAddedBroadcast(this.Identity.UserName, groupName)
                    )
                );
            }
        }

        public async Task RemoveFromGroup(string groupName)
        {
            // Remove all connections for the current user from the specified group
            // Send an appropriate broadcast based on whether the group is being removed

            User user = this.UserHelper.GetUserByName(this.Identity.UserName);

            foreach(string connectionId in user.Connections)
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupName);

                // Removing the connection from the group will also remove the group if it has no more connections
                this.GroupHelper.RemoveGroupConnection(groupName, connectionId);
            }            

            if (this.GroupHelper.GroupExists(groupName))
            {
                await Clients.Group(groupName).SendAsync(
                    "GroupUserRemoved",
                    JsonSerializer.Serialize(
                        new GroupUserRemovedBroadcast(this.Identity.UserName, groupName)
                    )
                );
            }
            else
            {
                await BroadcastRemovedGroup(groupName);
            }
        }

        public async Task SendPublicMessage(string content)
        {
            await Clients.All.SendAsync(
                "PublicMessageReceived",
                JsonSerializer.Serialize(
                    new PublicMessageReceivedBroadcast(this.Identity.UserName, content)
                )
            );
        }

        public async Task SendGroupMessage(string groupName, string content)
        {
            await Clients.Group(groupName).SendAsync(
                "GroupMessageReceived",
                JsonSerializer.Serialize(
                    new GroupMessageReceivedBroadcast(this.Identity.UserName, groupName, content)
                )
            );
        }

        public async Task SendPrivateMessage(string userName, string content)
        {
            // Broadcast the message to all connections for the sender and recipient

            User recipient = this.UserHelper.GetUserByName(userName);

            if (recipient != null)
            {
                User user = this.UserHelper.GetUserByName(this.Identity.UserName);

                foreach (string connectionId in recipient.Connections)
                {
                    await Clients.Client(connectionId).SendAsync(
                        "PrivateMessageReceived",
                        JsonSerializer.Serialize(
                            new PrivateMessageReceivedBroadcast(user.Name, recipient.Name, content)
                        )
                    );
                }

                foreach (string connectionId in user.Connections)
                {
                    await Clients.Client(connectionId).SendAsync(
                        "PrivateMessageReceived",
                        JsonSerializer.Serialize(
                            new PrivateMessageReceivedBroadcast(user.Name, recipient.Name, content)
                        )
                    );
                }
            }
        }
    }
}
