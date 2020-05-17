/*
Question 13
Level 2

Question:
Write a program that accepts a sentence and calculate the number of letters and digits.
Suppose the following input is supplied to the program:
hello world! 123
Then, the output should be:
LETTERS 10
DIGITS 3

Hints:
In case of input data being supplied to the question, it should be assumed to be a console input.
*/

sentence = scan();
letters = 0;
digits = 0;

foreach (c in sentence) {
	if (char.IsLetter(c))
		letters++;
	else if (char.IsDigit(c))
		digits++;
}

cout << "LETTERS " + letters + endl 
	  + "DIGITS " + digits;
scan();