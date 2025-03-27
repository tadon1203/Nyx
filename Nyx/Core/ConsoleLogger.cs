using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nyx.Core;

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

	private static readonly object Lock = new();

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
		string logMessage = $"[{timestamp}] [{type}] {message}";

		Console.WriteLine(logMessage);

		try
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string nyxDirectory = Path.Combine(baseDirectory, "Nyx");
			string logFilePath = Path.Combine(nyxDirectory, "Log.txt");

			if (!Directory.Exists(nyxDirectory))
			{
				Directory.CreateDirectory(nyxDirectory);
			}

			lock (Lock)
			{
				using (StreamWriter writer = new(logFilePath, true))
				{
					writer.WriteLine(logMessage);
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"An error occurred while writing the log: {ex.Message}");
		}
	}
}