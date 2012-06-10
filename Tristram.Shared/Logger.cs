using System;
using System.Text;

namespace Tristram.Shared
{
    public enum ELogLevel
    {
        Info,
        Warn,
        Error,
        Exception,
        Debug
    }
    public static class Logger
    {
        public delegate void LogHandler(string pOutput);

        public static event LogHandler OnOutput;

        public static void WriteLine(ELogLevel pLogLevel, string pFormat, params object[] pArgs) { OnOutput(DateTime.Now.ToString() + " <" + pLogLevel.ToString() + "> " + string.Format(pFormat, pArgs)); }

        public static void Dump(byte[] pBuffer, int pStart, int pLength)
        {
            if (pLength > 0)
            {
                string[] split = (pLength > 0 ? BitConverter.ToString(pBuffer, pStart, pLength) : "").Split('-');
                StringBuilder hex = new StringBuilder(16 * 3);
                StringBuilder ascii = new StringBuilder(16);
                StringBuilder buffer = new StringBuilder();
                char temp;
                for (int index = 0; index < split.Length; ++index)
                {
                    temp = Convert.ToChar(pBuffer[pStart + index]);
                    hex.Append(split[index] + ' ');

                    if (char.IsWhiteSpace(temp) || char.IsControl(temp)) temp = '.';

                    ascii.Append(temp);
                    if ((index + 1) % 16 == 0)
                    {
                        buffer.AppendLine(string.Format("{0} {1}", hex, ascii));
                        hex.Length = 0;
                        ascii.Length = 0;
                    }
                }
                if (hex.Length > 0)
                {
                    if (hex.Length < (16 * 3)) hex.Append(new string(' ', (16 * 3) - hex.Length));
                    buffer.AppendLine(string.Format("{0} {1}", hex, ascii));
                }
                OnOutput(buffer.ToString());
            }
        }
    }
}
