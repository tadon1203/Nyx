using ImGuiNET;
using Nyx.Core.Utils;
using Nyx.UI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Nyx.Core.Managers;

public static class NotificationManager
{
	private static List<Notification> _notifications = new();
	private static float _padding = 12.0f;
	private static float _width = 300.0f;
	private static float _fadeOutTime = 1.5f;
		
	private static Vector4 _accentColor = new Vector4(148f/255f, 226f/255f, 213f/255f, 1.0f);
		
	public static void AddNotification(string title, string content, float duration = 5.0f)
	{
		var notification = new Notification(title, content, duration);
		Vector2 windowSize = ImGui.GetIO().DisplaySize;
		notification.Position = new Vector2(windowSize.X - _width - _padding, windowSize.Y - _padding);
		notification.CurrentY = windowSize.Y - _padding;
		notification.StartTime = ImGui.GetTime();
		_notifications.Add(notification);
	}
		
	public static void Update(float deltaTime)
	{
		for (int i = _notifications.Count - 1; i >= 0; i--)
		{
			Notification notification = _notifications[i];
			notification.TimeLeft -= deltaTime;
				
			if (notification.TimeLeft <= 0)
			{
				_notifications.RemoveAt(i);
				continue;
			}
				
			if (notification.TimeLeft < _fadeOutTime)
			{
				notification.Alpha = notification.TimeLeft / _fadeOutTime;
					
				notification.Position = new Vector2(
					notification.Position.X + (12.0f * deltaTime),
					notification.Position.Y
				);
			}
			else
			{
				float timeSinceStart = (float)(ImGui.GetTime() - notification.StartTime);
				if (timeSinceStart < 0.3f)
				{
					notification.Alpha = Math.Min(timeSinceStart / 0.3f, 1.0f);
				}
			}
		}
			
		Vector2 windowSize = new(UnityEngine.Screen.width, UnityEngine.Screen.height);
		float targetY = windowSize.Y - _padding;
			
		for (int i = 0; i < _notifications.Count; i++)
		{
			Notification notification = _notifications[i];
			float height = CalculateNotificationHeight(notification);
			targetY -= height;
				
			float easeFactor = 10.0f * deltaTime;
			notification.CurrentY = MathUtils.Lerp(notification.CurrentY, targetY, easeFactor);
			notification.Position = new Vector2(windowSize.X - _width - _padding, notification.CurrentY);
			targetY -= _padding;
		}
	}
		
	public static void Render()
	{
		ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
		foreach (var notification in _notifications)
		{
			RenderNotification(drawList, notification);
		}
	}
		
	private static void RenderNotification(ImDrawListPtr drawList, Notification notification)
	{
		Vector2 position = notification.Position;
		float height = CalculateNotificationHeight(notification);
			
		uint backgroundColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.12f, 0.14f, 0.17f, notification.Alpha * 0.95f));
		uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
			_accentColor.X, _accentColor.Y, _accentColor.Z, notification.Alpha * 0.7f));
		uint titleColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, notification.Alpha));
		uint contentColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.9f, 0.9f, notification.Alpha * 0.9f));
		uint accentBarColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
			_accentColor.X, _accentColor.Y, _accentColor.Z, notification.Alpha));
			
		float cornerRadius = 6.0f;
			
		drawList.AddRectFilled(
			position,
			new Vector2(position.X + _width, position.Y + height),
			backgroundColor,
			cornerRadius
		);
			
		drawList.AddRectFilled(
			position,
			new Vector2(position.X + 4.0f, position.Y + height),
			accentBarColor,
			cornerRadius, 
			ImDrawFlags.RoundCornersLeft
		);
			
		float timeRatio = notification.TimeLeft / notification.Duration;
		float progressWidth = _width * timeRatio;
			
		drawList.AddRectFilled(
			new Vector2(position.X, position.Y),
			new Vector2(position.X + progressWidth, position.Y + 2.0f),
			accentBarColor,
			0.0f
		);
			
		drawList.AddRect(
			position,
			new Vector2(position.X + _width, position.Y + height),
			borderColor,
			cornerRadius,
			ImDrawFlags.RoundCornersAll,
			1.0f
		);
			
		Vector2 titlePos = new Vector2(position.X + _padding + 4.0f, position.Y + _padding);
			
		drawList.AddText(
			new Vector2(titlePos.X + 1, titlePos.Y + 1),
			ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, notification.Alpha * 0.5f)),
			notification.Title
		);
		drawList.AddText(titlePos, titleColor, notification.Title);
			
		Vector2 contentPos = new Vector2(position.X + _padding + 4.0f, position.Y + _padding + ImGui.GetTextLineHeightWithSpacing());
		drawList.AddText(
			ImGui.GetFont(),
			ImGui.GetFontSize(),
			contentPos,
			contentColor,
			notification.Content,
			_width - (_padding * 2) - 4.0f
		);
	}
		
	private static float CalculateNotificationHeight(Notification notification)
	{
		float titleHeight = ImGui.GetTextLineHeight();
		Vector2 contentSize = ImGui.CalcTextSize(notification.Content, false, _width - (_padding * 2) - 4.0f);
		float contentHeight = contentSize.Y;
		return _padding * 2 + titleHeight + ImGui.GetStyle().ItemSpacing.Y + contentHeight;
	}
}