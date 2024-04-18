using System;
using System.ComponentModel;
using System.Diagnostics;
using Young.Setting;

namespace GameSideProgramAutoStarter.Cs
{
	[Serializable]
	public class csProgramLinkMaster : csAutoSaveLoad
	{
		private static csProgramLinkMaster instance;
		public static csProgramLinkMaster GetInstance()
		{
			if (instance == null) instance = new csProgramLinkMaster();
			return instance;
		}
		private csProgramLinkMaster()
		{
			Load();

			Thread thread = new Thread(CheckProcess)
			{
				IsBackground = true,
			};
			thread.Start();
		}

		private frmAlarm alarm = frmAlarm.GetInstance();

		[DisplayName("자동실행 조건")]
		[Description("자행실행 조건을 설정합니다.")]
		public BindingList<csProgramLinkSub> list { get; set; } = new BindingList<csProgramLinkSub>();

		public void SetProcess(Process process)
		{
			foreach (csProgramLinkSub sub in list)
			{
				if (process.ProcessName == sub.GameProcessName && sub.isRun == false && string.IsNullOrEmpty(sub.SideProgramPath) == false)
				{
					sub.isRun = true;
					sub.SideProcessName = Process.Start(sub.SideProgramPath).ProcessName;

					alarm.ShowMSG(process.ProcessName + "감지\r\n" + sub.SideProcessName + "이(가)\r\n자동으로 실행됩니다.");
				}
			}
		}
		public void SetProcess(string processName)
		{
			foreach (csProgramLinkSub sub in list)
			{
				if (processName == sub.GameProcessName && sub.isRun == false && string.IsNullOrEmpty(sub.SideProgramPath) == false)
				{
					sub.isRun = true;
					sub.SideProcessName = Process.Start(sub.SideProgramPath).ProcessName;

					alarm.ShowMSG(processName + "감지\r\n" + sub.SideProcessName + "이(가)\r\n자동으로 실행됩니다.");
				}
			}
		}

		private void CheckProcess()
		{
			try
			{
				while (true)
				{
					foreach (csProgramLinkSub sub in list)
					{
						if (sub.isRun)
						{
							Process[] p = Process.GetProcessesByName(sub.GameProcessName);
							if (p == null || p.Length == 0)
							{
								sub.isRun = false;

								if (sub.AutoClose && string.IsNullOrEmpty(sub.SideProgramPath) == false)
								{
									foreach (Process pSide in Process.GetProcessesByName(sub.SideProcessName))
									{
										pSide.Kill();
									}

									alarm.Invoke((MethodInvoker)delegate
									{
										alarm.ShowMSG(sub.GameProcessName + " 종료 감지\r\n" + sub.SideProcessName + "이(가)\r\n자동으로 종료됩니다.");
									});
								}
							}
						}
					}
					Thread.Sleep(1000);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}
	}
	[Serializable]
	public class csProgramLinkSub : csAutoSaveLoad
	{
		[DisplayName("게임 프로세스 이름")]
		public string? GameProcessName { get; set; }

		[DisplayName("사이드 프로그램 실행 파일 위치")]
		[String(StringAttributeEnum.File)]
		public string? SideProgramPath { get; set; }

		public string SideProcessName = null;

		[DisplayName("자동 종료")]
		[DefaultValue(false)]
		public bool AutoClose { get; set; }

		public bool isRun = false;
	}
}
