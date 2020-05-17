/*
 * stockfish chess engine
 * Dec 2018 ©, AP
 */

/*
****************************************************************************
* Some definitions                                                         *
****************************************************************************
*/

white = 0;
black = 1;

pawn = 0;
knight = 1;
bishop = 2;
rook = 3;
queen = 4;
king = 5;

empty = 6;
mate = 10000;

value_piece = [100, 300, 310, 500, 900, 10000];

piece = [];
color = [];
side = 0; /* side to move, value = BLACK or WHITE */

col = function(x) { return x & 7 }
row = function(x) { return x >> 3 }

function init_board() {
	side = white;
	piece = [
		3,1,2,4,5,2,1,3,
		0,0,0,0,0,0,0,0,
		6,6,6,6,6,6,6,6,
		6,6,6,6,6,6,6,6,
		6,6,6,6,6,6,6,6,
		6,6,6,6,6,6,6,6,
		0,0,0,0,0,0,0,0,
		3,1,2,4,5,2,1,3
	];
	color = [
		1,1,1,1,1,1,1,1,
		1,1,1,1,1,1,1,1,
		6,6,6,6,6,6,6,6,
		6,6,6,6,6,6,6,6,
		6,6,6,6,6,6,6,6,
		6,6,6,6,6,6,6,6,
		0,0,0,0,0,0,0,0,
		0,0,0,0,0,0,0,0
	];
	for (var i = 0; i < 300; i++)
		hist[i] = new Hist();
}

/* For move generation */
MOVE_TYPE_NONE = 0;
MOVE_TYPE_NORMAL = 1;
MOVE_TYPE_CASTLE = 2;
MOVE_TYPE_EP = 3;
MOVE_TYPE_PROMOTION_TO_QUEEN = 4;
MOVE_TYPE_PROMOTION_TO_ROOK = 5;
MOVE_TYPE_PROMOTION_TO_BISHOP = 6;
MOVE_TYPE_PROMOTION_TO_KNIGHT = 7;

class Move
{
	public from;
	public dest;
	public type;
}

/* For storing all moves of game */
class Hist
{
	public m = new Move();
	public cap;
}

hist = array_fill(null, 300);	/* Game length < 1000 */
hdp = 0;	/* Current move order */

/* For searching */
nodes = 0;	/* Count all visited nodes when searching */
ply = 0;	/* ply of search */

mailbox = [
	-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
	-1,00,01,02,03,04,05,06,07,-1,
	-1,08,09,10,11,12,13,14,15,-1,
	-1,16,17,18,19,20,21,22,23,-1,
	-1,24,25,26,27,28,29,30,31,-1,
	-1,32,33,34,35,36,37,38,39,-1,
	-1,40,41,42,43,44,45,46,47,-1,
	-1,48,49,50,51,52,53,54,55,-1,
	-1,56,57,58,59,60,61,62,63,-1,
	-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
];

mailbox64 = [
	21,22,23,24,25,26,27,28,
	31,32,33,34,35,36,37,38,
	41,42,43,44,45,46,47,48,
	51,52,53,54,55,56,57,58,
	61,62,63,64,65,66,67,68,
	71,72,73,74,75,76,77,78,
	81,82,83,84,85,86,87,88,
	91,92,93,94,95,96,97,98
];

slide = [false, false, true, true, true, false];
offsets = [0, 8, 4, 4, 8, 8];
vector = [
	[0,0,0,0,0,0,0,0],
	[-21,-19,-12,-8,8,12,19,21],
	[-11,-9,9,11,0,0,0,0],
	[-10,-1,1,10,0,0,0,0],
	[-11,-10,-9,-1,1,9,10,11],
	[-11,-10,-9,-1,1,9,10,11]
];

/* * * * * * * * * * * * *
 * Piece Square Tables
 * * * * * * * * * * * * */
/* When evaluating the position we'll add a bonus (or malus) to each piece
 * depending on the very square where it's placed. Vg, a knight in d4 will
 * be given an extra +15, whilst a knight in a1 will be penalized with -40.
 * This simple idea allows the engine to make more sensible moves */

pst_pawn = [
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 15, 15, 0, 0, 0,
  0, 0, 0, 10, 10, 0, 0, 0,
  0, 0, 0, 5, 5, 0, 0, 0,
  0, 0, 0, -25, -25, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0
];

pst_knight = [
  -40, -25, -25, -25, -25, -25, -25, -40,
  -30, 0, 0, 0, 0, 0, 0, -30,
  -30, 0, 0, 0, 0, 0, 0, -30,
  -30, 0, 0, 15, 15, 0, 0, -30,
  -30, 0, 0, 15, 15, 0, 0, -30,
  -30, 0, 10, 0, 0, 10, 0, -30,
  -30, 0, 0, 5, 5, 0, 0, -30,
  -40, -30, -25, -25, -25, -25, -30, -40
];

