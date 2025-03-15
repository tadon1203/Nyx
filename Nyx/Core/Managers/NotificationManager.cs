using ImGuiNET;
using Nyx.Core.Utils;
using Nyx.UI;
using System;
using System.Collections.Generic;

namespace Nyx.Core.Managers
{
	public static class NotificationManager
	{
		private static List<Notification> notifications = new();
		private static float padding = 12.0f;
		private static float width = 300.0f;
		private static float fadeOutTime = 1.5f;
		
		private static Vector4 accentColor = new Vector4(148f/255f, 226f/255f, 213f/255f, 1.0f);
		
		public static void AddNotification(string title, string content, float duration = 5.0f)
		{
			var notification = new Notification(title, content, duration);
			Vector2 windowSize = ImGui.GetIO().DisplaySize;
			notification.Position = new Vector2(windowSize.x - width - padding, windowSize.y - padding);
			notification.CurrentY = windowSize.y - padding;
			notification.StartTime = ImGui.GetTime();
			notifications.Add(notification);
		}
		
		public static void Update(float deltaTime)
		{
			for (int i = notifications.Count - 1; i >= 0; i--)
			{
				Notification notification = notifications[i];
				notification.TimeLeft -= deltaTime;
				
				if (notification.TimeLeft <= 0)
				{
					notifications.RemoveAt(i);
					continue;
				}
				
				if (notification.TimeLeft < fadeOutTime)
				{
					notification.Alpha = notification.TimeLeft / fadeOutTime;
					
					notification.Position = new Vector2(
						notification.Position.x + (12.0f * deltaTime),
						notification.Position.y
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
			float targetY = windowSize.y - padding;
			
			for (int i = 0; i < notifications.Count; i++)
			{
				Notification notification = notifications[i];
				float height = CalculateNotificationHeight(notification);
				targetY -= height;
				
				float easeFactor = 10.0f * deltaTime;
				notification.CurrentY = MathUtils.Lerp(notification.CurrentY, targetY, easeFactor);
				notification.Position = new Vector2(windowSize.x - width - padding, notification.CurrentY);
				targetY -= padding;
			}
		}
		
		public static void Render()
		{
			ImDrawListPtr drawList = ImGui.GetForegroundDrawList();
			foreach (var notification in notifications)
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
				accentColor.x, accentColor.y, accentColor.z, notification.Alpha * 0.7f));
			uint titleColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, notification.Alpha));
			uint contentColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.9f, 0.9f, notification.Alpha * 0.9f));
			uint accentBarColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
				accentColor.x, accentColor.y, accentColor.z, notification.Alpha));
			
			float cornerRadius = 6.0f;
			
			drawList.AddRectFilled(
				position,
				new Vector2(position.x + width, position.y + height),
				backgroundColor,
				cornerRadius
			);
			
			drawList.AddRectFilled(
				position,
				new Vector2(position.x + 4.0f, position.y + height),
				accentBarColor,
				cornerRadius, 
				ImDrawFlags.RoundCornersLeft
			);
			
			float timeRatio = notification.TimeLeft / notification.Duration;
			float progressWidth = width * timeRatio;
			
			drawList.AddRectFilled(
				new Vector2(position.x, position.y),
				new Vector2(position.x + progressWidth, position.y + 2.0f),
				accentBarColor,
				0.0f
			);
			
			drawList.AddRect(
				position,
				new Vector2(position.x + width, position.y + height),
				borderColor,
				cornerRadius,
				ImDrawFlags.RoundCornersAll,
				1.0f
			);
			
			Vector2 titlePos = new Vector2(position.x + padding + 4.0f, position.y + padding);
			
			drawList.AddText(
				new Vector2(titlePos.x + 1, titlePos.y + 1),
				ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, notification.Alpha * 0.5f)),
				notification.Title
			);
			drawList.AddText(titlePos, titleColor, notification.Title);
			
			Vector2 contentPos = new Vector2(position.x + padding + 4.0f, 
											position.y + padding + ImGui.GetTextLineHeightWithSpacing());
			drawList.AddText(
				ImGui.GetFont(),
				ImGui.GetFontSize(),
				contentPos,
				contentColor,
				notification.Content,
				width - (padding * 2) - 4.0f
			);
		}
		
		private static float CalculateNotificationHeight(Notification notification)
		{
			float titleHeight = ImGui.GetTextLineHeight();
			Vector2 contentSize = ImGui.CalcTextSize(notification.Content, false, width - (padding * 2) - 4.0f);
			float contentHeight = contentSize.y;
			return padding * 2 + titleHeight + ImGui.GetStyle().ItemSpacing.Y + contentHeight;
		}
	}
}
