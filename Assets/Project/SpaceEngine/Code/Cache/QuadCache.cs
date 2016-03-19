using System;

using UnityEngine;

[Serializable]
public abstract class QuadCache
{
    public QuadStorage Owner;
    public Quad.Id ID;

    public QuadCache(Quad.Id ID, QuadStorage Owner)
    {
        this.ID = ID;

        this.Owner = Owner;
    }

    public virtual void Init()
    {

    }

    public virtual void TransferTo(Quad q)
    {

    }

    public virtual void TransferFrom(Quad q)
    {

    }

    public virtual void OnDestroy()
    {

    }

    public void BeginTransfer()
    {
        GUILayer[] gui = GameObject.FindObjectsOfType<GUILayer>();

        for (int i = 0; i < gui.Length; i++)
        {
            gui[i].enabled = false;
        }
    }

    public void EndTransfer()
    {
        GUILayer[] gui = GameObject.FindObjectsOfType<GUILayer>();

        for (int i = 0; i < gui.Length; i++)
        {
            gui[i].enabled = true;
        }
    }
}