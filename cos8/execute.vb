'
' cos8 programming language
' ECMAScript © Jan 2019 Alex Phaus
'
Enum state_t
    [if]
    [elseif]
    [while]
    [for]
    [foreach]
    [break]
    [continue]
    [function]
    [extension]
    [il_func]
    [return]
    [import]
    [from]
    [set_var]
    [set_index]
    [set_property]
    [add_item]
    [lambda]
    [increment]
    [decrement]
    [var]
    [throw]
    [switch]
    [case]
    [class]
    [static_class]
    [goto]
    [set_hxi]
End Enum

Class State
    Public type As state_t

    ' common
    Public expression As Object
    Public task As Task

    ' buffers
    Public str As String
    Public i As Integer
    Public bool As Boolean
    Public obj As Object
    Public buf As Object

    ' error info
    Public line As Integer
End Class

Class Task : Inherits List(Of State)
    ' look for case that matches switch
    Public switch As Object = Constants.null
    ' parameters
    Public parameters As String()
End Class

Partial Class cos8

    Private Sub macro(s As String, state As State)
        ' get condition from parenthesis
        Dim condition = s.inside("("c, ")"c)
        state.expression = parse(condition)

        ' get task address or one-liner
        Dim key = s.IndexOf("("c) ' skip keyword
        Dim block = s.Substring(key + 1 + condition.Length + 1)

        If block.starts("#") Then
            state.task = parseCode(MEM(block.address)) ' block address
        Else
            state.task = parseCode(block)
        End If
    End Sub

    '* preprocess statements for fast performance
    Private Function parseCode(src As String) As Task
        Dim result As New Task()
        Dim c As Task
        Dim lines = src.splits(";"c)

        For Each s In lines
            Dim state As New State ' new instance of state()
            state.line = result.Count + 1

            If s.starts("if(") Then
                state.type = state_t.if
                macro(s, state)

            ElseIf s.starts("elseif(") Then
                state.type = state_t.elseif
                macro(s, state)

            ElseIf s.starts("else") Then
                state.type = state_t.elseif
                state.expression = True
                state.task = parseCode(If(s(4) = "#"c, MEM(s.address), s.Substring(4)))

            ElseIf s.starts("while(") Then
                state.type = state_t.while
                macro(s, state)

            ElseIf s.starts("for(") Then
                If Not s.Contains(";") Then
                    ' direct for
                    state.type = state_t.for

                    Dim pos = s.inside("("c, ")"c).Length + 5
                    Dim range = s.Substring(4, pos - 5).splits(","c)

                    Dim iter = range(0).before("="c).shift()
                    Dim start = parse(range(0).after("="c))
                    Dim [end] = parse(range(1))
                    Dim [step] = If(range.Length = 3, parse(range(2)), 1)

                    With state
                        '.str = iter
                        .i = hexkeys.IndexOf(iter)
                        If .i = -1 Then
                            .i = hexkeys.Count
                            hexkeys.Add(iter)
                        End If
                        .expression = start
                        .obj = [end]
                        .buf = [step]
                    End With

                    state.task = parseCode(If(s(pos) = "#"c, MEM(s.address), s.Substring(pos)))
                Else
                    ' transform to while syntax
                    state.type = state_t.while
                    Dim pos = s.inside("("c, ")"c).Length + 5
                    Dim range = s.Substring(4, pos - 5).Split(";"c)
                    Dim iterator = parseCode(range(0))(0)
                    Dim steps = parseCode(range(2))
                    Call If(c, result).Add(iterator)
                    state.expression = parse(range(1))
                    state.task = parseCode(If(s(pos) = "#"c, MEM(s.address), s.Substring(pos)))
                    state.task.Add(steps(0))
                    state.bool = True ' which means, if continue, then execute steps
                    state.obj = steps ' for continue
                End If

            ElseIf s.starts("foreach(") Then
                state.type = state_t.foreach
                Dim pos = s.inside("(", ")").Length + 9
                Dim col = s.Substring(8, pos - 9)
                If col.starts("var ") Then
                    state.bool = True
                    col = col.Substring(4)
                End If
                Dim spt = col.IndexOf("·in·")
                state.str = col.Substring(0, spt) ' iterator
                state.expression = parse(col.Substring(spt + 4))
                state.task = parseCode(If(s(pos) = "#"c, MEM(s.address), s.Substring(pos)))

            ElseIf s.starts("switch(") Then
                state.type = state_t.switch
                macro(s, state)

            ElseIf s.starts("case ") Then
                state.type = state_t.case
                state.expression = parse(s.Substring(5)) ' data to evaluate
                state.task = New Task()
                result.Add(state)
                ' task container
                c = state.task
                Continue For

            ElseIf s.starts("function ") Then
                state.type = state_t.function
                state.str = s.Substring(9).before("("c) ' name of the function
                state.obj = create_function(s, state.str)

            ElseIf s.starts("<extension>") Then
                state.type = state_t.extension
                state.str = s.Substring(20).before("("c) ' name of the function
                state.obj = create_function(s, state.str)

            ElseIf s.starts("<il.emit>") Then
                state.type = state_t.il_func
                state.str = s.Substring(18).before("("c) ' name of the function
                state.obj = s

            ElseIf s = "return" Then
                state.type = state_t.return
                state.expression = Nothing

            ElseIf s.starts("return ") Then
                state.type = state_t.return
                state.expression = parse(s.Substring(7)) ' data to evaluate

            ElseIf s.starts("throw ") Then
                state.type = state_t.throw
                state.expression = parse(s.Substring(6))

            ElseIf s.starts("import ") Then
                state.type = state_t.import
                Dim package = s.Substring(7)

                If package.starts("#") Then ' string
                    state.str = MEM(package.address)

                ElseIf File.Exists(package & js) Then
                    state.str = package & js

                ElseIf File.Exists(libpath & package & js) Then
                    state.str = libpath & package & js

                ElseIf File.Exists(Microsoft_NET_v4 & package & ".dll") Then
                    state.str = Microsoft_NET_v4 & package & ".dll"
                Else
                    ExitCode("Couldn't import '{0}'. Path not found.", package)
                End If

                If state.str.ends(".dll") Then
                    state.bool = True ' .NET library
                Else ' .js
                    Dim plugin = clean(File.ReadAllText(state.str))
                    state.task = parseCode(plugin)
                End If

            ElseIf s.starts("from ") Then
                state.type = state_t.from
                Dim package = s.Substring(5, s.IndexOf("import ") - 5)
                state.obj = s.Substring(s.LastIndexOf(" "c) + 1).Split(","c) ' types
                state.str = Microsoft_NET_v4 & package & ".dll"

            ElseIf s.starts("var ") Then
                s = s.Substring(4)
                For Each tok In s.splits(","c)
                    Dim var As New State With {.type = state_t.var}
                    If tok.Contains("=") Then
                        var.str = tok.before("="c)
                        var.expression = parse(tok.after("="c))
                    Else
                        var.str = tok
                        var.expression = Nothing
                    End If
                    Call If(c, result).Add(var)
                Next
                Continue For

            ElseIf s = "break" Then
                state.type = state_t.break

            ElseIf s = "continue" Then
                state.type = state_t.continue

            ElseIf s.starts("class ") Then
                state.type = state_t.class
                'state.obj = create_type(s)
                Dim type = create_type(s)
                globals.Add(type.name, type)

            ElseIf s.starts("staticclass ") Then
                ' class non-inheritable
                state.type = state_t.static_class
                s = s.Substring(6)
                state.obj = create_type(s)

            ElseIf s.ends("++") Then
                state.type = state_t.increment
                state.str = s.Substring(0, s.Length - 2)

                If state.str.starts("$") Then
                    state.bool = True
                    state.i = hexkeys.IndexOf(state.str.shift)
                End If

            ElseIf s.ends("--") Then
                state.type = state_t.decrement
                state.str = s.Substring(0, s.Length - 2)

            ElseIf assignop.IsMatch(s) Then
                Dim eqpos = s.IndexOf("="c)
                Dim leftside = s.Substring(0, eqpos - 1)
                Dim rightside = s.Substring(eqpos + 1)
                Dim op = s(eqpos - 1)
                s = leftside & "=" & leftside & op & rightside
                GoTo EQUAL

            ElseIf s.IndexOf("="c) <> -1 Then
