/*
 * IL benchmark tests
 * archive records
 */

function bar() {
	a = 0;
	b = 1;
	return a + b;
}

<il.emit>
function foo() {
	ld_i 0;
	stloc 0; // a
	ld_i 1;
	stloc 1; // b
	ldloc 0; // a
	ldloc 1; // b
	add;
	ret;
}

for (i = 0, 1000000) {
	foo();
}

/* results:
   bar -> 4.12s
   foo (IL) -> 1.33s
*/