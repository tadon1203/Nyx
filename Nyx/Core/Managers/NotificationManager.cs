using ImGuiNET;
using Nyx.Core.Utils;
using Nyx.UI;
using System.Collections.Generic;

namespace Nyx.Core.Managers
{
	public static class NotificationManager
	{
		private static List<Notification> notifications = new();
		private static float padding = 10.0f;
		private static float width = 300.0f;
		private static Vector2 basePosition = new(10, 10); // 初期位置（右下の調整に使用）
		private static float fadeOutTime = 1.0f;

		public static void AddNotification(string title, string content, float duration = 5.0f)
		{
			var notification = new Notification(title, content, duration);

			Vector2 windowSize = ImGui.GetIO().DisplaySize;
			notification.Position = new Vector2(windowSize.x - width - padding, windowSize.y - padding);
			notification.CurrentY = windowSize.y - padding;

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
				}

				if (notification.TimeLeft < fadeOutTime)
				{
					notification.Position = new Vector2(
						notification.Position.x + (10.0f * deltaTime),
						notification.Position.y
					);
				}
			}

			Vector2 windowSize = new(UnityEngine.Screen.width, UnityEngine.Screen.height);
			float targetY = windowSize.y - padding;

			for (int i = 0; i < notifications.Count; i++)
			{
				Notification notification = notifications[i];
				float height = CalculateNotificationHeight(notification);

				targetY -= height;
				notification.CurrentY = MathUtils.Lerp(notification.CurrentY, targetY, 8.0f * deltaTime);
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

			uint backgroundColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.05f, 0.05f, 0.05f, notification.Alpha * 0.95f));
			uint borderColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 0.5f, notification.Alpha));
			uint titleColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, notification.Alpha));
			uint contentColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.9f, 0.9f, notification.Alpha * 0.9f));

			drawList.AddRectFilled(
				position,
				new Vector2(position.x + width, position.y + height),
				backgroundColor,
				6.0f
			);

			drawList.AddRect(
				position,
				new Vector2(position.x + width, position.y + height),
				borderColor,
				6.0f,
				ImDrawFlags.RoundCornersAll,
				1.0f
			);

			Vector2 titlePos = new Vector2(position.x + padding, position.y + padding);
			drawList.AddText(titlePos, titleColor, notification.Title);

			Vector2 contentPos = new Vector2(position.x + padding, position.y + padding + ImGui.GetTextLineHeightWithSpacing());
			drawList.AddText(contentPos, contentColor, notification.Content);
		}

		private static float CalculateNotificationHeight(Notification notification)
		{
			float titleHeight = ImGui.GetTextLineHeight();

			ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
			Vector2 contentSize = ImGui.CalcTextSize(notification.Content, false, width - (padding * 2));
			ImGui.PopStyleColor();

			float contentHeight = contentSize.y;

			return padding * 2 + titleHeight + ImGui.GetStyle().ItemSpacing.Y + contentHeight;
		}
	}
}