EQUAL:          Dim keys() As String = s.before("="c).splits("."c)
                Dim key As String = keys(0)
                state.expression = parse(s.after("="c))
                If keys.Length = 1 Then
                    If key.Contains("[") Then ' indexer
                        ' get position of index
                        Dim pos, u As Integer
                        For i = key.Length - 1 To 0 Step -1
                            Select Case key(i)
                                Case "]"c : u += 1
                                Case "["c : u -= 1
                                    If u = 0 Then
                                        pos = i
                                        Exit For
                                    End If
                            End Select
                        Next
                        state.obj = parse(key.Substring(0, pos)) ' array to evaluate
                        Dim selector = key.Substring(pos + 1).pop
                        If selector = "" Then
                            ' syntax for adding item to array
                            state.type = state_t.add_item
                        Else
                            state.type = state_t.set_index
                            state.buf = parse(selector) ' selector to evaluate
                        End If
                    Else ' literal
                        If key.starts("$") Then
                            state.type = state_t.set_hxi
                            Dim var = key.shift()
                            Dim pos = hexkeys.IndexOf(var)
                            If pos <> -1 Then
                                state.i = pos
                            Else
                                state.i = hexkeys.Count
                                hexkeys.Add(var)
                            End If
                        Else
                            state.type = state_t.set_var
                            state.str = key
                        End If
                    End If
                Else ' property set
                    state.type = state_t.set_property
                    state.obj = parse(Join(keys.Take(keys.Length - 1).ToArray, "."))
                    state.str = keys.Last
                End If
            Else
                state.type = state_t.lambda
                state.expression = parse(s)
            End If

            Call If(c, result).Add(state)
        Next

        Return result

    End Function

    ''' <summary>
    ''' run a list of instructions and return a value
    ''' </summary>
    ''' <param name="task">block of code</param>
    Private Function execute(task As Task)
        Dim last_if As Boolean
        Dim r As Object
        Dim state As State

        For i = 0 To task.Count - 1
            state = task(i)
            breakpoint = state

            Select Case state.type

                Case state_t.set_var
                    locals.lookup_set(state.str, eval(state.expression))

                Case state_t.set_hxi
                    hexloc(state.i) = eval(state.expression)

                Case state_t.set_index
                    Dim obj = eval(state.obj)
                    Dim index = eval(state.buf)
                    Dim value = eval(state.expression)

                    If TypeOf obj Is List(Of Object) Then
                        DirectCast(obj, List(Of Object))(DirectCast(index, Integer)) = value

                    ElseIf TypeOf obj Is storage Then
                        DirectCast(obj, storage)(DirectCast(index, String)) = value
                    Else
                        obj(index) = value
                    End If

                Case state_t.add_item
                    DirectCast(eval(state.obj), List(Of Object)).Add(eval(state.expression))

                Case state_t.set_property
                    Dim value = eval(state.expression)
                    Dim obj = eval(state.obj)
                    If TypeOf obj Is [class] Then
                        Dim clss = DirectCast(obj, [class])
                        If clss.TryGetValue("set_" & state.str, fx) Then
                            fx.Invoke({value})

                        ElseIf clss.type.publics.Contains(state.str) Then
                            clss(state.str) = value
                        Else
                            ExitCode("Couldn't assign value to field '{0}' in class '{1}'\n   Maybe it's readonly property or it's NOT defined or it's private", state.str, clss.type.name)
                        End If
                    Else ' .NET
                        Dim type = obj.GetType()
                        Dim pi = type.GetProperty(state.str)
                        If pi IsNot Nothing Then
                            pi.SetValue(obj, value, Nothing)
                        Else
                            Dim ei = type.GetEvent(state.str)
                            If ei IsNot Nothing Then
                                Dim lcpy = locals ' import local variables to event function
                                Dim fn = Sub(sender As Object, e As Object) value.Invoke({sender, e, lcpy})
                                Dim handler = [Delegate].CreateDelegate(ei.EventHandlerType, fn.Target, fn.Method)
                                ei.AddEventHandler(obj, handler)
                            Else
                                ExitCode("Couldn't set property because not found '{0}' in type '{1}'", state.str, type.FullName)
                            End If
                        End If
                    End If

                Case state_t.lambda
                    eval(state.expression)

                Case state_t.if
