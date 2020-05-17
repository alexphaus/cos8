/*
 * lib::stdlib.js v2.0 Jan 2019
 * standard library (mscorlib)
 */

/************************
         GLOBALS
 ************************/
import mscorlib;
import System;

/* primitive types */
int = System.Int32;
float = System.Double;
char = System.Char;
bool = System.Boolean;
String = System.String;
Object = System.Object;

/* store data */
Dict = System.Collections.Hashtable;
List = System.Collections.Generic.List~1[Object];

Math = System.Math;
//filepath = System.Environment.GetCommandLineArgs()[1];
endl = System.Environment.NewLine;

/************************
         KERNEL
 ************************/

function exit() {
	System.Environment.Exit(0);
}

function print(msg) {
	System.Console.WriteLine(msg);
}

function scan() {
	return System.Console.ReadLine();
}

function read(path) {
	return System.IO.File.ReadAllText(path);
}

function write(path, content) {
	return System.IO.File.WriteAllText(path, content);
}

static class cout
{
	// print string without quotes
	showqts = false;

	tokf(item) {
		if (item == null) item = "null";
		else if (item instanceof String && showqts)
			item = "'" + item + "'";
		else if (item instanceof List)
			item = listf(item);
		else if (item instanceof Dict)
			item = dictf(item);
		else
			item = item.ToString();
		return item;
	}

	dictf(dct) {
		showqts = true;
		var str = "";
		foreach (item in dct)
			str += tokf(item.Key) + ': ' + tokf(item.Value) + ', ';
		return str != "" ? '{' + str.slice(0, -2) + '}' : "{}" ;
	}

	listf(lst) {
		showqts = true;
		var str = "";
		foreach (item in lst)
			str += tokf(item) + ', ';
		return str != "" ? '[' + str.slice(0, -2) + ']' : "[]" ;
	}

	operator <<(value) {
		showqts = false;
		System.Console.Write(tokf(value));
		return this;
	}
}

/************************
         ARRAY
 ************************/

// join all elements from array to string
<extension>
function join(lst, separator) {
	return String.Join(separator, lst.ToArray());
}

// remove first item from array or string
<extension>
function shift(lst) {
	if (lst instanceof String)
		return lst.Substring(1);
	else {
		lst.RemoveAt(0);
		return lst;
	}
}

// remove last item from array or string
<extension>
function pop(lst) {
	if (lst instanceof String)
		return lst.Substring(0, lst.Length - 1);
	else {
		lst.RemoveAt(lst.Count - 1);
		return lst;
	}
}

function array_fill(value, length) {
	var l = [];
	for (var i = 0; i < length; i++)
		l[] = value;
	return l;
}

/************************
         STRING
 ************************/

<extension>
function toFixed(floatNumber, n) {
	return floatNumber.ToString("F" + n);
}

// extracts a text from string
<extension>
function slice(str, start, end) {
	if (start < 0) { // negative index
		start += str.Length;
		if (start < 0) start = 0;
	} else if (start >= str.Length)
		return "";
	if (end == null) // omitted
		end = str.Length;
	else if (end < 0)
		end += str.Length;
	return str.Substring(start, end - start);
}

// get text from start to the first match
<extension>
function before(str, c) {
	return str.Substring(0, str.IndexOf(c));
}

// get text from the first match to the end
<extension>
function after(str, c) {
	return str.Substring(str.IndexOf(c) + 1);
}

Ordinal = System.StringComparison.Ordinal;
// check if string starts with given value
<extension>
function prefix(str, value) {
	return str.StartsWith(value, Ordinal);
}

// check if string ends with given value
<extension>
function postfix(str, value) {
	return str.EndsWith(value, Ordinal);
}

// returns number of occurrences of specific char
<extension>
function count(str, c) {
	var cc = 0; // char count
	c = c as char;
	for (var i = str.Length - 1; i >= 0; i--)
		if (str[i] == c) cc++;
	return cc;
}

// split string by delimiter
<extension>
function split(str, c) {
	var a = str.Split([c as char] as char[]);
	return new List(a);
}

// repeat string number of times
<extension>
function repeat(str, n) {
	var r;
	for (var i = 0; i < n; i++)
		r += str;
	return r;
}
