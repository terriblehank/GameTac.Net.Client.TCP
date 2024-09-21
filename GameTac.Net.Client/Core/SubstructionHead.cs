using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;

namespace GameTac.Net.Client.Core
{
    public partial class Substruction
    {
        private readonly static Substruction _instance = new();
        public static Substruction Instance
        {
            get
            {
                return _instance;
            }
        }

        private Substruction() { }

        //定义套接字
        Socket socket;
        //接收缓冲区
        ByteArray readBuff;
        //写入队列
        Queue<ByteArray> writeQueue;
        //是否正在连接
        bool isConnecting = false;
        //是否正在关闭
        bool isClosing = false;
        //是否已加载协议
        public bool ProtLoaded { get; private set; }
        //消息列表
        List<ProtocolBase> msgList = new List<ProtocolBase>();
        //消息列表长度
        int msgCount = 0;
        //每一次Update处理的消息量
        readonly int MAX_MESSAGE_FIRE = 10;


        //Msg名/类型对照表 用于在收到消息时，通过名称实例化对象
        private readonly Dictionary<string, Type> protTypeTable = new Dictionary<string, Type>();
        //Handle委托
        public delegate string ProtHandle(ProtocolBase body);
        //Handle注册表
        private readonly Dictionary<string, ProtHandle> protEventTable = new Dictionary<string, ProtHandle>();
    }
}