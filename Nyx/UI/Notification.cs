using System.Numerics;

namespace Nyx.UI;

public class Notification(string title, string content, float duration)
{
	public string Title { get; set; } = title;
	public string Content { get; set; } = content;
	public float TimeLeft { get; set; } = duration;
	public float Duration { get; set; } = duration;
	public Vector2 Position { get; set; }
	public float CurrentY { get; set; }
	public float Alpha { get; set; }
	public double StartTime { get; set; }
}