using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTac.Net.Client.Core;
using GameTac.Net.Client.Protocols;

namespace GameTac.Net.Client.Services.Sys
{
    public class HeartBeat : MonoBehaviour
    {
        //是否启用心跳
        public bool isUsePing = true;
        //心跳间隔时间
        public int pingInterval = 30;
        //上一次发送PING的时间
        [SerializeField]
        float lastPingTime = 0;
        //上一次收到PONG的时间
        [SerializeField]
        float lastPongTime = 0;

        private void Awake()
        {
            //上一次发送PING的时间
            lastPingTime = Time.time;
            //上一次收到PONG的时间
            lastPongTime = Time.time;
        }

        // Start is called before the first frame update
        void Start()
        {
            Dashboard.AddProtEvent<ProtPong>(OnProtPong);
        }

        // Update is called once per frame
        void Update()
        {
            PingUpdate();
        }

        //发送PING协议
        private void PingUpdate()
        {
            //是否启用
            if (!isUsePing)
            {
                return;
            }
            //发送PING
            if (Time.time - lastPingTime > pingInterval)
            {
                ProtPing msgPing = new ProtPing();
                ProtPong msgPong = new ProtPong();
                Substruction.Instance.Send(msgPing);
                Substruction.Instance.Send(msgPong);
                lastPingTime = Time.time;
            }
            //检测PONG时间
            if (Time.time - lastPongTime > pingInterval * 4)
            {
                Substruction.Instance.Close();
            }
        }

        //监听PONG协议
        private string OnProtPong(ProtocolBase protBase)
        {
            lastPongTime = Time.time;
            Debug.Log("set pong time");

            return "pong";
        }
    }
}
