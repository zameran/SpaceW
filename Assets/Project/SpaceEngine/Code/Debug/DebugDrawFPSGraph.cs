using System;
using System.Diagnostics;

using UnityEngine;

using Debug = UnityEngine.Debug;

public class DebugDrawFPSGraph : DebugDraw
{
    public bool audioFeedback = false;
    public float audioFeedbackVolume = 0.5f;
    public int graphMultiply = 2;
    public Vector2 graphPosition = new Vector2(0.0f, 0.0f);
    public int frameHistoryLength = 120;

    public Color CpuColor = new Color(53.0f / 255.0f, 136.0f / 255.0f, 167.0f / 255.0f, 1.0f);
    public Color RenderColor = new Color(112.0f / 255.0f, 156.0f / 255.0f, 6.0f / 255.0f, 1.0f);
    public Color OtherColor = new Color(193.0f / 255.0f, 108.0f / 255.0f, 1.0f / 255.0f, 1.0f);

    readonly int[] numberBits = new[] { 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 };
    readonly int[] fpsBits = new[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 };
    readonly int[] mbBits = new[] { 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
    readonly float[] bNote = new[] { 0.01300049f, 0.02593994f, 0.03900146f, 0.03894043f, 0.05194092f, 0.06494141f, 0.06494141f, 0.06494141f, 0.07791138f, 0.09091187f, 0.07791138f, 0.09091187f, 0.1039124f, 0.1038818f, 0.1168823f, 0.1168823f, 0.1298828f, 0.1298218f, 0.1429443f, 0.1427612f, 0.1429443f, 0.1687622f, 0.1688843f, 0.1687927f, 0.1818237f, 0.1818542f, 0.1947327f, 0.1949158f, 0.2206421f, 0.2079163f, 0.2206726f, 0.2208557f, 0.2337036f, 0.2208252f, 0.2207642f, 0.2337646f, 0.2467651f, 0.2337341f, 0.2597656f, 0.2467651f, 0.2597046f, 0.2727661f, 0.2597046f, 0.2597656f, 0.2857361f, 0.2856445f, 0.2857666f, 0.2986755f, 0.2987061f, 0.3117065f, 0.311676f, 0.324646f, 0.3247375f, 0.324585f, 0.3507385f, 0.337616f, 0.350647f, 0.363678f, 0.3765564f, 0.363678f, 0.3766174f, 0.3765869f, 0.389679f, 0.4025269f, 0.4286499f, 0.4284668f, 0.4286499f, 0.4544983f, 0.4545898f, 0.4674988f, 0.4675293f, 0.4805603f, 0.4804382f, 0.5065918f, 0.5194092f, 0.5195007f, 0.5195007f, 0.5324097f, 0.5455322f, 0.5583801f, 0.5585022f, 0.5713501f, 0.5715027f, 0.5713806f, 0.5844421f, 0.5974121f, 0.5973511f, 0.6104431f, 0.6103821f, 0.6233521f, 0.6364136f, 0.649292f, 0.6493835f, 0.6493225f, 0.662384f, 0.6752625f, 0.6753845f, 0.6882935f, 0.7012634f, 0.6883545f, 0.7012634f, 0.7142944f, 0.7273254f, 0.7272034f, 0.7402954f, 0.7402344f, 0.7402649f, 0.7402954f, 0.7532043f, 0.7532654f, 0.7792053f, 0.7792358f, 0.7792053f, 0.7792358f, 0.7922058f, 0.7921753f, 0.7922668f, 0.8051147f, 0.8052979f, 0.7921143f, 0.7922668f, 0.8051758f, 0.8051453f, 0.7922974f, 0.7921143f, 0.8182678f, 0.8051453f, 0.8182068f, 0.8051758f, 0.7922058f, 0.8052063f, 0.7922058f, 0.7922058f, 0.7662354f, 0.7662354f, 0.7532043f, 0.7403564f, 0.7401428f, 0.7403259f, 0.7142944f, 0.7012024f, 0.7014771f, 0.6751404f, 0.6494751f, 0.662262f, 0.6364136f, 0.6103516f, 0.6104126f, 0.6103821f, 0.5844116f, 0.5714111f, 0.5584717f, 0.5454102f, 0.5325317f, 0.5324097f, 0.5195313f, 0.5064392f, 0.4935303f, 0.4805298f, 0.4675293f, 0.4545593f, 0.4674988f, 0.4675598f, 0.4544983f, 0.4416504f, 0.4544373f, 0.4546509f, 0.4284668f, 0.4416504f, 0.4285278f, 0.4285583f, 0.4286194f, 0.4284973f, 0.4286499f, 0.4415283f, 0.4285583f, 0.4285889f, 0.4545288f, 0.4415588f, 0.4415894f, 0.4415283f, 0.4675293f, 0.4545898f, 0.4804382f, 0.4676208f, 0.4674683f, 0.4805298f, 0.4805298f, 0.5065308f, 0.4934082f, 0.5066223f, 0.5323181f, 0.5455627f, 0.5454102f, 0.5584717f, 0.5584412f, 0.5713806f, 0.5714722f, 0.5713806f, 0.5974731f, 0.5973511f, 0.5974121f, 0.6234131f, 0.5973206f, 0.5975037f, 0.623291f, 0.6234131f, 0.6233826f, 0.6233521f, 0.6493835f, 0.6233521f, 0.6363525f, 0.6234131f, 0.6233215f, 0.6364441f, 0.636261f, 0.6364441f, 0.636322f, 0.6104126f, 0.6363525f, 0.6104126f, 0.610321f, 0.6234436f, 0.6103516f, 0.5974121f, 0.6104431f, 0.623291f, 0.6104736f, 0.623291f, 0.6104736f, 0.6233215f, 0.5974121f, 0.6234436f, 0.61026f, 0.6105347f, 0.5972595f, 0.6235046f, 0.6102905f, 0.5974731f, 0.5973511f, 0.6104431f, 0.5843506f, 0.5844727f, 0.5843811f, 0.5844421f, 0.5714111f, 0.5714722f, 0.5713501f, 0.5585327f, 0.5583496f, 0.5585022f, 0.5324707f, 0.5454407f, 0.5194702f, 0.5194702f, 0.4935303f, 0.5064697f, 0.4935608f, 0.4934082f, 0.4806213f, 0.4804077f, 0.4806519f, 0.4674072f, 0.4676208f, 0.4674683f, 0.4935608f, 0.4804993f, 0.4804993f, 0.4675903f, 0.4674377f, 0.4676514f, 0.4544373f, 0.4546204f, 0.4545288f, 0.4674988f, 0.4675903f, 0.4544678f, 0.4546509f, 0.4544373f, 0.4416199f, 0.4544983f, 0.4415894f, 0.4415894f, 0.4414978f, 0.4545898f, 0.4284973f, 0.4546509f, 0.4414673f, 0.4545898f, 0.4415894f, 0.4544678f, 0.4416504f, 0.4414673f, 0.4286499f, 0.4414978f, 0.4286499f, 0.4284668f, 0.4286804f, 0.4414368f, 0.4287109f, 0.4284363f, 0.4286804f, 0.4154968f, 0.4286194f, 0.4285583f, 0.4155884f, 0.4025879f, 0.4026184f, 0.3895874f, 0.4026184f, 0.4025574f, 0.3766479f, 0.3896179f, 0.3766174f, 0.3896484f, 0.3635254f, 0.3637695f, 0.3375244f, 0.3507996f, 0.3245544f, 0.3247681f, 0.311615f, 0.3117065f, 0.2857361f, 0.285675f, 0.2598267f, 0.2596436f, 0.2468262f, 0.2207336f, 0.2077942f, 0.2208252f, 0.1947327f, 0.1818848f, 0.1687927f, 0.1558533f, 0.1428528f, 0.1298828f, 0.1038513f, 0.1169128f, 0.09091187f, 0.09088135f, 0.06500244f, 0.06484985f, 0.06500244f, 0.0519104f, 0.03897095f, 0.02600098f, 0.02593994f, 0.01300049f, 3.051758E-05f, -9.155273E-05f, -0.01287842f, -9.155273E-05f, -0.02590942f, -0.02603149f, -0.03890991f, -0.03900146f, -0.03894043f, -0.03897095f, -0.05194092f, -0.06494141f, -0.05194092f, -0.07794189f, -0.07791138f, -0.07791138f, -0.09091187f, -0.1039429f, -0.1038513f, -0.1168823f, -0.1168823f, -0.1299133f, -0.1427917f, -0.1429443f, -0.1557312f, -0.1429443f, -0.1687622f, -0.1689148f, -0.1817017f, -0.1949463f, -0.1946716f, -0.2078857f, -0.2337341f, -0.2207642f, -0.2207947f, -0.2467651f, -0.2467346f, -0.2338257f, -0.2856445f, -0.2987671f, -0.2986145f, -0.3117676f, -0.3116455f, -0.3117371f, -0.337616f, -0.3506775f, -0.337616f, -0.350708f, -0.3506165f, -0.3636169f, -0.4026794f, -0.4024658f, -0.3897705f, -0.3894653f, -0.4026794f, -0.4155884f, -0.4155273f, -0.4286499f, -0.4284973f, -0.4415894f, -0.4545593f, -0.4545288f, -0.4415283f, -0.4416199f, -0.4544983f, -0.4545593f, -0.4675598f, -0.4674683f, -0.4675903f, -0.4674988f, -0.4805603f, -0.4674988f, -0.4805298f, -0.4675293f, -0.4804993f, -0.4805908f, -0.4804382f, -0.4805908f, -0.4804382f, -0.4675903f, -0.4675293f, -0.4674988f, -0.4675903f, -0.4674377f, -0.4676208f, -0.4415283f, -0.4545288f, -0.4545898f, -0.4674683f, -0.4416199f, -0.4415283f, -0.4285889f, -0.4285583f, -0.4155884f, -0.4026184f, -0.4155884f, -0.4025269f, -0.3897095f, -0.3894958f, -0.3767395f, -0.3765564f, -0.350647f, -0.363678f, -0.3635864f, -0.3377075f, -0.337616f, -0.3377075f, -0.311676f, -0.311676f, -0.2987366f, -0.2856445f, -0.2728271f, -0.2726135f, -0.2598267f, -0.2596741f, -0.2338257f, -0.2207336f, -0.2078247f, -0.2077332f, -0.2078857f, -0.1947021f, -0.1949158f, -0.1687317f, -0.1689148f, -0.1557922f, -0.1558838f, -0.1427917f, -0.1299744f, -0.1167908f, -0.1169434f, -0.09085083f, -0.1039429f, -0.09088135f, -0.07794189f, -0.06491089f, -0.05197144f, -0.03894043f, -0.02600098f, -0.01293945f, -0.02603149f, -0.01296997f, 3.051758E-05f, 0.01293945f, 0.01306152f, 0.03887939f, 0.03900146f, 0.03894043f, 0.03897095f, 0.06497192f, 0.06488037f, 0.05200195f, 0.07785034f, 0.0909729f, 0.07788086f, 0.1039124f, 0.09091187f, 0.09091187f, 0.09088135f, 0.1039124f, 0.1039124f, 0.1298218f, 0.1169434f, 0.1298218f, 0.1298828f, 0.1169128f, 0.1298218f, 0.1428833f, 0.1169128f, 0.1297913f, 0.1299744f, 0.1427612f, 0.1169434f, 0.1298523f, 0.1558228f, 0.1558838f, 0.1298523f, 0.1428528f, 0.1428528f, 0.1298828f, 0.1298523f, 0.1169128f, 0.1298523f, 0.1168518f, 0.1169434f, 0.1038513f, 0.1169128f, 0.1038818f, 0.1168823f, 0.09088135f, 0.1169434f, 0.09085083f, 0.0909729f, 0.1038208f, 0.1039734f, 0.07785034f, 0.0909729f, 0.07785034f, 0.0909729f, 0.07788086f, 0.07797241f, 0.06484985f, 0.0909729f, 0.06491089f, 0.06494141f, 0.07794189f, 0.07785034f, 0.07803345f, 0.07781982f, 0.0909729f, 0.1038513f, 0.1039429f, 0.1038818f, 0.1168823f, 0.1168823f, 0.1168518f, 0.1299438f, 0.1427917f, 0.1428833f, 0.1558533f, 0.1558228f, 0.1558533f, 0.1688232f, 0.1818237f, 0.1817932f, 0.1948547f, 0.1947632f, 0.2207947f, 0.2207642f, 0.2337646f, 0.2337952f, 0.2597046f, 0.2337952f, 0.2597351f, 0.2727051f, 0.2727661f, 0.2726746f, 0.2987671f, 0.285675f, 0.2987061f, 0.2987061f, 0.311676f, 0.324707f, 0.3376465f, 0.324646f, 0.3247375f, 0.3246155f, 0.3377075f, 0.3376465f, 0.350647f, 0.337677f, 0.3376465f, 0.3506775f, 0.337616f, 0.363678f, 0.3506165f, 0.3506775f, 0.337677f, 0.337616f, 0.350708f, 0.324585f, 0.337738f, 0.324646f, 0.3246765f, 0.2857361f, 0.2986755f, 0.2857056f, 0.2727661f, 0.2726746f, 0.2727661f, 0.2597351f, 0.2467651f, 0.2467346f, 0.2337646f, 0.2207947f, 0.2207642f, 0.2078552f, 0.2206726f, 0.1949158f, 0.1817017f, 0.1819458f, 0.1687317f, 0.1688843f, 0.1688232f, 0.1557922f, 0.1559448f, 0.1557312f, 0.1559448f, 0.1427917f, 0.1558838f, 0.1557922f, 0.1429443f, 0.1557617f, 0.1558838f, 0.1558228f, 0.1688538f, 0.1688232f, 0.1948242f, 0.1947632f, 0.2078247f, 0.2207947f, 0.2337036f, 0.2338867f, 0.246582f, 0.2729187f, 0.2855835f, 0.3117371f, 0.3246765f, 0.337616f, 0.350708f, 0.3895874f, 0.3766174f, 0.4155884f, 0.4285583f, 0.4545898f, 0.4674683f, 0.4805603f, 0.4935303f, 0.5324097f, 0.5455322f, 0.5583496f, 0.5844727f, 0.5974121f, 0.6233215f, 0.6364441f, 0.6622314f, 0.6754456f, 0.7011719f, 0.701416f, 0.7142029f, 0.7273254f, 0.7532349f, 0.7532349f, 0.7532654f, 0.7791748f, 0.7792969f, 0.7921448f, 0.8182373f, 0.8051147f, 0.8052673f, 0.8181458f, 0.8312073f, 0.8311157f, 0.8181763f, 0.8182068f, 0.8312073f, 0.8181152f, 0.8052673f, 0.8050842f, 0.8052979f, 0.7921143f, 0.8052979f, 0.7921143f, 0.7922974f, 0.7791443f, 0.7662354f, 0.7662659f, 0.7662048f, 0.7532959f, 0.7402344f, 0.7142334f, 0.7143555f, 0.7272034f, 0.7013855f, 0.7012329f, 0.688324f, 0.688324f, 0.675293f, 0.662384f, 0.6493225f, 0.6493225f, 0.6364441f, 0.636261f, 0.6235046f, 0.62323f, 0.6235046f, 0.59729f, 0.5975342f, 0.61026f, 0.5975037f, 0.5973206f, 0.5844727f, 0.5844116f, 0.5973511f, 0.5845032f, 0.5713196f, 0.5715332f, 0.5713806f, 0.5584412f, 0.5584717f, 0.5583801f, 0.5455322f, 0.5454102f, 0.5454712f, 0.5324707f, 0.5324402f, 0.5065308f, 0.5194397f, 0.5065308f, 0.4934692f, 0.4805603f, 0.4934692f, 0.4675598f, 0.4415283f, 0.4545898f, 0.4545288f, 0.4415283f, 0.4285889f, 0.4285889f, 0.4025574f, 0.3897095f, 0.3894653f, 0.3767395f, 0.3765564f, 0.3636475f, 0.3506775f, 0.3376465f, 0.3246765f, 0.3246765f, 0.311676f, 0.2987061f, 0.2857361f, 0.2727051f, 0.2727356f, 0.2727356f, 0.2467041f, 0.2597961f, 0.2597046f, 0.2597656f, 0.2467651f, 0.2467041f, 0.2337952f, 0.2337646f, 0.2467651f, 0.2337341f, 0.2468262f, 0.2596436f, 0.2598267f, 0.2596741f, 0.2727966f, 0.2726746f, 0.2727661f, 0.285675f, 0.2987061f, 0.2987366f, 0.2986755f, 0.324707f, 0.324646f, 0.3376465f, 0.3377075f, 0.3505859f, 0.3507385f, 0.3505554f, 0.3637085f, 0.3635864f, 0.363678f, 0.3635864f, 0.3766479f, 0.3636475f, 0.3506165f, 0.363678f, 0.3506165f, 0.3376465f, 0.3377075f, 0.3505859f, 0.3247375f, 0.324646f, 0.2987061f, 0.2857361f, 0.2466736f, 0.2728577f, 0.2466125f, 0.2209167f, 0.2076721f, 0.1819153f, 0.1557617f, 0.1559143f, 0.1168213f, 0.1039429f, 0.07788086f, 0.03900146f, 0.01293945f, 0.01303101f, -0.03897095f, -0.06497192f, -0.09085083f, -0.1169434f, -0.1298218f, -0.1558533f, -0.1948242f, -0.2077637f, -0.2207947f, -0.2597351f, -0.2727356f, -0.2987061f, -0.3116455f, -0.3377075f, -0.337616f, -0.363678f, -0.3636475f, -0.3895569f, -0.4026489f, -0.4155579f, -0.4155579f, -0.4286499f, -0.4414978f, -0.4415894f, -0.4415283f, -0.4545593f, -0.4415588f, -0.4675598f, -0.4415283f, -0.4415588f, -0.4545593f, -0.4545288f, -0.4285889f, -0.4285583f, -0.4285889f, -0.4025879f, -0.4155884f, -0.4025879f, -0.3766479f, -0.3765869f, -0.3636475f, -0.3636475f, -0.3636475f, -0.3376465f, -0.3246765f, -0.3246765f, -0.311676f, -0.2987366f, -0.2986755f, -0.2857056f, -0.2727661f, -0.285675f, -0.2597656f, -0.2597351f, -0.2467346f, -0.2338257f, -0.2336731f, -0.2208557f, -0.2207336f, -0.1948547f, -0.2077637f, -0.1948242f, -0.1947632f, -0.1948547f, -0.1947632f, -0.1818848f, -0.1817627f, -0.1818542f, -0.1817932f, -0.1818237f, -0.1818237f, -0.1817932f, -0.1558838f, -0.1817932f, -0.1558838f, -0.1557922f, -0.1558838f, -0.1687622f, -0.1559448f, -0.1557617f, -0.1429138f, -0.1688232f, -0.1558228f, -0.1558533f, -0.1558533f, -0.1298218f, -0.1429443f, -0.1297913f, -0.1299438f, -0.1297913f, -0.09094238f, -0.1168823f, -0.09091187f, -0.1038818f, -0.07797241f, -0.1037903f, -0.09103394f, -0.07781982f, -0.07800293f, -0.06488037f, -0.06497192f, -0.0519104f, -0.03900146f, -0.03890991f, -0.03903198f, -0.03887939f, -0.03903198f, -0.03887939f, -0.02606201f, -0.02590942f, -0.02600098f, -0.02597046f, -0.03894043f, -0.01303101f, -0.02593994f, -0.02597046f, -0.03900146f, -0.02590942f, -0.02606201f, -0.03887939f, -0.03900146f, -0.05194092f, -0.03894043f, -0.03900146f, -0.05187988f, -0.06503296f, -0.07781982f, -0.07800293f, -0.09082031f, -0.09100342f, -0.09082031f, -0.1040039f, -0.09082031f, -0.09094238f, -0.09091187f, -0.09091187f, -0.1038818f, -0.1039429f, -0.1038208f, -0.1039734f, -0.1168213f, -0.1039734f, -0.1168213f, -0.1169128f, -0.1298523f, -0.1039124f, -0.1168823f, -0.1168823f, -0.09091187f, -0.1168823f, -0.1168823f, -0.1038818f, -0.1169128f, -0.1168213f, -0.1039734f, -0.1298218f, -0.1168823f, -0.1039124f, -0.1168518f, -0.1039124f, -0.1169128f, -0.1168518f, -0.1168518f, -0.0909729f, -0.1168213f, -0.1039734f, -0.09082031f, -0.1039734f, -0.1038208f, -0.07803345f, -0.07781982f, -0.0909729f, -0.06488037f, -0.07797241f, -0.07788086f, -0.06500244f, -0.07781982f, -0.07803345f, -0.07781982f, -0.07800293f, -0.09082031f, -0.07803345f, -0.09082031f, -0.07797241f, -0.07788086f, -0.09094238f, -0.09088135f, -0.0909729f, -0.09085083f, -0.1168823f, -0.1169434f, -0.1167603f, -0.1170654f, -0.1426697f, -0.1430054f, -0.1427917f, -0.1428223f, -0.1559143f, -0.1557922f, -0.1688538f, -0.1558533f, -0.1817627f, -0.1688843f, -0.1947937f, -0.1947937f, -0.2078247f, -0.2077332f, -0.1948547f, -0.2077942f, -0.2077332f, -0.1948853f, -0.1947327f, -0.2078552f, -0.2077637f, -0.1948242f, -0.1947632f, -0.1948242f, -0.1948242f, -0.1947632f, -0.2078552f, -0.1947327f, -0.1818848f, -0.1817627f, -0.1688843f, -0.1557922f, -0.1558838f, -0.1428528f, -0.1298218f, -0.1169739f, -0.1038208f, -0.09094238f, -0.1039124f, -0.07788086f, -0.07794189f, -0.05194092f, -0.03897095f, -0.0519104f, -0.01309204f, -0.02581787f, -0.0001525879f };
    readonly Color[] graphKeys = new[] { new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f) };

