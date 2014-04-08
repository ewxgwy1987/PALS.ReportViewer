'=====================================================================================
' Copyright 2007, Xu Jian, All Rights Reserved.
'=====================================================================================
'FileName       GlobalAppInfo.vb
'Revision:      1.0 -   04 Oct 2007, By Xu Jian
'=====================================================================================

Option Explicit On
Option Strict On

Imports System.Globalization

Public Enum GUIStype
    ListBox = 1
    TreeView = 2
End Enum

Public Enum DTFormatType
    CultureDefault = 1
    CultureCustomized = 2
End Enum

Public Enum DTFormatPart
    LongDatePattern = 1
    ShortDatePattern = 2
    LongTimePattern = 3
    ShortTimePattern = 4
End Enum

Public Structure ReportIdentity
    Dim Gourp As String
    Dim Name As String
End Structure

Namespace Application

    Public NotInheritable Class GlobalAppInfo

#Region "1. Class Fields Declaration."

        Private Const PATH_SEPARATOR As String = "\"

        Private m_AppName As String = "PALS.ReportViewer"
        Private m_AppStartedTime As Date
        Private m_Company As String = "Pteris"
        Private m_Department As String = "CSI"
        Private m_Author As String = "XuJian"

        'Added by Guo Wenyu 2014/04/08
        Private m_dbconnstring As String
        Private m_stp_GetReportsByUser As String
        Private m_ReportsPath As Hashtable

        Private m_ReportServerRUL As String

        Private m_UserName As String
        Private m_Password As String
        Private m_Domain As String

        Private m_ShowPrintLayout As Boolean
        Private m_AutoCloseViewer As Boolean
        Private m_ReportListStyle As GUIStype
        ' whether the group name is shown in the TreeView if there is only one report group 
        Private m_ShowSingleGroupName As Boolean

        Private m_ReportHashTable As Hashtable
        Private m_ReportSyncdHash As Hashtable

        'Since the data in the Hashtable is not sorted, m_RptList is used to host the enabled 
        'report list according to the report order in XML configuration file.
        Private m_ReportList As New ArrayList
        'Number of report group (or <reports> nodes) that are defined in the XML configuration file.
        Private m_ReportGroups As New ArrayList

        'Return current thread culture date and time format settings. 
        'Each culture could have two formats: Culture Default Setting and Culture Customized Setting.
        Private m_CurrentCultureName As String
        Private m_CultureDefaultLongDateFormat As String
        Private m_CultureDefaultShortDateFormat As String
        Private m_CultureDefaultShortTimeFormat As String
        Private m_CultureDefaultLongTimeFormat As String
        Private m_CultureCustomizedLongDateFormat As String
        Private m_CultureCustomizedShortDateFormat As String
        Private m_CultureCustomizedShortTimeFormat As String
        Private m_CultureCustomizedLongTimeFormat As String

        Private m_DateFormatType As DTFormatType
        Private m_DateFormatPart As DTFormatPart
        Private m_TimeFormatType As DTFormatType
        Private m_TimeFormatPart As DTFormatPart

        '<!-- previewZoom attribute mode: 0 - FullPage, 1 - PageWidth, 2 - Percent;
        'attribute percent: integer value of zoom percentage. Percentage will only be used when mode attribute is Percent -->
        '<previewZoom mode="1" percent="75" />
        Private m_PreviewZoomMode As Integer
        Private m_PreviewZoomPercent As Integer

#End Region

#Region "2. Class Constructor and Destructor Declaration."

        Public Sub New()
            'Initialize the HashTable for reports.
            m_ReportHashTable = New Hashtable
            m_ReportSyncdHash = Hashtable.Synchronized(m_ReportHashTable)
            m_ReportsPath = New Hashtable

            'Get customized format settings of culture used by current thread. (settings defined by Windows Control Panel/
            'Regional and Language Options/Regional Options/Standards and formats/Customize.../Customize Regional Options.
            m_CurrentCultureName = CultureInfo.CurrentCulture.Name
            m_CultureCustomizedLongDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern
            m_CultureCustomizedShortDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
            m_CultureCustomizedLongTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern
            m_CultureCustomizedShortTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern

            'Get default format settings of culture used by current thread. The culture default setting will not be changes
            'even though the customized setting has been set.
            Dim defaultCulture As CultureInfo = New CultureInfo(m_CurrentCultureName, False)
            m_CultureDefaultLongDateFormat = defaultCulture.DateTimeFormat.LongDatePattern
            m_CultureDefaultShortDateFormat = defaultCulture.DateTimeFormat.ShortDatePattern
            m_CultureDefaultLongTimeFormat = defaultCulture.DateTimeFormat.LongTimePattern
            m_CultureDefaultShortTimeFormat = defaultCulture.DateTimeFormat.ShortTimePattern

            m_DateFormatType = DTFormatType.CultureCustomized
            m_DateFormatPart = DTFormatPart.ShortDatePattern
            m_TimeFormatType = DTFormatType.CultureCustomized
            m_TimeFormatPart = DTFormatPart.LongTimePattern
        End Sub

