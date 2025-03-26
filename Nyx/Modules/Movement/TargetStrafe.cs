using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Utils;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Movement;

public class TargetStrafe()
    : ModuleBase("TargetStrafe", "Allows you to strafe around the player.", ModuleCategory.Movement),
        IConfigurableModule
{
    private VRCPlayerApi _target;
    private float _strafeSpeed = 2.0f;
    private float _strafeRadius = 2.0f;
    private float _currentAngle;

    public override void OnUpdate()
    {
        if (_target != null)
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;

            GameObject targetObject = _target.gameObject;
            if (targetObject == null)
                return;

            Vector3 targetPosition = targetObject.transform.position;

            _currentAngle += _strafeSpeed * Time.deltaTime;
            float x = targetPosition.x + _strafeRadius * Mathf.Cos(_currentAngle);
            float z = targetPosition.z + _strafeRadius * Mathf.Sin(_currentAngle);

            Vector3 newPosition = new Vector3(x, localPlayer.GetPosition().y, z);

            localPlayer.gameObject.transform.position = newPosition;

            Vector3 lookDirection = targetPosition - newPosition;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
                localPlayer.gameObject.transform.rotation = lookRotation;
            }
        }
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

    public override void OnMenu()
    {
        float speed = _strafeSpeed;
        if (ImGui.SliderFloat("Strafe Speed", ref speed, 0.5f, 10.0f, "%.1f"))
        {
            _strafeSpeed = speed;
        }

        float radius = _strafeRadius;
        if (ImGui.SliderFloat("Strafe Radius", ref radius, 0.0f, 10.0f, "%.1f"))
        {
            _strafeRadius = radius;
        }
            
        ImGui.Separator();
        ImGui.Text("Select Target Player:");

        if (Networking.LocalPlayer == null)
            return;

        if (ImGui.BeginListBox("##PlayerList", new Vec2(-1, 200)))
        {
            foreach (var player in VRCPlayerApi.AllPlayers)
            {
                if (player.isLocal)
                    continue;
                    
                bool isSelected = _target != null && _target.playerId == player.playerId;
                string playerName = player.displayName;
                    
                if (ImGui.Selectable(playerName, isSelected))
                {
                    _target = player;
                    Enable();
                }
                    
                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndListBox();
        }
            
        if (_target != null)
        {
            ImGui.Text($"Current target: {_target.displayName}");
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1, 0.5f, 0.5f, 1), "No target selected");
        }
    }

    public void SaveModuleConfig(ModuleConfig config)
    {
        config.SetSetting("Strafe speed", _strafeSpeed);
        config.SetSetting("Strafe radius", _strafeRadius);
    }
    public void LoadModuleConfig(ModuleConfig config)
    {
        _strafeSpeed = config.GetSetting("Strafe speed", _strafeSpeed);
        _strafeRadius = config.GetSetting("Strafe radius", _strafeRadius);
    }
}