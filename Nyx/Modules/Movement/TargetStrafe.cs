using ImGuiNET;
using Nyx.Core.Configuration;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Movement
{
	public class TargetStrafe : ModuleBase, IConfigurableModule
	{
        public TargetStrafe() : base("TargetStrafe", "Allows you to strafe around the player.", ModuleCategory.Movement) { }

        private VRCPlayerApi target;
        private float strafeSpeed = 2.0f;
        private float strafeRadius = 2.0f;
        private float currentAngle = 0.0f;

        public override void OnUpdate()
        {
            if (target != null)
            {
                VRCPlayerApi localPlayer = Networking.LocalPlayer;

                GameObject targetObject = target.gameObject;
                if (targetObject == null)
                    return;

                Vector3 targetPosition = targetObject.transform.position;

                currentAngle += strafeSpeed * Time.deltaTime;
                float x = targetPosition.x + strafeRadius * Mathf.Cos(currentAngle);
                float z = targetPosition.z + strafeRadius * Mathf.Sin(currentAngle);

                Vector3 newPosition = new Vector3(x, localPlayer.GetPosition().y, z);

                localPlayer.TeleportTo(newPosition, localPlayer.GetRotation());

                Vector3 lookDirection = targetPosition - newPosition;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                    localPlayer.gameObject.transform.rotation = lookRotation;
                }
            }
        }

        public override void OnMenu()
        {
            
            float speed = strafeSpeed;
            if (ImGui.SliderFloat("Strafe Speed", ref speed, 0.5f, 10.0f, "%.1f"))
            {
                strafeSpeed = speed;
            }

            float radius = strafeRadius;
            if (ImGui.SliderFloat("Strafe Radius", ref radius, 1.0f, 10.0f, "%.1f"))
            {
                strafeRadius = radius;
            }
            
            ImGui.Separator();
            ImGui.Text("Select Target Player:");

            if (Networking.LocalPlayer == null)
                return;

            if (ImGui.BeginListBox("##PlayerList", new Core.Utils.Vector2(-1, 200)))
            {
                foreach (var player in VRCPlayerApi.AllPlayers)
                {
                    if (player.isLocal)
                        continue;
                    
                    bool isSelected = (target != null && target.playerId == player.playerId);
                    string playerName = player.displayName;
                    
                    if (ImGui.Selectable(playerName, isSelected))
                    {
                        target = player;
                        Enable();
                    }
                    
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndListBox();
            }
            
            if (target != null)
            {
                ImGui.Text($"Current target: {target.displayName}");
            }
            else
            {
                ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0.5f, 1), "No target selected");
            }
        }

        public void SaveModuleConfig(ModuleConfig config)
        {
            config.SetSetting("Strafe speed", strafeSpeed);
            config.SetSetting("Strafe radius", strafeRadius);
        }
		public void LoadModuleConfig(ModuleConfig config)
        {
            strafeSpeed = config.GetSetting("Strafe speed", strafeSpeed);
            strafeRadius = config.GetSetting("Strafe radius", strafeRadius);
        }
    }
}