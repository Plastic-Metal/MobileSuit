﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PlasticMetal.MobileSuit.Core;

namespace PlasticMetal.MobileSuit.UI
{
    public partial class IOHub
    {
        private StringBuilder PrefixBuilder { get; } = new StringBuilder();
        private Stack<int> PrefixLengthStack { get; } = new Stack<int>();

        /// <summary>
        ///     Disable Time marks which shows in Output-Redirected Environment.
        /// </summary>
        public bool DisableTimeMark { get; set; }

        /// <summary>
        ///     Check if this IOServer's error stream is redirected (NOT stderr)
        /// </summary>
        public bool IsErrorRedirected => !Console.Error.Equals(ErrorStream);

        /// <summary>
        ///     Check if this IOServer's output stream is redirected (NOT stdout)
        /// </summary>
        public bool IsOutputRedirected => !Console.Out.Equals(Output);

        /// <summary>
        ///     Error stream (default stderr)
        /// </summary>
        public TextWriter ErrorStream { get; set; }

        /// <summary>
        ///     Output stream (default stdout)
        /// </summary>
        public TextWriter Output { get; set; }

        /// <summary>
        ///     The prefix of WriteLine() output, usually used to make indentation.
        /// </summary>
        public string Prefix
        {
            get => PrefixBuilder.ToString();
            set
            {
                ResetWriteLinePrefix();
                PrefixBuilder.Append(value);
                PrefixLengthStack.Push(value?.Length ?? 0);
            }
        }

        /// <summary>
        ///     Reset this IOServer's error stream to stderr
        /// </summary>
        public void ResetError()
        {
            ErrorStream = Console.Error;
        }

        /// <summary>
        ///     Reset this IOServer's output stream to stdout
        /// </summary>
        public void ResetOutput()
        {
            Output = Console.Out;
        }

        /// <summary>
        ///     Append a str to Prefix, usually used to increase indentation
        /// </summary>
        /// <param name="str">the str to append</param>
        public void AppendWriteLinePrefix(string str = "\t")
        {
            PrefixBuilder.Append(str);
            PrefixLengthStack.Push(str?.Length ?? 0);
        }

        /// <summary>
        ///     Subtract a str from Prefix, usually used to decrease indentation
        /// </summary>
        public void SubtractWriteLinePrefix()
        {
            if (PrefixLengthStack.Count == 0) return;
            var l = PrefixLengthStack.Pop();
            PrefixBuilder.Remove(PrefixBuilder.Length - l, l);
        }

        /// <summary>
        ///     Writes some content to output stream. With certain color in console.
        /// </summary>
        /// <param name="content">content to output.</param>
        /// <param name="customColor">Customized color in console</param>
        public void Write(string content, ConsoleColor? customColor)
        {
            Write(content, OutputType.Default, customColor);
        }

