
import System.Windows.Forms;
import System.Drawing;

function initializeComponent() {
	self = new System.Windows.Forms.Form();
	self.SuspendLayout();
	
	self.AutoScaleDimensions = new System.Drawing.SizeF(6, 13);
	self.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	self.ClientSize = new System.Drawing.Size(284, 261);
	self.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
	self.Name = "MainForm";
	self.Text = "MainForm";
	self.WindowState = System.Windows.Forms.FormWindowState.Maximized;
	self.ResumeLayout(false);
	
	self.ShowDialog();
}

initializeComponent();
