

function Foo1 {
    [OutputType([System.Text.StringBuilder])]
    param()

    $sb = [System.Text.StringBuilder]::new("FOO1")
    return $sb.Append(" = FOO1;")
}

function Foo2 {
    param(
        [Parameter(ValueFromPipeline)]
        [System.Text.StringBuilder]$sb
    )

    if (-not $sb) {
        $sb = [System.Text.StringBuilder]::new()
    }

    $sb.Append("FOO2 = FOO2;") | Out-Null

    $sb.ToString()
}

Foo1 | Foo2