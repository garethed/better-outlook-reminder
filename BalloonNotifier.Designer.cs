namespace BetterOutlookReminder
{
  partial class BalloonNotifier
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
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

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BalloonNotifier));
      this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.Noisy = new System.Windows.Forms.ToolStripMenuItem();
      this.Quiet = new System.Windows.Forms.ToolStripMenuItem();
      this.Silent = new System.Windows.Forms.ToolStripMenuItem();
      this.Separator = new System.Windows.Forms.ToolStripSeparator();
      this.Quit = new System.Windows.Forms.ToolStripMenuItem();
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.pollTimer = new System.Windows.Forms.Timer(this.components);
      this.notifyTimer = new System.Windows.Forms.Timer(this.components);
      this.warningTimer = new System.Windows.Forms.Timer(this.components);
      this.contextMenu.SuspendLayout();
      // 
      // contextMenu
      // 
      this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Noisy,
            this.Quiet,
            this.Silent,
            this.Separator,
            this.Quit});
      this.contextMenu.Name = "contextMenu";
      this.contextMenu.Size = new System.Drawing.Size(105, 98);
      // 
      // Noisy
      // 
      this.Noisy.Name = "Noisy";
      this.Noisy.Size = new System.Drawing.Size(104, 22);
      this.Noisy.Text = "Noisy";
      // 
      // Quiet
      // 
      this.Quiet.Name = "Quiet";
      this.Quiet.Size = new System.Drawing.Size(104, 22);
      this.Quiet.Text = "Quiet";
      // 
      // Silent
      // 
      this.Silent.Name = "Silent";
      this.Silent.Size = new System.Drawing.Size(104, 22);
      this.Silent.Text = "Silent";
      // 
      // Separator
      // 
      this.Separator.Name = "Separator";
      this.Separator.Size = new System.Drawing.Size(101, 6);
      // 
      // Quit
      // 
      this.Quit.Name = "Quit";
      this.Quit.Size = new System.Drawing.Size(104, 22);
      this.Quit.Text = "Quit";
      this.Quit.Click += new System.EventHandler(this.Quit_Click);
      // 
      // notifyIcon
      // 
      this.notifyIcon.ContextMenuStrip = this.contextMenu;
      this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
      this.notifyIcon.Text = "Appointment Reminder";
      this.notifyIcon.Visible = true;
      this.notifyIcon.Click += new System.EventHandler(this.notifyIcon_Click);
      // 
      // pollTimer
      // 
      this.pollTimer.Enabled = true;
      this.pollTimer.Interval = 300000;
      this.pollTimer.Tick += new System.EventHandler(this.pollTimer_Tick);
      // 
      // notifyTimer
      // 
      this.notifyTimer.Tick += new System.EventHandler(this.notifyTimer_Tick);
      // 
      // warningTimer
      // 
      this.warningTimer.Tick += new System.EventHandler(this.warningTimer_Tick);
      this.contextMenu.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ContextMenuStrip contextMenu;
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.ToolStripMenuItem Noisy;
    private System.Windows.Forms.ToolStripMenuItem Quiet;
    private System.Windows.Forms.ToolStripMenuItem Silent;
    private System.Windows.Forms.ToolStripSeparator Separator;
    private System.Windows.Forms.ToolStripMenuItem Quit;
    private System.Windows.Forms.Timer pollTimer;
    private System.Windows.Forms.Timer notifyTimer;
    private System.Windows.Forms.Timer warningTimer;
  }
}
