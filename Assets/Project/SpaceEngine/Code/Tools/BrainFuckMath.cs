﻿#region License
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

using UnityEngine;

public static class BrainFuckMath
{
    public static void DefineAxis(ref bool staticX, ref bool staticY, ref bool staticZ, Vector3 size)
    {
        if (size.x == 0)
            staticX = true;
        else if (size.y == 0)
            staticY = true;
        else if (size.z == 0)
            staticZ = true;
    }

    public static void LockAxis(ref float tempAxisValue, ref Vector3 temp, bool staticX, bool staticY, bool staticZ)
    {
        if (staticX)
            tempAxisValue = temp.x;
        if (staticY)
            tempAxisValue = temp.y;
        if (staticZ)
            tempAxisValue = temp.z;
    }

    public static void UnlockAxis(ref Vector3 temp, ref float tempAxisValue, bool staticX, bool staticY, bool staticZ)
    {
        if (staticX)
            temp.x = tempAxisValue;
        if (staticY)
            temp.y = tempAxisValue;
        if (staticZ)
            temp.z = tempAxisValue;
    }

    public static decimal CalculateK(int lodLevel)
    {
        if (lodLevel == 1)
            return 0.5M;
        else
        {
            decimal prev = CalculateK(lodLevel - 1);
            decimal summ = 1.0M;

            for (int i = 0; i < lodLevel; i++)
            {
                summ = summ / 2.0M;
            }

            return prev + summ;
        }
    }

    public static decimal CalculateJ(int lodLevel)
    {
        if (lodLevel == 0)
            return 15.0M;
        else if (lodLevel == 1)
            return 7.5M;

        decimal prev1 = CalculateJ(lodLevel - 1);
        decimal prev2 = CalculateJ(lodLevel - 2);

        decimal summ = System.Math.Abs(prev1 - prev2) / 2.0M;

        return prev1 + summ;
    }

    public static decimal CalculateI(decimal J)
    {
        return 15.0M / J;
    }

    public static void CalculatePatchCubeCenter(int lodLevel, Vector3 patchCubeCenter, ref Vector3 temp)
    {
        /*
        1 : 15.0 / 7,5f | 0,5f
        2 : 15.0 / 11,25f | 0,75f
        3 : 15.0 / 13,125f | 0,875f
        4 : 15.0 / 14,0625f | 0,9375f
        5 : 15.0 / 14,53125f | 0,96875f
        6 : 15.0 / 14,765625f | 0,984375f
        7 : 15.0 / 14,8828125f | 0,9921875f
        8 : 15.0 / 14,94140625f | 0,99609375f
        9 : 15.0 / 14,970703125f | 0,998046875f
        10 : 15.0 / 14,9853515625f | 0,9990234375f
        11 : 15.0 / 14,99267578125f | 0,99951171875f
        12 : 15.0 / 14,996337890625f | 0,999755859375f
        13 : 15.0 / 14,9981689453125f | 0,9998779296875f
        14 : 15.0 / 14,99908447265625f | 0,99993896484375f
        15 : 15.0 / 14,999542236328125f | 0,999969482421875f
        16 : 15.0 / 14,9997711181640625f | 0,9999847412109375f
        17 : 15.0 / 14,99988555908203125f | 0,99999237060546875f
        18 : 15.0 / 14,999942779541015625f | 0,999996185302734375f
        19 : 15.0 / 14,9999713897705078125f | 0,9999980926513671875f
        20 : 15.0 / 14,99998569488525390625f | 0,99999904632568359375f
        21 : 15.0 / 14,999992847442626953125f | 0,999999523162841796875f
        22 : 15.0 / 14,9999964237213134765625f | 0,9999997615814208984375f
        23 : 15.0 / 14,99999821186065673828125f | 0,99999988079071044921875f
        24 : 15.0 / 14,999999105930328369140625f | 0,999999940395355224609375f
        25 : 15.0 / 14,9999995529651641845703125f | 0,9999999701976776123046875f
        26 : 15.0 / 14,99999977648258209228515625f | 0,99999998509883880615234375f
        27 : 15.0 / 14,999999888241291046142578125f | 0,999999992549419403076171875f
        28 : 15.0 / 14,999999944120645523071289062f | 0,9999999962747097015380859375f
        29 : 15.0 / 14,999999972060322761535644530f | 0,9999999981373548507690429687f
        30 : 15.0 / 14,999999986030161380767822264f | 0,9999999990686774253845214843f
        31 : 15.0 / 14,999999993015080690383911131f | 0,9999999995343387126922607421f
        32 : 15.0 / 14,999999996507540345191955564f | 0,9999999997671693563461303710f
        */

        //TODO: Make a formula! - Formula is done!
        //So. We have exponential modifier... WTF!?
        //Fuck dat shit. 7 LOD level more than i need. fuck. dat.
        //Too small numbers... So. Solution is planet radius scaling. 1 unit = 1 milion unity units,
        //then i simply gonna "scale" the overhaul planet.

        //WARNING!!! Magic! Ya, it works...
        if (lodLevel >= 1)
        {
            if (lodLevel == 1)
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 7.5f), 0.5f); //0.5f
            else if (lodLevel == 2)
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 11.25f), 0.75f); //0.5f + 0.5f / 2.0f
            else if (lodLevel == 3)
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 13.125f), 0.875f); //0.75f + ((0.5f / 2.0f) / 2.0f)
            else if (lodLevel == 4)
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.0625f), 0.9375f); //0.875f + (((0.5f / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 5)
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.53125f), 0.96875f); //0.9375f + ((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 6)
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.765625f), 0.984375f); //0.96875f + (((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 7) //Experimental!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.8828125f), 0.9921875f); //0.984375f + ((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 8) //Experimental!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.94140625f), 0.99609375f); //0.9921875f + (((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 9) //Experimental! Maybe float precision have place on small planet radius!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.970703125f), 0.998046875f); //0.99609375f + ((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 10) //Sooooo deep... what i'am doing?
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.9853515625f), 0.9990234375f); //0.998046875f + (((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 11) //WHY?!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.99267578125f), 0.99951171875f); //0.9990234375f + ((((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 12) //NOOOOO! STOP IT! STOP THIS!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.996337890625f), 0.999755859375f); //0.99951171875f + (((((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            //OMG...
        }
        //End of magic here.
    }
}