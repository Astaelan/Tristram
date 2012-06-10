using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Tristram.Lobby.Network;
using Tristram.Shared;
using Tristram.Shared.Reflection;

namespace Tristram.Lobby
{
    internal static class Program
    {
        private static bool sRunning = true;
        private static Socket sClientListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static ConcurrentQueue<Client> sConnectingClients = new ConcurrentQueue<Client>();
        private static ConcurrentQueue<Client> sDisconnectingClients = new ConcurrentQueue<Client>();
        private static List<Client> sConnectedClients = new List<Client>();
        private static Dictionary<uint, ClientServiceAttribute> sClientServices = new Dictionary<uint, ClientServiceAttribute>();
        private static Dictionary<uint, uint> sClientServiceHashes = new Dictionary<uint, uint>();

        private static ConcurrentQueue<Action> sCallbacks = new ConcurrentQueue<Action>();

        private static void Main(string[] pArgs)
        {
            if (Startup(pArgs)) Run();
            Shutdown();
        }

        private static bool Startup(string[] pArgs)
        {
            Stopwatch startupTimer = Stopwatch.StartNew();
            Console.SetWindowSize(120, 40);
            Logger.OnOutput += (o) => Console.WriteLine(o);
            Config.Load();
            Logger.WriteLine(ELogLevel.Info, "Starting");

            Reflector.FindAllClasses<ClientServiceAttribute>().ForEach(s => 
            {
                sClientServices[s.ServiceId] = s;
                sClientServiceHashes[s.Hash] = s.ServiceId;
            });
            Reflector.FindAllMethods<ClientServiceMethodAttribute, ClientServiceMethodDelegate>().ForEach(t =>
            {
                t.Item1.Method = t.Item2;
                if (sClientServices.ContainsKey(t.Item1.ServiceId))
                {
                    sClientServices[t.Item1.ServiceId].AddMethod(t.Item1.MethodId, t.Item1);
                }
            });
            foreach (KeyValuePair<uint, ClientServiceAttribute> kv in sClientServices)
            {
                Logger.WriteLine(ELogLevel.Info, "Loaded Client Service {0} - {1}, {2} Registered Methods", kv.Key, ClientServiceIds.ToString(kv.Key), kv.Value.MethodCount);
            }

            sClientListener.Bind(new IPEndPoint(IPAddress.Any, Config.Instance.ClientPort));
            sClientListener.Listen(Config.Instance.ClientBacklog);
            Logger.WriteLine(ELogLevel.Info, "Started Client Services");
            BeginClientAccept(null);

            startupTimer.Stop();
            Logger.WriteLine(ELogLevel.Info, "Started ({0})", startupTimer.Elapsed);
            return true;
        }

        private static void Shutdown()
        {
            sClientListener.Close();
        }

        public static void AddCallback(Action pCallback) { sCallbacks.Enqueue(pCallback); }

        private static void BeginClientAccept(SocketAsyncEventArgs pArgs)
        {
            if (pArgs == null)
            {
                pArgs = new SocketAsyncEventArgs();
                pArgs.Completed += (s, a) => EndClientAccept(a);
            }
            pArgs.AcceptSocket = null;
            if (!sClientListener.AcceptAsync(pArgs)) EndClientAccept(pArgs);
        }

        private static void EndClientAccept(SocketAsyncEventArgs pArgs)
        {
            if (pArgs.SocketError == SocketError.Success) sConnectingClients.Enqueue(new Client(pArgs.AcceptSocket));
            BeginClientAccept(pArgs);
        }

        internal static void DisconnectClient(Client pClient) { sDisconnectingClients.Enqueue(pClient); }
        private static void Run()
        {
            while (sRunning) { if (Pulse()) Thread.Sleep(1); }
            foreach (Client connected in sConnectedClients) connected.Disconnect();
            while (Pulse()) ;
        }

        private static bool Pulse()
        {
            bool idle = true;
            Client client = null;
            Action callback = null;
            while (sConnectingClients.TryDequeue(out client)) { idle = false; sConnectedClients.Add(client); client.Connected(); }
            while (sCallbacks.TryDequeue(out callback)) { idle = false; callback(); }
            while (sDisconnectingClients.TryDequeue(out client)) { idle = false; sConnectedClients.Remove(client); client.Disconnected(); }
            AccountCache.CheckExpired();
            return idle;
        }

        internal static bool GetClientServiceId(uint pHash, out uint pServiceId) { return sClientServiceHashes.TryGetValue(pHash, out pServiceId); }
        internal static bool GetClientServiceMethod(uint pServiceId, uint pMethodId, out ClientServiceMethodAttribute pMethod)
        {
            pMethod = null;
            ClientServiceAttribute service = null;
            if (!sClientServices.TryGetValue(pServiceId, out service)) return false;
            return service.TryGetMethod(pMethodId, out pMethod);
        }
    }
}
