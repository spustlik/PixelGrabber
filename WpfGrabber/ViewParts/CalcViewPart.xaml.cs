using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using WpfGrabber.Services;
using WpfGrabber.Shell;

namespace WpfGrabber.ViewParts
{
    public class CalcVM : SimpleDataObject
    {

        #region Expression property
        private string _expression;
        public string Expression
        {
            get => _expression;
            set => Set(ref _expression, value);
        }
        #endregion

        #region Result property
        private string _result;
        [XmlIgnore]
        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }
        #endregion

        #region History property
        private string _history;
        public string History
        {
            get => _history;
            set => Set(ref _history, value);
        }

        #endregion
        public void AddHistory(string expression, string result)
        {
            var history = History?.Split('\n') ?? new string [0];
            if (history.LastOrDefault()?.StartsWith(expression) == true)
                history = history.Skip(1).ToArray();
            var append = $"{expression} = {result}";
            History = String.Join("\n", history) + "\n" + append;
        }

    }

    public class CalcViewPartBase : ViewPartDataViewer<CalcVM>
    {
    }

    /// <summary>
    /// Interaction logic for FileMapViewPart.xaml
    /// </summary>
    public partial class CalcViewPart : CalcViewPartBase
    {
        public CalcViewPart()
        {
            InitializeComponent();
        }

        protected override void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ViewModel_PropertyChanged(sender, e);
            if (e.PropertyName == nameof(ViewModel.Expression))
            {
                if (DoCalculate())
                {
                    ViewModel.AddHistory(ViewModel.Expression, ViewModel.Result);
                }
            }
        }

        private bool DoCalculate()
        {
            try
            {
                var r = GetResult(ViewModel.Expression, out var errors);
                if (errors != null)
                {
                    ViewModel.Result = $"Error: {errors}";
                    return false;
                }
                if (r is int num)
                {
                    r = $"{r}(0x{num:X4})";
                }
                ViewModel.Result = r?.ToString() ?? "null";
                return r != null;
            }
            catch (Exception ex)
            {
                ViewModel.Result = $"Exception: {ex.Message}";
                return false;
            }
        }
        private object GetResult(string expression, out string errors)
        {
            errors = null;
            //var provider = CodeDomProvider.CreateProvider("CSharp");
            var provider = new Microsoft.CSharp.CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
            var options = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = true
            };
            options.TempFiles.KeepFiles = false;
            options.ReferencedAssemblies.Add(typeof(System.Data.DataTable).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(System.Linq.Enumerable).Assembly.Location);
            const string CLASS = "Executor";
            const string METHOD = "Execute";
            var source = $@"
#line hidden
using System;
using System.Data;
using System.Linq;
public class {CLASS}
{{
    public static object {METHOD}()
    {{
    //#line 1 ""expression""
    return {expression};
    }}
}}";
            var result = provider.CompileAssemblyFromSource(options, source);
            if (result.Errors.Count > 0)
            {
                errors = string.Join("\n", result.Errors.Cast<CompilerError>().Where(e=>!e.IsWarning).Select(e => e.ErrorText));
                return false;
            }
            var type = result.CompiledAssembly.GetType(CLASS);
            var method = type.GetMethod(METHOD);
            return method.Invoke(null, new object[0]);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.History = "";
        }
    }
}
