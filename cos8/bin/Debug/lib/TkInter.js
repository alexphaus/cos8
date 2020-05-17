/*
 * lib::TkInter (Tk interface) v2.0 Jan 2019
 * default graphic library for cos8
 */

// import .NET libraries
from System.Windows.Forms import *;
from System.Drawing import *;

// set os default style and text redering
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);

function start(form) {
	Application.Run(form);
}

// set properties via tk commands
function tkset(obj, prop) {
	var root = obj;
	var container = root;
	// loop throw properties
	foreach (var attr in prop.split(',')) {
		if (attr == "") {
			// nothing to do
		}
		else if (attr[0] == \"@") {
			// set container
			if (attr == "@this")
				container = obj;
			else if (attr == "@root")
				container = root;
			else
				container = root.Controls.Find(attr.shift(), true)[0];
		}
		else if (attr[0] == \"*") {
			// set the name
			var n = attr.shift();
			obj.Name = n;
			global.Add(n, obj); // use as variable
		}
		else if (attr[0] == \"#") {
			// set background-color by HEX
			obj.BackColor = ColorTranslator.FromHtml(attr);
		}
		else if (attr[0] == \"+") {
			// add new control
			attr = attr.shift();
			var c;
			
			switch (attr) {
				case "button": c = new Button();
				case "a":
					c = new Label();
					c.AutoSize = true;
				case "link": c = new LinkLabel();
				case "text": c = new TextBox();
				case "number": c = new NumericUpDown();
				case "rtf": c = new RichTextBox();
				case "img": c = new PictureBox();
				case "div": c = new Panel();
				case "check": c = new CheckBox();
				case "radio": c = new RadioButton();
				case "list": c = new ListBox();
				case "select": c = new ComboBox();
				case "bar": c = new ProgressBar();
				case "split": c = new SplitContainer();
				case "tab": c = new TabControl();
				case "table": c = new DataGridView();
				case "menu": c = new MenuStrip();
			}
			
			container.Controls.Add(c);
			obj = c;
		}
		else if (attr == "center") {
			// center the object into container
			if (obj instanceof Form)
				obj.StartPosition = FormStartPosition.CenterScreen;
			else {
				obj.Anchor = AnchorStyles.None; // re-center if parent changed size
				obj.Left = ((obj.Parent.Width - obj.Width) / 2) as int;
				obj.Top = ((obj.Parent.Height - obj.Height) / 2) as int;
			}
		}
		else if (attr == "center-x") {
			// center horizontally
			obj.Anchor = AnchorStyles.Top;
			obj.Left = ((obj.Parent.Width - obj.Width) / 2) as int;
		}
		else if (attr == "center-y") {
			// center vertically
			obj.Anchor = AnchorStyles.Left;
			obj.Top = ((obj.Parent.Height - obj.Height) / 2) as int;
		}
		else if (attr == "full") {
			// dock to fill
			if (obj instanceof Form) {
				obj.FormBorderStyle = 0;
				obj.Location = new Point(0, 0);
				obj.Size = SystemInformation.PrimaryMonitorSize;
			}
			else
				obj.Dock = DockStyle.Fill;
		}
		else if (attr == "maximized") {
			// maximize the window
			obj.WindowState = FormWindowState.Maximized;
		}
		else if (attr == "minimized") {
			// minimize the window
			obj.WindowState = FormWindowState.Minimized;
		}
		else if (attr == "fixed") {
			// set border to 1px
			obj.BorderStyle = BorderStyle.FixedSingle;
		}
		else if (attr == "flat") {
			// change button appearance
			obj.FlatStyle = FlatStyle.Flat;
		}
		else if (attr == "no-repeat") {
			// set background-image layout to none
			obj.BackgroundImageLayout = ImageLayout.None;
		}
		else if (attr == "zoom") {
			// set background-image layout to zoom
			obj.BackgroundImageLayout = ImageLayout.Zoom;
		}
		else if (attr == "edit") {
			// edit size and location runtime
			dragging = false;
			resize = false;
			beginX = 0;
			beginY = 0;
			obj.MouseDown = function(s, e) {
				if (e.X > s.Width - 10 && e.Y > s.Height - 10) {
					resize = true;
					s.Cursor = Cursors.SizeNWSE;
				}
				else {
					dragging = true;
					beginX = e.X;
					beginY = e.Y;
					s.Cursor = Cursors.SizeAll;
				}
			}
			obj.MouseMove = function(s, e) {
				if (dragging) {
					s.Left = s.Left + e.X - beginX;
					s.Top = s.Top + e.Y - beginY;
				}
				else if (resize) {
					s.Height = e.Y;
					s.Width = e.X;
				}
			}
			obj.MouseUp = function(s, e) {
				dragging = false;
				resize = false;
				s.Cursor = Cursors.Default;
				// get size and location
				var clip = "" + s.Width + 'x' + s.Height + ',' + s.Left + ':' + s.Top;
				s.FindForm().Text = clip;
				Clipboard.SetText(clip);
			}
		}
		else if (attr like "^\d+[x:]\d+$") {
			// set size or location
			var oper = attr.Contains("x") ? "x" : ":";
			var s = attr.split(oper);
			
			if (oper == "x") {
				// size
				obj.Width = s[0] as int;
				obj.Height = s[1] as int;
			}
			else {
				if (obj instanceof Form)
					obj.StartPosition = FormStartPosition.Manual;
				// location
				obj.Left = s[0] as int;
				obj.Top = s[1] as int;
			}
		}
		else if (attr like "^[a-z-]+:.+") {
			// css styling
			var property = attr.before(':');
			var value = attr.after(':');
			
			switch (property) {
				// background
				case "background-color":
					obj.BackColor = ColorTranslator.FromHtml(value);
				case "background-image":
					obj.BackgroundImage = Image.FromFile(value);
				case "background-layout":
					var layout;
					switch (value) {
						case "none": layout = ImageLayout.None;
						case "tile": layout = ImageLayout.Tile;
						case "center": layout = ImageLayout.Center;
						case "stretch": layout = ImageLayout.Stretch;
						case "zoom": layout = ImageLayout.Zoom;
					}
					obj.BackgroundImageLayout = layout;
				// font
				case "font-family":
					obj.Font = new Font(value, obj.Font.Size);
				case "font-size":
					var px = value.slice(0,-2) as int;
					obj.Font = new Font(obj.Font.FontFamily, px);
				case "font-weight":
					if (value == "bold")
						obj.Font = new Font(obj.Font, obj.Font.Style | FontStyle.Bold);
				case "font-style":
					if (value == "italic")
						obj.Font = new Font(obj.Font, obj.Font.Style | FontStyle.Italic);
				// text
				case "color":
					obj.ForeColor = ColorTranslator.FromHtml(value);
				case "text-decoration":
					if (value == "underline")
						obj.Font = new Font(obj.Font, obj.Font.Style | FontStyle.Underline);
					else if (value == "line-through")
						obj.Font = new Font(obj.Font, obj.Font.Style | FontStyle.Strikeout);
				case "text-transform":
					if (value == "uppercase")
						obj.CharacterCasing = CharacterCasing.Upper;
					else if (value == "lowercase")
						obj.CharacterCasing = CharacterCasing.Lower;
				// padding
				case "padding":
					var left, top, right, bottom;
					var px = value.split(' ');
					switch (px.Count) {
						case 4:
							top = px[0] as int;
							right = px[1] as int;
							bottom = px[2] as int;
							left = px[3] as int;
						case 3:
							top = px[0] as int;
							right = px[1] as int;
							bottom = px[2] as int;
							left = right;
						case 2:
							top = px[0] as int;
							right = px[1] as int;
							bottom = top;
							left = right;
						case 1:
							top = px[0] as int;
							right = top;
							bottom = top;
							left = top;
					}
					obj.Padding = new Padding(left, top, right, bottom);
				// positioning
				case "left":
					obj.Left = value as int;
				case "top":
					obj.Top = value as int;
				// dimension
				case "width":
					obj.Width = value as int;
				case "height":
					obj.Height = value as int;
				// border
				case "border":
					var sets = value.split(" ");
					var px = sets[0].slice(0,-2) as int;
					var style;
					switch (sets[1]) {
						case "solid":
							style = DashStyle.Solid;
						case "dashed":
							style = DashStyle.Dash;
						case "dotted":
							style = DashStyle.Dot;
					}
					var color = ColorTranslator.FromHtml(sets[2]);
					
					var p = new Pen(color, px);
					p.DashStyle = style;
					
					obj.Paint = function(sender, e, local) {
						var p = local["p"];
						var halfThickness = Math.Floor(p.Width / 2) as int;
						e.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness, (sender.Width - p.Width) as int, (sender.Height - p.Width) as int));
					}
					
					obj.SizeChanged = function(sender, e) {
						sender.Invalidate();
					}
				// classification
				case "cursor":
					switch (value) {
						case "default":
							obj.Cursor = Cursors.Default;
						case "arrow":
							obj.Cursor = Cursors.Arrow;
						case "size-all":
							obj.Cursor = Cursors.SizeAll;
						case "size-ns":
							obj.Cursor = Cursors.SizeNS;
						case "size-we":
							obj.Cursor = Cursors.SizeWE;
						case "size-nesw":
							obj.Cursor = Cursors.SizeNESW;
						case "size-nwse":
							obj.Cursor = Cursors.SizeNWSE;
						case "cross":
							obj.Cursor = Cursors.Cross;
						case "text":
							obj.Cursor = Cursors.IBeam;
						case "pointer":
							obj.Cursor = Cursors.Hand;
						case "wait":
							obj.Cursor = Cursors.WaitCursor;
						case "progress":
							obj.Cursor = Cursors.AppStarting;
						case "not-allowed":
							obj.Cursor = Cursors.No;
					}
				// control
				case "placeholder":
					var EM_SETCUEBANNER = 5377;
					external.SendMessage(obj.Handle, EM_SETCUEBANNER, 0, value);
				case "flat-border-size":
					obj.FlatAppearance.BorderSize = value as int;
				case "flat-border-color":
					obj.FlatAppearance.BorderColor = ColorTranslator.FromHtml(value);
			}
		}
		else if (Color.FromName(attr).IsKnownColor) {
			// set background-color by name
			obj.BackColor = ColorTranslator.FromHtml(attr);
		}
		else if (attr.EndsWith(".jpg")) {
			// set backgroundImage
			obj.BackgroundImage = Image.FromFile(attr);
		}
		else if (attr.EndsWith(".png")) {
			obj.BackgroundImage = Image.FromFile(attr);
		}
		else if (attr.EndsWith(".ico")) {
			obj.Icon = new Icon(attr);
		}
		else {
			obj.Text = attr;
		}
	}
}