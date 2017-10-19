Imports System.Net.Sockets
Imports System.Threading
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
<Serializable()> _
Public Class mData
    Public xPosition As Integer
    Public yPosition As Integer
    Public eMouseEvent As String
    Public keyBoardEvent As String
    Public keyB As String
    Public stringData As String
    Public fileName As String
End Class
Public Class server

    Dim AbandonedMutexException As IntPtr

    <DllImport("dwmapi.dll", PreserveSig:=False)> _
    Public Shared Function DwmIsCompositionEnabled() As Boolean
    End Function

    <DllImport("dwmapi.dll", PreserveSig:=False)> _
    Public Shared Sub DwmEnableComposition(ByVal bEnable As Boolean)
    End Sub

    <DllImport("user32.dll", EntryPoint:="GetCursorInfo")> _
    Public Shared Function GetCursorInfo(ByRef pci As CURSORINFO) As Boolean
    End Function

    <DllImport("user32.dll", EntryPoint:="CopyIcon")> _
    Public Shared Function CopyIcon(ByVal hIcon As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", EntryPoint:="GetIconInfo")> _
    Public Shared Function GetIconInfo(ByVal hIcon As IntPtr, ByRef piconinfo As ICONINFO) As Boolean
    End Function

    Dim aeroIsEnabled As Boolean
    Dim b As Bitmap

    Private server As TcpListener 'puerto para escuchar
    Private client As New TcpClient
    Private port As Integer
    Public clients As New Hashtable()
    Dim ns As NetworkStream
    Dim listening As New Thread(AddressOf listen)
    Public Event newClient(ByVal countClients As Integer, ByVal IDTerminal As Net.IPEndPoint)
    Public clientAutorized As String
    Public flagNewClient As String = "None"
    Public flagRemoveClient As String = "None"
    Public idEndClient As Net.IPEndPoint 'Ultimo cliente conectado
    Private includeCursorImage As Boolean = True
    Private initListen As Boolean = False
    Private Structure infoCLient
        Public nsStream As NetworkStream
        Public Thread As Thread
        Public id As Net.IPEndPoint
    End Structure

    Property PuertoDeEscucha() As Integer
        Get
            PuertoDeEscucha = port
        End Get
        Set(ByVal Value As Integer)
            port = Value
        End Set
    End Property

    Public Sub listen()
        server.Start()
        While True
            client = server.AcceptTcpClient
            Dim infoCLient As New infoCLient
            With infoCLient
                Try
                    SyncLock Me
                        .nsStream = client.GetStream
                        .Thread = New Thread(AddressOf readMouseData)
                        .id = client.Client.RemoteEndPoint
                        idEndClient = client.Client.RemoteEndPoint
                        clients.Add(idEndClient, infoCLient)
                        'Genero el evento Nueva conexion
                        RaiseEvent newClient(clients.Count, idEndClient)
                        'Inicio el thread encargado de escuchar los mensajes del cliente
                        .Thread.Start()
                        flagNewClient = client.Client.RemoteEndPoint.ToString
                    End SyncLock
                Catch ex As Exception
                End Try
            End With
        End While
    End Sub
    Public Sub start()
        If Not initListen Then
            server = New TcpListener(port)
            listening.Start()
            initListen = True
        End If
    End Sub
    Public Sub readMouseData()
        Try
            While True
                Dim Cliente As New infoCLient
                For Each Cliente In clients.Values
                    ' Debug.WriteLine("Cliente: " & Cliente.id.ToString)
                    If Not IsNothing(clientAutorized) Then
                        If Cliente.id.ToString = Me.clientAutorized Then
                            Try
                                Dim bf As New BinaryFormatter
                                Dim data As New mData
                                ns = Cliente.nsStream
                                Dim obMouse As Hashtable = Nothing
                                Try
                                    obMouse = DirectCast(bf.Deserialize(ns), Hashtable)
                                    ns.Flush()
                                    Dim de As DictionaryEntry
                                    For Each de In obMouse
                                        If de.Key = "xPosition" Then
                                            data.xPosition = de.Value
                                        ElseIf de.Key = "yPosition" Then
                                            data.yPosition = de.Value
                                        ElseIf de.Key = "stringEvent" Then
                                            data.eMouseEvent = de.Value
                                        ElseIf de.Key = "stringData" Then
                                            data.stringData = de.Value
                                        ElseIf de.Key = "fileName" Then
                                            data.fileName = de.Value
                                        ElseIf de.Key = "keyBoardEvent" Then
                                            data.keyBoardEvent = de.Value
                                        ElseIf de.Key = "key" Then
                                            data.keyB = de.Value
                                        End If
                                    Next
                                    Call readMyMouseData(data)
                                    Call readKeyData(data)
                                Catch ex As Exception
                                    clientAutorized = Nothing
                                End Try
                            Catch ex As SerializationException
                                'Debug.Print(ex.Message)
                            End Try
                        End If
                    End If
                Next
            End While
        Catch ex As Exception

        End Try
    End Sub
    Public Function copyDisplay() As Image
        Dim bound As Rectangle = Nothing
        Dim screenShot As System.Drawing.Bitmap = Nothing
        Dim graph As Graphics = Nothing
        bound = Screen.PrimaryScreen.Bounds
        screenShot = New Bitmap(bound.Width, bound.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        graph = Graphics.FromImage(screenShot)
        graph.CopyFromScreen(bound.X, bound.Y, 0, 0, bound.Size, CopyPixelOperation.SourceCopy)
        If includeCursorImage Then
            If aeroIsEnabled Then
                DisableAero()
            End If
            'DESHABILITA EL AERO INVESTIGAR SI ES POSIBLE DEJARLO COMO OPCION DE CONFIG.
            Dim x As Integer
            Dim y As Integer
            Dim cursorBmp As Bitmap = CaptureCursor(x, y)
            If Not IsNothing(cursorBmp) Then
                graph.DrawImage(cursorBmp, x, y)
                cursorBmp.Dispose()
            End If
        End If
        graph.Dispose()
        Return screenShot
    End Function
   
    Private Declare Auto Function GetDesktopWindow Lib "user32.dll" () As IntPtr

    Private Declare Auto Function GetWindowDC Lib "user32.dll" (ByVal windowHandle As IntPtr) As IntPtr

    Private Declare Auto Function ReleaseDC Lib "user32.dll" (ByVal _
    windowHandle As IntPtr, ByVal dc As IntPtr) As Integer

    Private Declare Auto Function BitBlt Lib "gdi32.dll" (ByVal _
    hdcDest As IntPtr, ByVal nXDest As Integer, ByVal _
    nYDest As Integer, ByVal nWidth As Integer, ByVal _
    nHeight As Integer, ByVal hdcSrc As IntPtr, ByVal nXSrc _
    As Integer, ByVal nYSrc As Integer, ByVal dwRop As  _
    System.Int32) As Boolean

    Private Const SRCCOPY As Integer = &HCC0020

    Public Function GetScreenshot(ByVal windowHandle As IntPtr, _
    ByVal location As Point, ByVal size As Size) As Image

        Dim myImage As Image = New Bitmap(size.Width, size.Height)
        Dim g As Graphics = Graphics.FromImage(myImage)
        
        Dim destDeviceContext As IntPtr = g.GetHdc
        Dim srcDeviceContext As IntPtr = GetWindowDC(windowHandle)

        BitBlt(destDeviceContext, 0, 0, size.Width, size.Height, _
               srcDeviceContext, location.X, location.Y, SRCCOPY)
        ReleaseDC(windowHandle, srcDeviceContext)
        g.ReleaseHdc(destDeviceContext)
        If includeCursorImage Then
            If aeroIsEnabled Then
                DisableAero()
            End If
            'DESHABILITA EL AERO INVESTIGAR SI ES POSIBLE DEJARLO COMO OPCION DE CONFIG.
            Dim x As Integer
            Dim y As Integer
            Dim cursorBmp As Bitmap = CaptureCursor(x, y)
            If Not IsNothing(cursorBmp) Then
                g.DrawImage(cursorBmp, x, y)
                cursorBmp.Dispose()
            End If
        End If
        g.Dispose()

        Return myImage
    End Function

    Public Sub sendDisplay()
        If clients.Count > 0 Then
            Try
                Dim Cliente As infoCLient
                Dim bf As New BinaryFormatter
                Dim image As Image = GetScreenshot(AbandonedMutexException, New Point(0, 0), New System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)) 'copyDisplay()
                For Each Cliente In clients.Values
                    idEndClient = Cliente.id
                    ns = Cliente.nsStream
                    Try
                        If ns.CanWrite Then
                            bf.Serialize(ns, image)
                        Else
                            'Call CerrarThread(idEndClient)
                        End If
                    Catch ex As Exception
                        Debug.Print(idEndClient.ToString & " - " & ex.Message.ToString)
                        Call CerrarThread(idEndClient)
                    End Try
                Next
                image.Dispose()
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub CerrarThread(ByVal IDCliente As Net.IPEndPoint)
        If clients.Count > 0 Then
            Dim InfoClientCurrent As infoCLient
            'Cierro el thread que se encargaba de escuchar al cliente especificado
            InfoClientCurrent = clients(IDCliente)
            Try
                InfoClientCurrent.Thread.Abort()
                SyncLock Me
                    'Elimino el cliente del HashArray que guarda la informacion de los clientes
                    clients.Remove(IDCliente)
                    clientAutorized = Nothing
                    flagRemoveClient = IDCliente.ToString
                End SyncLock
            Catch e As Exception
                MessageBox.Show(e.ToString)
            End Try
        End If
    End Sub
    Private Sub DisableAero()
        Try
            aeroIsEnabled = DwmIsCompositionEnabled()
            If aeroIsEnabled = True Then
                DwmEnableComposition(False)
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Declare Sub mouse_event Lib "user32.dll" (ByVal dwFlags As Integer, ByVal dx As Integer, ByVal dy As Integer, ByVal cButtons As Integer, ByVal dwExtraInfo As IntPtr)
    Public Declare Auto Function SetCursorPos Lib "User32.dll" (ByVal X As Integer, ByVal Y As Integer) As Long

    Public Const MOUSEEVENTF_LEFTDOWN = &H2 ' left button down
    Public Const MOUSEEVENTF_LEFTUP = &H4 ' left button up
    Public Const MOUSEEVENTF_MIDDLEDOWN = &H20 ' middle button down
    Public Const MOUSEEVENTF_MIDDLEUP = &H40 ' middle button up
    Public Const MOUSEEVENTF_RIGHTDOWN = &H8 ' right button down
    Public Const MOUSEEVENTF_RIGHTUP = &H10 ' right button up

    Private Sub PerformMouseClick(ByVal LClick_RClick_DClick As String)

        If LClick_RClick_DClick = "RClick" Then
            Debug.Print("Click Derecho")
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, IntPtr.Zero)
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, IntPtr.Zero)
        ElseIf LClick_RClick_DClick = "LClick" Then
            Debug.Print("Click Izquierdo")
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero)
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero)
        ElseIf LClick_RClick_DClick = "DClick" Then
            Debug.Print("DobleClick")
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero)
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero)
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero)
        End If
    End Sub

    Private Sub fileCreate(ByRef data As mData)
        Try
            Debug.Print(data.fileName)
            Debug.Print(data.stringData)
            If data.fileName <> "" Then
                Dim fileName As String = Path.GetFileName(data.fileName)
                Dim _path As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "/" & fileName
                ' Create or overwrite the file.
                Dim fs As FileStream = File.Create(_path)

                ' Add text to the file.
                Dim info As Byte() = New UTF8Encoding(True).GetBytes(data.stringData)
                fs.Write(info, 0, info.Length)
                fs.Close()
            End If
        Catch ex As Exception
            MessageBox.Show("Sucedio un error mientras se intentaba copiar el archivo: " & ex.Message.ToString)
        End Try
    End Sub

    Private Shared Function CaptureCursor(ByRef x As Integer, ByRef y As Integer) As Bitmap
        Dim bmp As Bitmap
        Dim hicon As IntPtr
        Dim ci As New CURSORINFO()
        Dim icInfo As ICONINFO
        ci.cbSize = Marshal.SizeOf(ci)
        If GetCursorInfo(ci) Then
            hicon = CopyIcon(ci.hCursor)
            If GetIconInfo(hicon, icInfo) Then
                x = ci.ptScreenPos.X - CInt(icInfo.xHotspot)
                y = ci.ptScreenPos.Y - CInt(icInfo.yHotspot)
                Dim ic As Icon = Icon.FromHandle(hicon)
                bmp = ic.ToBitmap()
                ic.Dispose()
                Return bmp
            End If
        End If
        Return Nothing
    End Function
    Public Sub readMyMouseData(ByVal mD As mData)
        Debug.Print(" x = " & mD.xPosition & " y = " & mD.yPosition & " event = " & mD.eMouseEvent)
        'Debug.Print(mD.stringData)
        Try
            System.Windows.Forms.Cursor.Position = New Point(mD.xPosition, mD.yPosition)
            Try
                If mD.eMouseEvent = "Right" Then
                    Call PerformMouseClick("RClick")
                ElseIf mD.eMouseEvent = "Left" Then
                    Call PerformMouseClick("LClick")
                ElseIf mD.eMouseEvent = "DClick" Then
                    Call PerformMouseClick("DClick")
                ElseIf mD.eMouseEvent = "DragDrop" Then
                    Call fileCreate(mD)
                End If
                mD = Nothing
            Catch ex As Exception
            End Try
        Catch ex As Exception
        End Try
    End Sub
    Public Sub readKeyData(ByVal mD As mData)
        Dim kc As New KeysConverter
        Dim mKey As String = kc.ConvertToString(mD.keyB)
        Debug.Print("tecla enviada => " + mKey)
        SendKeys.SendWait(mD.keyB)
    End Sub


    <StructLayout(LayoutKind.Sequential)> _
    Public Structure CURSORINFO
        Public cbSize As Int32
        Public flags As Int32
        Public hCursor As IntPtr
        Public ptScreenPos As Point
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure ICONINFO
        Public fIcon As Boolean
        Public xHotspot As Int32
        Public yHotspot As Int32
        Public hbmMask As IntPtr
        Public hbmColor As IntPtr
    End Structure
End Class
