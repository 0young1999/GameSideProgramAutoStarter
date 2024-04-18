using System.Diagnostics;
using System.Text;

namespace GameSideProgramAutoStarter.Cs
{
	internal class csLog
	{
		private static csLog instance;
		public static csLog GetInstance()
		{
			if (instance == null) instance = new csLog();
			return instance;
		}
		private csLog() { }

		public void ErrorLog(string className, string StackTrace, string msg)
		{
			lock (this)
			{
				if (Directory.Exists("log") == false)
				{
					Directory.CreateDirectory("log");
				}
				if (Directory.Exists("log\\ErrorLog") == false)
				{
					Directory.CreateDirectory("log\\ErrorLog");
				}

				string fileName = "log\\ErrorLog\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
				StringBuilder sb = new StringBuilder();

				// start
				sb.AppendLine("-----STX-----");
				// Time
				sb.Append("[").Append(DateTime.Now.ToString("HH:mm:ss:fff")).AppendLine("]");
				// ClassName
				sb.AppendLine(className);
				// ST
				sb.AppendLine(StackTrace);
				// message
				sb.AppendLine(msg);
				// end
				sb.AppendLine("-----ETX-----");

				File.AppendAllText(fileName, sb.ToString());
			}
		}

		public void ErrorLog(string className, Exception e)
		{
			lock (this)
			{
				ErrorLog(className, e.StackTrace ?? "No StackTrace", e.Message);
			}
		}

		private string ProcessStartTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

		public void ProcessLog(string name, bool isAlive)
		{
			lock (this)
			{
				if (Directory.Exists("log") == false)
				{
					Directory.CreateDirectory("log");
				}
				if (Directory.Exists("log\\ProcessLog") == false)
				{
					Directory.CreateDirectory("log\\ProcessLog");
				}

				string fileName = "log\\ProcessLog\\" + ProcessStartTime + ".txt";
				StringBuilder sb = new StringBuilder();

				sb.Append("[").Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff")).Append("]");
				sb.Append(isAlive ? "1" : "0").Append(" : ").AppendLine(name);

				File.AppendAllText(fileName, sb.ToString());
			}
		}
	}
}