pst_bishop = [
  -10, 0, 0, 0, 0, 0, 0, -10,
  -10, 5, 0, 0, 0, 0, 5, -10,
  -10, 0, 5, 0, 0, 5, 0, -10,
  -10, 0, 0, 10, 10, 0, 0, -10,
  -10, 0, 5, 10, 10, 5, 0, -10,
  -10, 0, 5, 0, 0, 5, 0, -10,
  -10, 5, 0, 0, 0, 0, 5, -10,
  -10, -20, -20, -20, -20, -20, -20, -10
];

pst_rook = [
  0, 0, 0, 0, 0, 0, 0, 0,
  10, 10, 10, 10, 10, 10, 10, 10,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 5, 5, 0, 0, 0
];

pst_king = [
  -25, -25, -25, -25, -25, -25, -25, -25,
  -25, -25, -25, -25, -25, -25, -25, -25,
  -25, -25, -25, -25, -25, -25, -25, -25,
  -25, -25, -25, -25, -25, -25, -25, -25,
  -25, -25, -25, -25, -25, -25, -25, -25,
  -25, -25, -25, -25, -25, -25, -25, -25,
  -25, -25, -25, -25, -25, -25, -25, -25,
  10, 15, -15, -15, -15, -15, 15, 10
];

/* The flip array is used to calculate the piece/square
values for BLACKS pieces, without needing to write the
arrays for them (idea taken from TSCP).
The piece/square value of a white pawn is pst_pawn[sq]
and the value of a black pawn is pst_pawn[flip[sq]] */
flip = [
  56, 57, 58, 59, 60, 61, 62, 63,
  48, 49, 50, 51, 52, 53, 54, 55,
  40, 41, 42, 43, 44, 45, 46, 47,
  32, 33, 34, 35, 36, 37, 38, 39,
  24, 25, 26, 27, 28, 29, 30, 31,
  16, 17, 18, 19, 20, 21, 22, 23,
  8, 9, 10, 11, 12, 13, 14, 15,
  0, 1, 2, 3, 4, 5, 6, 7
];

/*
****************************************************************************
* Move generator                                                           *
* Lack: no enpassant, no castle                                            *
****************************************************************************
*/

function gen_push(from, dest, type, buffer) {
	var m = new Move();
	m.from = from;
	m.dest = dest;
	m.type = type;
	buffer[] = m;
}

function gen_push_pawn(from, dest, buffer) {
	if (dest > 7 && dest < 56)
		gen_push(from, dest, MOVE_TYPE_NORMAL, buffer);
	else {
		gen_push(from, dest, MOVE_TYPE_PROMOTION_TO_QUEEN, buffer);
		gen_push(from, dest, MOVE_TYPE_PROMOTION_TO_ROOK, buffer);
		gen_push(from, dest, MOVE_TYPE_PROMOTION_TO_BISHOP, buffer);
		gen_push(from, dest, MOVE_TYPE_PROMOTION_TO_KNIGHT, buffer);
	}
}

/* generate all possible movements for a side */
function gen(s) {
	var buffer = [];
	var i, n, j;
	
	for (i = 0; i < 64; i++) {
		if (color[i] == s) {
			if (piece[i] == pawn) {
				if (s == white) {
					if (col(i) != 0 && color[sq = i - 9] == black)
						gen_push(i, sq, MOVE_TYPE_NORMAL, buffer);
					if (col(i) != 7 && color[sq = i - 7] == black)
						gen_push(i, sq, MOVE_TYPE_NORMAL, buffer);
					if (color[sq = i - 8] == empty) {
						gen_push_pawn(i, sq, buffer);
						if (i >= 48 && color[sq = i - 16] == empty)
							gen_push(i, sq, MOVE_TYPE_NORMAL, buffer);
					}
				}
				else {
					if (col(i) != 0 && color[sq = i + 7] == white)
						gen_push(i, sq, MOVE_TYPE_NORMAL, buffer);
					if (col(i) != 7 && color[sq = i + 9] == white)
						gen_push(i, sq, MOVE_TYPE_NORMAL, buffer);
					if (color[sq = i + 8] == empty) {
						gen_push_pawn(i, sq, buffer);
						if (i <= 15 && color[sq = i + 16] == empty)
							gen_push(i, sq, MOVE_TYPE_NORMAL, buffer);
					}
				}
			}
			else {
				for (j = offsets[piece[i]] - 1; j >= 0; j--) {
					n = i;
					while (true) {
						n = mailbox[mailbox64[n] + vector[piece[i]][j]];
						if (n == -1)
							break;
						if (color[n] != empty) {
							if (color[n] == 1-s)
								gen_push(i, n, MOVE_TYPE_NORMAL, buffer);
							break;
						}
						gen_push(i, n, MOVE_TYPE_NORMAL, buffer);
						if (!slide[piece[i]])
							break;
					}
				}
			}
		}
	}
	return buffer;
}

