using System;
using System.Collections.Generic;
using ImGuiNET;
using Nyx.Core.Utils;
using Nyx.UI;
using UnityEngine;

namespace Nyx.Core.Managers;

public static class NotificationManager
{
	private static readonly List<Notification> Notifications = [];
	private const float Padding = 12.0f;
	private const float Width = 300.0f;
	private const float FadeOutTime = 1.5f;
	
	private static readonly SysVec4 AccentColor = new(0.16f, 0.29f, 0.48f, 1.00f);
		
	public static void AddNotification(string title, string content, float duration = 5.0f)
	{
		if (ImGui.GetCurrentContext() == IntPtr.Zero)
			return;
		
		var notification = new Notification(title, content, duration);
		SysVec2 windowSize = ImGui.GetIO().DisplaySize;
		notification.Position = new(windowSize.X - Width - Padding, windowSize.Y - Padding);
		notification.CurrentY = windowSize.Y - Padding;
		notification.StartTime = ImGui.GetTime();
		Notifications.Add(notification);
	}
		
	public static void Update(float deltaTime)
	{
		for (int i = Notifications.Count - 1; i >= 0; i--)
		{
			Notification notification = Notifications[i];
			notification.TimeLeft -= deltaTime;
				
			if (notification.TimeLeft <= 0)
			{
				Notifications.RemoveAt(i);
				continue;
			}
				
			if (notification.TimeLeft < FadeOutTime)
			{
				notification.Alpha = notification.TimeLeft / FadeOutTime;
					
				notification.Position = new(
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
			
		SysVec2 windowSize = new(Screen.width, Screen.height);
		float targetY = windowSize.Y - Padding;
			
		for (int i = 0; i < Notifications.Count; i++)
		{
			Notification notification = Notifications[i];
			float height = CalculateNotificationHeight(notification);
			targetY -= height;
				
			float easeFactor = 10.0f * deltaTime;
			notification.CurrentY = MathUtils.Lerp(notification.CurrentY, targetY, easeFactor);
			notification.Position = new(windowSize.X - Width - Padding, notification.CurrentY);
			targetY -= Padding;
		}
	}
		
	public static void Render()
	{
		ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
		foreach (var notification in Notifications)
		{
			RenderNotification(drawList, notification);
		}
	}
		
	private static void RenderNotification(ImDrawListPtr drawList, Notification notification)
	{
		SysVec2 position = notification.Position;
		float height = CalculateNotificationHeight(notification);
		
		uint backgroundColor = ImGui.ColorConvertFloat4ToU32(new(0.06f, 0.06f, 0.06f, notification.Alpha * 0.94f));
		
		uint borderColor = ImGui.ColorConvertFloat4ToU32(new(0.43f, 0.43f, 0.50f, notification.Alpha * 0.50f));
		
		uint titleColor = ImGui.ColorConvertFloat4ToU32(new(1.0f, 1.0f, 1.0f, notification.Alpha));
		uint contentColor = ImGui.ColorConvertFloat4ToU32(new(0.50f, 0.50f, 0.50f, notification.Alpha * 0.9f));
		
		uint accentBarColor = ImGui.ColorConvertFloat4ToU32(new(
			AccentColor.X, AccentColor.Y, AccentColor.Z, notification.Alpha));
			
		float cornerRadius = 6.0f;
			
		drawList.AddRectFilled(
			position,
			new(position.X + Width, position.Y + height),
			backgroundColor,
			cornerRadius
		);
			
		drawList.AddRectFilled(
			position,
			new(position.X + 4.0f, position.Y + height),
			accentBarColor,
			cornerRadius, 
			ImDrawFlags.RoundCornersLeft
		);
			
		float timeRatio = notification.TimeLeft / notification.Duration;
		float progressWidth = Width * timeRatio;
			
		drawList.AddRectFilled(
			new(position.X, position.Y),
			new(position.X + progressWidth, position.Y + 2.0f),
			accentBarColor,
			0.0f
		);
			
		drawList.AddRect(
			position,
			new(position.X + Width, position.Y + height),
			borderColor,
			cornerRadius,
			ImDrawFlags.RoundCornersAll,
			1.0f
		);
			
		SysVec2 titlePos = new(position.X + Padding + 4.0f, position.Y + Padding);
			
		drawList.AddText(
			new(titlePos.X + 1, titlePos.Y + 1),
			ImGui.ColorConvertFloat4ToU32(new(0.0f, 0.0f, 0.0f, notification.Alpha * 0.5f)),
			notification.Title
		);
		drawList.AddText(titlePos, titleColor, notification.Title);
			
		SysVec2 contentPos = new(position.X + Padding + 4.0f, position.Y + Padding + ImGui.GetTextLineHeightWithSpacing());
		drawList.AddText(
			ImGui.GetFont(),
			ImGui.GetFontSize(),
			contentPos,
			contentColor,
			notification.Content,
			Width - (Padding * 2) - 4.0f
		);
	}
		
	private static float CalculateNotificationHeight(Notification notification)
	{
		float titleHeight = ImGui.GetTextLineHeight();
		SysVec2 contentSize = ImGui.CalcTextSize(notification.Content, false, Width - (Padding * 2) - 4.0f);
		float contentHeight = contentSize.Y;
		return Padding * 2 + titleHeight + ImGui.GetStyle().ItemSpacing.Y + contentHeight;
	}
}