
function add(a, b) {
	return a + b;
}

var c = 0;
for (i = 0; i < 1000000; i++) {
	c = add(c, 1);
}

cout << c;

// elapsed cos8v2.0.5 --> 4.5s
// elapsed cos8v2 --> 7s
// elapsed cos8v1 --> 45s