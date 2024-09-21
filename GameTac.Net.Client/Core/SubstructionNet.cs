using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Linq;
using static GameTac.Net.Client.Util.Print;
using UnityEditor;

namespace GameTac.Net.Client.Core
{
	public partial class Substruction
	{
		//连接
		public void Connect(string ip, int port)
		{
			if (!ProtLoaded)
			{
				PrintE("Connect fail, Protocol 尚未被加载，请先执行LoadProtocols！");
				return;
			}

			//状态判断
			if (socket != null && socket.Connected)
			{
				PrintE("Connect fail, already connected!");
				return;
			}
			if (isConnecting)
			{
				PrintE("Connect fail, isConnecting");
				return;
			}
			//初始化成员
			InitState();
			//参数设置
			socket.NoDelay = true;
			//Connect
			isConnecting = true;
			socket.BeginConnect(ip, port, ConnectCallback, socket);
		}

		//初始化状态
		private void InitState()
		{
			//Socket
			socket = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			//接收缓冲区
			readBuff = new ByteArray();
			//写入队列
			writeQueue = new Queue<ByteArray>();
			//是否正在连接
			isConnecting = false;
			//是否正在关闭
			isClosing = false;
			//消息列表
			msgList = new List<ProtocolBase>();
			//消息列表长度
			msgCount = 0;
		}

		//Connect回调
		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				socket.EndConnect(ar);
				PrintM("Socket Connect Succ ");
				isConnecting = false;
				//开始接收
				socket.BeginReceive(readBuff.bytes, readBuff.writeIdx,
												readBuff.remain, 0, ReceiveCallback, socket);

			}
			catch (SocketException ex)
			{
				PrintE("Socket Connect fail " + ex.ToString());
				isConnecting = false;
			}
		}

		//关闭连接
		public void Close()
		{
			//状态判断
			if (socket == null || !socket.Connected)
			{
				return;
			}
			if (isConnecting)
			{
				return;
			}
			//还有数据在发送
			if (writeQueue.Count > 0)
			{
				isClosing = true;
			}
			//没有数据在发送
			else
			{
				socket.Close();
			}
		}

		//发送数据
		public void Send(ProtocolBase msg)
		{
			//状态判断
			if (socket == null || !socket.Connected)
			{
				return;
			}
			if (isConnecting)
			{
				return;
			}
			if (isClosing)
			{
				return;
			}
			//数据编码
			byte[] nameBytes = ProtocolBase.EncodeName(msg);
			byte[] bodyBytes = ProtocolBase.Encode(msg);
			int len = nameBytes.Length + bodyBytes.Length;
			byte[] sendBytes = new byte[2 + len];
			//组装长度
			sendBytes[0] = (byte)(len % 256);
			sendBytes[1] = (byte)(len / 256);
			//组装名字
			Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
			//组装消息体
			Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
			//写入队列
			ByteArray ba = new ByteArray(sendBytes);
			int count = 0;  //writeQueue的长度
			lock (writeQueue)
			{
				writeQueue.Enqueue(ba);
				count = writeQueue.Count;
			}
			//send
			if (count == 1)
			{
				socket.BeginSend(sendBytes, 0, sendBytes.Length,
					0, SendCallback, socket);
			}
		}

		//Send回调
		public void SendCallback(IAsyncResult ar)
		{

			//获取state、EndSend的处理
			Socket socket = (Socket)ar.AsyncState;
			//状态判断
			if (socket == null || !socket.Connected)
			{
				return;
			}
			//EndSend
			int count = socket.EndSend(ar);
			//获取写入队列第一条数据            
			ByteArray ba;
			lock (writeQueue)
			{
				ba = writeQueue.First();
			}
			//完整发送
			ba.readIdx += count;
			if (ba.length == 0)
			{
				lock (writeQueue)
				{
					writeQueue.Dequeue();
					ba = writeQueue.First();
				}
			}
			//继续发送
			if (ba != null)
			{
				socket.BeginSend(ba.bytes, ba.readIdx, ba.length,
					0, SendCallback, socket);
			}
			//正在关闭
			else if (isClosing)
			{
				socket.Close();
			}
		}

		//Receive回调
		public void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				Socket socket = (Socket)ar.AsyncState;
				//获取接收数据长度
				int count = socket.EndReceive(ar);
				readBuff.writeIdx += count;
				//处理二进制消息
				OnReceiveData();
				//继续接收数据
				if (readBuff.remain < 8)
				{
					readBuff.MoveBytes();
					readBuff.ReSize(readBuff.length * 2);
				}
				socket.BeginReceive(readBuff.bytes, readBuff.writeIdx,
						readBuff.remain, 0, ReceiveCallback, socket);
			}
			catch (SocketException ex)
			{
				PrintE("Socket Receive fail" + ex.ToString());
			}
		}

		//数据处理
		public void OnReceiveData()
		{
			//消息长度
			if (readBuff.length <= 2)
			{
				return;
			}
			//获取消息体长度
			int readIdx = readBuff.readIdx;
			byte[] bytes = readBuff.bytes;
			Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
			if (readBuff.length < bodyLength)
				return;
			readBuff.readIdx += 2;
			//解析协议名
			int nameCount = 0;
			string protoName = ProtocolBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
			if (protoName == "")
			{
				PrintE("OnReceiveData MsgBase.DecodeName fail");
				return;
			}
			readBuff.readIdx += nameCount;
			//解析协议体
			int bodyCount = bodyLength - nameCount;
			ProtocolBase msgBase = ProtocolBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
			readBuff.readIdx += bodyCount;
			readBuff.CheckAndMoveBytes();
			//添加到消息队列
			lock (msgList)
			{
				msgList.Add(msgBase);
				msgCount++;
			}
			//继续读取消息
			if (readBuff.length > 2)
			{
				OnReceiveData();
			}
		}

		//分发消息
		private void FireMsg(ProtocolBase msg)
		{
			string msgName = msg.GetType().Name;
			protEventTable.TryGetValue(msgName, out ProtHandle @event);
			if (@event is not null)
			{
				@event.Invoke(msg);
			}
			else
			{
				PrintW($"收到 {msgName} 但对应的事件回调为空！");
			}
		}

		//Update
		public void Update()
		{
			MsgUpdate();
		}

		//更新消息
		public void MsgUpdate()
		{
			//初步判断，提升效率
			if (msgCount == 0)
			{
				return;
			}
			//重复处理消息
			for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
			{
				//获取第一条消息
				ProtocolBase msgBase = null;
				lock (msgList)
				{
					if (msgList.Count > 0)
					{
						msgBase = msgList[0];
						msgList.RemoveAt(0);
						msgCount--;
					}
				}
				//分发消息
				if (msgBase != null)
				{
					FireMsg(msgBase);
				}
				//没有消息了
				else
				{
					break;
				}
			}
		}
	}
}
