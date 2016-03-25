using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

public class SunGlare : MonoBehaviour
{
    /*
    public Shader sunglareShader;
    public Material sunglareMaterial;

    Texture2D sunSpikes;
    Texture2D sunFlare;
    Texture2D sunGhost1;
    Texture2D sunGhost2;

    bool transmittanceSet = false;

    Vector3 sunViewPortPos = Vector3.zero;

    RaycastHit hit;
    bool hitStatus = false;
    bool eclipse = false;

    float sunGlareScale = 1;

    Mesh screenMesh;

    Vector3 flareSettings = Vector3.zero;
    Vector3 spikesSettings = Vector3.zero;

    List<Vector4> ghost1SettingsList = new List<Vector4> { };
    List<Vector4> ghost2SettingsList = new List<Vector4> { };


    public void start()
    {
        sunglareMaterial = new Material(sunglareShader);

        sunSpikes = new Texture2D(1, 1);
        sunFlare = new Texture2D(1, 1);
        sunGhost1 = new Texture2D(1, 1);
        sunGhost2 = new Texture2D(1, 1);

        sunglareMaterial.SetTexture("sunSpikes", sunSpikes);
        sunglareMaterial.SetTexture("sunFlare", sunFlare);
        sunglareMaterial.SetTexture("sunGhost1", sunGhost1);
        sunglareMaterial.SetTexture("sunGhost2", sunGhost2);
        //sunglareMaterial.SetTexture ("_customDepthTexture", inCore.customDepthBufferTexture);

        sunglareMaterial.renderQueue = 3000;

        screenMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false, false, false);
        screenMesh.bounds = new Bounds(Vector4.zero, new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));

        //didn't want to serialize the matrices directly as the result is pretty unreadable
        Matrix4x4 ghost1Settings = Matrix4x4.zero;
        for (int i = 0; i < ghost1SettingsList.Count; i++)
        {
            ghost1Settings.SetRow(i, ghost1SettingsList[i]);
        }

        Matrix4x4 ghost2Settings = Matrix4x4.zero;
        for (int i = 0; i < ghost2SettingsList.Count; i++)
        {
            ghost2Settings.SetRow(i, ghost2SettingsList[i]);
        }

        sunglareMaterial.SetVector("flareSettings", flareSettings);
        sunglareMaterial.SetVector("spikesSettings", spikesSettings);
        sunglareMaterial.SetMatrix("ghost1Settings", ghost1Settings);
        sunglareMaterial.SetMatrix("ghost2Settings", ghost2Settings);
    }

    public void OnPreRender()
    {
        sunViewPortPos = inCore.scaledSpaceCamera.WorldToViewportPoint
            (ScaledSpace.LocalToScaledSpace(inCore.sunCelestialBody.transform.position));

        sunGlareScale = (float)(inCore.scaledSpaceCamera.transform.position - ScaledSpace.LocalToScaledSpace(inCore.sunCelestialBody.transform.position))
            .magnitude / 2266660f;

        hitStatus = false;

        hitStatus = Physics.Raycast(inCore.farCamera.transform.position,
                                     (inCore.sunCelestialBody.transform.position - inCore.farCamera.transform.position).normalized,
                                     out hit, Mathf.Infinity, (int)((1 << 15) + (1 << 0)));

        if (!hitStatus)
        {
            hitStatus = Physics.Raycast(inCore.scaledSpaceCamera.transform.position,
                                         (ScaledSpace.LocalToScaledSpace(inCore.sunCelestialBody.transform.position) - inCore.scaledSpaceCamera.transform.position)
                                         .normalized, out hit, Mathf.Infinity, (int)((1 << 10)));
        }


        eclipse = hitStatus;

        sunglareMaterial.SetVector("sunViewPortPos", sunViewPortPos);
        sunglareMaterial.SetFloat("aspectRatio", inCore.nearCamera.aspect);
        sunglareMaterial.SetFloat("sunGlareScale", sunGlareScale);

        if (!transmittanceSet)
        {
            sunglareMaterial.SetFloat("Rg", inCore.managerWactivePQS.m_skyNode.Rg);
            sunglareMaterial.SetFloat("Rt", inCore.managerWactivePQS.m_skyNode.Rt);
            sunglareMaterial.SetTexture("_Sky_Transmittance", inCore.managerWactivePQS.m_skyNode.m_transmit);

            sunglareMaterial.SetFloat("useTransmittance", 1f);
            transmittanceSet = true;
        }

        if (transmittanceSet)
        {
            if (!inCore.pqsEnabled)
            {
                sunglareMaterial.SetFloat("useTransmittance", 0f);
                transmittanceSet = false;
            }
            else
            {
                sunglareMaterial.SetVector("_Globals_WorldCameraPos", inCore.farCamera.transform.position - lastActivePQS.parentCelestialBody.transform.position);
                sunglareMaterial.SetVector("_Sun_WorldSunDir", lastActivePQS.getDirectionToSun().normalized);
            }
        }
    }

    public void updateNode()
    {
        if (!eclipse && (sunViewPortPos.z > 0))
        {
            Graphics.DrawMesh(screenMesh, Vector3.zero, Quaternion.identity, sunglareMaterial, 10,
                               inCore.scaledSpaceCamera, 0, null, false, false);

        }
    }
    */
}