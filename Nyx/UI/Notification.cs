using Nyx.Core.Utils;

namespace Nyx.UI
{
	public class Notification
	{
		public string Title { get; set; }
		public string Content { get; set; }
		public float TimeLeft { get; set; }
		public Vector2 Position { get; set; }
		public float Alpha { get; set; }
		public float CurrentY { get; set; }

		public Notification(string title, string content, float duration = 5.0f)
		{
			Title = title;
			Content = content;
			TimeLeft = duration;
			Alpha = 1.0f;
		}
	}
}
