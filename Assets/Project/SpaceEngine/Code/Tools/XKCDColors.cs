#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;

using UnityEngine;

namespace SpaceEngine.Tools
{
    public static class XKCDColors
    {
        internal class ColorTranslator
        {
            public static Color FromHtml(string hexString)
            {
                return new Color(Convert.ToInt32(hexString.Substring(1, 2), 16) / 255f,
                                 Convert.ToInt32(hexString.Substring(3, 2), 16) / 255f,
                                 Convert.ToInt32(hexString.Substring(5, 2), 16) / 255f);
            }

            public static string ToRGBHex(Color color)
            {
                return $"#{ToByte(color.r):X2}{ToByte(color.g):X2}{ToByte(color.b):X2}";
            }

            private static byte ToByte(float value)
            {
                value = Mathf.Clamp01(value);

                return (byte)(value * 255);
            }
        }

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.9960784, 0.03529412)
        /// </summary>
        public static readonly Color AcidGreen = new Color(0.5607843f, 0.9960784f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7411765, 0.4235294, 0.282353)
        /// </summary>
        public static readonly Color Adobe = new Color(0.7411765f, 0.4235294f, 0.282353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3294118, 0.6745098, 0.4078431)
        /// </summary>
        public static readonly Color Algae = new Color(0.3294118f, 0.6745098f, 0.4078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1294118, 0.7647059, 0.4352941)
        /// </summary>
        public static readonly Color AlgaeGreen = new Color(0.1294118f, 0.7647059f, 0.4352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02745098, 0.05098039, 0.05098039)
        /// </summary>
        public static readonly Color AlmostBlack = new Color(0.02745098f, 0.05098039f, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.7019608, 0.03137255)
        /// </summary>
        public static readonly Color Amber = new Color(0.9960784f, 0.7019608f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6078432, 0.372549, 0.7529412)
        /// </summary>
        public static readonly Color Amethyst = new Color(0.6078432f, 0.372549f, 0.7529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4313726, 0.7960784, 0.2352941)
        /// </summary>
        public static readonly Color Apple = new Color(0.4313726f, 0.7960784f, 0.2352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4627451, 0.8039216, 0.1490196)
        /// </summary>
        public static readonly Color AppleGreen = new Color(0.4627451f, 0.8039216f, 0.1490196f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6941177, 0.427451)
        /// </summary>
        public static readonly Color Apricot = new Color(1, 0.6941177f, 0.427451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.07450981, 0.9176471, 0.7882353)
        /// </summary>
        public static readonly Color Aqua = new Color(0.07450981f, 0.9176471f, 0.7882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.8470588, 0.9137255)
        /// </summary>
        public static readonly Color AquaBlue = new Color(0.007843138f, 0.8470588f, 0.9137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.07058824, 0.8823529, 0.5764706)
        /// </summary>
        public static readonly Color AquaGreen = new Color(0.07058824f, 0.8823529f, 0.5764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1803922, 0.9098039, 0.7333333)
        /// </summary>
        public static readonly Color AquaMarine = new Color(0.1803922f, 0.9098039f, 0.7333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.8470588, 0.6980392)
        /// </summary>
        public static readonly Color Aquamarine = new Color(0.01568628f, 0.8470588f, 0.6980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2941177, 0.3647059, 0.08627451)
        /// </summary>
        public static readonly Color ArmyGreen = new Color(0.2941177f, 0.3647059f, 0.08627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4666667, 0.6705883, 0.3372549)
        /// </summary>
        public static readonly Color Asparagus = new Color(0.4666667f, 0.6705883f, 0.3372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2392157, 0.02745098, 0.2039216)
        /// </summary>
        public static readonly Color Aubergine = new Color(0.2392157f, 0.02745098f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6039216, 0.1882353, 0.003921569)
        /// </summary>
        public static readonly Color Auburn = new Color(0.6039216f, 0.1882353f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5647059, 0.6941177, 0.2039216)
        /// </summary>
        public static readonly Color Avocado = new Color(0.5647059f, 0.6941177f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.6627451, 0.1333333)
        /// </summary>
        public static readonly Color AvocadoGreen = new Color(0.5294118f, 0.6627451f, 0.1333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1137255, 0.3647059, 0.9254902)
        /// </summary>
        public static readonly Color Azul = new Color(0.1137255f, 0.3647059f, 0.9254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.6039216, 0.9529412)
        /// </summary>
        public static readonly Color Azure = new Color(0.02352941f, 0.6039216f, 0.9529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6352941, 0.8117647, 0.9960784)
        /// </summary>
        public static readonly Color BabyBlue = new Color(0.6352941f, 0.8117647f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5490196, 1, 0.6196079)
        /// </summary>
        public static readonly Color BabyGreen = new Color(0.5490196f, 1, 0.6196079f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.7176471, 0.8078431)
        /// </summary>
        public static readonly Color BabyPink = new Color(1, 0.7176471f, 0.8078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6705883, 0.5647059, 0.01568628)
        /// </summary>
        public static readonly Color BabyPoo = new Color(0.6705883f, 0.5647059f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5764706, 0.4862745, 0)
        /// </summary>
        public static readonly Color BabyPoop = new Color(0.5764706f, 0.4862745f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.5960785, 0.01960784)
        /// </summary>
        public static readonly Color BabyPoopGreen = new Color(0.5607843f, 0.5960785f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7137255, 0.7686275, 0.02352941)
        /// </summary>
        public static readonly Color BabyPukeGreen = new Color(0.7137255f, 0.7686275f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 0.6078432, 0.9686275)
        /// </summary>
        public static readonly Color BabyPurple = new Color(0.7921569f, 0.6078432f, 0.9686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6784314, 0.5647059, 0.05098039)
        /// </summary>
        public static readonly Color BabyShitBrown = new Color(0.6784314f, 0.5647059f, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5333334, 0.5921569, 0.09019608)
        /// </summary>
        public static readonly Color BabyShitGreen = new Color(0.5333334f, 0.5921569f, 0.09019608f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.4941176)
        /// </summary>
        public static readonly Color Banana = new Color(1, 1, 0.4941176f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9803922, 0.9960784, 0.2941177)
        /// </summary>
        public static readonly Color BananaYellow = new Color(0.9803922f, 0.9960784f, 0.2941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.2745098, 0.6470588)
        /// </summary>
        public static readonly Color BarbiePink = new Color(0.9960784f, 0.2745098f, 0.6470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5803922, 0.6745098, 0.007843138)
        /// </summary>
        public static readonly Color BarfGreen = new Color(0.5803922f, 0.6745098f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.1137255, 0.7215686)
        /// </summary>
        public static readonly Color Barney = new Color(0.6745098f, 0.1137255f, 0.7215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.627451, 0.01568628, 0.5960785)
        /// </summary>
        public static readonly Color BarneyPurple = new Color(0.627451f, 0.01568628f, 0.5960785f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4196078, 0.4862745, 0.5215687)
        /// </summary>
        public static readonly Color BattleshipGrey = new Color(0.4196078f, 0.4862745f, 0.5215687f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9019608, 0.854902, 0.6509804)
        /// </summary>
        public static readonly Color Beige = new Color(0.9019608f, 0.854902f, 0.6509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6, 0.05882353, 0.2941177)
        /// </summary>
        public static readonly Color Berry = new Color(0.6f, 0.05882353f, 0.2941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7098039, 0.7647059, 0.02352941)
        /// </summary>
        public static readonly Color Bile = new Color(0.7098039f, 0.7647059f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0, 0)
        /// </summary>
        public static readonly Color Black = new Color(0, 0, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.6862745, 0.6588235, 0.5450981)
        /// </summary>
        public static readonly Color Bland = new Color(0.6862745f, 0.6588235f, 0.5450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4666667, 0, 0.003921569)
        /// </summary>
        public static readonly Color Blood = new Color(0.4666667f, 0, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.2941177, 0.01176471)
        /// </summary>
        public static readonly Color BloodOrange = new Color(0.9960784f, 0.2941177f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5960785, 0, 0.007843138)
        /// </summary>
        public static readonly Color BloodRed = new Color(0.5960785f, 0, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.2627451, 0.8745098)
        /// </summary>
        public static readonly Color Blue = new Color(0.01176471f, 0.2627451f, 0.8745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1333333, 0.2588235, 0.7803922)
        /// </summary>
        public static readonly Color BlueBlue = new Color(0.1333333f, 0.2588235f, 0.7803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.07450981, 0.4941176, 0.427451)
        /// </summary>
        public static readonly Color BlueGreen = new Color(0.07450981f, 0.4941176f, 0.427451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3764706, 0.4862745, 0.5568628)
        /// </summary>
        public static readonly Color BlueGrey = new Color(0.3764706f, 0.4862745f, 0.5568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3411765, 0.1607843, 0.8078431)
        /// </summary>
        public static readonly Color BluePurple = new Color(0.3411765f, 0.1607843f, 0.8078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3647059, 0.02352941, 0.9137255)
        /// </summary>
        public static readonly Color BlueViolet = new Color(0.3647059f, 0.02352941f, 0.9137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3254902, 0.2352941, 0.7764706)
        /// </summary>
        public static readonly Color BlueWithAHintOfPurple = new Color(0.3254902f, 0.2352941f, 0.7764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.05882353, 0.6078432, 0.5568628)
        /// </summary>
        public static readonly Color Blue_Green = new Color(0.05882353f, 0.6078432f, 0.5568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.5529412, 0.6392157)
        /// </summary>
        public static readonly Color Blue_Grey = new Color(0.4588235f, 0.5529412f, 0.6392157f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3529412, 0.02352941, 0.9372549)
        /// </summary>
        public static readonly Color Blue_Purple = new Color(0.3529412f, 0.02352941f, 0.9372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2745098, 0.254902, 0.5882353)
        /// </summary>
        public static readonly Color Blueberry = new Color(0.2745098f, 0.254902f, 0.5882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.4784314, 0.4745098)
        /// </summary>
        public static readonly Color Bluegreen = new Color(0.003921569f, 0.4784314f, 0.4745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5215687, 0.6392157, 0.6980392)
        /// </summary>
        public static readonly Color Bluegrey = new Color(0.5215687f, 0.6392157f, 0.6980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1686275, 0.6941177, 0.4745098)
        /// </summary>
        public static readonly Color BlueyGreen = new Color(0.1686275f, 0.6941177f, 0.4745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5372549, 0.627451, 0.6901961)
        /// </summary>
        public static readonly Color BlueyGrey = new Color(0.5372549f, 0.627451f, 0.6901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3843137, 0.254902, 0.7803922)
        /// </summary>
        public static readonly Color BlueyPurple = new Color(0.3843137f, 0.254902f, 0.7803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1607843, 0.4627451, 0.7333333)
        /// </summary>
        public static readonly Color Bluish = new Color(0.1607843f, 0.4627451f, 0.7333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.0627451, 0.6509804, 0.454902)
        /// </summary>
        public static readonly Color BluishGreen = new Color(0.0627451f, 0.6509804f, 0.454902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.454902, 0.5450981, 0.5921569)
        /// </summary>
        public static readonly Color BluishGrey = new Color(0.454902f, 0.5450981f, 0.5921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4392157, 0.2313726, 0.9058824)
        /// </summary>
        public static readonly Color BluishPurple = new Color(0.4392157f, 0.2313726f, 0.9058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3333333, 0.2235294, 0.8)
        /// </summary>
        public static readonly Color Blurple = new Color(0.3333333f, 0.2235294f, 0.8f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9490196, 0.6196079, 0.5568628)
        /// </summary>
        public static readonly Color Blush = new Color(0.9490196f, 0.6196079f, 0.5568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.509804, 0.5490196)
        /// </summary>
        public static readonly Color BlushPink = new Color(0.9960784f, 0.509804f, 0.5490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6078432, 0.7098039, 0.2352941)
        /// </summary>
        public static readonly Color Booger = new Color(0.6078432f, 0.7098039f, 0.2352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5882353, 0.7058824, 0.01176471)
        /// </summary>
        public static readonly Color BoogerGreen = new Color(0.5882353f, 0.7058824f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0, 0.172549)
        /// </summary>
        public static readonly Color Bordeaux = new Color(0.4823529f, 0, 0.172549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3882353, 0.7019608, 0.3960784)
        /// </summary>
        public static readonly Color BoringGreen = new Color(0.3882353f, 0.7019608f, 0.3960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.2901961, 0.01960784)
        /// </summary>
        public static readonly Color BottleGreen = new Color(0.01568628f, 0.2901961f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.627451, 0.2117647, 0.1372549)
        /// </summary>
        public static readonly Color Brick = new Color(0.627451f, 0.2117647f, 0.1372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7568628, 0.2901961, 0.03529412)
        /// </summary>
        public static readonly Color BrickOrange = new Color(0.7568628f, 0.2901961f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.07843138, 0.007843138)
        /// </summary>
        public static readonly Color BrickRed = new Color(0.5607843f, 0.07843138f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04313726, 0.9764706, 0.9176471)
        /// </summary>
        public static readonly Color BrightAqua = new Color(0.04313726f, 0.9764706f, 0.9176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.3960784, 0.9882353)
        /// </summary>
        public static readonly Color BrightBlue = new Color(0.003921569f, 0.3960784f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.254902, 0.9921569, 0.9960784)
        /// </summary>
        public static readonly Color BrightCyan = new Color(0.254902f, 0.9921569f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 1, 0.02745098)
        /// </summary>
        public static readonly Color BrightGreen = new Color(0.003921569f, 1, 0.02745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.3764706, 1)
        /// </summary>
        public static readonly Color BrightLavender = new Color(0.7803922f, 0.3764706f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.1490196, 0.9686275, 0.9921569)
        /// </summary>
        public static readonly Color BrightLightBlue = new Color(0.1490196f, 0.9686275f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1764706, 0.9960784, 0.3294118)
        /// </summary>
        public static readonly Color BrightLightGreen = new Color(0.1764706f, 0.9960784f, 0.3294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 0.3686275, 0.9843137)
        /// </summary>
        public static readonly Color BrightLilac = new Color(0.7882353f, 0.3686275f, 0.9843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.9921569, 0.01960784)
        /// </summary>
        public static readonly Color BrightLime = new Color(0.5294118f, 0.9921569f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.9960784, 0.03137255)
        /// </summary>
        public static readonly Color BrightLimeGreen = new Color(0.3960784f, 0.9960784f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.03137255, 0.9098039)
        /// </summary>
        public static readonly Color BrightMagenta = new Color(1, 0.03137255f, 0.9098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6117647, 0.7333333, 0.01568628)
        /// </summary>
        public static readonly Color BrightOlive = new Color(0.6117647f, 0.7333333f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.3568628, 0)
        /// </summary>
        public static readonly Color BrightOrange = new Color(1, 0.3568628f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.003921569, 0.6941177)
        /// </summary>
        public static readonly Color BrightPink = new Color(0.9960784f, 0.003921569f, 0.6941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.01176471, 0.9921569)
        /// </summary>
        public static readonly Color BrightPurple = new Color(0.7450981f, 0.01176471f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0, 0.05098039)
        /// </summary>
        public static readonly Color BrightRed = new Color(1, 0, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 1, 0.6509804)
        /// </summary>
        public static readonly Color BrightSeaGreen = new Color(0.01960784f, 1, 0.6509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.8, 0.9960784)
        /// </summary>
        public static readonly Color BrightSkyBlue = new Color(0.007843138f, 0.8f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.9764706, 0.7764706)
        /// </summary>
        public static readonly Color BrightTeal = new Color(0.003921569f, 0.9764706f, 0.7764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.05882353, 0.9960784, 0.9764706)
        /// </summary>
        public static readonly Color BrightTurquoise = new Color(0.05882353f, 0.9960784f, 0.9764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6784314, 0.03921569, 0.9921569)
        /// </summary>
        public static readonly Color BrightViolet = new Color(0.6784314f, 0.03921569f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9921569, 0.003921569)
        /// </summary>
        public static readonly Color BrightYellow = new Color(1, 0.9921569f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 1, 0)
        /// </summary>
        public static readonly Color BrightYellowGreen = new Color(0.6156863f, 1, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 0.282353, 0.05098039)
        /// </summary>
        public static readonly Color BritishRacingGreen = new Color(0.01960784f, 0.282353f, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.4745098, 0)
        /// </summary>
        public static readonly Color Bronze = new Color(0.6588235f, 0.4745098f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.2156863, 0)
        /// </summary>
        public static readonly Color Brown = new Color(0.3960784f, 0.2156863f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.4392157, 0.4235294, 0.06666667)
        /// </summary>
        public static readonly Color BrownGreen = new Color(0.4392157f, 0.4235294f, 0.06666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5529412, 0.5176471, 0.4078431)
        /// </summary>
        public static readonly Color BrownGrey = new Color(0.5529412f, 0.5176471f, 0.4078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7254902, 0.4117647, 0.007843138)
        /// </summary>
        public static readonly Color BrownOrange = new Color(0.7254902f, 0.4117647f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.572549, 0.1686275, 0.01960784)
        /// </summary>
        public static readonly Color BrownRed = new Color(0.572549f, 0.1686275f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.5921569, 0.01960784)
        /// </summary>
        public static readonly Color BrownYellow = new Color(0.6980392f, 0.5921569f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6117647, 0.427451, 0.3411765)
        /// </summary>
        public static readonly Color Brownish = new Color(0.6117647f, 0.427451f, 0.3411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4156863, 0.4313726, 0.03529412)
        /// </summary>
        public static readonly Color BrownishGreen = new Color(0.4156863f, 0.4313726f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5254902, 0.4666667, 0.372549)
        /// </summary>
        public static readonly Color BrownishGrey = new Color(0.5254902f, 0.4666667f, 0.372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.4666667, 0.1372549)
        /// </summary>
        public static readonly Color BrownishOrange = new Color(0.7960784f, 0.4666667f, 0.1372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7607843, 0.4941176, 0.4745098)
        /// </summary>
        public static readonly Color BrownishPink = new Color(0.7607843f, 0.4941176f, 0.4745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4627451, 0.2588235, 0.3058824)
        /// </summary>
        public static readonly Color BrownishPurple = new Color(0.4627451f, 0.2588235f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6196079, 0.2117647, 0.1372549)
        /// </summary>
        public static readonly Color BrownishRed = new Color(0.6196079f, 0.2117647f, 0.1372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 0.6901961, 0.01176471)
        /// </summary>
        public static readonly Color BrownishYellow = new Color(0.7882353f, 0.6901961f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4352941, 0.4235294, 0.03921569)
        /// </summary>
        public static readonly Color BrownyGreen = new Color(0.4352941f, 0.4235294f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 0.4196078, 0.007843138)
        /// </summary>
        public static readonly Color BrownyOrange = new Color(0.7921569f, 0.4196078f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4941176, 0.2509804, 0.4431373)
        /// </summary>
        public static readonly Color Bruise = new Color(0.4941176f, 0.2509804f, 0.4431373f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4117647, 0.6862745)
        /// </summary>
        public static readonly Color BubbleGumPink = new Color(1, 0.4117647f, 0.6862745f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4235294, 0.7098039)
        /// </summary>
        public static readonly Color Bubblegum = new Color(1, 0.4235294f, 0.7098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.5137255, 0.8)
        /// </summary>
        public static readonly Color BubblegumPink = new Color(0.9960784f, 0.5137255f, 0.8f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.9647059, 0.6196079)
        /// </summary>
        public static readonly Color Buff = new Color(0.9960784f, 0.9647059f, 0.6196079f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3803922, 0, 0.1372549)
        /// </summary>
        public static readonly Color Burgundy = new Color(0.3803922f, 0, 0.1372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7529412, 0.3058824, 0.003921569)
        /// </summary>
        public static readonly Color BurntOrange = new Color(0.7529412f, 0.3058824f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6235294, 0.1372549, 0.01960784)
        /// </summary>
        public static readonly Color BurntRed = new Color(0.6235294f, 0.1372549f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7176471, 0.3215686, 0.01176471)
        /// </summary>
        public static readonly Color BurntSiena = new Color(0.7176471f, 0.3215686f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6901961, 0.3058824, 0.05882353)
        /// </summary>
        public static readonly Color BurntSienna = new Color(0.6901961f, 0.3058824f, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.627451, 0.2705882, 0.05490196)
        /// </summary>
        public static readonly Color BurntUmber = new Color(0.627451f, 0.2705882f, 0.05490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8352941, 0.6705883, 0.03529412)
        /// </summary>
        public static readonly Color BurntYellow = new Color(0.8352941f, 0.6705883f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4078431, 0.1960784, 0.8901961)
        /// </summary>
        public static readonly Color Burple = new Color(0.4078431f, 0.1960784f, 0.8901961f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.5058824)
        /// </summary>
        public static readonly Color Butter = new Color(1, 1, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9921569, 0.454902)
        /// </summary>
        public static readonly Color ButterYellow = new Color(1, 0.9921569f, 0.454902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.6941177, 0.2784314)
        /// </summary>
        public static readonly Color Butterscotch = new Color(0.9921569f, 0.6941177f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3058824, 0.454902, 0.5882353)
        /// </summary>
        public static readonly Color CadetBlue = new Color(0.3058824f, 0.454902f, 0.5882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7764706, 0.6235294, 0.3490196)
        /// </summary>
        public static readonly Color Camel = new Color(0.7764706f, 0.6235294f, 0.3490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.5607843, 0.3058824)
        /// </summary>
        public static readonly Color Camo = new Color(0.4980392f, 0.5607843f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3215686, 0.3960784, 0.145098)
        /// </summary>
        public static readonly Color CamoGreen = new Color(0.3215686f, 0.3960784f, 0.145098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2941177, 0.3803922, 0.07450981)
        /// </summary>
        public static readonly Color CamouflageGreen = new Color(0.2941177f, 0.3803922f, 0.07450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 1, 0.3882353)
        /// </summary>
        public static readonly Color Canary = new Color(0.9921569f, 1, 0.3882353f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9960784, 0.2509804)
        /// </summary>
        public static readonly Color CanaryYellow = new Color(1, 0.9960784f, 0.2509804f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.3882353, 0.9137255)
        /// </summary>
        public static readonly Color CandyPink = new Color(1, 0.3882353f, 0.9137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6862745, 0.4352941, 0.03529412)
        /// </summary>
        public static readonly Color Caramel = new Color(0.6862745f, 0.4352941f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.007843138, 0.08627451)
        /// </summary>
        public static readonly Color Carmine = new Color(0.6156863f, 0.007843138f, 0.08627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.4745098, 0.5607843)
        /// </summary>
        public static readonly Color Carnation = new Color(0.9921569f, 0.4745098f, 0.5607843f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4980392, 0.654902)
        /// </summary>
        public static readonly Color CarnationPink = new Color(1, 0.4980392f, 0.654902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5411765, 0.7215686, 0.9960784)
        /// </summary>
        public static readonly Color CarolinaBlue = new Color(0.5411765f, 0.7215686f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.9921569, 0.7176471)
        /// </summary>
        public static readonly Color Celadon = new Color(0.7450981f, 0.9921569f, 0.7176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7568628, 0.9921569, 0.5843138)
        /// </summary>
        public static readonly Color Celery = new Color(0.7568628f, 0.9921569f, 0.5843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0.6392157, 0.5686275)
        /// </summary>
        public static readonly Color Cement = new Color(0.6470588f, 0.6392157f, 0.5686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8705882, 0.04705882, 0.3843137)
        /// </summary>
        public static readonly Color Cerise = new Color(0.8705882f, 0.04705882f, 0.3843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.5215687, 0.8196079)
        /// </summary>
        public static readonly Color Cerulean = new Color(0.01568628f, 0.5215687f, 0.8196079f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 0.4313726, 0.9333333)
        /// </summary>
        public static readonly Color CeruleanBlue = new Color(0.01960784f, 0.4313726f, 0.9333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2039216, 0.2196078, 0.2156863)
        /// </summary>
        public static readonly Color Charcoal = new Color(0.2039216f, 0.2196078f, 0.2156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2352941, 0.254902, 0.2588235)
        /// </summary>
        public static readonly Color CharcoalGrey = new Color(0.2352941f, 0.254902f, 0.2588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7568628, 0.972549, 0.03921569)
        /// </summary>
        public static readonly Color Chartreuse = new Color(0.7568628f, 0.972549f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8117647, 0.007843138, 0.2039216)
        /// </summary>
        public static readonly Color Cherry = new Color(0.8117647f, 0.007843138f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9686275, 0.007843138, 0.1647059)
        /// </summary>
        public static readonly Color CherryRed = new Color(0.9686275f, 0.007843138f, 0.1647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.454902, 0.1568628, 0.007843138)
        /// </summary>
        public static readonly Color Chestnut = new Color(0.454902f, 0.1568628f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2392157, 0.1098039, 0.007843138)
        /// </summary>
        public static readonly Color Chocolate = new Color(0.2392157f, 0.1098039f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.254902, 0.09803922, 0)
        /// </summary>
        public static readonly Color ChocolateBrown = new Color(0.254902f, 0.09803922f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.3098039, 0.02352941)
        /// </summary>
        public static readonly Color Cinnamon = new Color(0.6745098f, 0.3098039f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4078431, 0, 0.09411765)
        /// </summary>
        public static readonly Color Claret = new Color(0.4078431f, 0, 0.09411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7137255, 0.4156863, 0.3137255)
        /// </summary>
        public static readonly Color Clay = new Color(0.7137255f, 0.4156863f, 0.3137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.4431373, 0.2392157)
        /// </summary>
        public static readonly Color ClayBrown = new Color(0.6980392f, 0.4431373f, 0.2392157f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1411765, 0.4784314, 0.9921569)
        /// </summary>
        public static readonly Color ClearBlue = new Color(0.1411765f, 0.4784314f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.7607843, 0.8509804)
        /// </summary>
        public static readonly Color CloudyBlue = new Color(0.6745098f, 0.7607843f, 0.8509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1176471, 0.282353, 0.5607843)
        /// </summary>
        public static readonly Color Cobalt = new Color(0.1176471f, 0.282353f, 0.5607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.03921569, 0.654902)
        /// </summary>
        public static readonly Color CobaltBlue = new Color(0.01176471f, 0.03921569f, 0.654902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.372549, 0.2588235)
        /// </summary>
        public static readonly Color Cocoa = new Color(0.5294118f, 0.372549f, 0.2588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6509804, 0.5058824, 0.2980392)
        /// </summary>
        public static readonly Color Coffee = new Color(0.6509804f, 0.5058824f, 0.2980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2862745, 0.5176471, 0.7215686)
        /// </summary>
        public static readonly Color CoolBlue = new Color(0.2862745f, 0.5176471f, 0.7215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2, 0.7215686, 0.3921569)
        /// </summary>
        public static readonly Color CoolGreen = new Color(0.2f, 0.7215686f, 0.3921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5843138, 0.6392157, 0.6509804)
        /// </summary>
        public static readonly Color CoolGrey = new Color(0.5843138f, 0.6392157f, 0.6509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7137255, 0.3882353, 0.145098)
        /// </summary>
        public static readonly Color Copper = new Color(0.7137255f, 0.3882353f, 0.145098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.3529412, 0.3137255)
        /// </summary>
        public static readonly Color Coral = new Color(0.9882353f, 0.3529412f, 0.3137255f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.3803922, 0.3882353)
        /// </summary>
        public static readonly Color CoralPink = new Color(1, 0.3803922f, 0.3882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4156863, 0.4745098, 0.9686275)
        /// </summary>
        public static readonly Color Cornflower = new Color(0.4156863f, 0.4745098f, 0.9686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3176471, 0.4392157, 0.8431373)
        /// </summary>
        public static readonly Color CornflowerBlue = new Color(0.3176471f, 0.4392157f, 0.8431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6196079, 0, 0.227451)
        /// </summary>
        public static readonly Color Cranberry = new Color(0.6196079f, 0, 0.227451f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.7607843)
        /// </summary>
        public static readonly Color Cream = new Color(1, 1, 0.7607843f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.7137255)
        /// </summary>
        public static readonly Color Creme = new Color(1, 1, 0.7137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5490196, 0, 0.05882353)
        /// </summary>
        public static readonly Color Crimson = new Color(0.5490196f, 0, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9921569, 0.4705882)
        /// </summary>
        public static readonly Color Custard = new Color(1, 0.9921569f, 0.4705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 1, 1)
        /// </summary>
        public static readonly Color Cyan = new Color(0, 1, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.8745098, 0.03137255)
        /// </summary>
        public static readonly Color Dandelion = new Color(0.9960784f, 0.8745098f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1058824, 0.1411765, 0.1921569)
        /// </summary>
        public static readonly Color Dark = new Color(0.1058824f, 0.1411765f, 0.1921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 0.4117647, 0.4196078)
        /// </summary>
        public static readonly Color DarkAqua = new Color(0.01960784f, 0.4117647f, 0.4196078f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.4509804, 0.4431373)
        /// </summary>
        public static readonly Color DarkAquamarine = new Color(0.003921569f, 0.4509804f, 0.4431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.5764706, 0.3843137)
        /// </summary>
        public static readonly Color DarkBeige = new Color(0.6745098f, 0.5764706f, 0.3843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.01176471, 0.3568628)
        /// </summary>
        public static readonly Color DarkBlue = new Color(0, 0.01176471f, 0.3568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.3215686, 0.2862745)
        /// </summary>
        public static readonly Color DarkBlueGreen = new Color(0, 0.3215686f, 0.2862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1215686, 0.2313726, 0.3019608)
        /// </summary>
        public static readonly Color DarkBlueGrey = new Color(0.1215686f, 0.2313726f, 0.3019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2039216, 0.1098039, 0.007843138)
        /// </summary>
        public static readonly Color DarkBrown = new Color(0.2039216f, 0.1098039f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8117647, 0.3215686, 0.3058824)
        /// </summary>
        public static readonly Color DarkCoral = new Color(0.8117647f, 0.3215686f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9529412, 0.6039216)
        /// </summary>
        public static readonly Color DarkCream = new Color(1, 0.9529412f, 0.6039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03921569, 0.5333334, 0.5411765)
        /// </summary>
        public static readonly Color DarkCyan = new Color(0.03921569f, 0.5333334f, 0.5411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.1764706, 0.01568628)
        /// </summary>
        public static readonly Color DarkForestGreen = new Color(0, 0.1764706f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.02745098, 0.3490196)
        /// </summary>
        public static readonly Color DarkFuchsia = new Color(0.6156863f, 0.02745098f, 0.3490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7098039, 0.5803922, 0.0627451)
        /// </summary>
        public static readonly Color DarkGold = new Color(0.7098039f, 0.5803922f, 0.0627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2196078, 0.5019608, 0.01568628)
        /// </summary>
        public static readonly Color DarkGrassGreen = new Color(0.2196078f, 0.5019608f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.2078431, 0)
        /// </summary>
        public static readonly Color DarkGreen = new Color(0.01176471f, 0.2078431f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.1215686, 0.3882353, 0.3411765)
        /// </summary>
        public static readonly Color DarkGreenBlue = new Color(0.1215686f, 0.3882353f, 0.3411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2117647, 0.2156863, 0.2156863)
        /// </summary>
        public static readonly Color DarkGrey = new Color(0.2117647f, 0.2156863f, 0.2156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1607843, 0.2745098, 0.3568628)
        /// </summary>
        public static readonly Color DarkGreyBlue = new Color(0.1607843f, 0.2745098f, 0.3568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8509804, 0.003921569, 0.4)
        /// </summary>
        public static readonly Color DarkHotPink = new Color(0.8509804f, 0.003921569f, 0.4f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1215686, 0.03529412, 0.3294118)
        /// </summary>
        public static readonly Color DarkIndigo = new Color(0.1215686f, 0.03529412f, 0.3294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6078432, 0.5607843, 0.3333333)
        /// </summary>
        public static readonly Color DarkKhaki = new Color(0.6078432f, 0.5607843f, 0.3333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5215687, 0.4039216, 0.5960785)
        /// </summary>
        public static readonly Color DarkLavender = new Color(0.5215687f, 0.4039216f, 0.5960785f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6117647, 0.427451, 0.6470588)
        /// </summary>
        public static readonly Color DarkLilac = new Color(0.6117647f, 0.427451f, 0.6470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5176471, 0.7176471, 0.003921569)
        /// </summary>
        public static readonly Color DarkLime = new Color(0.5176471f, 0.7176471f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4941176, 0.7411765, 0.003921569)
        /// </summary>
        public static readonly Color DarkLimeGreen = new Color(0.4941176f, 0.7411765f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5882353, 0, 0.3372549)
        /// </summary>
        public static readonly Color DarkMagenta = new Color(0.5882353f, 0, 0.3372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2352941, 0, 0.03137255)
        /// </summary>
        public static readonly Color DarkMaroon = new Color(0.2352941f, 0, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.2980392, 0.3843137)
        /// </summary>
        public static readonly Color DarkMauve = new Color(0.5294118f, 0.2980392f, 0.3843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.282353, 0.7529412, 0.4470588)
        /// </summary>
        public static readonly Color DarkMint = new Color(0.282353f, 0.7529412f, 0.4470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1254902, 0.7529412, 0.4509804)
        /// </summary>
        public static readonly Color DarkMintGreen = new Color(0.1254902f, 0.7529412f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.5372549, 0.01960784)
        /// </summary>
        public static readonly Color DarkMustard = new Color(0.6588235f, 0.5372549f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.01568628, 0.2078431)
        /// </summary>
        public static readonly Color DarkNavy = new Color(0, 0.01568628f, 0.2078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.007843138, 0.1803922)
        /// </summary>
        public static readonly Color DarkNavyBlue = new Color(0, 0.007843138f, 0.1803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2156863, 0.2431373, 0.007843138)
        /// </summary>
        public static readonly Color DarkOlive = new Color(0.2156863f, 0.2431373f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2352941, 0.3019608, 0.01176471)
        /// </summary>
        public static readonly Color DarkOliveGreen = new Color(0.2352941f, 0.3019608f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7764706, 0.3176471, 0.007843138)
        /// </summary>
        public static readonly Color DarkOrange = new Color(0.7764706f, 0.3176471f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3372549, 0.682353, 0.3411765)
        /// </summary>
        public static readonly Color DarkPastelGreen = new Color(0.3372549f, 0.682353f, 0.3411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8705882, 0.4941176, 0.3647059)
        /// </summary>
        public static readonly Color DarkPeach = new Color(0.8705882f, 0.4941176f, 0.3647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4, 0.372549, 0.8196079)
        /// </summary>
        public static readonly Color DarkPeriwinkle = new Color(0.4f, 0.372549f, 0.8196079f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.254902, 0.4196078)
        /// </summary>
        public static readonly Color DarkPink = new Color(0.7960784f, 0.254902f, 0.4196078f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2470588, 0.003921569, 0.172549)
        /// </summary>
        public static readonly Color DarkPlum = new Color(0.2470588f, 0.003921569f, 0.172549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2078431, 0.02352941, 0.2431373)
        /// </summary>
        public static readonly Color DarkPurple = new Color(0.2078431f, 0.02352941f, 0.2431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5176471, 0, 0)
        /// </summary>
        public static readonly Color DarkRed = new Color(0.5176471f, 0, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.7098039, 0.282353, 0.3647059)
        /// </summary>
        public static readonly Color DarkRose = new Color(0.7098039f, 0.282353f, 0.3647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.02352941, 0.4352941)
        /// </summary>
        public static readonly Color DarkRoyalBlue = new Color(0.007843138f, 0.02352941f, 0.4352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3490196, 0.5215687, 0.3372549)
        /// </summary>
        public static readonly Color DarkSage = new Color(0.3490196f, 0.5215687f, 0.3372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.3529412, 0.3254902)
        /// </summary>
        public static readonly Color DarkSalmon = new Color(0.7843137f, 0.3529412f, 0.3254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.5607843, 0.3490196)
        /// </summary>
        public static readonly Color DarkSand = new Color(0.6588235f, 0.5607843f, 0.3490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.06666667, 0.5294118, 0.3647059)
        /// </summary>
        public static readonly Color DarkSeaGreen = new Color(0.06666667f, 0.5294118f, 0.3647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1215686, 0.7098039, 0.4784314)
        /// </summary>
        public static readonly Color DarkSeafoam = new Color(0.1215686f, 0.7098039f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2431373, 0.6862745, 0.4627451)
        /// </summary>
        public static readonly Color DarkSeafoamGreen = new Color(0.2431373f, 0.6862745f, 0.4627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2666667, 0.5568628, 0.8941177)
        /// </summary>
        public static readonly Color DarkSkyBlue = new Color(0.2666667f, 0.5568628f, 0.8941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1294118, 0.2784314, 0.3803922)
        /// </summary>
        public static readonly Color DarkSlateBlue = new Color(0.1294118f, 0.2784314f, 0.3803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6862745, 0.5333334, 0.2901961)
        /// </summary>
        public static readonly Color DarkTan = new Color(0.6862745f, 0.5333334f, 0.2901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.4078431, 0.3058824)
        /// </summary>
        public static readonly Color DarkTaupe = new Color(0.4980392f, 0.4078431f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.3019608, 0.3058824)
        /// </summary>
        public static readonly Color DarkTeal = new Color(0.003921569f, 0.3019608f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.3607843, 0.3529412)
        /// </summary>
        public static readonly Color DarkTurquoise = new Color(0.01568628f, 0.3607843f, 0.3529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2039216, 0.003921569, 0.2470588)
        /// </summary>
        public static readonly Color DarkViolet = new Color(0.2039216f, 0.003921569f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8352941, 0.7137255, 0.03921569)
        /// </summary>
        public static readonly Color DarkYellow = new Color(0.8352941f, 0.7137255f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4470588, 0.5607843, 0.007843138)
        /// </summary>
        public static readonly Color DarkYellowGreen = new Color(0.4470588f, 0.5607843f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.02745098, 0.3921569)
        /// </summary>
        public static readonly Color Darkblue = new Color(0.01176471f, 0.02745098f, 0.3921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 0.2862745, 0.02745098)
        /// </summary>
        public static readonly Color Darkgreen = new Color(0.01960784f, 0.2862745f, 0.02745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.254902, 0.509804)
        /// </summary>
        public static readonly Color DarkishBlue = new Color(0.003921569f, 0.254902f, 0.509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1568628, 0.4862745, 0.2156863)
        /// </summary>
        public static readonly Color DarkishGreen = new Color(0.1568628f, 0.4862745f, 0.2156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.854902, 0.2745098, 0.4901961)
        /// </summary>
        public static readonly Color DarkishPink = new Color(0.854902f, 0.2745098f, 0.4901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.09803922, 0.4509804)
        /// </summary>
        public static readonly Color DarkishPurple = new Color(0.4588235f, 0.09803922f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6627451, 0.01176471, 0.03137255)
        /// </summary>
        public static readonly Color DarkishRed = new Color(0.6627451f, 0.01176471f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03137255, 0.4705882, 0.4980392)
        /// </summary>
        public static readonly Color DeepAqua = new Color(0.03137255f, 0.4705882f, 0.4980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.007843138, 0.4509804)
        /// </summary>
        public static readonly Color DeepBlue = new Color(0.01568628f, 0.007843138f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.254902, 0.007843138, 0)
        /// </summary>
        public static readonly Color DeepBrown = new Color(0.254902f, 0.007843138f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.3490196, 0.05882353)
        /// </summary>
        public static readonly Color DeepGreen = new Color(0.007843138f, 0.3490196f, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5529412, 0.3686275, 0.7176471)
        /// </summary>
        public static readonly Color DeepLavender = new Color(0.5529412f, 0.3686275f, 0.7176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5882353, 0.4313726, 0.7411765)
        /// </summary>
        public static readonly Color DeepLilac = new Color(0.5882353f, 0.4313726f, 0.7411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.627451, 0.007843138, 0.3607843)
        /// </summary>
        public static readonly Color DeepMagenta = new Color(0.627451f, 0.007843138f, 0.3607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8627451, 0.3019608, 0.003921569)
        /// </summary>
        public static readonly Color DeepOrange = new Color(0.8627451f, 0.3019608f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.003921569, 0.3843137)
        /// </summary>
        public static readonly Color DeepPink = new Color(0.7960784f, 0.003921569f, 0.3843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2117647, 0.003921569, 0.2470588)
        /// </summary>
        public static readonly Color DeepPurple = new Color(0.2117647f, 0.003921569f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6039216, 0.007843138, 0)
        /// </summary>
        public static readonly Color DeepRed = new Color(0.6039216f, 0.007843138f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.2784314, 0.4039216)
        /// </summary>
        public static readonly Color DeepRose = new Color(0.7803922f, 0.2784314f, 0.4039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.3294118, 0.509804)
        /// </summary>
        public static readonly Color DeepSeaBlue = new Color(0.003921569f, 0.3294118f, 0.509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.05098039, 0.4588235, 0.972549)
        /// </summary>
        public static readonly Color DeepSkyBlue = new Color(0.05098039f, 0.4588235f, 0.972549f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.3333333, 0.3529412)
        /// </summary>
        public static readonly Color DeepTeal = new Color(0, 0.3333333f, 0.3529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.4509804, 0.454902)
        /// </summary>
        public static readonly Color DeepTurquoise = new Color(0.003921569f, 0.4509804f, 0.454902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2862745, 0.02352941, 0.282353)
        /// </summary>
        public static readonly Color DeepViolet = new Color(0.2862745f, 0.02352941f, 0.282353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2313726, 0.3882353, 0.5490196)
        /// </summary>
        public static readonly Color Denim = new Color(0.2313726f, 0.3882353f, 0.5490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2313726, 0.3568628, 0.572549)
        /// </summary>
        public static readonly Color DenimBlue = new Color(0.2313726f, 0.3568628f, 0.572549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8, 0.6784314, 0.3764706)
        /// </summary>
        public static readonly Color Desert = new Color(0.8f, 0.6784314f, 0.3764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6235294, 0.5137255, 0.01176471)
        /// </summary>
        public static readonly Color Diarrhea = new Color(0.6235294f, 0.5137255f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5411765, 0.4313726, 0.2705882)
        /// </summary>
        public static readonly Color Dirt = new Color(0.5411765f, 0.4313726f, 0.2705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5137255, 0.3960784, 0.2235294)
        /// </summary>
        public static readonly Color DirtBrown = new Color(0.5137255f, 0.3960784f, 0.2235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2470588, 0.509804, 0.6156863)
        /// </summary>
        public static readonly Color DirtyBlue = new Color(0.2470588f, 0.509804f, 0.6156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4, 0.4941176, 0.172549)
        /// </summary>
        public static readonly Color DirtyGreen = new Color(0.4f, 0.4941176f, 0.172549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.4627451, 0.02352941)
        /// </summary>
        public static readonly Color DirtyOrange = new Color(0.7843137f, 0.4627451f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 0.4823529, 0.5019608)
        /// </summary>
        public static readonly Color DirtyPink = new Color(0.7921569f, 0.4823529f, 0.5019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4509804, 0.2901961, 0.3960784)
        /// </summary>
        public static readonly Color DirtyPurple = new Color(0.4509804f, 0.2901961f, 0.3960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8039216, 0.772549, 0.03921569)
        /// </summary>
        public static readonly Color DirtyYellow = new Color(0.8039216f, 0.772549f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2431373, 0.509804, 0.9882353)
        /// </summary>
        public static readonly Color DodgerBlue = new Color(0.2431373f, 0.509804f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.5137255, 0.2666667)
        /// </summary>
        public static readonly Color Drab = new Color(0.509804f, 0.5137255f, 0.2666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.454902, 0.5843138, 0.3176471)
        /// </summary>
        public static readonly Color DrabGreen = new Color(0.454902f, 0.5843138f, 0.3176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2941177, 0.003921569, 0.003921569)
        /// </summary>
        public static readonly Color DriedBlood = new Color(0.2941177f, 0.003921569f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7647059, 0.9843137, 0.9568627)
        /// </summary>
        public static readonly Color DuckEggBlue = new Color(0.7647059f, 0.9843137f, 0.9568627f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2862745, 0.4588235, 0.6117647)
        /// </summary>
        public static readonly Color DullBlue = new Color(0.2862745f, 0.4588235f, 0.6117647f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.4313726, 0.2941177)
        /// </summary>
        public static readonly Color DullBrown = new Color(0.5294118f, 0.4313726f, 0.2941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.454902, 0.6509804, 0.3843137)
        /// </summary>
        public static readonly Color DullGreen = new Color(0.454902f, 0.6509804f, 0.3843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8470588, 0.5254902, 0.2313726)
        /// </summary>
        public static readonly Color DullOrange = new Color(0.8470588f, 0.5254902f, 0.2313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8352941, 0.5254902, 0.6156863)
        /// </summary>
        public static readonly Color DullPink = new Color(0.8352941f, 0.5254902f, 0.6156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5176471, 0.3490196, 0.4941176)
        /// </summary>
        public static readonly Color DullPurple = new Color(0.5176471f, 0.3490196f, 0.4941176f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7333333, 0.2470588, 0.2470588)
        /// </summary>
        public static readonly Color DullRed = new Color(0.7333333f, 0.2470588f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.372549, 0.6196079, 0.5607843)
        /// </summary>
        public static readonly Color DullTeal = new Color(0.372549f, 0.6196079f, 0.5607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9333333, 0.8627451, 0.3568628)
        /// </summary>
        public static readonly Color DullYellow = new Color(0.9333333f, 0.8627451f, 0.3568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3058824, 0.3294118, 0.5058824)
        /// </summary>
        public static readonly Color Dusk = new Color(0.3058824f, 0.3294118f, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1490196, 0.3254902, 0.5529412)
        /// </summary>
        public static readonly Color DuskBlue = new Color(0.1490196f, 0.3254902f, 0.5529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2784314, 0.372549, 0.5803922)
        /// </summary>
        public static readonly Color DuskyBlue = new Color(0.2784314f, 0.372549f, 0.5803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8, 0.4784314, 0.5450981)
        /// </summary>
        public static readonly Color DuskyPink = new Color(0.8f, 0.4784314f, 0.5450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5372549, 0.3568628, 0.4823529)
        /// </summary>
        public static readonly Color DuskyPurple = new Color(0.5372549f, 0.3568628f, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7294118, 0.4078431, 0.4509804)
        /// </summary>
        public static readonly Color DuskyRose = new Color(0.7294118f, 0.4078431f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.6, 0.4313726)
        /// </summary>
        public static readonly Color Dust = new Color(0.6980392f, 0.6f, 0.4313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3529412, 0.5254902, 0.6784314)
        /// </summary>
        public static readonly Color DustyBlue = new Color(0.3529412f, 0.5254902f, 0.6784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4627451, 0.6627451, 0.4509804)
        /// </summary>
        public static readonly Color DustyGreen = new Color(0.4627451f, 0.6627451f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.5254902, 0.6588235)
        /// </summary>
        public static readonly Color DustyLavender = new Color(0.6745098f, 0.5254902f, 0.6588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9411765, 0.5137255, 0.227451)
        /// </summary>
        public static readonly Color DustyOrange = new Color(0.9411765f, 0.5137255f, 0.227451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8352941, 0.5411765, 0.5803922)
        /// </summary>
        public static readonly Color DustyPink = new Color(0.8352941f, 0.5411765f, 0.5803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.372549, 0.5294118)
        /// </summary>
        public static readonly Color DustyPurple = new Color(0.509804f, 0.372549f, 0.5294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7254902, 0.282353, 0.3058824)
        /// </summary>
        public static readonly Color DustyRed = new Color(0.7254902f, 0.282353f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7529412, 0.4509804, 0.4784314)
        /// </summary>
        public static readonly Color DustyRose = new Color(0.7529412f, 0.4509804f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2980392, 0.5647059, 0.5215687)
        /// </summary>
        public static readonly Color DustyTeal = new Color(0.2980392f, 0.5647059f, 0.5215687f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6352941, 0.3960784, 0.2431373)
        /// </summary>
        public static readonly Color Earth = new Color(0.6352941f, 0.3960784f, 0.2431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5490196, 0.9921569, 0.4941176)
        /// </summary>
        public static readonly Color EasterGreen = new Color(0.5490196f, 0.9921569f, 0.4941176f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7529412, 0.4431373, 0.9960784)
        /// </summary>
        public static readonly Color EasterPurple = new Color(0.7529412f, 0.4431373f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 1, 0.7921569)
        /// </summary>
        public static readonly Color Ecru = new Color(0.9960784f, 1, 0.7921569f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9882353, 0.7686275)
        /// </summary>
        public static readonly Color EggShell = new Color(1, 0.9882353f, 0.7686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2196078, 0.03137255, 0.2078431)
        /// </summary>
        public static readonly Color Eggplant = new Color(0.2196078f, 0.03137255f, 0.2078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2627451, 0.01960784, 0.254902)
        /// </summary>
        public static readonly Color EggplantPurple = new Color(0.2627451f, 0.01960784f, 0.254902f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.8313726)
        /// </summary>
        public static readonly Color Eggshell = new Color(1, 1, 0.8313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7686275, 1, 0.9686275)
        /// </summary>
        public static readonly Color EggshellBlue = new Color(0.7686275f, 1, 0.9686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.3215686, 1)
        /// </summary>
        public static readonly Color ElectricBlue = new Color(0.02352941f, 0.3215686f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.1294118, 0.9882353, 0.05098039)
        /// </summary>
        public static readonly Color ElectricGreen = new Color(0.1294118f, 0.9882353f, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 1, 0.01568628)
        /// </summary>
        public static readonly Color ElectricLime = new Color(0.6588235f, 1, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.01568628, 0.5647059)
        /// </summary>
        public static readonly Color ElectricPink = new Color(1, 0.01568628f, 0.5647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6666667, 0.1372549, 1)
        /// </summary>
        public static readonly Color ElectricPurple = new Color(0.6666667f, 0.1372549f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.627451, 0.2862745)
        /// </summary>
        public static readonly Color Emerald = new Color(0.003921569f, 0.627451f, 0.2862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.5607843, 0.1176471)
        /// </summary>
        public static readonly Color EmeraldGreen = new Color(0.007843138f, 0.5607843f, 0.1176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 0.2784314, 0.1647059)
        /// </summary>
        public static readonly Color Evergreen = new Color(0.01960784f, 0.2784314f, 0.1647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.5490196, 0.7333333)
        /// </summary>
        public static readonly Color FadedBlue = new Color(0.3960784f, 0.5490196f, 0.7333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0.6980392, 0.454902)
        /// </summary>
        public static readonly Color FadedGreen = new Color(0.4823529f, 0.6980392f, 0.454902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9411765, 0.5803922, 0.3019608)
        /// </summary>
        public static readonly Color FadedOrange = new Color(0.9411765f, 0.5803922f, 0.3019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8705882, 0.6156863, 0.6745098)
        /// </summary>
        public static readonly Color FadedPink = new Color(0.8705882f, 0.6156863f, 0.6745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5686275, 0.4313726, 0.6)
        /// </summary>
        public static readonly Color FadedPurple = new Color(0.5686275f, 0.4313726f, 0.6f);

