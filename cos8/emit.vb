
Enum opcodes
    ld_i
    ld_str
    ldloc
    stloc
    add
    cgt
    clt
    [call]
    ret
    br
    brfalse
    brtrue
    nop
End Enum

Class OpCode
    Public op As opcodes
    Public i As Integer
    Public f As Double
    Public s As String
    Public c As Boolean
    Public o As Object
    Public fn As Func(Of Object(), Object)
End Class

Partial Class cos8

    Private stack As New Stack(Of Object)

    Private Function create_il_func(s As String) As Func(Of Object(), Object)

        Dim parameters = s.inside("("c, ")"c).Split(","c)

        Dim instructions = MEM(s.address).Split(";"c)
        Dim ilemit As New List(Of OpCode)

        For Each ln In instructions
            Dim opc As New OpCode

            With opc
                If ln = String.Empty Then
                    Continue For

                ElseIf [Enum].TryParse(ln, opc.op) Then
                    ' nothing to do

                ElseIf ln.starts("call") Then
                    .op = opcodes.call
                    .fn = globals(ln.Substring(4))

                ElseIf ln.starts("ld_i") Then
                    .op = opcodes.ld_i
                    .i = Integer.Parse(ln.Substring(4))

                ElseIf ln.starts("ld_str") Then
                    .op = opcodes.ld_str
                    .s = MEM(ln.Substring(6).address)

                ElseIf ln.starts("ldloc") Then
                    .op = opcodes.ldloc
                    .i = Integer.Parse(ln.Substring(5))

                ElseIf ln.starts("stloc") Then
                    .op = opcodes.stloc
                    .i = Integer.Parse(ln.Substring(5))

                ElseIf ln.starts("brfalse") Then
                    .op = opcodes.brfalse
                    .i = Integer.Parse(ln.Substring(7)) - 1

                ElseIf ln.starts("brtrue") Then
                    .op = opcodes.brtrue
                    .i = Integer.Parse(ln.Substring(6)) - 1

                ElseIf ln.starts("br") Then
                    .op = opcodes.br
                    .i = Integer.Parse(ln.Substring(2)) - 1
                End If
            End With

            ilemit.Add(opc)
        Next

        Dim il = ilemit.ToArray()
        Dim len = il.Length - 1
        Dim oc As OpCode
        Dim local(64) As Object

        Return Function(arg() As Object)

                   For i = 0 To len
                       oc = il(i)

                       Select Case oc.op
                           Case opcodes.ld_i
                               stack.Push(oc.i)

                           Case opcodes.ld_str
                               stack.Push(oc.s)

                           Case opcodes.ldloc
                               stack.Push(local(oc.i))

                           Case opcodes.stloc
                               local(oc.i) = stack.Pop

                           Case opcodes.add
                               stack.Push(stack.Pop + stack.Pop)

                           Case opcodes.cgt
                               stack.Push(stack.Pop < stack.Pop)

                           Case opcodes.clt
                               stack.Push(stack.Pop > stack.Pop)

                           Case opcodes.call
                               Dim r = oc.fn(stack.ToArray)
                               stack.Clear()
                               stack.Push(r)

                           Case opcodes.brfalse
                               If Not stack.Pop Then i = oc.i

                           Case opcodes.brtrue
                               If stack.Pop Then i = oc.i

                           Case opcodes.br
                               i = oc.i

                           Case opcodes.ret
                               Return stack.Pop

                           Case opcodes.nop
                               ' nothing to do

                       End Select

                   Next

                   Return Nothing

               End Function

    End Function

End Class