        /// <summary>
        ///     Writes some content to output stream. With certain color in console.
        /// </summary>
        /// <param name="content">Content to output.</param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like.</param>
        /// <param name="customColor">Optional. Customized color in console</param>
        public void Write(string content, OutputType type = OutputType.Default, ConsoleColor? customColor = null)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var dColor = Console.ForegroundColor;
                Console.ForegroundColor = SelectColor(type, customColor);
                Console.Write(content);
                Console.ForegroundColor = dColor;
            }
            else
            {
                if (type != OutputType.Prompt)
                    Output.Write(content);
            }
        }

        /// <inheritdoc />
        public async Task WriteAsync(string content, ConsoleColor frontColor, ConsoleColor backColor)
        {
            if (!IsOutputRedirected)
            {
                var dColor = Console.ForegroundColor;
                var bColor = Console.BackgroundColor;
                Console.ForegroundColor = frontColor;
                Console.BackgroundColor = backColor;
                await Output.WriteAsync(content).ConfigureAwait(false);
                Console.ForegroundColor = dColor;
                Console.BackgroundColor = bColor;
            }
            else
            {
                await Output.WriteAsync(content).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream. With certain color in console.
        /// </summary>
        /// <param name="content">content to output.</param>
        /// <param name="customColor">Customized color in console</param>
        public async Task WriteAsync(string content, ConsoleColor? customColor)
        {
            await WriteAsync(content, default, customColor).ConfigureAwait(false);
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream. With certain color in console.
        /// </summary>
        /// <param name="content">Content to output.</param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like.</param>
        /// <param name="customColor">Optional. Customized color in console</param>
        public async Task WriteAsync(string content, OutputType type = OutputType.Default,
            ConsoleColor? customColor = null)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();

                var dColor = Console.ForegroundColor;
                Console.ForegroundColor = SelectColor(type, customColor);
                await Output.WriteAsync(content).ConfigureAwait(false);
                Console.ForegroundColor = dColor;
            }
            else
            {
                if (type != OutputType.Prompt)
                    await Output.WriteAsync(content).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Write a blank line to output stream.
        /// </summary>
        public void WriteLine()
        {
            Write("\n");
        }

        /// <summary>
        ///     Writes some content to output stream, with line break. With certain color in console.
        /// </summary>
        /// <param name="content">content to output.</param>
        /// <param name="customColor">Customized color in console.</param>
        public void WriteLine(string content, ConsoleColor customColor)
        {
            WriteLine(content, default, customColor);
        }

        /// <summary>
        ///     Writes some content to output stream, with line break. With certain color in console.
        /// </summary>
        /// <param name="content">Content to output.</param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        /// <param name="customColor">Optional. Customized color in console.</param>
        public void WriteLine(string content, OutputType type = OutputType.Default, ConsoleColor? customColor = null)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var dColor = Console.ForegroundColor;
                Console.ForegroundColor = SelectColor(type, customColor);
                Console.WriteLine(Prefix + content);
                Console.ForegroundColor = dColor;
            }
            else
            {
                var sb = new StringBuilder("[");
                sb.Append(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                sb.Append(']');
                sb.Append(IIOHub.GetLabel(type));
                sb.Append(content);
                Output?.WriteLine(sb.ToString());
            }
        }
        /// <inheritdoc />
        public void Write(IEnumerable<(string, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var dColor = Console.ForegroundColor;
                var defaultColor = SelectColor(type);

                Console.ForegroundColor = defaultColor;
                foreach (var (content, color) in contentArray)
                {
                    Console.ForegroundColor = color ?? defaultColor;
                    Write(content,type,color);
                }

                Console.ForegroundColor = dColor;
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var (content, _) in contentArray) sb.Append(content);

                Output.Write(sb.ToString());
            }
        }
        /// <inheritdoc />
        public void Write(IEnumerable<(string, ConsoleColor?, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var oldForeColor = Console.ForegroundColor;
                var oldBackColor = Console.BackgroundColor;
                var defaultColor = SelectColor(type);

                Console.ForegroundColor = defaultColor;
                foreach (var (content, inputForeColor, inputBackColor) in contentArray)
                {
                    Console.ForegroundColor = inputForeColor ?? defaultColor;
                    Console.BackgroundColor = inputBackColor ?? oldBackColor;
                    Console.Write(content);
                }

                
                Console.ForegroundColor = oldForeColor;
                Console.BackgroundColor = oldBackColor;
            }
            else
            {
                var sb = new StringBuilder();

                foreach (var (content, _, _) in contentArray) sb.Append(content);

                Output.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        ///     Writes some content to output stream, with line break. With certain color for each part of content in console.
        /// </summary>
        /// <param name="contentArray">
        ///     TupleArray. FOR EACH Tuple, first is a part of content; second is optional, the color of
        ///     output (in console)
        /// </param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        public void WriteLine(IEnumerable<(string, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            Write(IsOutputRedirected ? GetLinePrefix(type) : Prefix);

            Write(contentArray, type);

            Output.WriteLine();

        }
        private string GetLinePrefix(OutputType type)
        {
            var sb = new StringBuilder();
            if (DisableTimeMark) return "";
            AppendTimeStamp(sb);
            AppendTimeStamp(sb);
            sb.Append(IIOHub.GetLabel(type));
            return sb.ToString();
        }
        private static void AppendTimeStamp(StringBuilder sb)
        {
            sb.Append('[');
            sb.Append(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            sb.Append(']');
        }
        /// <summary>
        ///     Writes some content to output stream, with line break. With certain color for each part of content in console.
        /// </summary>
        /// <param name="contentArray">
        ///     TupleArray.
        ///     FOR EACH Tuple, first is a part of content;
        ///     second is optional, the foreground color of output (in console),
        ///     third is optional, the background color of output.
        /// </param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        public void WriteLine(IEnumerable<(string, ConsoleColor?, ConsoleColor?)> contentArray,
            OutputType type = OutputType.Default)
        {
            Write(IsOutputRedirected ? GetLinePrefix(type) : Prefix);

            Write(contentArray, type);

            Output.WriteLine();
        }

        /// <summary>
        ///     Asynchronously writes a blank line to output stream.
        /// </summary>
        public async Task WriteLineAsync()
        {
            await WriteAsync("\n").ConfigureAwait(false);
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream, with line break. With certain color in console.
        /// </summary>
        /// <param name="content">content to output.</param>
        /// <param name="customColor">Customized color in console.</param>
        public async Task WriteLineAsync(string content, ConsoleColor customColor)
        {
            await WriteLineAsync(content, default, customColor).ConfigureAwait(false);
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream, with line break. With certain color in console.
        /// </summary>
        /// <param name="content">Content to output.</param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        /// <param name="customColor">Optional. Customized color in console.</param>
        public async Task WriteLineAsync(string content, OutputType type = OutputType.Default,
            ConsoleColor? customColor = null)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var dColor = Console.ForegroundColor;
                Console.ForegroundColor = SelectColor(type, customColor);
                await Output.WriteLineAsync(Prefix + content).ConfigureAwait(false);
                Console.ForegroundColor = dColor;
            }
            else
            {
                var sb = new StringBuilder();
                await WriteAsync(GetLinePrefix(type));

                sb.Append(content);
                await Output.WriteLineAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream, with line break. With certain color for each part of content
        ///     in console.
        /// </summary>
        /// <param name="contentArray">
        ///     TupleArray. FOR EACH Tuple, first is a part of content; second is optional, the color of
        ///     output (in console)
        /// </param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        public async Task WriteLineAsync(IEnumerable<(string, ConsoleColor?)> contentArray,
            OutputType type = OutputType.Default)
        {
            await WriteAsync(IsOutputRedirected ? GetLinePrefix(type) : Prefix);

            await WriteAsync(contentArray, type);

            await WriteLineAsync();
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream, with line break. With certain color for each part of content
        ///     in console.
        /// </summary>
        /// <param name="contentArray">
        ///     TupleArray. FOR EACH Tuple, first is a part of content; second is optional, the color of
        ///     output (in console)
        /// </param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        public async Task WriteLineAsync(IAsyncEnumerable<(string, ConsoleColor?)> contentArray,
            OutputType type = OutputType.Default)
        {
            await WriteAsync(IsOutputRedirected ? GetLinePrefix(type) : Prefix);

            await WriteAsync(contentArray, type);

            await WriteLineAsync();
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream, with line break. With certain color for each part of content
        ///     in console.
        /// </summary>
        /// <param name="contentArray">
        ///     TupleArray.
        ///     FOR EACH Tuple, first is a part of content;
        ///     second is optional, the foreground color of output (in console),
        ///     third is optional, the background color of output.
        /// </param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        public async Task WriteLineAsync(IEnumerable<(string, ConsoleColor?, ConsoleColor?)> contentArray,
            OutputType type = OutputType.Default)
        {
            await WriteAsync(IsOutputRedirected ? GetLinePrefix(type) : Prefix);

            await WriteAsync(contentArray, type);

            await WriteLineAsync();
        }
        /// <inheritdoc />
        public async Task WriteAsync(IAsyncEnumerable<(string, ConsoleColor?, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var oldForeColor = Console.ForegroundColor;
                var oldBackColor = Console.BackgroundColor;
                var defaultColor = SelectColor(type);

                Console.ForegroundColor = defaultColor;
                await foreach (var (content, inputForeColor, inputBackColor) in contentArray)
                {
                    Console.BackgroundColor = inputBackColor ?? oldBackColor;
                    await WriteAsync(content, type, inputForeColor ?? defaultColor).ConfigureAwait(false);
                }

                Console.ForegroundColor = oldForeColor;
                Console.BackgroundColor = oldBackColor;
            }
            else
            {
                var sb = new StringBuilder();

                await foreach (var (content, _, _) in contentArray) sb.Append(content);

                await Output.WriteAsync(sb.ToString()).ConfigureAwait(false);
            }
        }
        /// <inheritdoc />
        public async Task WriteAsync(IEnumerable<(string, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var dColor = Console.ForegroundColor;
                var defaultColor = Console.ForegroundColor = SelectColor(type);
                Console.ForegroundColor = defaultColor;
                foreach (var (content, color) in contentArray)
                {
                    await WriteAsync(content, type, color ?? defaultColor).ConfigureAwait(false);
                }


                Console.ForegroundColor = dColor;
            }
            else
            {
                var sb = new StringBuilder();

                foreach (var (content, _) in contentArray) sb.Append(content);

                await WriteAsync(sb.ToString()).ConfigureAwait(false);
            }
        }
        /// <inheritdoc />
        public async Task WriteAsync(IAsyncEnumerable<(string, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var dColor = Console.ForegroundColor;
                var defaultColor = Console.ForegroundColor = SelectColor(type);
                Console.ForegroundColor = defaultColor;
                await foreach (var (content, color) in contentArray)
                {
                    await WriteAsync(content, type, color ?? defaultColor).ConfigureAwait(false);
                }

                
                Console.ForegroundColor = dColor;
            }
            else
            {
                var sb = new StringBuilder();

                await foreach (var (content, _) in contentArray) sb.Append(content);

                await WriteAsync(sb.ToString()).ConfigureAwait(false);
            }
        }
        /// <inheritdoc />
        public async Task WriteAsync(IEnumerable<(string, ConsoleColor?, ConsoleColor?)> contentArray, OutputType type = OutputType.Default)
        {
            if (!IsOutputRedirected)
            {
                if (type == OutputType.Error) Console.Beep();
                var oldForeColor = Console.ForegroundColor;
                var oldBackColor = Console.BackgroundColor;
                var defaultColor = SelectColor(type);

                Console.ForegroundColor = defaultColor;
                foreach (var (content, inputForeColor, inputBackColor) in contentArray)
                {
                    Console.BackgroundColor = inputBackColor ?? oldBackColor;
                    await WriteAsync(content, type, inputForeColor ?? defaultColor).ConfigureAwait(false);
                }

                Console.ForegroundColor = oldForeColor;
                Console.BackgroundColor = oldBackColor;
            }
            else
            {
                var sb = new StringBuilder();

                foreach (var (content, _, _) in contentArray) sb.Append(content);

                await Output.WriteAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Asynchronously writes some content to output stream, with line break. With certain color for each part of content
        ///     in console.
        /// </summary>
        /// <param name="contentArray">
        ///     TupleArray.
        ///     FOR EACH Tuple, first is a part of content;
        ///     second is optional, the foreground color of output (in console),
        ///     third is optional, the background color of output.
        /// </param>
        /// <param name="type">Optional. Type of this content, this decides how will it be like (color in Console, label in file).</param>
        public async Task WriteLineAsync(IAsyncEnumerable<(string, ConsoleColor?, ConsoleColor?)> contentArray,
            OutputType type = OutputType.Default)
        {
            await WriteAsync(IsOutputRedirected ? GetLinePrefix(type) : Prefix);

            await WriteAsync(contentArray, type);

            await WriteLineAsync();
        }

        private ConsoleColor SelectColor(OutputType type = OutputType.Default, ConsoleColor? customColor = null)
        {
            return IColorSetting.SelectColor(ColorSetting, type, customColor);
        }

        private void ResetWriteLinePrefix()
        {
            PrefixBuilder.Clear();
            PrefixLengthStack.Clear();
        }
    }
}