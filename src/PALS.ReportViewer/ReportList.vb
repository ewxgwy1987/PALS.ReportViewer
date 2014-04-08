'=====================================================================================
' Copyright 2007-, Xu Jian, All Rights Reserved.
'=====================================================================================
'FileName:      ReportList.vb
'Revision:      04 Oct 2007, By XJ, First release.
'=====================================================================================

Option Explicit On
Option Strict On

Imports System.Timers
Imports System.Xml
Imports PALS.Utilities

Public Class ReportList

    Private Const NODE_SEPARATER As String = "\"

    'The name of current class 
    Private Shared ReadOnly m_ClassName As String = _
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString
    'Create a logger for use in this class
    Private Shared ReadOnly m_Logger As log4net.ILog = log4net.LogManager.GetLogger( _
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private m_Initializer As Application.Initializer
    Private m_IR_Bin_Folder As String
    Private m_CurrentPath As String
    Private m_ParentPath As String
    Private m_ParentParentPath As String
    Private m_GlobalInfo As Application.GlobalAppInfo
    Private m_DefaultReportGroup As String

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Determines if there already is a 'BHS_Console' running in the local host.
    ''' </summary>
    ''' <param name="processName"></param>
    ''' <returns>true if it finds more than one processName running</returns>
    ''' <remarks>Called in the Main() sub of the application.</remarks>
    ''' <history>
    ''' 	[xujian]	22/08/2006	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Private Function HasDuplicateProcess(ByVal processName As String, ByRef Proc As Process) As Boolean
        'function returns true if it finds more than one 'processName' running
        Dim Procs() As Process
        Dim temp As Process
        Procs = Process.GetProcesses()
        Dim Count As Integer = 0

        For Each temp In Procs
            If temp.ProcessName.ToString.Equals(processName) Then
                Proc = temp
                Count += 1
            End If
        Next temp

        If Count > 1 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub ReportList_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"

        '=======================================================================
        'Follow codes are not required in VB.Net 2005. It is because the single
        'instance checking is handled by VB.Net 2005 Windows Application Framework
        'Properties "Make single instance application".
        '
        'Dim Proc As Process = Nothing
        'If HasDuplicateProcess("BHS_ReportViewer", Proc) Then
        '    'Only single instance of BHS Console Application can be running in a machine!
        '    'MsgBox("There is a instance of BHS_ReportViewer has been launched!", MsgBoxStyle.Information, "System Info")

        '    'Kill previous process before open a new process
        '    Proc.Kill()
        'End If
        '=======================================================================

        Try
            'Get the full path of application setting and telegram defination XML configuration files
            Dim XMLFileOfAppSetting As String = PALS.Utilities.Functions.GetXMLFileFullName("PALS_BASE", "CFG_Reporting.xml")
            If XMLFileOfAppSetting Is Nothing Then
                XMLFileOfAppSetting = PALS.Utilities.Functions.GetXMLFileFullName("PALS_BASE", "ReportViewer\CFG_Reporting.xml")

                If XMLFileOfAppSetting Is Nothing Then
                    XMLFileOfAppSetting = PALS.Utilities.Functions.GetXMLFileFullName(String.Empty, "CFG_Reporting.xml")

                    If XMLFileOfAppSetting Is Nothing Then
                        Throw New System.Exception("XML Config file (CFG_Reporting.xml) is unable to be located! " & _
                                                    "Check the Windows Environment Parameter ""PALS_BASE"" setting.")
                    End If
                End If
            End If

            If Not System.IO.File.Exists(XMLFileOfAppSetting) Then
                MsgBox("XML Config file (CFG_Reporting.xml) is not existed in following directories:" & _
                        vbCrLf & "Environment Param [PALS_BASE]: " & m_IR_Bin_Folder & vbCrLf & m_CurrentPath & _
                        vbCrLf & m_ParentPath & vbCrLf & m_ParentParentPath, MsgBoxStyle.Critical, "System Error")
                End
            Else
                m_GlobalInfo = New Application.GlobalAppInfo
                m_Initializer = New Application.Initializer(XMLFileOfAppSetting, m_GlobalInfo)
                If Not m_Initializer.Init() Then
                    MsgBox("The application initialization failure!", MsgBoxStyle.Critical, "System Error")
                    End
                End If
            End If

            Select Case m_GlobalInfo.ReportListStyle
                Case GUIStype.ListBox
                    SetGUIStyle(GUIStype.ListBox)
                    FillInListBox()

                Case GUIStype.TreeView
                    SetGUIStyle(GUIStype.TreeView)
                    FillInTreeView()

                Case Else
                    SetGUIStyle(GUIStype.TreeView)
                    FillInTreeView()
            End Select

        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("Class:[" & m_ClassName & "] object initialization failure.<" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox("Class:[" & m_ClassName & "] object initialization failure! " & vbCrLf & _
                    ex.Message, MsgBoxStyle.Critical, "System Error")

            End
        End Try
    End Sub

    Private Sub FillInListBox()
        'Fill the report names into the ListBox
        If Not m_GlobalInfo.ReportList Is Nothing Then
            Dim RptID As ReportIdentity

            For i As Integer = 0 To m_GlobalInfo.ReportList.Count - 1
                RptID = CType(m_GlobalInfo.ReportList.Item(i), ReportIdentity)

                If m_GlobalInfo.ReportGroupCount > 1 Then
                    lbReports.Items.Add(RptID.Gourp & " " & RptID.Name)
                Else
                    m_DefaultReportGroup = RptID.Gourp

                    If m_GlobalInfo.ShowSingleGroupName Then
                        lbReports.Items.Add(RptID.Gourp & " " & RptID.Name)
                    Else
                        lbReports.Items.Add(RptID.Name)
                    End If
                End If
            Next
            HideReportTypes()
            lbReports.SelectedIndex = 0
        End If
    End Sub

    Private Sub FillInTreeView()
        'Fill the report names into the ListBox
        If Not m_GlobalInfo.ReportList Is Nothing Then
            ' Suppress repainting the TreeView until all the objects have been created.
            tvReports.BeginUpdate()
            ' Clear the TreeView each time the method is called.
            tvReports.Nodes.Clear()

            'If are there more than one report groups, then display the group node in the TreeView 
            If m_GlobalInfo.ReportGroupCount > 1 Then
                For i As Integer = 0 To m_GlobalInfo.ReportGroupCount - 1
                    Dim RptGrup As String = CType(m_GlobalInfo.ReportGroups.Item(i), String)
                    tvReports.Nodes.Add(RptGrup, RptGrup, "Group1", "Group1")
                Next

                Dim RptID As ReportIdentity
                For i As Integer = 0 To m_GlobalInfo.ReportList.Count - 1
                    RptID = CType(m_GlobalInfo.ReportList.Item(i), ReportIdentity)
                    tvReports.Nodes.Item(RptID.Gourp).Nodes.Add(RptID.Name, RptID.Name, "Report1", "Report1")
                Next
            Else
                'If there is only one report group is defined in XML file
                For i As Integer = 0 To m_GlobalInfo.ReportGroupCount - 1
                    Dim RptGrup As String = CType(m_GlobalInfo.ReportGroups.Item(i), String)
                    m_DefaultReportGroup = RptGrup

                    If m_GlobalInfo.ShowSingleGroupName Then
                        tvReports.Nodes.Add(RptGrup, RptGrup, "Group1", "Group1")
                    End If
                Next

                Dim RptID As ReportIdentity
                For i As Integer = 0 To m_GlobalInfo.ReportList.Count - 1
                    RptID = CType(m_GlobalInfo.ReportList.Item(i), ReportIdentity)

                    If m_GlobalInfo.ShowSingleGroupName Then
                        tvReports.Nodes.Item(RptID.Gourp).Nodes.Add(RptID.Name, RptID.Name, "Report1", "Report1")
                    Else
                        tvReports.Nodes.Add(RptID.Name, RptID.Name, "Report1", "Report1")
                    End If
                Next
            End If

            ' Begin repainting the TreeView.
            tvReports.EndUpdate()
            tvReports.ExpandAll()
            tvReports.PathSeparator = NODE_SEPARATER

            HideReportTypes()

            'Set tab stop on TreeView object
            tvReports.Select()
            'Set the TreeView selection to the most top node 
            tvReports.SelectedNode = tvReports.Nodes(0)
        End If
    End Sub

    Private Sub SetGUIStyle(ByVal Style As GUIStype)
        Dim Loc As New Point(10, 12)
        Dim BoxSize As New Size(348, 264)

        lbReports.Location = Loc
        lbReports.Size = BoxSize
        tvReports.Location = Loc
        tvReports.Size = BoxSize

        Select Case m_GlobalInfo.ReportListStyle
            Case GUIStype.ListBox
                lbReports.Visible = True
                tvReports.Visible = False
            Case GUIStype.TreeView
                lbReports.Visible = False
                tvReports.Visible = True
            Case Else
                lbReports.Visible = False
                tvReports.Visible = True
        End Select

    End Sub

    Private Sub HideReportTypes()
        rbType1.Visible = False
        rbType2.Visible = False
        rbType3.Visible = False
        rbType4.Visible = False
        rbType5.Visible = False
        rbType6.Visible = False
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        m_Initializer.Dispose()
        End
    End Sub

    Private Sub lbReports_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbReports.SelectedIndexChanged
        Dim Name As String
        Dim Rpt As Reporting.Report

        HideReportTypes()

        If m_GlobalInfo.ReportGroupCount > 1 Then
            Name = lbReports.SelectedItem.ToString
        Else
            If m_GlobalInfo.ShowSingleGroupName Then
                Name = lbReports.SelectedItem.ToString
            Else
                Name = m_DefaultReportGroup & " " & lbReports.SelectedItem.ToString
            End If
        End If

        Rpt = CType(m_GlobalInfo.ReportSyncdHash.Item(Name), Reporting.Report)
        If Not Rpt Is Nothing Then
            If Rpt.SubReportCount > 0 Then
                For i As Integer = 0 To Rpt.SubReportCount - 1
                    Select Case i
                        Case 0
                            If Rpt.SubReports(i).Enabled Then
                                rbType1.Text = Rpt.SubReports(i).Type
                                rbType1.Visible = True
                                rbType1.Checked = True
                            End If
                        Case 1
                            If Rpt.SubReports(i).Enabled Then
                                rbType2.Text = Rpt.SubReports(i).Type
                                rbType2.Visible = True
                            End If
                        Case 2
                            If Rpt.SubReports(i).Enabled Then
                                rbType3.Text = Rpt.SubReports(i).Type
                                rbType3.Visible = True
                            End If
                        Case 3
                            If Rpt.SubReports(i).Enabled Then
                                rbType4.Text = Rpt.SubReports(i).Type
                                rbType4.Visible = True
                            End If
                        Case 4
                            If Rpt.SubReports(i).Enabled Then
                                rbType5.Text = Rpt.SubReports(i).Type
                                rbType5.Visible = True
                            End If
                        Case 5
                            If Rpt.SubReports(i).Enabled Then
                                rbType6.Text = Rpt.SubReports(i).Type
                                rbType6.Visible = True
                            End If
                    End Select
                Next
            End If
        End If
    End Sub

    Private Function GetSelectedReportType() As String
        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"
        Dim Type As String = Nothing

        Try
            If (rbType1.Checked) And (rbType1.Visible) Then
                Type = Trim(rbType1.Text)
            End If

            If (rbType2.Checked) And (rbType2.Visible) Then
                Type = Trim(rbType2.Text)
            End If

            If (rbType3.Checked) And (rbType3.Visible) Then
                Type = Trim(rbType3.Text)
            End If

            If (rbType4.Checked) And (rbType4.Visible) Then
                Type = Trim(rbType4.Text)
            End If

            If (rbType5.Checked) And (rbType5.Visible) Then
                Type = Trim(rbType5.Text)
            End If

            If (rbType6.Checked) And (rbType6.Visible) Then
                Type = Trim(rbType6.Text)
            End If

            Return Type
        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("System Error! <" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
            Return Nothing
        End Try
    End Function

    Private Function GetSelectedReport(ByVal Name As String, ByVal Type As String) As Reporting.Report
        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"
        Dim Report, Temp As Reporting.Report

        Try
            Report = Nothing
            If m_GlobalInfo.ReportSyncdHash.Contains(Name) Then
                Temp = CType(m_GlobalInfo.ReportSyncdHash.Item(Name), Reporting.Report)

                If Type = "" Then
                    Report = Temp
                Else
                    For i As Integer = 0 To Temp.SubReportCount - 1
                        If Temp.SubReports(i).Type = Type Then
                            Report = Temp.SubReports(i)
                            Exit For
                        End If
                    Next
                End If
            Else
                Report = Nothing
            End If

            Return Report
        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("System Error! <" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
            Return Nothing
        End Try
    End Function

    Private Function ComposeReportIdentity(ByVal FullPath As String) As String
        Dim Name As String = Nothing

        Try
            If (m_GlobalInfo.ReportGroupCount = 1) And (Not m_GlobalInfo.ShowSingleGroupName) Then
                Name = m_DefaultReportGroup & " " & FullPath
            Else
                Dim Values() As String
                Values = Split(FullPath, NODE_SEPARATER, , CompareMethod.Binary)

                Dim i As Integer
                For i = 0 To Values.Length - 1
                    If Name Is Nothing Then
                        Name = Values(i)
                    Else
                        Name = Name & " " & Values(i)
                    End If
                Next
            End If

            Return Name
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub tvReports_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvReports.AfterSelect
        Dim FullPath, Name As String
        Dim Rpt As Reporting.Report

        HideReportTypes()

        If tvReports.SelectedNode.Nodes.Count > 0 Then
            btnPreview.Enabled = False
            Exit Sub
        Else
            FullPath = tvReports.SelectedNode.FullPath
            btnPreview.Enabled = True
        End If

        Name = ComposeReportIdentity(FullPath)

        Rpt = CType(m_GlobalInfo.ReportSyncdHash.Item(Name), Reporting.Report)
        If Not Rpt Is Nothing Then
            If Rpt.SubReportCount > 0 Then
                For i As Integer = 0 To Rpt.SubReportCount - 1
                    Select Case i
                        Case 0
                            If Rpt.SubReports(i).Enabled Then
                                rbType1.Text = Rpt.SubReports(i).Type
                                rbType1.Visible = True
                                rbType1.Checked = True
                            End If
                        Case 1
                            If Rpt.SubReports(i).Enabled Then
                                rbType2.Text = Rpt.SubReports(i).Type
                                rbType2.Visible = True
                            End If
                        Case 2
                            If Rpt.SubReports(i).Enabled Then
                                rbType3.Text = Rpt.SubReports(i).Type
                                rbType3.Visible = True
                            End If
                        Case 3
                            If Rpt.SubReports(i).Enabled Then
                                rbType4.Text = Rpt.SubReports(i).Type
                                rbType4.Visible = True
                            End If
                        Case 4
                            If Rpt.SubReports(i).Enabled Then
                                rbType5.Text = Rpt.SubReports(i).Type
                                rbType5.Visible = True
                            End If
                        Case 5
                            If Rpt.SubReports(i).Enabled Then
                                rbType6.Text = Rpt.SubReports(i).Type
                                rbType6.Visible = True
                            End If
                    End Select
                Next
            End If
        End If
    End Sub

    Private Sub btnPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPreview.Click
        Dim Name, Type, FullPath As String
        Dim Report As Reporting.Report
        Dim Viewer As ReportViewer

        Try
            Name = Nothing
            Select Case m_GlobalInfo.ReportListStyle
                Case GUIStype.ListBox
                    If m_GlobalInfo.ReportGroupCount > 1 Then
                        Name = lbReports.SelectedItem.ToString
                    Else
                        If m_GlobalInfo.ShowSingleGroupName Then
                            Name = lbReports.SelectedItem.ToString
                        Else
                            Name = m_DefaultReportGroup & " " & lbReports.SelectedItem.ToString
                        End If
                    End If
                Case GUIStype.TreeView
                    FullPath = tvReports.SelectedNode.FullPath
                    Name = ComposeReportIdentity(FullPath)
                Case Else
                    FullPath = tvReports.SelectedNode.FullPath
                    Name = ComposeReportIdentity(FullPath)
            End Select

            Type = GetSelectedReportType()
            Report = GetSelectedReport(Name, Type)

            If Not Report Is Nothing Then
                Dim UID, PWD, Domain As String

                'Collect user name and password for individual reports if its "authentication" setting is True.
                UID = String.Empty : PWD = String.Empty : Domain = m_GlobalInfo.Domain
                If Report.Authentication = True Then
                    Dim frmLogin As Login = New Login()
                    Dim isCanceled As Boolean = frmLogin.CallMe(UID, PWD, Domain)

                    If isCanceled Then
                        Exit Sub
                    Else
                        If (UID = String.Empty) Then
                            MsgBox("User name can not be empty!", MsgBoxStyle.Information, "Information")
                            Exit Sub
                        End If
                    End If
                Else
                    UID = m_GlobalInfo.UserName
                    PWD = m_GlobalInfo.Password
                    Domain = m_GlobalInfo.Domain
                End If

                Viewer = New ReportViewer

                If Trim(Type) <> "" Then
                    Viewer.Text = Name & " (" & Type & ")"
                Else
                    Viewer.Text = Name
                End If
                Viewer.DateFormat = m_GlobalInfo.DateFormat
                Viewer.TimeFormat = m_GlobalInfo.TimeFormat
                Viewer.UserName = UID
                Viewer.Password = PWD
                Viewer.Domain = Domain
                Viewer.ShowPrintLayout = m_GlobalInfo.ShowPrintLayout
                Viewer.AutoCloseViewer = m_GlobalInfo.AutoCloseViewer
                Viewer.PreviewZoomMode = m_GlobalInfo.PreviewZoomMode
                Viewer.PreviewZoomPercent = m_GlobalInfo.PreviewZoomPercent
                Viewer.ReportServerURL = Report.ReportServerURL
                Viewer.ReportPath = Report.ReportPath
                Viewer.NeedDateTimeFormat = Report.NeedDateTimeFormat
                Viewer.RepartParams = Report.ReportParams

                Try
                    Viewer.Show()
                    Viewer.ShowReport()
                Catch ex As Exception
                    MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
                End Try
            Else
                MsgBox("No report is selected!", MsgBoxStyle.Information, "System Information")
            End If

        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Reporting Error")
        End Try
    End Sub

    Private Sub ApplicationInfoToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ApplicationInfoToolStripMenuItem.Click
        Dim assembly As String = Me.GetType.Assembly.GetName.Name
        Dim version As String = Me.GetType.Assembly.GetName.Version.ToString

        MsgBox("PALS Report Viewer" & vbCrLf & vbCrLf & "Assembly: " & vbTab & assembly & vbCrLf & "Version#: " & vbTab & version, MsgBoxStyle.Information, "Application Info")
    End Sub
End Class