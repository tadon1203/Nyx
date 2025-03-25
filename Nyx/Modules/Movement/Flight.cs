using ImGuiNET;
using Nyx.Core.Configuration;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Movement;

public class Flight() : ModuleBase("Flight", "Allows you to fly.", ModuleCategory.Movement, KeyCode.F),
	IConfigurableModule
{
	private float _speed = 10f;

	public override void OnUpdate()
	{
		Transform transform = Networking.LocalPlayer.gameObject.transform;

		if (Input.GetKey(KeyCode.W))
		{
			transform.position += transform.forward * _speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.S))
		{
			transform.position -= transform.forward * _speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.A))
		{
			transform.position -= transform.right * _speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.D))
		{
			transform.position += transform.right * _speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.Space))
		{
			transform.position += transform.up * _speed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.LeftShift))
		{
			transform.position -= transform.up * _speed * Time.deltaTime;
		}

		Networking.LocalPlayer.SetVelocity(Vector3.zero);
	}

	public override void OnMenu()
	{
		ImGui.PushID("fly_speed");
		float spd = _speed;
		if (ImGui.SliderFloat("Speed", ref spd, 1.0f, 50.0f))
		{
			_speed = spd;
		}
		ImGui.PopID();
	}

	public override void OnEnable()
	{
		if (Networking.LocalPlayer == null)
			return;

		Networking.LocalPlayer.gameObject.GetComponent<CharacterController>().enabled = false;
	}

	public override void OnDisable()
	{
		if (Networking.LocalPlayer == null)
			return;

		Networking.LocalPlayer.gameObject.GetComponent<CharacterController>().enabled = true;
	}

	public void SaveModuleConfig(ModuleConfig config)
	{
		config.SetSetting("Speed", _speed);
	}

	public void LoadModuleConfig(ModuleConfig config)
	{
		_speed = config.GetSetting("Speed", _speed);
	}
}