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

Imports System.Data
Imports System.Data.SqlClient


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

    'Added by Guo Wenyu 2014/04/08
    Private m_ReportsByUser As Hashtable

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
                    XMLFileOfAppSetting = PALS.Utilities.Functions.GetXMLFileFullName(String.Empty, "cfg\CFG_Reporting.xml")

                    If XMLFileOfAppSetting Is Nothing Then
                        XMLFileOfAppSetting = PALS.Utilities.Functions.GetXMLFileFullName(String.Empty, "CFG_Reporting.xml")

                        If XMLFileOfAppSetting Is Nothing Then
                            Throw New System.Exception("XML Config file (CFG_Reporting.xml) is unable to be located! " & _
                                                        "Check the Windows Environment Parameter ""PALS_BASE"" setting.")
                        End If
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

            'Add by Guo Wenyu 2014/04/08
            'Access report server to get all report names and paths
            m_ReportsByUser = GetReportsByCurrentUser()

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


            'Add by Guo Wenyu 2014/01/11 for display winform in Extended Monitor
            Dim screen_width As Integer
            Dim screen_heigh As Integer
            Dim pos_x As Integer
            Dim pos_y As Integer
            screen_width = Screen.PrimaryScreen.Bounds.Width
            screen_heigh = Screen.PrimaryScreen.Bounds.Height
            pos_y = CInt(screen_heigh / 2 - Me.ClientSize.Height / 2)
            pos_x = CInt(screen_width / 4 - Me.ClientSize.Width / 2)
            Me.Location = New Point(pos_x, pos_y)



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
    'This function is used to get all reports info from report server database by current logged username
    Private Function GetReportDataTable(ByVal username As String, ByVal reportfolder As String) As DataTable

        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"

        Dim dt_reports As DataTable = Nothing
        Dim ds_reports As New DataSet
        Dim sqlconn As SqlConnection = Nothing
        Dim sqlcmd As SqlCommand = Nothing
        Try
            sqlconn = New SqlConnection
            sqlconn.ConnectionString = m_GlobalInfo.SSRS_DBConnString
            sqlcmd = New SqlCommand(m_GlobalInfo.SSRS_GetReportsByUser, sqlconn)
            sqlcmd.CommandType = CommandType.StoredProcedure

            Dim sqlpara As SqlParameter = sqlcmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100)
            sqlpara.Value = username

            Dim sqlpara2 As SqlParameter = sqlcmd.Parameters.Add("@Folder", SqlDbType.VarChar, 100)
            sqlpara2.Value = reportfolder

            Dim sqladapter As New SqlDataAdapter(sqlcmd)

            sqlconn.Open()
            sqladapter.Fill(ds_reports)
            dt_reports = ds_reports.Tables(0)
        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("System Error! <" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
        End Try

        Return dt_reports

    End Function

    'This function is used to get all reports info from report server database by current logged username
    Private Function GetReportsByCurrentUser() As Hashtable

        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"
        Dim reports As New Hashtable

        Try
            Dim dt_report_list As DataTable
            Dim username As String = System.Environment.UserName
            Dim domain As String = System.Environment.UserDomainName
            Dim reportfolder As String = "ALL" 'the default folder is BHSReports

            'If Me.rbProduction.Checked = True Then
            '    reportfolder = m_GlobalInfo.SSRS_ProductionFolder
            'ElseIf Me.rbHistorical.Checked = True Then
            '    reportfolder = m_GlobalInfo.SSRS_HistoricalFolder
            'End If

            dt_report_list = GetReportDataTable(username, reportfolder)

            If Not dt_report_list.Rows.Count > 0 Then
                dt_report_list = GetReportDataTable(domain + "\" + username, reportfolder)
            End If

            'Dim i As Integer
            Dim dr As DataRow = Nothing
            Dim report_name, report_path As String
            If dt_report_list.Rows.Count > 0 Then
                For Each dr In dt_report_list.Rows
                    report_name = CType(dr("Name"), String)
                    report_path = CType(dr("Path"), String)

                    If Not reports.Contains(report_path) Then
                        reports.Add(report_path, report_name)
                    End If

                Next
            End If



        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("System Error! <" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
        End Try

        Return reports

    End Function
    'Function IsSSRSReport() is used to check the path of the report and its subreports(different types) exists or not in SSRS 
    Private Function IsSSRSReport(ByRef RptID As ReportIdentity) As Boolean
        Dim key As String = RptID.Gourp & " " & RptID.Name
        Dim Rpt As Reporting.Report = CType(m_GlobalInfo.ReportSyncdHash.Item(key), Reporting.Report)
        Dim report_path As String

        If Not Rpt Is Nothing Then
            report_path = Rpt.ReportPath
            If Not report_path Is Nothing Then
                If m_ReportsByUser.Contains(report_path) Then
                    Return True
                End If
            End If

            If Rpt.SubReportCount > 0 Then
                For i As Integer = 0 To Rpt.SubReportCount - 1
                    If m_ReportsByUser.Contains(Rpt.SubReports(i).ReportPath) Then
                        Return True
                    End If
                Next
            End If
        Else
            Return False
        End If

        Return False

    End Function

    Private Function IsSSRSReport(ByVal report_uid As String) As Boolean

        Dim report_path As String = CType(m_GlobalInfo.ReportsPathList(report_uid), String)

        If m_ReportsByUser.Contains(report_path) Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Sub FillInListBox()
        'Fill the report names into the ListBox
        If Not m_GlobalInfo.ReportList Is Nothing Then
            Dim RptID As ReportIdentity

            lbReports.Items.Clear() 'Add by Guo Wenyu 2014/04/20

            For i As Integer = 0 To m_GlobalInfo.ReportList.Count - 1
                RptID = CType(m_GlobalInfo.ReportList.Item(i), ReportIdentity)

                'Modified by Guo Wenyu 2014/04/21
                If IsSSRSReport(RptID) Then
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

                    'Modified by Guo Wenyu 2014/04/21
                    If IsSSRSReport(RptID) Then
                        tvReports.Nodes.Item(RptID.Gourp).Nodes.Add(RptID.Name, RptID.Name, "Report1", "Report1")
                    End If
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

                    'Modified by Guo Wenyu 2014/04/21
                    If IsSSRSReport(RptID) Then
                        If m_GlobalInfo.ShowSingleGroupName Then
                            tvReports.Nodes.Item(RptID.Gourp).Nodes.Add(RptID.Name, RptID.Name, "Report1", "Report1")
                        Else
                            tvReports.Nodes.Add(RptID.Name, RptID.Name, "Report1", "Report1")
                        End If
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
        Dim BoxSize As New Size(348, 355)

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

        Dim report_uid As String
        Rpt = CType(m_GlobalInfo.ReportSyncdHash.Item(Name), Reporting.Report)
        If Not Rpt Is Nothing Then
            If Rpt.SubReportCount > 0 Then
                For i As Integer = 0 To Rpt.SubReportCount - 1

                    report_uid = Rpt.SubReports(i).Name & ";" & Rpt.SubReports(i).Group & ";" & Rpt.SubReports(i).Type

                    'Add code to check if report is authenticated by current user - Modified by Guo Wenyu 2014/04/21
                    If IsSSRSReport(report_uid) Then
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
                    End If
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


        Dim report_uid As String

        If Not Rpt Is Nothing Then
            If Rpt.SubReportCount > 0 Then
                For i As Integer = 0 To Rpt.SubReportCount - 1

                    report_uid = Rpt.SubReports(i).Name & ";" & Rpt.SubReports(i).Group & ";" & Rpt.SubReports(i).Type

                    'Add code to check if report is authenticated by current user - Modified by Guo Wenyu 2014/04/21
                    If IsSSRSReport(report_uid) Then
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
                    End If
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

    Private Sub btnBackup_Click(sender As System.Object, e As System.EventArgs) Handles btnBackup.Click
        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"

        Dim sqlconn As SqlConnection = Nothing
        Dim sqlcmd As SqlCommand = Nothing

        Dim stp_result As String = ""

        Try
            sqlconn = New SqlConnection
            sqlconn.ConnectionString = m_GlobalInfo.PRD_DBconnstring
            sqlcmd = New SqlCommand(m_GlobalInfo.STP_BackupDatabase, sqlconn)
            sqlcmd.CommandType = CommandType.StoredProcedure

            Dim sqlpara_dbname As SqlParameter = sqlcmd.Parameters.Add("@DATABASE_NAME", SqlDbType.VarChar, 50)
            sqlpara_dbname.Value = m_GlobalInfo.PRD_DBName
            sqlpara_dbname.Direction = ParameterDirection.Input

            Dim sqlpara_bakpath As SqlParameter = sqlcmd.Parameters.Add("@BACKUP_DIRPATH", SqlDbType.VarChar, 1000)
            sqlpara_bakpath.Value = m_GlobalInfo.BackupPath
            sqlpara_bakpath.Direction = ParameterDirection.Input

            Dim sqlpara_stpres As SqlParameter = sqlcmd.Parameters.Add("@STP_RESULT", SqlDbType.VarChar, 50)
            sqlpara_stpres.Direction = ParameterDirection.Output

            sqlconn.Open()
            sqlcmd.ExecuteNonQuery()
            stp_result = CType(sqlpara_stpres.Value, String)

            If stp_result <> String.Empty Then
                Dim err_str As String = ""

                err_str += "Application Error: Database<" & m_GlobalInfo.PRD_DBName & "> backup failed.(" & stp_result & ")"

                If m_Logger.IsErrorEnabled Then
                    m_Logger.Error(err_str)
                End If
                MsgBox(err_str, MsgBoxStyle.Critical, "System Error")
            Else
                Dim info_str As String = ""
                info_str += "Database<" & m_GlobalInfo.PRD_DBName & "> backup to " & m_GlobalInfo.BackupPath & " successfully."
                MsgBox(info_str, MsgBoxStyle.Information, "Application Info")
            End If

        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("System Error! <" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
        End Try


    End Sub

    Private Sub btnRestore_Click(sender As System.Object, e As System.EventArgs) Handles btnRestore.Click
        Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"

        Dim sqlconn As SqlConnection = Nothing
        Dim sqlcmd As SqlCommand = Nothing

        Dim stp_result As String = "22"
        Dim prompt_str As String

        Try

            prompt_str = "Before Restoring database, you must remove database<" & m_GlobalInfo.HIS_DBName & "> at first. Do you want to continue?"
            Dim msgres As Integer
            msgres = MsgBox(prompt_str, MsgBoxStyle.YesNo Or MsgBoxStyle.DefaultButton2 Or MsgBoxStyle.Question, "Remove Database <" & m_GlobalInfo.HIS_DBName & "> ?")

            If msgres = 6 Then 'Msgbox with 'YES' button clicked

                'At first remove original backupped database
                sqlconn = New SqlConnection
                sqlconn.ConnectionString = m_GlobalInfo.PRD_DBconnstring
                sqlcmd = New SqlCommand(m_GlobalInfo.STP_RemoveBackupDatabase, sqlconn)
                sqlcmd.CommandType = CommandType.StoredProcedure

                Dim sqlpara_dbname As SqlParameter = sqlcmd.Parameters.Add("@DATABASE_NAME", SqlDbType.VarChar, 50)
                sqlpara_dbname.Value = m_GlobalInfo.HIS_DBName
                sqlpara_dbname.Direction = ParameterDirection.Input

                Dim sqlpara_stpres As SqlParameter = sqlcmd.Parameters.Add("@STP_RESULT", SqlDbType.VarChar, 50)
                sqlpara_stpres.Direction = ParameterDirection.Output

                sqlconn.Open()
                sqlcmd.ExecuteNonQuery()
                stp_result = CType(sqlpara_stpres.Value, String)

                If stp_result = String.Empty Or stp_result = "Database does not exist." Then
                    Dim info_str As String = ""
                    If stp_result = String.Empty Then
                        info_str += "Database<" & m_GlobalInfo.HIS_DBName & "> Removed successfully."
                    Else
                        info_str += "Database<" & m_GlobalInfo.HIS_DBName & "> does not exist. No need to remove."
                    End If

                    MsgBox(info_str, MsgBoxStyle.Information, "Application Info")

                    'Then pop up window to select the database backup file to restore
                    Dim backup_filepath As String = ""
                    Me.OpenBackupFileDialog.InitialDirectory = m_GlobalInfo.BackupPath
                    Me.OpenBackupFileDialog.FileName = ""
                    Dim openres As DialogResult = Me.OpenBackupFileDialog.ShowDialog()

                    If openres = DialogResult.OK Then
                        backup_filepath = Me.OpenBackupFileDialog.FileName

                        'Restore the selected backup file to restore database
                        stp_result = ""
                        sqlconn = New SqlConnection
                        sqlconn.ConnectionString = m_GlobalInfo.PRD_DBconnstring

                        sqlcmd = New SqlCommand(m_GlobalInfo.STP_RestoreDatabase, sqlconn)
                        sqlcmd.CommandType = CommandType.StoredProcedure
                        sqlcmd.CommandTimeout = 1800

                        sqlpara_dbname = sqlcmd.Parameters.Add("@DATABASE_NAME", SqlDbType.VarChar, 50)
                        sqlpara_dbname.Value = m_GlobalInfo.HIS_DBName
                        sqlpara_dbname.Direction = ParameterDirection.Input

                        Dim sqlpara_bakpath As SqlParameter = sqlcmd.Parameters.Add("@BACKUP_FILEPATH", SqlDbType.VarChar, 1000)
                        sqlpara_bakpath.Value = backup_filepath
                        sqlpara_bakpath.Direction = ParameterDirection.Input

                        Dim sqlpara_mdfpath As SqlParameter = sqlcmd.Parameters.Add("@MDF_FILEPATH", SqlDbType.VarChar, 1000)
                        sqlpara_mdfpath.Value = m_GlobalInfo.RestoreMDFPath
                        sqlpara_mdfpath.Direction = ParameterDirection.Input

                        Dim sqlpara_ldfpath As SqlParameter = sqlcmd.Parameters.Add("@LDF_FILEPATH", SqlDbType.VarChar, 1000)
                        sqlpara_ldfpath.Value = m_GlobalInfo.RestoreLDFPath
                        sqlpara_ldfpath.Direction = ParameterDirection.Input

                        sqlpara_stpres = sqlcmd.Parameters.Add("@STP_RESULT", SqlDbType.VarChar, 50)
                        sqlpara_stpres.Direction = ParameterDirection.Output

                        sqlconn.Open()
                        sqlcmd.ExecuteNonQuery()
                        stp_result = CType(sqlpara_stpres.Value, String)

                        If stp_result <> String.Empty Then
                            Dim err_str As String = ""

                            err_str += "Application Error: Database<" & m_GlobalInfo.HIS_DBName & "> restore failed.(" & stp_result & ")"

                            If m_Logger.IsErrorEnabled Then
                                m_Logger.Error(err_str)
                            End If
                            MsgBox(err_str, MsgBoxStyle.Critical, "System Error")
                        Else
                            info_str = ""
                            info_str += "Database<" & m_GlobalInfo.HIS_DBName & "> restore successfully."
                            MsgBox(info_str, MsgBoxStyle.Information, "Application Info")
                        End If
                    End If


                Else
                    Dim err_str As String = ""

                    err_str += "Application Error: Database<" & m_GlobalInfo.HIS_DBName & "> restore failed.(" & stp_result & ")"

                    If m_Logger.IsErrorEnabled Then
                        m_Logger.Error(err_str)
                    End If
                    MsgBox(err_str, MsgBoxStyle.Critical, "System Error")
                End If
            End If



        Catch ex As Exception
            If m_Logger.IsErrorEnabled Then
                m_Logger.Error("System Error! <" & ThisMethod & _
                        "> has exception: Source = " & ex.Source & " | Type : " & ex.GetType.ToString & _
                        " | Message : " & ex.Message)
            End If
            MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")
        End Try
    End Sub

    Private Sub rbProduction_CheckedChanged(sender As System.Object, e As System.EventArgs)

    End Sub

    Private Sub rbHistorical_CheckedChanged(sender As System.Object, e As System.EventArgs)

    End Sub
End Class
