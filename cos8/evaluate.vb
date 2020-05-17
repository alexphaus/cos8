
Enum optor
    cast
    pow
    mult
    div
    [mod]
    add
    [sub]
    lshift
    rshift
    less
    greater
    less_eq
    greater_eq
    instanceof
    eq
    not_eq
    [and]
    [or]
    [not]
    [andalso]
    [orelse]
    if_T
    if_F
    assign
End Enum

Enum exp_t
    operation
    variable
    invoker
    constructor
    capture
    array
    hierarchy
    pointer
    hxi
End Enum

Class Unit
    Public type As exp_t

    Public str As String
    Public c As Boolean
    Public i As Integer

    Public arg As Object()
    Public idx As Object()

    Public obj As Object
    Public val As Object
    Public op As optor

    Public ac As Unit()
End Class

Class pointer
    Public var As String
    Public sto As storage
End Class

Class nspace : Inherits Dictionary(Of String, Object)
    Public name As String
End Class

Partial Class cos8
    ''' <summary>
    ''' Convert string to object expression
    ''' </summary>
    Private Function parse(expr As String)

        If expr.starts("&") Then
            Return New Unit With {
                .type = exp_t.pointer,
                .str = expr.shift()
            }
        End If

        Dim values As New List(Of Object)
        Dim operators As New List(Of optor)
        Dim priority As New List(Of Integer)
        Dim token As New StringBuilder
        Dim depth As Integer = 0
        Dim last_s As Integer = -1
        Dim s As Integer
        Dim c As Char

        For i = 0 To expr.Length - 1
            c = expr(i)
            If c = "("c OrElse c = "["c Then
                depth += 1
            ElseIf c = ")"c OrElse c = "]"c Then
                depth -= 1
            ElseIf depth = 0 Then
                s = arith.IndexOf(c)
                If s <> -1 Then ' is operator
                    If s = optor.sub And last_s = i - 1 Then
                        GoTo Append ' unary minus
                    End If
                    values.Add(parsetok(token.ToString))
                    operators.Add(s)
                    priority.Add(precedence(s))
                    token.Clear()
                    last_s = i
                    Continue For
                End If
            End If
Append:     token.Append(c)
        Next
        ' add last token
        values.Add(parsetok(token.ToString))

        While operators.Count
            s = priority.IndexOf(priority.Min)

            Dim oper As New Unit With {
                .type = exp_t.operation,
                .obj = values(s),
                .val = values(s + 1),
                .op = operators(s)
            }
            ' special case for ternary
            If oper.op = optor.if_T Then
                oper.val = New Object() {oper.val, values(s + 2)}
                values.RemoveAt(s + 2)
                operators.RemoveAt(s + 1)
                priority.RemoveAt(s + 1)
            End If

            values(s) = oper
            values.RemoveAt(s + 1)
            operators.RemoveAt(s)
            priority.RemoveAt(s)

        End While

        Return values(0)

    End Function

    ''' <summary>
    ''' objectify token
    ''' </summary>
    Private Function parsetok(tok As String)

        Dim x As Object ' any value

        If String.IsNullOrEmpty(tok) Then
            Return Nothing

        ElseIf Integer.TryParse(tok, x) Then ' int
            Return x

        ElseIf Double.TryParse(tok, x) Then ' double
            Return x

        ElseIf Boolean.TryParse(tok, x) Then ' boolean
            Return x

        ElseIf tok = "null" Then ' null
            Return Nothing

        ElseIf tok = "default" Then ' for switch
            Return constants.default

        ElseIf tok.starts("new ") Then
            Return New Unit With {
                .type = exp_t.constructor,
                .obj = parse(tok.Substring(4, tok.IndexOf("("c) - 4)),
                .arg = parsearg(tok)
            }
        ElseIf tok.starts("function(") Then
            Return create_function(tok, "(Anonymous function)")

        ElseIf isadr.IsMatch(tok) Then ' hashtable
            Return MEM(tok.address)

        ElseIf tok.starts("\#") Then ' char
            Return MEM(tok.address)(0)

        ElseIf tok.starts("-") Then
            Return New Unit With {
                .type = exp_t.capture,
                .c = True,
                .obj = parse(tok.shift)
            }
        ElseIf tok.starts("$") Then
            Return New Unit With {
                .type = exp_t.hxi,
                .i = hexkeys.IndexOf(tok.shift)
            }
        Else
            Dim seqce = tok.splits("."c) ' sequence

            If seqce.Length = 1 Then

                If tok(0) = "["c Then
                    Return New Unit With {
                        .type = exp_t.array,
                        .idx = remove_indexer(tok),
                        .arg = (From item In tok.inside("["c, "]"c).splits(","c) Select parse(item)).ToArray
                    }
                ElseIf tok(0) = "("c Then
                    Return New Unit With {
                        .type = exp_t.capture,
                        .idx = remove_indexer(tok),
                        .obj = parse(tok.inside("("c, ")"c))
                    }
                ElseIf isvar.IsMatch(tok) Then
                    Return New Unit With {
                        .type = exp_t.variable,
                        .idx = remove_indexer(tok),
                        .str = tok
                    }
                ElseIf iscall.IsMatch(tok) Then
                    Return New Unit With {
                        .type = exp_t.invoker,
                        .idx = remove_indexer(tok),
                        .str = tok.before("("c),
                        .arg = parsearg(tok)
                    }
                End If

            Else ' hierarchy syntax; more than 2 accessors

                Dim acc As String = seqce(0)
                Dim lstac As New List(Of Unit)
                Dim hier As New Unit With {
                    .type = exp_t.hierarchy,
                    .obj = parse(acc)
                }
                For i = 1 To seqce.Length - 1
                    acc = seqce(i)

                    Dim ac As New Unit()

                    If acc.ends("]") Then ' fetch indexers
                        ac.idx = remove_indexer(acc)
                    End If

                    If iscall.IsMatch(acc) Then ' method
                        ac.str = acc.before("("c)
                        ac.arg = parsearg(acc)
                        ac.c = True
                    Else ' field
                        ac.str = acc
                    End If

                    lstac.Add(ac)
                Next

                hier.ac = lstac.ToArray

                Return hier

            End If

        End If

    End Function

    ''' <summary>
    ''' evaluate object expression
    ''' </summary>
    Private Function eval(obj)

        If TypeOf obj Is Unit Then
            Dim exp = DirectCast(obj, Unit)

            Select Case exp.type
                Case exp_t.operation
                    If exp.op = optor.andalso Then
                        Return eval(exp.obj) AndAlso eval(exp.val)

                    ElseIf exp.op = optor.orelse Then
                        Return eval(exp.obj) OrElse eval(exp.val)

                    ElseIf exp.op = optor.assign Then
                        Dim key = DirectCast(exp.obj, Unit).str,
                            val = eval(exp.val)
                        locals.lookup_set(key, val)
                        Return val
                    End If

                    Dim left = eval(exp.obj),
                        right = eval(exp.val)

                    ' perform class operator
                    If TypeOf left Is [class] Then
                        If exp.op = optor.instanceof Then
                            Dim clss = DirectCast(left, [class])
                            Dim type = DirectCast(right, _type)
                            Return Equals(clss.type, type)

                        ElseIf exp.op = optor.eq Then
                            Return Equals(left, right)

                        ElseIf exp.op = optor.not_eq Then
                            Return Not Equals(left, right)

                        Else ' any other operator
                            Dim clss = DirectCast(left, [class])
                            If clss.TryGetValue("operator" & arith(exp.op), fx) Then
                                Return fx.Invoke(New Object() {right})
                            End If
                            ExitCode("Not defined operator '{0}' in class '{1}'", arith(exp.op), clss.type.name)
                        End If
                    End If

                    Select Case exp.op
                        Case optor.cast : Return cast(left, right)
                        Case optor.pow : Return left ^ right
                        Case optor.mult : Return left * right
                        Case optor.div : Return left / right
                        Case optor.mod : Return left Mod right
                        Case optor.add : Return If(TypeOf left Is String, left & right, left + right)
                        Case optor.sub : Return left - right
                        Case optor.lshift : Return left << right
                        Case optor.rshift : Return left >> right
                        Case optor.less : Return left < right
                        Case optor.greater : Return left > right
                        Case optor.less_eq : Return left <= right
                        Case optor.greater_eq : Return left >= right
                        Case optor.instanceof : Return Equals(left?.GetType, right)
                        Case optor.eq : Return Equals(left, right)
                        Case optor.not_eq : Return Not Equals(left, right)
                        Case optor.and : Return left And right
                        Case optor.or : Return left Or right
                        Case optor.not : Return Not right
                        Case optor.if_T
                            Dim s = DirectCast(right, Object())
                            Return eval(If(left, s(0), s(1)))
                        Case optor.if_F
                            Return Regex.IsMatch(DirectCast(left, String), DirectCast(right, String))
                    End Select

                Case exp_t.variable

                    Dim value = locals.lookup_get(exp.str)

                    If exp.idx IsNot Nothing Then _
                        apply_selectors(value, exp.idx)

                    Return value

                Case exp_t.hxi

                    Return hexloc(exp.i)

                Case exp_t.pointer
                    Return New pointer With {
                        .var = exp.str,
                        .sto = locals.getdict(.var)
                    }

                Case exp_t.invoker
                    fx = locals.lookup_get(exp.str)
                    Dim value = fx.Invoke(evalarg(exp.arg))

                    If exp.idx IsNot Nothing Then _
                        apply_selectors(value, exp.idx)

                    Return value

                Case exp_t.constructor
                    Dim type = eval(exp.obj)
                    Dim arg = evalarg(exp.arg)

                    If TypeOf type Is _type Then
                        Return create_instance(DirectCast(type, _type), arg)
                    Else ' .NET
                        Return Activator.CreateInstance(DirectCast(type, Type), arg)
                    End If

                Case exp_t.capture
                    Dim value = eval(exp.obj)
                    If exp.c Then value = -value

                    If exp.idx IsNot Nothing Then _
                        apply_selectors(value, exp.idx)

                    Return value

                Case exp_t.array
                    Dim value As Object = New List(Of Object)(evalarg(exp.arg))

                    If exp.idx IsNot Nothing Then _
                        apply_selectors(value, exp.idx)

                    Return value

                Case exp_t.hierarchy
                    Dim r As Object = eval(exp.obj)
                    Dim field As String

                    For Each ac In exp.ac
                        field = ac.str

                        If ac.c Then ' call

                            If TypeOf r Is [class] Then

                                Dim clss = DirectCast(r, [class])
                                If clss.TryGetValue(field, fx) AndAlso clss.type.publics.Contains(field) Then
                                    r = fx.Invoke(evalarg(ac.arg))
                                Else
                                    ExitCode("Not found method '{0}' in class '{1}'",
                                             field, clss.type.name)
                                End If

                            ElseIf extfuncs.Contains(field) Then ' extension

                                Dim arg = evalarg(ac.arg)
                                Dim thisarg(arg.Length) As Object
                                thisarg(0) = r
                                Array.Copy(arg, 0, thisarg, 1, arg.Length)
                                If globals.TryGetValue(field, fx) Then
                                    r = fx.Invoke(thisarg)
                                End If

                            Else ' .NET

                                Dim m As MethodInfo
                                Dim len = ac.arg.Length - 1
                                Dim arg(len) As Object
                                Dim types(len) As Type

                                For i = 0 To len
                                    arg(i) = eval(ac.arg(i))
                                    types(i) = If(arg(i)?.GetType, GetType(Object))
                                Next

                                Dim type = If(TypeOf r Is Type, DirectCast(r, Type), r.GetType)
                                m = type.GetMethod(field, types)

                                If m Is Nothing Then
                                    ExitCode("Inaccessible method '{0}' or bad signature", field)
                                End If

                                r = m.Invoke(r, arg)

                            End If

                        ElseIf TypeOf r Is [class] Then ' property or field

                            Dim clss = DirectCast(r, [class])
                            If clss.TryGetValue("get_" & field, fx) Then
                                r = fx.Invoke(Nothing)

                            ElseIf clss.type.publics.Contains(field) Then
                                r = clss(field)
                            Else
                                ExitCode("Not found public field '{0}' in class '{1}'", field, clss.type.name)
                            End If

                        ElseIf TypeOf r Is nspace Then

                            Dim ns = DirectCast(r, nspace)
                            If ns.TryGetValue(field, tmp) Then
                                r = tmp
                            Else
                                ExitCode("Not found '{0}' in namespace '{1}'", field, ns.name)
                            End If

                        Else ' .NET

                            Dim type = If(TypeOf r Is Type, DirectCast(r, Type), r.GetType)

                            Dim pi = type.GetProperty(field)
                            If pi IsNot Nothing Then ' property
                                r = pi.GetValue(r, Nothing)
                            Else
                                Dim fi = type.GetField(field)
                                If fi IsNot Nothing Then
                                    r = fi.GetValue(r)
                                Else
                                    ExitCode("Couldn't find property or field '{0}' in type '{1}'", field, type.FullName)
                                End If
                            End If

                        End If

                        If ac.idx IsNot Nothing Then _
                            apply_selectors(r, ac.idx)
                    Next

                    Return r

            End Select

        End If

        Return obj

    End Function

    Private Function parsearg(s As String) As Object()
        Return (From e In s.inside("("c, ")"c).splits(","c) Select parse(e)).ToArray
    End Function

    Private Function evalarg(arg() As Object) As Object()
        Dim len = arg.Length - 1
        Dim argv(len) As Object
        For e = 0 To len
            argv(e) = eval(arg(e))
        Next
        Return argv
    End Function

    Private Function remove_indexer(ByRef s As String) As Object()
        Dim r = New List(Of Object)
        Dim depth = 0
        Dim anchor = 0
        Dim begin = -1
        For i = 0 To s.Length - 1
            Select Case s(i)
                Case "("c
                    depth += 1
                Case ")"c
                    depth -= 1
                Case "["c
                    If depth = 0 Then
                        anchor = i + 1
                        If begin = -1 AndAlso i > 0 Then begin = i
                    End If
                    depth += 1
                Case "]"c
                    depth -= 1
                    If depth = 0 AndAlso begin > 0 Then
                        r.Add(parse(s.Substring(anchor, i - anchor)))
                    End If
            End Select
        Next
        If begin <> -1 Then s = s.Substring(0, begin)
        Return If(r.Count = 0, Nothing, r.ToArray)
    End Function

    Private Sub apply_selectors(ByRef obj As Object, indexer() As Object)

        For Each index In indexer

            index = eval(index)

            If TypeOf obj Is List(Of Object) Then
                obj = DirectCast(obj, List(Of Object))(DirectCast(index, Integer))

            ElseIf TypeOf obj Is String Then
                obj = DirectCast(obj, String)(DirectCast(index, Integer))

            ElseIf TypeOf obj Is storage Then
                obj = DirectCast(obj, storage)(DirectCast(index, String))

            ElseIf TypeOf obj Is Type Then
                If index Is Nothing Then ' array type
                    obj = DirectCast(obj, Type).MakeArrayType
                Else ' generic type
                    Dim len = indexer.Length - 1
                    Dim generics(len) As Type
                    generics(0) = DirectCast(index, Type)
                    For e = 1 To len
                        generics(e) = DirectCast(eval(indexer(e)), Type)
                    Next
                    obj = DirectCast(obj, Type).MakeGenericType(generics)
                    Exit Sub
                End If
            Else
                obj = obj(index)
            End If

        Next

    End Sub

End Class