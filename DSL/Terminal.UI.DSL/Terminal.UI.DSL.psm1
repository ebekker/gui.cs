$dslBinPath = "$(Split-Path $PSCommandPath)\..\bin\Debug\net461"
Add-Type -Path $dslBinPath\DSL.dll
Add-Type -Path $dslBinPath\Terminal.Gui.dll

[psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')::Add('dim', [Terminal.Gui.Dim])
[psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')::Add('rect', [Terminal.Gui.Rect])

function New-Layout {
    param(
        [Parameter(Mandatory, Position=0)]
        [scriptblock]$NestedBlock,
        [Parameter()]
        [string]$Namespace,

        [string]$ClassName,
        [string]$OutPath,
        [switch]$ReturnBuilderObjects
    )

    $dslSourceFull = $PSCmdlet.MyInvocation.PSCommandPath
    $dslSourceBase = $dslSourceFull -ireplace '.tui.ps1',''

    if (-not $ClassName) {
        ## Assume classname equal to the base name of the TUI DSL file
        $ClassName = [System.IO.Path]::GetFileNameWithoutExtension(
                $dslSourceBase).Replace(".", "_")
    }
    if (-not $Namespace) {
        ## Assume the namespace equal to the name of the immediate forlder
        $Namespace = [System.IO.Path]::GetFileName(
            [System.IO.Path]::GetDirectoryName($dslSourceBase))
    }

    if (-not $OutPath) {
        $OutPath = $dslSourceBase
    }

    $TUI_BUILDER = [Terminal.UI.DSL.TuiBuilder]::new()
    $invokeResult = $NestedBlock.Invoke()
    $TUI_BUILDER.EmitCode($OutPath, $Namespace, $ClassName)

    if ($ReturnBuilderObjects) {
        return $invokeResult
    }
}
Set-Alias -Name Layout -Value Terminal.UI.DSL\New-Layout

function New-View {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Type,
        [Parameter(Position=1)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [string]$Name,
        [Parameter()]
        [array]$Args,
        [Parameter()]
        [hashtable]$Props
    )

    if ((-not $TUI_BUILDER) -or (-not ($TUI_BUILDER -is [Terminal.UI.DSL.TuiBuilder]))) {
        throw "No TUI Builder in context"
    }

    $TUI_PARENT_VIEW = $TUI_VIEW
    $TUI_PARENT_ELEMENT = $TUI_ELEMENT

    $TUI_VIEW = $TUI_BUILDER.AddView($Type, $Name);
    $TUI_ELEMENT = $TUI_VIEW

    if ($args.Count) {
        foreach ($a in $Args) {
            $TUI_VIEW.AddArg($a)
        }
    }
    if ($props.Count) {
        foreach ($p in $Props.Keys) {
            $TUI_VIEW.AddProp($p, $Props[$p])
        }
    }

    if ($NestedBlock) {
        $invokeResult = $NestedBlock.Invoke()
    }

    if ($TUI_PARENT_VIEW) {
        $TUI_PARENT_VIEW.AddChild($TUI_VIEW)
    }

    return $TUI_VIEW
}
Set-Alias -Name View -Value Terminal.UI.DSL\New-View

function Set-Property {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Name,
        [Parameter(Mandatory, Position=1, ParameterSetName='Literal')]
        [object]$Value,
        [Parameter(Mandatory, ParameterSetName='Expression')]
        [object]$Expression
    )

    if ($Expression) {
        if ($Expression -is [scriptblock]) {
            $resolved = $Expression.Invoke()
        }
        else {
            $resolved = $Expression
        }
        $resolved = [Terminal.UI.DSL.TuiExpr]::Of($resolved)
    }
    else {
        if ($Value -is [scriptblock]) {
            $resolved = $Value.Invoke()
        }
        else {
            $resolved = $Value
        }
    }

    if ((-not $TUI_ELEMENT) -or (-not ($TUI_ELEMENT -is [Terminal.UI.DSL.TuiElement]))) {
        throw "No TUI Element in context"
    }

    ([Terminal.UI.DSL.TuiElement]$TUI_ELEMENT).AddProp($Name, $resolved)
}
Set-Alias -Name "-set" -Value Set-Property

function Add-EventHandler {
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [Terminal.UI.DSL.TuiElement]$TuiElement,
        [Parameter(Mandatory, Position=1)]
        [string]$EventName,
        [Parameter(Position=2)]
        [string]$HandlerName
    )

    $eventHandler = $null
    if ($HandlerName) {
        $eventHandler = [Terminal.UI.DSL.TuiExpr]::Of($HandlerName)
    }

    $TuiElement.AddEvent($EventName, $eventHandler)
}
Set-Alias -Name "-handle" -Value Add-EventHandler

