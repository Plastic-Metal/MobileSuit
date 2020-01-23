﻿#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace PlasticMetal.MobileSuit.IO
{
    partial class IoServer
    {
        public TextReader Input { get; set; }
        public bool IsInputRedirected => !Console.In.Equals(Input);

        public void ResetInput()
        {
            Input = Console.In;
        }

        public string? ReadLine(string? prompt, bool newLine, ConsoleColor? customPromptColor = null)
        {
            return ReadLine(prompt, null, newLine, customPromptColor);
        }

        public string? ReadLine(string? prompt, ConsoleColor? customPromptColor)
        {
            return ReadLine(prompt, null, false, customPromptColor);
        }

        public string? ReadLine(string? prompt, string? defaultValue,
            ConsoleColor? customPromptColor = null)
        {
            return ReadLine(prompt, defaultValue, false, customPromptColor);
        }

        public string? ReadLine(string? prompt = null, string? defaultValue = null,
            bool newLine = false, ConsoleColor? customPromptColor = null)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                if (newLine)
                    WriteLine(prompt + '>', OutputType.Prompt, customPromptColor);
                else
                    Write(prompt + '>', OutputType.Prompt, customPromptColor);
            }

            var r = Input.ReadLine();
            if (!IsInputRedirected) LastCursorLocation = (Console.CursorLeft, Console.CursorTop);
            return string.IsNullOrEmpty(r) ? defaultValue : r;
        }

        public async Task<string?> ReadLineAsync(string? prompt, bool newLine = false,
            ConsoleColor? customPromptColor = null)
        {
            return await ReadLineAsync(prompt, null, newLine, customPromptColor);
        }

        public async Task<string?> ReadLineAsync(string? prompt, ConsoleColor? customPromptColor = null)
        {
            return await ReadLineAsync(prompt, null, false, customPromptColor);
        }

        public async Task<string?> ReadLineAsync(string? prompt, string? defaultValue = null,
            ConsoleColor? customPromptColor = null)
        {
            return await ReadLineAsync(prompt, defaultValue, false, customPromptColor);
        }

        public async Task<string?> ReadLineAsync(string? prompt = null, string? defaultValue = null,
            bool newLine = false, ConsoleColor? customPromptColor = null)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                if (newLine)
                    await WriteLineAsync(prompt + '>', OutputType.Prompt, customPromptColor);
                else
                    await WriteAsync(prompt + '>', OutputType.Prompt, customPromptColor);
            }

            var r = await Input.ReadLineAsync();
            if (!IsInputRedirected) LastCursorLocation = (Console.CursorLeft, Console.CursorTop);
            return string.IsNullOrEmpty(r) ? defaultValue : r;
        }

        public int Peek()
        {
            return Input.Peek();
        }

        public int Read()
        {
            var r = Input.Read();
            if (!IsInputRedirected) LastCursorLocation = (Console.CursorLeft, Console.CursorTop);
            return r;
        }

        public string ReadToEnd()
        {
            var r = Input.ReadToEnd();
            if (!IsInputRedirected) LastCursorLocation = (Console.CursorLeft, Console.CursorTop);
            return r;
        }

        public async Task<string> ReadToEndAsync()
        {
            var r = await Input.ReadToEndAsync();
            if (!IsInputRedirected) LastCursorLocation = (Console.CursorLeft, Console.CursorTop);
            return r;
        }
    }
}