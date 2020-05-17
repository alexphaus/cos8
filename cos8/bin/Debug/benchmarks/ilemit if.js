/*
 * IL benchmark tests
 * archive records
 */

function bar() {
	x = 5;
	if (x > 0) {
		x = 1;
	}
}

<il.emit>
function foo() {
	ld_i 5;
	stloc 0; // x
	ldloc 0; // x
	ld_i 0;
	cgt;
	brfalse 8;
	ld_i 1;
	stloc 0;
}

for (i = 0, 1000000) {
	bar();
}

/* results:
   bar -> 3.46s
   foo (IL) -> 1.57s
*/