function New-MenuBar {
    param(
        [Parameter(Mandatory, Position=0)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [string]$Name
    )

    if ((-not $TUI_BUILDER) -or (-not ($TUI_BUILDER -is [Terminal.UI.DSL.TuiBuilder]))) {
        throw "No TUI Builder in context"
    }

    $TUI_PARENT_VIEW = $TUI_VIEW
    $TUI_PARENT_ELEMENT = $TUI_ELEMENT

    $TUI_MENUBAR = ([Terminal.UI.DSL.TuiBuilder]$TUI_BUILDER).AddMenuBar($Name)
    $TUI_VIEW    = $TUI_MENUBAR
    $TUI_MENUBAR = $TUI_VIEW

    $invokeResult = $NestedBlock.Invoke()

    if ($TUI_PARENT_VIEW) {
        $TUI_PARENT_VIEW.AddChild($TUI_VIEW)
    }

    return $TUI_MENUBAR
}
Set-Alias -Name MenuBar -Value Terminal.UI.DSL\New-MenuBar

function New-MenuBarItem {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Title,
        [Parameter(Mandatory, Position=1)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [string]$Name
    )

    if ((-not $TUI_MENUBAR) -or (-not ($TUI_MENUBAR -is [Terminal.UI.DSL.TuiMenuBar]))) {
        throw "No TUI MenuBar in context"
    }

    $TUI_MENUBAR_ITEM = ([Terminal.UI.DSL.TuiMenuBar]$TUI_MENUBAR).AddMenuBarItem($Title, $Name)

    $invokeResult = $NestedBlock.Invoke()

    return $TUI_MENUBAR_ITEM
}
Set-Alias -Name MenuBarItem -Value Terminal.UI.DSL\New-MenuBarItem

function New-MenuItem {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Title,

        [Parameter()]
        [string]$Help,
        [Parameter()]
        [string]$Name
    )

    if ((-not $TUI_MENUBAR_ITEM) -or (-not ($TUI_MENUBAR_ITEM -is [Terminal.UI.DSL.TuiMenuBarItem]))) {
        throw "No TUI MenuBarItem in context"
    }

    $TUI_MENU_ITEM = ([Terminal.UI.DSL.TuiMenuBarItem]$TUI_MENUBAR_ITEM).AddMenuItem($Title, $Name, $Help)

    return $TUI_MENU_ITEM
}
Set-Alias -Name MenuItem -Value Terminal.UI.DSL\New-MenuItem


