﻿using ImGuiNET;
using Nyx.Core.Configuration;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Movement
{
	public class Flight : ModuleBase, IConfigurableModule
	{
		private float speed = 10f;

		public Flight() : base("Flight", "Allows you to fly.", ModuleCategory.Movement, KeyCode.F) { }

		public override void OnUpdate()
		{
			Transform transform = Networking.LocalPlayer.gameObject.transform;

			if (Input.GetKey(KeyCode.W))
			{
				transform.position += transform.forward * speed * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.S))
			{
				transform.position -= transform.forward * speed * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.A))
			{
				transform.position -= transform.right * speed * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.D))
			{
				transform.position += transform.right * speed * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.Space))
			{
				transform.position += transform.up * speed * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.LeftShift))
			{
				transform.position -= transform.up * speed * Time.deltaTime;
			}

			Networking.LocalPlayer.SetVelocity(Vector3.zero);
		}

		public override void OnMenu()
		{
			ImGui.PushID("fly_speed");
			float spd = speed;
			if (ImGui.SliderFloat("Speed", ref spd, 1.0f, 50.0f))
			{
				speed = spd;
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
			config.SetSetting("Speed", speed);
		}

		public void LoadModuleConfig(ModuleConfig config)
		{
			speed = config.GetSetting("Speed", speed);
		}
	}
}
