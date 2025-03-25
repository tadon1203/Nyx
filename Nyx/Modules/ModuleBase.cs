﻿using Nyx.Core.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules;

public abstract class ModuleBase(
	string name,
	string description,
	ModuleCategory category,
	KeyCode toggleKey = KeyCode.None,
	bool isEnabled = false)
{
	public string Name { get; } = name;
	public string Description { get; } = description;
	public ModuleCategory Category { get; } = category;
	public KeyCode ToggleKey { get; private set; } = toggleKey;
	public bool IsEnabled { get; private set; } = isEnabled;

	public virtual IReadOnlyList<string> AvailableModes => new List<string>();
	public int CurrentMode { get; private set; } = -1;

	public virtual void OnEnable() { }
	public virtual void OnDisable() { }
	public virtual void OnModeChanged(int newModeIndex) { }
	public virtual void OnUpdate() { }
	public virtual void OnImGuiRender() { }
	public virtual void OnMenu() { }

	public void Enable()
	{
		if (!IsEnabled)
		{
			NotificationManager.AddNotification("Module", $"Enabled {Name}.");
			IsEnabled = true;
			OnEnable();
		}
	}

	public void Disable()
	{
		if (IsEnabled)
		{
			NotificationManager.AddNotification("Module", $"Disabled {Name}.");
			IsEnabled = false;
			OnDisable();
		}
	}

	public void Toggle()
	{
		if (IsEnabled)
			Disable();
		else
			Enable();
	}

	public void SetMode(int modeIndex)
	{
		if (modeIndex < 0 || modeIndex >= AvailableModes.Count)
		{
			throw new ArgumentException($"ModeIndex {modeIndex} is not available on module '{Name}'.");
		}
		if (CurrentMode != modeIndex)
		{
			CurrentMode = modeIndex;
			OnModeChanged(modeIndex);
		}
	}

	public void SetToggleKey(KeyCode key)
	{
		ToggleKey = key;
	}

	public void Update()
	{
		if (Networking.LocalPlayer != null && IsEnabled)
		{
			OnUpdate();
		}
		if (ToggleKey != KeyCode.None && Input.GetKeyDown(ToggleKey))
		{
			Toggle();
		}
	}
}

public enum ModuleCategory
{
	Movement,
	Visual,
	Exploit
}