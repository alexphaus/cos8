
import tkinter;
import stockfish;

frmPromote = new Form();
frmPromote.Opacity = 0.9;
frmPromote.FormBorderStyle = FormBorderStyle.None;
frmPromote.ShowInTaskbar = false;
tkset(frmPromote, "342x150,379:300,border:3px solid black,+a,Choose Promotion Piece,font-family:consolas,font-size:17px,font-weight:bold,center-x,top:15,+button,*btnQ,nogal/wq.png,80x80,zoom,8:50,+button,*btnR,nogal/wr.png,80x80,zoom,90:50,+button,*btnB,nogal/wb.png,80x80,zoom,172:50,+button,*btnN,nogal/wn.png,80x80,zoom,254:50");

btnQ.Click = function(sender, e) {
	move_t = MOVE_TYPE_PROMOTION_TO_QUEEN;
	frmPromote.Close();
}

btnR.Click = function(sender, e) {
	move_t = MOVE_TYPE_PROMOTION_TO_ROOK;
	frmPromote.Close();
}

btnB.Click = function(sender, e) {
	move_t = MOVE_TYPE_PROMOTION_TO_BISHOP;
	frmPromote.Close();
}

btnN.Click = function(sender, e) {
	move_t = MOVE_TYPE_PROMOTION_TO_KNIGHT;
	frmPromote.Close();
}

frmwon = new Form();
frmwon.Opacity = 0.9;
frmwon.FormBorderStyle = FormBorderStyle.None;
frmwon.ShowInTaskbar = false;
tkset(frmwon, "342x150,379:300,border:3px solid black,+a,You won!,font-family:consolas,font-size:24px,font-weight:bold,center-x,top:15,+img,assets/happy.png,64x64,zoom,center-x,top:60");
frmwon.Deactivate = function(sender, e) {
	sender.Close();
}

frmlost = new Form();
frmlost.Opacity = 0.9;
frmlost.FormBorderStyle = FormBorderStyle.None;
frmlost.ShowInTaskbar = false;
tkset(frmlost, "342x150,379:300,border:3px solid black,+a,You lost!,font-family:consolas,font-size:24px,font-weight:bold,color:red,center-x,top:15,+img,assets/sad.png,64x64,zoom,center-x,top:60");
frmlost.Deactivate = function(sender, e) {
	sender.Close();
}

f = new Form();

tkset(f, "Chess,maximized,assets/wall.jpg,no-repeat,+a,ChessBase,transparent,color:white,font-family:verdana,font-size:30px,font-weight:bold,250:10,+div,transparent,border:5px dashed orange,880:70,300x600,@this,+button,*btnnewgame,color:white,Start new game,200x60,center-x,top:30,#E6912C,font-family:calibri,font-size:14px,font-weight:bold,flat,+button,color:white,flat,90x90,50:100,font-size:40px,font-family:consolas,flat-border-size:0,<,+button,color:white,flat,90x90,159:100,font-size:40px,font-family:consolas,flat-border-size:0,>,+a,Notación,color:blue,50:200,font-family:calibri,font-size:13px,font-weight:bold,+rtf,*lstmoves,50:225,200x300,font-family:fira mono,font-size:12px,+img,*editboard,assets/chess-board.png,46x46,27:531,zoom,cursor:pointer,+a,*curpiece,font-family:Chess Merida Arena,font-size:35px,color:white,p,70:531");

btnnewgame.Click = function(sender, e) {
	init_board();
	var s = 0;
	for (var i = 0; i < 8; i++) {
		for (var j = 0; j < 8; j++) {
			var sqr = f.Controls[s as String];
			if (i == 0)
				sqr.ImageLocation = theme + "/b" + ("rnbqkbnr")[j] + ".png";
			else if (i == 1)
				sqr.ImageLocation = theme + "/bp.png";
			else if (i == 6)
				sqr.ImageLocation = theme + "/wp.png";
			else if (i == 7)
				sqr.ImageLocation = theme + "/w" + ("rnbqkbnr")[j] + ".png";
			else
				sqr.Image = null;
			s++;
		}
	}
	if (sqrfrom != null) {
		sqrfrom.BackColor = sqrfrombg;
		sqrdest.BackColor = sqrdestbg;
	}
	lstmoves.Clear();
	newgamesound.Play();
}