        /// <summary>
        /// A formatted XKCD survey colour (0.827451, 0.2862745, 0.3058824)
        /// </summary>
        public static readonly Color FadedRed = new Color(0.827451f, 0.2862745f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 1, 0.4980392)
        /// </summary>
        public static readonly Color FadedYellow = new Color(0.9960784f, 1, 0.4980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8117647, 0.6862745, 0.4823529)
        /// </summary>
        public static readonly Color Fawn = new Color(0.8117647f, 0.6862745f, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3882353, 0.6627451, 0.3137255)
        /// </summary>
        public static readonly Color Fern = new Color(0.3882353f, 0.6627451f, 0.3137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3294118, 0.5529412, 0.2666667)
        /// </summary>
        public static readonly Color FernGreen = new Color(0.3294118f, 0.5529412f, 0.2666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0, 0.007843138)
        /// </summary>
        public static readonly Color FireEngineRed = new Color(0.9960784f, 0, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2352941, 0.4509804, 0.6588235)
        /// </summary>
        public static readonly Color FlatBlue = new Color(0.2352941f, 0.4509804f, 0.6588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4117647, 0.6156863, 0.2980392)
        /// </summary>
        public static readonly Color FlatGreen = new Color(0.4117647f, 0.6156863f, 0.2980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03137255, 1, 0.03137255)
        /// </summary>
        public static readonly Color FluorescentGreen = new Color(0.03137255f, 1, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03921569, 1, 0.007843138)
        /// </summary>
        public static readonly Color FluroGreen = new Color(0.03921569f, 1, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5647059, 0.9921569, 0.6627451)
        /// </summary>
        public static readonly Color FoamGreen = new Color(0.5647059f, 0.9921569f, 0.6627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04313726, 0.3333333, 0.03529412)
        /// </summary>
        public static readonly Color Forest = new Color(0.04313726f, 0.3333333f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.2784314, 0.04705882)
        /// </summary>
        public static readonly Color ForestGreen = new Color(0.02352941f, 0.2784314f, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.08235294, 0.2666667, 0.02352941)
        /// </summary>
        public static readonly Color ForrestGreen = new Color(0.08235294f, 0.2666667f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2627451, 0.4196078, 0.6784314)
        /// </summary>
        public static readonly Color FrenchBlue = new Color(0.2627451f, 0.4196078f, 0.6784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4117647, 0.8470588, 0.3098039)
        /// </summary>
        public static readonly Color FreshGreen = new Color(0.4117647f, 0.8470588f, 0.3098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.345098, 0.7372549, 0.03137255)
        /// </summary>
        public static readonly Color FrogGreen = new Color(0.345098f, 0.7372549f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9294118, 0.05098039, 0.8509804)
        /// </summary>
        public static readonly Color Fuchsia = new Color(0.9294118f, 0.05098039f, 0.8509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8588235, 0.7058824, 0.04705882)
        /// </summary>
        public static readonly Color Gold = new Color(0.8588235f, 0.7058824f, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9607843, 0.7490196, 0.01176471)
        /// </summary>
        public static readonly Color Golden = new Color(0.9607843f, 0.7490196f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.4784314, 0.003921569)
        /// </summary>
        public static readonly Color GoldenBrown = new Color(0.6980392f, 0.4784314f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9764706, 0.7372549, 0.03137255)
        /// </summary>
        public static readonly Color GoldenRod = new Color(0.9764706f, 0.7372549f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.7764706, 0.08235294)
        /// </summary>
        public static readonly Color GoldenYellow = new Color(0.9960784f, 0.7764706f, 0.08235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9803922, 0.7607843, 0.01960784)
        /// </summary>
        public static readonly Color Goldenrod = new Color(0.9803922f, 0.7607843f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4235294, 0.2039216, 0.3803922)
        /// </summary>
        public static readonly Color Grape = new Color(0.4235294f, 0.2039216f, 0.3803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3647059, 0.07843138, 0.3176471)
        /// </summary>
        public static readonly Color GrapePurple = new Color(0.3647059f, 0.07843138f, 0.3176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.3490196, 0.3372549)
        /// </summary>
        public static readonly Color Grapefruit = new Color(0.9921569f, 0.3490196f, 0.3372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3607843, 0.6745098, 0.1764706)
        /// </summary>
        public static readonly Color Grass = new Color(0.3607843f, 0.6745098f, 0.1764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2470588, 0.6078432, 0.04313726)
        /// </summary>
        public static readonly Color GrassGreen = new Color(0.2470588f, 0.6078432f, 0.04313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.254902, 0.6117647, 0.01176471)
        /// </summary>
        public static readonly Color GrassyGreen = new Color(0.254902f, 0.6117647f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.08235294, 0.6901961, 0.1019608)
        /// </summary>
        public static readonly Color Green = new Color(0.08235294f, 0.6901961f, 0.1019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3686275, 0.8627451, 0.1215686)
        /// </summary>
        public static readonly Color GreenApple = new Color(0.3686275f, 0.8627451f, 0.1215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.7058824, 0.5450981)
        /// </summary>
        public static readonly Color GreenBlue = new Color(0.02352941f, 0.7058824f, 0.5450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3294118, 0.3058824, 0.01176471)
        /// </summary>
        public static readonly Color GreenBrown = new Color(0.3294118f, 0.3058824f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4666667, 0.572549, 0.4352941)
        /// </summary>
        public static readonly Color GreenGrey = new Color(0.4666667f, 0.572549f, 0.4352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04705882, 0.7098039, 0.4666667)
        /// </summary>
        public static readonly Color GreenTeal = new Color(0.04705882f, 0.7098039f, 0.4666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 1, 0.1529412)
        /// </summary>
        public static readonly Color GreenYellow = new Color(0.7882353f, 1, 0.1529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.7529412, 0.5529412)
        /// </summary>
        public static readonly Color Green_Blue = new Color(0.003921569f, 0.7529412f, 0.5529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7098039, 0.8078431, 0.03137255)
        /// </summary>
        public static readonly Color Green_Yellow = new Color(0.7098039f, 0.8078431f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1372549, 0.7686275, 0.5450981)
        /// </summary>
        public static readonly Color Greenblue = new Color(0.1372549f, 0.7686275f, 0.5450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2509804, 0.6392157, 0.4078431)
        /// </summary>
        public static readonly Color Greenish = new Color(0.2509804f, 0.6392157f, 0.4078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 0.8196079, 0.4745098)
        /// </summary>
        public static readonly Color GreenishBeige = new Color(0.7882353f, 0.8196079f, 0.4745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04313726, 0.5450981, 0.5294118)
        /// </summary>
        public static readonly Color GreenishBlue = new Color(0.04313726f, 0.5450981f, 0.5294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4117647, 0.3803922, 0.07058824)
        /// </summary>
        public static readonly Color GreenishBrown = new Color(0.4117647f, 0.3803922f, 0.07058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1647059, 0.9960784, 0.7176471)
        /// </summary>
        public static readonly Color GreenishCyan = new Color(0.1647059f, 0.9960784f, 0.7176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5882353, 0.682353, 0.5529412)
        /// </summary>
        public static readonly Color GreenishGrey = new Color(0.5882353f, 0.682353f, 0.5529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7372549, 0.7960784, 0.4784314)
        /// </summary>
        public static readonly Color GreenishTan = new Color(0.7372549f, 0.7960784f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1960784, 0.7490196, 0.5176471)
        /// </summary>
        public static readonly Color GreenishTeal = new Color(0.1960784f, 0.7490196f, 0.5176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.9843137, 0.6901961)
        /// </summary>
        public static readonly Color GreenishTurquoise = new Color(0, 0.9843137f, 0.6901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8039216, 0.9921569, 0.007843138)
        /// </summary>
        public static readonly Color GreenishYellow = new Color(0.8039216f, 0.9921569f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2588235, 0.7019608, 0.5843138)
        /// </summary>
        public static readonly Color GreenyBlue = new Color(0.2588235f, 0.7019608f, 0.5843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4117647, 0.3764706, 0.02352941)
        /// </summary>
        public static readonly Color GreenyBrown = new Color(0.4117647f, 0.3764706f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4941176, 0.627451, 0.4784314)
        /// </summary>
        public static readonly Color GreenyGrey = new Color(0.4941176f, 0.627451f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7764706, 0.972549, 0.03137255)
        /// </summary>
        public static readonly Color GreenyYellow = new Color(0.7764706f, 0.972549f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.572549, 0.5843138, 0.5686275)
        /// </summary>
        public static readonly Color Grey = new Color(0.572549f, 0.5843138f, 0.5686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4196078, 0.5450981, 0.6431373)
        /// </summary>
        public static readonly Color GreyBlue = new Color(0.4196078f, 0.5450981f, 0.6431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.4392157, 0.3254902)
        /// </summary>
        public static readonly Color GreyBrown = new Color(0.4980392f, 0.4392157f, 0.3254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4705882, 0.6078432, 0.4509804)
        /// </summary>
        public static readonly Color GreyGreen = new Color(0.4705882f, 0.6078432f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7647059, 0.5647059, 0.6078432)
        /// </summary>
        public static readonly Color GreyPink = new Color(0.7647059f, 0.5647059f, 0.6078432f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.427451, 0.5490196)
        /// </summary>
        public static readonly Color GreyPurple = new Color(0.509804f, 0.427451f, 0.5490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3686275, 0.6078432, 0.5411765)
        /// </summary>
        public static readonly Color GreyTeal = new Color(0.3686275f, 0.6078432f, 0.5411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3921569, 0.4901961, 0.5568628)
        /// </summary>
        public static readonly Color Grey_Blue = new Color(0.3921569f, 0.4901961f, 0.5568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5254902, 0.6313726, 0.4901961)
        /// </summary>
        public static readonly Color Grey_Green = new Color(0.5254902f, 0.6313726f, 0.4901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4666667, 0.6313726, 0.7098039)
        /// </summary>
        public static readonly Color Greyblue = new Color(0.4666667f, 0.6313726f, 0.7098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.6431373, 0.5843138)
        /// </summary>
        public static readonly Color Greyish = new Color(0.6588235f, 0.6431373f, 0.5843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3686275, 0.5058824, 0.6156863)
        /// </summary>
        public static readonly Color GreyishBlue = new Color(0.3686275f, 0.5058824f, 0.6156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4784314, 0.4156863, 0.3098039)
        /// </summary>
        public static readonly Color GreyishBrown = new Color(0.4784314f, 0.4156863f, 0.3098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.6509804, 0.4901961)
        /// </summary>
        public static readonly Color GreyishGreen = new Color(0.509804f, 0.6509804f, 0.4901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.5529412, 0.5803922)
        /// </summary>
        public static readonly Color GreyishPink = new Color(0.7843137f, 0.5529412f, 0.5803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5333334, 0.4431373, 0.5686275)
        /// </summary>
        public static readonly Color GreyishPurple = new Color(0.5333334f, 0.4431373f, 0.5686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4431373, 0.6235294, 0.5686275)
        /// </summary>
        public static readonly Color GreyishTeal = new Color(0.4431373f, 0.6235294f, 0.5686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.627451, 0.7490196, 0.08627451)
        /// </summary>
        public static readonly Color GrossGreen = new Color(0.627451f, 0.7490196f, 0.08627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3254902, 0.3843137, 0.4039216)
        /// </summary>
        public static readonly Color Gunmetal = new Color(0.3254902f, 0.3843137f, 0.4039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5568628, 0.4627451, 0.09411765)
        /// </summary>
        public static readonly Color Hazel = new Color(0.5568628f, 0.4627451f, 0.09411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6431373, 0.5176471, 0.6745098)
        /// </summary>
        public static readonly Color Heather = new Color(0.6431373f, 0.5176471f, 0.6745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8509804, 0.3098039, 0.9607843)
        /// </summary>
        public static readonly Color Heliotrope = new Color(0.8509804f, 0.3098039f, 0.9607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1058824, 0.9882353, 0.02352941)
        /// </summary>
        public static readonly Color HighlighterGreen = new Color(0.1058824f, 0.9882353f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6078432, 0.8980392, 0.6666667)
        /// </summary>
        public static readonly Color HospitalGreen = new Color(0.6078432f, 0.8980392f, 0.6666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.145098, 1, 0.1607843)
        /// </summary>
        public static readonly Color HotGreen = new Color(0.145098f, 1, 0.1607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9607843, 0.01568628, 0.7882353)
        /// </summary>
        public static readonly Color HotMagenta = new Color(0.9607843f, 0.01568628f, 0.7882353f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.007843138, 0.5529412)
        /// </summary>
        public static readonly Color HotPink = new Color(1, 0.007843138f, 0.5529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0, 0.9607843)
        /// </summary>
        public static readonly Color HotPurple = new Color(0.7960784f, 0, 0.9607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04313726, 0.2509804, 0.03137255)
        /// </summary>
        public static readonly Color HunterGreen = new Color(0.04313726f, 0.2509804f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8392157, 1, 0.9803922)
        /// </summary>
        public static readonly Color Ice = new Color(0.8392157f, 1, 0.9803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8431373, 1, 0.9960784)
        /// </summary>
        public static readonly Color IceBlue = new Color(0.8431373f, 1, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.682353, 0.1333333)
        /// </summary>
        public static readonly Color IckyGreen = new Color(0.5607843f, 0.682353f, 0.1333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5215687, 0.05490196, 0.01568628)
        /// </summary>
        public static readonly Color IndianRed = new Color(0.5215687f, 0.05490196f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2196078, 0.007843138, 0.509804)
        /// </summary>
        public static readonly Color Indigo = new Color(0.2196078f, 0.007843138f, 0.509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.227451, 0.09411765, 0.6941177)
        /// </summary>
        public static readonly Color IndigoBlue = new Color(0.227451f, 0.09411765f, 0.6941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3843137, 0.345098, 0.7686275)
        /// </summary>
        public static readonly Color Iris = new Color(0.3843137f, 0.345098f, 0.7686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.5843138, 0.1607843)
        /// </summary>
        public static readonly Color IrishGreen = new Color(0.003921569f, 0.5843138f, 0.1607843f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.7960784)
        /// </summary>
        public static readonly Color Ivory = new Color(1, 1, 0.7960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1215686, 0.654902, 0.454902)
        /// </summary>
        public static readonly Color Jade = new Color(0.1215686f, 0.654902f, 0.454902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1686275, 0.6862745, 0.4156863)
        /// </summary>
        public static readonly Color JadeGreen = new Color(0.1686275f, 0.6862745f, 0.4156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.509804, 0.2627451)
        /// </summary>
        public static readonly Color JungleGreen = new Color(0.01568628f, 0.509804f, 0.2627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.5764706, 0.2156863)
        /// </summary>
        public static readonly Color KelleyGreen = new Color(0, 0.5764706f, 0.2156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.6705883, 0.1803922)
        /// </summary>
        public static readonly Color KellyGreen = new Color(0.007843138f, 0.6705883f, 0.1803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3607843, 0.6980392, 0)
        /// </summary>
        public static readonly Color KermitGreen = new Color(0.3607843f, 0.6980392f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.682353, 1, 0.4313726)
        /// </summary>
        public static readonly Color KeyLime = new Color(0.682353f, 1, 0.4313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6666667, 0.6509804, 0.3843137)
        /// </summary>
        public static readonly Color Khaki = new Color(0.6666667f, 0.6509804f, 0.3843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4470588, 0.5254902, 0.2235294)
        /// </summary>
        public static readonly Color KhakiGreen = new Color(0.4470588f, 0.5254902f, 0.2235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6117647, 0.9372549, 0.2627451)
        /// </summary>
        public static readonly Color Kiwi = new Color(0.6117647f, 0.9372549f, 0.2627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5568628, 0.8980392, 0.2470588)
        /// </summary>
        public static readonly Color KiwiGreen = new Color(0.5568628f, 0.8980392f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.6235294, 0.9372549)
        /// </summary>
        public static readonly Color Lavender = new Color(0.7803922f, 0.6235294f, 0.9372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5450981, 0.5333334, 0.972549)
        /// </summary>
        public static readonly Color LavenderBlue = new Color(0.5450981f, 0.5333334f, 0.972549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8666667, 0.5215687, 0.8431373)
        /// </summary>
        public static readonly Color LavenderPink = new Color(0.8666667f, 0.5215687f, 0.8431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3019608, 0.6431373, 0.03529412)
        /// </summary>
        public static readonly Color LawnGreen = new Color(0.3019608f, 0.6431373f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4431373, 0.6666667, 0.2039216)
        /// </summary>
        public static readonly Color Leaf = new Color(0.4431373f, 0.6666667f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3607843, 0.6627451, 0.01568628)
        /// </summary>
        public static readonly Color LeafGreen = new Color(0.3607843f, 0.6627451f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3176471, 0.7176471, 0.2313726)
        /// </summary>
        public static readonly Color LeafyGreen = new Color(0.3176471f, 0.7176471f, 0.2313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.454902, 0.2039216)
        /// </summary>
        public static readonly Color Leather = new Color(0.6745098f, 0.454902f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 1, 0.3215686)
        /// </summary>
        public static readonly Color Lemon = new Color(0.9921569f, 1, 0.3215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6784314, 0.972549, 0.007843138)
        /// </summary>
        public static readonly Color LemonGreen = new Color(0.6784314f, 0.972549f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7490196, 0.9960784, 0.1568628)
        /// </summary>
        public static readonly Color LemonLime = new Color(0.7490196f, 0.9960784f, 0.1568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 1, 0.2196078)
        /// </summary>
        public static readonly Color LemonYellow = new Color(0.9921569f, 1, 0.2196078f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.7137255, 0.4823529)
        /// </summary>
        public static readonly Color Lichen = new Color(0.5607843f, 0.7137255f, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5490196, 1, 0.8588235)
        /// </summary>
        public static readonly Color LightAqua = new Color(0.5490196f, 1, 0.8588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0.9921569, 0.7803922)
        /// </summary>
        public static readonly Color LightAquamarine = new Color(0.4823529f, 0.9921569f, 0.7803922f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9960784, 0.7137255)
        /// </summary>
        public static readonly Color LightBeige = new Color(1, 0.9960784f, 0.7137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5843138, 0.8156863, 0.9882353)
        /// </summary>
        public static readonly Color LightBlue = new Color(0.5843138f, 0.8156863f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4941176, 0.9843137, 0.7019608)
        /// </summary>
        public static readonly Color LightBlueGreen = new Color(0.4941176f, 0.9843137f, 0.7019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7176471, 0.7882353, 0.8862745)
        /// </summary>
        public static readonly Color LightBlueGrey = new Color(0.7176471f, 0.7882353f, 0.8862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4627451, 0.9921569, 0.6588235)
        /// </summary>
        public static readonly Color LightBluishGreen = new Color(0.4627451f, 0.9921569f, 0.6588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3254902, 0.9960784, 0.3607843)
        /// </summary>
        public static readonly Color LightBrightGreen = new Color(0.3254902f, 0.9960784f, 0.3607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6784314, 0.5058824, 0.3137255)
        /// </summary>
        public static readonly Color LightBrown = new Color(0.6784314f, 0.5058824f, 0.3137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.254902, 0.3568628)
        /// </summary>
        public static readonly Color LightBurgundy = new Color(0.6588235f, 0.254902f, 0.3568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 1, 0.9882353)
        /// </summary>
        public static readonly Color LightCyan = new Color(0.6745098f, 1, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5372549, 0.2705882, 0.5215687)
        /// </summary>
        public static readonly Color LightEggplant = new Color(0.5372549f, 0.2705882f, 0.5215687f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3098039, 0.5686275, 0.3254902)
        /// </summary>
        public static readonly Color LightForestGreen = new Color(0.3098039f, 0.5686275f, 0.3254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.8627451, 0.3607843)
        /// </summary>
        public static readonly Color LightGold = new Color(0.9921569f, 0.8627451f, 0.3607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6039216, 0.9686275, 0.3921569)
        /// </summary>
        public static readonly Color LightGrassGreen = new Color(0.6039216f, 0.9686275f, 0.3921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5882353, 0.9764706, 0.4823529)
        /// </summary>
        public static readonly Color LightGreen = new Color(0.5882353f, 0.9764706f, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3372549, 0.9882353, 0.6352941)
        /// </summary>
        public static readonly Color LightGreenBlue = new Color(0.3372549f, 0.9882353f, 0.6352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3882353, 0.9686275, 0.7058824)
        /// </summary>
        public static readonly Color LightGreenishBlue = new Color(0.3882353f, 0.9686275f, 0.7058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8470588, 0.8627451, 0.8392157)
        /// </summary>
        public static readonly Color LightGrey = new Color(0.8470588f, 0.8627451f, 0.8392157f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.7372549, 0.8313726)
        /// </summary>
        public static readonly Color LightGreyBlue = new Color(0.6156863f, 0.7372549f, 0.8313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7176471, 0.8823529, 0.6313726)
        /// </summary>
        public static readonly Color LightGreyGreen = new Color(0.7176471f, 0.8823529f, 0.6313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.427451, 0.3529412, 0.8117647)
        /// </summary>
        public static readonly Color LightIndigo = new Color(0.427451f, 0.3529412f, 0.8117647f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9019608, 0.9490196, 0.6352941)
        /// </summary>
        public static readonly Color LightKhaki = new Color(0.9019608f, 0.9490196f, 0.6352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9372549, 0.7529412, 0.9960784)
        /// </summary>
        public static readonly Color LightLavendar = new Color(0.9372549f, 0.7529412f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8745098, 0.772549, 0.9960784)
        /// </summary>
        public static readonly Color LightLavender = new Color(0.8745098f, 0.772549f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 1, 0.9843137)
        /// </summary>
        public static readonly Color LightLightBlue = new Color(0.7921569f, 1, 0.9843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 1, 0.6901961)
        /// </summary>
        public static readonly Color LightLightGreen = new Color(0.7843137f, 1, 0.6901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9294118, 0.7843137, 1)
        /// </summary>
        public static readonly Color LightLilac = new Color(0.9294118f, 0.7843137f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.682353, 0.9921569, 0.4235294)
        /// </summary>
        public static readonly Color LightLime = new Color(0.682353f, 0.9921569f, 0.4235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7254902, 1, 0.4)
        /// </summary>
        public static readonly Color LightLimeGreen = new Color(0.7254902f, 1, 0.4f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9803922, 0.372549, 0.9686275)
        /// </summary>
        public static readonly Color LightMagenta = new Color(0.9803922f, 0.372549f, 0.9686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6352941, 0.282353, 0.3411765)
        /// </summary>
        public static readonly Color LightMaroon = new Color(0.6352941f, 0.282353f, 0.3411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7607843, 0.572549, 0.6313726)
        /// </summary>
        public static readonly Color LightMauve = new Color(0.7607843f, 0.572549f, 0.6313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7137255, 1, 0.7333333)
        /// </summary>
        public static readonly Color LightMint = new Color(0.7137255f, 1, 0.7333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6509804, 0.9843137, 0.6980392)
        /// </summary>
        public static readonly Color LightMintGreen = new Color(0.6509804f, 0.9843137f, 0.6980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6509804, 0.7843137, 0.4588235)
        /// </summary>
        public static readonly Color LightMossGreen = new Color(0.6509804f, 0.7843137f, 0.4588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9686275, 0.8352941, 0.3764706)
        /// </summary>
        public static readonly Color LightMustard = new Color(0.9686275f, 0.8352941f, 0.3764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.08235294, 0.3137255, 0.5176471)
        /// </summary>
        public static readonly Color LightNavy = new Color(0.08235294f, 0.3137255f, 0.5176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1803922, 0.3529412, 0.5333334)
        /// </summary>
        public static readonly Color LightNavyBlue = new Color(0.1803922f, 0.3529412f, 0.5333334f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3058824, 0.9921569, 0.3294118)
        /// </summary>
        public static readonly Color LightNeonGreen = new Color(0.3058824f, 0.9921569f, 0.3294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.7490196, 0.4117647)
        /// </summary>
        public static readonly Color LightOlive = new Color(0.6745098f, 0.7490196f, 0.4117647f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6431373, 0.7450981, 0.3607843)
        /// </summary>
        public static readonly Color LightOliveGreen = new Color(0.6431373f, 0.7450981f, 0.3607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.6666667, 0.282353)
        /// </summary>
        public static readonly Color LightOrange = new Color(0.9921569f, 0.6666667f, 0.282353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.9843137, 0.6470588)
        /// </summary>
        public static readonly Color LightPastelGreen = new Color(0.6980392f, 0.9843137f, 0.6470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7686275, 0.9960784, 0.509804)
        /// </summary>
        public static readonly Color LightPeaGreen = new Color(0.7686275f, 0.9960784f, 0.509804f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.8470588, 0.6941177)
        /// </summary>
        public static readonly Color LightPeach = new Color(1, 0.8470588f, 0.6941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7568628, 0.7764706, 0.9882353)
        /// </summary>
        public static readonly Color LightPeriwinkle = new Color(0.7568628f, 0.7764706f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.8196079, 0.8745098)
        /// </summary>
        public static readonly Color LightPink = new Color(1, 0.8196079f, 0.8745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.3411765, 0.5137255)
        /// </summary>
        public static readonly Color LightPlum = new Color(0.6156863f, 0.3411765f, 0.5137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7490196, 0.4666667, 0.9647059)
        /// </summary>
        public static readonly Color LightPurple = new Color(0.7490196f, 0.4666667f, 0.9647059f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.2784314, 0.2980392)
        /// </summary>
        public static readonly Color LightRed = new Color(1, 0.2784314f, 0.2980392f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.772549, 0.7960784)
        /// </summary>
        public static readonly Color LightRose = new Color(1, 0.772549f, 0.7960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.227451, 0.1803922, 0.9960784)
        /// </summary>
        public static readonly Color LightRoyalBlue = new Color(0.227451f, 0.1803922f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7372549, 0.9254902, 0.6745098)
        /// </summary>
        public static readonly Color LightSage = new Color(0.7372549f, 0.9254902f, 0.6745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.6627451, 0.5764706)
        /// </summary>
        public static readonly Color LightSalmon = new Color(0.9960784f, 0.6627451f, 0.5764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5960785, 0.9647059, 0.6901961)
        /// </summary>
        public static readonly Color LightSeaGreen = new Color(0.5960785f, 0.9647059f, 0.6901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.627451, 0.9960784, 0.7490196)
        /// </summary>
        public static readonly Color LightSeafoam = new Color(0.627451f, 0.9960784f, 0.7490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.654902, 1, 0.7098039)
        /// </summary>
        public static readonly Color LightSeafoamGreen = new Color(0.654902f, 1, 0.7098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7764706, 0.9882353, 1)
        /// </summary>
        public static readonly Color LightSkyBlue = new Color(0.7764706f, 0.9882353f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.9843137, 0.9333333, 0.6745098)
        /// </summary>
        public static readonly Color LightTan = new Color(0.9843137f, 0.9333333f, 0.6745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5647059, 0.8941177, 0.7568628)
        /// </summary>
        public static readonly Color LightTeal = new Color(0.5647059f, 0.8941177f, 0.7568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4941176, 0.9568627, 0.8)
        /// </summary>
        public static readonly Color LightTurquoise = new Color(0.4941176f, 0.9568627f, 0.8f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7019608, 0.4352941, 0.9647059)
        /// </summary>
        public static readonly Color LightUrple = new Color(0.7019608f, 0.4352941f, 0.9647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8392157, 0.7058824, 0.9882353)
        /// </summary>
        public static readonly Color LightViolet = new Color(0.8392157f, 0.7058824f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9960784, 0.4784314)
        /// </summary>
        public static readonly Color LightYellow = new Color(1, 0.9960784f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8, 0.9921569, 0.4980392)
        /// </summary>
        public static readonly Color LightYellowGreen = new Color(0.8f, 0.9921569f, 0.4980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7607843, 1, 0.5372549)
        /// </summary>
        public static readonly Color LightYellowishGreen = new Color(0.7607843f, 1, 0.5372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0.7843137, 0.9647059)
        /// </summary>
        public static readonly Color Lightblue = new Color(0.4823529f, 0.7843137f, 0.9647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.9921569, 0.3882353)
        /// </summary>
        public static readonly Color LighterGreen = new Color(0.4588235f, 0.9921569f, 0.3882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0.3529412, 0.9568627)
        /// </summary>
        public static readonly Color LighterPurple = new Color(0.6470588f, 0.3529412f, 0.9568627f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4627451, 1, 0.4823529)
        /// </summary>
        public static readonly Color Lightgreen = new Color(0.4627451f, 1, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2392157, 0.4784314, 0.9921569)
        /// </summary>
        public static readonly Color LightishBlue = new Color(0.2392157f, 0.4784314f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3803922, 0.8823529, 0.3764706)
        /// </summary>
        public static readonly Color LightishGreen = new Color(0.3803922f, 0.8823529f, 0.3764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0.3215686, 0.9019608)
        /// </summary>
        public static readonly Color LightishPurple = new Color(0.6470588f, 0.3215686f, 0.9019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.1843137, 0.2901961)
        /// </summary>
        public static readonly Color LightishRed = new Color(0.9960784f, 0.1843137f, 0.2901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8078431, 0.6352941, 0.9921569)
        /// </summary>
        public static readonly Color Lilac = new Color(0.8078431f, 0.6352941f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7686275, 0.5568628, 0.9921569)
        /// </summary>
        public static readonly Color Liliac = new Color(0.7686275f, 0.5568628f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6666667, 1, 0.1960784)
        /// </summary>
        public static readonly Color Lime = new Color(0.6666667f, 1, 0.1960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5372549, 0.9960784, 0.01960784)
        /// </summary>
        public static readonly Color LimeGreen = new Color(0.5372549f, 0.9960784f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8156863, 0.9960784, 0.1137255)
        /// </summary>
        public static readonly Color LimeYellow = new Color(0.8156863f, 0.9960784f, 0.1137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8352941, 0.09019608, 0.3058824)
        /// </summary>
        public static readonly Color Lipstick = new Color(0.8352941f, 0.09019608f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7529412, 0.007843138, 0.1843137)
        /// </summary>
        public static readonly Color LipstickRed = new Color(0.7529412f, 0.007843138f, 0.1843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9372549, 0.7058824, 0.2078431)
        /// </summary>
        public static readonly Color MacaroniAndCheese = new Color(0.9372549f, 0.7058824f, 0.2078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7607843, 0, 0.4705882)
        /// </summary>
        public static readonly Color Magenta = new Color(0.7607843f, 0, 0.4705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2901961, 0.003921569, 0)
        /// </summary>
        public static readonly Color Mahogany = new Color(0.2901961f, 0.003921569f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.9568627, 0.8156863, 0.3294118)
        /// </summary>
        public static readonly Color Maize = new Color(0.9568627f, 0.8156863f, 0.3294118f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6509804, 0.1686275)
        /// </summary>
        public static readonly Color Mango = new Color(1, 0.6509804f, 0.1686275f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9803922, 0.5254902)
        /// </summary>
        public static readonly Color Manilla = new Color(1, 0.9803922f, 0.5254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.7529412, 0.02352941)
        /// </summary>
        public static readonly Color Marigold = new Color(0.9882353f, 0.7529412f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.1803922, 0.3764706)
        /// </summary>
        public static readonly Color Marine = new Color(0.01568628f, 0.1803922f, 0.3764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.2196078, 0.4156863)
        /// </summary>
        public static readonly Color MarineBlue = new Color(0.003921569f, 0.2196078f, 0.4156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0, 0.1294118)
        /// </summary>
        public static readonly Color Maroon = new Color(0.3960784f, 0, 0.1294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.682353, 0.4431373, 0.5058824)
        /// </summary>
        public static readonly Color Mauve = new Color(0.682353f, 0.4431373f, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.172549, 0.4352941, 0.7333333)
        /// </summary>
        public static readonly Color MediumBlue = new Color(0.172549f, 0.4352941f, 0.7333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.3176471, 0.07058824)
        /// </summary>
        public static readonly Color MediumBrown = new Color(0.4980392f, 0.3176471f, 0.07058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2235294, 0.6784314, 0.282353)
        /// </summary>
        public static readonly Color MediumGreen = new Color(0.2235294f, 0.6784314f, 0.282353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4901961, 0.4980392, 0.4862745)
        /// </summary>
        public static readonly Color MediumGrey = new Color(0.4901961f, 0.4980392f, 0.4862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9529412, 0.3803922, 0.5882353)
        /// </summary>
        public static readonly Color MediumPink = new Color(0.9529412f, 0.3803922f, 0.5882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6196079, 0.2627451, 0.6352941)
        /// </summary>
        public static readonly Color MediumPurple = new Color(0.6196079f, 0.2627451f, 0.6352941f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4705882, 0.3333333)
        /// </summary>
        public static readonly Color Melon = new Color(1, 0.4705882f, 0.3333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4509804, 0, 0.2235294)
        /// </summary>
        public static readonly Color Merlot = new Color(0.4509804f, 0, 0.2235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3098039, 0.4509804, 0.5568628)
        /// </summary>
        public static readonly Color MetallicBlue = new Color(0.3098039f, 0.4509804f, 0.5568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1529412, 0.4156863, 0.7019608)
        /// </summary>
        public static readonly Color MidBlue = new Color(0.1529412f, 0.4156863f, 0.7019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3137255, 0.654902, 0.2784314)
        /// </summary>
        public static readonly Color MidGreen = new Color(0.3137255f, 0.654902f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.003921569, 0.1764706)
        /// </summary>
        public static readonly Color Midnight = new Color(0.01176471f, 0.003921569f, 0.1764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0, 0.2078431)
        /// </summary>
        public static readonly Color MidnightBlue = new Color(0.007843138f, 0, 0.2078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1568628, 0.003921569, 0.2156863)
        /// </summary>
        public static readonly Color MidnightPurple = new Color(0.1568628f, 0.003921569f, 0.2156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4, 0.4862745, 0.2431373)
        /// </summary>
        public static readonly Color MilitaryGreen = new Color(0.4f, 0.4862745f, 0.2431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.3058824, 0.1176471)
        /// </summary>
        public static readonly Color MilkChocolate = new Color(0.4980392f, 0.3058824f, 0.1176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6235294, 0.9960784, 0.6901961)
        /// </summary>
        public static readonly Color Mint = new Color(0.6235294f, 0.9960784f, 0.6901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 1, 0.6235294)
        /// </summary>
        public static readonly Color MintGreen = new Color(0.5607843f, 1, 0.6235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04313726, 0.9686275, 0.4901961)
        /// </summary>
        public static readonly Color MintyGreen = new Color(0.04313726f, 0.9686275f, 0.4901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.4627451, 0.3176471)
        /// </summary>
        public static readonly Color Mocha = new Color(0.6156863f, 0.4627451f, 0.3176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4627451, 0.6, 0.345098)
        /// </summary>
        public static readonly Color Moss = new Color(0.4627451f, 0.6f, 0.345098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.5450981, 0.2196078)
        /// </summary>
        public static readonly Color MossGreen = new Color(0.3960784f, 0.5450981f, 0.2196078f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3882353, 0.5450981, 0.1529412)
        /// </summary>
        public static readonly Color MossyGreen = new Color(0.3882353f, 0.5450981f, 0.1529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4509804, 0.3607843, 0.07058824)
        /// </summary>
        public static readonly Color Mud = new Color(0.4509804f, 0.3607843f, 0.07058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3764706, 0.2745098, 0.05882353)
        /// </summary>
        public static readonly Color MudBrown = new Color(0.3764706f, 0.2745098f, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3764706, 0.4, 0.007843138)
        /// </summary>
        public static readonly Color MudGreen = new Color(0.3764706f, 0.4f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5333334, 0.4078431, 0.02352941)
        /// </summary>
        public static readonly Color MuddyBrown = new Color(0.5333334f, 0.4078431f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.454902, 0.1960784)
        /// </summary>
        public static readonly Color MuddyGreen = new Color(0.3960784f, 0.454902f, 0.1960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7490196, 0.6745098, 0.01960784)
        /// </summary>
        public static readonly Color MuddyYellow = new Color(0.7490196f, 0.6745098f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.572549, 0.03921569, 0.3058824)
        /// </summary>
        public static readonly Color Mulberry = new Color(0.572549f, 0.03921569f, 0.3058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4235294, 0.4784314, 0.05490196)
        /// </summary>
        public static readonly Color MurkyGreen = new Color(0.4235294f, 0.4784314f, 0.05490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7294118, 0.6196079, 0.5333334)
        /// </summary>
        public static readonly Color Mushroom = new Color(0.7294118f, 0.6196079f, 0.5333334f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8078431, 0.7019608, 0.003921569)
        /// </summary>
        public static readonly Color Mustard = new Color(0.8078431f, 0.7019608f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.4941176, 0.01568628)
        /// </summary>
        public static readonly Color MustardBrown = new Color(0.6745098f, 0.4941176f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.7098039, 0.01568628)
        /// </summary>
        public static readonly Color MustardGreen = new Color(0.6588235f, 0.7098039f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8235294, 0.7411765, 0.03921569)
        /// </summary>
        public static readonly Color MustardYellow = new Color(0.8235294f, 0.7411765f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2313726, 0.4431373, 0.6235294)
        /// </summary>
        public static readonly Color MutedBlue = new Color(0.2313726f, 0.4431373f, 0.6235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.372549, 0.627451, 0.3215686)
        /// </summary>
        public static readonly Color MutedGreen = new Color(0.372549f, 0.627451f, 0.3215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8196079, 0.4627451, 0.5607843)
        /// </summary>
        public static readonly Color MutedPink = new Color(0.8196079f, 0.4627451f, 0.5607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5019608, 0.3568628, 0.5294118)
        /// </summary>
        public static readonly Color MutedPurple = new Color(0.5019608f, 0.3568628f, 0.5294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4392157, 0.6980392, 0.2470588)
        /// </summary>
        public static readonly Color NastyGreen = new Color(0.4392157f, 0.6980392f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.08235294, 0.2431373)
        /// </summary>
        public static readonly Color Navy = new Color(0.003921569f, 0.08235294f, 0.2431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.06666667, 0.2745098)
        /// </summary>
        public static readonly Color NavyBlue = new Color(0, 0.06666667f, 0.2745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2078431, 0.3254902, 0.03921569)
        /// </summary>
        public static readonly Color NavyGreen = new Color(0.2078431f, 0.3254902f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.8509804, 1)
        /// </summary>
        public static readonly Color NeonBlue = new Color(0.01568628f, 0.8509804f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.04705882, 1, 0.04705882)
        /// </summary>
        public static readonly Color NeonGreen = new Color(0.04705882f, 1, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.003921569, 0.6039216)
        /// </summary>
        public static readonly Color NeonPink = new Color(0.9960784f, 0.003921569f, 0.6039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7372549, 0.07450981, 0.9960784)
        /// </summary>
        public static readonly Color NeonPurple = new Color(0.7372549f, 0.07450981f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.02745098, 0.227451)
        /// </summary>
        public static readonly Color NeonRed = new Color(1, 0.02745098f, 0.227451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8117647, 1, 0.01568628)
        /// </summary>
        public static readonly Color NeonYellow = new Color(0.8117647f, 1, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.0627451, 0.4784314, 0.6901961)
        /// </summary>
        public static readonly Color NiceBlue = new Color(0.0627451f, 0.4784314f, 0.6901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.01176471, 0.282353)
        /// </summary>
        public static readonly Color NightBlue = new Color(0.01568628f, 0.01176471f, 0.282353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.4823529, 0.572549)
        /// </summary>
        public static readonly Color Ocean = new Color(0.003921569f, 0.4823529f, 0.572549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.4431373, 0.6117647)
        /// </summary>
        public static readonly Color OceanBlue = new Color(0.01176471f, 0.4431373f, 0.6117647f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2392157, 0.6, 0.4509804)
        /// </summary>
        public static readonly Color OceanGreen = new Color(0.2392157f, 0.6f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7490196, 0.6078432, 0.04705882)
        /// </summary>
        public static readonly Color Ocher = new Color(0.7490196f, 0.6078432f, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7490196, 0.5647059, 0.01960784)
        /// </summary>
        public static readonly Color Ochre = new Color(0.7490196f, 0.5647059f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7764706, 0.6117647, 0.01568628)
        /// </summary>
        public static readonly Color Ocre = new Color(0.7764706f, 0.6117647f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3372549, 0.5176471, 0.682353)
        /// </summary>
        public static readonly Color OffBlue = new Color(0.3372549f, 0.5176471f, 0.682353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4196078, 0.6392157, 0.3254902)
        /// </summary>
        public static readonly Color OffGreen = new Color(0.4196078f, 0.6392157f, 0.3254902f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.8941177)
        /// </summary>
        public static readonly Color OffWhite = new Color(1, 1, 0.8941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.945098, 0.9529412, 0.2470588)
        /// </summary>
        public static readonly Color OffYellow = new Color(0.945098f, 0.9529412f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.4745098, 0.5254902)
        /// </summary>
        public static readonly Color OldPink = new Color(0.7803922f, 0.4745098f, 0.5254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.4980392, 0.5372549)
        /// </summary>
        public static readonly Color OldRose = new Color(0.7843137f, 0.4980392f, 0.5372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4313726, 0.4588235, 0.05490196)
        /// </summary>
        public static readonly Color Olive = new Color(0.4313726f, 0.4588235f, 0.05490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3921569, 0.3294118, 0.01176471)
        /// </summary>
        public static readonly Color OliveBrown = new Color(0.3921569f, 0.3294118f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4352941, 0.4627451, 0.1960784)
        /// </summary>
        public static readonly Color OliveDrab = new Color(0.4352941f, 0.4627451f, 0.1960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4039216, 0.4784314, 0.01568628)
        /// </summary>
        public static readonly Color OliveGreen = new Color(0.4039216f, 0.4784314f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7607843, 0.7176471, 0.03529412)
        /// </summary>
        public static readonly Color OliveYellow = new Color(0.7607843f, 0.7176471f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9764706, 0.4509804, 0.02352941)
        /// </summary>
        public static readonly Color Orange = new Color(0.9764706f, 0.4509804f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.3921569, 0)
        /// </summary>
        public static readonly Color OrangeBrown = new Color(0.7450981f, 0.3921569f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4352941, 0.3215686)
        /// </summary>
        public static readonly Color OrangePink = new Color(1, 0.4352941f, 0.3215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.254902, 0.1176471)
        /// </summary>
        public static readonly Color OrangeRed = new Color(0.9921569f, 0.254902f, 0.1176471f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6784314, 0.003921569)
        /// </summary>
        public static readonly Color OrangeYellow = new Color(1, 0.6784314f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.5529412, 0.2862745)
        /// </summary>
        public static readonly Color Orangeish = new Color(0.9921569f, 0.5529412f, 0.2862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.2588235, 0.05882353)
        /// </summary>
        public static readonly Color Orangered = new Color(0.9960784f, 0.2588235f, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 0.3764706, 0.007843138)
        /// </summary>
        public static readonly Color OrangeyBrown = new Color(0.6941177f, 0.3764706f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9803922, 0.2588235, 0.1411765)
        /// </summary>
        public static readonly Color OrangeyRed = new Color(0.9803922f, 0.2588235f, 0.1411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.7254902, 0.08235294)
        /// </summary>
        public static readonly Color OrangeyYellow = new Color(0.9921569f, 0.7254902f, 0.08235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.509804, 0.2901961)
        /// </summary>
        public static readonly Color Orangish = new Color(0.9882353f, 0.509804f, 0.2901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.372549, 0.01176471)
        /// </summary>
        public static readonly Color OrangishBrown = new Color(0.6980392f, 0.372549f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9568627, 0.2117647, 0.01960784)
        /// </summary>
        public static readonly Color OrangishRed = new Color(0.9568627f, 0.2117647f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.4588235, 0.7686275)
        /// </summary>
        public static readonly Color Orchid = new Color(0.7843137f, 0.4588235f, 0.7686275f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9764706, 0.8156863)
        /// </summary>
        public static readonly Color Pale = new Color(1, 0.9764706f, 0.8156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7215686, 1, 0.9215686)
        /// </summary>
        public static readonly Color PaleAqua = new Color(0.7215686f, 1, 0.9215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8156863, 0.9960784, 0.9960784)
        /// </summary>
        public static readonly Color PaleBlue = new Color(0.8156863f, 0.9960784f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 0.5686275, 0.4313726)
        /// </summary>
        public static readonly Color PaleBrown = new Color(0.6941177f, 0.5686275f, 0.4313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7176471, 1, 0.9803922)
        /// </summary>
        public static readonly Color PaleCyan = new Color(0.7176471f, 1, 0.9803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.8705882, 0.4235294)
        /// </summary>
        public static readonly Color PaleGold = new Color(0.9921569f, 0.8705882f, 0.4235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.9921569, 0.7098039)
        /// </summary>
        public static readonly Color PaleGreen = new Color(0.7803922f, 0.9921569f, 0.7098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.9921569, 0.9960784)
        /// </summary>
        public static readonly Color PaleGrey = new Color(0.9921569f, 0.9921569f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9333333, 0.8117647, 0.9960784)
        /// </summary>
        public static readonly Color PaleLavender = new Color(0.9333333f, 0.8117647f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 0.9882353, 0.6)
        /// </summary>
        public static readonly Color PaleLightGreen = new Color(0.6941177f, 0.9882353f, 0.6f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8941177, 0.7960784, 1)
        /// </summary>
        public static readonly Color PaleLilac = new Color(0.8941177f, 0.7960784f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.9921569, 0.4509804)
        /// </summary>
        public static readonly Color PaleLime = new Color(0.7450981f, 0.9921569f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 1, 0.3960784)
        /// </summary>
        public static readonly Color PaleLimeGreen = new Color(0.6941177f, 1, 0.3960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8431373, 0.4039216, 0.6784314)
        /// </summary>
        public static readonly Color PaleMagenta = new Color(0.8431373f, 0.4039216f, 0.6784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.8156863, 0.9882353)
        /// </summary>
        public static readonly Color PaleMauve = new Color(0.9960784f, 0.8156863f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7254902, 0.8, 0.5058824)
        /// </summary>
        public static readonly Color PaleOlive = new Color(0.7254902f, 0.8f, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 0.8235294, 0.4823529)
        /// </summary>
        public static readonly Color PaleOliveGreen = new Color(0.6941177f, 0.8235294f, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.654902, 0.3372549)
        /// </summary>
        public static readonly Color PaleOrange = new Color(1, 0.654902f, 0.3372549f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.8980392, 0.6784314)
        /// </summary>
        public static readonly Color PalePeach = new Color(1, 0.8980392f, 0.6784314f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.8117647, 0.8627451)
        /// </summary>
        public static readonly Color PalePink = new Color(1, 0.8117647f, 0.8627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7176471, 0.5647059, 0.8313726)
        /// </summary>
        public static readonly Color PalePurple = new Color(0.7176471f, 0.5647059f, 0.8313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8509804, 0.3294118, 0.3019608)
        /// </summary>
        public static readonly Color PaleRed = new Color(0.8509804f, 0.3294118f, 0.3019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.7568628, 0.772549)
        /// </summary>
        public static readonly Color PaleRose = new Color(0.9921569f, 0.7568628f, 0.772549f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6941177, 0.6039216)
        /// </summary>
        public static readonly Color PaleSalmon = new Color(1, 0.6941177f, 0.6039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7411765, 0.9647059, 0.9960784)
        /// </summary>
        public static readonly Color PaleSkyBlue = new Color(0.7411765f, 0.9647059f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.7960784, 0.6980392)
        /// </summary>
        public static readonly Color PaleTeal = new Color(0.509804f, 0.7960784f, 0.6980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0.9843137, 0.8352941)
        /// </summary>
        public static readonly Color PaleTurquoise = new Color(0.6470588f, 0.9843137f, 0.8352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8078431, 0.682353, 0.9803922)
        /// </summary>
        public static readonly Color PaleViolet = new Color(0.8078431f, 0.682353f, 0.9803922f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.5176471)
        /// </summary>
        public static readonly Color PaleYellow = new Color(1, 1, 0.5176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.9882353, 0.6862745)
        /// </summary>
        public static readonly Color Parchment = new Color(0.9960784f, 0.9882353f, 0.6862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6352941, 0.7490196, 0.9960784)
        /// </summary>
        public static readonly Color PastelBlue = new Color(0.6352941f, 0.7490196f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6901961, 1, 0.6156863)
        /// </summary>
        public static readonly Color PastelGreen = new Color(0.6901961f, 1, 0.6156863f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.5882353, 0.3098039)
        /// </summary>
        public static readonly Color PastelOrange = new Color(1, 0.5882353f, 0.3098039f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.7294118, 0.8039216)
        /// </summary>
        public static readonly Color PastelPink = new Color(1, 0.7294118f, 0.8039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 0.627451, 1)
        /// </summary>
        public static readonly Color PastelPurple = new Color(0.7921569f, 0.627451f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.8588235, 0.345098, 0.3372549)
        /// </summary>
        public static readonly Color PastelRed = new Color(0.8588235f, 0.345098f, 0.3372549f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9960784, 0.4431373)
        /// </summary>
        public static readonly Color PastelYellow = new Color(1, 0.9960784f, 0.4431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6431373, 0.7490196, 0.1254902)
        /// </summary>
        public static readonly Color Pea = new Color(0.6431373f, 0.7490196f, 0.1254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5568628, 0.6705883, 0.07058824)
        /// </summary>
        public static readonly Color PeaGreen = new Color(0.5568628f, 0.6705883f, 0.07058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.572549, 0.6, 0.003921569)
        /// </summary>
        public static readonly Color PeaSoup = new Color(0.572549f, 0.6f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5803922, 0.6509804, 0.09019608)
        /// </summary>
        public static readonly Color PeaSoupGreen = new Color(0.5803922f, 0.6509804f, 0.09019608f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6901961, 0.4862745)
        /// </summary>
        public static readonly Color Peach = new Color(1, 0.6901961f, 0.4862745f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6039216, 0.5411765)
        /// </summary>
        public static readonly Color PeachyPink = new Color(1, 0.6039216f, 0.5411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.4039216, 0.5843138)
        /// </summary>
        public static readonly Color PeacockBlue = new Color(0.003921569f, 0.4039216f, 0.5843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.972549, 0.372549)
        /// </summary>
        public static readonly Color Pear = new Color(0.7960784f, 0.972549f, 0.372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5568628, 0.509804, 0.9960784)
        /// </summary>
        public static readonly Color Periwinkle = new Color(0.5568628f, 0.509804f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.6, 0.9843137)
        /// </summary>
        public static readonly Color PeriwinkleBlue = new Color(0.5607843f, 0.6f, 0.9843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.5490196, 0.9058824)
        /// </summary>
        public static readonly Color Perrywinkle = new Color(0.5607843f, 0.5490196f, 0.9058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.372549, 0.4156863)
        /// </summary>
        public static readonly Color Petrol = new Color(0, 0.372549f, 0.4156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9058824, 0.5568628, 0.6470588)
        /// </summary>
        public static readonly Color PigPink = new Color(0.9058824f, 0.5568628f, 0.6470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1686275, 0.3647059, 0.2039216)
        /// </summary>
        public static readonly Color Pine = new Color(0.1686275f, 0.3647059f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03921569, 0.282353, 0.1176471)
        /// </summary>
        public static readonly Color PineGreen = new Color(0.03921569f, 0.282353f, 0.1176471f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.5058824, 0.7529412)
        /// </summary>
        public static readonly Color Pink = new Color(1, 0.5058824f, 0.7529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8588235, 0.2941177, 0.854902)
        /// </summary>
        public static readonly Color PinkPurple = new Color(0.8588235f, 0.2941177f, 0.854902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9607843, 0.01960784, 0.3098039)
        /// </summary>
        public static readonly Color PinkRed = new Color(0.9607843f, 0.01960784f, 0.3098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9372549, 0.1137255, 0.9058824)
        /// </summary>
        public static readonly Color Pink_Purple = new Color(0.9372549f, 0.1137255f, 0.9058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8313726, 0.4156863, 0.4941176)
        /// </summary>
        public static readonly Color Pinkish = new Color(0.8313726f, 0.4156863f, 0.4941176f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 0.4470588, 0.3803922)
        /// </summary>
        public static readonly Color PinkishBrown = new Color(0.6941177f, 0.4470588f, 0.3803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.6745098, 0.6627451)
        /// </summary>
        public static readonly Color PinkishGrey = new Color(0.7843137f, 0.6745098f, 0.6627451f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4470588, 0.2980392)
        /// </summary>
        public static readonly Color PinkishOrange = new Color(1, 0.4470588f, 0.2980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8392157, 0.282353, 0.8431373)
        /// </summary>
        public static readonly Color PinkishPurple = new Color(0.8392157f, 0.282353f, 0.8431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.945098, 0.04705882, 0.2705882)
        /// </summary>
        public static readonly Color PinkishRed = new Color(0.945098f, 0.04705882f, 0.2705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8509804, 0.6078432, 0.509804)
        /// </summary>
        public static readonly Color PinkishTan = new Color(0.8509804f, 0.6078432f, 0.509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.5254902, 0.6666667)
        /// </summary>
        public static readonly Color Pinky = new Color(0.9882353f, 0.5254902f, 0.6666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 0.2980392, 0.7450981)
        /// </summary>
        public static readonly Color PinkyPurple = new Color(0.7882353f, 0.2980392f, 0.7450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.1490196, 0.2784314)
        /// </summary>
        public static readonly Color PinkyRed = new Color(0.9882353f, 0.1490196f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8666667, 0.8392157, 0.09411765)
        /// </summary>
        public static readonly Color PissYellow = new Color(0.8666667f, 0.8392157f, 0.09411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7529412, 0.9803922, 0.5450981)
        /// </summary>
        public static readonly Color Pistachio = new Color(0.7529412f, 0.9803922f, 0.5450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.345098, 0.05882353, 0.254902)
        /// </summary>
        public static readonly Color Plum = new Color(0.345098f, 0.05882353f, 0.254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3058824, 0.01960784, 0.3137255)
        /// </summary>
        public static readonly Color PlumPurple = new Color(0.3058824f, 0.01960784f, 0.3137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2509804, 0.9921569, 0.07843138)
        /// </summary>
        public static readonly Color PoisonGreen = new Color(0.2509804f, 0.9921569f, 0.07843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5607843, 0.4509804, 0.01176471)
        /// </summary>
        public static readonly Color Poo = new Color(0.5607843f, 0.4509804f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5333334, 0.372549, 0.003921569)
        /// </summary>
        public static readonly Color PooBrown = new Color(0.5333334f, 0.372549f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.3686275, 0)
        /// </summary>
        public static readonly Color Poop = new Color(0.4980392f, 0.3686275f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.4784314, 0.3490196, 0.003921569)
        /// </summary>
        public static readonly Color PoopBrown = new Color(0.4784314f, 0.3490196f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4352941, 0.4862745, 0)
        /// </summary>
        public static readonly Color PoopGreen = new Color(0.4352941f, 0.4862745f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.6941177, 0.8196079, 0.9882353)
        /// </summary>
        public static readonly Color PowderBlue = new Color(0.6941177f, 0.8196079f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6980392, 0.8156863)
        /// </summary>
        public static readonly Color PowderPink = new Color(1, 0.6980392f, 0.8156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03137255, 0.01568628, 0.9764706)
        /// </summary>
        public static readonly Color PrimaryBlue = new Color(0.03137255f, 0.01568628f, 0.9764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.2705882, 0.4666667)
        /// </summary>
        public static readonly Color PrussianBlue = new Color(0, 0.2705882f, 0.4666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0.4941176, 0.3215686)
        /// </summary>
        public static readonly Color Puce = new Color(0.6470588f, 0.4941176f, 0.3215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0.6470588, 0.007843138)
        /// </summary>
        public static readonly Color Puke = new Color(0.6470588f, 0.6470588f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5803922, 0.4666667, 0.02352941)
        /// </summary>
        public static readonly Color PukeBrown = new Color(0.5803922f, 0.4666667f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6039216, 0.682353, 0.02745098)
        /// </summary>
        public static readonly Color PukeGreen = new Color(0.6039216f, 0.682353f, 0.02745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7607843, 0.7450981, 0.05490196)
        /// </summary>
        public static readonly Color PukeYellow = new Color(0.7607843f, 0.7450981f, 0.05490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8823529, 0.4666667, 0.003921569)
        /// </summary>
        public static readonly Color Pumpkin = new Color(0.8823529f, 0.4666667f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9843137, 0.4901961, 0.02745098)
        /// </summary>
        public static readonly Color PumpkinOrange = new Color(0.9843137f, 0.4901961f, 0.02745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.01176471, 0.8862745)
        /// </summary>
        public static readonly Color PureBlue = new Color(0.007843138f, 0.01176471f, 0.8862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4941176, 0.1176471, 0.6117647)
        /// </summary>
        public static readonly Color Purple = new Color(0.4941176f, 0.1176471f, 0.6117647f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3882353, 0.1764706, 0.9137255)
        /// </summary>
        public static readonly Color PurpleBlue = new Color(0.3882353f, 0.1764706f, 0.9137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4039216, 0.227451, 0.2470588)
        /// </summary>
        public static readonly Color PurpleBrown = new Color(0.4039216f, 0.227451f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5254902, 0.4352941, 0.5215687)
        /// </summary>
        public static readonly Color PurpleGrey = new Color(0.5254902f, 0.4352941f, 0.5215687f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8784314, 0.2470588, 0.8470588)
        /// </summary>
        public static readonly Color PurplePink = new Color(0.8784314f, 0.2470588f, 0.8470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6, 0.003921569, 0.2784314)
        /// </summary>
        public static readonly Color PurpleRed = new Color(0.6f, 0.003921569f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3647059, 0.1294118, 0.8156863)
        /// </summary>
        public static readonly Color Purple_Blue = new Color(0.3647059f, 0.1294118f, 0.8156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8431373, 0.145098, 0.8705882)
        /// </summary>
        public static readonly Color Purple_Pink = new Color(0.8431373f, 0.145098f, 0.8705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5960785, 0.3372549, 0.5529412)
        /// </summary>
        public static readonly Color Purpleish = new Color(0.5960785f, 0.3372549f, 0.5529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3803922, 0.2509804, 0.9372549)
        /// </summary>
        public static readonly Color PurpleishBlue = new Color(0.3803922f, 0.2509804f, 0.9372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8745098, 0.3058824, 0.7843137)
        /// </summary>
        public static readonly Color PurpleishPink = new Color(0.8745098f, 0.3058824f, 0.7843137f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.3372549, 0.8941177)
        /// </summary>
        public static readonly Color Purpley = new Color(0.5294118f, 0.3372549f, 0.8941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.372549, 0.2039216, 0.9058824)
        /// </summary>
        public static readonly Color PurpleyBlue = new Color(0.372549f, 0.2039216f, 0.9058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5803922, 0.4941176, 0.5803922)
        /// </summary>
        public static readonly Color PurpleyGrey = new Color(0.5803922f, 0.4941176f, 0.5803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.2352941, 0.7254902)
        /// </summary>
        public static readonly Color PurpleyPink = new Color(0.7843137f, 0.2352941f, 0.7254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5803922, 0.3372549, 0.5490196)
        /// </summary>
        public static readonly Color Purplish = new Color(0.5803922f, 0.3372549f, 0.5490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3764706, 0.1176471, 0.9764706)
        /// </summary>
        public static readonly Color PurplishBlue = new Color(0.3764706f, 0.1176471f, 0.9764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4196078, 0.2588235, 0.2784314)
        /// </summary>
        public static readonly Color PurplishBrown = new Color(0.4196078f, 0.2588235f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4784314, 0.4078431, 0.4980392)
        /// </summary>
        public static readonly Color PurplishGrey = new Color(0.4784314f, 0.4078431f, 0.4980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8078431, 0.3647059, 0.682353)
        /// </summary>
        public static readonly Color PurplishPink = new Color(0.8078431f, 0.3647059f, 0.682353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6901961, 0.01960784, 0.2941177)
        /// </summary>
        public static readonly Color PurplishRed = new Color(0.6901961f, 0.01960784f, 0.2941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5960785, 0.2470588, 0.6980392)
        /// </summary>
        public static readonly Color Purply = new Color(0.5960785f, 0.2470588f, 0.6980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4, 0.1019608, 0.9333333)
        /// </summary>
        public static readonly Color PurplyBlue = new Color(0.4f, 0.1019608f, 0.9333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9411765, 0.4588235, 0.9019608)
        /// </summary>
        public static readonly Color PurplyPink = new Color(0.9411765f, 0.4588235f, 0.9019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.682353, 0.5411765)
        /// </summary>
        public static readonly Color Putty = new Color(0.7450981f, 0.682353f, 0.5411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.2745098, 0)
        /// </summary>
        public static readonly Color RacingGreen = new Color(0.003921569f, 0.2745098f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.172549, 0.9803922, 0.1215686)
        /// </summary>
        public static readonly Color RadioactiveGreen = new Color(0.172549f, 0.9803922f, 0.1215686f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6901961, 0.003921569, 0.2862745)
        /// </summary>
        public static readonly Color Raspberry = new Color(0.6901961f, 0.003921569f, 0.2862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6039216, 0.3843137, 0)
        /// </summary>
        public static readonly Color RawSienna = new Color(0.6039216f, 0.3843137f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.654902, 0.3686275, 0.03529412)
        /// </summary>
        public static readonly Color RawUmber = new Color(0.654902f, 0.3686275f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8313726, 1, 1)
        /// </summary>
        public static readonly Color ReallyLightBlue = new Color(0.8313726f, 1, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.8980392, 0, 0)
        /// </summary>
        public static readonly Color Red = new Color(0.8980392f, 0, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.5450981, 0.1803922, 0.08627451)
        /// </summary>
        public static readonly Color RedBrown = new Color(0.5450981f, 0.1803922f, 0.08627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.2352941, 0.02352941)
        /// </summary>
        public static readonly Color RedOrange = new Color(0.9921569f, 0.2352941f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9803922, 0.1647059, 0.3333333)
        /// </summary>
        public static readonly Color RedPink = new Color(0.9803922f, 0.1647059f, 0.3333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.02745098, 0.2784314)
        /// </summary>
        public static readonly Color RedPurple = new Color(0.509804f, 0.02745098f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6196079, 0.003921569, 0.4078431)
        /// </summary>
        public static readonly Color RedViolet = new Color(0.6196079f, 0.003921569f, 0.4078431f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5490196, 0, 0.2039216)
        /// </summary>
        public static readonly Color RedWine = new Color(0.5490196f, 0, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7686275, 0.2588235, 0.2509804)
        /// </summary>
        public static readonly Color Reddish = new Color(0.7686275f, 0.2588235f, 0.2509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.1686275, 0.03921569)
        /// </summary>
        public static readonly Color ReddishBrown = new Color(0.4980392f, 0.1686275f, 0.03921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6, 0.4588235, 0.4392157)
        /// </summary>
        public static readonly Color ReddishGrey = new Color(0.6f, 0.4588235f, 0.4392157f);

