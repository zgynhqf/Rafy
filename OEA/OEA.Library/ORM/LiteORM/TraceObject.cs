using System;
using System.Data;
using System.Diagnostics;

namespace OEA.ORM
{
	public class TraceObject
	{
		public static readonly TraceObject Instance = new TraceObject();
		
		private readonly LiteSwitch liteSwitch;
		private TraceListener listener;
		
		private TraceObject()
		{
			liteSwitch = new LiteSwitch();
			if (liteSwitch.Attributes.ContainsKey("listener"))
			{
				string listenerName = liteSwitch.Attributes["listener"];
				foreach (TraceListener tl in Trace.Listeners)
				{
					if (tl.Name == listenerName)
					{
						listener = tl;
						break;
					}
				}
				if (listener != null && liteSwitch.Attributes.ContainsKey("dedicated"))
				{
					string dedicated = liteSwitch.Attributes["dedicated"];
					if (dedicated == "true")
						Trace.Listeners.Remove(listener);
				}
			}
		}
		
		public void TraceCommand(IDbCommand cmd)
		{
			if (!Enabled)
				return;
			
			if (cmd == null)
			{
				TraceMessage("TraceObject.TraceCommand was given a null command");
				return;
			};
			
			lock (this)
			{
				TraceMessage(cmd.CommandText);
				if (cmd.Parameters.Count > 0)
				{
					TraceMessage("(");
					Indent();
					string format = "{0}:{1}:{2}:{3}";
					object[] values = new object[4];
					foreach (IDbDataParameter p in cmd.Parameters)
					{
						values[0] = p.ParameterName;
						values[1] = p.DbType.ToString();
						values[2] = p.Direction.ToString();
						values[3] = (p.Value == null || p.Value == DBNull.Value) ? "null" : p.Value.ToString();
						TraceMessage(string.Format(format, values));
					}
					Unindent();
					TraceMessage(")");
				}
			}
		}
		
		public void TraceMessage(string message)
		{
			if (Enabled)
			{
				if (listener != null)
				{
					listener.WriteLine(message);
					listener.Flush();
				}
				else
					Trace.WriteLine(message);
			}
		}
		
		public void Indent()
		{
			if (Enabled)
			{
				if (listener != null)
					listener.IndentLevel = listener.IndentLevel + 1;
				else
					Trace.Indent();
			}
		}
		
		public void Unindent()
		{
			if (Enabled)
			{
				if (listener != null)
					listener.IndentLevel = listener.IndentLevel - 1;
				else
					Trace.Unindent();
			}
		}
		
		public TraceListener Listener
		{
			set { listener = value; }
		}
		
		public bool Enabled
		{
			get { return liteSwitch.Enabled; }
			set { liteSwitch.Enabled = value; }
		}
		
		//
		// Private class.
		//
		private class LiteSwitch : BooleanSwitch
		{
			public LiteSwitch() : base("OEA.ORM", "OEA.ORM")
			{}
			
			override protected string[] GetSupportedAttributes()
			{
				return new string[] { "listener", "dedicated" };
			}
		}
	}
}
