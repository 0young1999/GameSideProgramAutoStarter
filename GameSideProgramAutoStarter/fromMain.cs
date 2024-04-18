using GameSideProgramAutoStarter;
using GameSideProgramAutoStarter.Cs;
using System;
using System.Diagnostics;
using Young.Setting;

namespace GameSideProgramAutoStarter
{
	public partial class formMain : Form
	{
		private static formMain instance;
		public static formMain GetInstance()
		{
			if (instance == null) instance = new formMain();
			return instance;
		}

		private csCore core = csCore.GetInstance();

		private frmMessageBox? programExitFMB;
		private frmAlarm alarm = frmAlarm.GetInstance();
		private SettingForm? sf;
		private csProcessLog pl = csProcessLog.GetInstance();

		private formMain()
		{
			InitializeComponent();

			ContextMenuStrip strip = new ContextMenuStrip();

			ToolStripMenuItem itemSetting = new ToolStripMenuItem();
			itemSetting.Text = "����";
			itemSetting.Click += (s, e) =>
			{
				if (sf != null) return;
				
				sf = new SettingForm();
				sf._SetObject(csProgramLinkMaster.GetInstance(), "��ũ");
				sf.ShowDialog();

				sf = null;
			};
			strip.Items.Add(itemSetting);

			ToolStripMenuItem itemExit = new ToolStripMenuItem();
			itemExit.Text = "����";
			itemExit.Click += (s, e) =>
			{
				if (programExitFMB != null) return;

				programExitFMB = new frmMessageBox(frmMessageBox.fmbButtonType.OKNO, frmMessageBox.fmbIconType.icon, false, this.Text, "���� �Ͻðٽ��ϱ�?");
				if (programExitFMB.ShowDialog() == DialogResult.OK)
				{
					Close();
				}

				programExitFMB = null;
			};
			strip.Items.Add(itemExit);

			notifyIcon1.ContextMenuStrip = strip;
		}

		private Thread? threadProcessCheck = null;

		private void formMain_Load(object sender, EventArgs e)
		{
			threadProcessCheck = new Thread(checkProcess);
			threadProcessCheck.IsBackground = true;
			threadProcessCheck.Start();

			Thread thread = new Thread(HideForm1);
			thread.IsBackground = true;
			thread.Start();
		}

		private void HideForm1()
		{
			Thread.Sleep(100);

			Invoke((MethodInvoker)delegate
			{
				this.Visible = false;
				if (core.isFirstRun)
				{
					frmMessageBox fmb = new frmMessageBox(frmMessageBox.fmbButtonType.OK, frmMessageBox.fmbIconType.icon, false,
						this.Text,
						"���� ���α׷��� �ڵ����� ���������.\r\n������ �Ʒ����� ���� �� ������ �Ҽ� �ּ���!");
					fmb.ShowDialog();

					core.isFirstRun = false;
					core.Save();
				}

				alarm.ShowMSG("������!");
			});

			this.Invoke((MethodInvoker)delegate
			{
				this.Visible = false;
			});
		}

		private csProgramLinkMaster master = csProgramLinkMaster.GetInstance();

		private void checkProcess()
		{
			while (true)
			{
				try
				{
					Process[] array = Process.GetProcesses();
					foreach (Process process in array)
					{
					}

					Thread.Sleep(1000);
				}
				catch { }
			}
		}
	}
}