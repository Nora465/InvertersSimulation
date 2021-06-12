Namespace MyModbusTCP
    Public Class ModbusServer
        Private debug As Boolean = False

        'Public Property LogFileFilename As String
        '    Get
        '        Return StoreLogData.Instance.Filename
        '    End Get
        '    Set(ByVal value As String)
        '        StoreLogData.Instance.Filename = value

        '        If StoreLogData.Instance.Filename IsNot Nothing Then
        '            Debug = True
        '        Else
        '            Debug = False
        '        End If
        '    End Set
        'End Property
    End Class

    Public Class HoldingRegisters
        Public localArray As Int16() = New Int16(65534) {}
        Private modbusServer As ModbusServer

        Public Sub New(ByVal modbusServer As MyModbusTCP.ModbusServer)
            Me.modbusServer = modbusServer
        End Sub

        Default Public Property Item(ByVal x As Integer) As Int16
            Get
                Return Me.localArray(x)
            End Get
            Set(ByVal value As Int16)
                Me.localArray(x) = value
            End Set
        End Property
    End Class

    Public Class InputRegisters
        Public localArray As Int16() = New Int16(65534) {}
        Private modbusServer As ModbusServer

        Public Sub New(ByVal modbusServer As MyModbusTCP.ModbusServer)
            Me.modbusServer = modbusServer
        End Sub

        Default Public Property Item(ByVal x As Integer) As Int16
            Get
                Return Me.localArray(x)
            End Get
            Set(ByVal value As Int16)
                Me.localArray(x) = value
            End Set
        End Property
    End Class

    Public Class Coils
        Public localArray As Boolean() = New Boolean(65534) {}
        Private modbusServer As ModbusServer

        Public Sub New(ByVal modbusServer As MyModbusTCP.ModbusServer)
            Me.modbusServer = modbusServer
        End Sub

        Default Public Property Item(ByVal x As Integer) As Boolean
            Get
                Return Me.localArray(x)
            End Get
            Set(ByVal value As Boolean)
                Me.localArray(x) = value
            End Set
        End Property
    End Class

    Public Class DiscreteInputs
        Public localArray As Boolean() = New Boolean(65534) {}
        Private modbusServer As ModbusServer

        Public Sub New(ByVal modbusServer As MyModbusTCP.ModbusServer)
            Me.modbusServer = modbusServer
        End Sub

        Default Public Property Item(ByVal x As Integer) As Boolean
            Get
                Return Me.localArray(x)
            End Get
            Set(ByVal value As Boolean)
                Me.localArray(x) = value
            End Set
        End Property
    End Class

End Namespace