        /// <summary>
        /// A formatted XKCD survey colour (0.972549, 0.282353, 0.1098039)
        /// </summary>
        public static readonly Color ReddishOrange = new Color(0.972549f, 0.282353f, 0.1098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.172549, 0.3294118)
        /// </summary>
        public static readonly Color ReddishPink = new Color(0.9960784f, 0.172549f, 0.3294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5686275, 0.03529412, 0.3176471)
        /// </summary>
        public static readonly Color ReddishPurple = new Color(0.5686275f, 0.03529412f, 0.3176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4313726, 0.0627451, 0.01960784)
        /// </summary>
        public static readonly Color ReddyBrown = new Color(0.4313726f, 0.0627451f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.1058824, 0.9764706)
        /// </summary>
        public static readonly Color RichBlue = new Color(0.007843138f, 0.1058824f, 0.9764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4470588, 0, 0.345098)
        /// </summary>
        public static readonly Color RichPurple = new Color(0.4470588f, 0, 0.345098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5411765, 0.945098, 0.9960784)
        /// </summary>
        public static readonly Color RobinEggBlue = new Color(0.5411765f, 0.945098f, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.427451, 0.9294118, 0.9921569)
        /// </summary>
        public static readonly Color RobinSEgg = new Color(0.427451f, 0.9294118f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5960785, 0.9372549, 0.9764706)
        /// </summary>
        public static readonly Color RobinSEggBlue = new Color(0.5960785f, 0.9372549f, 0.9764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.5254902, 0.6431373)
        /// </summary>
        public static readonly Color Rosa = new Color(0.9960784f, 0.5254902f, 0.6431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8117647, 0.3843137, 0.4588235)
        /// </summary>
        public static readonly Color Rose = new Color(0.8117647f, 0.3843137f, 0.4588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9686275, 0.5294118, 0.6039216)
        /// </summary>
        public static readonly Color RosePink = new Color(0.9686275f, 0.5294118f, 0.6039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.003921569, 0.2352941)
        /// </summary>
        public static readonly Color RoseRed = new Color(0.7450981f, 0.003921569f, 0.2352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9647059, 0.4078431, 0.5568628)
        /// </summary>
        public static readonly Color RosyPink = new Color(0.9647059f, 0.4078431f, 0.5568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6705883, 0.07058824, 0.2235294)
        /// </summary>
        public static readonly Color Rouge = new Color(0.6705883f, 0.07058824f, 0.2235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04705882, 0.09019608, 0.5764706)
        /// </summary>
        public static readonly Color Royal = new Color(0.04705882f, 0.09019608f, 0.5764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01960784, 0.01568628, 0.6666667)
        /// </summary>
        public static readonly Color RoyalBlue = new Color(0.01960784f, 0.01568628f, 0.6666667f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2941177, 0, 0.4313726)
        /// </summary>
        public static readonly Color RoyalPurple = new Color(0.2941177f, 0, 0.4313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 0.003921569, 0.2784314)
        /// </summary>
        public static readonly Color Ruby = new Color(0.7921569f, 0.003921569f, 0.2784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6313726, 0.2235294, 0.01960784)
        /// </summary>
        public static readonly Color Russet = new Color(0.6313726f, 0.2235294f, 0.01960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.2352941, 0.03529412)
        /// </summary>
        public static readonly Color Rust = new Color(0.6588235f, 0.2352941f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5450981, 0.1921569, 0.01176471)
        /// </summary>
        public static readonly Color RustBrown = new Color(0.5450981f, 0.1921569f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7686275, 0.3333333, 0.03137255)
        /// </summary>
        public static readonly Color RustOrange = new Color(0.7686275f, 0.3333333f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6666667, 0.1529412, 0.01568628)
        /// </summary>
        public static readonly Color RustRed = new Color(0.6666667f, 0.1529412f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8039216, 0.3490196, 0.03529412)
        /// </summary>
        public static readonly Color RustyOrange = new Color(0.8039216f, 0.3490196f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6862745, 0.1843137, 0.05098039)
        /// </summary>
        public static readonly Color RustyRed = new Color(0.6862745f, 0.1843137f, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.6980392, 0.03529412)
        /// </summary>
        public static readonly Color Saffron = new Color(0.9960784f, 0.6980392f, 0.03529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5294118, 0.682353, 0.4509804)
        /// </summary>
        public static readonly Color Sage = new Color(0.5294118f, 0.682353f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5333334, 0.7019608, 0.4705882)
        /// </summary>
        public static readonly Color SageGreen = new Color(0.5333334f, 0.7019608f, 0.4705882f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.4745098, 0.4235294)
        /// </summary>
        public static readonly Color Salmon = new Color(1, 0.4745098f, 0.4235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.4823529, 0.4862745)
        /// </summary>
        public static readonly Color SalmonPink = new Color(0.9960784f, 0.4823529f, 0.4862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8862745, 0.7921569, 0.4627451)
        /// </summary>
        public static readonly Color Sand = new Color(0.8862745f, 0.7921569f, 0.4627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.6470588, 0.3764706)
        /// </summary>
        public static readonly Color SandBrown = new Color(0.7960784f, 0.6470588f, 0.3764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.8823529, 0.4)
        /// </summary>
        public static readonly Color SandYellow = new Color(0.9882353f, 0.8823529f, 0.4f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 0.682353, 0.454902)
        /// </summary>
        public static readonly Color Sandstone = new Color(0.7882353f, 0.682353f, 0.454902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.945098, 0.854902, 0.4784314)
        /// </summary>
        public static readonly Color Sandy = new Color(0.945098f, 0.854902f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7686275, 0.6509804, 0.3803922)
        /// </summary>
        public static readonly Color SandyBrown = new Color(0.7686275f, 0.6509804f, 0.3803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.9333333, 0.4509804)
        /// </summary>
        public static readonly Color SandyYellow = new Color(0.9921569f, 0.9333333f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3607843, 0.5450981, 0.08235294)
        /// </summary>
        public static readonly Color SapGreen = new Color(0.3607843f, 0.5450981f, 0.08235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1294118, 0.2196078, 0.6705883)
        /// </summary>
        public static readonly Color Sapphire = new Color(0.1294118f, 0.2196078f, 0.6705883f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7450981, 0.003921569, 0.09803922)
        /// </summary>
        public static readonly Color Scarlet = new Color(0.7450981f, 0.003921569f, 0.09803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2352941, 0.6, 0.572549)
        /// </summary>
        public static readonly Color Sea = new Color(0.2352941f, 0.6f, 0.572549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.454902, 0.5843138)
        /// </summary>
        public static readonly Color SeaBlue = new Color(0.01568628f, 0.454902f, 0.5843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3254902, 0.9882353, 0.6313726)
        /// </summary>
        public static readonly Color SeaGreen = new Color(0.3254902f, 0.9882353f, 0.6313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5019608, 0.9764706, 0.6784314)
        /// </summary>
        public static readonly Color Seafoam = new Color(0.5019608f, 0.9764706f, 0.6784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4705882, 0.8196079, 0.7137255)
        /// </summary>
        public static readonly Color SeafoamBlue = new Color(0.4705882f, 0.8196079f, 0.7137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4784314, 0.9764706, 0.6705883)
        /// </summary>
        public static readonly Color SeafoamGreen = new Color(0.4784314f, 0.9764706f, 0.6705883f);

