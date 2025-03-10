using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nyx.Core
{
	public enum LogType
	{
		Info,
		Warning,
		Error,
		Debug
	}

	public static class ConsoleLogger
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AllocConsole();

		public static void Init()
		{
			if (!AllocConsole())
			{
				return;
			}

			Console.Title = "Nyx";

			var stdOut = Console.OpenStandardOutput();
			var writer = new StreamWriter(stdOut) { AutoFlush = true };
			Console.SetOut(writer);

			Console.WriteLine(@"    _   __          
   / | / /_  ___  __
  /  |/ / / / / |/_/
 / /|  / /_/ />  <  
/_/ |_/\__, /_/|_|  
      /____/        
");
		}

		public static void Log(LogType type, string message)
		{
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			Console.WriteLine($"[{timestamp}] [{type}] {message}");
		}
	}
}
