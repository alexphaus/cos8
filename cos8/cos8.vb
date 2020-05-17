'
' cos8 programming language
' ECMAScript © Jan 2019 Alex Phaus
'
Partial Class cos8
    ' storage
    Public globals As New storage()
    Private locals As storage = globals

    Private hexloc(1024) As Object
    Private hexkeys As New List(Of String)

    ' allocate strings, blocks
    Private MEM As New List(Of String)
    Private address As Integer = 0

    Private extfuncs As New HashSet(Of String)
    Private pragma_once As New HashSet(Of String)

    Private script As Task

    Sub New(src As String)

        src = clean(src)
        script = parseCode(src)

        globals.Add("global", globals)
        globals.Add("alert", DirectCast(
            Function(arg() As Object)
                MsgBox(arg(0)?.ToString)
            End Function, Func(Of Object(), Object))
        )

        'Dim w As New Stopwatch()
        'w.Start()
        'Dim pr = parse("1+1")
        'Dim r As Object

        'For i = 0 To 100_000_000
        '    eval(pr)
        'Next

        'w.Stop()
        'Dim elapsed = w.Elapsed.TotalSeconds
        'Stop

    End Sub

    Private Sub eval1()

    End Sub

    ''' <summary>
    ''' remove whitespace, comments, strings, blocks..
    ''' </summary>
    Private Function clean(src As String) As String

        ' allocate strings in memory
        Dim sb, cap As New StringBuilder()
        Dim seek, c As Char

        For i = 0 To src.Length - 1
            c = src(i)
            If seek = Nothing Then
                If c = """"c OrElse c = "'"c Then
                    seek = c
                Else
                    sb.Append(c)
                End If
            Else ' capturing
                If c = seek Then
                    MEM.Add(cap.ToString)
                    cap.Clear()
                    sb.Append("#" & address)
                    address += 1
                    seek = Nothing
                Else
                    cap.Append(c)
                End If
            End If
        Next

        src = sb.ToString()

        ' remove multiline comments
        Dim start, ends As Integer
        While src.Contains("/*")
            start = src.IndexOf("/*")
            ends = src.IndexOf("*/") + 2
            src = src.Remove(start, ends - start)
        End While

        src = comments.Replace(src, "")
        src = src.
            Replace("}", "};").
            Replace("};)", "})").
            Replace(" as ", "∆").
            Replace(" like ", ":").
            Replace(" in ", "·in·").
            Replace(" instanceof ", "∫").
            Replace("==", "≡").
            Replace("!=", "≠").
            Replace("<=", "≤").
            Replace(">=", "≥").
            Replace("<<", "«").
            Replace(">>", "»").
            Replace("&&", "§").
            Replace("||", "‖").
            Replace("~", "`")

        src = caselines.Replace(src, Function(m) m.Value.pop() & ";")
        src = whitespaces.Replace(src, "") ' spaces, tabs, newlines

        ' replace braces {...} to address key ##
        Dim block As String
        While src.IndexOf("{"c) <> -1
            ' last braces
            start = src.LastIndexOf("{"c)
            ends = src.IndexOf("}"c, start) + 1
            block = src.Substring(start + 1, ends - start - 2)
            MEM.Add(block)
            src = src.Remove(start, ends - start)
            src = src.Insert(start, "#" & address)
            address += 1
        End While

        Return src

    End Function

    Private Function create_function(line As String, name As String) As Func(Of Object(), Object)
        ' get parameters names
        Dim parameters = line.inside("("c, ")"c).Split(","c)

        ' get statements reference block by address
        Dim task = parseCode(MEM(line.address))

        Return Function(arg() As Object)

                   Dim fxn = fxname
                   fxname = name

                   Dim lcpy = locals ' copy

                   locals = New storage() With {.previous = globals}
                   locals.Add("arg", arg) ' useful for undefined parameters

                   For i = 0 To parameters.Length - 1
                       locals.Add(parameters(i), If(arg.Length > i, arg(i), Nothing))
                   Next

                   Dim r = execute(task)

                   locals = lcpy
                   fxname = fxn

                   Return r

               End Function

    End Function

    Private Function create_function(task As Task, this As [class], name As String) As Func(Of Object(), Object)

        Return Function(arg() As Object)

                   fxname = name

                   Dim lcpy = locals

                   locals = New storage() With {.previous = this}
                   locals.Add("arg", arg)
                   locals.Add("this", this)

                   For i = 0 To task.parameters.Length - 1
                       locals.Add(task.parameters(i), If(arg.Length > i, arg(i), Nothing))
                   Next

                   Dim r = execute(task)

                   locals = lcpy

                   Return r

               End Function

    End Function

    Private Function create_type(src As String) As _type

        Dim type As New _type With {
            .address = src.address()
        }

        Dim head = src.Substring(6, src.IndexOf("#"c) - 6) 'class ___#1'
        Dim extends As String = ""

        If head.Contains("«") Then ' extends

            Dim s = head.Split("«"c)
            type.name = s(0)
            type.super = globals(s(1))
            extends = MEM(type.super.address)

        Else

            type.name = head

        End If

        Dim lines = (extends & ";" & MEM(type.address)).splits(";"c)
        Dim [public] As Boolean

        For Each ln In lines

            If ln = "" Then
                Continue For
            End If

            [public] = False

            If ln.starts("public ") Then

                [public] = True
                ln = ln.Substring(7)

            End If

            If ln.starts("operator") Then

                GoTo __call__

            ElseIf ln.starts("get ") OrElse ln.starts("set ") Then

                ln = ln.Replace(" ", "_")
                GoTo __call__

            ElseIf iscall.IsMatch(ln) Then

__call__:       Dim fn = ln.before("("c)
                Dim params = ln.inside("("c, ")"c).Split({","c}, StringSplitOptions.RemoveEmptyEntries)
                Dim fx As Task = parseCode(MEM(ln.address))
                fx.parameters = params

                type.xml.Add(fn, fx)
                If [public] Then type.publics.Add(fn)

            Else

                Dim prop As String ' property name
                Dim value As Object = Nothing

                If ln.Contains("=") Then

                    Dim s = ln.Split("="c)
                    prop = s(0)
                    value = parse(s(1))

                Else

                    prop = ln

                End If

                type.xml.Add(prop, value)
                If [public] Then type.publics.Add(prop)

            End If

        Next

        Return type

    End Function

    Private Function create_instance(type As _type, arg() As Object) As [class]

        Dim instance As New [class] With {
            .type = type,
            .previous = globals
        }

        For Each field In type.xml

            If TypeOf field.Value Is Task Then
                ' create function targeting this instance
                instance.Add(field.Key, create_function(field.Value, instance, instance.type.name & "." & field.Key))

            Else
                ' evaluate property value
                instance.Add(field.Key, eval(field.Value))

            End If

        Next

        ' call constructor with arguments
        If instance.TryGetValue("constructor", fx) Then
            fx.Invoke(arg)
        End If

        Return instance

    End Function

    Private Sub import_dll(path As String)
        For Each t In Assembly.LoadFrom(path).GetExportedTypes()
            If t.IsPublic Then add_NET_type(t)
        Next
    End Sub

    Private Sub import_types(dllpath As String, types() As String)
        Dim all As Boolean = types(0) = "*"
        For Each t In Assembly.LoadFrom(dllpath).GetExportedTypes()
            If t.IsPublic Then
                If all OrElse Array.IndexOf(types, t.Name) <> -1 Then
                    If globals.ContainsKey(t.Name) Then
                        add_NET_type(t)
                    Else
                        globals.Add(t.Name, t)
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub add_NET_type(type As Type)
        Dim nsdict As Dictionary(Of String, Object) = globals
        Dim spaces = type.FullName.Split("."c)
        Dim len = spaces.Length - 1
        Dim sp As String

        For i = 0 To len
            sp = spaces(i)

            If nsdict.TryGetValue(sp, tmp) Then
                If TypeOf tmp Is nspace Then
                    nsdict = DirectCast(tmp, nspace)
                End If
            Else
                If i = len Then
                    nsdict.Add(sp, type)
                Else
                    Dim ns As New nspace With {.name = sp}
                    nsdict.Add(sp, ns)
                    nsdict = ns
                End If
            End If
        Next
    End Sub

    Public Sub run()
        ' add standart resources
        Dim src = clean(File.ReadAllText(libpath & "stdlib.js"))
        Dim std = parseCode(src)
        execute(std)

        ' execute script
        execute(script)
    End Sub

    Private Function cast(value As Object, conversionType As Type)
        If conversionType.IsArray Then
            Dim arrayvalue = DirectCast(value, List(Of Object)).ToArray()
            Dim elemType = conversionType.GetElementType()
            Dim destinationArray = Array.CreateInstance(elemType, arrayvalue.Length)
            Array.Copy(arrayvalue, destinationArray, arrayvalue.Length)
            Return destinationArray
        Else
            Return Convert.ChangeType(value, conversionType)
        End If
    End Function

End Class

Class [class] : Inherits storage

    Public type As _type

    Overrides Function ToString() As String
        Return "class+" & type.name
    End Function

End Class

Class _type
    Public xml As New Dictionary(Of String, Object)
    Public publics As New HashSet(Of String)
    Public address As Integer ' MEM address
    Public super As _type
    Public name As String
End Class

Class storage : Inherits Dictionary(Of String, Object)
    ' handle variables from previous scope
    Public previous As storage

    Public Function lookup_get(key As String)
        If getdict(key) IsNot Nothing Then
            If TypeOf tmp Is pointer Then
                Dim ptr = DirectCast(tmp, pointer)
                Return ptr.sto(ptr.var)
            End If
            Return tmp
        End If
        ExitCode("Unknown identifier '{0}'", key)
        Return Nothing
    End Function

    Public Sub lookup_set(key As String, value As Object)
        Dim sto = getdict(key)
        If sto IsNot Nothing Then
            If TypeOf tmp Is pointer Then
                Dim ptr = DirectCast(tmp, pointer)
                ptr.sto(ptr.var) = value
            Else
                sto(key) = value
            End If
        Else
            Add(key, value)
        End If
    End Sub

    Function getdict(key As String) As storage
        If TryGetValue(key, tmp) Then
            Return Me
        Else
            Return previous?.getdict(key)
        End If
    End Function

End Class