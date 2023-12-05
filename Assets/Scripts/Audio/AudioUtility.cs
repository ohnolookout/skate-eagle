using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class AudioUtility
{
    public static void UpdateModifiers(Dictionary<Rigidbody2D, SoundModifiers> modifiers, LiveRunManager runManager, float maxSoundDistance)
    {
        foreach (var body in modifiers.Keys.ToList())
        {
            //If body is null, track intensity of playerBody, pulled from runManager so it updates to lowSpine if ragdoll
            if (body == null || !runManager.PlayerIsRagdoll)
            {
                //modifiers[body].SetIntensity(AudioUtility.Intensity(runManager.PlayerBody, intensityDenominator));
                modifiers[body].intensity = Intensity(runManager.PlayerBody, AudioManager.intensityDenominator);
                //Distance and pan are not relevant for playerbody, so continue to next body.
                continue;
            }
            modifiers[body].intensity = Intensity(body, AudioManager.intensityDenominator);
            float soundDistance = 0;
            if (body.transform.position.x > runManager.LeadingCameraCorner.x)
            {
                soundDistance = body.transform.position.x - runManager.LeadingCameraCorner.x;
            }
            else if (body.transform.position.x < runManager.TrailingCameraCorner.x)
            {
                soundDistance = runManager.TrailingCameraCorner.x - body.transform.position.x;
            }
            else
            {
                modifiers[body].distance = 1;
            }
            modifiers[body].distance = (maxSoundDistance - soundDistance) / maxSoundDistance;
            modifiers[body].pan = GetPan(runManager.PlayerBody, body, runManager);
        }
    }

    public static float TotalModifier(Sound sound, SoundModifiers modifier, float zoomModifier, bool playerIsRagdoll)
    {
        float totalMod = 1;
        if (sound.trackZoom)
        {
            totalMod *= zoomModifier;
        }
        if (!playerIsRagdoll)
        {
            return totalMod;
        }
        if (sound.trackDistance)
        {
            totalMod *= modifier.distance;
        }
        return totalMod;
    }

    public static float EaseIn(float t)
    {
        return t * t;
    }

    public static float EaseOut(float t)
    {
        return Flip(EaseIn(Flip(t)));
    }

    static float Flip(float x)
    {
        return 1 - x;
    }

    public static float EaseInOut(float t)
    {
        return Mathf.Lerp(EaseIn(t), EaseOut(t), t);
    }

    public static float StopDuration(float xVelocity)
    {
        return (Mathf.Log(1 / xVelocity) / -0.083f) / 50;
        //Denominator should be equal to ln(1 - deceleration coeffeciient).
    }

    public static float InterpolateValue(float startVal, float endVal, float currentVal, float returnFloor, float returnCeiling)
    {
        // Ensure currentVal is within the specified range
        float clampedCurrentVal = Mathf.Clamp(currentVal, startVal, endVal);

        // Calculate the normalized position of currentVal between startVal and endVal
        float normalizedPosition = (clampedCurrentVal - startVal) / (endVal - startVal);

        // Interpolate the value between returnFloor and returnCeiling based on the normalized position
        float result = returnFloor + normalizedPosition * (returnCeiling - returnFloor);

        return result;
    }

    public static float Intensity(Rigidbody2D trackingBody, float denominator)
    {
        return -1 + Mathf.Clamp(trackingBody.velocity.magnitude / denominator, 0, 2);
    }


    public static float CalculateZoomModifier(LiveRunManager runManager, float zoomLimit)
    {
        if (Mathf.Abs(runManager.DefaultCameraSize - runManager.CameraSize) < 0.3f)
        {
            return 1;
        }
        return InterpolateValue(runManager.DefaultCameraSize, zoomLimit, runManager.CameraSize, 1, 0.1f);
    }

    public static float GetPan(Rigidbody2D playerBody, Rigidbody2D panBody, LiveRunManager runManager)
    {
        float halfCamWidth = runManager.LeadingCameraCorner.x - runManager.CameraCenter.x;
        float distanceFromCenter = panBody.position.x - playerBody.position.x;
        return distanceFromCenter / halfCamWidth;
    }
}