    AudioClip audioClip;
    AudioSource audioSource;

    Texture2D graphTexture;
    int graphHeight = 100;

    int[,] textOverlayMask;

    float[,] dtHistory;
    int i;
    int j;
    int x;
    int y;
    float val;
    Color color;
    Color32 color32;
    float maxFrame = 0.0f;
    float yMulti;

    static Color[] fpsColors;
    static Color[] fpsColorsTo;

    Color lineColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);
    Color darkenedBack = new Color(0.0f, 0.0f, 0.0f, 0.5f);
    Color darkenedBackWhole = new Color(0.0f, 0.0f, 0.0f, 0.25f);

    Color32[] colorsWrite;

    Rect graphSizeGUI;

    Stopwatch stopWatch;
    float lastElapsed;
    float fps;
    int graphSizeMin;


    float beforeRender;

    float[] fpsVals = new float[3];

    float x1;
    float x2;
    float y1;
    float y2;
    float xOff;
    float yOff;

    int[] lineY = new[] { 25, 50, 99 };
    int[] lineY2 = new[] { 21, 46, 91 };
    int[] keyOffX = new[] { 61, 34, 1 };

    string[] splitMb;

    int first;
    int second;

    void Awake()
    {
        if (gameObject.GetComponent<Camera>() == null)
            Debug.LogWarning("FPS Graph needs to be attached to a Camera object");

        CreateLineMaterial();

        fpsColors = new[] { RenderColor, CpuColor, OtherColor };
        fpsColorsTo = new[] { fpsColors[0] * 0.7f, fpsColors[1] * 0.7f, fpsColors[2] * 0.7f };
    }

    protected override void Start()
    {
        base.Start();

        graphSizeMin = frameHistoryLength > 95 ? frameHistoryLength : 95;

        textOverlayMask = new int[graphHeight, graphSizeMin];

        dtHistory = new float[3, frameHistoryLength];

        stopWatch = new Stopwatch();
        stopWatch.Start();

        graphTexture = new Texture2D(graphSizeMin, 7, TextureFormat.ARGB32, false, false);

        colorsWrite = new Color32[graphTexture.width * 7];
        graphTexture.filterMode = FilterMode.Point;
        graphSizeGUI = new Rect(0f, 0f, graphTexture.width * graphMultiply, graphTexture.height * graphMultiply);

        AddFPSAt(14, 23);
        AddFPSAt(14, 48);
        AddFPSAt(14, 93);

        for (int x = 0; x < graphTexture.width; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                if (x < 95 && y < 5)
                {
                    color = graphKeys[y * 95 + x];
                }
                else
                {
                    color.a = 0.0f;
                }

                graphTexture.SetPixel(x, y, color);
                colorsWrite[(y) * graphTexture.width + x] = color;
            }
        }

        graphTexture.Apply();

        if (audioFeedback)
            InitAudio();
    }

    public void InitAudio()
    {
        audioClip = AudioClip.Create("FPS-BNote", bNote.Length, 1, 44100, false, null);
        audioClip.SetData(bNote, 0);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = audioClip;
    }

    int xExtern;
    int yExtern;
    int startAt;
    int yOffset;
    int xLength;

    void AddFPSAt(int startX, int startY)
    {
        yExtern = startY;

        for (int y = 0; y < 4; y++)
        {
            xExtern = startX;
            yOffset = y * 11;

            for (int x = 0; x < 11; x++)
            {
                textOverlayMask[yExtern, xExtern] = fpsBits[yOffset + x];
                xExtern++;
            }

            yExtern++;
        }
    }

    void AddNumberAt(int startX, int startY, int num, bool isLeading)
    {
        if (isLeading && num == 0) num = -1;

        startAt = num * 4;
        xLength = startAt + 3;

        yExtern = startY;

        for (int y = 0; y < 5; y++)
        {
            xExtern = startX;
            yOffset = y * 39;

            for (int x = startAt; x < xLength; x++)
            {
                if (num != -1 && numberBits[yOffset + x] == 1)
                {
                    x1 = xExtern * graphMultiply + xOff;
                    y1 = yExtern * graphMultiply + yOff;

                    GL.Vertex3(x1, y1, 0);
                    GL.Vertex3(x1, y1 + 1 * graphMultiply, 0);
                    GL.Vertex3(x1 + 1 * graphMultiply, y1 + 1 * graphMultiply, 0);
                    GL.Vertex3(x1 + 1 * graphMultiply, y1, 0);
                }
                xExtern++;
            }

            yExtern++;
        }
    }

    void addPeriodAt(int startX, int startY)
    {
        x1 = startX * graphMultiply + xOff;
        x2 = (startX + 1) * graphMultiply + xOff;
        y1 = startY * graphMultiply + yOff;
        y2 = (startY - 1) * graphMultiply + yOff;

        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x1, y2, 0);
        GL.Vertex3(x2, y2, 0);
        GL.Vertex3(x2, y1, 0);
    }

    float totalSeconds;
    float renderSeconds;
    float lateSeconds;
    float dt;
    int frameIter = 0;
    float eTotalSeconds;

    void Update()
    {
        eTotalSeconds = (float)stopWatch.Elapsed.TotalSeconds;
        dt = eTotalSeconds - lastElapsed;
        lastElapsed = eTotalSeconds;

        if (Time.frameCount > 4)
        {
            dtHistory[0, frameIter] = dt;

            frameIter++;

            if (audioFeedback)
            {
                if (audioClip == null)
                    InitAudio();

                if (audioSource.isPlaying == false)
                    audioSource.Play();
            }
            else if (audioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            if (audioClip)
            {
                audioSource.pitch = Mathf.Clamp(dt * 90.0f - 0.7f, 0.1f, 50.0f);
                audioSource.volume = audioFeedbackVolume;
            }

            if (frameIter >= frameHistoryLength)
                frameIter = 0;

            beforeRender = (float)stopWatch.Elapsed.TotalSeconds;
        }
    }

    void LateUpdate()
    {
        eTotalSeconds = (float)stopWatch.Elapsed.TotalSeconds;
        dt = (eTotalSeconds - beforeRender);

        dtHistory[2, frameIter] = dt;

        beforeRender = eTotalSeconds;
    }

    protected override void OnPostRender()
    {
        base.OnPostRender();
    }

    void OnGUI()
    {
        if (Time.frameCount > 4)
            GUI.DrawTexture(new Rect(graphPosition.x * (Screen.width - graphMultiply * frameHistoryLength), graphPosition.y * (Screen.height - graphMultiply * 107) + 100 * graphMultiply, graphSizeGUI.width, graphSizeGUI.height), graphTexture);
    }

    protected override void Draw()
    {
        GL.PushMatrix();
        lineMaterial.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);

        xOff = graphPosition.x * (Screen.width - frameHistoryLength * graphMultiply);
        yOff = Screen.height - 100 * graphMultiply - graphPosition.y * (Screen.height - graphMultiply * 107);

        GL.Color(darkenedBackWhole);
        GL.Vertex3(xOff, yOff - 8 * graphMultiply, 0);
        GL.Vertex3(xOff, 100 * graphMultiply + yOff, 0);
        GL.Vertex3(graphSizeMin * graphMultiply + xOff, 100.0f * graphMultiply + yOff, 0);
        GL.Vertex3(graphSizeMin * graphMultiply + xOff, yOff - 8 * graphMultiply, 0);

        maxFrame = 0.0f;

        for (int x = 0; x < frameHistoryLength; ++x)
        {
            totalSeconds = dtHistory[0, x];

            if (totalSeconds > maxFrame)
                maxFrame = totalSeconds;

            totalSeconds *= yMulti;
            fpsVals[0] = totalSeconds;

            renderSeconds = dtHistory[1, x];
            renderSeconds *= yMulti;
            fpsVals[1] = renderSeconds;

            lateSeconds = dtHistory[2, x];
            lateSeconds *= yMulti;
            fpsVals[2] = lateSeconds;

            i = x - frameIter - 1;

            if (i < 0) i = frameHistoryLength + i;

            x1 = i * graphMultiply + xOff;
            x2 = (i + 1) * graphMultiply + xOff;

            for (int j = 0; j < fpsVals.Length; j++)
            {
                y1 = j < fpsVals.Length - 1 ? fpsVals[j + 1] * graphMultiply + yOff : yOff;
                y2 = fpsVals[j] * graphMultiply + yOff;

                GL.Color(fpsColorsTo[j]);
                GL.Vertex3(x1, y1, 0);
                GL.Vertex3(x2, y1, 0);
                GL.Color(fpsColors[j]);
                GL.Vertex3(x2, y2, 0);
                GL.Vertex3(x1, y2, 0);
            }
        }

        if (maxFrame < 1.0f / 120.0f)
        {
            maxFrame = 1.0f / 120.0f;
        }
        else if (maxFrame < 1.0f / 60.0f)
        {
            maxFrame = 1.0f / 60.0f;
        }
        else if (maxFrame < 1.0f / 30.0f)
        {
            maxFrame = 1.0f / 30.0f;
        }
        else if (maxFrame < 1.0f / 15.0f)
        {
            maxFrame = 1.0f / 15.0f;
        }
        else if (maxFrame < 1.0f / 10.0f)
        {
            maxFrame = 1.0f / 10.0f;
        }
        else if (maxFrame < 1.0f / 5.0f)
        {
            maxFrame = 1.0f / 5.0f;
        }

        yMulti = graphHeight / maxFrame;

        GL.Color(lineColor);

        x1 = 28 * graphMultiply + xOff;
        x2 = graphSizeMin * graphMultiply + xOff;

        for (int i = 0; i < lineY.Length; i++)
        {
            y1 = lineY[i] * graphMultiply + yOff;
            y2 = (lineY[i] + 1) * graphMultiply + yOff;

            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x1, y2, 0);
            GL.Vertex3(x2, y2, 0);
            GL.Vertex3(x2, y1, 0);
        }

        // Add FPS Shadows
        GL.Color(darkenedBack);

        x2 = 27 * graphMultiply + xOff;

        for (int i = 0; i < lineY.Length; i++)
        {
            y1 = lineY2[i] * graphMultiply + yOff;
            y2 = (lineY2[i] + 9) * graphMultiply + yOff;

            GL.Vertex3(xOff, y1, 0);
            GL.Vertex3(xOff, y2, 0);
            GL.Vertex3(x2, y2, 0);
            GL.Vertex3(x2, y1, 0);
        }

        // Add Key Boxes
        for (int i = 0; i < keyOffX.Length; i++)
        {
            x1 = keyOffX[i] * graphMultiply + xOff + 1 * graphMultiply;
            x2 = (keyOffX[i] + 4) * graphMultiply + xOff + 1 * graphMultiply;
            y1 = (5) * graphMultiply + yOff - 9 * graphMultiply;
            y2 = (1) * graphMultiply + yOff - 9 * graphMultiply;

            GL.Color(fpsColorsTo[i]);
            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x1, y2, 0);
            GL.Vertex3(x2, y2, 0);
            GL.Vertex3(x2, y1, 0);
        }

        for (int i = 0; i < keyOffX.Length; i++)
        {
            x1 = keyOffX[i] * graphMultiply + xOff;
            x2 = (keyOffX[i] + 4) * graphMultiply + xOff;
            y1 = (5) * graphMultiply + yOff - 8 * graphMultiply;
            y2 = (1) * graphMultiply + yOff - 8 * graphMultiply;

            GL.Color(fpsColors[i]);
            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x1, y2, 0);
            GL.Vertex3(x2, y2, 0);
            GL.Vertex3(x2, y1, 0);
        }

        GL.Color(Color.white);

        for (int x = 0; x < graphTexture.width; ++x)
        {
            for (int y = 0; y < graphHeight; ++y)
            {
                // Draw Text
                if (textOverlayMask[y, x] == 1)
                {
                    x1 = x * graphMultiply + xOff;
                    x2 = x * graphMultiply + 1 * graphMultiply + xOff;
                    y1 = y * graphMultiply + yOff;
                    y2 = y * graphMultiply + 1 * graphMultiply + yOff;

                    GL.Vertex3(x1, y1, 0);
                    GL.Vertex3(x1, y2, 0);
                    GL.Vertex3(x2, y2, 0);
                    GL.Vertex3(x2, y1, 0);
                }
            }
        }

        // Draw Mb
        for (int x = 0; x < 9; ++x)
        {
            for (int y = 0; y < 4; ++y)
            {
                if (mbBits[y * 9 + x] == 1)
                {
                    x1 = x * graphMultiply + xOff + 111 * graphMultiply;
                    x2 = x * graphMultiply + 1 * graphMultiply + xOff + 111 * graphMultiply;
                    y1 = y * graphMultiply + yOff + -7 * graphMultiply;
                    y2 = y * graphMultiply + 1 * graphMultiply + yOff + -7 * graphMultiply;

                    GL.Vertex3(x1, y1, 0);
                    GL.Vertex3(x1, y2, 0);
                    GL.Vertex3(x2, y2, 0);
                    GL.Vertex3(x2, y1, 0);
                }
            }
        }

        if (maxFrame > 0)
        {
            fps = Mathf.Round(1.0f / maxFrame);

            AddNumberAt(1, 93, (int)((fps / 100) % 10), true);
            AddNumberAt(5, 93, (int)((fps / 10.0) % 10), true);
            AddNumberAt(9, 93, (int)(fps % 10), false);

            fps *= 2;

            AddNumberAt(1, 48, (int)((fps / 100) % 10), true);
            AddNumberAt(5, 48, (int)((fps / 10) % 10), true);
            AddNumberAt(9, 48, (int)(fps % 10), false);

            fps *= 1.5f;

            AddNumberAt(1, 23, (int)((fps / 100) % 10), true);
            AddNumberAt(5, 23, (int)((fps / 10) % 10), true);
            AddNumberAt(9, 23, (int)(fps % 10), false);

            float mem = (GC.GetTotalMemory(true)) / 1000000.0f;

            if (mem < 1.0)
            {
                splitMb = mem.ToString("F2").Split("."[0]);

                if (splitMb[1][0] == "0"[0])
                {
                    first = 0;
                    second = int.Parse(splitMb[1]);
                }
                else
                {
                    first = int.Parse(splitMb[1]);
                    second = first % 10;
                    first = (first / 10) % 10;
                }

                addPeriodAt(100, -6);
                AddNumberAt(102, -7, first, false);
                AddNumberAt(106, -7, second, false);
            }
            else
            {
                splitMb = mem.ToString("F1").Split("."[0]);
                first = int.Parse(splitMb[0]);

                if (first >= 10) AddNumberAt(96, -7, first / 10, false);

                second = first % 10;

                if (second < 0) second = 0;

                AddNumberAt(100, -7, second, false);
                addPeriodAt(104, -6);
                AddNumberAt(106, -7, int.Parse(splitMb[1]), false);
            }
        }

        GL.End();
        GL.PopMatrix();

        dt = ((float)stopWatch.Elapsed.TotalSeconds - beforeRender);

        dtHistory[1, frameIter] = dt;

        eTotalSeconds = (float)stopWatch.Elapsed.TotalSeconds;

        dt = (eTotalSeconds - lastElapsed);
    }
}