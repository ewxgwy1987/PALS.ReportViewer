'=====================================================================================
' Copyright 2007, Xu Jian, All Rights Reserved.
'=====================================================================================
'FileName       Initializer.vb
'Revision:      1.0 -   04 Oct 2007, By Xu Jian 
'=====================================================================================

Option Explicit On
Option Strict On

Imports System.Xml
Imports PALS.Net.Common
Imports PALS.Utilities

Namespace Application

    ''' -----------------------------------------------------------------------------
    ''' Project	 : BHS_ReportViewer
    ''' Class	 : Application.Initializer
    ''' 
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    ''' 	[xujian]	04/10/2007	Created
    ''' </history>
    ''' -----------------------------------------------------------------------------
    Public Class Initializer
        Implements IDisposable

#Region "1. Class Fields Declaration."
        Private Const CONFIGSET_APPINITIALIZER As String = "Application.Initializer"
        Private Const CONFIGSET_REPORTING As String = "Reporting"
        Private Const XML_NODE_REPORTSERVERURL As String = "reportServerUrl"
        Private Const XML_NODE_REPORTSERVERCRED As String = "reportServerCredential"
        Private Const XML_NODE_PREVIEWZOOM As String = "previewZoom"
        Private Const XML_NODE_SHOWPRINTLAYOUT As String = "showPrintLayout"
        Private Const XML_NODE_AUTOCLOSEVIEWER As String = "autoCloseViewer"
        Private Const XML_NODE_REPORTLISTSTYLE As String = "reportListStyle"
        Private Const XML_NODE_SHOWSINGLEGROUPNAME As String = "showSingleGroupName"
        Private Const XML_NODE_DATEFORMAT As String = "dateFormat"
        Private Const XML_NODE_TIMEFORMAT As String = "timeFormat"

        Private Const XML_NODE_REPORTS As String = "reports"
        Private Const XML_NODE_REPORT As String = "report"
        Private Const XML_NODE_REPORTAUTHENTICATION As String = "authentication"
        Private Const XML_NODE_REPORTPATH As String = "reportPath"
        Private Const XML_NODE_REPORTNEEDDTFORMAT As String = "needDateTimeFormat"
        Private Const XML_NODE_PARAMS As String = "params"
        Private Const XML_NODE_PARAM As String = "param"

        Private Const REPORT_GROUP As String = "Group"

        'Added by Guo Wenyu 2014/04/08 
        Private Const XML_NODE_DBCONNECTIONSTRING As String = "DBConnectionString"
        Private Const XML_NODE_GETREPORTSBYUSER As String = "GetReportsByUser"

        'The name of current class 
        Private Shared ReadOnly m_ClassName As String = _
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString
        'Create a logger for use in this class
        Private Shared ReadOnly m_Logger As log4net.ILog = _
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Private m_ConfigFile As String
        Private m_ConfigRoot As XmlElement
        Private m_GlobalInfo As GlobalAppInfo
#End Region

#Region "2. Class Constructor and Destructor Declaration."
        Public Sub New(ByVal ConfigFile As String, ByRef GlobalInfo As GlobalAppInfo)
            MyBase.new()

            'Step 1. Set the configuration file name
            If Trim(ConfigFile) = "" Then
                Throw New Exception("The XML config file name is mandatory!")
            Else
                m_ConfigFile = ConfigFile
                m_GlobalInfo = GlobalInfo
            End If

            'Step 2. Set the root XmlElement of configuration file
            m_ConfigRoot = XMLConfig.GetConfigFileRootElement(m_ConfigFile)
            If m_ConfigRoot Is Nothing Then
                Throw New Exception("Open XML configuration file failure!")
            End If

            'Step 3. Initialize the Log4Net config settings
            Dim Log4NetConfigSet As XmlElement = m_ConfigRoot.Item("log4net")
            If Log4NetConfigSet Is Nothing Then
                Throw New Exception("There is no LOG4NET settings in the XML configuration file!")
            Else
                log4net.Config.XmlConfigurator.Configure(Log4NetConfigSet)
                m_Logger.Info(".")
                m_Logger.Info(".")
                m_Logger.Info(".")
                m_Logger.Info("[..................] <" & m_ClassName & ".New()>")
                m_Logger.Info("[...App Starting...] <" & m_ClassName & ".New()>")
                m_Logger.Info("[..................] <" & m_ClassName & ".New()>")
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If m_Logger.IsInfoEnabled Then
                m_Logger.Info("[..................] <" & m_ClassName & ".Dispose()>")
                m_Logger.Info("[...App Stopping...] <" & m_ClassName & ".Dispose()>")
                m_Logger.Info("[..................] <" & m_ClassName & ".Dispose()>")
                m_Logger.Info("Class:[" & m_ClassName & "] object is going to be destroyed... <" & _
                            m_ClassName & ".Dispose()>")
            End If

            m_ConfigRoot = Nothing

            If Not (m_GlobalInfo.ReportSyncdHash Is Nothing) Then
                m_GlobalInfo.ReportSyncdHash.Clear()
                m_GlobalInfo.ReportSyncdHash = Nothing
            End If

            If m_Logger.IsInfoEnabled Then
                m_Logger.Info("Class:[" & m_ClassName & "] object has been destroyed! <" & _
                            m_ClassName & ".Dispose()>")
            End If
        End Sub 'Dispose

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
#End Region

#Region "3. Class Properties Defination."
        Public ReadOnly Property ClassName() As String
            Get
                Return m_ClassName
            End Get
        End Property
#End Region

#Region "4. Class Overrides Method Declaration."

#End Region

#Region "5. Class Method Declaration."

        Public Function Init() As Boolean
            Dim ThisMethod As String = m_ClassName & "." & _
                    System.Reflection.MethodBase.GetCurrentMethod().Name & "()"

            Try
                If m_Logger.IsInfoEnabled Then
                    m_Logger.Info("Class:[" & m_ClassName & _
                            "] object is initializing... <" & ThisMethod & ">")
                End If

                If m_GlobalInfo Is Nothing Then
                    m_GlobalInfo = New Application.GlobalAppInfo
                End If

                'Step 1: Read General application infomation setting parameters
                m_GlobalInfo.AppName = m_ConfigRoot.GetAttribute("name")
                m_GlobalInfo.AppStartedTime = Now

                Dim GeneralAppInfoConfigSet As XmlNode
                GeneralAppInfoConfigSet = XMLConfig.GetConfigSetElement(m_ConfigRoot, "configSet", _
                                            "name", CONFIGSET_APPINITIALIZER)
                If GeneralAppInfoConfigSet Is Nothing Then
                    Throw New Exception("There is no """ & CONFIGSET_APPINITIALIZER & _
                                """ configSet in the XML config file!")
                Else
                    m_GlobalInfo.Company = XMLConfig.GetSettingFromInnerText(GeneralAppInfoConfigSet, "company", "Inter-Roller")
                    m_GlobalInfo.Department = XMLConfig.GetSettingFromInnerText(GeneralAppInfoConfigSet, "department", "CSI")
                    m_GlobalInfo.Author = XMLConfig.GetSettingFromInnerText(GeneralAppInfoConfigSet, "author", "XuJian")
                End If

                'Step 3: Create MessageHandler object.
                Dim ReportingSet As XmlNode
                ReportingSet = XMLConfig.GetConfigSetElement(m_ConfigRoot, "configSet", "name", CONFIGSET_REPORTING)
                If ReportingSet Is Nothing Then
                    If m_Logger.IsErrorEnabled Then
                        m_Logger.Error("There is no """ & CONFIGSET_REPORTING & _
                                """ configSet in the XML config file! <" & _
                                ThisMethod & ">")
                    End If
                    Return False
                Else
                    'Read report information from XML configuration file
                    GetReportInfoFromXML(ReportingSet)

                End If

                If m_Logger.IsInfoEnabled Then
                    m_Logger.Info("Class:[" & m_ClassName & "] object has been initialized. <" & ThisMethod & ">")
                End If

                Return True
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical, "System Error")

                Return False
            End Try
        End Function

        '<reports group="MDS">
        '   <report enabled="True" type="Statistic" name="Equipment Fault Report">
        '       <reportServerUrl>http://iFix/ReportServer$SQLExpress</reportServerUrl>
        '       <reportPath>/BHSReports/EquipFault_ST</reportPath>
        '       <needDateTimeFormat>True</needDateTimeFormat>
        '       <params>
        '           <param name="DTFrom" datatype="DateTime" value="-1"></param>
        '           <parem name="DTTo" datatype="DateTime" value="0"></parem>          
        '       </params>        
        '   </report>
        '</reports>
        '<reports group="SAC">
        '   ...
        '</reports>
        Private Sub GetReportInfoFromXML(ByRef ConfigSet As XmlNode)
            Dim ThisMethod As String = m_ClassName & "." & System.Reflection.MethodBase.GetCurrentMethod().Name & "()"
            Dim Temp As String

            '<!-- Global Report Server URL. Individual report can has own reportServerUrl node, so that your
            'can print the reports that are located in the different report servers. If the individual report 
            'don't have reportServerUrl node, this global report server URL will be used. -->
            '<!-- User identity of log onto reporting server. If userName is empty, then Windows authentication 
            'of current logged in user on client computer will be used for logging onto reporting server. Otherwise,
            'the given username and password will be passed to reporting server for authentication.
            'If reporting server is domain node, then the value of attribute "domain" should be domain name.
            'If reporting server is workgroup server, then the value of attribute "domain" should be computer 
            'name of reporting server. 
            'Note:
            '1. Empty user name should be assigned for SQL Server 2005 Reporting Service;
            '2. Actual user name, password and domain should be assigned for SQL Server 2008 Reporting Service;
            '3. DO NOT ASSIGN ADMINISTRATOR USER NAME AND PASSWORD IN THIS FILE.
            '-->
            '<reportServerCredential userName="reportuser" pasword="BHSRep0rtUser" domain="SQL2008EXP" />
            '<!-- previewZoom attribute mode: 0 - FullPage, 1 - PageWidth, 2 - Percent;
            'attribute percent: integer value of zoom percentage. Percentage will only be used when mode attribute is Percent -->
            '<previewZoom mode="1" percent="75" />
            '<showPrintLayout>True</showPrintLayout>
            '<!-- autoCloseViewer decide whether the report window to be closed automatically or not when it lose focus. -->
            '<autoCloseViewer>False</autoCloseViewer>
            '<!-- reportListStyle, 1: ListBox, 2: TreeView -->
            '<reportListStyle>2</reportListStyle>
            '<!-- displaySingleGroupName decide whether the group name is shown in the TreeView if there is only one report group -->
            '<showSingleGroupName>False</showSingleGroupName>

            '<!-- dateFormat and timeFormat are used to define the Date or Time format string.
            'attribute "valueType" has two valid settings: CultureDefault & CultureCustomized.
            'CultureDefault    - To inform PALS.ReportViewer to pass the defautl
            '                    format string of culture used by current thread;
            'CultureCustomized - To inform PALS.ReportViewer to pass user selected (customized)
            '                    format string of culture used by current thread;

            'attribute "valuePart" has two valid settings: ShortDatePattern & ShortTimePattern.
            'ShortDatePattern  - Use Short Date format string;
            'ShortTimePattern  - Use Short Time format string;

            'If dateFormat and timeFormat are missing or assigned with invalid setting, but report template
            'requires the Date & Time format string as the report parameters, then the CultureCustomized 
            'short date and short time format string will be used.
            '-->
            '<dateFormat valueType="CultureCustomized" valuePart="ShortDatePattern"></dateFormat>
            '<timeFormat valueType="CultureCustomized" valuePart="ShortTimePattern"></timeFormat>

            'Added by Guo Wenyu 2014/04/08
            'Make report viewer can connect to report server database to get report info by Username
            Temp = Nothing
            Temp = XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_DBCONNECTIONSTRING, Nothing)
            If Trim(Temp) = "" Then
                If m_Logger.IsErrorEnabled Then
                    m_Logger.Error("The value of " & XML_NODE_DBCONNECTIONSTRING & "is invalid or missing in the config set <" & _
                                ConfigSet.Name & "> of XML configuration file! <" & ThisMethod & ">")
                End If
                Throw New System.Exception("The value of " & XML_NODE_DBCONNECTIONSTRING & _
                                "is invalid or missing in the XML configuration file!")
            Else
                m_GlobalInfo.DBConnectionString = Temp
            End If

            Temp = Nothing
            Temp = XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_GETREPORTSBYUSER, Nothing)
            If Trim(Temp) = "" Then
                If m_Logger.IsErrorEnabled Then
                    m_Logger.Error("The value of " & XML_NODE_GETREPORTSBYUSER & "is invalid or missing in the config set <" & _
                                ConfigSet.Name & "> of XML configuration file! <" & ThisMethod & ">")
                End If
                Throw New System.Exception("The value of " & XML_NODE_GETREPORTSBYUSER & _
                                "is invalid or missing in the XML configuration file!")
            Else
                m_GlobalInfo.STP_GetReportsByUser = Temp
            End If

            Temp = Nothing
            Temp = XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_REPORTSERVERURL, Nothing)
            If Trim(Temp) = "" Then
                If m_Logger.IsErrorEnabled Then
                    m_Logger.Error("The value of " & XML_NODE_REPORTSERVERURL & "is invalid or missing in the config set <" & _
                                ConfigSet.Name & "> of XML configuration file! <" & ThisMethod & ">")
                End If
                Throw New System.Exception("The value of " & XML_NODE_REPORTSERVERURL & _
                                "is invalid or missing in the XML configuration file!")
            Else
                m_GlobalInfo.ReportServerRUL = Temp
            End If

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_REPORTSERVERCRED, "userName", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.UserName = Nothing
            Else
                m_GlobalInfo.UserName = Temp
            End If

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_REPORTSERVERCRED, "password", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.Password = Nothing
            Else
                m_GlobalInfo.Password = Temp
            End If

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_REPORTSERVERCRED, "domain", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.Domain = Nothing
            Else
                m_GlobalInfo.Domain = Temp
            End If

            m_GlobalInfo.PreviewZoomMode = CType(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_PREVIEWZOOM, "mode", "0"), Integer)
            m_GlobalInfo.PreviewZoomPercent = CType(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_PREVIEWZOOM, "percent", "100"), Integer)

            m_GlobalInfo.ShowPrintLayout = CType(XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_SHOWPRINTLAYOUT, "False"), Boolean)
            m_GlobalInfo.AutoCloseViewer = CType(XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_AUTOCLOSEVIEWER, "True"), Boolean)
            m_GlobalInfo.ShowSingleGroupName = CType(XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_SHOWSINGLEGROUPNAME, "True"), Boolean)

            Try
                m_GlobalInfo.ReportListStyle = CType(XMLConfig.GetSettingFromInnerText(ConfigSet, XML_NODE_REPORTLISTSTYLE, "2"), GUIStype)
            Catch ex As Exception
                MsgBox("Invalid <reportListStyle> setting! Default TreeView style will be used.", MsgBoxStyle.Critical, "System Error")
                m_GlobalInfo.ReportListStyle = GUIStype.TreeView
            End Try

            '<dateFormat valueType="CultureCustomized" valuePart="ShortDatePattern"></dateFormat>
            '<timeFormat valueType="CultureCustomized" valuePart="ShortTimePattern"></timeFormat>

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_DATEFORMAT, "valueType", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.DateFormatType = DTFormatType.CultureCustomized
            Else
                Select Case Temp
                    Case DTFormatType.CultureDefault.ToString
                        m_GlobalInfo.DateFormatType = DTFormatType.CultureDefault
                    Case DTFormatType.CultureCustomized.ToString
                        m_GlobalInfo.DateFormatType = DTFormatType.CultureCustomized
                    Case Else
                        m_GlobalInfo.DateFormatType = DTFormatType.CultureCustomized
                End Select
            End If

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_DATEFORMAT, "valuePart", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.DateFormatPart = DTFormatPart.ShortDatePattern
            Else
                Select Case Temp
                    Case DTFormatPart.LongDatePattern.ToString
                        m_GlobalInfo.DateFormatPart = DTFormatPart.LongDatePattern
                    Case DTFormatPart.ShortDatePattern.ToString
                        m_GlobalInfo.DateFormatPart = DTFormatPart.ShortDatePattern
                    Case Else
                        m_GlobalInfo.DateFormatPart = DTFormatPart.ShortDatePattern
                End Select
            End If

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_TIMEFORMAT, "valueType", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.TimeFormatType = DTFormatType.CultureCustomized
            Else
                Select Case Temp
                    Case DTFormatType.CultureDefault.ToString
                        m_GlobalInfo.TimeFormatType = DTFormatType.CultureDefault
                    Case DTFormatType.CultureCustomized.ToString
                        m_GlobalInfo.TimeFormatType = DTFormatType.CultureCustomized
                    Case Else
                        m_GlobalInfo.TimeFormatType = DTFormatType.CultureCustomized
                End Select
            End If

            Temp = Nothing
            Temp = Trim(XMLConfig.GetSettingFromAttribute(ConfigSet, XML_NODE_TIMEFORMAT, "valuePart", Nothing))
            If Temp = Nothing Then
                m_GlobalInfo.TimeFormatPart = DTFormatPart.ShortTimePattern
            Else
                Select Case Temp
                    Case DTFormatPart.LongTimePattern.ToString
                        m_GlobalInfo.TimeFormatPart = DTFormatPart.LongTimePattern
                    Case DTFormatPart.ShortTimePattern.ToString
                        m_GlobalInfo.TimeFormatPart = DTFormatPart.ShortTimePattern
                    Case Else
                        m_GlobalInfo.TimeFormatPart = DTFormatPart.LongTimePattern
                End Select
            End If

            ' If the first <reports> XML node in the XML configuration file does not have the "group" 
            ' attribute or its value is empty, the its default category value will be "Group1". 
            ' If the subsequence <reports> node also does not have the "group" attribute or its 
            ' value is empty, then application will assign "Group?" as it group name. "?" here
            ' is the sequence number that the <reports> node is located in the XML file.
            Dim ReportsConfigSet As XmlNode
            Dim RptGroup As String
            Dim iLoop, CatCount As Integer
            CatCount = ConfigSet.ChildNodes.Count
            For iLoop = 0 To CatCount - 1
                ReportsConfigSet = Nothing
                RptGroup = Nothing

                If ConfigSet.ChildNodes(iLoop).Name = XML_NODE_REPORTS Then
                    ReportsConfigSet = ConfigSet.ChildNodes(iLoop)

                    RptGroup = XMLConfig.GetSettingFromAttribute(ReportsConfigSet, "group", "")
                    If Trim(RptGroup) = "" Then
                        RptGroup = REPORT_GROUP & m_GlobalInfo.ReportGroupCount.ToString
                    End If
                    If Not m_GlobalInfo.ReportGroups.Contains(RptGroup) Then
                        m_GlobalInfo.ReportGroups.Add(RptGroup)
                    Else
                        If m_Logger.IsErrorEnabled Then
                            m_Logger.Error("The report group name must be unique! Multiple report group """ & _
                                    RptGroup & """ are detected in the XML configuration file!<" & ThisMethod & ">")
                        End If
                        Throw New System.Exception("The report group name must be unique! " & _
                                    "Multiple report group """ & RptGroup & """ are detected in the XML configuration file!")
                    End If

                    Dim i, Size As Integer
                    Size = ReportsConfigSet.ChildNodes.Count
                    For i = 0 To Size - 1
                        If ReportsConfigSet.ChildNodes(i).Name = XML_NODE_REPORT Then
                            Dim Report As New Reporting.Report

                            '1. Get the Report Name -
                            Report.Name = XMLConfig.GetSettingFromAttribute(ReportsConfigSet.ChildNodes(i), "name", Nothing)
                            If Trim(Report.Name) = "" Then
                                If m_Logger.IsErrorEnabled Then
                                    m_Logger.Error("The report name in XML configuration file can not be empty! <" & ThisMethod & ">")
                                End If
                                Throw New System.Exception("The report name in XML configuration file can not be empty!")
                            End If

                            '2. Get the Report Type & Group -
                            Report.Type = XMLConfig.GetSettingFromAttribute(ReportsConfigSet.ChildNodes(i), "type", Nothing)
                            Report.Group = RptGroup

                            '3. Get the Report Availiablity -
                            Report.Enabled = CType(XMLConfig.GetSettingFromAttribute(ReportsConfigSet.ChildNodes(i), "enabled", "False"), Boolean)

                            'Since the data in the Hashtable is not sorted, m_RptList is used to host the enabled 
                            'report list according to the report order in XML configuration file.
                            If Report.Enabled Then
                                Dim RptID As ReportIdentity
                                RptID.Gourp = Report.Group
                                RptID.Name = Report.Name

                                If Not m_GlobalInfo.ReportList.Contains(RptID) Then
                                    m_GlobalInfo.ReportList.Add(RptID)
                                End If
                            End If

                            '3.1. Get the Report Authentication Setting -
                            Temp = Nothing
                            Temp = XMLConfig.GetSettingFromInnerText(ReportsConfigSet.ChildNodes(i), XML_NODE_REPORTAUTHENTICATION, Nothing)
                            If Trim(Temp) = "" Then
                                Report.Authentication = False
                            Else
                                Report.Authentication = CType(Temp, Boolean)
                            End If

                            '4. Get the Report Server URL -
                            Temp = Nothing
                            Temp = XMLConfig.GetSettingFromInnerText(ReportsConfigSet.ChildNodes(i), XML_NODE_REPORTSERVERURL, Nothing)
                            If Trim(Temp) = "" Then
                                Report.ReportServerURL = m_GlobalInfo.ReportServerRUL
                            Else
                                Report.ReportServerURL = Temp
                            End If

                            '5. Get the Report Path -
                            Temp = Nothing
                            Temp = XMLConfig.GetSettingFromInnerText(ReportsConfigSet.ChildNodes(i), XML_NODE_REPORTPATH, Nothing)
                            If Trim(Temp) = "" Then
                                If m_Logger.IsErrorEnabled Then
                                    m_Logger.Error("The report path in XML configuration file can not be empty! <" & ThisMethod & ">")
                                End If
                                Throw New System.Exception("The report path in XML configuration file can not be empty!")
                            Else
                                Report.ReportPath = Temp
                            End If

                            '5.1 Set Reports Path map - Guo Wenyu 2014/04/08
                            If Not m_GlobalInfo.ReportsPath.Contains(Report.Name) Then
                                m_GlobalInfo.ReportsPath.Add(Report.Name, Report.ReportPath)
                            End If

                            '5. Get the needDateTimeFormat -
                            Report.NeedDateTimeFormat = CType(XMLConfig.GetSettingFromInnerText(ReportsConfigSet.ChildNodes(i), XML_NODE_REPORTNEEDDTFORMAT, "False"), Boolean)

                            '6. Get the Report parameters -
                            '  <params>
                            '    <param name="DTFrom" datatype="DateTime" value="-1"></param>
                            '    <parem name="DTTo" datatype="DateTime" value="0"></parem>          
                            '  </params>        
                            Dim ParamsConfigSet As XmlNode
                            ParamsConfigSet = XMLConfig.GetConfigSetElement(ReportsConfigSet.ChildNodes(i), XML_NODE_PARAMS)
                            If Not ParamsConfigSet Is Nothing Then
                                Dim pi, pCount, pSize As Integer

                                pCount = ParamsConfigSet.ChildNodes.Count
                                For pi = 0 To pCount - 1
                                    If ParamsConfigSet.ChildNodes(pi).Name = XML_NODE_PARAM Then
                                        Dim TmpParam As New Reporting.ReportParameter
                                        TmpParam.Name = XMLConfig.GetSettingFromAttribute( _
                                                    ParamsConfigSet.ChildNodes(pi), "name", Nothing)
                                        TmpParam.DataType = XMLConfig.GetSettingFromAttribute( _
                                                    ParamsConfigSet.ChildNodes(pi), "datatype", Nothing)
                                        TmpParam.Value = XMLConfig.GetSettingFromAttribute( _
                                                    ParamsConfigSet.ChildNodes(pi), "value", Nothing)
                                        TmpParam.DateOnly = CType(XMLConfig.GetSettingFromAttribute( _
                                                    ParamsConfigSet.ChildNodes(pi), "dateonly", "False"), Boolean)

                                        pSize = Report.ReportParamsCount

                                        ReDim Preserve Report.ReportParams(pSize)
                                        Report.ReportParams(pSize) = TmpParam
                                    End If
                                Next
                            End If

                            '7. Create the Report Name and Type relationship for displaying on GUI -
                            Dim Key As String = Report.Group & " " & Report.Name
                            If Trim(Report.Type) = "" Then
                                If m_GlobalInfo.ReportSyncdHash.Contains(Key) Then
                                    If m_Logger.IsErrorEnabled Then
                                        m_Logger.Error("More than one reports have the same name (" & Key & _
                                                "), the report type in XML configuration file can not be empty! <" & ThisMethod & ">")
                                    End If
                                    Throw New System.Exception("More than one reports have the same name (" & Key & _
                                                "), the report type in XML configuration file can not be empty!")
                                Else
                                    m_GlobalInfo.ReportSyncdHash.Add(Key, Report)
                                End If
                            Else
                                Dim Rpt As Reporting.Report

                                If m_GlobalInfo.ReportSyncdHash.Contains(Key) Then
                                    Rpt = CType(m_GlobalInfo.ReportSyncdHash.Item(Key), Reporting.Report)
                                    For k As Integer = 0 To Rpt.SubReportCount - 1
                                        If Rpt.SubReports(k).Type = Report.Type Then
                                            If m_Logger.IsErrorEnabled Then
                                                m_Logger.Error("The same report type can not be assigned to more " & _
                                                        "than one report that have the same report name (" & Key & _
                                                        ")! <" & ThisMethod & ">")
                                            End If
                                            Throw New System.Exception("One report type can not be assigned to more " & _
                                                        "than one reports that have the same report name (" & Key & _
                                                        ")!")
                                        End If
                                    Next
                                Else
                                    Rpt = New Reporting.Report(Key)
                                End If

                                Dim j As Integer = Rpt.SubReportCount
                                ReDim Preserve Rpt.SubReports(j)

                                Rpt.SubReports(j) = Report

                                'If any one type report (XMLNode enabled attribute setting) is enabled, then this report
                                'name needs to be shown in the report list of GUI, even though other type of the same report
                                'is disabled. 
                                If Report.Enabled = True Then
                                    Rpt.Enabled = True
                                End If

                                If m_GlobalInfo.ReportSyncdHash.Contains(Key) Then
                                    m_GlobalInfo.ReportSyncdHash.Item(Key) = Rpt
                                Else
                                    m_GlobalInfo.ReportSyncdHash.Add(Key, Rpt)
                                End If

                            End If
                        End If
                    Next
                End If
            Next

            If m_GlobalInfo.ReportGroupCount = 0 Then
                If m_Logger.IsErrorEnabled Then
                    m_Logger.Error("The XML node <" & XML_NODE_REPORTS & "> is missing in the config set [" & _
                            ConfigSet.Name & "]! <" & ThisMethod & ">")
                End If
                Throw New System.Exception("The XML node <" & XML_NODE_REPORTS & "> is missing in the XML configuration file!")
            End If


        End Sub



#End Region

#Region "6. Class Events Defination."

#End Region

    End Class

End Namespace

