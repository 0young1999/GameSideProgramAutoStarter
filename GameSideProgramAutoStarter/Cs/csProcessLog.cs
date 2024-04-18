using System.Diagnostics;

namespace GameSideProgramAutoStarter.Cs
{
	public class csProcessLog
	{
		private static csProcessLog instance;
		public static csProcessLog GetInstance()
		{
			if (instance == null) instance = new csProcessLog();
			return instance;
		}
		private csProcessLog()
		{
			WDT = new Thread(WDTMethod);
			WDT.IsBackground = true;
			WDT.Start();
		}

		private csLog log = csLog.GetInstance();
		private frmAlarm alarm = frmAlarm.GetInstance();

		public class ProcessData
		{
			public string ProcessName = "";
			public bool isAlive = false;
		}

		private Thread WDT;
		public bool setAction = false;

		private List<ProcessData> pds = new List<ProcessData>();

		private void WDTMethod()
		{
			Thread.Sleep(100);

			while (true)
			{
				try
				{
					while (setAction)
					{
						foreach (ProcessData data in pds)
						{
							data.isAlive = false;
						}

						Process[] pss = Process.GetProcesses();
						// 살아나는거 확인
						foreach (Process p in pss)
						{
							int findIndex = pds.FindIndex(item => item.ProcessName.Equals(p.ProcessName));
							if (findIndex != -1)
							{
								pds[findIndex].isAlive = true;
							}
							else
							{
								pds.Add(new ProcessData()
								{
									ProcessName = p.ProcessName,
									isAlive = true,
								});
								log.ProcessLog(p.ProcessName, true);
								UpdateProcessEvent(p.ProcessName, true);
							}
						}

						// 죽은거 확인
						var CopyPds = pds.Where(item => item.isAlive == false);
						if (CopyPds != null)
						{
							foreach (ProcessData item in CopyPds)
							{
								log.ProcessLog(item.ProcessName, false);
								UpdateProcessEvent(item.ProcessName, false);
								pds.Remove(item);
								break;
							}
						}

						Thread.Sleep(1);
					}
					Thread.Sleep(10);
				}
				catch (Exception e)
				{
					log.ErrorLog(GetType().Name, e);
					alarm.ShowMSG("프로세스 모니터링에\n오류가 발생하였습니다.");
				}
			}
		}

		public EventHandler<ProcessEventArgs> ProcessEvent;
		public class ProcessEventArgs : EventArgs
		{
			public bool isAlive;
			public string? ProcessName;
		}
		public void UpdateProcessEvent(string name, bool isAlive)
		{
			try
			{
				ProcessEvent.Invoke(instance, new ProcessEventArgs()
				{
					ProcessName = name,
					isAlive = isAlive
				});
			}
			catch { }
		}
	}
}