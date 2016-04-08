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

    public static void CalculatePatchCubeCenter(int lodLevel, Vector3 patchCubeCenter, ref Vector3 temp)
    {
        //TODO : Make a formula!
        //So. We have exponential modifier... WTF!?
        //Fuck dat shit. 7 LOD level more than i need. fuck. dat.

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
            else if (lodLevel == 7) //Experimental! Maybe float precision have place on small planet radius!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.8828125f), 0.9921875f); //0.984375f + ((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 8) //Experimental! Maybe float precision have place on small planet radius!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.94140625f), 0.99609375f); //0.9921875f + (((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 9) //Experimental! Maybe float precision have place on small planet radius!
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.970703100f), 0.998046875f); //0.99609375f + ((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            else if (lodLevel == 10) //Sooooo deep... what i'am doing?
                temp = Vector3.Lerp(temp, patchCubeCenter * (15.0f / 14.9853515000f), 0.999023438f); //0.998046875f + (((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
            //OMG...
        }
        //End of magic here.
    }
}