using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace OOS.Terminal
{
    public partial class CommandWindow : Window
    {
        private readonly FakeTerminal _terminal = new();

        public CommandWindow()
        {
            InitializeComponent();
            AppendOutput("OfficeOfShadows Terminal\nType 'help' for commands.\n");
        }

        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var input = InputBox.Text?.Trim() ?? "";
                InputBox.Clear();
                AppendOutput($"> {input}\n");

                var response = await Task.Run(() => _terminal.Execute(input));
                await PrintLinesAnimated(response);
                e.Handled = true;
            }
        }

        private void AppendOutput(string text)
        {
            OutputText.Text += text;
            OutputScroll.ScrollToEnd();
        }

        private async Task PrintLinesAnimated(string text)
        {
            var lines = text.Replace("\r", "").Split('\n');
            foreach (var line in lines)
            {
                await TypeLine(line + "\n");
                await Task.Delay(60);
            }
        }

        private async Task TypeLine(string line)
        {
            foreach (var ch in line)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    OutputText.Text += ch;
                    OutputScroll.ScrollToEnd();
                }, DispatcherPriority.Background);
                await Task.Delay(8);
            }
        }
    }
}
