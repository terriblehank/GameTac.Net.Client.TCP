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
		public void LoadProtocols()
		{
			if (ProtLoaded)
			{
				PrintE("尝试重复加载协议！");
				return;
			}

			ProtocolBase.Init();

			ProtEvent.Init();

			ProtLoaded = true;
		}
		public void RegisterEvent(Type protType, ProtHandle protEvent)
		{
			string name = protType.Name;
			if (!protEventTable.TryAdd(name, protEvent)) PrintE("正在向ProtEventTable中重复添加Prot！name= " + name);
		}

		public void AddEvent(Type protType, ProtHandle protEvent)
		{
			string name = protType.Name;
			if (protEventTable.TryGetValue(name, out ProtHandle value))
			{
				protEventTable[name] = value == ProtEvent.NullEvent ? protEvent : value + protEvent;
			}
			else
			{
				PrintW($"尝试添加ProtEvent时未能在表中找到对应的记录 {name}");
			}
		}

		public void RemoveEvent(Type protType, ProtHandle protEvent)
		{
			string name = protType.Name;

			if (protEventTable.TryGetValue(name, out ProtHandle value))
			{
				protEventTable[name] = value - protEvent ?? ProtEvent.NullEvent;
			}
			else
			{
				PrintW($"尝试移除ProtEvent时未能在表中找到对应的记录 {name}");
			}
		}

		public void RegisterProtType(string name, Type type)
		{
			if (!protTypeTable.TryAdd(name, type)) PrintE("正在向ProtTypeTable中重复添加Prot！name= " + name);
		}

		public Type GetProtType(string name)
		{
			protTypeTable.TryGetValue(name, out Type value);
			return value;
		}
	}
}
