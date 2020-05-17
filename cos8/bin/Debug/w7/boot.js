
import System.Windows.Forms;
import System.Drawing;

function initializeComponent() {
	self = new System.Windows.Forms.Form();
	var pictureBox1 = new System.Windows.Forms.PictureBox();
	var label1 = new System.Windows.Forms.Label();
	self.SuspendLayout();
	// 
	// pictureBox1
	// 
	pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
	pictureBox1.Image = System.Drawing.Image.FromFile("images/loading.gif");
	pictureBox1.Location = new System.Drawing.Point(386, 128);
	pictureBox1.Name = "pictureBox1";
	pictureBox1.Size = new System.Drawing.Size(150, 150);
	pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
	pictureBox1.TabIndex = 0;
	pictureBox1.TabStop = false;
	// 
	// label1
	// 
	label1.Anchor = System.Windows.Forms.AnchorStyles.None;
	label1.AutoSize = true;
	label1.Font = new System.Drawing.Font("Calibri", 20, System.Drawing.FontStyle.Regular);
	label1.ForeColor = System.Drawing.Color.DarkGray;
	label1.Location = new System.Drawing.Point(349, 319);
	label1.Name = "label1";
	label1.Size = new System.Drawing.Size(226, 33);
	label1.TabIndex = 1;
	label1.Text = "Cargando Windows";
	// 
	// Form1
	// 
	self.AutoScaleDimensions = new System.Drawing.SizeF(6, 13);
	self.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	self.BackColor = System.Drawing.Color.Black;
	self.ClientSize = new System.Drawing.Size(921, 491);
	self.Controls.Add(label1);
	self.Controls.Add(pictureBox1);
	self.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
	self.Name = "Form1";
	self.Text = "Form1";
	self.WindowState = System.Windows.Forms.FormWindowState.Maximized;
	self.ResumeLayout(false);
	self.PerformLayout();
	self.ShowDialog();
}

initializeComponent();
