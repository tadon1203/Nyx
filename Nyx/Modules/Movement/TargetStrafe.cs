using ImGuiNET;
using Nyx.Core.Settings;
using UnityEngine;
using VRC.SDKBase;
using Vector2 = System.Numerics.Vector2;

namespace Nyx.Modules.Movement;

public class TargetStrafe : ModuleBase
{
    [FloatSetting("Strafe Speed", "Orbit speed", 2.0f, 0.1f, 10.0f)]
    private float _strafeSpeed = 2.0f;

    [FloatSetting("Strafe Radius", "Orbit radius", 2.0f, 0.5f, 5.0f)]
    private float _strafeRadius = 2.0f;

    [Setting("Auto Height", "Automatically adjust height", "true")]
    private bool _autoHeight = true;

    [Setting("Look At Target", "Look at target", "true")]
    private bool _lookAtTarget = true;

    private VRCPlayerApi _target;
    private float _currentAngle;
    private UnityVec3 _lastTargetPosition;

    public TargetStrafe() : base("TargetStrafe", "Allows you to strafe around the player.", ModuleCategory.Movement)
    {
        RegisterSettings();
    }

    public override void OnUpdate()
    {
        if (!IsEnabled || _target == null || Networking.LocalPlayer == null) 
            return;

        UpdateStrafePosition();
    }

    private void UpdateStrafePosition()
    {
        var localPlayer = Networking.LocalPlayer;
        var targetTransform = _target.gameObject.transform;
        _lastTargetPosition = targetTransform.position;
        
        _currentAngle += _strafeSpeed * Time.deltaTime;
        
        Vector3 newPosition = CalculateStrafePosition(targetTransform, localPlayer);
        
        localPlayer.gameObject.transform.position = newPosition;
        
        if (_lookAtTarget)
        {
            UpdatePlayerRotation(localPlayer, targetTransform.position, newPosition);
        }
    }

    private UnityVec3 CalculateStrafePosition(Transform targetTransform, VRCPlayerApi localPlayer)
    {
        float x = _lastTargetPosition.x + _strafeRadius * Mathf.Cos(_currentAngle);
        float z = _lastTargetPosition.z + _strafeRadius * Mathf.Sin(_currentAngle);
        float y = _autoHeight ? _lastTargetPosition.y : localPlayer.GetPosition().y;
        
        return new(x, y, z);
    }

    private void UpdatePlayerRotation(VRCPlayerApi player, Vector3 targetPos, Vector3 currentPos)
    {
        Vector3 lookDirection = targetPos - currentPos;
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            player.gameObject.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    public override void OnEnable()
    {
        if (Networking.LocalPlayer == null) return;
        
        var controller = Networking.LocalPlayer.gameObject.GetComponent<CharacterController>();
        if (controller != null) 
        {
            controller.enabled = false;
        }
    }

    public override void OnDisable()
    {
        if (Networking.LocalPlayer == null) return;
        
        var controller = Networking.LocalPlayer.gameObject.GetComponent<CharacterController>();
        if (controller != null) 
        {
            controller.enabled = true;
        }
    }

    public override void OnImGuiRender()
    {
        if (!IsEnabled) return;

        ImGui.Begin("Target Strafe Settings", ImGuiWindowFlags.AlwaysAutoResize);
        {
            RenderPlayerSelection();
            RenderStatusInfo();
        }
        ImGui.End();
    }

    private void RenderPlayerSelection()
    {
        ImGui.Text("Select Target Player:");
        
        if (ImGui.BeginListBox("##PlayerList", new(-1, 200)))
        {
            foreach (var player in VRCPlayerApi.AllPlayers)
            {
                if (player.isLocal) continue;
                
                bool isSelected = _target != null && _target.playerId == player.playerId;
                if (ImGui.Selectable($"{player.displayName}##{player.playerId}", isSelected))
                {
                    _target = player;
                    _currentAngle = 0;
                    _lastTargetPosition = player.GetPosition();
                }
                
                if (isSelected) 
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndListBox();
        }
    }

    private void RenderStatusInfo()
    {
        if (_target != null)
        {
            ImGui.TextColored(new(0, 1, 0, 1), $"Target: {_target.displayName}");
            ImGui.Text($"Distance: {Vector3.Distance(Networking.LocalPlayer.GetPosition(), _target.GetPosition()):F2}m");
        }
        else
        {
            ImGui.TextColored(new(1, 0.5f, 0.5f, 1), "No target selected");
        }
    }
}
