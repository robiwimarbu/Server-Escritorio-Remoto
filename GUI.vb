Public Class GUI
    Public Sub newClient(ByVal countCLients As Integer, ByVal idClient As System.Net.IPEndPoint, ByVal name As String, ByRef pn As Panel)
        Dim btn As New Button()
        Dim itemNumbers As Integer = countCLients
        name = idClient.ToString
        btn.Name = idClient.ToString()
        btn.Top = countCLients * 100
        btn.Height = 90
        btn.Text = name
        pn.Controls.Add(btn)
    End Sub
    Public Sub removeClient(ByVal idClient As System.Net.IPEndPoint)

    End Sub
End Class