        /// <summary>
        /// A formatted XKCD survey colour (0.09411765, 0.8196079, 0.4823529)
        /// </summary>
        public static readonly Color Seaweed = new Color(0.09411765f, 0.8196079f, 0.4823529f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2078431, 0.6784314, 0.4196078)
        /// </summary>
        public static readonly Color SeaweedGreen = new Color(0.2078431f, 0.6784314f, 0.4196078f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5960785, 0.3686275, 0.1686275)
        /// </summary>
        public static readonly Color Sepia = new Color(0.5960785f, 0.3686275f, 0.1686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.7058824, 0.2980392)
        /// </summary>
        public static readonly Color Shamrock = new Color(0.003921569f, 0.7058824f, 0.2980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.7568628, 0.3019608)
        /// </summary>
        public static readonly Color ShamrockGreen = new Color(0.007843138f, 0.7568628f, 0.3019608f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4980392, 0.372549, 0)
        /// </summary>
        public static readonly Color Shit = new Color(0.4980392f, 0.372549f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0.345098, 0.01568628)
        /// </summary>
        public static readonly Color ShitBrown = new Color(0.4823529f, 0.345098f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.5019608, 0)
        /// </summary>
        public static readonly Color ShitGreen = new Color(0.4588235f, 0.5019608f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.9960784, 0.007843138, 0.6352941)
        /// </summary>
        public static readonly Color ShockingPink = new Color(0.9960784f, 0.007843138f, 0.6352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.7254902, 0.172549)
        /// </summary>
        public static readonly Color SickGreen = new Color(0.6156863f, 0.7254902f, 0.172549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5803922, 0.6980392, 0.1098039)
        /// </summary>
        public static readonly Color SicklyGreen = new Color(0.5803922f, 0.6980392f, 0.1098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8156863, 0.8941177, 0.1607843)
        /// </summary>
        public static readonly Color SicklyYellow = new Color(0.8156863f, 0.8941177f, 0.1607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6627451, 0.3372549, 0.1176471)
        /// </summary>
        public static readonly Color Sienna = new Color(0.6627451f, 0.3372549f, 0.1176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.772549, 0.7882353, 0.7803922)
        /// </summary>
        public static readonly Color Silver = new Color(0.772549f, 0.7882353f, 0.7803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.509804, 0.7921569, 0.9882353)
        /// </summary>
        public static readonly Color Sky = new Color(0.509804f, 0.7921569f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.7333333, 0.9921569)
        /// </summary>
        public static readonly Color SkyBlue = new Color(0.4588235f, 0.7333333f, 0.9921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3176471, 0.3960784, 0.4470588)
        /// </summary>
        public static readonly Color Slate = new Color(0.3176471f, 0.3960784f, 0.4470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3568628, 0.4862745, 0.6)
        /// </summary>
        public static readonly Color SlateBlue = new Color(0.3568628f, 0.4862745f, 0.6f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.5529412, 0.427451)
        /// </summary>
        public static readonly Color SlateGreen = new Color(0.3960784f, 0.5529412f, 0.427451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3490196, 0.3960784, 0.427451)
        /// </summary>
        public static readonly Color SlateGrey = new Color(0.3490196f, 0.3960784f, 0.427451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6, 0.8, 0.01568628)
        /// </summary>
        public static readonly Color SlimeGreen = new Color(0.6f, 0.8f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6745098, 0.7333333, 0.05098039)
        /// </summary>
        public static readonly Color Snot = new Color(0.6745098f, 0.7333333f, 0.05098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6156863, 0.7568628, 0)
        /// </summary>
        public static readonly Color SnotGreen = new Color(0.6156863f, 0.7568628f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.3921569, 0.5333334, 0.9176471)
        /// </summary>
        public static readonly Color SoftBlue = new Color(0.3921569f, 0.5333334f, 0.9176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4352941, 0.7607843, 0.4627451)
        /// </summary>
        public static readonly Color SoftGreen = new Color(0.4352941f, 0.7607843f, 0.4627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.6901961, 0.7529412)
        /// </summary>
        public static readonly Color SoftPink = new Color(0.9921569f, 0.6901961f, 0.7529412f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6509804, 0.4352941, 0.7098039)
        /// </summary>
        public static readonly Color SoftPurple = new Color(0.6509804f, 0.4352941f, 0.7098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1176471, 0.972549, 0.4627451)
        /// </summary>
        public static readonly Color Spearmint = new Color(0.1176471f, 0.972549f, 0.4627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6627451, 0.9764706, 0.4431373)
        /// </summary>
        public static readonly Color SpringGreen = new Color(0.6627451f, 0.9764706f, 0.4431373f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03921569, 0.372549, 0.2196078)
        /// </summary>
        public static readonly Color Spruce = new Color(0.03921569f, 0.372549f, 0.2196078f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9490196, 0.6705883, 0.08235294)
        /// </summary>
        public static readonly Color Squash = new Color(0.9490196f, 0.6705883f, 0.08235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4509804, 0.5215687, 0.5843138)
        /// </summary>
        public static readonly Color Steel = new Color(0.4509804f, 0.5215687f, 0.5843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3529412, 0.4901961, 0.6039216)
        /// </summary>
        public static readonly Color SteelBlue = new Color(0.3529412f, 0.4901961f, 0.6039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4352941, 0.509804, 0.5411765)
        /// </summary>
        public static readonly Color SteelGrey = new Color(0.4352941f, 0.509804f, 0.5411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6784314, 0.6470588, 0.5294118)
        /// </summary>
        public static readonly Color Stone = new Color(0.6784314f, 0.6470588f, 0.5294118f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3137255, 0.4823529, 0.6117647)
        /// </summary>
        public static readonly Color StormyBlue = new Color(0.3137255f, 0.4823529f, 0.6117647f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.9647059, 0.4745098)
        /// </summary>
        public static readonly Color Straw = new Color(0.9882353f, 0.9647059f, 0.4745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9843137, 0.1607843, 0.2627451)
        /// </summary>
        public static readonly Color Strawberry = new Color(0.9843137f, 0.1607843f, 0.2627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04705882, 0.02352941, 0.9686275)
        /// </summary>
        public static readonly Color StrongBlue = new Color(0.04705882f, 0.02352941f, 0.9686275f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.02745098, 0.5372549)
        /// </summary>
        public static readonly Color StrongPink = new Color(1, 0.02745098f, 0.5372549f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.8745098, 0.1333333)
        /// </summary>
        public static readonly Color SunYellow = new Color(1, 0.8745098f, 0.1333333f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.772549, 0.07058824)
        /// </summary>
        public static readonly Color Sunflower = new Color(1, 0.772549f, 0.07058824f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.854902, 0.01176471)
        /// </summary>
        public static readonly Color SunflowerYellow = new Color(1, 0.854902f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9764706, 0.09019608)
        /// </summary>
        public static readonly Color SunnyYellow = new Color(1, 0.9764706f, 0.09019608f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9921569, 0.2156863)
        /// </summary>
        public static readonly Color SunshineYellow = new Color(1, 0.9921569f, 0.2156863f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4117647, 0.5137255, 0.2235294)
        /// </summary>
        public static readonly Color Swamp = new Color(0.4117647f, 0.5137255f, 0.2235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.454902, 0.5215687, 0)
        /// </summary>
        public static readonly Color SwampGreen = new Color(0.454902f, 0.5215687f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.8196079, 0.6980392, 0.4352941)
        /// </summary>
        public static readonly Color Tan = new Color(0.8196079f, 0.6980392f, 0.4352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6705883, 0.4941176, 0.2980392)
        /// </summary>
        public static readonly Color TanBrown = new Color(0.6705883f, 0.4941176f, 0.2980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6627451, 0.7450981, 0.4392157)
        /// </summary>
        public static readonly Color TanGreen = new Color(0.6627451f, 0.7450981f, 0.4392157f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.5803922, 0.03137255)
        /// </summary>
        public static readonly Color Tangerine = new Color(1, 0.5803922f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7254902, 0.6352941, 0.5058824)
        /// </summary>
        public static readonly Color Taupe = new Color(0.7254902f, 0.6352941f, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3960784, 0.6705883, 0.4862745)
        /// </summary>
        public static readonly Color Tea = new Color(0.3960784f, 0.6705883f, 0.4862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7411765, 0.972549, 0.6392157)
        /// </summary>
        public static readonly Color TeaGreen = new Color(0.7411765f, 0.972549f, 0.6392157f);

