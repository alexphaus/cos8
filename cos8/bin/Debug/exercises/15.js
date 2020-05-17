/*
Question 15
Level 2

Question:
Write a program that computes the value of a+aa+aaa+aaaa with a given digit as the value of a.
Suppose the following input is supplied to the program:
9
Then, the output should be:
11106

Hints:
In case of input data being supplied to the question, it should be assumed to be a console input.
*/

a = scan();

n1 = a as int;
n2 = a.repeat(2) as int;
n3 = a.repeat(3) as int;
n4 = a.repeat(4) as int;

cout << n1 + n2 + n3 + n4;
scan();