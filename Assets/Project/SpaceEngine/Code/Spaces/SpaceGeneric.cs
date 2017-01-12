#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.01.11
// Creation Time: 22:38
// Creator: zameran
#endregion

using System;

using UnityEngine;

public abstract class SpaceGeneric
{
    public Guid GUID { get; private set; }

    public Vector3d Position { get; protected set; }

    public SpaceGeneric Parent;
    public SpaceGeneric Current;

    public abstract byte Size { get; }
    public abstract long SpaceSize { get; }
    public byte HalfSize { get { return (byte)(Size / 2); } }
    public long SideSize { get { return Size * SpaceSize; } }
    public long HalfSpaceSize { get { return SpaceSize / 2; } }
    public Vector3d Shift { get { return new Vector3d(HalfSpaceSize, HalfSpaceSize, HalfSpaceSize); } }

    protected SpaceGeneric()
    {
        this.GUID = Guid.NewGuid();

        this.Position = Vector3d.zero;
    }

    protected SpaceGeneric(Vector3d Position)
    {
        this.GUID = Guid.NewGuid();

        this.Position = Position;
    }

    protected void UpdateFromParent(Vector3d Position)
    {
        if (Parent == null) return;

        Update(Position);
    }

    public abstract void Init();

    public abstract void Update(Vector3d Position);

    public abstract void DrawDebug();
}

public class SpaceGeneric<TChildType> : SpaceGeneric where TChildType : SpaceGeneric, new()
{
    public TChildType[,,] LeafNodes;

    public override byte Size { get { return 8; } }
    public override long SpaceSize { get { return 65536; } }

    public override void Init()
    {
        LeafNodes = new TChildType[Size, Size, Size];

        for (var x = -HalfSize; x < HalfSize; x++)
        {
            for (var y = -HalfSize; y < HalfSize; y++)
            {
                for (var z = -HalfSize; z < HalfSize; z++)
                {
                    var idx = x + HalfSize;
                    var idy = y + HalfSize;
                    var idz = z + HalfSize;

                    var position = new Vector3d(x * SpaceSize, y * SpaceSize, z * SpaceSize) + Shift;
                    var node = new TChildType();

                    node.Parent = this;
                    node.Init();
                    node.Update(Position + position);

                    LeafNodes[idx, idy, idz] = node;
                }
            }
        }
    }

    public override void Update(Vector3d Position)
    {
        if (LeafNodes != null)
        {
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    for (var z = 0; z < Size; z++)
                    {
                        var node = LeafNodes[x, y, z];

                        if (node != null)
                        {
                            node.Update(Position + (node.Position - this.Position));
                        }
                    }
                }
            }
        }

        this.Position = Position;
    }

    public override void DrawDebug()
    {
        if (LeafNodes != null)
        {
            var size = Vector3.one * SpaceSize;

            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    for (var z = 0; z < Size; z++)
                    {
                        var node = LeafNodes[x, y, z];

                        if (node != null)
                        {
                            Gizmos.DrawWireCube(node.Position, size);
                        }
                    }
                }
            }
        }
    }
}