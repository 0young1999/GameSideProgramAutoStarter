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
	}
}
