﻿using GTFO_VR;
using GTFO_VR.Core;
using GTFO_VR.Events;
using GTFO_VR.Input;
using GTFO_VR.UI;
using GTFO_VR.Util;
using HarmonyLib;
using UnityEngine;



namespace GTFO_VR_BepInEx.Core
{
    /// <summary>
    /// Makes the first person items follow the position and aim direction of the main controller(s) of the player
    /// </summary>
    [HarmonyPatch(typeof(FirstPersonItemHolder),"LateUpdate")]
    class InjectControllerAim
    {
        static void Prefix(FirstPersonItemHolder __instance, ItemEquippable ___WieldedItem)
        {
       
            if (PlayerVR.VRSetup && VRSettings.UseVRControllers)
            {
                if (___WieldedItem == null)
                {
                    return;
                }
               
                Vector3 gripOffset = ___WieldedItem.transform.position - ___WieldedItem.transform.TransformPoint(WeaponArchetypeVRData.GetVRWeaponData(___WieldedItem.ArchetypeName).transformToVRGrip);

                ___WieldedItem.transform.position = Controllers.GetControllerPosition() + gripOffset;

                if (VRSettings.twoHandedAimingEnabled && Controllers.aimingTwoHanded && WeaponArchetypeVRData.GetVRWeaponData(___WieldedItem.ArchetypeName).allowsDoubleHanded)
                {
                    ___WieldedItem.transform.rotation = Controllers.GetTwoHandedRotation();
                }
                else
                {
                    ___WieldedItem.transform.rotation = Controllers.GetControllerAimRotation();
                }

            }
        }
    }




    /// <summary>
    /// Inject this twice because otherwise data like weapon muzzle position is not updated correctly (makes tracers spawn in wrong location, for instance)
    /// </summary>
    [HarmonyPatch(typeof(FirstPersonItemHolder), "Update")]
    class InjectControllerAimAlign
    {
        static void Postfix(FirstPersonItemHolder __instance, ItemEquippable ___WieldedItem)
        {
            if (PlayerVR.VRSetup && VRSettings.UseVRControllers)
            {
                if (___WieldedItem == null)
                {
                    return;
                }
                Vector3 gripOffset = ___WieldedItem.transform.position - ___WieldedItem.transform.TransformPoint(WeaponArchetypeVRData.GetVRWeaponData(___WieldedItem.ArchetypeName).transformToVRGrip);

                ___WieldedItem.transform.position = Controllers.GetControllerPosition() + gripOffset;

                if(VRSettings.twoHandedAimingEnabled && Controllers.aimingTwoHanded && WeaponArchetypeVRData.GetVRWeaponData(___WieldedItem.ArchetypeName).allowsDoubleHanded)
                {
                    ___WieldedItem.transform.rotation = Controllers.GetTwoHandedRotation();
                } else
                {
                    ___WieldedItem.transform.rotation = Controllers.GetControllerAimRotation();
                }
            }
        }
    }
    

    /// <summary>
    /// Changes most actions (placing, firing, throwing) to follow the controller forward instead of camera forward 
    /// NOTE: Does not affect flashlight aggro
    /// </summary>
    /// 

    [HarmonyPatch(typeof(FPSCamera), "UpdateCameraRay")]
    class InjectForwardInteractions
    {
        static void Postfix(FPSCamera __instance)
        {
            if (PlayerVR.VRSetup && VRSettings.UseVRControllers)
            {
                __instance.CameraRayDir = Controllers.GetAimForward();
                RaycastHit hit;
                if (Physics.Raycast(Controllers.GetAimFromPos(), Controllers.GetAimForward(), out hit, 50f, LayerManager.MASK_CAMERA_RAY))
                {
                    __instance.CameraRayPos = hit.point;
                    __instance.CameraRayCollider = hit.collider;
                    __instance.CameraRayNormal = hit.normal;
                    __instance.CameraRayObject = hit.collider.gameObject;
                    __instance.CameraRayDist = hit.distance;
                    
                }
                else
                {
                    __instance.CameraRayPos = Controllers.GetAimFromPos() + Controllers.GetAimForward() * 50f;
                    __instance.CameraRayCollider = null;
                    __instance.CameraRayNormal = -Controllers.GetAimForward();
                    __instance.CameraRayObject = null;
                    __instance.CameraRayDist = 0.0f;
                }
            }
        }
    }

    
}