function search(alpha, beta, depth, bestMove) {
	var value;
	var canMove = false;
	nodes++;

	foreach (var m in gen(side)) {

		if (makeMove(m)) {
			takeBack(); // illegal move
			continue;
		}

		canMove = true;
		if (depth-1 > 0)
			value = -search(-beta, -alpha, depth-1);
		else
			value = eval();

		takeBack();

		if (value > alpha) {
			if (value >= beta)
				return beta; // this move is so good
			alpha = value;
			bestMove = m;
		}
	}
	if (!canMove) {
		// checkmate or stalemate
		if (check(side))
			return -mate + ply;
		else
			return 0;
	}
	return alpha;
}

function eval() {
	var i, score = 0;
	
	for (i = 0; i < 64; i++) {
		if (color[i] == white) {
			score += value_piece[piece[i]];
			switch (piece[i]) {
				case pawn:
					score += pst_pawn[i];
				case knight:
					score += pst_knight[i];
				case bishop:
					score += pst_bishop[i];
				case rook:
					score += pst_rook[i];
				case king:
					score += pst_king[i];
			}
		}
		else if (color[i] == black) {
			score -= value_piece[piece[i]];
			switch (piece[i]) {
				case pawn:
					score -= pst_pawn[flip[i]];
				case knight:
					score -= pst_knight[flip[i]];
				case bishop:
					score -= pst_bishop[flip[i]];
				case rook:
					score -= pst_rook[flip[i]];
				case king:
					score -= pst_king[flip[i]];
			}
		}
	}
	
	if (side == white)
		return -score;
	
	return score;
}

function check(s) {
	for (var i = 0; i < 64; i++)
		if (piece[i] == king && color[i] == s)
			return attack(i, 1-s);
}

function attack(sq, s) {
	var i, n, j, p;
	for (i = 0; i < 64; i++) {
		if (color[i] == s) {
			p = piece[i];
			if (p == pawn) {
				if (col(i) && i + (s ? 7 : -9) == sq)
					return true;
				if (col(i) != 7 && i + (s ? 9 : -7) == sq)
					return true;
			}
			else {
				for (j = offsets[p] - 1; j >= 0; j--) {
					n = i;
					while (true) {
						n = mailbox[mailbox64[n] + vector[p][j]];
						if (n == -1)
							break;
						if (n == sq)
							return true;
						if (color[n] != empty)
							break;
						if (!slide[p])
							break;
					}
				}
			}
		}
	}
	return false;
}

function makeMove(m) {
	hist[hdp].m = m;
	hist[hdp].cap = piece[m.dest];

	piece[m.dest] = piece[m.from];
	piece[m.from] = empty;

	color[m.dest] = color[m.from];
	color[m.from] = empty;

	switch (m.type) {
		case MOVE_TYPE_PROMOTION_TO_QUEEN:
			piece[m.dest] = queen;
		case MOVE_TYPE_PROMOTION_TO_ROOK:
			piece[m.dest] = rook;
		case MOVE_TYPE_PROMOTION_TO_BISHOP:
			piece[m.dest] = bishop;
		case MOVE_TYPE_PROMOTION_TO_KNIGHT:
			piece[m.dest] = knight;
	}

	ply++;
	hdp++;
	var r = check(side);
	side = 1-side;
	return r;
}

function takeBack(m) {
	side = 1-side;
	hdp--;
	ply--;
	
	var ht = hist[hdp];
	var f = ht.m.from;
	var to = ht.m.dest;
	
	piece[f] = piece[to];
	piece[to] = ht.cap;

	color[f] = side;
	color[to] = ht.cap != empty ? 1-side : empty;

	if (ht.m.type >= 4)
		piece[f] = pawn;
}

function think(depth) {
	var m;
	ply = 0;
	nodes = 0;
	search(-mate, mate, depth, &m);
	if (m != null) {
		System.Console.WriteLine("bestmove {0}{1}{2}{3}", [(97 + col(m.from)) as char, 8 - row(m.from),
			(97 + col(m.dest)) as char, 8 - row(m.dest)].ToArray());
		System.Console.WriteLine("Nodes: " + nodes);
	}
	return m;
}

function print_board() {
	piece_name = "PNBRQKpnbrqk";
	for (var i = 0; i < 64; i++) {
		if ((i & 7) == 0) {
			cout << "   +---+---+---+---+---+---+---+---+" << endl;
			if (i <= 56)
				cout << " " << 8 - (i >> 3) << " |";
		}
		if (piece[i] == empty)
			cout << "   |";
		else
			cout << " " << piece_name[piece[i] + (color[i] == white ? 0 : 6)] << " |";
		if ((i & 7) == 7)
			cout << endl;
	}
	cout << "   +---+---+---+---+---+---+---+---+" << endl;
}

function userMove(from, dest, type) {
	foreach (var m in gen(side)) {
		if ((m.from == from) && (m.dest == dest) && (m.type == type)) {
			if (makeMove(m)) {
				takeBack(); // illegal move
				return false;
			}
			return true;
		}
	}
}
