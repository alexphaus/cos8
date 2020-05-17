Module std

    Public Const Microsoft_NET_v4 As String = "C:/Windows/Microsoft.NET/Framework/v4.0.30319/"
    Public libpath As String = AppDomain.CurrentDomain.BaseDirectory & "lib\"
    Public Const js As String = ".js"

    ' for parsing
    Public whitespaces As New Regex("(?<!(?<!\.)\b(from|import|function|return|new|class|public|get|set|var|case|throw|goto))\s+", RegexOptions.Compiled)
    Public comments As New Regex("//.*", RegexOptions.Compiled)
    Public caselines As New Regex("case .*?:", RegexOptions.Compiled)
    Public assignop As New Regex("[*/^+-]=", RegexOptions.Compiled)

    ' for execution
    Enum constants
        null
        break
        [continue]
        [default]
    End Enum

    ' for evaluation
    Public Const arith As String = "∆^*/%+-«»<>≤≥∫≡≠&|!§‖?:="
    Public precedence() As Integer = {0, 1, 2, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 6, 7, 7, 8, 9, 10, 11, 12, 13, 13, 14}
    Public iscall As New Regex("^\w+\(", RegexOptions.Compiled)
    Public isvar As New Regex("^\w+(\[.*\])?$", RegexOptions.Compiled) '^\w+$
    Public isadr As New Regex("^#\d+$", RegexOptions.Compiled)

    ' temporal data
    Public tmp As Object
    Public fx As Func(Of Object(), Object)

    ' error info provider
    Public breakpoint As State
    Public fxname As String = "(global scope)"

    <Extension()>
    Function before(str As String, c As Char) As String
        Return str.Substring(0, str.IndexOf(c))
    End Function

    <Extension()>
    Function after(str As String, c As Char) As String
        Return str.Substring(str.IndexOf(c) + 1)
    End Function

    <Extension()>
    Function shift(str As String) As String
        Return str.Substring(1)
    End Function

    <Extension()>
    Function pop(str As String) As String
        Return str.Substring(0, str.Length - 1)
    End Function

    <Extension()>
    Function starts(str As String, value As String) As Boolean
        Return str.StartsWith(value, StringComparison.Ordinal)
    End Function

    <Extension()>
    Function ends(str As String, value As String) As Boolean
        Return str.EndsWith(value, StringComparison.Ordinal)
    End Function

    <Extension()>
    Function splits(src As String, c As Char) As String()
        Dim u As Integer = 0
        For i = 0 To src.Length - 1
            Select Case src(i)
                Case "("c, "["c : u += 1
                Case ")"c, "]"c : u -= 1
                Case c
                    If u = 0 Then src = src.Remove(i, 1).Insert(i, "‡")
            End Select
        Next
        Return src.Split({"‡"c}, StringSplitOptions.RemoveEmptyEntries)
    End Function

    <Extension()>
    Function address(str As String) As Integer
        Return CInt(str.Substring(str.LastIndexOf("#"c) + 1))
    End Function

    <Extension()>
    Function inside(str As String, startChar As Char, endChar As Char, Optional startIndex As Integer = 0) As String
        Dim u, pos As Integer
        For c = startIndex To str.Length - 1
            Select Case str(c)
                Case startChar
                    If u = 0 Then pos = c + 1
                    u += 1
                Case endChar
                    u -= 1
                    If u = 0 Then Return str.Substring(pos, c - pos)
            End Select
        Next
    End Function

    Sub ExitCode(exception As String, ParamArray args() As Object)
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine(vbLf & "ERRORS")
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("   " & exception.Replace("\n", vbLf) & vbLf, args)
        Console.WriteLine(">> State: [" & breakpoint.type.ToString.ToUpper & "]")
        Console.WriteLine("   significant line: " & breakpoint.line)
        Console.WriteLine("   function name: " & fxname)
        Console.ForegroundColor = ConsoleColor.White
        Console.Write(vbLf & "Press any key to exit . . .")
        Console.Beep()
        Console.ReadKey()
        End
    End Sub

End Module