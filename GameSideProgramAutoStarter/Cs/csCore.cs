using System.ComponentModel;
using Young.Setting;

namespace GameSideProgramAutoStarter.Cs
{
	public class csCore : csAutoSaveLoad
	{
		private static csCore instance;
		public static csCore GetInstance()
		{
			if (instance == null) instance = new csCore();
			return instance;
		}
		private csCore()
		{
			Load();
		}

		[DisplayName("튜토리얼 보기")]
		[DefaultValue(true)]
		public bool isFirstRun { get; set; }

		[DisplayName("튜토리얼 자동 다신 안보기")]
		[DefaultValue(true)]
		public bool firstRunAutoOff { get; set; }

		[DisplayName("시스템 모니터 주기(ms)")]
		[DefaultValue(5000)]
		public int SystemMonitorCycleTime { get; set; }

		[DisplayName("CPU 사용량 경고 %")]
		[DefaultValue(50)]
		public float CPUAlarmPersent { get; set; }

		[DisplayName("Ram 사용량 경고 GB")]
		[DefaultValue(10)]
		public float RAMAlarmPersent { get; set; }
	}
}
