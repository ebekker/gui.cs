//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DSL.Example
{
    using System;
    using Mono.Terminal;
    using Terminal.Gui;
    
    
    public partial class Example2
    {
        
        private MenuBar mainMenuBar;
        
        private Window mainWindow;
        
        private CheckBox acceptedCheckBox;
        
        private Label acceptedDateLabel;
        
        private Button okButton;
        
        private Button cancelButton;
        
        partial void okButton_Clicked();
        
        partial void cancelButton_Clicked();
        
        partial void OnExit();
        
        partial void MenuItem1_Action();
        
        partial void MenuItem2_Action();
        
        private void InitLayout()
        {
            MenuItem MenuItem1 = new MenuItem("Open", "", null);
            MenuItem MenuItem2 = new MenuItem("Close", "", null);
            MenuItem MenuItem3 = new MenuItem("Exit", "", null);
            MenuBarItem MenuBarItem1 = new MenuBarItem("File", new MenuItem[] {
                        MenuItem1,
                        MenuItem2,
                        MenuItem3});
            this.mainMenuBar = new MenuBar(new MenuBarItem[] {
                        MenuBarItem1});
            this.mainWindow = new Window("Terminal UI via DSL: Example #2");
            Label Label1 = new Label(5, 1, "First:");
            TextField TextField1 = new TextField(15, 1, 15, "");
            Label Label2 = new Label(5, 3, "Last:");
            TextField TextField2 = new TextField(15, 3, 15, "");
            Label Label3 = new Label(35, 1, "Password:");
            TextField TextField3 = new TextField(45, 1, 15, "");
            Label Label4 = new Label(35, 3, "Again:");
            TextField TextField4 = new TextField(45, 3, 15, "");
            FrameView FrameView1 = new FrameView(new Rect(5, 6, 25, 5), "ToS:");
            TextView TextView1 = new TextView(new Rect(0, 0, 23, 3));
            this.acceptedCheckBox = new CheckBox(7, 12, "Accepted");
            CheckBox CheckBox1 = new CheckBox(7, 13, "Skimmed", true);
            Label Label5 = new Label(45, 15, "Accepted Date:");
            this.acceptedDateLabel = new Label(new Rect(60, 15, 20, 1), " ");
            this.okButton = new Button(10, 18, "OK", true);
            this.cancelButton = new Button(20, 18, "Cancel");
            Button Button1 = new Button("Exit");
            // mainWindow
            mainWindow.Add(Label1);
            mainWindow.Add(TextField1);
            mainWindow.Add(Label2);
            mainWindow.Add(TextField2);
            mainWindow.Add(Label3);
            mainWindow.Add(TextField3);
            mainWindow.Add(Label4);
            mainWindow.Add(TextField4);
            mainWindow.Add(FrameView1);
            mainWindow.Add(acceptedCheckBox);
            mainWindow.Add(CheckBox1);
            mainWindow.Add(Label5);
            mainWindow.Add(acceptedDateLabel);
            mainWindow.Add(okButton);
            mainWindow.Add(cancelButton);
            mainWindow.Add(Button1);
            mainWindow.X = 0;
            mainWindow.Y = 1;
            mainWindow.Width = Dim.Fill();
            mainWindow.Height = Dim.Fill() - 2;
            // 
            TextField3.Secret = true;
            // 
            TextField4.Secret = true;
            // 
            FrameView1.Add(TextView1);
            // 
            TextView1.Text = "This is a block of hard to read\nlegalese text\nfor your review.";
            // okButton
            okButton.Clicked = () => okButton_Clicked();
            // cancelButton
            cancelButton.Clicked = () => cancelButton_Clicked();
            // 
            Button1.X = Pos.Right(mainWindow) - 10;
            Button1.Y = Pos.Bottom(mainWindow) - 5;
            Button1.Clicked = () => OnExit();
            // mainMenuBar
            MenuItem1.Action = () => MenuItem1_Action();
            MenuItem2.Action = () => MenuItem2_Action();
            MenuItem3.Action = () => OnExit();
        }
    }
}
