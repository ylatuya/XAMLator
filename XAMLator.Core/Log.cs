//
// Log.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//   Andoni Morales <ylatuya@gmail.com>
//
// Copyright (C) 2005-2007 Novell, Inc.
// Copyright (C) 2018 Andoni Morales Alastruey
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace XAMLator
{
	public static class Log
	{
		static readonly object writeLock = new object();

		public enum LogEntryType
		{
			Debug,
			Warning,
			Error,
			Information
		}

		public static bool Debugging { get; set; }

		public static Action<string> WriteFunc { get; set; } = DefaultWriteFunc;

		public static void Debug(string message)
		{
			if (Debugging)
			{
				Commit(LogEntryType.Debug, message);
			}
		}

		public static void Information(string message) => Commit(LogEntryType.Information, message);

		public static void Warning(string message) => Commit(LogEntryType.Warning, message);

		public static void Error(string message) => Commit(LogEntryType.Error, message);

		public static void Exception(Exception e)
		{
			Stack<Exception> exception_chain = new Stack<Exception>();
			StringBuilder builder = new StringBuilder();

			while (e != null)
			{
				exception_chain.Push(e);
				e = e.InnerException;
			}

			while (exception_chain.Count > 0)
			{
				e = exception_chain.Pop();
				builder.AppendFormat("{0}: {1} (in `{2}')", e.GetType(), e.Message, e.Source).AppendLine();
				builder.Append(e.StackTrace);
				if (exception_chain.Count > 0)
				{
					builder.AppendLine();
				}
			}
			Log.Warning($"Caught an exception {builder.ToString()}");
		}

		static void Commit(LogEntryType type, string message)
		{
			if (type == LogEntryType.Debug && !Debugging)
			{
				return;
			}

			if (type != LogEntryType.Information)
			{
				switch (type)
				{
					case LogEntryType.Error:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Red;
						break;
					case LogEntryType.Warning:
						ConsoleCrayon.ForegroundColor = ConsoleColor.DarkYellow;
						break;
					case LogEntryType.Information:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Green;
						break;
					case LogEntryType.Debug:
						ConsoleCrayon.ForegroundColor = ConsoleColor.Blue;
						break;
				}
			}

			var thread_name = String.Empty;
			if (Debugging)
			{
				thread_name = $"{Thread.CurrentThread.ManagedThreadId}";
			}

			lock (writeLock)
			{
				Write("[{5} {0} {1:00}:{2:00}:{3:00}.{4:000}]", TypeString(type), DateTime.Now.Hour,
					DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond, thread_name);
				ConsoleCrayon.ResetColor();
				Write($" {message}\n");
			}
		}

		static void Write(string format, params object[] args)
		{
			WriteFunc(string.Format(format, args));
		}

		static void DefaultWriteFunc(string msg)
		{
			Console.Write(msg);
		}

		static string TypeString(LogEntryType type)
		{
			switch (type)
			{
				case LogEntryType.Debug:
					return "Debug";
				case LogEntryType.Warning:
					return "Warn ";
				case LogEntryType.Error:
					return "Error";
				case LogEntryType.Information:
					return "Info ";
			}
			return null;
		}
	}
}
