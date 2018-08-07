Import-Module ..\DSL\Terminal.UI.DSL -Force

$winTitle = "Terminal UI via DSL"
$winSubtitle = "Example #2"

Layout {
    MenuBar -Name mainMenuBar {
        MenuBarItem "File" {
            MenuItem "Open" | -handle Action
            MenuItem "Close" | -handle Action
            MenuItem "Exit" | -handle Action OnExit
        }
    }

    Window -Name mainWindow "$winTitle`: $winSubtitle" {
        -set X 0
        -set Y 1
        -set Width -Expr "Dim.Fill()"
        -set Height -Expr "Dim.Fill() - 2"

        ## Name fields
        Label     -X 5  -Y 1 "First:"
        TextField -X 15 -Y 1 -Width 15
        Label     -X 5  -Y 3 "Last:"
        TextField -X 15 -Y 3 -Width 15

        ## Collect a Password
        Label     -X 35 -Y 1 "Password:"
        TextField -X 45 -Y 1 -Width 15 -Secret
        Label     -X 35 -Y 3 "Again:"
        TextField -X 45 -Y 3 -Width 15 -Secret

        ## ToS Languge and Acceptance
        FrameView -Title "ToS:" -Bounds "new Rect(5, 6, 25, 5)" {
            TextView -Text "This is a block of hard to read\nlegalese text\nfor your review." `
                -Frame "new Rect(0, 0, 23, 3)"
        }
        Checkbox -X 7 -Y 12 "Accepted" -Name acceptedCheckBox
        Checkbox -X 7 -Y 13 "Skimmed" -Checked

        ## Captures date of ToS Acceptance
        Label -X 45 -Y 15 "Accepted Date:"
        Label -Frame "new Rect(60, 15, 20, 1)" " " -Name acceptedDateLabel

        ## Submit or Abort
        Button -Name okButton     -X 10 -Y 18 "OK" -IsDefault |
            -handle Clicked
        Button -Name cancelButton -X 20 -Y 18 "Cancel" |
            -handle Clicked

        ## Graceful Exit -- reuses existing handler as menu item
        Button -Text "Exit" {
            -set X -Expr "Pos.Right(mainWindow) - 10"
            -set Y -Expr "Pos.Bottom(mainWindow) - 5"
        } | -handle Clicked OnExit

    }
}