#End Region

#Region "3. Class Properties Defination."

        Public ReadOnly Property PathSeparator() As String
            Get
                Return PATH_SEPARATOR
            End Get
        End Property

        Public Property ReportListStyle() As GUIStype
            Get
                Return m_ReportListStyle
            End Get
            Set(ByVal Value As GUIStype)
                m_ReportListStyle = Value
            End Set
        End Property

        Public Property ShowPrintLayout() As Boolean
            Get
                Return m_ShowPrintLayout
            End Get
            Set(ByVal Value As Boolean)
                m_ShowPrintLayout = Value
            End Set
        End Property

        Public Property ShowSingleGroupName() As Boolean
            Get
                Return m_ShowSingleGroupName
            End Get
            Set(ByVal Value As Boolean)
                m_ShowSingleGroupName = Value
            End Set
        End Property

        Public Property AutoCloseViewer() As Boolean
            Get
                Return m_AutoCloseViewer
            End Get
            Set(ByVal Value As Boolean)
                m_AutoCloseViewer = Value
            End Set
        End Property

        Public Property ReportGroups() As ArrayList
            Get
                Return m_ReportGroups
            End Get
            Set(ByVal Value As ArrayList)
                m_ReportGroups = Value
            End Set
        End Property

        Public ReadOnly Property ReportGroupCount() As Integer
            Get
                Return m_ReportGroups.Count
            End Get
        End Property

        Public Property ReportList() As ArrayList
            Get
                Return m_ReportList
            End Get
            Set(ByVal Value As ArrayList)
                m_ReportList = Value
            End Set
        End Property

        Public Property ReportSyncdHash() As Hashtable
            Get
                Return m_ReportSyncdHash
            End Get
            Set(ByVal value As Hashtable)
                m_ReportSyncdHash = value
            End Set
        End Property

        Public Property AppName() As String
            Get
                Return m_AppName
            End Get
            Set(ByVal Value As String)
                m_AppName = Value
            End Set
        End Property

        Public Property AppStartedTime() As Date
            Get
                Return m_AppStartedTime
            End Get
            Set(ByVal Value As Date)
                m_AppStartedTime = Value
            End Set
        End Property

        Public Property Company() As String
            Get
                Return m_Company
            End Get
            Set(ByVal Value As String)
                m_Company = Value
            End Set
        End Property

        Public Property Department() As String
            Get
                Return m_Department
            End Get
            Set(ByVal Value As String)
                m_Department = Value
            End Set
        End Property

        Public Property Author() As String
            Get
                Return m_Author
            End Get
            Set(ByVal Value As String)
                m_Author = Value
            End Set
        End Property
        'Added by Guo Wenyu 2014/04/08
        Public Property DBConnectionString() As String
            Get
                Return m_dbconnstring
            End Get
            Set(ByVal Value As String)
                m_dbconnstring = Value
            End Set
        End Property
        'Added by Guo Wenyu 2014/04/08
        Public Property STP_GetReportsByUser() As String
            Get
                Return m_stp_GetReportsByUser
            End Get
            Set(ByVal Value As String)
                m_stp_GetReportsByUser = Value
            End Set
        End Property
        'Added by Guo Wenyu 2014/04/08
        Public Property ReportsPath() As Hashtable
            Get
                Return m_ReportsPath
            End Get
            Set(ByVal value As Hashtable)
                m_ReportsPath = value
            End Set
        End Property

        Public Property ReportServerRUL() As String
            Get
                Return m_ReportServerRUL
            End Get
            Set(ByVal Value As String)
                m_ReportServerRUL = Value
            End Set
        End Property

        Public Property UserName() As String
            Get
                Return m_UserName
            End Get
            Set(ByVal Value As String)
                m_UserName = Value
            End Set
        End Property

        Public Property Password() As String
            Get
                Return m_Password
            End Get
            Set(ByVal Value As String)
                m_Password = Value
            End Set
        End Property

        Public Property Domain() As String
            Get
                Return m_Domain
            End Get
            Set(ByVal Value As String)
                m_Domain = Value
            End Set
        End Property

        Public ReadOnly Property CurrentCultureName() As String
            Get
                Return m_CurrentCultureName
            End Get
        End Property

        'Public ReadOnly Property CultureDefaultShortDateFormat() As String
        '    Get
        '        Return m_CultureDefaultShortDateFormat
        '    End Get
        'End Property

        'Public ReadOnly Property CultureDefaultTimeFormat() As String
        '    Get
        '        Return m_CultureDefaultShortTimeFormat
        '    End Get
        'End Property

        'Public ReadOnly Property CultureCustomizedShortDateFormat() As String
        '    Get
        '        Return m_CultureCustomizedShortDateFormat
        '    End Get
        'End Property

        'Public ReadOnly Property CultureCustomizedTimeFormat() As String
        '    Get
        '        Return m_CultureCustomizedShortTimeFormat
        '    End Get
        'End Property

        Public Property DateFormatType() As DTFormatType
            Get
                Return m_DateFormatType
            End Get
            Set(ByVal Value As DTFormatType)
                m_DateFormatType = Value
            End Set
        End Property

        Public Property DateFormatPart() As DTFormatPart
            Get
                Return m_DateFormatPart
            End Get
            Set(ByVal Value As DTFormatPart)
                m_DateFormatPart = Value
            End Set
        End Property

        Public Property TimeFormatType() As DTFormatType
            Get
                Return m_TimeFormatType
            End Get
            Set(ByVal Value As DTFormatType)
                m_TimeFormatType = Value
            End Set
        End Property

        Public Property TimeFormatPart() As DTFormatPart
            Get
                Return m_TimeFormatPart
            End Get
            Set(ByVal Value As DTFormatPart)
                m_TimeFormatPart = Value
            End Set
        End Property

        Public ReadOnly Property DateFormat() As String
            Get
                Dim Rtn As String = Nothing
                Select Case m_DateFormatType
                    Case PALS.ReportViewer.DTFormatType.CultureDefault
                        Select Case m_DateFormatPart
                            Case DTFormatPart.LongDatePattern
                                Rtn = m_CultureDefaultLongDateFormat
                            Case DTFormatPart.ShortDatePattern
                                Rtn = m_CultureDefaultShortDateFormat
                            Case Else
                                Rtn = m_CultureDefaultShortDateFormat
                        End Select
                    Case PALS.ReportViewer.DTFormatType.CultureCustomized
                        Select Case m_DateFormatPart
                            Case DTFormatPart.LongDatePattern
                                Rtn = m_CultureCustomizedLongDateFormat
                            Case DTFormatPart.ShortDatePattern
                                Rtn = m_CultureCustomizedShortDateFormat
                            Case Else
                                Rtn = m_CultureCustomizedShortDateFormat
                        End Select
                    Case Else
                        Rtn = m_CultureCustomizedShortDateFormat
                End Select

                Return Rtn
            End Get
        End Property

        Public ReadOnly Property TimeFormat() As String
            Get
                Dim Rtn As String = Nothing
                Select Case m_TimeFormatType
                    Case PALS.ReportViewer.DTFormatType.CultureDefault
                        Select Case m_TimeFormatPart
                            Case DTFormatPart.LongTimePattern
                                Rtn = m_CultureDefaultLongTimeFormat
                            Case DTFormatPart.ShortTimePattern
                                Rtn = m_CultureDefaultShortTimeFormat
                            Case Else
                                Rtn = m_CultureDefaultLongTimeFormat
                        End Select
                    Case PALS.ReportViewer.DTFormatType.CultureCustomized
                        Select Case m_TimeFormatPart
                            Case DTFormatPart.LongTimePattern
                                Rtn = m_CultureCustomizedLongTimeFormat
                            Case DTFormatPart.ShortTimePattern
                                Rtn = m_CultureCustomizedShortTimeFormat
                            Case Else
                                Rtn = m_CultureCustomizedLongTimeFormat
                        End Select
                    Case Else
                        Rtn = m_CultureCustomizedLongTimeFormat
                End Select

                Return Rtn
            End Get
        End Property


        Public Property PreviewZoomMode() As Integer
            Get
                Return m_PreviewZoomMode
            End Get
            Set(ByVal Value As Integer)
                m_PreviewZoomMode = Value
            End Set
        End Property

        Public Property PreviewZoomPercent() As Integer
            Get
                Return m_PreviewZoomPercent
            End Get
            Set(ByVal Value As Integer)
                m_PreviewZoomPercent = Value
            End Set
        End Property

#End Region

    End Class

End Namespace

