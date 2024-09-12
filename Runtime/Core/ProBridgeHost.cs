using System;
using System.Threading;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;
using NetMQ.Monitoring;

namespace ProBridge
{
    [AddComponentMenu("ProBridge/Host")]
    public class ProBridgeHost : MonoBehaviour
    {
        public string addr = "127.0.0.1";
        public int port = 47778;

        [HideInInspector] public PushSocket pushSocket;

        public event EventHandler onSubscriberConnect;
        private NetMQMonitor monitor;

        private Thread monitoringThread;
        private bool shouldStopMonitoring = false;

        private void OnEnable()
        {
            AsyncIO.ForceDotNet.Force();
            pushSocket = new PushSocket();
            pushSocket.Bind($"tcp://{addr}:{port}");
            pushSocket.Options.Linger = new TimeSpan(0, 0, 1);
            
            monitor = new NetMQMonitor(pushSocket, $"inproc://monitor-{addr}:{port}", SocketEvents.All);
            monitor.Accepted += (s, e) => onSubscriberConnect?.Invoke(this, EventArgs.Empty);

            monitor.StartAsync();
        }


        private void OnDisable()
        {
            pushSocket.Close();
            pushSocket?.Dispose();

            monitor.Stop();
            monitor?.Dispose();
        }

        private void OnDestroy()
        {
            NetMQConfig.Cleanup(false); // Must be here to work more than once, and false to not block when there are unprocessed messages.
        }
    }
}