Module Program

    Sub Main()
        ' avoid string to double parsing issues like comma instead of dot
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture

        Dim filepath As String

        If Debugger.IsAttached Then
            filepath = AppDomain.CurrentDomain.BaseDirectory & "debug.js"

            Dim source = File.ReadAllText(filepath)
            Dim engine As New cos8(source)
            engine.run()
        Else
            filepath = Environment.GetCommandLineArgs(1)
            Directory.SetCurrentDirectory(Path.GetDirectoryName(filepath))

            Try
                Dim source = File.ReadAllText(filepath)
                Dim engine As New cos8(source)

                Dim w As New Stopwatch()
                w.Start()

                engine.run()

                w.Stop()

                Console.Write(vbLf & "---------------------------" & vbLf & "Elapsed ")
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine(w.Elapsed.TotalSeconds)
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.Write("Press any key to exit . . .")
                Console.Read()

            Catch ex As Exception
                ExitCode(ex.Message)
            End Try

        End If

    End Sub

End Module