movesound = new System.Media.SoundPlayer("sounds\move.wav");
checksound = new System.Media.SoundPlayer("sounds\check.wav");
capturesound = new System.Media.SoundPlayer("sounds\capture.wav");
checkmatesound = new System.Media.SoundPlayer("sounds\mate.wav");
castlesound = new System.Media.SoundPlayer("sounds\castle.wav");
newgamesound = new System.Media.SoundPlayer("sounds\newgame.wav");
illegalsound = new System.Media.SoundPlayer("sounds\illegal.wav");

editing = false;
pieceBit = 0; // stockfish piece representation
editboard.Click = function(s, e) {
	editing = true;
	init_board();
	for (var i = 0; i < 64; i++) {
		piece[i] = empty;
		color[i] = empty;
		
		f.Controls[i as String].Image = null;
	}
}

f.KeyPreview = true;
f.KeyPress = function(s, e) {
	if (editing) {
		switch (e.KeyChar) {
			case \'p':
				curpiece.Text = "p";
				pieceBit = 0;
			case \'n':
				curpiece.Text = "n";
				pieceBit = 1;
			case \'b':
				curpiece.Text = "b";
				pieceBit = 2;
			case \'r':
				curpiece.Text = "r";
				pieceBit = 3;
			case \'q':
				curpiece.Text = "q";
				pieceBit = 4;
			case \'k':
				curpiece.Text = "k";
				pieceBit = 5;
		}
	}
}

f.KeyUp = function(s, e) {
	if (e.KeyCode == Keys.L) {
		editing = false;
		// print fen
		cout << "piece" << endl;
		for (var i = 0; i < 64; i++) {
			cout << piece[i] << ",";
			if ((i & 7) == 7)
				cout << endl;
		}
		cout << "color" << endl;
		for (i = 0; i < 64; i++) {
			cout << color[i] << ",";
			if ((i & 7) == 7)
				cout << endl;
		}
	}
	else if (e.KeyCode == Keys.D) {
		while (true) {
			var m = think(3);
			if (m != null) {
				makeMove(m);
				drawMove( f.Controls[m.from as String] ,
						  f.Controls[m.dest as String] ,
						  m.type);
			}
			else {
				alert("CheckMate, " + (1-side==0?"White":"Black") + " won");
				break;
			}
		}
	}
}

var sqrfrom, sqrdest;
var sqrfrombg, sqrdestbg;
var mcnt = 1, m2 = false;

function drawMove(picfrom, picdest, movetype) {
	/* restore colors */
	if (sqrfrom != null) {
		sqrfrom.BackColor = sqrfrombg;
		sqrdest.BackColor = sqrdestbg;
	}
	if (check(side)) {
		checksound.Play();
	}
	else if (picdest.Image != null) {
		capturesound.Play();
	}
	else {
		movesound.Play();
	}
	/* move image */
	
	switch (movetype) {
		case MOVE_TYPE_NORMAL:
			picdest.Image = picfrom.Image;
		case MOVE_TYPE_PROMOTION_TO_QUEEN:
			picdest.ImageLocation = theme + "/" + (side?'w':'b') + "q.png";
		case MOVE_TYPE_PROMOTION_TO_ROOK:
			picdest.ImageLocation = theme + "/" + (side?'w':'b') + "r.png";
		case MOVE_TYPE_PROMOTION_TO_BISHOP:
			picdest.ImageLocation = theme + "/" + (side?'w':'b') + "b.png";
		case MOVE_TYPE_PROMOTION_TO_KNIGHT:
			picdest.ImageLocation = theme + "/" + (side?'w':'b') + "n.png";
	}
	
	picfrom.Image = null;
	
	/* highlight squares */
	sqrfrom = picfrom;
	sqrdest = picdest;
	sqrfrombg = picfrom.BackColor;
	sqrdestbg = picdest.BackColor;
	picfrom.BackColor = lightcolor;
	picdest.BackColor = lightcolor;
	
	/* show notation */
	var hst = hist[hdp-1];
	var output = "";
	if (m2)
		mcnt++;
	else
		output += mcnt + ".";
	output += " ";
	var pce = piece[hst.m.dest];
	if (pce != pawn)
		output += ("PNBRQK")[pce];
	if (hst.cap != empty) {
		if (pce == pawn)
			output += (97 + col(hst.m.from)) as char;
		output += "x";
	}
	output += picdest.Tag;
	if (check(side))
		output += "+";
	if (m2)
		output += endl;
	m2 = !m2;
	lstmoves.AppendText(output);
	
	/* refresh interface */
	Application.DoEvents();
}

