using System;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace GitCleanup
{
    public static class CredentialsFactory
    {
        public static CredentialsHandler CreateHandler(AuthType authType, string name, string password)
        {
            if (authType == AuthType.Ntlm)
            {
                return (url, fromUrl, types) => new DefaultCredentials();
            }
            else
            {
                if (name == null || password == null)
                {
                    throw new Exception("User name or password is not specified");
                }
                return (url, fromUrl, types) => new UsernamePasswordCredentials
                {
                    Username = name,
                    Password = password
                };
            }
        }
    }
}