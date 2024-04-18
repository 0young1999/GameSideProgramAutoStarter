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

		[DisplayName("최초 실행 여부")]
		[DefaultValue(true)]
		[SettingType(SettingTypeAttributeEnum.NotShow)]
		public bool isFirstRun { get; set; }
	}
}
