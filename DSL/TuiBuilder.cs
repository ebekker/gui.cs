using System;
using System.Text;
using System.Collections.Generic;
using Terminal.Gui;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Terminal.UI.DSL
{
    public class TuiBuilder
    {
        public CodeCompileUnit CodeUnit1 { get; set; }
        public CodeCompileUnit CodeUnit2 { get; set; }
        public CodeTypeDeclaration CodeClass1 { get; set; }
        public CodeTypeDeclaration CodeClass2 { get; set; }
        public CodeMemberMethod CodeInitMethod { get; set; }
        

        private Dictionary<string, int> _typeCounts = new Dictionary<string, int>();
        private List<TuiView> _views = new List<TuiView>();
        private List<TuiMenuBar> _menuBars = new List<TuiMenuBar>();

        public string LastLocalName { get; set; }
        public TuiView LastView { get; set; }
        
        public void EmitCode(string pathPrefix,
            string nsName = null,
            string classname = null)
        {
            this.CodeUnit1 = new CodeCompileUnit();
            this.CodeUnit2 = new CodeCompileUnit();

            if (string.IsNullOrEmpty(nsName))
                nsName = "TuiApp";
            if (string.IsNullOrEmpty(classname))
                classname = "TuiAppLayout";

            var codeNs1 = new CodeNamespace(nsName);
            var codeNs2 = new CodeNamespace(nsName);
            codeNs2.Comments.Clear();
            codeNs2.Comments.Add(new CodeCommentStatement("This file will be auto-generated if missing with"));
            codeNs2.Comments.Add(new CodeCommentStatement("sample code, otherwise it will be left untouched"));

            CodeUnit1.Namespaces.Add(codeNs1);
            codeNs1.Imports.Add(new CodeNamespaceImport("System"));
            codeNs1.Imports.Add(new CodeNamespaceImport("Mono.Terminal"));
            codeNs1.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            CodeUnit2.Namespaces.Add(codeNs2);
            codeNs2.Imports.Add(new CodeNamespaceImport("System"));
            codeNs2.Imports.Add(new CodeNamespaceImport("Mono.Terminal"));
            codeNs2.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            this.CodeClass1 = new CodeTypeDeclaration(classname)
            {
                IsPartial = true,
            };
            codeNs1.Types.Add(CodeClass1);

            this.CodeClass2 = new CodeTypeDeclaration(classname)
            {
                IsPartial = true,
            };
            codeNs2.Types.Add(CodeClass2);

            var menuVars = new CodeStatementCollection();
            var eventHandlers = new List<string>();

            void InitTuiElement(string type, TuiElement te,
                IEnumerable<CodeExpression> args)
            {
                te.LocalName = te.LocalName ?? LocalName(type, te.Name);
                var teInitExpr = new CodeObjectCreateExpression(type);
                if (args != null)
                    foreach (var a in args)
                        teInitExpr.Parameters.Add(a);

                if (!string.IsNullOrEmpty(te.Name))
                {
                    CodeClass1.Members.Add(
                        new CodeMemberField(type, te.Name));
                    menuVars.Add(new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            te.Name), teInitExpr));
                }
                else
                {
                    menuVars.Add(new CodeVariableDeclarationStatement(
                        type, te.LocalName, teInitExpr));
                }
            }

            foreach (var mb in _menuBars)
            {
                if (mb.Items == null)
                    continue;

                foreach (var mbi in mb.Items)
                {
                    if (mbi.Items == null)
                        continue;

                    foreach (var mi in mbi.Items)
                    {
                        var help = mi.Help == null ? "null" : $@"""{mi.Help}""";
                        InitTuiElement("MenuItem", mi, new CodeExpression[] {
                            new CodeSnippetExpression($@"""{mi.Title}"""),
                            new CodeSnippetExpression(help),
                            new CodeSnippetExpression("null")
                            });
                    }
                }

                foreach (var mbi in mb.Items)
                {
                    // We have to build an array of MI even if its empty
                    // because MBI only has one constructor signature
                    var arr = new CodeArrayCreateExpression("MenuItem");
                    if (mbi.Items != null)
                    {
                        foreach (var mi in mbi.Items)
                            arr.Initializers.Add(
                                new CodeVariableReferenceExpression(mi.LocalName));
                    }

                    InitTuiElement("MenuBarItem", mbi, new CodeExpression[] {
                        new CodeSnippetExpression($@"""{mbi.Title}"""),
                        arr });
                }
            }

            foreach (var v in _menuBars.Concat(_views))
            {
                if (!string.IsNullOrEmpty(v.Name))
                    CodeClass1.Members.Add(new CodeMemberField(v.Type, v.Name));
            }

            this.CodeInitMethod = new CodeMemberMethod()
            {
                Name = "InitLayout",
                Attributes = MemberAttributes.Private,
            };
            CodeClass1.Members.Add(CodeInitMethod);
            CodeInitMethod.Statements.AddRange(menuVars);

            // View/MB constructors with args
            foreach (var v in _menuBars.Concat(_views))
            {
                var args = v.Args?.Count > 0
                    ? string.Join(", ", v.Args.Select(x => x.Emit()))
                    : string.Empty;
                CodeExpression initExpr = new CodeSnippetExpression(
                        $"new {v.Type}({args})");

                if (v is TuiMenuBar mb)
                {
                    var arr = new CodeArrayCreateExpression("MenuBarItem");
                    foreach (var mbi in mb.Items)
                        arr.Initializers.Add(
                            new CodeVariableReferenceExpression(mbi.LocalName));
                    initExpr = new CodeObjectCreateExpression(v.Type, arr);
                }

                if (string.IsNullOrEmpty(v.Name))
                    CodeInitMethod.Statements.Add(new CodeVariableDeclarationStatement(
                            v.Type, v.LocalName, initExpr));
                else
                    CodeInitMethod.Statements.Add(new CodeAssignStatement(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), v.Name),
                            initExpr));
            }

            foreach (var v in _menuBars.Concat(_views))
            {
                var stt = new CodeStatementCollection();

                if (v.Children?.Count > 0)
                {
                    foreach (var c in v.Children)
                    {
                        var mre = new CodeMethodReferenceExpression(
                            new CodeVariableReferenceExpression(v.LocalName), "Add");
                        var mie = new CodeMethodInvokeExpression(mre,
                            new CodeVariableReferenceExpression(c.LocalName));

                        stt.Add(mie);
                    }
                }

                if (v.Props?.Count > 0)
                {
                    foreach (var p in v.Props)
                    {
                        var cas = new CodeAssignStatement(
                            new CodePropertyReferenceExpression(
                                new CodeVariableReferenceExpression(v.LocalName), p.Key),
                            new CodeSnippetExpression(p.Value.Emit())
                        );
                        stt.Add(cas);
                    }
                }

                AddEvents(stt, v);

                if (stt.Count > 0)
                {
                    CodeInitMethod.Statements.Add(new CodeCommentStatement(v.Name ?? $"({v.LocalName})"));
                    CodeInitMethod.Statements.AddRange(stt);
                }
            }

            foreach (var mb in _menuBars)
            {
                var stt = new CodeStatementCollection();

                if (mb.Items == null)
                    continue;

                foreach (var mbi in mb.Items)
                {
                    if (mbi.Items == null)
                        continue;

                    foreach (var mi in mbi.Items)
                    {
                        AddEvents(stt, mi);
                    }
                }

                if (stt.Count > 0)
                {
                    CodeInitMethod.Statements.Add(new CodeCommentStatement(mb.Name ?? $"({mb.LocalName})"));
                    CodeInitMethod.Statements.AddRange(stt);
                }
            }

            void AddEvents(CodeStatementCollection stt, TuiElement te)
            {
                if (te.Events?.Count > 0)
                {
                    foreach (var e in te.Events)
                    {
                        var eh = e.Value?.ToString();
                        if (eh == null)
                            eh = $"{te.LocalName}_{e.Key}";
                            // eh = $"{te.Name ?? te.LocalName.Replace(".", "_")}_{e.Key}";

                        var cas = new CodeAssignStatement(
                            new CodePropertyReferenceExpression(
                                new CodeVariableReferenceExpression(te.LocalName), e.Key),
                            new CodeSnippetExpression($"() => {eh}()")
                        );
                        stt.Add(cas);

                        if (!eventHandlers.Contains(eh))
                        {
                            // Adding Partial Method is a hack:
                            //    https://stackoverflow.com/a/2164838/5428506
                            CodeClass1.Members.Add(new CodeMemberField
                            {
                                Name = eh + "()",
                                Attributes = MemberAttributes.ScopeMask,
                                Type = new CodeTypeReference("partial void"),
                            });
                            eventHandlers.Add(eh);
                        }
                    }
                }
            }

            var codeCons = new CodeConstructor()
            {
                Attributes = MemberAttributes.Public,
            };
            CodeClass2.Members.Add(codeCons);
            codeCons.Statements.Add(new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                            new CodeThisReferenceExpression(),
                            "InitLayout")));

            var file1 = pathPrefix + ".tui.cs";
            var file2 = pathPrefix + ".cs";

            var prov = CodeDomProvider.CreateProvider("CSharp");;
            var opts = new CodeGeneratorOptions
            {
                BracingStyle = "C",
            };

            Directory.CreateDirectory(Path.GetDirectoryName(file1));
            using (var fs = File.Open(file1, FileMode.Create))
            using (var sw = new StreamWriter(fs))
            {
                prov.GenerateCodeFromCompileUnit(CodeUnit1, sw, opts);
            }

            if (!File.Exists(file2))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file2));
                string body;
                using (var sw = new StringWriter())
                {
                    prov.GenerateCodeFromCompileUnit(CodeUnit2, sw, opts);
                    body = sw.ToString();
                }

                body = Regex.Replace(body,
                        @"//-+\s+// <auto-generated>[\S\s]+</auto-generated>\s+//-+",
                        "", RegexOptions.Multiline);
                
                File.WriteAllText(file2, body);
            }
        }

	    public TuiView AddView(string type, string name = null)
        {
            var last = LastView;
            LastView = new TuiView
            {
                Builder = this,
                Type = type,
                Name = name,
                LocalName = LocalName(type, name),
            };
            _views.Add(LastView);
            return LastView;
        }

        public TuiMenuBar AddMenuBar(string name = null)
        {
            var menu = new TuiMenuBar
            {
                Builder = this,
                Type = nameof(MenuBar),
                Name = name,
                LocalName = LocalName(name, nameof(MenuBar)),
            };
            _menuBars.Add(menu);
            return menu;
        }

        private string LocalName(string type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                LastLocalName = name;
            }
            else
            {
                _typeCounts.TryGetValue(type, out var count);
                LastLocalName = $"{type}{++count}";
                _typeCounts[type] = count;
            }
            return LastLocalName;
        }
    }

    public class TuiValue
    {
        public object Value { get; set; }

        public virtual string Emit()
        {
            if (Value is null)
                return "null";

            if (Value is string s)
                return $@"""{s}""";
            
            return Value.ToString();
        }

        public override string ToString() => Emit();

        public static TuiValue Of(object value) => new TuiValue { Value = value };

        public static implicit operator TuiValue(string value) => new TuiValue { Value = value };
        public static implicit operator TuiValue(int value) => new TuiValue { Value = value };
        public static implicit operator TuiValue(long value) => new TuiValue { Value = value };
    }

    public class TuiExpr : TuiValue
    {
        public override string Emit() => Value.ToString();

        public static TuiExpr Of(string value) => new TuiExpr { Value = value };
    }

    public class TuiMenuBar : TuiView
    {
        public List<TuiMenuBarItem> Items { get; set; }

        public TuiMenuBarItem AddMenuBarItem(string title, string name = null)
        {
            var item = new TuiMenuBarItem
            {
                Parent = this,
                Title = title,
                Name = name,
            };

            Items = Items ?? new List<TuiMenuBarItem>();
            Items.Add(item);

            return item;
        }
    }

    public class TuiMenuBarItem : TuiElement
    {
        public string Title { get; set; }

        public TuiMenuBar Parent { get; set; }

        public List<TuiMenuItem> Items { get; set; }

        public TuiMenuItem AddMenuItem(string title, string name = null, string help = null)
        {
            var item = new TuiMenuItem
            {
                Parent = this,
                Title = title,
                Name = name,
                Help = help,
            };

            Items = Items ?? new List<TuiMenuItem>();
            Items.Add(item);

            return item;
        }
    }

    public class TuiMenuItem : TuiElement
    {
        public string Title { get; set; }

        public string Help { get; set; }

        public TuiMenuBarItem Parent { get; set; }

        public new TuiMenuItem AddArg(TuiValue value) => (TuiMenuItem)base.AddArg(value);
        public new TuiMenuItem AddProp(string name, TuiValue value) => (TuiMenuItem)base.AddProp(name, value);
        public new TuiMenuItem AddEvent(string name, TuiValue value) => (TuiMenuItem)base.AddEvent(name, value);
    }

    public class TuiView : TuiElement
    {
        public string Type { get; set; }

        public TuiView Parent { get; set; }

        public IList<TuiView> Children { get; set; }

        public new TuiView AddArg(TuiValue value) => (TuiView)base.AddArg(value);
        public new TuiView AddProp(string name, TuiValue value) => (TuiView)base.AddProp(name, value);
        public new TuiView AddEvent(string name, TuiValue value) => (TuiView)base.AddEvent(name, value);

        /// <summary>
        /// Add child.  Returns this instance (not child).
        /// </summary>
        public TuiView AddChild(TuiView child)
        {
            Children = Children ?? new List<TuiView>();

            // TODO: should we worry about loop detection
            //       or leave user to their own peril?

            child.Parent?.Children.Remove(child);
            child.Parent = this;
            Children.Add(child);

            return this;
        }
    }

    public class TuiElement
    {
        public TuiBuilder Builder { get; set; }

        public string Name { get; set; }

        public string LocalName { get; set; }

        public IList<TuiValue> Args { get; set; }
        public IList<KeyValuePair<string, TuiValue>> Props { get; set; }
        public IList<KeyValuePair<string, object>> Events { get; set; }

        /// <summary>
        /// Add construtor argument.  Returns this instance.
        /// </summary>
        public TuiElement AddArg(TuiValue value)
        {
            Args = Args ?? new List<TuiValue>();
            Args.Add(value);
            return this;
        }

        /// <summary>
        /// Add property initializer.  Returns this instance.
        /// </summary>
        public TuiElement AddProp(string name, TuiValue value)
        {
            Props = Props ?? new List<KeyValuePair<string, TuiValue>>();
            Props.Add(new KeyValuePair<string, TuiValue>(name, value));
            return this;
        }

        /// <summary>
        /// Add event handler.  Returns this instance.
        /// </summary>
        public TuiElement AddEvent(string name, TuiValue value)
        {
            Events = Events ?? new List<KeyValuePair<string, object>>();
            Events.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }
    }
}
