using System;

namespace SignalServer.Users
{
    public interface IUserManager
    {
        UserInfo GetUserInfo(string username);

        UserInfo CheckUserCredentials(string username, string password);
    }
}
