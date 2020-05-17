/*
Question 7
Level 2

Question:
Write a program which takes 2 digits, X,Y as input and generates a 2-dimensional array. The element value in the i-th row and j-th column of the array should be i*j.
Note: i=0,1.., X-1; j=0,1,¡­Y-1.
Example
Suppose the following inputs are given to the program:
3,5
Then, the output of the program should be:
[[0, 0, 0, 0, 0], [0, 1, 2, 3, 4], [0, 2, 4, 6, 8]] 

Hints:
Note: In case of input data being supplied to the question, it should be assumed to be a console input in a comma-separated form.
*/

print("Write 2 digits X,Y");
raw = scan().split(',');
x = raw[0] as int;
y = raw[1] as int;
a = [];

for (i = 0; i < x; i++) {
	b = [];
	for (j = 0; j < y; j++)
		b.Add(i * j);
	a.Add(b);
}

cout << a;

scan();