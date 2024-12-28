using System.Diagnostics;
using System.Management;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameSideProgramAutoStarter.Cs
{
	public class csProcessMonitor
	{
		private static csProcessMonitor instance;
		public static csProcessMonitor GetInstance()
		{
			if (instance == null) instance = new csProcessMonitor();
			return instance;
		}
		private csProcessMonitor()
		{
			WDT = new Thread(WDTMethod);
			WDT.IsBackground = true;
			WDT.Start();
		}

		private csLog log = csLog.GetInstance();
		private frmAlarm alarm = frmAlarm.GetInstance();
		private csProgramLink link;

		public class ProcessData
		{
			public string runnedInstance;
			public int id = -1;
			public string ProcessName = "";
			public bool isAlive = false;
			public DateTime lastCheckSystemUse = DateTime.MinValue;
			public PerformanceCounter? CpuCounter = null;
			public PerformanceCounter? RamCounter = null;
			public Thread threadSystemCheck = null;
			public bool isRun = false;

			public void CheckSystemUse()
			{
				try
				{
					if (string.IsNullOrEmpty(runnedInstance) == false)
					{
						CpuCounter = new PerformanceCounter("Process", "% Processor Time", runnedInstance);
					}
				}
				catch { }

				try
				{
					if (string.IsNullOrEmpty(runnedInstance) == false)
					{
						RamCounter = new PerformanceCounter("Process", "Working Set - Private", runnedInstance);
					}
				}
				catch { }

				if ((CpuCounter == null && RamCounter == null) == false)
				{
					frmAlarm alarm = frmAlarm.GetInstance();
					csCore core = csCore.GetInstance();
					csLog log = csLog.GetInstance();

					DateTime timeOut;

					while (isRun && (CpuCounter == null && RamCounter == null) == false)
					{
						timeOut = lastCheckSystemUse + new TimeSpan(0, 0, 0, 0, core.SystemMonitorCycleTime);

						while (timeOut >= DateTime.Now)
						{
							Thread.Sleep(100);
						}

						try
						{
							if (CpuCounter != null)
							{
								float cpuUse = CpuCounter.NextValue();
								cpuUse /= Environment.ProcessorCount;
								if (cpuUse >= core.CPUAlarmPersent)
								{
									string msg = string.Format("CPU 경고\n{0}[{2}]\n{1:N1}%", ProcessName, cpuUse, id);
									alarm.ShowMSG(msg);
									log.ProcessUseLog(msg);
								}
							}
						}
						catch
						{
							try
							{
								CpuCounter.Close();
								CpuCounter.Dispose();
								CpuCounter = null;
							}
							catch { }
						}

						try
						{
							if (RamCounter != null)
							{
								float RamUse = RamCounter.NextValue();
								RamUse /= (1024 * 1024 * 1024);
								if (RamUse >= core.RAMAlarmPersent)
								{
									string msg = (string.Format("RAM 경고\n{0}[{2}]\n{1:N1}GB", ProcessName, RamUse, id));
									alarm.ShowMSG(msg);
									log.ProcessUseLog(msg);
								}
							}
						}
						catch
						{
							try
							{
								RamCounter.Close();
								RamCounter.Dispose();
								RamCounter = null;
							}
							catch { }
						}
					}
				}

				threadSystemCheck = null;
			}
		}

		private Thread WDT;
		public bool setAction = false;

		private List<ProcessData> pds = new List<ProcessData>();

		private void WDTMethod()
		{
			Thread.Sleep(100);

			csCore core = csCore.GetInstance();

			bool isFirst = true;
			DateTime startTime = DateTime.Now;

			link = csProgramLink.GetInstance();

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

						foreach (csProgramLinkSub data in link.list)
						{
							data.isAlive = false;
						}

						Process[] pss = Process.GetProcesses().Where(item => item.Id != 0 && item.Id != 4 && IsSystemProcessByName(item) == false).ToArray();

						// 살아나는거 확인
						for (int i = 0; i < pss.Length; i++)
						{
							csProgramLinkSub[] links = link.list.Where(item => item.GameProcessName.Equals(pss[i].ProcessName) && item.isAlive == false).ToArray();

							foreach (csProgramLinkSub item in links)
							{
								item.isAlive = true;
							}

							int findIndex = pds.FindIndex(item => item.id.Equals(pss[i].Id));

							if (findIndex != -1)
							{
								pds[findIndex].lastCheckSystemUse = DateTime.Now;

								pds[findIndex].isAlive = true;
							}
							else
							{
								string runnedInstance = GetPerformanceCounterInstanceName(pss[i]);

								ProcessData temp = new ProcessData()
								{
									runnedInstance = runnedInstance,
									id = pss[i].Id,
									ProcessName = pss[i].ProcessName,
									isAlive = true,
									isRun = true,
								};

								pds.Add(temp);
								log.ProcessLog(pss[i].ProcessName, true);
							}

							if (isFirst)
							{
								alarm.ShowMSG(string.Format("초기 색인 중\n{2}\n{0}/{1}", i, pss.Length, pss[i].ProcessName));
							}
						}

						if (isFirst)
						{
							foreach (ProcessData data in pds)
							{
								data.threadSystemCheck = new Thread(data.CheckSystemUse);
								data.threadSystemCheck.Start();
							}

							isFirst = false;
							alarm.ShowMSG(string.Format("초기 색인 완료\n{0:N1}초 걸림", (DateTime.Now - startTime).TotalSeconds));
						}

						// 죽은거 확인
						var CopyPds = pds.Where(item => item.isAlive == false);
						if (CopyPds != null)
						{
							foreach (ProcessData item in CopyPds)
							{
								log.ProcessLog(string.Format("{0}[{1}]", item.ProcessName, item.id), false);

								item.isRun = false;

								pds.Remove(item);
								break;
							}
						}

						// 사이드 프로그램 자동 시작 컨트롤
						foreach (csProgramLinkSub data in link.list)
						{
							if (data.isAlive && string.IsNullOrEmpty(data.SideProcessName))
							{
								UpdateProcessEvent(data.GameProcessName, true);
							}
							else if (data.isAlive == false && string.IsNullOrEmpty(data.SideProcessName) == false)
							{
								UpdateProcessEvent(data.GameProcessName, false);
							}
						}

						Thread.Sleep(10);
					}
					Thread.Sleep(10);
				}
				catch (Exception e)
				{
					log.ErrorLog(GetType().Name, e);
					alarm.ShowMSG("프로세스 모니터링에\n오류가 발생하였습니다.\n관리자 권한으로 실행되었는지 확인해주세요.");
					Thread.Sleep(5000);
				}
			}
		}

		//This method is only used in windows
		private static string GetPerformanceCounterInstanceName(Process process)
		{
			var processId = process.Id;
			var processCategory = new PerformanceCounterCategory("Process");
			var runnedInstances = processCategory.GetInstanceNames();

			foreach (string runnedInstance in runnedInstances)
			{
				if (!runnedInstance.StartsWith(process.ProcessName, StringComparison.OrdinalIgnoreCase))
					continue;

				if (process.HasExited)
					return string.Empty;

				using (var performanceCounter = new PerformanceCounter("Process", "ID Process", runnedInstance, true))
				{
					var counterProcessId = 0;

					try
					{
						counterProcessId = (int)performanceCounter.RawValue;
					}
					catch
					{
						continue;
					}

					if (counterProcessId == processId)
					{
						return runnedInstance;
					}
				}
			}

			return process.ProcessName;
		}
		public static bool IsSystemProcessByName(Process p)
		{
			string[] systemProcessNames = { "System", "Idle", "smss.exe", "csrss.exe", "wininit.exe", "lsass.exe" };
			return systemProcessNames.Contains(p.ProcessName);
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