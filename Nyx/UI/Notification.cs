using System.Numerics;

namespace Nyx.UI
{
	public class Notification
	{
		public string Title { get; set; }
		public string Content { get; set; }
		public float TimeLeft { get; set; }
		public float Duration { get; set; }
		public Vector2 Position { get; set; }
		public float CurrentY { get; set; }
		public float Alpha { get; set; } = 0.0f;
		public double StartTime { get; set; }
		
		public Notification(string title, string content, float duration)
		{
			Title = title;
			Content = content;
			TimeLeft = duration;
			Duration = duration;
		}
	}
}
