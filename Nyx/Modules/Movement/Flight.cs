using System.Collections.Generic;
using Nyx.Core.Settings;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Movement;

public class Flight : ModuleBase
{
    [FloatSetting("Flight Speed", "Basic flight speed", 10.0f, 1.0f, 50.0f)]
    private float _speed = 10f;

    [FloatSetting("Vertical Boost", "Vertical boost multiplier", 1.5f, 0.5f, 5.0f)]
    private float _verticalBoost = 1.5f;

    [FloatSetting("Acceleration", "Movement acceleration", 6.0f, 0.1f, 10.0f)]
    private float _acceleration = 2f;

    private UnityVec3 _currentVelocity;
    
    public override IReadOnlyList<string> AvailableModes { get; } =
    [
        "Smooth Movement",
        "Instant Movement"
    ];

    public Flight() : base("Flight", "Allows you to fly.", ModuleCategory.Movement, KeyCode.F)
    {
        RegisterSettings();
    }

    public override void OnUpdate()
    {
        if (!IsEnabled || Networking.LocalPlayer == null || Camera.main == null) return;

        Transform playerTransform = Networking.LocalPlayer.gameObject.transform;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 inputDirection = GetInputDirection(cameraForward);
        Vector3 targetVelocity = inputDirection * _speed;
        
        if (CurrentMode == 0)
        {
            _currentVelocity = Vector3.Lerp(
                _currentVelocity, 
                targetVelocity, 
                _acceleration * Time.deltaTime
            );
        }
        else
        {
            _currentVelocity = targetVelocity;
        }

        playerTransform.position += _currentVelocity * Time.deltaTime;
        Networking.LocalPlayer.SetVelocity(Vector3.zero);
    }

    private Vector3 GetInputDirection(Vector3 cameraForward)
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) direction += cameraForward;
        if (Input.GetKey(KeyCode.S)) direction -= cameraForward;
        if (Input.GetKey(KeyCode.A)) direction -= Camera.main.transform.right;
        if (Input.GetKey(KeyCode.D)) direction += Camera.main.transform.right;
        if (Input.GetKey(KeyCode.Space)) direction += Camera.main.transform.up * _verticalBoost;
        if (Input.GetKey(KeyCode.LeftShift)) direction -= Camera.main.transform.up * _verticalBoost;

        return direction.normalized;
    }

    public override void OnModeChanged(int newModeIndex)
    {
        _currentVelocity = Vector3.zero;
    }

    public override void OnEnable()
    {
        if (Networking.LocalPlayer == null) return;
        
        var controller = Networking.LocalPlayer.gameObject.GetComponent<CharacterController>();
        if (controller != null) 
        {
            controller.enabled = false;
        }
        
        _currentVelocity = Vector3.zero;
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
}
