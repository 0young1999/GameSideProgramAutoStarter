using System.Diagnostics;

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
			public int id = -1;
			public string ProcessName = "";
			public bool isAlive = false;
			public DateTime lastCheckSystemUse = DateTime.MinValue;
			public PerformanceCounter? CpuCounter = null;
			public PerformanceCounter? RamCounter = null;
			public PerformanceCounter? DiskReadCounter = null;
			public PerformanceCounter? DiskWriteCounter = null;
		}

		private Thread WDT;
		public bool setAction = false;

		private List<ProcessData> pds = new List<ProcessData>();

		private void WDTMethod()
		{
			Thread.Sleep(100);

			csCore core = csCore.GetInstance();

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

						Process[] pss = Process.GetProcesses();
						// 살아나는거 확인
						foreach (Process p in pss)
						{
							if (p.Id == 0) continue;

							int findIndex = pds.FindIndex(item => item.id.Equals(p.Id));

							if (findIndex != -1)
							{
								do
								{
									Thread.Sleep(1);
									Application.DoEvents();
								}
								while (pds[findIndex].lastCheckSystemUse > DateTime.Now + new TimeSpan(0, 0, 0, 0, core.SystemMonitorCycleTime));

								pds[findIndex].lastCheckSystemUse = DateTime.Now;

								pds[findIndex].isAlive = true;

								if (pds[findIndex].CpuCounter != null)
								{
									try
									{
										float cpuUse = pds[findIndex].CpuCounter.NextValue();
										cpuUse /= Environment.ProcessorCount;
										if (cpuUse >= core.CPUAlarmPersent)
										{
											string msg = string.Format("CPU 경고\n{0}[{2}]\n{1:N1}%", p.ProcessName, cpuUse, p.Id);
											alarm.ShowMSG(msg);
											log.ProcessUseLog(msg);
										}
									}
									catch
									{
										continue;
									}
								}

								if (pds[findIndex].RamCounter != null)
								{

									try
									{
										float RamUse = pds[findIndex].RamCounter.NextValue();
										RamUse /= (1024 * 1024 * 1024);
										if (RamUse >= core.RAMAlarmPersent)
										{
											string msg = (string.Format("RAM 경고\n{0}[{2}]\n{1:N1}GB", p.ProcessName, RamUse, p.Id));
											alarm.ShowMSG(msg);
											log.ProcessUseLog(msg);
										}
									}
									catch
									{
										continue;
									}
								}

								if (pds[findIndex].DiskReadCounter != null)
								{

									try
									{
										float Use = pds[findIndex].DiskReadCounter.NextValue();
										Use /= (1024 * 1024);
										if (Use >= core.DiskReadAlarmPersent)
										{
											string msg = (string.Format("디스크 읽기 경고\n{0}[{2}]\n{1:N1}MB", p.ProcessName, Use, p.Id));
											alarm.ShowMSG(msg);
											log.ProcessUseLog(msg);
										}
									}
									catch
									{
										continue;
									}
								}

								if (pds[findIndex].DiskWriteCounter != null)
								{

									try
									{
										float Use = pds[findIndex].DiskWriteCounter.NextValue();
										Use /= (1024 * 1024);
										if (Use >= core.DiskWriteAlarmPersent)
										{
											string msg = (string.Format("디스크 쓰기 경고\n{0}[{2}]\n{1:N1}MB", p.ProcessName, Use, p.Id));
											alarm.ShowMSG(msg);
											log.ProcessUseLog(msg);
										}
									}
									catch
									{
										continue;
									}
								}
							}
							else
							{
								string runnedInstance = GetPerformanceCounterInstanceName(p);
								PerformanceCounter CPUCounter = null;
								try
								{
									if (string.IsNullOrEmpty(runnedInstance) == false)
									{
										CPUCounter = new PerformanceCounter("Process", "% Processor Time", runnedInstance);
									}
								}
								catch (Exception e)
								{
									log.ErrorLog(GetType().Name + "CPU COUNT", e);
								}

								PerformanceCounter RAMCounter = null;
								try
								{
									if (string.IsNullOrEmpty(runnedInstance) == false)
									{
										RAMCounter = new PerformanceCounter("Process", "Working Set - Private", runnedInstance);
									}
								}
								catch (Exception e)
								{
									log.ErrorLog(GetType().Name + "RAM COUNT", e);
								}

								PerformanceCounter DiskReadCounter = null;
								try
								{
									if (string.IsNullOrEmpty(runnedInstance) == false)
									{
										DiskReadCounter = new PerformanceCounter("Process", "IO Read Bytes/sec", runnedInstance);
									}
								}
								catch (Exception e)
								{
									log.ErrorLog(GetType().Name + "Disk Read COUNT", e);
								}

								PerformanceCounter DiskWriteCounter = null;
								try
								{
									if (string.IsNullOrEmpty(runnedInstance) == false)
									{
										DiskWriteCounter = new PerformanceCounter("Process", "IO Write Bytes/sec", runnedInstance);
									}
								}
								catch (Exception e)
								{
									log.ErrorLog(GetType().Name + "Disk Write COUNT", e);
								}

								pds.Add(new ProcessData()
								{
									id = p.Id,
									ProcessName = p.ProcessName,
									isAlive = true,
									CpuCounter = CPUCounter,
									RamCounter = RAMCounter,
									DiskReadCounter = DiskReadCounter,
									DiskWriteCounter = DiskWriteCounter,
								});
								log.ProcessLog(p.ProcessName, true);
							}

							Thread.Sleep(1);
						}

						Thread.Sleep(10);

						// 죽은거 확인
						var CopyPds = pds.Where(item => item.isAlive == false);
						if (CopyPds != null)
						{
							foreach (ProcessData item in CopyPds)
							{
								log.ProcessLog(string.Format("{0}[{1}]", item.ProcessName, item.id), false);

								if (item.CpuCounter != null)
								{
									item.CpuCounter.Close();
									item.CpuCounter.Dispose();
								}
								if (item.RamCounter != null)
								{
									item.RamCounter.Close();
									item.RamCounter.Dispose();
								}
								if (item.DiskReadCounter != null)
								{
									item.DiskReadCounter.Close();
									item.DiskReadCounter.Dispose();
								}
								if (item.DiskWriteCounter != null)
								{
									item.DiskWriteCounter.Close();
									item.DiskWriteCounter.Dispose();
								}

								pds.Remove(item);
								break;
							}

							Thread.Sleep(1);
						}

						Thread.Sleep(10);

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

							Thread.Sleep(1);
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
					catch //that process has been shutdown
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