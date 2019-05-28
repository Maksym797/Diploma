using System;
using System.Windows.Forms;

namespace SimAGS.Handlers
{
    public class CustomMessageHandler
    {
        private readonly string _message;
        protected static Action<string> _messageShower { get; private set; } = (m) => MessageBox.Show(m);
        private static TextBox _textBox { get; set; }

        public static void Config(TextBox viewer)
        {
            _textBox = viewer;
            _messageShower = (m) =>
            {
                _textBox.Text += $"{m.Replace("\n", "\r\n")}\r\n";
            };
        }

        private CustomMessageHandler(string message)
        {
            _message = message;
        }

        public static CustomMessageHandler Show(string message)
        {
            _messageShower(message);
            return new CustomMessageHandler(message);
        }

        public static CustomMessageHandler println(string message)
        {
            _messageShower(message);
            return new CustomMessageHandler(message);
        }
        public static CustomMessageHandler print(string message)
        {
            _messageShower(message);
            return new CustomMessageHandler(message);
        }

        #region methods-chaining

        public CustomMessageHandler SetColor()
        {
            throw new NotImplementedException();
        }

        public CustomMessageHandler MessBox()
        {
            MessageBox.Show(_message);
            return this;
        }

        #endregion
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
