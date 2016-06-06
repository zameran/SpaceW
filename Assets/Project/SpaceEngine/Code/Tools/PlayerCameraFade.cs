#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;

using UnityEngine;

public class PlayerCameraFade : MonoBehaviour
{
    private static PlayerCameraFade mInstance = null;

    private static PlayerCameraFade instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = GameObject.FindObjectOfType(typeof(PlayerCameraFade)) as PlayerCameraFade;

                if (mInstance == null)
                {
                    mInstance = new GameObject("CameraFade").AddComponent<PlayerCameraFade>();
                }
            }

            return mInstance;
        }
    }

    void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this;
            instance.init();
        }
    }

    public GUIStyle m_BackgroundStyle = new GUIStyle();						// Style for background tiling
    public Texture2D m_FadeTexture;											// 1x1 pixel texture used for fading
    public Color m_CurrentScreenOverlayColor = new Color(0, 0, 0, 0);		// default starting color: black and fully transparrent
    public Color m_TargetScreenOverlayColor = new Color(0, 0, 0, 0);		// default target color: black and fully transparrent
    public Color m_DeltaColor = new Color(0, 0, 0, 0);						// the delta-color is basically the "speed / second" at which the current color should change
    public int m_FadeGUIDepth = -1000;										// make sure this texture is drawn on top of everything

    private static Action m_OnFadeFinish = null;
    private Action OnFadeFinish
    {
        get { return m_OnFadeFinish; }
        set { m_OnFadeFinish = value; }
    }

    // Initialize the texture, background-style and initial color:
    public void init()
    {
        instance.m_FadeTexture = new Texture2D(1, 1);
        instance.m_BackgroundStyle.normal.background = instance.m_FadeTexture;
    }

    // Draw the texture and perform the fade:
    void OnGUI()
    {
        // If the current color of the screen is not equal to the desired color: keep fading!
        if (instance.m_CurrentScreenOverlayColor != instance.m_TargetScreenOverlayColor)
        {
            // If the difference between the current alpha and the desired alpha is smaller than delta-alpha * deltaTime, then we're pretty much done fading:
            if (Mathf.Abs(instance.m_CurrentScreenOverlayColor.a - instance.m_TargetScreenOverlayColor.a) < Mathf.Abs(instance.m_DeltaColor.a) * Time.deltaTime)
            {
                instance.m_CurrentScreenOverlayColor = instance.m_TargetScreenOverlayColor;
                SetScreenOverlayColor(instance.m_CurrentScreenOverlayColor);
                instance.m_DeltaColor = new Color(0, 0, 0, 0);

                if (instance.OnFadeFinish != null)
                    instance.OnFadeFinish();

                Die();
            }
            else
            {
                // Fade!
                SetScreenOverlayColor(instance.m_CurrentScreenOverlayColor + instance.m_DeltaColor * Time.deltaTime);
            }
        }

        // Only draw the texture when the alpha value is greater than 0:
        if (m_CurrentScreenOverlayColor.a > 0)
        {
            GUI.depth = instance.m_FadeGUIDepth;
            GUI.Label(new Rect(-10, -10, Screen.width + 10, Screen.height + 10), instance.m_FadeTexture, instance.m_BackgroundStyle);
        }
    }

    /// <summary>
    /// Sets the color of the screen overlay instantly.  Useful to start a fade.
    /// </summary>
    /// <param name='newScreenOverlayColor'>
    /// New screen overlay color.
    /// </param>
    private static void SetScreenOverlayColor(Color newScreenOverlayColor)
    {
        instance.m_CurrentScreenOverlayColor = newScreenOverlayColor;
        instance.m_FadeTexture.SetPixel(0, 0, instance.m_CurrentScreenOverlayColor);
        instance.m_FadeTexture.Apply();
    }

    private static void SetAction(Action action)
    {
        m_OnFadeFinish = action;
    }

    public static void FadeIn(float fadeDuration, Action onFinish = null)
    {
        StartAlphaFade(new Color(0, 0, 0, 1), false, fadeDuration);
        SetAction(onFinish);
    }

    public static void FadeOut(float fadeDuration, Action onFinish = null)
    {
        StartAlphaFade(new Color(0, 0, 0, 1), true, fadeDuration);
        SetAction(onFinish);
    }

    /// <summary>
    /// Starts the fade from color newScreenOverlayColor. If isFadeIn, start fully opaque, else start transparent.
    /// </summary>
    /// <param name='newScreenOverlayColor'>
    /// Target screen overlay Color.
    /// </param>
    /// <param name='fadeDuration'>
    /// Fade duration.
    /// </param>
    private static void StartAlphaFade(Color newScreenOverlayColor, bool isFadeIn, float fadeDuration)
    {
        if (fadeDuration <= 0.0f)
        {
            SetScreenOverlayColor(newScreenOverlayColor);
        }
        else
        {
            if (isFadeIn)
            {
                instance.m_TargetScreenOverlayColor = new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0);
                SetScreenOverlayColor(newScreenOverlayColor);
            }
            else
            {
                instance.m_TargetScreenOverlayColor = newScreenOverlayColor;
                SetScreenOverlayColor(new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0));
            }

            instance.m_DeltaColor = (instance.m_TargetScreenOverlayColor - instance.m_CurrentScreenOverlayColor) / fadeDuration;
        }
    }

    /// <summary>
    /// Starts the fade from color newScreenOverlayColor. If isFadeIn, start fully opaque, else start transparent, after a delay, with Action OnFadeFinish.
    /// </summary>
    /// <param name='newScreenOverlayColor'>
    /// New screen overlay color.
    /// </param>
    /// <param name='fadeDuration'>
    /// Fade duration.
    /// </param>
    /// <param name='OnFadeFinish'>
    /// On fade finish, doWork().
    /// </param>
    private static void StartAlphaFade(Color newScreenOverlayColor, bool isFadeIn, float fadeDuration, /*float fadeDelay,*/ Action OnFadeFinish)
    {
        if (fadeDuration <= 0.0f)
        {
            SetScreenOverlayColor(newScreenOverlayColor);
        }
        else
        {
            instance.OnFadeFinish = OnFadeFinish;

            if (isFadeIn)
            {
                instance.m_TargetScreenOverlayColor = new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0);
                SetScreenOverlayColor(newScreenOverlayColor);
            }
            else
            {
                instance.m_TargetScreenOverlayColor = newScreenOverlayColor;
                SetScreenOverlayColor(new Color(newScreenOverlayColor.r, newScreenOverlayColor.g, newScreenOverlayColor.b, 0));
            }

            instance.m_DeltaColor = (instance.m_TargetScreenOverlayColor - instance.m_CurrentScreenOverlayColor) / fadeDuration;
        }
    }

    void Die()
    {
        mInstance = null;
        Destroy(gameObject);
    }

    void OnApplicationQuit()
    {
        mInstance = null;
    }
}