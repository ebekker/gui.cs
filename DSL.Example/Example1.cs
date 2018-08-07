

// This file will be auto-generated if missing with
// sample code, otherwise it will be left untouched
namespace TuiApp
{
    using System;
    using Mono.Terminal;
    using Terminal.Gui;
    
    
    public partial class Example1
    {
        
        public Example1()
        {
            this.InitLayout();
        }

        partial void Button1_Clicked()
        {
            
        }

        partial void cancelButton_Clicked()
        {

        }

        partial void OnExit()
        {
            if (ConfirmExit())
                Application.Top.Running = false;
        }

        static bool ConfirmExit ()
        {
            var n = MessageBox.Query (50, 7, "Quit Demo", "Are you sure you want to quit this demo?", "Yes", "No");
            return n == 0;
        }

        public static void Start()
        {
            var layout = new Example1();

            //Application.UseSystemConsole = true;
            Application.Init ();

            var top = Application.Top;
            var tframe = top.Frame;

            top.Add(layout.mainWindow);
            top.Add(layout.mainMenuBar);

            // Application.RootMouseEvent += delegate (MouseEvent me) {
            //     ml.Text = $"Mouse: ({me.X},{me.Y}) - {me.Flags} {count++}";
            // };

    		Application.Run ();

        }
    }
}
