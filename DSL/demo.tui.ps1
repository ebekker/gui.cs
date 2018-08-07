
[psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')::Add('dim', [Terminal.Gui.Dim])
[psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')::Add('rect', [Terminal.Gui.Rect])

# function New-Rect {
#     param(
#         [Parameter(Mandatory)]
#         [int]$X,
#         [Parameter(Mandatory)]
#         [int]$Y,
#         [Parameter(Mandatory)]
#         [int]$Width,
#         [Parameter(Mandatory)]
#         [int]$Height
#     )
#     [Terminal.Gui.Rect]::new($X, $Y, $Width, $Height)
# }
# Set-Alias -Name "rect" -Value New-Rect

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
    }
    else {
        if ($Value -is [scriptblock]) {
            $resolved = $Value.Invoke()
        }
        else {
            $resolved = $Value
        }
        if ($resolved -is [string]) {
            $resolved = """$resolved"""
        }
    }

    $top[$Name] = $resolved
}
Set-Alias -Name "-set" -Value Set-Property

function Add-EventHandler {
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [System.Text.StringBuilder]$ViewStringBuilder,
        [Parameter(Mandatory, Position=1)]
        [string]$EventName
    )

    $ViewStringBuilder.Append("view.$EventName = () => { }") | Out-Null
}
Set-Alias -Name "-handle" -Value Add-EventHandler

function New-Window {
    param(
        [Parameter(Mandatory, Position=0, ParameterSetName="WithTitle")]
        [string]$Title,
        [int]$Padding,
        [Terminal.Gui.Rect]$Rect,
        [string]$Name='topView',

        [Parameter(Mandatory, Position=1, ParameterSetName="WithTitle")]
        [Parameter(Mandatory, Position=0, ParameterSetName="Default")]
        [scriptblock]$ChildrenBlock
    )

    
    $top = [ordered]@{ '$Name' = $Name }
    $children = [System.Collections.ArrayList]::new()
    $topVar = [psvariable]::new("Top", $top)
    $childrenVar = [psvariable]::new('Children', $children)
    $IGNORE = $ChildrenBlock.InvokeWithContext($null, $topVar)

    $buff = ''
    if ($top['$Name']) {
        $buff += "var $($top['$Name']) = "
    }
    $buff += 'new Window('
    if ($Rect) {
        $buff += "$Rect"
    }
    if ($Title) {
        $buff += """$Title"""
    }
    else {
        $buff += 'null'
    }
    if ($Padding) {
        $buff += ", $Padding"
    }
    $buff += ')'

    Write-Output $buff
    #Write-Output "New-Window ($Title, $Padding) $($top|ConvertTo-Json -Compress)"

    Write-Output '{'
    foreach ($p in $top.Keys) {
        if ($p.StartsWith('$')) {
            continue
        }
        Write-Output "  $p = $($top[$p]),"
    }
    Write-Output '};'
    if ($children.Count -gt 0) {
        Write-Output "$($top['$Name']).Add("
        Write-Output "    $($children -join ",`r`n    ")"
        Write-Output ');'
    }

    Write-Output "--8<--------------------------"
    Write-Output $IGNORE
}
Set-Alias -Name Window -Value New-Window

function Add-Label {
    [OutputType([System.Text.StringBuilder])]
    param(
        [Parameter(Mandatory, Position=0, ParameterSetName="Rect")]
        [rect]$Rect,
        [Parameter(Mandatory, Position=0, ParameterSetName="XY")]
        [int]$X,
        [Parameter(Mandatory, Position=1, ParameterSetName="XY")]
        [int]$Y,

        [Parameter(Position=1, ParameterSetName="Rect")]
        [Parameter(Position=2, ParameterSetName="XY")]
        [Parameter(Position=0)]
        [string]$Text
    )

    $buff = [System.Text.StringBuilder]::new("new Label(")
    if ($PSCmdlet.ParameterSetName -eq 'Rect') {
        $buff.Append("$Rect, ") | Out-Null
    }
    elseif ($PSCmdlet.ParameterSetName -eq 'XY') {
        $buff.Append("$X, $Y, ") | Out-Null
    }
    if ($Text) {
        $buff.Append("""$Text""") | Out-Null
    }
    else {
        $buff.Append('null') | Out-Null
    }
    $buff.Append(')') | Out-Null
    $children.Add($buff) | Out-Null

    $buff
}
Set-Alias -Name Label -Value Add-Label

function Add-Button {
    [OutputType([System.Text.StringBuilder])]
    param(
        [Parameter(Mandatory, Position=0, ParameterSetName="XY")]
        [int]$X,
        [Parameter(Mandatory, Position=1, ParameterSetName="XY")]
        [int]$Y,

        [Parameter(Position=2, ParameterSetName="XY")]
        [Parameter(Position=0, ParameterSetName="Default")]
        [string]$Text,

        [switch]$IsDefault
    )

    $buff = [System.Text.StringBuilder]::new("new Button(")
    if ($PSCmdlet.ParameterSetName -eq 'XY') {
        $buff.Append("$X, $Y, ") | Out-Null
    }
    if ($Text) {
        $buff.Append("""$Text""") | Out-Null
    }
    else {
        $buff.Append("null") | Out-Null
    }
    if ($IsDefault) {
        $buff.Append(', true') | Out-Null
    }
    $buff.Append(')') | Out-Null
    $children.Add($buff) | Out-Null

    $buff
}
Set-Alias -Name Button -Value Add-Button


Window "My Window" {
    -set Width 50
    -set Height 50

    Button "OK" -IsDefault
    Button "Cancel"
}

Window {
    -name TopWindow
    -set Title "Another Window"
    -set Width { 5 + 5 }

    Label 5 2 "Label 1:" 
    Label ([rect]::new(5, 3, 30, 1)) "Label 2:"

    Button 20 2 "Button 1" | -handle Clicked
    Button 35 3 "Button 2" | -handle Clicked
}
