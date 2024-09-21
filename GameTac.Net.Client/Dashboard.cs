using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Unity.VisualScripting;
using GameTac.Net.Client.Core;
using GameTac.Net.Client.Services;
using static GameTac.Net.Client.Util.Print;

namespace GameTac.Net.Client
{
    internal class Dashboard
    {
        private readonly static Dashboard _instance = new();
        internal static Dashboard Instance
        {
            get
            {
                return _instance;
            }
        }
        private Dashboard() { }

        private readonly Substruction state = Substruction.Instance;

        public void LoadProtocols()
        {
            Substruction.Instance.LoadProtocols();
        }

        public void Connect(string ip, int listenPort)
        {
            state.Connect(ip, listenPort);
        }

        public void Update()
        {
            state.Update();
        }
        
        public static bool AddProtEvent<T>(Substruction.ProtHandle @event) where T : ProtocolBase
        {
            if (!Substruction.Instance.ProtLoaded)
            {
                PrintE("尝试添加ProtEvent但协议还没有被加载！");
                return false;
            }

            ProtEvent.Add<T>(@event);
            return true;
        }

        public static bool RemoveProtEvent<T>(Substruction.ProtHandle @event) where T : ProtocolBase
        {
            if (!Substruction.Instance.ProtLoaded)
            {
                PrintE("尝试移除ProtEvent但协议还没有被加载！");
                return false;
            }
            ProtEvent.Remove<T>(@event);
            return true;
        }
    }
}


