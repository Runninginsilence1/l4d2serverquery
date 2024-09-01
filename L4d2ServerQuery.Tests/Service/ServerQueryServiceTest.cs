using System;
using System.Threading;
using JetBrains.Annotations;
using L4d2ServerQuery.Model;
using L4d2ServerQuery.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace L4d2ServerQuery.Tests.Service;

[TestClass]
[TestSubject(typeof(ServerQueryService))]
public class ServerQueryServiceTest
{

    [TestMethod]
    public void METHOD()
    {
        FavoriteServer favoriteServer = new FavoriteServer()
        {
            Host = "42.192.4.35",
            Port = 42300,
        };
        
        ServerInformation serverInformation = new ServerInformation(favoriteServer);

        for (int i = 0; i < 5; i++)
        {
            Thread.Sleep(1000);
            if (serverInformation.GetSteamQueryInformation() == null)
            {
                Console.WriteLine("Server is offline.");
            }
            else
            {
                Console.WriteLine($"Information {serverInformation.GetSteamQueryInformation().OnlinePlayers}:");

            }
        }
    }
}