/*
 * IL benchmark tests
 * archive records
 */

function bar() {
	for (i = 0, 10000000) {
		
	}
}

<il.emit>
function foo() {
	ld_i 0;
	stloc 0; // i
	br 8;
	// body
	nop;
	// i++
	ldloc 0;
	ld_i 1;
	add;
	stloc 0;
	// i < 1000000
	ldloc 0;
	ld_i 10000000;
	clt;
	// loop
	brtrue 3;
}

foo();

/* results:
   bar -> 11.84s
   foo (IL) -> 7.85s
*/