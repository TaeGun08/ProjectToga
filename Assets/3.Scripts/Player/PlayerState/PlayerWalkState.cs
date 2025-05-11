using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerWalkState : PlayerState
{
    private static readonly int WALK = Animator.StringToHash("Walk");
    public override StateName Name => StateName.Walk;

    private Camera mainCam;

    private float turnCalmVelocity;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    public override void StateEnter(PlayerController playerController)
    {
        this.playerController = playerController;
        animator.SetTrigger(WALK);
    }

    public override void StateUpdate()
    {
        if (localPlayer.CCType.Equals(CrowdControlType.Unknown) == false)
        {
            playerController.ChangeState(StateName.CrowdControl);
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && localPlayer.UseDash == false)
        {
            playerController.ChangeState(StateName.Dash);
            return;
        }
        
        Vector2 inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        playerController.ReloadWeapon(inputAxis, false);
        
        if(localPlayer.IsReload) return;

        if (localPlayer.IsZoom)
        {
            playerController.ChangeState(StateName.FireIdle);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (playerController.currentWeapon.Data.Ammo <= 0f)
            {
                playerController.ReloadWeapon(inputAxis, true);
            }
            
            playerController.ChangeState(StateName.FireIdle);
            return;
        }

        if (inputAxis != Vector2.zero)
        {
            UpdateRotation(playerController, inputAxis, 0.1f);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerController.ChangeState(StateName.Run);
            return;
        }

        if (!inputAxis.Equals(Vector2.zero)) return;

        playerController.ChangeState(StateName.Idle);
    }

    private void UpdateRotation(PlayerController playerController, Vector2 inputAxis, float smoothTime)
    {
        float targetAngle = Mathf.Atan2(inputAxis.x, inputAxis.y) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
        float angle =
            Mathf.SmoothDampAngle(localPlayer.transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, smoothTime);
        localPlayer.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        playerController.CharacterController.Move(moveDirection.normalized *
                                                  (playerController.LocalPlayer.status.WalkSpeed * Time.deltaTime));
    }

    public override void StateExit()
    {
        animator.ResetTrigger(WALK);
        gameObject.SetActive(false);
    }
}