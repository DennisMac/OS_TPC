using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//From Code Monkey

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] Image Crosshair;
    [SerializeField] bool Projectilebullets = true;

    [SerializeField] Transform GeometryTransform;
    [SerializeField] float correctionAngle = -45f;

    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity = 1.0f;
    [SerializeField] private float aimSensitivity = 0.5f;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform spawnBulletPosition;

    [SerializeField] private Transform vfxHitGreen;
    [SerializeField] private Transform vfxHitRed;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    private void Awake()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }
        else
        {
            mouseWorldPosition = spawnBulletPosition.position + 100f * ray.direction;
        }

        if (starterAssetsInputs.aim )
        {
            Crosshair.enabled = true;
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 5f));
            TurnTowards(mouseWorldPosition);
            GeometryTransform.localRotation = Quaternion.Lerp(GeometryTransform.localRotation, Quaternion.AngleAxis(correctionAngle, Vector3.up), Time.deltaTime *5f);

        }
        else
        {
            GeometryTransform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.AngleAxis(correctionAngle, Vector3.up), Time.deltaTime * 2f);
            Crosshair.enabled = false;
            thirdPersonController.SetRotateOnMove(true);
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 2f));
        }


        if (starterAssetsInputs.shoot)
        {  
            TurnTowards(mouseWorldPosition, true);
            GeometryTransform.localRotation = Quaternion.Lerp(GeometryTransform.localRotation, Quaternion.AngleAxis(correctionAngle, Vector3.up), Time.deltaTime * 10f);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            if (Projectilebullets)
            {
                Vector3 aimDirection = (mouseWorldPosition - spawnBulletPosition.position).normalized;
                Instantiate(bulletProjectilePrefab, spawnBulletPosition.position, Quaternion.LookRotation(aimDirection));
            }
            else
            {
                if (hitTransform != null)
                {
                    if (hitTransform.GetComponent<BulletTarget>() != null)
                    {
                        Instantiate(vfxHitGreen, raycastHit.point, Quaternion.LookRotation(-transform.forward));
                    }
                    else
                    {
                        Instantiate(vfxHitRed, raycastHit.point, Quaternion.LookRotation(-transform.forward));
                    }
                }
            }
            starterAssetsInputs.shoot = false;

        }
    }

    private void TurnTowards(Vector3 mouseWorldPosition, bool instant = false)
    {
        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
        transform.forward = instant ? aimDirection : Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }
}


