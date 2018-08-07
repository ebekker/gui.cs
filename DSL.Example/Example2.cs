

// This file will be auto-generated if missing with
// sample code, otherwise it will be left untouched
namespace DSL.Example
{
    using System;
    using Mono.Terminal;
    using Terminal.Gui;
    
    public partial class Example2
    {
        public Example2()
        {
            this.InitLayout();
        }

        partial void OnExit()
        {
            if (ConfirmExit())
                Application.Top.Running = false;
        }

        partial void okButton_Clicked()
        {
            if (!acceptedCheckBox.Checked)
            {
                MessageBox.Query(60, 10, "Accept ToS",
                        "You must accept our Terms of Service to continue!", "OK");
            }
            else
            {
                acceptedDateLabel.Text = DateTime.Now.ToString();
            }
        }

        partial void cancelButton_Clicked()
        {
            Application.Top.Running = false;
        }

        static bool ConfirmExit ()
        {
            var n = MessageBox.Query (50, 7, "Quit Demo",
                    "Are you sure you want to quit this demo?", "Yes", "No");
            return n == 0;
        }

        public static void Start()
        {
            var layout = new Example2();

            Application.Init ();

            Application.Top.Add(new View[] {
                layout.mainWindow,
                layout.mainMenuBar,
            });

    		Application.Run ();

        }
    }
}