var x = 250;
var y = 70;
var bg = 0;
var colors = [Color.FromArgb(238,238,210), Color.FromArgb(118,150,86)];
var theme = "nogal";
selected = null;
selected_color = null;
lightcolor = Color.FromArgb(255, 243, 117);
var n = 0;
for (i = 0; i < 8; i++) {
	for (j = 0; j < 8; j++) {
		var sqr = new PictureBox();
		sqr.Name = n as String;
		sqr.Tag = ("abcdefgh")[j] as String + (8 - i);
		sqr.Size = new Size(75, 75);
		sqr.Location = new Point(x, y);
		sqr.BackColor = colors[bg];
		sqr.SizeMode = PictureBoxSizeMode.Zoom;
		if (i == 0)
			sqr.ImageLocation = theme + "/b" + ("rnbqkbnr")[j] + ".png";
		else if (i == 1)
			sqr.ImageLocation = theme + "/bp.png";
		else if (i == 6)
			sqr.ImageLocation = theme + "/wp.png";
		else if (i == 7)
			sqr.ImageLocation = theme + "/w" + ("rnbqkbnr")[j] + ".png";
		
		sqr.MouseClick = function(sender, e) {
			if (editing) {
				if (e.Button == MouseButtons.Left) {
					// white piece
					sender.ImageLocation = theme + "/w" + curpiece.Text + ".png";
					piece[sender.Name as int] = pieceBit;
					color[sender.Name as int] = white;
				}
				else if (e.Button == MouseButtons.Middle) {
					// delete piece
					sender.Image = null;
					piece[sender.Name as int] = empty;
					color[sender.Name as int] = empty;
				}
				else if (e.Button == MouseButtons.Right) {
					// black piece
					sender.ImageLocation = theme + "/b" + curpiece.Text + ".png";
					piece[sender.Name as int] = pieceBit;
					color[sender.Name as int] = black;
				}
			}
			else if (selected == null) {
				if (sender.Image != null) {
					selected = sender;
					selected_color = selected.BackColor;
					selected.BackColor = lightcolor;
				}
			}
			else {
				// computer think
				var sqrf = selected.Name as int;
				var sqrto = sender.Name as int;
				global["move_t"] = MOVE_TYPE_NORMAL;
				if (piece[sqrf] == pawn && row(sqrf) == 1 && row(sqrto) == 0) {
					frmPromote.ShowDialog();
				}
				if (userMove(sqrf, sqrto, move_t) == true) {
					
					drawMove(selected, sender, move_t);
					
					/*---------------------*/
					var w = new System.Diagnostics.Stopwatch();
					w.Start();
					
					var bestMove = think(3);
					
					w.Stop();
					f.Text = w.Elapsed.TotalSeconds as String;
					/*---------------------*/
					
					if (bestMove == null) {
						checkmatesound.Play();
						frmwon.Show();
					}
					else {
						makeMove(bestMove);
					
						drawMove( f.Controls[bestMove.from as String] ,
								  f.Controls[bestMove.dest as String] ,
								  bestMove.type);
								  
						if (think(1) == null) {
							// checkmate
							checkmatesound.Play();
							frmlost.Show();
						}
						
						print_board();
					}
				}
				else {
					illegalsound.Play();
				}
				selected.BackColor = selected_color;
				selected = null;
			}
		}
		n++;
		bg = 1 - bg;
		x += 75;
		f.Controls.Add(sqr);
	}
	x = 250;
	y += 75;
	bg = 1 - bg;
}

init_board();
newgamesound.Play();

start(f);