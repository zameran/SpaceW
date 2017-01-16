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
// Creation Time: 19:13
// Creator: zameran
#endregion

using UnityEngine;

public class Universe : MonoBehaviour
{
    public FlyCamera FlyCamera;

    public Segment Entry;

    public int ClustersCount { get { return Entry.Chunks.Length * Chunk.Size; } }
    public int ChunksCount { get { return Entry.Chunks.Length; } }
    public int SegmentsCount { get { return 1; } }

    private void Start()
    {
        Entry = new Segment(Vector3d.zero);
        Entry.Init();
    }

    private void Update()
    {
        if (Entry == null) return;

        Entry.Update(transform.position);
    }

    private void OnDrawGizmos()
    {
        if (Entry == null) return;

        Entry.OnDrawGizmos();
    }

    private void OnGUI()
    {
        GUILayoutExtensions.Vertical(() =>
        {
            GUILayoutExtensions.LabelWithSpace(string.Format("Segment Position: {0}", Entry.Position), -8);
            GUILayoutExtensions.LabelWithSpace(string.Format("Camera Position: {0}", FlyCamera.transform.position), -8);
            GUILayoutExtensions.LabelWithSpace(string.Format("Clusters Count: {0}", ClustersCount), -8);
            GUILayoutExtensions.LabelWithSpace(string.Format("Chunks Count: {0}", ChunksCount), -8);
            GUILayoutExtensions.LabelWithSpace(string.Format("Segments Count: {0}", SegmentsCount), -8);
            GUILayoutExtensions.LabelWithSpace(string.Format("Current Chunk Position: {0}", Entry.Current != null ? Entry.Current.GUID.ToString() : "Null"), -8);
        });
    }
}