case_if:            last_if = DirectCast(eval(state.expression), Boolean) ' evaluate condition
                    If last_if Then
                        r = execute(state.task) ' should get null
                        If r IsNot Nothing Then Return r
                    End If

                Case state_t.elseif
                    If Not last_if Then GoTo case_if

                Case state_t.increment
                    If state.bool Then
                        hexloc(state.i) = DirectCast(hexloc(state.i), Integer) + 1
                    Else
                        Dim dict = locals.getdict(state.str)
                        dict(state.str) = DirectCast(tmp, Integer) + 1
                    End If

                Case state_t.decrement
                    Dim dict = locals.getdict(state.str)
                    dict(state.str) = DirectCast(tmp, Integer) - 1

                Case state_t.foreach
                    Dim iterator As String = state.str
                    If state.bool Then
                        locals.Add(iterator, Nothing)
                    End If
                    For Each item In eval(state.expression)
                        locals(iterator) = item
                        r = execute(state.task)
                        If r IsNot Nothing Then
                            If TypeOf r Is constants Then
                                Select Case DirectCast(r, constants)
                                    Case constants.break
                                        Exit For
                                    Case constants.continue
                                        ' pass
                                End Select
                            Else
                                Return r
                            End If
                        End If
                    Next

                Case state_t.while
                    While DirectCast(eval(state.expression), Boolean)
                        r = execute(state.task)
                        If r IsNot Nothing Then
                            If TypeOf r Is constants Then
                                Select Case DirectCast(r, constants)
                                    Case constants.break
                                        Exit While
                                    Case constants.continue
                                        If state.bool Then ' for bucle
                                            execute(DirectCast(state.obj, Task))
                                        End If
                                End Select
                            Else
                                Return r
                            End If
                        End If
                    End While

                Case state_t.for
                    Dim start = DirectCast(eval(state.expression), Integer)
                    Dim [end] = DirectCast(eval(state.obj), Integer) - 1
                    Dim [step] = DirectCast(eval(state.buf), Integer)

                    For iter = start To [end] Step [step]
                        hexloc(state.i) = iter
                        r = execute(state.task)

                        If r IsNot Nothing Then
                            If TypeOf r Is constants Then
                                Select Case DirectCast(r, constants)
                                    Case constants.break
                                        Exit For
                                    Case constants.continue
                                        ' pass
                                End Select
                            Else
                                Return r
                            End If
                        End If
                    Next

                Case state_t.switch
                    state.task.switch = eval(state.expression)
                    r = execute(state.task)
                    If r IsNot Nothing Then Return r

                Case state_t.case
                    If Equals(eval(state.expression), task.switch) Then
                        Return execute(state.task)
                    End If

                Case state_t.break
                    Return constants.break

                Case state_t.continue
                    Return constants.continue

                Case state_t.return
                    Return eval(state.expression)

                Case state_t.function
                    globals.Add(state.str, state.obj)

                Case state_t.extension
                    extfuncs.Add(state.str)
                    globals.Add(state.str, state.obj)

                Case state_t.il_func
                    globals.Add(state.str, create_il_func(state.obj))

                Case state_t.throw
                    ExitCode(eval(state.expression))

                Case state_t.class
                    'Dim type = DirectCast(state.obj, _type)
                    'globals.Add(type.name, type)

                Case state_t.static_class
                    Dim obj = create_instance(DirectCast(state.obj, _type), Nothing)
                    globals.Add(obj.type.name, obj)

                Case state_t.import
                    If state.bool Then ' .NET
                        import_dll(state.str)

                    ElseIf Not pragma_once.Contains(state.str) Then ' .js
                        pragma_once.Add(state.str)
                        execute(state.task)
                    End If

                Case state_t.from
                    import_types(state.str, DirectCast(state.obj, String()))

                Case state_t.var
                    locals(state.str) = eval(state.expression)
            End Select
        Next

        Return Nothing

    End Function

End Class