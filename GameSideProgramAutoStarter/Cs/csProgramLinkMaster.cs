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

			pl.ProcessEvent += pl_Event;
			pl.setAction = true;
		}

		private frmAlarm alarm = frmAlarm.GetInstance();
		private csProcessLog pl = csProcessLog.GetInstance();

		private void pl_Event(object? sender, csProcessLog.ProcessEventArgs e)
		{
			if (string.IsNullOrEmpty(e.ProcessName)) return;

			if (e.isAlive)
			{
				foreach (csProgramLinkSub item in list.Where(item => item.GameProcessName == e.ProcessName))
				{
					if (string.IsNullOrEmpty(item.SideProgramPath)) return;

					item.SideProcessName = Process.Start(item.SideProgramPath).ProcessName;

					alarm.ShowMSG(e.ProcessName + "감지\r\n" + item.SideProcessName + "이(가)\r\n자동으로 실행됩니다.");
				}
			}
			else
			{
				foreach (csProgramLinkSub item in list.Where(item => (item.GameProcessName == e.ProcessName && item.AutoClose)))
				{
					foreach (Process pSide in Process.GetProcessesByName(item.SideProcessName))
					{
						pSide.Kill();
					}

					alarm.ShowMSG(item.GameProcessName + " 종료 감지\r\n" + item.SideProcessName + "이(가)\r\n자동으로 종료됩니다.");
				}
			}
		}

		[DisplayName("자동실행 조건")]
		[Description("자행실행 조건을 설정합니다.")]
		public BindingList<csProgramLinkSub> list { get; set; } = new BindingList<csProgramLinkSub>();
	}
	[Serializable]
	public class csProgramLinkSub : csAutoSaveLoad
	{
		[DisplayName("게임 프로세스 이름")]
		public string? GameProcessName { get; set; }

		[DisplayName("사이드 프로그램 실행 파일 위치")]
		[String(StringAttributeEnum.File)]
		public string? SideProgramPath { get; set; }

		public string? SideProcessName = null;

		[DisplayName("자동 종료")]
		[DefaultValue(false)]
		public bool AutoClose { get; set; }
	}
}
