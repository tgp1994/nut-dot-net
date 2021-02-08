﻿using NUTDotNetServer;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Testing
{
    /// <summary>
    /// Creates a mockup server and connection only once that is available for all tests in this class.
    /// </summary>
    public class ServerFixture : IDisposable
    {
        public Server testServer { get; private set; }
        private Task serverTask;
        public TcpClient testClient { get; private set; }

        public ServerFixture()
        {
            testServer = new Server();
            serverTask = new Task(() => testServer.BeginListening());
            serverTask.Start();
            testClient = new TcpClient("localhost", testServer.ListenPort);
        }

        public void Dispose()
        {
            Stream baseStream = testClient.GetStream();
            StreamReader sr = new StreamReader(baseStream);
            StreamWriter sw = new StreamWriter(baseStream);

            sw.WriteLine("LOGOUT");
            sw.Flush();
            string result = sr.ReadLine();
            Assert.Equal("OK Goodbye", result);

            sr.Close();
            sw.Close();
            testClient.Close();
            serverTask.Wait();
        }
    }

    public class ServerTests : IClassFixture<ServerFixture>
    {
        // Make the public fixture variables accessible
        ServerFixture serverFixture;

        public ServerTests(ServerFixture fixture)
        {
            serverFixture = fixture;
        }

        [Fact]
        public void GetServerVersion()
        {
            Stream baseStream = serverFixture.testClient.GetStream();
            StreamReader sr = new StreamReader(baseStream);
            StreamWriter sw = new StreamWriter(baseStream);

            sw.WriteLine("VER");
            sw.Flush();
            string result = sr.ReadLine();
            Assert.Equal(serverFixture.testServer.ServerVersion, result);
        }

        [Fact]
        public void GetNetworkProtocolVersion()
        {
            Stream baseStream = serverFixture.testClient.GetStream();
            StreamReader sr = new StreamReader(baseStream);
            StreamWriter sw = new StreamWriter(baseStream);

            sw.WriteLine("NETVER");
            sw.Flush();
            string result = sr.ReadLine();
            Assert.Equal(Server.NETVER, result);
        }

        [Fact]
        public void AttemptIncorrectCommand()
        {
            Stream baseStream = serverFixture.testClient.GetStream();
            StreamReader sr = new StreamReader(baseStream);
            StreamWriter sw = new StreamWriter(baseStream);

            sw.WriteLine("TRY UNKNOWN COMMAND");
            sw.Flush();
            string result = sr.ReadLine();
            Assert.Equal("UNKNOWN-COMMAND", result);
        }
    }
}