function New-FrameView {
    param(
        [Parameter()]
        [string]$Title,
        [Parameter(Mandatory, Position=0)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [string]$Bounds,
        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    $viewArgs.Add($Title)
    if ($PSBoundParameters.ContainsKey("Bounds")) {
        $viewArgs.Insert(0, [Terminal.UI.DSL.TuiExpr]::Of($Bounds))
    }

    New-View -Type "FrameView" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs
}
Set-Alias -Name FrameView -Value Terminal.UI.DSL\New-FrameView


function New-Window {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Title,
        [Parameter(Mandatory, Position=1)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [rect]$Frame,
        [Parameter()]
        [int]$Padding,
        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    $viewArgs.Add($Title)
    if ($PSBoundParameters.ContainsKey("Frame")) {
        $viewArgs.Insert(0, $Frame)
    }
    if ($PSBoundParameters.ContainsKey("Padding")) {
        $viewArgs.Add($Padding)
    }

    New-View -Type "Window" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs
}
Set-Alias -Name Window -Value Terminal.UI.DSL\New-Window

function New-Button {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Text,
        [Parameter()]
        [switch]$IsDefault,
        [Parameter(Position=1)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [int]$X,
        [Parameter()]
        [int]$Y,
        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    $viewArgs.Add($Text)
    if ($IsDefault) {
        $viewArgs.Add([Terminal.UI.DSL.TuiExpr]::Of("true"))
    }
    if ($PSBoundParameters.ContainsKey("X") -or $PSBoundParameters.ContainsKey("Y")) {
        $viewArgs.Insert(0, $X)
        $viewArgs.Insert(1, $Y)
    }

    New-View -Type "Button" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs
}
Set-Alias -Name Button -Value Terminal.UI.DSL\New-Button

function New-TextView {
    param(
        [Parameter(Position=0)]
        [scriptblock]$NestedBlock,
        [string]$Text,
        [string]$Frame,

        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    if ($PSBoundParameters.ContainsKey("Frame")) {
        $viewArgs.Add([Terminal.UI.DSL.TuiExpr]::Of($Frame))
    }

    $viewProps = [ordered]@{}
    if ($PSBoundParameters.ContainsKey("Text")) {
        $viewProps.Add("Text", $Text)
    }

    New-View -Type "TextView" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs -Props $viewProps
}
Set-Alias -Name TextView -Value Terminal.UI.DSL\New-TextView

function New-TextField {
    param(
        [string]$Text,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [switch]$Secret,
        [Parameter(Position=0)]
        [scriptblock]$NestedBlock,

        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    if ($PSBoundParameters.ContainsKey("X") -or
            $PSBoundParameters.ContainsKey("Y") -or
            $PSBoundParameters.ContainsKey("Width")) {
        $viewArgs.Add($X)
        $viewArgs.Add($Y)
        $viewArgs.Add($Width)
    }
    $viewArgs.Add($Text)

    $viewProps = [ordered]@{}
    if ($Secret) {
        $viewProps.Add("Secret", [Terminal.UI.DSL.TuiExpr]::Of("true"))
    }

    New-View -Type "TextField" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs -Props $viewProps
}
Set-Alias -Name TextField -Value Terminal.UI.DSL\New-TextField


function New-Label {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Text,
        [Parameter(Position=1)]
        [scriptblock]$NestedBlock,

        [int]$X,
        [int]$Y,
        [string]$Frame,
        [ValidateSet('Left', 'Right', 'Centered', 'Justified')]
        [string]$TextAlignment,

        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    if ($PSBoundParameters.ContainsKey("X") -or
            $PSBoundParameters.ContainsKey("Y")) {
        $viewArgs.Add($X)
        $viewArgs.Add($Y)
    }
    elseif ($PSBoundParameters.ContainsKey("Frame")) {
        $viewArgs.Add([Terminal.UI.DSL.TuiExpr]::Of($Frame))
    }
    $viewArgs.Add($Text)

    $viewProps = [ordered]@{}
    if ($PSBoundParameters.ContainsKey("TextAlignment")) {
        $viewProps.Add("TextAlignment", [Terminal.UI.DSL.TuiExpr]::Of("TextAlignment.$TextAlignment"))
    }

    New-View -Type "Label" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs -Props $viewProps
}
Set-Alias -Name Label -Value Terminal.UI.DSL\New-Label


function New-CheckBox {
    param(
        [Parameter(Mandatory, Position=0)]
        [string]$Text,
        [Parameter(Position=1)]
        [scriptblock]$NestedBlock,

        [int]$X,
        [int]$Y,
        [switch]$Checked,

        [Parameter()]
        [string]$Name
    )

    $viewArgs = [System.Collections.ArrayList]::new()
    if ($PSBoundParameters.ContainsKey("X") -or
            $PSBoundParameters.ContainsKey("Y")) {
        $viewArgs.Add($X)
        $viewArgs.Add($Y)
    }
    $viewArgs.Add($Text)
    if ($Checked) {
        $viewArgs.Add([Terminal.UI.DSL.TuiExpr]::Of("true"))
    }

    New-View -Type "CheckBox" -NestedBlock $NestedBlock -Name $Name -Args $viewArgs
}
Set-Alias -Name CheckBox -Value Terminal.UI.DSL\New-CheckBox
