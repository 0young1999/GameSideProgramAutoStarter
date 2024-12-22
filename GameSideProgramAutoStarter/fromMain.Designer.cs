namespace GameSideProgramAutoStarter
{
	partial class formMain
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
			notifyIcon1 = new NotifyIcon(components);
			SuspendLayout();
			// 
			// notifyIcon1
			// 
			notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
			notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
			notifyIcon1.Text = "AUTO STARTER";
			notifyIcon1.Visible = true;
			// 
			// formMain
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(231, 291);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "formMain";
			ShowInTaskbar = false;
			Text = "GameSideProgramAutoStarter";
			Load += formMain_Load;
			ResumeLayout(false);
		}

		#endregion
		private NotifyIcon notifyIcon1;
	}
}