        /// <summary>
        /// A formatted XKCD survey colour (0.007843138, 0.5764706, 0.5254902)
        /// </summary>
        public static readonly Color Teal = new Color(0.007843138f, 0.5764706f, 0.5254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.5333334, 0.6235294)
        /// </summary>
        public static readonly Color TealBlue = new Color(0.003921569f, 0.5333334f, 0.6235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.145098, 0.6392157, 0.4352941)
        /// </summary>
        public static readonly Color TealGreen = new Color(0.145098f, 0.6392157f, 0.4352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1411765, 0.7372549, 0.6588235)
        /// </summary>
        public static readonly Color Tealish = new Color(0.1411765f, 0.7372549f, 0.6588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.04705882, 0.8627451, 0.4509804)
        /// </summary>
        public static readonly Color TealishGreen = new Color(0.04705882f, 0.8627451f, 0.4509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7882353, 0.3921569, 0.2313726)
        /// </summary>
        public static readonly Color TerraCotta = new Color(0.7882353f, 0.3921569f, 0.2313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.4078431, 0.2627451)
        /// </summary>
        public static readonly Color Terracota = new Color(0.7960784f, 0.4078431f, 0.2627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7921569, 0.4, 0.254902)
        /// </summary>
        public static readonly Color Terracotta = new Color(0.7921569f, 0.4f, 0.254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0.9490196, 0.854902)
        /// </summary>
        public static readonly Color TiffanyBlue = new Color(0.4823529f, 0.9490196f, 0.854902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9372549, 0.2509804, 0.1490196)
        /// </summary>
        public static readonly Color Tomato = new Color(0.9372549f, 0.2509804f, 0.1490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9254902, 0.1764706, 0.003921569)
        /// </summary>
        public static readonly Color TomatoRed = new Color(0.9254902f, 0.1764706f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.07450981, 0.7333333, 0.6862745)
        /// </summary>
        public static readonly Color Topaz = new Color(0.07450981f, 0.7333333f, 0.6862745f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.6745098, 0.4901961)
        /// </summary>
        public static readonly Color Toupe = new Color(0.7803922f, 0.6745098f, 0.4901961f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3803922, 0.8705882, 0.1647059)
        /// </summary>
        public static readonly Color ToxicGreen = new Color(0.3803922f, 0.8705882f, 0.1647059f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1647059, 0.4941176, 0.09803922)
        /// </summary>
        public static readonly Color TreeGreen = new Color(0.1647059f, 0.4941176f, 0.09803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.003921569, 0.05882353, 0.8)
        /// </summary>
        public static readonly Color TrueBlue = new Color(0.003921569f, 0.05882353f, 0.8f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03137255, 0.5803922, 0.01568628)
        /// </summary>
        public static readonly Color TrueGreen = new Color(0.03137255f, 0.5803922f, 0.01568628f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.7607843, 0.6745098)
        /// </summary>
        public static readonly Color Turquoise = new Color(0.02352941f, 0.7607843f, 0.6745098f);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.6941177, 0.7686275)
        /// </summary>
        public static readonly Color TurquoiseBlue = new Color(0.02352941f, 0.6941177f, 0.7686275f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01568628, 0.9568627, 0.5372549)
        /// </summary>
        public static readonly Color TurquoiseGreen = new Color(0.01568628f, 0.9568627f, 0.5372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.7215686, 0.3098039)
        /// </summary>
        public static readonly Color TurtleGreen = new Color(0.4588235f, 0.7215686f, 0.3098039f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3058824, 0.3176471, 0.5450981)
        /// </summary>
        public static readonly Color Twilight = new Color(0.3058824f, 0.3176471f, 0.5450981f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03921569, 0.2627451, 0.4784314)
        /// </summary>
        public static readonly Color TwilightBlue = new Color(0.03921569f, 0.2627451f, 0.4784314f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1921569, 0.4, 0.5411765)
        /// </summary>
        public static readonly Color UglyBlue = new Color(0.1921569f, 0.4f, 0.5411765f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4901961, 0.4431373, 0.01176471)
        /// </summary>
        public static readonly Color UglyBrown = new Color(0.4901961f, 0.4431373f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4784314, 0.5921569, 0.01176471)
        /// </summary>
        public static readonly Color UglyGreen = new Color(0.4784314f, 0.5921569f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8039216, 0.4588235, 0.5176471)
        /// </summary>
        public static readonly Color UglyPink = new Color(0.8039216f, 0.4588235f, 0.5176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6431373, 0.2588235, 0.627451)
        /// </summary>
        public static readonly Color UglyPurple = new Color(0.6431373f, 0.2588235f, 0.627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8156863, 0.7568628, 0.003921569)
        /// </summary>
        public static readonly Color UglyYellow = new Color(0.8156863f, 0.7568628f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1254902, 0, 0.6941177)
        /// </summary>
        public static readonly Color Ultramarine = new Color(0.1254902f, 0, 0.6941177f);

        /// <summary>
        /// A formatted XKCD survey colour (0.09411765, 0.01960784, 0.8588235)
        /// </summary>
        public static readonly Color UltramarineBlue = new Color(0.09411765f, 0.01960784f, 0.8588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6980392, 0.3921569, 0)
        /// </summary>
        public static readonly Color Umber = new Color(0.6980392f, 0.3921569f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.4588235, 0.03137255, 0.3176471)
        /// </summary>
        public static readonly Color Velvet = new Color(0.4588235f, 0.03137255f, 0.3176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9568627, 0.1960784, 0.04705882)
        /// </summary>
        public static readonly Color Vermillion = new Color(0.9568627f, 0.1960784f, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0, 0.003921569, 0.2)
        /// </summary>
        public static readonly Color VeryDarkBlue = new Color(0, 0.003921569f, 0.2f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1137255, 0.007843138, 0)
        /// </summary>
        public static readonly Color VeryDarkBrown = new Color(0.1137255f, 0.007843138f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.02352941, 0.1803922, 0.01176471)
        /// </summary>
        public static readonly Color VeryDarkGreen = new Color(0.02352941f, 0.1803922f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1647059, 0.003921569, 0.2039216)
        /// </summary>
        public static readonly Color VeryDarkPurple = new Color(0.1647059f, 0.003921569f, 0.2039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8352941, 1, 1)
        /// </summary>
        public static readonly Color VeryLightBlue = new Color(0.8352941f, 1, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.827451, 0.7137255, 0.5137255)
        /// </summary>
        public static readonly Color VeryLightBrown = new Color(0.827451f, 0.7137255f, 0.5137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8196079, 1, 0.7411765)
        /// </summary>
        public static readonly Color VeryLightGreen = new Color(0.8196079f, 1, 0.7411765f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.9568627, 0.9490196)
        /// </summary>
        public static readonly Color VeryLightPink = new Color(1, 0.9568627f, 0.9490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9647059, 0.8078431, 0.9882353)
        /// </summary>
        public static readonly Color VeryLightPurple = new Color(0.9647059f, 0.8078431f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8392157, 1, 0.9960784)
        /// </summary>
        public static readonly Color VeryPaleBlue = new Color(0.8392157f, 1, 0.9960784f);

        /// <summary>
        /// A formatted XKCD survey colour (0.8117647, 0.9921569, 0.7372549)
        /// </summary>
        public static readonly Color VeryPaleGreen = new Color(0.8117647f, 0.9921569f, 0.7372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.01176471, 0.2235294, 0.972549)
        /// </summary>
        public static readonly Color VibrantBlue = new Color(0.01176471f, 0.2235294f, 0.972549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.03921569, 0.8666667, 0.03137255)
        /// </summary>
        public static readonly Color VibrantGreen = new Color(0.03921569f, 0.8666667f, 0.03137255f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6784314, 0.01176471, 0.8705882)
        /// </summary>
        public static readonly Color VibrantPurple = new Color(0.6784314f, 0.01176471f, 0.8705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6039216, 0.05490196, 0.9176471)
        /// </summary>
        public static readonly Color Violet = new Color(0.6039216f, 0.05490196f, 0.9176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.3176471, 0.03921569, 0.7882353)
        /// </summary>
        public static readonly Color VioletBlue = new Color(0.3176471f, 0.03921569f, 0.7882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9843137, 0.372549, 0.9882353)
        /// </summary>
        public static readonly Color VioletPink = new Color(0.9843137f, 0.372549f, 0.9882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6470588, 0, 0.3333333)
        /// </summary>
        public static readonly Color VioletRed = new Color(0.6470588f, 0, 0.3333333f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1176471, 0.5686275, 0.4039216)
        /// </summary>
        public static readonly Color Viridian = new Color(0.1176471f, 0.5686275f, 0.4039216f);

        /// <summary>
        /// A formatted XKCD survey colour (0.08235294, 0.1803922, 1)
        /// </summary>
        public static readonly Color VividBlue = new Color(0.08235294f, 0.1803922f, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.1843137, 0.9372549, 0.0627451)
        /// </summary>
        public static readonly Color VividGreen = new Color(0.1843137f, 0.9372549f, 0.0627451f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6, 0, 0.9803922)
        /// </summary>
        public static readonly Color VividPurple = new Color(0.6f, 0, 0.9803922f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6352941, 0.6431373, 0.08235294)
        /// </summary>
        public static readonly Color Vomit = new Color(0.6352941f, 0.6431373f, 0.08235294f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5372549, 0.6352941, 0.01176471)
        /// </summary>
        public static readonly Color VomitGreen = new Color(0.5372549f, 0.6352941f, 0.01176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7803922, 0.7568628, 0.04705882)
        /// </summary>
        public static readonly Color VomitYellow = new Color(0.7803922f, 0.7568628f, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.2941177, 0.3411765, 0.8588235)
        /// </summary>
        public static readonly Color WarmBlue = new Color(0.2941177f, 0.3411765f, 0.8588235f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5882353, 0.3058824, 0.007843138)
        /// </summary>
        public static readonly Color WarmBrown = new Color(0.5882353f, 0.3058824f, 0.007843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5921569, 0.5411765, 0.5176471)
        /// </summary>
        public static readonly Color WarmGrey = new Color(0.5921569f, 0.5411765f, 0.5176471f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9843137, 0.3333333, 0.5058824)
        /// </summary>
        public static readonly Color WarmPink = new Color(0.9843137f, 0.3333333f, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5843138, 0.1803922, 0.5607843)
        /// </summary>
        public static readonly Color WarmPurple = new Color(0.5843138f, 0.1803922f, 0.5607843f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7372549, 0.9607843, 0.6509804)
        /// </summary>
        public static readonly Color WashedOutGreen = new Color(0.7372549f, 0.9607843f, 0.6509804f);

        /// <summary>
        /// A formatted XKCD survey colour (0.05490196, 0.5294118, 0.8)
        /// </summary>
        public static readonly Color WaterBlue = new Color(0.05490196f, 0.5294118f, 0.8f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9921569, 0.2745098, 0.3490196)
        /// </summary>
        public static readonly Color Watermelon = new Color(0.9921569f, 0.2745098f, 0.3490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.227451, 0.8980392, 0.4980392)
        /// </summary>
        public static readonly Color WeirdGreen = new Color(0.227451f, 0.8980392f, 0.4980392f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9843137, 0.8666667, 0.4941176)
        /// </summary>
        public static readonly Color Wheat = new Color(0.9843137f, 0.8666667f, 0.4941176f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 1)
        /// </summary>
        public static readonly Color White = new Color(1, 1, 1);

        /// <summary>
        /// A formatted XKCD survey colour (0.2156863, 0.4705882, 0.7490196)
        /// </summary>
        public static readonly Color WindowsBlue = new Color(0.2156863f, 0.4705882f, 0.7490196f);

        /// <summary>
        /// A formatted XKCD survey colour (0.5019608, 0.003921569, 0.2470588)
        /// </summary>
        public static readonly Color Wine = new Color(0.5019608f, 0.003921569f, 0.2470588f);

        /// <summary>
        /// A formatted XKCD survey colour (0.4823529, 0.01176471, 0.1372549)
        /// </summary>
        public static readonly Color WineRed = new Color(0.4823529f, 0.01176471f, 0.1372549f);

        /// <summary>
        /// A formatted XKCD survey colour (0.1254902, 0.9764706, 0.5254902)
        /// </summary>
        public static readonly Color Wintergreen = new Color(0.1254902f, 0.9764706f, 0.5254902f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6588235, 0.4901961, 0.7607843)
        /// </summary>
        public static readonly Color Wisteria = new Color(0.6588235f, 0.4901961f, 0.7607843f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 1, 0.07843138)
        /// </summary>
        public static readonly Color Yellow = new Color(1, 1, 0.07843138f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7176471, 0.5803922, 0)
        /// </summary>
        public static readonly Color YellowBrown = new Color(0.7176471f, 0.5803922f, 0);

        /// <summary>
        /// A formatted XKCD survey colour (0.7529412, 0.9843137, 0.1764706)
        /// </summary>
        public static readonly Color YellowGreen = new Color(0.7529412f, 0.9843137f, 0.1764706f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7960784, 0.6156863, 0.02352941)
        /// </summary>
        public static readonly Color YellowOchre = new Color(0.7960784f, 0.6156863f, 0.02352941f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.6901961, 0.003921569)
        /// </summary>
        public static readonly Color YellowOrange = new Color(0.9882353f, 0.6901961f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.8901961, 0.4313726)
        /// </summary>
        public static readonly Color YellowTan = new Color(1, 0.8901961f, 0.4313726f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7843137, 0.9921569, 0.2392157)
        /// </summary>
        public static readonly Color Yellow_Green = new Color(0.7843137f, 0.9921569f, 0.2392157f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7333333, 0.9764706, 0.05882353)
        /// </summary>
        public static readonly Color Yellowgreen = new Color(0.7333333f, 0.9764706f, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9803922, 0.9333333, 0.4)
        /// </summary>
        public static readonly Color Yellowish = new Color(0.9803922f, 0.9333333f, 0.4f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6078432, 0.4784314, 0.003921569)
        /// </summary>
        public static readonly Color YellowishBrown = new Color(0.6078432f, 0.4784314f, 0.003921569f);

        /// <summary>
        /// A formatted XKCD survey colour (0.6901961, 0.8666667, 0.08627451)
        /// </summary>
        public static readonly Color YellowishGreen = new Color(0.6901961f, 0.8666667f, 0.08627451f);

        /// <summary>
        /// A formatted XKCD survey colour (1, 0.6705883, 0.05882353)
        /// </summary>
        public static readonly Color YellowishOrange = new Color(1, 0.6705883f, 0.05882353f);

        /// <summary>
        /// A formatted XKCD survey colour (0.9882353, 0.9882353, 0.5058824)
        /// </summary>
        public static readonly Color YellowishTan = new Color(0.9882353f, 0.9882353f, 0.5058824f);

        /// <summary>
        /// A formatted XKCD survey colour (0.682353, 0.5450981, 0.04705882)
        /// </summary>
        public static readonly Color YellowyBrown = new Color(0.682353f, 0.5450981f, 0.04705882f);

        /// <summary>
        /// A formatted XKCD survey colour (0.7490196, 0.945098, 0.1568628)
        /// </summary>
        public static readonly Color YellowyGreen = new Color(0.7490196f, 0.945098f, 0.1568628f);
    }
}