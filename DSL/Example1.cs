using System.IO;
using Terminal.Gui;

namespace Terminal.UI.DSL
{
    public class ExampleBuilder
    {
        public static void Main()
        {
            var tui = new TuiBuilder();

            var menu = tui.AddMenuBar("mainMenuBar")
                .AddMenuBarItem("File")
                    .AddMenuItem("Open")
                    .AddEvent("Action", TuiExpr.Of("OnOpen"))
                .Parent
                    .AddMenuItem("Close", "closeMenutItem")
                    .AddEvent("Action", TuiExpr.Of("OnClose"))
                .Parent
                    .AddMenuItem("Exit")
                    .AddEvent("Action", TuiExpr.Of("OnExit"))
                ;

            var top = tui.AddView("Window", "mainWindow");
            top.AddArg("Hello")
                .AddProp("X", 0)
                .AddProp("Y", 1)
                .AddProp("Width", TuiExpr.Of("Dim.Fill()"))
                .AddProp("Height", TuiExpr.Of("Dim.Fill() - 1"))
                ;

            top.AddChild(tui.AddView("Label", "ml")
                .AddArg(TuiExpr.Of("new Rect(3,17, 47, 1)"))
                .AddArg("Mouse: "));

            top.AddChild(tui.AddView("Label")
                .AddArg(TuiExpr.Of("new Rect(3,18, 47, 1)"))
                .AddArg("Cursor: "));

            top.AddChild(tui.AddView("Label")
                .AddArg(TuiExpr.Of("new Rect(3,19, 47, 1)"))
                .AddArg("Keyboard: "));

            top.AddChild(tui.AddView("Button")
                // .AddArg(TuiExpr.Of("new Rect(3,19, 47, 1)"))
                .AddArg(3).AddArg(19)
                .AddArg("OK")
                .AddEvent("Clicked", null));
            top.AddChild(tui.AddView("Button", "cancelButton")
                // .AddArg(TuiExpr.Of("new Rect(3,19, 47, 1)"))
                .AddArg(3).AddArg(20)
                .AddArg("Cancel")
                .AddEvent("Clicked", null));


            var emitPath = @"..\DSL.Example\Example";
            Directory.CreateDirectory(Path.GetDirectoryName(emitPath));
            tui.EmitCode(emitPath, classname: "Example1");
        }
    }
}