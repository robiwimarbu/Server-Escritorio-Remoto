Public Class Form1
    Private clients As Integer = -1
    Dim WithEvents sv As New server
    Public WithEvents tmrConect As New Timer
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim port As Integer = Integer.Parse(txtPort.Text)
        startListen(port)
    End Sub
    Public Sub startListen(ByVal port As Integer)
        sv.PuertoDeEscucha = port
        sv.start()
        tmrConect.Start()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        sv.sendDisplay()
    End Sub

    Private Sub btnSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSend.Click
        Timer1.Start()
    End Sub
    Public Sub newClientFrm(ByVal countCLients As Integer, ByVal idClient As String)
        sv.flagNewClient = "None"
        showClients()
        Label1.Text = "Usuarios conectados [" & sv.clients.Count & "]"
    End Sub
    Public Sub checkModifyClient() Handles tmrConect.Tick
        If sv.flagNewClient <> "None" Then
            newClientFrm(sv.clients.Count, sv.flagNewClient)
        End If
        If sv.flagRemoveClient <> "None" Then
            removeClientFrm(sv.flagRemoveClient)
        End If
    End Sub
    Public Sub removeClientFrm(ByVal idClient As String)
        Try
            'sv.flagRemoveClient = "None"
            'pnUsersConect.Controls.Remove(pnUsersConect.Controls.Item(idClient))
            sv.flagRemoveClient = "None"
            showClients()
            Label1.Text = "Usuarios conectados [" & sv.clients.Count & "]"
        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try

    End Sub
    Public Sub acceptClient(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim strEndPoint As String
        strEndPoint = sender.name
        Debug.Print("Llego: " & sender.name.ToString)
        If sv.clientAutorized = strEndPoint Then
            sv.clientAutorized = ""
            sender.text = "Pemitir Control"
        Else
            sender.text = "Detener Control"
            sv.clientAutorized = strEndPoint
        End If

    End Sub

    Public Sub showClients()

        pnUsersConect.Controls.Clear()
        Dim i As Integer = 0
        If sv.clients.Count > 0 Then
            For Each Cliente In sv.clients.Values
                Dim name As String = Cliente.id.ToString
                Dim itemNumbers As Integer = i
                Dim itemeUser As New ItemUser
                itemeUser.LnkAllowControl.Name = name
                itemeUser.lblName.Text = Cliente.id.ToString()
                itemeUser.Location = New Point(5, (itemNumbers * 65) + 1)
                itemeUser.Size = New Size(pnUsersConect.Width - 11, 61)
                pnUsersConect.Controls.Add(itemeUser)
                AddHandler itemeUser.LnkAllowControl.Click, AddressOf acceptClient
                i = i + 1
            Next

        End If
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub
End Class
