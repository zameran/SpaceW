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
        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.9960784, 0.03529412)
        /// </summary>
        public static readonly Color AcidGreen = new(0.5607843f, 0.9960784f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7411765, 0.4235294, 0.282353)
        /// </summary>
        public static readonly Color Adobe = new(0.7411765f, 0.4235294f, 0.282353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3294118, 0.6745098, 0.4078431)
        /// </summary>
        public static readonly Color Algae = new(0.3294118f, 0.6745098f, 0.4078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1294118, 0.7647059, 0.4352941)
        /// </summary>
        public static readonly Color AlgaeGreen = new(0.1294118f, 0.7647059f, 0.4352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02745098, 0.05098039, 0.05098039)
        /// </summary>
        public static readonly Color AlmostBlack = new(0.02745098f, 0.05098039f, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.7019608, 0.03137255)
        /// </summary>
        public static readonly Color Amber = new(0.9960784f, 0.7019608f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6078432, 0.372549, 0.7529412)
        /// </summary>
        public static readonly Color Amethyst = new(0.6078432f, 0.372549f, 0.7529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4313726, 0.7960784, 0.2352941)
        /// </summary>
        public static readonly Color Apple = new(0.4313726f, 0.7960784f, 0.2352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4627451, 0.8039216, 0.1490196)
        /// </summary>
        public static readonly Color AppleGreen = new(0.4627451f, 0.8039216f, 0.1490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6941177, 0.427451)
        /// </summary>
        public static readonly Color Apricot = new(1, 0.6941177f, 0.427451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.07450981, 0.9176471, 0.7882353)
        /// </summary>
        public static readonly Color Aqua = new(0.07450981f, 0.9176471f, 0.7882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.8470588, 0.9137255)
        /// </summary>
        public static readonly Color AquaBlue = new(0.007843138f, 0.8470588f, 0.9137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.07058824, 0.8823529, 0.5764706)
        /// </summary>
        public static readonly Color AquaGreen = new(0.07058824f, 0.8823529f, 0.5764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1803922, 0.9098039, 0.7333333)
        /// </summary>
        public static readonly Color AquaMarine = new(0.1803922f, 0.9098039f, 0.7333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.8470588, 0.6980392)
        /// </summary>
        public static readonly Color Aquamarine = new(0.01568628f, 0.8470588f, 0.6980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2941177, 0.3647059, 0.08627451)
        /// </summary>
        public static readonly Color ArmyGreen = new(0.2941177f, 0.3647059f, 0.08627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4666667, 0.6705883, 0.3372549)
        /// </summary>
        public static readonly Color Asparagus = new(0.4666667f, 0.6705883f, 0.3372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2392157, 0.02745098, 0.2039216)
        /// </summary>
        public static readonly Color Aubergine = new(0.2392157f, 0.02745098f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6039216, 0.1882353, 0.003921569)
        /// </summary>
        public static readonly Color Auburn = new(0.6039216f, 0.1882353f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5647059, 0.6941177, 0.2039216)
        /// </summary>
        public static readonly Color Avocado = new(0.5647059f, 0.6941177f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.6627451, 0.1333333)
        /// </summary>
        public static readonly Color AvocadoGreen = new(0.5294118f, 0.6627451f, 0.1333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1137255, 0.3647059, 0.9254902)
        /// </summary>
        public static readonly Color Azul = new(0.1137255f, 0.3647059f, 0.9254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.6039216, 0.9529412)
        /// </summary>
        public static readonly Color Azure = new(0.02352941f, 0.6039216f, 0.9529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6352941, 0.8117647, 0.9960784)
        /// </summary>
        public static readonly Color BabyBlue = new(0.6352941f, 0.8117647f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5490196, 1, 0.6196079)
        /// </summary>
        public static readonly Color BabyGreen = new(0.5490196f, 1, 0.6196079f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.7176471, 0.8078431)
        /// </summary>
        public static readonly Color BabyPink = new(1, 0.7176471f, 0.8078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6705883, 0.5647059, 0.01568628)
        /// </summary>
        public static readonly Color BabyPoo = new(0.6705883f, 0.5647059f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5764706, 0.4862745, 0)
        /// </summary>
        public static readonly Color BabyPoop = new(0.5764706f, 0.4862745f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.5960785, 0.01960784)
        /// </summary>
        public static readonly Color BabyPoopGreen = new(0.5607843f, 0.5960785f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7137255, 0.7686275, 0.02352941)
        /// </summary>
        public static readonly Color BabyPukeGreen = new(0.7137255f, 0.7686275f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 0.6078432, 0.9686275)
        /// </summary>
        public static readonly Color BabyPurple = new(0.7921569f, 0.6078432f, 0.9686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6784314, 0.5647059, 0.05098039)
        /// </summary>
        public static readonly Color BabyShitBrown = new(0.6784314f, 0.5647059f, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5333334, 0.5921569, 0.09019608)
        /// </summary>
        public static readonly Color BabyShitGreen = new(0.5333334f, 0.5921569f, 0.09019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.4941176)
        /// </summary>
        public static readonly Color Banana = new(1, 1, 0.4941176f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9803922, 0.9960784, 0.2941177)
        /// </summary>
        public static readonly Color BananaYellow = new(0.9803922f, 0.9960784f, 0.2941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.2745098, 0.6470588)
        /// </summary>
        public static readonly Color BarbiePink = new(0.9960784f, 0.2745098f, 0.6470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5803922, 0.6745098, 0.007843138)
        /// </summary>
        public static readonly Color BarfGreen = new(0.5803922f, 0.6745098f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.1137255, 0.7215686)
        /// </summary>
        public static readonly Color Barney = new(0.6745098f, 0.1137255f, 0.7215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.627451, 0.01568628, 0.5960785)
        /// </summary>
        public static readonly Color BarneyPurple = new(0.627451f, 0.01568628f, 0.5960785f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4196078, 0.4862745, 0.5215687)
        /// </summary>
        public static readonly Color BattleshipGrey = new(0.4196078f, 0.4862745f, 0.5215687f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9019608, 0.854902, 0.6509804)
        /// </summary>
        public static readonly Color Beige = new(0.9019608f, 0.854902f, 0.6509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6, 0.05882353, 0.2941177)
        /// </summary>
        public static readonly Color Berry = new(0.6f, 0.05882353f, 0.2941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7098039, 0.7647059, 0.02352941)
        /// </summary>
        public static readonly Color Bile = new(0.7098039f, 0.7647059f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0, 0)
        /// </summary>
        public static readonly Color Black = new(0, 0, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6862745, 0.6588235, 0.5450981)
        /// </summary>
        public static readonly Color Bland = new(0.6862745f, 0.6588235f, 0.5450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4666667, 0, 0.003921569)
        /// </summary>
        public static readonly Color Blood = new(0.4666667f, 0, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.2941177, 0.01176471)
        /// </summary>
        public static readonly Color BloodOrange = new(0.9960784f, 0.2941177f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5960785, 0, 0.007843138)
        /// </summary>
        public static readonly Color BloodRed = new(0.5960785f, 0, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.2627451, 0.8745098)
        /// </summary>
        public static readonly Color Blue = new(0.01176471f, 0.2627451f, 0.8745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1333333, 0.2588235, 0.7803922)
        /// </summary>
        public static readonly Color BlueBlue = new(0.1333333f, 0.2588235f, 0.7803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.07450981, 0.4941176, 0.427451)
        /// </summary>
        public static readonly Color BlueGreen = new(0.07450981f, 0.4941176f, 0.427451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3764706, 0.4862745, 0.5568628)
        /// </summary>
        public static readonly Color BlueGrey = new(0.3764706f, 0.4862745f, 0.5568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3411765, 0.1607843, 0.8078431)
        /// </summary>
        public static readonly Color BluePurple = new(0.3411765f, 0.1607843f, 0.8078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3647059, 0.02352941, 0.9137255)
        /// </summary>
        public static readonly Color BlueViolet = new(0.3647059f, 0.02352941f, 0.9137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3254902, 0.2352941, 0.7764706)
        /// </summary>
        public static readonly Color BlueWithAHintOfPurple = new(0.3254902f, 0.2352941f, 0.7764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.05882353, 0.6078432, 0.5568628)
        /// </summary>
        public static readonly Color Blue_Green = new(0.05882353f, 0.6078432f, 0.5568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.5529412, 0.6392157)
        /// </summary>
        public static readonly Color Blue_Grey = new(0.4588235f, 0.5529412f, 0.6392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3529412, 0.02352941, 0.9372549)
        /// </summary>
        public static readonly Color Blue_Purple = new(0.3529412f, 0.02352941f, 0.9372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2745098, 0.254902, 0.5882353)
        /// </summary>
        public static readonly Color Blueberry = new(0.2745098f, 0.254902f, 0.5882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.4784314, 0.4745098)
        /// </summary>
        public static readonly Color Bluegreen = new(0.003921569f, 0.4784314f, 0.4745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5215687, 0.6392157, 0.6980392)
        /// </summary>
        public static readonly Color Bluegrey = new(0.5215687f, 0.6392157f, 0.6980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1686275, 0.6941177, 0.4745098)
        /// </summary>
        public static readonly Color BlueyGreen = new(0.1686275f, 0.6941177f, 0.4745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5372549, 0.627451, 0.6901961)
        /// </summary>
        public static readonly Color BlueyGrey = new(0.5372549f, 0.627451f, 0.6901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3843137, 0.254902, 0.7803922)
        /// </summary>
        public static readonly Color BlueyPurple = new(0.3843137f, 0.254902f, 0.7803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1607843, 0.4627451, 0.7333333)
        /// </summary>
        public static readonly Color Bluish = new(0.1607843f, 0.4627451f, 0.7333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.0627451, 0.6509804, 0.454902)
        /// </summary>
        public static readonly Color BluishGreen = new(0.0627451f, 0.6509804f, 0.454902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.454902, 0.5450981, 0.5921569)
        /// </summary>
        public static readonly Color BluishGrey = new(0.454902f, 0.5450981f, 0.5921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4392157, 0.2313726, 0.9058824)
        /// </summary>
        public static readonly Color BluishPurple = new(0.4392157f, 0.2313726f, 0.9058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3333333, 0.2235294, 0.8)
        /// </summary>
        public static readonly Color Blurple = new(0.3333333f, 0.2235294f, 0.8f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9490196, 0.6196079, 0.5568628)
        /// </summary>
        public static readonly Color Blush = new(0.9490196f, 0.6196079f, 0.5568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.509804, 0.5490196)
        /// </summary>
        public static readonly Color BlushPink = new(0.9960784f, 0.509804f, 0.5490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6078432, 0.7098039, 0.2352941)
        /// </summary>
        public static readonly Color Booger = new(0.6078432f, 0.7098039f, 0.2352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5882353, 0.7058824, 0.01176471)
        /// </summary>
        public static readonly Color BoogerGreen = new(0.5882353f, 0.7058824f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0, 0.172549)
        /// </summary>
        public static readonly Color Bordeaux = new(0.4823529f, 0, 0.172549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3882353, 0.7019608, 0.3960784)
        /// </summary>
        public static readonly Color BoringGreen = new(0.3882353f, 0.7019608f, 0.3960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.2901961, 0.01960784)
        /// </summary>
        public static readonly Color BottleGreen = new(0.01568628f, 0.2901961f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.627451, 0.2117647, 0.1372549)
        /// </summary>
        public static readonly Color Brick = new(0.627451f, 0.2117647f, 0.1372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7568628, 0.2901961, 0.03529412)
        /// </summary>
        public static readonly Color BrickOrange = new(0.7568628f, 0.2901961f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.07843138, 0.007843138)
        /// </summary>
        public static readonly Color BrickRed = new(0.5607843f, 0.07843138f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04313726, 0.9764706, 0.9176471)
        /// </summary>
        public static readonly Color BrightAqua = new(0.04313726f, 0.9764706f, 0.9176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.3960784, 0.9882353)
        /// </summary>
        public static readonly Color BrightBlue = new(0.003921569f, 0.3960784f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.254902, 0.9921569, 0.9960784)
        /// </summary>
        public static readonly Color BrightCyan = new(0.254902f, 0.9921569f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 1, 0.02745098)
        /// </summary>
        public static readonly Color BrightGreen = new(0.003921569f, 1, 0.02745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.3764706, 1)
        /// </summary>
        public static readonly Color BrightLavender = new(0.7803922f, 0.3764706f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1490196, 0.9686275, 0.9921569)
        /// </summary>
        public static readonly Color BrightLightBlue = new(0.1490196f, 0.9686275f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1764706, 0.9960784, 0.3294118)
        /// </summary>
        public static readonly Color BrightLightGreen = new(0.1764706f, 0.9960784f, 0.3294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 0.3686275, 0.9843137)
        /// </summary>
        public static readonly Color BrightLilac = new(0.7882353f, 0.3686275f, 0.9843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.9921569, 0.01960784)
        /// </summary>
        public static readonly Color BrightLime = new(0.5294118f, 0.9921569f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.9960784, 0.03137255)
        /// </summary>
        public static readonly Color BrightLimeGreen = new(0.3960784f, 0.9960784f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.03137255, 0.9098039)
        /// </summary>
        public static readonly Color BrightMagenta = new(1, 0.03137255f, 0.9098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6117647, 0.7333333, 0.01568628)
        /// </summary>
        public static readonly Color BrightOlive = new(0.6117647f, 0.7333333f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.3568628, 0)
        /// </summary>
        public static readonly Color BrightOrange = new(1, 0.3568628f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.003921569, 0.6941177)
        /// </summary>
        public static readonly Color BrightPink = new(0.9960784f, 0.003921569f, 0.6941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.01176471, 0.9921569)
        /// </summary>
        public static readonly Color BrightPurple = new(0.7450981f, 0.01176471f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0, 0.05098039)
        /// </summary>
        public static readonly Color BrightRed = new(1, 0, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 1, 0.6509804)
        /// </summary>
        public static readonly Color BrightSeaGreen = new(0.01960784f, 1, 0.6509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.8, 0.9960784)
        /// </summary>
        public static readonly Color BrightSkyBlue = new(0.007843138f, 0.8f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.9764706, 0.7764706)
        /// </summary>
        public static readonly Color BrightTeal = new(0.003921569f, 0.9764706f, 0.7764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.05882353, 0.9960784, 0.9764706)
        /// </summary>
        public static readonly Color BrightTurquoise = new(0.05882353f, 0.9960784f, 0.9764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6784314, 0.03921569, 0.9921569)
        /// </summary>
        public static readonly Color BrightViolet = new(0.6784314f, 0.03921569f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9921569, 0.003921569)
        /// </summary>
        public static readonly Color BrightYellow = new(1, 0.9921569f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 1, 0)
        /// </summary>
        public static readonly Color BrightYellowGreen = new(0.6156863f, 1, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 0.282353, 0.05098039)
        /// </summary>
        public static readonly Color BritishRacingGreen = new(0.01960784f, 0.282353f, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.4745098, 0)
        /// </summary>
        public static readonly Color Bronze = new(0.6588235f, 0.4745098f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.2156863, 0)
        /// </summary>
        public static readonly Color Brown = new(0.3960784f, 0.2156863f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4392157, 0.4235294, 0.06666667)
        /// </summary>
        public static readonly Color BrownGreen = new(0.4392157f, 0.4235294f, 0.06666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5529412, 0.5176471, 0.4078431)
        /// </summary>
        public static readonly Color BrownGrey = new(0.5529412f, 0.5176471f, 0.4078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7254902, 0.4117647, 0.007843138)
        /// </summary>
        public static readonly Color BrownOrange = new(0.7254902f, 0.4117647f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.572549, 0.1686275, 0.01960784)
        /// </summary>
        public static readonly Color BrownRed = new(0.572549f, 0.1686275f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.5921569, 0.01960784)
        /// </summary>
        public static readonly Color BrownYellow = new(0.6980392f, 0.5921569f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6117647, 0.427451, 0.3411765)
        /// </summary>
        public static readonly Color Brownish = new(0.6117647f, 0.427451f, 0.3411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4156863, 0.4313726, 0.03529412)
        /// </summary>
        public static readonly Color BrownishGreen = new(0.4156863f, 0.4313726f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5254902, 0.4666667, 0.372549)
        /// </summary>
        public static readonly Color BrownishGrey = new(0.5254902f, 0.4666667f, 0.372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.4666667, 0.1372549)
        /// </summary>
        public static readonly Color BrownishOrange = new(0.7960784f, 0.4666667f, 0.1372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7607843, 0.4941176, 0.4745098)
        /// </summary>
        public static readonly Color BrownishPink = new(0.7607843f, 0.4941176f, 0.4745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4627451, 0.2588235, 0.3058824)
        /// </summary>
        public static readonly Color BrownishPurple = new(0.4627451f, 0.2588235f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6196079, 0.2117647, 0.1372549)
        /// </summary>
        public static readonly Color BrownishRed = new(0.6196079f, 0.2117647f, 0.1372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 0.6901961, 0.01176471)
        /// </summary>
        public static readonly Color BrownishYellow = new(0.7882353f, 0.6901961f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4352941, 0.4235294, 0.03921569)
        /// </summary>
        public static readonly Color BrownyGreen = new(0.4352941f, 0.4235294f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 0.4196078, 0.007843138)
        /// </summary>
        public static readonly Color BrownyOrange = new(0.7921569f, 0.4196078f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4941176, 0.2509804, 0.4431373)
        /// </summary>
        public static readonly Color Bruise = new(0.4941176f, 0.2509804f, 0.4431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4117647, 0.6862745)
        /// </summary>
        public static readonly Color BubbleGumPink = new(1, 0.4117647f, 0.6862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4235294, 0.7098039)
        /// </summary>
        public static readonly Color Bubblegum = new(1, 0.4235294f, 0.7098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.5137255, 0.8)
        /// </summary>
        public static readonly Color BubblegumPink = new(0.9960784f, 0.5137255f, 0.8f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.9647059, 0.6196079)
        /// </summary>
        public static readonly Color Buff = new(0.9960784f, 0.9647059f, 0.6196079f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3803922, 0, 0.1372549)
        /// </summary>
        public static readonly Color Burgundy = new(0.3803922f, 0, 0.1372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7529412, 0.3058824, 0.003921569)
        /// </summary>
        public static readonly Color BurntOrange = new(0.7529412f, 0.3058824f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6235294, 0.1372549, 0.01960784)
        /// </summary>
        public static readonly Color BurntRed = new(0.6235294f, 0.1372549f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7176471, 0.3215686, 0.01176471)
        /// </summary>
        public static readonly Color BurntSiena = new(0.7176471f, 0.3215686f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6901961, 0.3058824, 0.05882353)
        /// </summary>
        public static readonly Color BurntSienna = new(0.6901961f, 0.3058824f, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.627451, 0.2705882, 0.05490196)
        /// </summary>
        public static readonly Color BurntUmber = new(0.627451f, 0.2705882f, 0.05490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8352941, 0.6705883, 0.03529412)
        /// </summary>
        public static readonly Color BurntYellow = new(0.8352941f, 0.6705883f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4078431, 0.1960784, 0.8901961)
        /// </summary>
        public static readonly Color Burple = new(0.4078431f, 0.1960784f, 0.8901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.5058824)
        /// </summary>
        public static readonly Color Butter = new(1, 1, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9921569, 0.454902)
        /// </summary>
        public static readonly Color ButterYellow = new(1, 0.9921569f, 0.454902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.6941177, 0.2784314)
        /// </summary>
        public static readonly Color Butterscotch = new(0.9921569f, 0.6941177f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3058824, 0.454902, 0.5882353)
        /// </summary>
        public static readonly Color CadetBlue = new(0.3058824f, 0.454902f, 0.5882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7764706, 0.6235294, 0.3490196)
        /// </summary>
        public static readonly Color Camel = new(0.7764706f, 0.6235294f, 0.3490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.5607843, 0.3058824)
        /// </summary>
        public static readonly Color Camo = new(0.4980392f, 0.5607843f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3215686, 0.3960784, 0.145098)
        /// </summary>
        public static readonly Color CamoGreen = new(0.3215686f, 0.3960784f, 0.145098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2941177, 0.3803922, 0.07450981)
        /// </summary>
        public static readonly Color CamouflageGreen = new(0.2941177f, 0.3803922f, 0.07450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 1, 0.3882353)
        /// </summary>
        public static readonly Color Canary = new(0.9921569f, 1, 0.3882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9960784, 0.2509804)
        /// </summary>
        public static readonly Color CanaryYellow = new(1, 0.9960784f, 0.2509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.3882353, 0.9137255)
        /// </summary>
        public static readonly Color CandyPink = new(1, 0.3882353f, 0.9137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6862745, 0.4352941, 0.03529412)
        /// </summary>
        public static readonly Color Caramel = new(0.6862745f, 0.4352941f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.007843138, 0.08627451)
        /// </summary>
        public static readonly Color Carmine = new(0.6156863f, 0.007843138f, 0.08627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.4745098, 0.5607843)
        /// </summary>
        public static readonly Color Carnation = new(0.9921569f, 0.4745098f, 0.5607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4980392, 0.654902)
        /// </summary>
        public static readonly Color CarnationPink = new(1, 0.4980392f, 0.654902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5411765, 0.7215686, 0.9960784)
        /// </summary>
        public static readonly Color CarolinaBlue = new(0.5411765f, 0.7215686f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.9921569, 0.7176471)
        /// </summary>
        public static readonly Color Celadon = new(0.7450981f, 0.9921569f, 0.7176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7568628, 0.9921569, 0.5843138)
        /// </summary>
        public static readonly Color Celery = new(0.7568628f, 0.9921569f, 0.5843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0.6392157, 0.5686275)
        /// </summary>
        public static readonly Color Cement = new(0.6470588f, 0.6392157f, 0.5686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8705882, 0.04705882, 0.3843137)
        /// </summary>
        public static readonly Color Cerise = new(0.8705882f, 0.04705882f, 0.3843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.5215687, 0.8196079)
        /// </summary>
        public static readonly Color Cerulean = new(0.01568628f, 0.5215687f, 0.8196079f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 0.4313726, 0.9333333)
        /// </summary>
        public static readonly Color CeruleanBlue = new(0.01960784f, 0.4313726f, 0.9333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2039216, 0.2196078, 0.2156863)
        /// </summary>
        public static readonly Color Charcoal = new(0.2039216f, 0.2196078f, 0.2156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2352941, 0.254902, 0.2588235)
        /// </summary>
        public static readonly Color CharcoalGrey = new(0.2352941f, 0.254902f, 0.2588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7568628, 0.972549, 0.03921569)
        /// </summary>
        public static readonly Color Chartreuse = new(0.7568628f, 0.972549f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8117647, 0.007843138, 0.2039216)
        /// </summary>
        public static readonly Color Cherry = new(0.8117647f, 0.007843138f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9686275, 0.007843138, 0.1647059)
        /// </summary>
        public static readonly Color CherryRed = new(0.9686275f, 0.007843138f, 0.1647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.454902, 0.1568628, 0.007843138)
        /// </summary>
        public static readonly Color Chestnut = new(0.454902f, 0.1568628f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2392157, 0.1098039, 0.007843138)
        /// </summary>
        public static readonly Color Chocolate = new(0.2392157f, 0.1098039f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.254902, 0.09803922, 0)
        /// </summary>
        public static readonly Color ChocolateBrown = new(0.254902f, 0.09803922f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.3098039, 0.02352941)
        /// </summary>
        public static readonly Color Cinnamon = new(0.6745098f, 0.3098039f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4078431, 0, 0.09411765)
        /// </summary>
        public static readonly Color Claret = new(0.4078431f, 0, 0.09411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7137255, 0.4156863, 0.3137255)
        /// </summary>
        public static readonly Color Clay = new(0.7137255f, 0.4156863f, 0.3137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.4431373, 0.2392157)
        /// </summary>
        public static readonly Color ClayBrown = new(0.6980392f, 0.4431373f, 0.2392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1411765, 0.4784314, 0.9921569)
        /// </summary>
        public static readonly Color ClearBlue = new(0.1411765f, 0.4784314f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.7607843, 0.8509804)
        /// </summary>
        public static readonly Color CloudyBlue = new(0.6745098f, 0.7607843f, 0.8509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1176471, 0.282353, 0.5607843)
        /// </summary>
        public static readonly Color Cobalt = new(0.1176471f, 0.282353f, 0.5607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.03921569, 0.654902)
        /// </summary>
        public static readonly Color CobaltBlue = new(0.01176471f, 0.03921569f, 0.654902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.372549, 0.2588235)
        /// </summary>
        public static readonly Color Cocoa = new(0.5294118f, 0.372549f, 0.2588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6509804, 0.5058824, 0.2980392)
        /// </summary>
        public static readonly Color Coffee = new(0.6509804f, 0.5058824f, 0.2980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2862745, 0.5176471, 0.7215686)
        /// </summary>
        public static readonly Color CoolBlue = new(0.2862745f, 0.5176471f, 0.7215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2, 0.7215686, 0.3921569)
        /// </summary>
        public static readonly Color CoolGreen = new(0.2f, 0.7215686f, 0.3921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5843138, 0.6392157, 0.6509804)
        /// </summary>
        public static readonly Color CoolGrey = new(0.5843138f, 0.6392157f, 0.6509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7137255, 0.3882353, 0.145098)
        /// </summary>
        public static readonly Color Copper = new(0.7137255f, 0.3882353f, 0.145098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.3529412, 0.3137255)
        /// </summary>
        public static readonly Color Coral = new(0.9882353f, 0.3529412f, 0.3137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.3803922, 0.3882353)
        /// </summary>
        public static readonly Color CoralPink = new(1, 0.3803922f, 0.3882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4156863, 0.4745098, 0.9686275)
        /// </summary>
        public static readonly Color Cornflower = new(0.4156863f, 0.4745098f, 0.9686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3176471, 0.4392157, 0.8431373)
        /// </summary>
        public static readonly Color CornflowerBlue = new(0.3176471f, 0.4392157f, 0.8431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6196079, 0, 0.227451)
        /// </summary>
        public static readonly Color Cranberry = new(0.6196079f, 0, 0.227451f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.7607843)
        /// </summary>
        public static readonly Color Cream = new(1, 1, 0.7607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.7137255)
        /// </summary>
        public static readonly Color Creme = new(1, 1, 0.7137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5490196, 0, 0.05882353)
        /// </summary>
        public static readonly Color Crimson = new(0.5490196f, 0, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9921569, 0.4705882)
        /// </summary>
        public static readonly Color Custard = new(1, 0.9921569f, 0.4705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 1, 1)
        /// </summary>
        public static readonly Color Cyan = new(0, 1, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.8745098, 0.03137255)
        /// </summary>
        public static readonly Color Dandelion = new(0.9960784f, 0.8745098f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1058824, 0.1411765, 0.1921569)
        /// </summary>
        public static readonly Color Dark = new(0.1058824f, 0.1411765f, 0.1921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 0.4117647, 0.4196078)
        /// </summary>
        public static readonly Color DarkAqua = new(0.01960784f, 0.4117647f, 0.4196078f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.4509804, 0.4431373)
        /// </summary>
        public static readonly Color DarkAquamarine = new(0.003921569f, 0.4509804f, 0.4431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.5764706, 0.3843137)
        /// </summary>
        public static readonly Color DarkBeige = new(0.6745098f, 0.5764706f, 0.3843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.01176471, 0.3568628)
        /// </summary>
        public static readonly Color DarkBlue = new(0, 0.01176471f, 0.3568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.3215686, 0.2862745)
        /// </summary>
        public static readonly Color DarkBlueGreen = new(0, 0.3215686f, 0.2862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1215686, 0.2313726, 0.3019608)
        /// </summary>
        public static readonly Color DarkBlueGrey = new(0.1215686f, 0.2313726f, 0.3019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2039216, 0.1098039, 0.007843138)
        /// </summary>
        public static readonly Color DarkBrown = new(0.2039216f, 0.1098039f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8117647, 0.3215686, 0.3058824)
        /// </summary>
        public static readonly Color DarkCoral = new(0.8117647f, 0.3215686f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9529412, 0.6039216)
        /// </summary>
        public static readonly Color DarkCream = new(1, 0.9529412f, 0.6039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03921569, 0.5333334, 0.5411765)
        /// </summary>
        public static readonly Color DarkCyan = new(0.03921569f, 0.5333334f, 0.5411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.1764706, 0.01568628)
        /// </summary>
        public static readonly Color DarkForestGreen = new(0, 0.1764706f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.02745098, 0.3490196)
        /// </summary>
        public static readonly Color DarkFuchsia = new(0.6156863f, 0.02745098f, 0.3490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7098039, 0.5803922, 0.0627451)
        /// </summary>
        public static readonly Color DarkGold = new(0.7098039f, 0.5803922f, 0.0627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2196078, 0.5019608, 0.01568628)
        /// </summary>
        public static readonly Color DarkGrassGreen = new(0.2196078f, 0.5019608f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.2078431, 0)
        /// </summary>
        public static readonly Color DarkGreen = new(0.01176471f, 0.2078431f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1215686, 0.3882353, 0.3411765)
        /// </summary>
        public static readonly Color DarkGreenBlue = new(0.1215686f, 0.3882353f, 0.3411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2117647, 0.2156863, 0.2156863)
        /// </summary>
        public static readonly Color DarkGrey = new(0.2117647f, 0.2156863f, 0.2156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1607843, 0.2745098, 0.3568628)
        /// </summary>
        public static readonly Color DarkGreyBlue = new(0.1607843f, 0.2745098f, 0.3568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8509804, 0.003921569, 0.4)
        /// </summary>
        public static readonly Color DarkHotPink = new(0.8509804f, 0.003921569f, 0.4f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1215686, 0.03529412, 0.3294118)
        /// </summary>
        public static readonly Color DarkIndigo = new(0.1215686f, 0.03529412f, 0.3294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6078432, 0.5607843, 0.3333333)
        /// </summary>
        public static readonly Color DarkKhaki = new(0.6078432f, 0.5607843f, 0.3333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5215687, 0.4039216, 0.5960785)
        /// </summary>
        public static readonly Color DarkLavender = new(0.5215687f, 0.4039216f, 0.5960785f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6117647, 0.427451, 0.6470588)
        /// </summary>
        public static readonly Color DarkLilac = new(0.6117647f, 0.427451f, 0.6470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5176471, 0.7176471, 0.003921569)
        /// </summary>
        public static readonly Color DarkLime = new(0.5176471f, 0.7176471f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4941176, 0.7411765, 0.003921569)
        /// </summary>
        public static readonly Color DarkLimeGreen = new(0.4941176f, 0.7411765f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5882353, 0, 0.3372549)
        /// </summary>
        public static readonly Color DarkMagenta = new(0.5882353f, 0, 0.3372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2352941, 0, 0.03137255)
        /// </summary>
        public static readonly Color DarkMaroon = new(0.2352941f, 0, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.2980392, 0.3843137)
        /// </summary>
        public static readonly Color DarkMauve = new(0.5294118f, 0.2980392f, 0.3843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.282353, 0.7529412, 0.4470588)
        /// </summary>
        public static readonly Color DarkMint = new(0.282353f, 0.7529412f, 0.4470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1254902, 0.7529412, 0.4509804)
        /// </summary>
        public static readonly Color DarkMintGreen = new(0.1254902f, 0.7529412f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.5372549, 0.01960784)
        /// </summary>
        public static readonly Color DarkMustard = new(0.6588235f, 0.5372549f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.01568628, 0.2078431)
        /// </summary>
        public static readonly Color DarkNavy = new(0, 0.01568628f, 0.2078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.007843138, 0.1803922)
        /// </summary>
        public static readonly Color DarkNavyBlue = new(0, 0.007843138f, 0.1803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2156863, 0.2431373, 0.007843138)
        /// </summary>
        public static readonly Color DarkOlive = new(0.2156863f, 0.2431373f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2352941, 0.3019608, 0.01176471)
        /// </summary>
        public static readonly Color DarkOliveGreen = new(0.2352941f, 0.3019608f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7764706, 0.3176471, 0.007843138)
        /// </summary>
        public static readonly Color DarkOrange = new(0.7764706f, 0.3176471f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3372549, 0.682353, 0.3411765)
        /// </summary>
        public static readonly Color DarkPastelGreen = new(0.3372549f, 0.682353f, 0.3411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8705882, 0.4941176, 0.3647059)
        /// </summary>
        public static readonly Color DarkPeach = new(0.8705882f, 0.4941176f, 0.3647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4, 0.372549, 0.8196079)
        /// </summary>
        public static readonly Color DarkPeriwinkle = new(0.4f, 0.372549f, 0.8196079f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.254902, 0.4196078)
        /// </summary>
        public static readonly Color DarkPink = new(0.7960784f, 0.254902f, 0.4196078f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2470588, 0.003921569, 0.172549)
        /// </summary>
        public static readonly Color DarkPlum = new(0.2470588f, 0.003921569f, 0.172549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2078431, 0.02352941, 0.2431373)
        /// </summary>
        public static readonly Color DarkPurple = new(0.2078431f, 0.02352941f, 0.2431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5176471, 0, 0)
        /// </summary>
        public static readonly Color DarkRed = new(0.5176471f, 0, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7098039, 0.282353, 0.3647059)
        /// </summary>
        public static readonly Color DarkRose = new(0.7098039f, 0.282353f, 0.3647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.02352941, 0.4352941)
        /// </summary>
        public static readonly Color DarkRoyalBlue = new(0.007843138f, 0.02352941f, 0.4352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3490196, 0.5215687, 0.3372549)
        /// </summary>
        public static readonly Color DarkSage = new(0.3490196f, 0.5215687f, 0.3372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.3529412, 0.3254902)
        /// </summary>
        public static readonly Color DarkSalmon = new(0.7843137f, 0.3529412f, 0.3254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.5607843, 0.3490196)
        /// </summary>
        public static readonly Color DarkSand = new(0.6588235f, 0.5607843f, 0.3490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.06666667, 0.5294118, 0.3647059)
        /// </summary>
        public static readonly Color DarkSeaGreen = new(0.06666667f, 0.5294118f, 0.3647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1215686, 0.7098039, 0.4784314)
        /// </summary>
        public static readonly Color DarkSeafoam = new(0.1215686f, 0.7098039f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2431373, 0.6862745, 0.4627451)
        /// </summary>
        public static readonly Color DarkSeafoamGreen = new(0.2431373f, 0.6862745f, 0.4627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2666667, 0.5568628, 0.8941177)
        /// </summary>
        public static readonly Color DarkSkyBlue = new(0.2666667f, 0.5568628f, 0.8941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1294118, 0.2784314, 0.3803922)
        /// </summary>
        public static readonly Color DarkSlateBlue = new(0.1294118f, 0.2784314f, 0.3803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6862745, 0.5333334, 0.2901961)
        /// </summary>
        public static readonly Color DarkTan = new(0.6862745f, 0.5333334f, 0.2901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.4078431, 0.3058824)
        /// </summary>
        public static readonly Color DarkTaupe = new(0.4980392f, 0.4078431f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.3019608, 0.3058824)
        /// </summary>
        public static readonly Color DarkTeal = new(0.003921569f, 0.3019608f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.3607843, 0.3529412)
        /// </summary>
        public static readonly Color DarkTurquoise = new(0.01568628f, 0.3607843f, 0.3529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2039216, 0.003921569, 0.2470588)
        /// </summary>
        public static readonly Color DarkViolet = new(0.2039216f, 0.003921569f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8352941, 0.7137255, 0.03921569)
        /// </summary>
        public static readonly Color DarkYellow = new(0.8352941f, 0.7137255f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4470588, 0.5607843, 0.007843138)
        /// </summary>
        public static readonly Color DarkYellowGreen = new(0.4470588f, 0.5607843f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.02745098, 0.3921569)
        /// </summary>
        public static readonly Color Darkblue = new(0.01176471f, 0.02745098f, 0.3921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 0.2862745, 0.02745098)
        /// </summary>
        public static readonly Color Darkgreen = new(0.01960784f, 0.2862745f, 0.02745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.254902, 0.509804)
        /// </summary>
        public static readonly Color DarkishBlue = new(0.003921569f, 0.254902f, 0.509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1568628, 0.4862745, 0.2156863)
        /// </summary>
        public static readonly Color DarkishGreen = new(0.1568628f, 0.4862745f, 0.2156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.854902, 0.2745098, 0.4901961)
        /// </summary>
        public static readonly Color DarkishPink = new(0.854902f, 0.2745098f, 0.4901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.09803922, 0.4509804)
        /// </summary>
        public static readonly Color DarkishPurple = new(0.4588235f, 0.09803922f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6627451, 0.01176471, 0.03137255)
        /// </summary>
        public static readonly Color DarkishRed = new(0.6627451f, 0.01176471f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03137255, 0.4705882, 0.4980392)
        /// </summary>
        public static readonly Color DeepAqua = new(0.03137255f, 0.4705882f, 0.4980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.007843138, 0.4509804)
        /// </summary>
        public static readonly Color DeepBlue = new(0.01568628f, 0.007843138f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.254902, 0.007843138, 0)
        /// </summary>
        public static readonly Color DeepBrown = new(0.254902f, 0.007843138f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.3490196, 0.05882353)
        /// </summary>
        public static readonly Color DeepGreen = new(0.007843138f, 0.3490196f, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5529412, 0.3686275, 0.7176471)
        /// </summary>
        public static readonly Color DeepLavender = new(0.5529412f, 0.3686275f, 0.7176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5882353, 0.4313726, 0.7411765)
        /// </summary>
        public static readonly Color DeepLilac = new(0.5882353f, 0.4313726f, 0.7411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.627451, 0.007843138, 0.3607843)
        /// </summary>
        public static readonly Color DeepMagenta = new(0.627451f, 0.007843138f, 0.3607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8627451, 0.3019608, 0.003921569)
        /// </summary>
        public static readonly Color DeepOrange = new(0.8627451f, 0.3019608f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.003921569, 0.3843137)
        /// </summary>
        public static readonly Color DeepPink = new(0.7960784f, 0.003921569f, 0.3843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2117647, 0.003921569, 0.2470588)
        /// </summary>
        public static readonly Color DeepPurple = new(0.2117647f, 0.003921569f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6039216, 0.007843138, 0)
        /// </summary>
        public static readonly Color DeepRed = new(0.6039216f, 0.007843138f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.2784314, 0.4039216)
        /// </summary>
        public static readonly Color DeepRose = new(0.7803922f, 0.2784314f, 0.4039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.3294118, 0.509804)
        /// </summary>
        public static readonly Color DeepSeaBlue = new(0.003921569f, 0.3294118f, 0.509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.05098039, 0.4588235, 0.972549)
        /// </summary>
        public static readonly Color DeepSkyBlue = new(0.05098039f, 0.4588235f, 0.972549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.3333333, 0.3529412)
        /// </summary>
        public static readonly Color DeepTeal = new(0, 0.3333333f, 0.3529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.4509804, 0.454902)
        /// </summary>
        public static readonly Color DeepTurquoise = new(0.003921569f, 0.4509804f, 0.454902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2862745, 0.02352941, 0.282353)
        /// </summary>
        public static readonly Color DeepViolet = new(0.2862745f, 0.02352941f, 0.282353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2313726, 0.3882353, 0.5490196)
        /// </summary>
        public static readonly Color Denim = new(0.2313726f, 0.3882353f, 0.5490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2313726, 0.3568628, 0.572549)
        /// </summary>
        public static readonly Color DenimBlue = new(0.2313726f, 0.3568628f, 0.572549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8, 0.6784314, 0.3764706)
        /// </summary>
        public static readonly Color Desert = new(0.8f, 0.6784314f, 0.3764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6235294, 0.5137255, 0.01176471)
        /// </summary>
        public static readonly Color Diarrhea = new(0.6235294f, 0.5137255f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5411765, 0.4313726, 0.2705882)
        /// </summary>
        public static readonly Color Dirt = new(0.5411765f, 0.4313726f, 0.2705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5137255, 0.3960784, 0.2235294)
        /// </summary>
        public static readonly Color DirtBrown = new(0.5137255f, 0.3960784f, 0.2235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2470588, 0.509804, 0.6156863)
        /// </summary>
        public static readonly Color DirtyBlue = new(0.2470588f, 0.509804f, 0.6156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4, 0.4941176, 0.172549)
        /// </summary>
        public static readonly Color DirtyGreen = new(0.4f, 0.4941176f, 0.172549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.4627451, 0.02352941)
        /// </summary>
        public static readonly Color DirtyOrange = new(0.7843137f, 0.4627451f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 0.4823529, 0.5019608)
        /// </summary>
        public static readonly Color DirtyPink = new(0.7921569f, 0.4823529f, 0.5019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4509804, 0.2901961, 0.3960784)
        /// </summary>
        public static readonly Color DirtyPurple = new(0.4509804f, 0.2901961f, 0.3960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8039216, 0.772549, 0.03921569)
        /// </summary>
        public static readonly Color DirtyYellow = new(0.8039216f, 0.772549f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2431373, 0.509804, 0.9882353)
        /// </summary>
        public static readonly Color DodgerBlue = new(0.2431373f, 0.509804f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.5137255, 0.2666667)
        /// </summary>
        public static readonly Color Drab = new(0.509804f, 0.5137255f, 0.2666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.454902, 0.5843138, 0.3176471)
        /// </summary>
        public static readonly Color DrabGreen = new(0.454902f, 0.5843138f, 0.3176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2941177, 0.003921569, 0.003921569)
        /// </summary>
        public static readonly Color DriedBlood = new(0.2941177f, 0.003921569f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7647059, 0.9843137, 0.9568627)
        /// </summary>
        public static readonly Color DuckEggBlue = new(0.7647059f, 0.9843137f, 0.9568627f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2862745, 0.4588235, 0.6117647)
        /// </summary>
        public static readonly Color DullBlue = new(0.2862745f, 0.4588235f, 0.6117647f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.4313726, 0.2941177)
        /// </summary>
        public static readonly Color DullBrown = new(0.5294118f, 0.4313726f, 0.2941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.454902, 0.6509804, 0.3843137)
        /// </summary>
        public static readonly Color DullGreen = new(0.454902f, 0.6509804f, 0.3843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8470588, 0.5254902, 0.2313726)
        /// </summary>
        public static readonly Color DullOrange = new(0.8470588f, 0.5254902f, 0.2313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8352941, 0.5254902, 0.6156863)
        /// </summary>
        public static readonly Color DullPink = new(0.8352941f, 0.5254902f, 0.6156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5176471, 0.3490196, 0.4941176)
        /// </summary>
        public static readonly Color DullPurple = new(0.5176471f, 0.3490196f, 0.4941176f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7333333, 0.2470588, 0.2470588)
        /// </summary>
        public static readonly Color DullRed = new(0.7333333f, 0.2470588f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.372549, 0.6196079, 0.5607843)
        /// </summary>
        public static readonly Color DullTeal = new(0.372549f, 0.6196079f, 0.5607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9333333, 0.8627451, 0.3568628)
        /// </summary>
        public static readonly Color DullYellow = new(0.9333333f, 0.8627451f, 0.3568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3058824, 0.3294118, 0.5058824)
        /// </summary>
        public static readonly Color Dusk = new(0.3058824f, 0.3294118f, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1490196, 0.3254902, 0.5529412)
        /// </summary>
        public static readonly Color DuskBlue = new(0.1490196f, 0.3254902f, 0.5529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2784314, 0.372549, 0.5803922)
        /// </summary>
        public static readonly Color DuskyBlue = new(0.2784314f, 0.372549f, 0.5803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8, 0.4784314, 0.5450981)
        /// </summary>
        public static readonly Color DuskyPink = new(0.8f, 0.4784314f, 0.5450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5372549, 0.3568628, 0.4823529)
        /// </summary>
        public static readonly Color DuskyPurple = new(0.5372549f, 0.3568628f, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7294118, 0.4078431, 0.4509804)
        /// </summary>
        public static readonly Color DuskyRose = new(0.7294118f, 0.4078431f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.6, 0.4313726)
        /// </summary>
        public static readonly Color Dust = new(0.6980392f, 0.6f, 0.4313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3529412, 0.5254902, 0.6784314)
        /// </summary>
        public static readonly Color DustyBlue = new(0.3529412f, 0.5254902f, 0.6784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4627451, 0.6627451, 0.4509804)
        /// </summary>
        public static readonly Color DustyGreen = new(0.4627451f, 0.6627451f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.5254902, 0.6588235)
        /// </summary>
        public static readonly Color DustyLavender = new(0.6745098f, 0.5254902f, 0.6588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9411765, 0.5137255, 0.227451)
        /// </summary>
        public static readonly Color DustyOrange = new(0.9411765f, 0.5137255f, 0.227451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8352941, 0.5411765, 0.5803922)
        /// </summary>
        public static readonly Color DustyPink = new(0.8352941f, 0.5411765f, 0.5803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.372549, 0.5294118)
        /// </summary>
        public static readonly Color DustyPurple = new(0.509804f, 0.372549f, 0.5294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7254902, 0.282353, 0.3058824)
        /// </summary>
        public static readonly Color DustyRed = new(0.7254902f, 0.282353f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7529412, 0.4509804, 0.4784314)
        /// </summary>
        public static readonly Color DustyRose = new(0.7529412f, 0.4509804f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2980392, 0.5647059, 0.5215687)
        /// </summary>
        public static readonly Color DustyTeal = new(0.2980392f, 0.5647059f, 0.5215687f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6352941, 0.3960784, 0.2431373)
        /// </summary>
        public static readonly Color Earth = new(0.6352941f, 0.3960784f, 0.2431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5490196, 0.9921569, 0.4941176)
        /// </summary>
        public static readonly Color EasterGreen = new(0.5490196f, 0.9921569f, 0.4941176f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7529412, 0.4431373, 0.9960784)
        /// </summary>
        public static readonly Color EasterPurple = new(0.7529412f, 0.4431373f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 1, 0.7921569)
        /// </summary>
        public static readonly Color Ecru = new(0.9960784f, 1, 0.7921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9882353, 0.7686275)
        /// </summary>
        public static readonly Color EggShell = new(1, 0.9882353f, 0.7686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2196078, 0.03137255, 0.2078431)
        /// </summary>
        public static readonly Color Eggplant = new(0.2196078f, 0.03137255f, 0.2078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2627451, 0.01960784, 0.254902)
        /// </summary>
        public static readonly Color EggplantPurple = new(0.2627451f, 0.01960784f, 0.254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.8313726)
        /// </summary>
        public static readonly Color Eggshell = new(1, 1, 0.8313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7686275, 1, 0.9686275)
        /// </summary>
        public static readonly Color EggshellBlue = new(0.7686275f, 1, 0.9686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.3215686, 1)
        /// </summary>
        public static readonly Color ElectricBlue = new(0.02352941f, 0.3215686f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1294118, 0.9882353, 0.05098039)
        /// </summary>
        public static readonly Color ElectricGreen = new(0.1294118f, 0.9882353f, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 1, 0.01568628)
        /// </summary>
        public static readonly Color ElectricLime = new(0.6588235f, 1, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.01568628, 0.5647059)
        /// </summary>
        public static readonly Color ElectricPink = new(1, 0.01568628f, 0.5647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6666667, 0.1372549, 1)
        /// </summary>
        public static readonly Color ElectricPurple = new(0.6666667f, 0.1372549f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.627451, 0.2862745)
        /// </summary>
        public static readonly Color Emerald = new(0.003921569f, 0.627451f, 0.2862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.5607843, 0.1176471)
        /// </summary>
        public static readonly Color EmeraldGreen = new(0.007843138f, 0.5607843f, 0.1176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 0.2784314, 0.1647059)
        /// </summary>
        public static readonly Color Evergreen = new(0.01960784f, 0.2784314f, 0.1647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.5490196, 0.7333333)
        /// </summary>
        public static readonly Color FadedBlue = new(0.3960784f, 0.5490196f, 0.7333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0.6980392, 0.454902)
        /// </summary>
        public static readonly Color FadedGreen = new(0.4823529f, 0.6980392f, 0.454902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9411765, 0.5803922, 0.3019608)
        /// </summary>
        public static readonly Color FadedOrange = new(0.9411765f, 0.5803922f, 0.3019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8705882, 0.6156863, 0.6745098)
        /// </summary>
        public static readonly Color FadedPink = new(0.8705882f, 0.6156863f, 0.6745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5686275, 0.4313726, 0.6)
        /// </summary>
        public static readonly Color FadedPurple = new(0.5686275f, 0.4313726f, 0.6f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.827451, 0.2862745, 0.3058824)
        /// </summary>
        public static readonly Color FadedRed = new(0.827451f, 0.2862745f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 1, 0.4980392)
        /// </summary>
        public static readonly Color FadedYellow = new(0.9960784f, 1, 0.4980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8117647, 0.6862745, 0.4823529)
        /// </summary>
        public static readonly Color Fawn = new(0.8117647f, 0.6862745f, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3882353, 0.6627451, 0.3137255)
        /// </summary>
        public static readonly Color Fern = new(0.3882353f, 0.6627451f, 0.3137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3294118, 0.5529412, 0.2666667)
        /// </summary>
        public static readonly Color FernGreen = new(0.3294118f, 0.5529412f, 0.2666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0, 0.007843138)
        /// </summary>
        public static readonly Color FireEngineRed = new(0.9960784f, 0, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2352941, 0.4509804, 0.6588235)
        /// </summary>
        public static readonly Color FlatBlue = new(0.2352941f, 0.4509804f, 0.6588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4117647, 0.6156863, 0.2980392)
        /// </summary>
        public static readonly Color FlatGreen = new(0.4117647f, 0.6156863f, 0.2980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03137255, 1, 0.03137255)
        /// </summary>
        public static readonly Color FluorescentGreen = new(0.03137255f, 1, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03921569, 1, 0.007843138)
        /// </summary>
        public static readonly Color FluroGreen = new(0.03921569f, 1, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5647059, 0.9921569, 0.6627451)
        /// </summary>
        public static readonly Color FoamGreen = new(0.5647059f, 0.9921569f, 0.6627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04313726, 0.3333333, 0.03529412)
        /// </summary>
        public static readonly Color Forest = new(0.04313726f, 0.3333333f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.2784314, 0.04705882)
        /// </summary>
        public static readonly Color ForestGreen = new(0.02352941f, 0.2784314f, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.08235294, 0.2666667, 0.02352941)
        /// </summary>
        public static readonly Color ForrestGreen = new(0.08235294f, 0.2666667f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2627451, 0.4196078, 0.6784314)
        /// </summary>
        public static readonly Color FrenchBlue = new(0.2627451f, 0.4196078f, 0.6784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4117647, 0.8470588, 0.3098039)
        /// </summary>
        public static readonly Color FreshGreen = new(0.4117647f, 0.8470588f, 0.3098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.345098, 0.7372549, 0.03137255)
        /// </summary>
        public static readonly Color FrogGreen = new(0.345098f, 0.7372549f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9294118, 0.05098039, 0.8509804)
        /// </summary>
        public static readonly Color Fuchsia = new(0.9294118f, 0.05098039f, 0.8509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8588235, 0.7058824, 0.04705882)
        /// </summary>
        public static readonly Color Gold = new(0.8588235f, 0.7058824f, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9607843, 0.7490196, 0.01176471)
        /// </summary>
        public static readonly Color Golden = new(0.9607843f, 0.7490196f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.4784314, 0.003921569)
        /// </summary>
        public static readonly Color GoldenBrown = new(0.6980392f, 0.4784314f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9764706, 0.7372549, 0.03137255)
        /// </summary>
        public static readonly Color GoldenRod = new(0.9764706f, 0.7372549f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.7764706, 0.08235294)
        /// </summary>
        public static readonly Color GoldenYellow = new(0.9960784f, 0.7764706f, 0.08235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9803922, 0.7607843, 0.01960784)
        /// </summary>
        public static readonly Color Goldenrod = new(0.9803922f, 0.7607843f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4235294, 0.2039216, 0.3803922)
        /// </summary>
        public static readonly Color Grape = new(0.4235294f, 0.2039216f, 0.3803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3647059, 0.07843138, 0.3176471)
        /// </summary>
        public static readonly Color GrapePurple = new(0.3647059f, 0.07843138f, 0.3176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.3490196, 0.3372549)
        /// </summary>
        public static readonly Color Grapefruit = new(0.9921569f, 0.3490196f, 0.3372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3607843, 0.6745098, 0.1764706)
        /// </summary>
        public static readonly Color Grass = new(0.3607843f, 0.6745098f, 0.1764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2470588, 0.6078432, 0.04313726)
        /// </summary>
        public static readonly Color GrassGreen = new(0.2470588f, 0.6078432f, 0.04313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.254902, 0.6117647, 0.01176471)
        /// </summary>
        public static readonly Color GrassyGreen = new(0.254902f, 0.6117647f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.08235294, 0.6901961, 0.1019608)
        /// </summary>
        public static readonly Color Green = new(0.08235294f, 0.6901961f, 0.1019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3686275, 0.8627451, 0.1215686)
        /// </summary>
        public static readonly Color GreenApple = new(0.3686275f, 0.8627451f, 0.1215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.7058824, 0.5450981)
        /// </summary>
        public static readonly Color GreenBlue = new(0.02352941f, 0.7058824f, 0.5450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3294118, 0.3058824, 0.01176471)
        /// </summary>
        public static readonly Color GreenBrown = new(0.3294118f, 0.3058824f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4666667, 0.572549, 0.4352941)
        /// </summary>
        public static readonly Color GreenGrey = new(0.4666667f, 0.572549f, 0.4352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04705882, 0.7098039, 0.4666667)
        /// </summary>
        public static readonly Color GreenTeal = new(0.04705882f, 0.7098039f, 0.4666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 1, 0.1529412)
        /// </summary>
        public static readonly Color GreenYellow = new(0.7882353f, 1, 0.1529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.7529412, 0.5529412)
        /// </summary>
        public static readonly Color Green_Blue = new(0.003921569f, 0.7529412f, 0.5529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7098039, 0.8078431, 0.03137255)
        /// </summary>
        public static readonly Color Green_Yellow = new(0.7098039f, 0.8078431f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1372549, 0.7686275, 0.5450981)
        /// </summary>
        public static readonly Color Greenblue = new(0.1372549f, 0.7686275f, 0.5450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2509804, 0.6392157, 0.4078431)
        /// </summary>
        public static readonly Color Greenish = new(0.2509804f, 0.6392157f, 0.4078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 0.8196079, 0.4745098)
        /// </summary>
        public static readonly Color GreenishBeige = new(0.7882353f, 0.8196079f, 0.4745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04313726, 0.5450981, 0.5294118)
        /// </summary>
        public static readonly Color GreenishBlue = new(0.04313726f, 0.5450981f, 0.5294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4117647, 0.3803922, 0.07058824)
        /// </summary>
        public static readonly Color GreenishBrown = new(0.4117647f, 0.3803922f, 0.07058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1647059, 0.9960784, 0.7176471)
        /// </summary>
        public static readonly Color GreenishCyan = new(0.1647059f, 0.9960784f, 0.7176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5882353, 0.682353, 0.5529412)
        /// </summary>
        public static readonly Color GreenishGrey = new(0.5882353f, 0.682353f, 0.5529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7372549, 0.7960784, 0.4784314)
        /// </summary>
        public static readonly Color GreenishTan = new(0.7372549f, 0.7960784f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1960784, 0.7490196, 0.5176471)
        /// </summary>
        public static readonly Color GreenishTeal = new(0.1960784f, 0.7490196f, 0.5176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.9843137, 0.6901961)
        /// </summary>
        public static readonly Color GreenishTurquoise = new(0, 0.9843137f, 0.6901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8039216, 0.9921569, 0.007843138)
        /// </summary>
        public static readonly Color GreenishYellow = new(0.8039216f, 0.9921569f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2588235, 0.7019608, 0.5843138)
        /// </summary>
        public static readonly Color GreenyBlue = new(0.2588235f, 0.7019608f, 0.5843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4117647, 0.3764706, 0.02352941)
        /// </summary>
        public static readonly Color GreenyBrown = new(0.4117647f, 0.3764706f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4941176, 0.627451, 0.4784314)
        /// </summary>
        public static readonly Color GreenyGrey = new(0.4941176f, 0.627451f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7764706, 0.972549, 0.03137255)
        /// </summary>
        public static readonly Color GreenyYellow = new(0.7764706f, 0.972549f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.572549, 0.5843138, 0.5686275)
        /// </summary>
        public static readonly Color Grey = new(0.572549f, 0.5843138f, 0.5686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4196078, 0.5450981, 0.6431373)
        /// </summary>
        public static readonly Color GreyBlue = new(0.4196078f, 0.5450981f, 0.6431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.4392157, 0.3254902)
        /// </summary>
        public static readonly Color GreyBrown = new(0.4980392f, 0.4392157f, 0.3254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4705882, 0.6078432, 0.4509804)
        /// </summary>
        public static readonly Color GreyGreen = new(0.4705882f, 0.6078432f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7647059, 0.5647059, 0.6078432)
        /// </summary>
        public static readonly Color GreyPink = new(0.7647059f, 0.5647059f, 0.6078432f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.427451, 0.5490196)
        /// </summary>
        public static readonly Color GreyPurple = new(0.509804f, 0.427451f, 0.5490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3686275, 0.6078432, 0.5411765)
        /// </summary>
        public static readonly Color GreyTeal = new(0.3686275f, 0.6078432f, 0.5411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3921569, 0.4901961, 0.5568628)
        /// </summary>
        public static readonly Color Grey_Blue = new(0.3921569f, 0.4901961f, 0.5568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5254902, 0.6313726, 0.4901961)
        /// </summary>
        public static readonly Color Grey_Green = new(0.5254902f, 0.6313726f, 0.4901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4666667, 0.6313726, 0.7098039)
        /// </summary>
        public static readonly Color Greyblue = new(0.4666667f, 0.6313726f, 0.7098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.6431373, 0.5843138)
        /// </summary>
        public static readonly Color Greyish = new(0.6588235f, 0.6431373f, 0.5843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3686275, 0.5058824, 0.6156863)
        /// </summary>
        public static readonly Color GreyishBlue = new(0.3686275f, 0.5058824f, 0.6156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4784314, 0.4156863, 0.3098039)
        /// </summary>
        public static readonly Color GreyishBrown = new(0.4784314f, 0.4156863f, 0.3098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.6509804, 0.4901961)
        /// </summary>
        public static readonly Color GreyishGreen = new(0.509804f, 0.6509804f, 0.4901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.5529412, 0.5803922)
        /// </summary>
        public static readonly Color GreyishPink = new(0.7843137f, 0.5529412f, 0.5803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5333334, 0.4431373, 0.5686275)
        /// </summary>
        public static readonly Color GreyishPurple = new(0.5333334f, 0.4431373f, 0.5686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4431373, 0.6235294, 0.5686275)
        /// </summary>
        public static readonly Color GreyishTeal = new(0.4431373f, 0.6235294f, 0.5686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.627451, 0.7490196, 0.08627451)
        /// </summary>
        public static readonly Color GrossGreen = new(0.627451f, 0.7490196f, 0.08627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3254902, 0.3843137, 0.4039216)
        /// </summary>
        public static readonly Color Gunmetal = new(0.3254902f, 0.3843137f, 0.4039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5568628, 0.4627451, 0.09411765)
        /// </summary>
        public static readonly Color Hazel = new(0.5568628f, 0.4627451f, 0.09411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6431373, 0.5176471, 0.6745098)
        /// </summary>
        public static readonly Color Heather = new(0.6431373f, 0.5176471f, 0.6745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8509804, 0.3098039, 0.9607843)
        /// </summary>
        public static readonly Color Heliotrope = new(0.8509804f, 0.3098039f, 0.9607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1058824, 0.9882353, 0.02352941)
        /// </summary>
        public static readonly Color HighlighterGreen = new(0.1058824f, 0.9882353f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6078432, 0.8980392, 0.6666667)
        /// </summary>
        public static readonly Color HospitalGreen = new(0.6078432f, 0.8980392f, 0.6666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.145098, 1, 0.1607843)
        /// </summary>
        public static readonly Color HotGreen = new(0.145098f, 1, 0.1607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9607843, 0.01568628, 0.7882353)
        /// </summary>
        public static readonly Color HotMagenta = new(0.9607843f, 0.01568628f, 0.7882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.007843138, 0.5529412)
        /// </summary>
        public static readonly Color HotPink = new(1, 0.007843138f, 0.5529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0, 0.9607843)
        /// </summary>
        public static readonly Color HotPurple = new(0.7960784f, 0, 0.9607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04313726, 0.2509804, 0.03137255)
        /// </summary>
        public static readonly Color HunterGreen = new(0.04313726f, 0.2509804f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8392157, 1, 0.9803922)
        /// </summary>
        public static readonly Color Ice = new(0.8392157f, 1, 0.9803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8431373, 1, 0.9960784)
        /// </summary>
        public static readonly Color IceBlue = new(0.8431373f, 1, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.682353, 0.1333333)
        /// </summary>
        public static readonly Color IckyGreen = new(0.5607843f, 0.682353f, 0.1333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5215687, 0.05490196, 0.01568628)
        /// </summary>
        public static readonly Color IndianRed = new(0.5215687f, 0.05490196f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2196078, 0.007843138, 0.509804)
        /// </summary>
        public static readonly Color Indigo = new(0.2196078f, 0.007843138f, 0.509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.227451, 0.09411765, 0.6941177)
        /// </summary>
        public static readonly Color IndigoBlue = new(0.227451f, 0.09411765f, 0.6941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3843137, 0.345098, 0.7686275)
        /// </summary>
        public static readonly Color Iris = new(0.3843137f, 0.345098f, 0.7686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.5843138, 0.1607843)
        /// </summary>
        public static readonly Color IrishGreen = new(0.003921569f, 0.5843138f, 0.1607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.7960784)
        /// </summary>
        public static readonly Color Ivory = new(1, 1, 0.7960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1215686, 0.654902, 0.454902)
        /// </summary>
        public static readonly Color Jade = new(0.1215686f, 0.654902f, 0.454902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1686275, 0.6862745, 0.4156863)
        /// </summary>
        public static readonly Color JadeGreen = new(0.1686275f, 0.6862745f, 0.4156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.509804, 0.2627451)
        /// </summary>
        public static readonly Color JungleGreen = new(0.01568628f, 0.509804f, 0.2627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.5764706, 0.2156863)
        /// </summary>
        public static readonly Color KelleyGreen = new(0, 0.5764706f, 0.2156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.6705883, 0.1803922)
        /// </summary>
        public static readonly Color KellyGreen = new(0.007843138f, 0.6705883f, 0.1803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3607843, 0.6980392, 0)
        /// </summary>
        public static readonly Color KermitGreen = new(0.3607843f, 0.6980392f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.682353, 1, 0.4313726)
        /// </summary>
        public static readonly Color KeyLime = new(0.682353f, 1, 0.4313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6666667, 0.6509804, 0.3843137)
        /// </summary>
        public static readonly Color Khaki = new(0.6666667f, 0.6509804f, 0.3843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4470588, 0.5254902, 0.2235294)
        /// </summary>
        public static readonly Color KhakiGreen = new(0.4470588f, 0.5254902f, 0.2235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6117647, 0.9372549, 0.2627451)
        /// </summary>
        public static readonly Color Kiwi = new(0.6117647f, 0.9372549f, 0.2627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5568628, 0.8980392, 0.2470588)
        /// </summary>
        public static readonly Color KiwiGreen = new(0.5568628f, 0.8980392f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.6235294, 0.9372549)
        /// </summary>
        public static readonly Color Lavender = new(0.7803922f, 0.6235294f, 0.9372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5450981, 0.5333334, 0.972549)
        /// </summary>
        public static readonly Color LavenderBlue = new(0.5450981f, 0.5333334f, 0.972549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8666667, 0.5215687, 0.8431373)
        /// </summary>
        public static readonly Color LavenderPink = new(0.8666667f, 0.5215687f, 0.8431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3019608, 0.6431373, 0.03529412)
        /// </summary>
        public static readonly Color LawnGreen = new(0.3019608f, 0.6431373f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4431373, 0.6666667, 0.2039216)
        /// </summary>
        public static readonly Color Leaf = new(0.4431373f, 0.6666667f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3607843, 0.6627451, 0.01568628)
        /// </summary>
        public static readonly Color LeafGreen = new(0.3607843f, 0.6627451f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3176471, 0.7176471, 0.2313726)
        /// </summary>
        public static readonly Color LeafyGreen = new(0.3176471f, 0.7176471f, 0.2313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.454902, 0.2039216)
        /// </summary>
        public static readonly Color Leather = new(0.6745098f, 0.454902f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 1, 0.3215686)
        /// </summary>
        public static readonly Color Lemon = new(0.9921569f, 1, 0.3215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6784314, 0.972549, 0.007843138)
        /// </summary>
        public static readonly Color LemonGreen = new(0.6784314f, 0.972549f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7490196, 0.9960784, 0.1568628)
        /// </summary>
        public static readonly Color LemonLime = new(0.7490196f, 0.9960784f, 0.1568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 1, 0.2196078)
        /// </summary>
        public static readonly Color LemonYellow = new(0.9921569f, 1, 0.2196078f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.7137255, 0.4823529)
        /// </summary>
        public static readonly Color Lichen = new(0.5607843f, 0.7137255f, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5490196, 1, 0.8588235)
        /// </summary>
        public static readonly Color LightAqua = new(0.5490196f, 1, 0.8588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0.9921569, 0.7803922)
        /// </summary>
        public static readonly Color LightAquamarine = new(0.4823529f, 0.9921569f, 0.7803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9960784, 0.7137255)
        /// </summary>
        public static readonly Color LightBeige = new(1, 0.9960784f, 0.7137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5843138, 0.8156863, 0.9882353)
        /// </summary>
        public static readonly Color LightBlue = new(0.5843138f, 0.8156863f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4941176, 0.9843137, 0.7019608)
        /// </summary>
        public static readonly Color LightBlueGreen = new(0.4941176f, 0.9843137f, 0.7019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7176471, 0.7882353, 0.8862745)
        /// </summary>
        public static readonly Color LightBlueGrey = new(0.7176471f, 0.7882353f, 0.8862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4627451, 0.9921569, 0.6588235)
        /// </summary>
        public static readonly Color LightBluishGreen = new(0.4627451f, 0.9921569f, 0.6588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3254902, 0.9960784, 0.3607843)
        /// </summary>
        public static readonly Color LightBrightGreen = new(0.3254902f, 0.9960784f, 0.3607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6784314, 0.5058824, 0.3137255)
        /// </summary>
        public static readonly Color LightBrown = new(0.6784314f, 0.5058824f, 0.3137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.254902, 0.3568628)
        /// </summary>
        public static readonly Color LightBurgundy = new(0.6588235f, 0.254902f, 0.3568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 1, 0.9882353)
        /// </summary>
        public static readonly Color LightCyan = new(0.6745098f, 1, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5372549, 0.2705882, 0.5215687)
        /// </summary>
        public static readonly Color LightEggplant = new(0.5372549f, 0.2705882f, 0.5215687f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3098039, 0.5686275, 0.3254902)
        /// </summary>
        public static readonly Color LightForestGreen = new(0.3098039f, 0.5686275f, 0.3254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.8627451, 0.3607843)
        /// </summary>
        public static readonly Color LightGold = new(0.9921569f, 0.8627451f, 0.3607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6039216, 0.9686275, 0.3921569)
        /// </summary>
        public static readonly Color LightGrassGreen = new(0.6039216f, 0.9686275f, 0.3921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5882353, 0.9764706, 0.4823529)
        /// </summary>
        public static readonly Color LightGreen = new(0.5882353f, 0.9764706f, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3372549, 0.9882353, 0.6352941)
        /// </summary>
        public static readonly Color LightGreenBlue = new(0.3372549f, 0.9882353f, 0.6352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3882353, 0.9686275, 0.7058824)
        /// </summary>
        public static readonly Color LightGreenishBlue = new(0.3882353f, 0.9686275f, 0.7058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8470588, 0.8627451, 0.8392157)
        /// </summary>
        public static readonly Color LightGrey = new(0.8470588f, 0.8627451f, 0.8392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.7372549, 0.8313726)
        /// </summary>
        public static readonly Color LightGreyBlue = new(0.6156863f, 0.7372549f, 0.8313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7176471, 0.8823529, 0.6313726)
        /// </summary>
        public static readonly Color LightGreyGreen = new(0.7176471f, 0.8823529f, 0.6313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.427451, 0.3529412, 0.8117647)
        /// </summary>
        public static readonly Color LightIndigo = new(0.427451f, 0.3529412f, 0.8117647f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9019608, 0.9490196, 0.6352941)
        /// </summary>
        public static readonly Color LightKhaki = new(0.9019608f, 0.9490196f, 0.6352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9372549, 0.7529412, 0.9960784)
        /// </summary>
        public static readonly Color LightLavendar = new(0.9372549f, 0.7529412f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8745098, 0.772549, 0.9960784)
        /// </summary>
        public static readonly Color LightLavender = new(0.8745098f, 0.772549f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 1, 0.9843137)
        /// </summary>
        public static readonly Color LightLightBlue = new(0.7921569f, 1, 0.9843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 1, 0.6901961)
        /// </summary>
        public static readonly Color LightLightGreen = new(0.7843137f, 1, 0.6901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9294118, 0.7843137, 1)
        /// </summary>
        public static readonly Color LightLilac = new(0.9294118f, 0.7843137f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.682353, 0.9921569, 0.4235294)
        /// </summary>
        public static readonly Color LightLime = new(0.682353f, 0.9921569f, 0.4235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7254902, 1, 0.4)
        /// </summary>
        public static readonly Color LightLimeGreen = new(0.7254902f, 1, 0.4f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9803922, 0.372549, 0.9686275)
        /// </summary>
        public static readonly Color LightMagenta = new(0.9803922f, 0.372549f, 0.9686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6352941, 0.282353, 0.3411765)
        /// </summary>
        public static readonly Color LightMaroon = new(0.6352941f, 0.282353f, 0.3411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7607843, 0.572549, 0.6313726)
        /// </summary>
        public static readonly Color LightMauve = new(0.7607843f, 0.572549f, 0.6313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7137255, 1, 0.7333333)
        /// </summary>
        public static readonly Color LightMint = new(0.7137255f, 1, 0.7333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6509804, 0.9843137, 0.6980392)
        /// </summary>
        public static readonly Color LightMintGreen = new(0.6509804f, 0.9843137f, 0.6980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6509804, 0.7843137, 0.4588235)
        /// </summary>
        public static readonly Color LightMossGreen = new(0.6509804f, 0.7843137f, 0.4588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9686275, 0.8352941, 0.3764706)
        /// </summary>
        public static readonly Color LightMustard = new(0.9686275f, 0.8352941f, 0.3764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.08235294, 0.3137255, 0.5176471)
        /// </summary>
        public static readonly Color LightNavy = new(0.08235294f, 0.3137255f, 0.5176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1803922, 0.3529412, 0.5333334)
        /// </summary>
        public static readonly Color LightNavyBlue = new(0.1803922f, 0.3529412f, 0.5333334f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3058824, 0.9921569, 0.3294118)
        /// </summary>
        public static readonly Color LightNeonGreen = new(0.3058824f, 0.9921569f, 0.3294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.7490196, 0.4117647)
        /// </summary>
        public static readonly Color LightOlive = new(0.6745098f, 0.7490196f, 0.4117647f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6431373, 0.7450981, 0.3607843)
        /// </summary>
        public static readonly Color LightOliveGreen = new(0.6431373f, 0.7450981f, 0.3607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.6666667, 0.282353)
        /// </summary>
        public static readonly Color LightOrange = new(0.9921569f, 0.6666667f, 0.282353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.9843137, 0.6470588)
        /// </summary>
        public static readonly Color LightPastelGreen = new(0.6980392f, 0.9843137f, 0.6470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7686275, 0.9960784, 0.509804)
        /// </summary>
        public static readonly Color LightPeaGreen = new(0.7686275f, 0.9960784f, 0.509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.8470588, 0.6941177)
        /// </summary>
        public static readonly Color LightPeach = new(1, 0.8470588f, 0.6941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7568628, 0.7764706, 0.9882353)
        /// </summary>
        public static readonly Color LightPeriwinkle = new(0.7568628f, 0.7764706f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.8196079, 0.8745098)
        /// </summary>
        public static readonly Color LightPink = new(1, 0.8196079f, 0.8745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.3411765, 0.5137255)
        /// </summary>
        public static readonly Color LightPlum = new(0.6156863f, 0.3411765f, 0.5137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7490196, 0.4666667, 0.9647059)
        /// </summary>
        public static readonly Color LightPurple = new(0.7490196f, 0.4666667f, 0.9647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.2784314, 0.2980392)
        /// </summary>
        public static readonly Color LightRed = new(1, 0.2784314f, 0.2980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.772549, 0.7960784)
        /// </summary>
        public static readonly Color LightRose = new(1, 0.772549f, 0.7960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.227451, 0.1803922, 0.9960784)
        /// </summary>
        public static readonly Color LightRoyalBlue = new(0.227451f, 0.1803922f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7372549, 0.9254902, 0.6745098)
        /// </summary>
        public static readonly Color LightSage = new(0.7372549f, 0.9254902f, 0.6745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.6627451, 0.5764706)
        /// </summary>
        public static readonly Color LightSalmon = new(0.9960784f, 0.6627451f, 0.5764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5960785, 0.9647059, 0.6901961)
        /// </summary>
        public static readonly Color LightSeaGreen = new(0.5960785f, 0.9647059f, 0.6901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.627451, 0.9960784, 0.7490196)
        /// </summary>
        public static readonly Color LightSeafoam = new(0.627451f, 0.9960784f, 0.7490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.654902, 1, 0.7098039)
        /// </summary>
        public static readonly Color LightSeafoamGreen = new(0.654902f, 1, 0.7098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7764706, 0.9882353, 1)
        /// </summary>
        public static readonly Color LightSkyBlue = new(0.7764706f, 0.9882353f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9843137, 0.9333333, 0.6745098)
        /// </summary>
        public static readonly Color LightTan = new(0.9843137f, 0.9333333f, 0.6745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5647059, 0.8941177, 0.7568628)
        /// </summary>
        public static readonly Color LightTeal = new(0.5647059f, 0.8941177f, 0.7568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4941176, 0.9568627, 0.8)
        /// </summary>
        public static readonly Color LightTurquoise = new(0.4941176f, 0.9568627f, 0.8f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7019608, 0.4352941, 0.9647059)
        /// </summary>
        public static readonly Color LightUrple = new(0.7019608f, 0.4352941f, 0.9647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8392157, 0.7058824, 0.9882353)
        /// </summary>
        public static readonly Color LightViolet = new(0.8392157f, 0.7058824f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9960784, 0.4784314)
        /// </summary>
        public static readonly Color LightYellow = new(1, 0.9960784f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8, 0.9921569, 0.4980392)
        /// </summary>
        public static readonly Color LightYellowGreen = new(0.8f, 0.9921569f, 0.4980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7607843, 1, 0.5372549)
        /// </summary>
        public static readonly Color LightYellowishGreen = new(0.7607843f, 1, 0.5372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0.7843137, 0.9647059)
        /// </summary>
        public static readonly Color Lightblue = new(0.4823529f, 0.7843137f, 0.9647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.9921569, 0.3882353)
        /// </summary>
        public static readonly Color LighterGreen = new(0.4588235f, 0.9921569f, 0.3882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0.3529412, 0.9568627)
        /// </summary>
        public static readonly Color LighterPurple = new(0.6470588f, 0.3529412f, 0.9568627f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4627451, 1, 0.4823529)
        /// </summary>
        public static readonly Color Lightgreen = new(0.4627451f, 1, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2392157, 0.4784314, 0.9921569)
        /// </summary>
        public static readonly Color LightishBlue = new(0.2392157f, 0.4784314f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3803922, 0.8823529, 0.3764706)
        /// </summary>
        public static readonly Color LightishGreen = new(0.3803922f, 0.8823529f, 0.3764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0.3215686, 0.9019608)
        /// </summary>
        public static readonly Color LightishPurple = new(0.6470588f, 0.3215686f, 0.9019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.1843137, 0.2901961)
        /// </summary>
        public static readonly Color LightishRed = new(0.9960784f, 0.1843137f, 0.2901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8078431, 0.6352941, 0.9921569)
        /// </summary>
        public static readonly Color Lilac = new(0.8078431f, 0.6352941f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7686275, 0.5568628, 0.9921569)
        /// </summary>
        public static readonly Color Liliac = new(0.7686275f, 0.5568628f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6666667, 1, 0.1960784)
        /// </summary>
        public static readonly Color Lime = new(0.6666667f, 1, 0.1960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5372549, 0.9960784, 0.01960784)
        /// </summary>
        public static readonly Color LimeGreen = new(0.5372549f, 0.9960784f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8156863, 0.9960784, 0.1137255)
        /// </summary>
        public static readonly Color LimeYellow = new(0.8156863f, 0.9960784f, 0.1137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8352941, 0.09019608, 0.3058824)
        /// </summary>
        public static readonly Color Lipstick = new(0.8352941f, 0.09019608f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7529412, 0.007843138, 0.1843137)
        /// </summary>
        public static readonly Color LipstickRed = new(0.7529412f, 0.007843138f, 0.1843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9372549, 0.7058824, 0.2078431)
        /// </summary>
        public static readonly Color MacaroniAndCheese = new(0.9372549f, 0.7058824f, 0.2078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7607843, 0, 0.4705882)
        /// </summary>
        public static readonly Color Magenta = new(0.7607843f, 0, 0.4705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2901961, 0.003921569, 0)
        /// </summary>
        public static readonly Color Mahogany = new(0.2901961f, 0.003921569f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9568627, 0.8156863, 0.3294118)
        /// </summary>
        public static readonly Color Maize = new(0.9568627f, 0.8156863f, 0.3294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6509804, 0.1686275)
        /// </summary>
        public static readonly Color Mango = new(1, 0.6509804f, 0.1686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9803922, 0.5254902)
        /// </summary>
        public static readonly Color Manilla = new(1, 0.9803922f, 0.5254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.7529412, 0.02352941)
        /// </summary>
        public static readonly Color Marigold = new(0.9882353f, 0.7529412f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.1803922, 0.3764706)
        /// </summary>
        public static readonly Color Marine = new(0.01568628f, 0.1803922f, 0.3764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.2196078, 0.4156863)
        /// </summary>
        public static readonly Color MarineBlue = new(0.003921569f, 0.2196078f, 0.4156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0, 0.1294118)
        /// </summary>
        public static readonly Color Maroon = new(0.3960784f, 0, 0.1294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.682353, 0.4431373, 0.5058824)
        /// </summary>
        public static readonly Color Mauve = new(0.682353f, 0.4431373f, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.172549, 0.4352941, 0.7333333)
        /// </summary>
        public static readonly Color MediumBlue = new(0.172549f, 0.4352941f, 0.7333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.3176471, 0.07058824)
        /// </summary>
        public static readonly Color MediumBrown = new(0.4980392f, 0.3176471f, 0.07058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2235294, 0.6784314, 0.282353)
        /// </summary>
        public static readonly Color MediumGreen = new(0.2235294f, 0.6784314f, 0.282353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4901961, 0.4980392, 0.4862745)
        /// </summary>
        public static readonly Color MediumGrey = new(0.4901961f, 0.4980392f, 0.4862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9529412, 0.3803922, 0.5882353)
        /// </summary>
        public static readonly Color MediumPink = new(0.9529412f, 0.3803922f, 0.5882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6196079, 0.2627451, 0.6352941)
        /// </summary>
        public static readonly Color MediumPurple = new(0.6196079f, 0.2627451f, 0.6352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4705882, 0.3333333)
        /// </summary>
        public static readonly Color Melon = new(1, 0.4705882f, 0.3333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4509804, 0, 0.2235294)
        /// </summary>
        public static readonly Color Merlot = new(0.4509804f, 0, 0.2235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3098039, 0.4509804, 0.5568628)
        /// </summary>
        public static readonly Color MetallicBlue = new(0.3098039f, 0.4509804f, 0.5568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1529412, 0.4156863, 0.7019608)
        /// </summary>
        public static readonly Color MidBlue = new(0.1529412f, 0.4156863f, 0.7019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3137255, 0.654902, 0.2784314)
        /// </summary>
        public static readonly Color MidGreen = new(0.3137255f, 0.654902f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.003921569, 0.1764706)
        /// </summary>
        public static readonly Color Midnight = new(0.01176471f, 0.003921569f, 0.1764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0, 0.2078431)
        /// </summary>
        public static readonly Color MidnightBlue = new(0.007843138f, 0, 0.2078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1568628, 0.003921569, 0.2156863)
        /// </summary>
        public static readonly Color MidnightPurple = new(0.1568628f, 0.003921569f, 0.2156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4, 0.4862745, 0.2431373)
        /// </summary>
        public static readonly Color MilitaryGreen = new(0.4f, 0.4862745f, 0.2431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.3058824, 0.1176471)
        /// </summary>
        public static readonly Color MilkChocolate = new(0.4980392f, 0.3058824f, 0.1176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6235294, 0.9960784, 0.6901961)
        /// </summary>
        public static readonly Color Mint = new(0.6235294f, 0.9960784f, 0.6901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 1, 0.6235294)
        /// </summary>
        public static readonly Color MintGreen = new(0.5607843f, 1, 0.6235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04313726, 0.9686275, 0.4901961)
        /// </summary>
        public static readonly Color MintyGreen = new(0.04313726f, 0.9686275f, 0.4901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.4627451, 0.3176471)
        /// </summary>
        public static readonly Color Mocha = new(0.6156863f, 0.4627451f, 0.3176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4627451, 0.6, 0.345098)
        /// </summary>
        public static readonly Color Moss = new(0.4627451f, 0.6f, 0.345098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.5450981, 0.2196078)
        /// </summary>
        public static readonly Color MossGreen = new(0.3960784f, 0.5450981f, 0.2196078f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3882353, 0.5450981, 0.1529412)
        /// </summary>
        public static readonly Color MossyGreen = new(0.3882353f, 0.5450981f, 0.1529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4509804, 0.3607843, 0.07058824)
        /// </summary>
        public static readonly Color Mud = new(0.4509804f, 0.3607843f, 0.07058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3764706, 0.2745098, 0.05882353)
        /// </summary>
        public static readonly Color MudBrown = new(0.3764706f, 0.2745098f, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3764706, 0.4, 0.007843138)
        /// </summary>
        public static readonly Color MudGreen = new(0.3764706f, 0.4f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5333334, 0.4078431, 0.02352941)
        /// </summary>
        public static readonly Color MuddyBrown = new(0.5333334f, 0.4078431f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.454902, 0.1960784)
        /// </summary>
        public static readonly Color MuddyGreen = new(0.3960784f, 0.454902f, 0.1960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7490196, 0.6745098, 0.01960784)
        /// </summary>
        public static readonly Color MuddyYellow = new(0.7490196f, 0.6745098f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.572549, 0.03921569, 0.3058824)
        /// </summary>
        public static readonly Color Mulberry = new(0.572549f, 0.03921569f, 0.3058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4235294, 0.4784314, 0.05490196)
        /// </summary>
        public static readonly Color MurkyGreen = new(0.4235294f, 0.4784314f, 0.05490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7294118, 0.6196079, 0.5333334)
        /// </summary>
        public static readonly Color Mushroom = new(0.7294118f, 0.6196079f, 0.5333334f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8078431, 0.7019608, 0.003921569)
        /// </summary>
        public static readonly Color Mustard = new(0.8078431f, 0.7019608f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.4941176, 0.01568628)
        /// </summary>
        public static readonly Color MustardBrown = new(0.6745098f, 0.4941176f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.7098039, 0.01568628)
        /// </summary>
        public static readonly Color MustardGreen = new(0.6588235f, 0.7098039f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8235294, 0.7411765, 0.03921569)
        /// </summary>
        public static readonly Color MustardYellow = new(0.8235294f, 0.7411765f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2313726, 0.4431373, 0.6235294)
        /// </summary>
        public static readonly Color MutedBlue = new(0.2313726f, 0.4431373f, 0.6235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.372549, 0.627451, 0.3215686)
        /// </summary>
        public static readonly Color MutedGreen = new(0.372549f, 0.627451f, 0.3215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8196079, 0.4627451, 0.5607843)
        /// </summary>
        public static readonly Color MutedPink = new(0.8196079f, 0.4627451f, 0.5607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5019608, 0.3568628, 0.5294118)
        /// </summary>
        public static readonly Color MutedPurple = new(0.5019608f, 0.3568628f, 0.5294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4392157, 0.6980392, 0.2470588)
        /// </summary>
        public static readonly Color NastyGreen = new(0.4392157f, 0.6980392f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.08235294, 0.2431373)
        /// </summary>
        public static readonly Color Navy = new(0.003921569f, 0.08235294f, 0.2431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.06666667, 0.2745098)
        /// </summary>
        public static readonly Color NavyBlue = new(0, 0.06666667f, 0.2745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2078431, 0.3254902, 0.03921569)
        /// </summary>
        public static readonly Color NavyGreen = new(0.2078431f, 0.3254902f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.8509804, 1)
        /// </summary>
        public static readonly Color NeonBlue = new(0.01568628f, 0.8509804f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04705882, 1, 0.04705882)
        /// </summary>
        public static readonly Color NeonGreen = new(0.04705882f, 1, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.003921569, 0.6039216)
        /// </summary>
        public static readonly Color NeonPink = new(0.9960784f, 0.003921569f, 0.6039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7372549, 0.07450981, 0.9960784)
        /// </summary>
        public static readonly Color NeonPurple = new(0.7372549f, 0.07450981f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.02745098, 0.227451)
        /// </summary>
        public static readonly Color NeonRed = new(1, 0.02745098f, 0.227451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8117647, 1, 0.01568628)
        /// </summary>
        public static readonly Color NeonYellow = new(0.8117647f, 1, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.0627451, 0.4784314, 0.6901961)
        /// </summary>
        public static readonly Color NiceBlue = new(0.0627451f, 0.4784314f, 0.6901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.01176471, 0.282353)
        /// </summary>
        public static readonly Color NightBlue = new(0.01568628f, 0.01176471f, 0.282353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.4823529, 0.572549)
        /// </summary>
        public static readonly Color Ocean = new(0.003921569f, 0.4823529f, 0.572549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.4431373, 0.6117647)
        /// </summary>
        public static readonly Color OceanBlue = new(0.01176471f, 0.4431373f, 0.6117647f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2392157, 0.6, 0.4509804)
        /// </summary>
        public static readonly Color OceanGreen = new(0.2392157f, 0.6f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7490196, 0.6078432, 0.04705882)
        /// </summary>
        public static readonly Color Ocher = new(0.7490196f, 0.6078432f, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7490196, 0.5647059, 0.01960784)
        /// </summary>
        public static readonly Color Ochre = new(0.7490196f, 0.5647059f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7764706, 0.6117647, 0.01568628)
        /// </summary>
        public static readonly Color Ocre = new(0.7764706f, 0.6117647f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3372549, 0.5176471, 0.682353)
        /// </summary>
        public static readonly Color OffBlue = new(0.3372549f, 0.5176471f, 0.682353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4196078, 0.6392157, 0.3254902)
        /// </summary>
        public static readonly Color OffGreen = new(0.4196078f, 0.6392157f, 0.3254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.8941177)
        /// </summary>
        public static readonly Color OffWhite = new(1, 1, 0.8941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.945098, 0.9529412, 0.2470588)
        /// </summary>
        public static readonly Color OffYellow = new(0.945098f, 0.9529412f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.4745098, 0.5254902)
        /// </summary>
        public static readonly Color OldPink = new(0.7803922f, 0.4745098f, 0.5254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.4980392, 0.5372549)
        /// </summary>
        public static readonly Color OldRose = new(0.7843137f, 0.4980392f, 0.5372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4313726, 0.4588235, 0.05490196)
        /// </summary>
        public static readonly Color Olive = new(0.4313726f, 0.4588235f, 0.05490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3921569, 0.3294118, 0.01176471)
        /// </summary>
        public static readonly Color OliveBrown = new(0.3921569f, 0.3294118f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4352941, 0.4627451, 0.1960784)
        /// </summary>
        public static readonly Color OliveDrab = new(0.4352941f, 0.4627451f, 0.1960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4039216, 0.4784314, 0.01568628)
        /// </summary>
        public static readonly Color OliveGreen = new(0.4039216f, 0.4784314f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7607843, 0.7176471, 0.03529412)
        /// </summary>
        public static readonly Color OliveYellow = new(0.7607843f, 0.7176471f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9764706, 0.4509804, 0.02352941)
        /// </summary>
        public static readonly Color Orange = new(0.9764706f, 0.4509804f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.3921569, 0)
        /// </summary>
        public static readonly Color OrangeBrown = new(0.7450981f, 0.3921569f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4352941, 0.3215686)
        /// </summary>
        public static readonly Color OrangePink = new(1, 0.4352941f, 0.3215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.254902, 0.1176471)
        /// </summary>
        public static readonly Color OrangeRed = new(0.9921569f, 0.254902f, 0.1176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6784314, 0.003921569)
        /// </summary>
        public static readonly Color OrangeYellow = new(1, 0.6784314f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.5529412, 0.2862745)
        /// </summary>
        public static readonly Color Orangeish = new(0.9921569f, 0.5529412f, 0.2862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.2588235, 0.05882353)
        /// </summary>
        public static readonly Color Orangered = new(0.9960784f, 0.2588235f, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 0.3764706, 0.007843138)
        /// </summary>
        public static readonly Color OrangeyBrown = new(0.6941177f, 0.3764706f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9803922, 0.2588235, 0.1411765)
        /// </summary>
        public static readonly Color OrangeyRed = new(0.9803922f, 0.2588235f, 0.1411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.7254902, 0.08235294)
        /// </summary>
        public static readonly Color OrangeyYellow = new(0.9921569f, 0.7254902f, 0.08235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.509804, 0.2901961)
        /// </summary>
        public static readonly Color Orangish = new(0.9882353f, 0.509804f, 0.2901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.372549, 0.01176471)
        /// </summary>
        public static readonly Color OrangishBrown = new(0.6980392f, 0.372549f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9568627, 0.2117647, 0.01960784)
        /// </summary>
        public static readonly Color OrangishRed = new(0.9568627f, 0.2117647f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.4588235, 0.7686275)
        /// </summary>
        public static readonly Color Orchid = new(0.7843137f, 0.4588235f, 0.7686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9764706, 0.8156863)
        /// </summary>
        public static readonly Color Pale = new(1, 0.9764706f, 0.8156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7215686, 1, 0.9215686)
        /// </summary>
        public static readonly Color PaleAqua = new(0.7215686f, 1, 0.9215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8156863, 0.9960784, 0.9960784)
        /// </summary>
        public static readonly Color PaleBlue = new(0.8156863f, 0.9960784f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 0.5686275, 0.4313726)
        /// </summary>
        public static readonly Color PaleBrown = new(0.6941177f, 0.5686275f, 0.4313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7176471, 1, 0.9803922)
        /// </summary>
        public static readonly Color PaleCyan = new(0.7176471f, 1, 0.9803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.8705882, 0.4235294)
        /// </summary>
        public static readonly Color PaleGold = new(0.9921569f, 0.8705882f, 0.4235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.9921569, 0.7098039)
        /// </summary>
        public static readonly Color PaleGreen = new(0.7803922f, 0.9921569f, 0.7098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.9921569, 0.9960784)
        /// </summary>
        public static readonly Color PaleGrey = new(0.9921569f, 0.9921569f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9333333, 0.8117647, 0.9960784)
        /// </summary>
        public static readonly Color PaleLavender = new(0.9333333f, 0.8117647f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 0.9882353, 0.6)
        /// </summary>
        public static readonly Color PaleLightGreen = new(0.6941177f, 0.9882353f, 0.6f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8941177, 0.7960784, 1)
        /// </summary>
        public static readonly Color PaleLilac = new(0.8941177f, 0.7960784f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.9921569, 0.4509804)
        /// </summary>
        public static readonly Color PaleLime = new(0.7450981f, 0.9921569f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 1, 0.3960784)
        /// </summary>
        public static readonly Color PaleLimeGreen = new(0.6941177f, 1, 0.3960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8431373, 0.4039216, 0.6784314)
        /// </summary>
        public static readonly Color PaleMagenta = new(0.8431373f, 0.4039216f, 0.6784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.8156863, 0.9882353)
        /// </summary>
        public static readonly Color PaleMauve = new(0.9960784f, 0.8156863f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7254902, 0.8, 0.5058824)
        /// </summary>
        public static readonly Color PaleOlive = new(0.7254902f, 0.8f, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 0.8235294, 0.4823529)
        /// </summary>
        public static readonly Color PaleOliveGreen = new(0.6941177f, 0.8235294f, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.654902, 0.3372549)
        /// </summary>
        public static readonly Color PaleOrange = new(1, 0.654902f, 0.3372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.8980392, 0.6784314)
        /// </summary>
        public static readonly Color PalePeach = new(1, 0.8980392f, 0.6784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.8117647, 0.8627451)
        /// </summary>
        public static readonly Color PalePink = new(1, 0.8117647f, 0.8627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7176471, 0.5647059, 0.8313726)
        /// </summary>
        public static readonly Color PalePurple = new(0.7176471f, 0.5647059f, 0.8313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8509804, 0.3294118, 0.3019608)
        /// </summary>
        public static readonly Color PaleRed = new(0.8509804f, 0.3294118f, 0.3019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.7568628, 0.772549)
        /// </summary>
        public static readonly Color PaleRose = new(0.9921569f, 0.7568628f, 0.772549f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6941177, 0.6039216)
        /// </summary>
        public static readonly Color PaleSalmon = new(1, 0.6941177f, 0.6039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7411765, 0.9647059, 0.9960784)
        /// </summary>
        public static readonly Color PaleSkyBlue = new(0.7411765f, 0.9647059f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.7960784, 0.6980392)
        /// </summary>
        public static readonly Color PaleTeal = new(0.509804f, 0.7960784f, 0.6980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0.9843137, 0.8352941)
        /// </summary>
        public static readonly Color PaleTurquoise = new(0.6470588f, 0.9843137f, 0.8352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8078431, 0.682353, 0.9803922)
        /// </summary>
        public static readonly Color PaleViolet = new(0.8078431f, 0.682353f, 0.9803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.5176471)
        /// </summary>
        public static readonly Color PaleYellow = new(1, 1, 0.5176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.9882353, 0.6862745)
        /// </summary>
        public static readonly Color Parchment = new(0.9960784f, 0.9882353f, 0.6862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6352941, 0.7490196, 0.9960784)
        /// </summary>
        public static readonly Color PastelBlue = new(0.6352941f, 0.7490196f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6901961, 1, 0.6156863)
        /// </summary>
        public static readonly Color PastelGreen = new(0.6901961f, 1, 0.6156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.5882353, 0.3098039)
        /// </summary>
        public static readonly Color PastelOrange = new(1, 0.5882353f, 0.3098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.7294118, 0.8039216)
        /// </summary>
        public static readonly Color PastelPink = new(1, 0.7294118f, 0.8039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 0.627451, 1)
        /// </summary>
        public static readonly Color PastelPurple = new(0.7921569f, 0.627451f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8588235, 0.345098, 0.3372549)
        /// </summary>
        public static readonly Color PastelRed = new(0.8588235f, 0.345098f, 0.3372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9960784, 0.4431373)
        /// </summary>
        public static readonly Color PastelYellow = new(1, 0.9960784f, 0.4431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6431373, 0.7490196, 0.1254902)
        /// </summary>
        public static readonly Color Pea = new(0.6431373f, 0.7490196f, 0.1254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5568628, 0.6705883, 0.07058824)
        /// </summary>
        public static readonly Color PeaGreen = new(0.5568628f, 0.6705883f, 0.07058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.572549, 0.6, 0.003921569)
        /// </summary>
        public static readonly Color PeaSoup = new(0.572549f, 0.6f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5803922, 0.6509804, 0.09019608)
        /// </summary>
        public static readonly Color PeaSoupGreen = new(0.5803922f, 0.6509804f, 0.09019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6901961, 0.4862745)
        /// </summary>
        public static readonly Color Peach = new(1, 0.6901961f, 0.4862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6039216, 0.5411765)
        /// </summary>
        public static readonly Color PeachyPink = new(1, 0.6039216f, 0.5411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.4039216, 0.5843138)
        /// </summary>
        public static readonly Color PeacockBlue = new(0.003921569f, 0.4039216f, 0.5843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.972549, 0.372549)
        /// </summary>
        public static readonly Color Pear = new(0.7960784f, 0.972549f, 0.372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5568628, 0.509804, 0.9960784)
        /// </summary>
        public static readonly Color Periwinkle = new(0.5568628f, 0.509804f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.6, 0.9843137)
        /// </summary>
        public static readonly Color PeriwinkleBlue = new(0.5607843f, 0.6f, 0.9843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.5490196, 0.9058824)
        /// </summary>
        public static readonly Color Perrywinkle = new(0.5607843f, 0.5490196f, 0.9058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.372549, 0.4156863)
        /// </summary>
        public static readonly Color Petrol = new(0, 0.372549f, 0.4156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9058824, 0.5568628, 0.6470588)
        /// </summary>
        public static readonly Color PigPink = new(0.9058824f, 0.5568628f, 0.6470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1686275, 0.3647059, 0.2039216)
        /// </summary>
        public static readonly Color Pine = new(0.1686275f, 0.3647059f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03921569, 0.282353, 0.1176471)
        /// </summary>
        public static readonly Color PineGreen = new(0.03921569f, 0.282353f, 0.1176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.5058824, 0.7529412)
        /// </summary>
        public static readonly Color Pink = new(1, 0.5058824f, 0.7529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8588235, 0.2941177, 0.854902)
        /// </summary>
        public static readonly Color PinkPurple = new(0.8588235f, 0.2941177f, 0.854902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9607843, 0.01960784, 0.3098039)
        /// </summary>
        public static readonly Color PinkRed = new(0.9607843f, 0.01960784f, 0.3098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9372549, 0.1137255, 0.9058824)
        /// </summary>
        public static readonly Color Pink_Purple = new(0.9372549f, 0.1137255f, 0.9058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8313726, 0.4156863, 0.4941176)
        /// </summary>
        public static readonly Color Pinkish = new(0.8313726f, 0.4156863f, 0.4941176f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 0.4470588, 0.3803922)
        /// </summary>
        public static readonly Color PinkishBrown = new(0.6941177f, 0.4470588f, 0.3803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.6745098, 0.6627451)
        /// </summary>
        public static readonly Color PinkishGrey = new(0.7843137f, 0.6745098f, 0.6627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4470588, 0.2980392)
        /// </summary>
        public static readonly Color PinkishOrange = new(1, 0.4470588f, 0.2980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8392157, 0.282353, 0.8431373)
        /// </summary>
        public static readonly Color PinkishPurple = new(0.8392157f, 0.282353f, 0.8431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.945098, 0.04705882, 0.2705882)
        /// </summary>
        public static readonly Color PinkishRed = new(0.945098f, 0.04705882f, 0.2705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8509804, 0.6078432, 0.509804)
        /// </summary>
        public static readonly Color PinkishTan = new(0.8509804f, 0.6078432f, 0.509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.5254902, 0.6666667)
        /// </summary>
        public static readonly Color Pinky = new(0.9882353f, 0.5254902f, 0.6666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 0.2980392, 0.7450981)
        /// </summary>
        public static readonly Color PinkyPurple = new(0.7882353f, 0.2980392f, 0.7450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.1490196, 0.2784314)
        /// </summary>
        public static readonly Color PinkyRed = new(0.9882353f, 0.1490196f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8666667, 0.8392157, 0.09411765)
        /// </summary>
        public static readonly Color PissYellow = new(0.8666667f, 0.8392157f, 0.09411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7529412, 0.9803922, 0.5450981)
        /// </summary>
        public static readonly Color Pistachio = new(0.7529412f, 0.9803922f, 0.5450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.345098, 0.05882353, 0.254902)
        /// </summary>
        public static readonly Color Plum = new(0.345098f, 0.05882353f, 0.254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3058824, 0.01960784, 0.3137255)
        /// </summary>
        public static readonly Color PlumPurple = new(0.3058824f, 0.01960784f, 0.3137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2509804, 0.9921569, 0.07843138)
        /// </summary>
        public static readonly Color PoisonGreen = new(0.2509804f, 0.9921569f, 0.07843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5607843, 0.4509804, 0.01176471)
        /// </summary>
        public static readonly Color Poo = new(0.5607843f, 0.4509804f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5333334, 0.372549, 0.003921569)
        /// </summary>
        public static readonly Color PooBrown = new(0.5333334f, 0.372549f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.3686275, 0)
        /// </summary>
        public static readonly Color Poop = new(0.4980392f, 0.3686275f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4784314, 0.3490196, 0.003921569)
        /// </summary>
        public static readonly Color PoopBrown = new(0.4784314f, 0.3490196f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4352941, 0.4862745, 0)
        /// </summary>
        public static readonly Color PoopGreen = new(0.4352941f, 0.4862745f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6941177, 0.8196079, 0.9882353)
        /// </summary>
        public static readonly Color PowderBlue = new(0.6941177f, 0.8196079f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6980392, 0.8156863)
        /// </summary>
        public static readonly Color PowderPink = new(1, 0.6980392f, 0.8156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03137255, 0.01568628, 0.9764706)
        /// </summary>
        public static readonly Color PrimaryBlue = new(0.03137255f, 0.01568628f, 0.9764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.2705882, 0.4666667)
        /// </summary>
        public static readonly Color PrussianBlue = new(0, 0.2705882f, 0.4666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0.4941176, 0.3215686)
        /// </summary>
        public static readonly Color Puce = new(0.6470588f, 0.4941176f, 0.3215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0.6470588, 0.007843138)
        /// </summary>
        public static readonly Color Puke = new(0.6470588f, 0.6470588f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5803922, 0.4666667, 0.02352941)
        /// </summary>
        public static readonly Color PukeBrown = new(0.5803922f, 0.4666667f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6039216, 0.682353, 0.02745098)
        /// </summary>
        public static readonly Color PukeGreen = new(0.6039216f, 0.682353f, 0.02745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7607843, 0.7450981, 0.05490196)
        /// </summary>
        public static readonly Color PukeYellow = new(0.7607843f, 0.7450981f, 0.05490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8823529, 0.4666667, 0.003921569)
        /// </summary>
        public static readonly Color Pumpkin = new(0.8823529f, 0.4666667f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9843137, 0.4901961, 0.02745098)
        /// </summary>
        public static readonly Color PumpkinOrange = new(0.9843137f, 0.4901961f, 0.02745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.01176471, 0.8862745)
        /// </summary>
        public static readonly Color PureBlue = new(0.007843138f, 0.01176471f, 0.8862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4941176, 0.1176471, 0.6117647)
        /// </summary>
        public static readonly Color Purple = new(0.4941176f, 0.1176471f, 0.6117647f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3882353, 0.1764706, 0.9137255)
        /// </summary>
        public static readonly Color PurpleBlue = new(0.3882353f, 0.1764706f, 0.9137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4039216, 0.227451, 0.2470588)
        /// </summary>
        public static readonly Color PurpleBrown = new(0.4039216f, 0.227451f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5254902, 0.4352941, 0.5215687)
        /// </summary>
        public static readonly Color PurpleGrey = new(0.5254902f, 0.4352941f, 0.5215687f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8784314, 0.2470588, 0.8470588)
        /// </summary>
        public static readonly Color PurplePink = new(0.8784314f, 0.2470588f, 0.8470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6, 0.003921569, 0.2784314)
        /// </summary>
        public static readonly Color PurpleRed = new(0.6f, 0.003921569f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3647059, 0.1294118, 0.8156863)
        /// </summary>
        public static readonly Color Purple_Blue = new(0.3647059f, 0.1294118f, 0.8156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8431373, 0.145098, 0.8705882)
        /// </summary>
        public static readonly Color Purple_Pink = new(0.8431373f, 0.145098f, 0.8705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5960785, 0.3372549, 0.5529412)
        /// </summary>
        public static readonly Color Purpleish = new(0.5960785f, 0.3372549f, 0.5529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3803922, 0.2509804, 0.9372549)
        /// </summary>
        public static readonly Color PurpleishBlue = new(0.3803922f, 0.2509804f, 0.9372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8745098, 0.3058824, 0.7843137)
        /// </summary>
        public static readonly Color PurpleishPink = new(0.8745098f, 0.3058824f, 0.7843137f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.3372549, 0.8941177)
        /// </summary>
        public static readonly Color Purpley = new(0.5294118f, 0.3372549f, 0.8941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.372549, 0.2039216, 0.9058824)
        /// </summary>
        public static readonly Color PurpleyBlue = new(0.372549f, 0.2039216f, 0.9058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5803922, 0.4941176, 0.5803922)
        /// </summary>
        public static readonly Color PurpleyGrey = new(0.5803922f, 0.4941176f, 0.5803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.2352941, 0.7254902)
        /// </summary>
        public static readonly Color PurpleyPink = new(0.7843137f, 0.2352941f, 0.7254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5803922, 0.3372549, 0.5490196)
        /// </summary>
        public static readonly Color Purplish = new(0.5803922f, 0.3372549f, 0.5490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3764706, 0.1176471, 0.9764706)
        /// </summary>
        public static readonly Color PurplishBlue = new(0.3764706f, 0.1176471f, 0.9764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4196078, 0.2588235, 0.2784314)
        /// </summary>
        public static readonly Color PurplishBrown = new(0.4196078f, 0.2588235f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4784314, 0.4078431, 0.4980392)
        /// </summary>
        public static readonly Color PurplishGrey = new(0.4784314f, 0.4078431f, 0.4980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8078431, 0.3647059, 0.682353)
        /// </summary>
        public static readonly Color PurplishPink = new(0.8078431f, 0.3647059f, 0.682353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6901961, 0.01960784, 0.2941177)
        /// </summary>
        public static readonly Color PurplishRed = new(0.6901961f, 0.01960784f, 0.2941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5960785, 0.2470588, 0.6980392)
        /// </summary>
        public static readonly Color Purply = new(0.5960785f, 0.2470588f, 0.6980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4, 0.1019608, 0.9333333)
        /// </summary>
        public static readonly Color PurplyBlue = new(0.4f, 0.1019608f, 0.9333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9411765, 0.4588235, 0.9019608)
        /// </summary>
        public static readonly Color PurplyPink = new(0.9411765f, 0.4588235f, 0.9019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.682353, 0.5411765)
        /// </summary>
        public static readonly Color Putty = new(0.7450981f, 0.682353f, 0.5411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.2745098, 0)
        /// </summary>
        public static readonly Color RacingGreen = new(0.003921569f, 0.2745098f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.172549, 0.9803922, 0.1215686)
        /// </summary>
        public static readonly Color RadioactiveGreen = new(0.172549f, 0.9803922f, 0.1215686f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6901961, 0.003921569, 0.2862745)
        /// </summary>
        public static readonly Color Raspberry = new(0.6901961f, 0.003921569f, 0.2862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6039216, 0.3843137, 0)
        /// </summary>
        public static readonly Color RawSienna = new(0.6039216f, 0.3843137f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.654902, 0.3686275, 0.03529412)
        /// </summary>
        public static readonly Color RawUmber = new(0.654902f, 0.3686275f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8313726, 1, 1)
        /// </summary>
        public static readonly Color ReallyLightBlue = new(0.8313726f, 1, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8980392, 0, 0)
        /// </summary>
        public static readonly Color Red = new(0.8980392f, 0, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5450981, 0.1803922, 0.08627451)
        /// </summary>
        public static readonly Color RedBrown = new(0.5450981f, 0.1803922f, 0.08627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.2352941, 0.02352941)
        /// </summary>
        public static readonly Color RedOrange = new(0.9921569f, 0.2352941f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9803922, 0.1647059, 0.3333333)
        /// </summary>
        public static readonly Color RedPink = new(0.9803922f, 0.1647059f, 0.3333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.02745098, 0.2784314)
        /// </summary>
        public static readonly Color RedPurple = new(0.509804f, 0.02745098f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6196079, 0.003921569, 0.4078431)
        /// </summary>
        public static readonly Color RedViolet = new(0.6196079f, 0.003921569f, 0.4078431f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5490196, 0, 0.2039216)
        /// </summary>
        public static readonly Color RedWine = new(0.5490196f, 0, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7686275, 0.2588235, 0.2509804)
        /// </summary>
        public static readonly Color Reddish = new(0.7686275f, 0.2588235f, 0.2509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.1686275, 0.03921569)
        /// </summary>
        public static readonly Color ReddishBrown = new(0.4980392f, 0.1686275f, 0.03921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6, 0.4588235, 0.4392157)
        /// </summary>
        public static readonly Color ReddishGrey = new(0.6f, 0.4588235f, 0.4392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.972549, 0.282353, 0.1098039)
        /// </summary>
        public static readonly Color ReddishOrange = new(0.972549f, 0.282353f, 0.1098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.172549, 0.3294118)
        /// </summary>
        public static readonly Color ReddishPink = new(0.9960784f, 0.172549f, 0.3294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5686275, 0.03529412, 0.3176471)
        /// </summary>
        public static readonly Color ReddishPurple = new(0.5686275f, 0.03529412f, 0.3176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4313726, 0.0627451, 0.01960784)
        /// </summary>
        public static readonly Color ReddyBrown = new(0.4313726f, 0.0627451f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.1058824, 0.9764706)
        /// </summary>
        public static readonly Color RichBlue = new(0.007843138f, 0.1058824f, 0.9764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4470588, 0, 0.345098)
        /// </summary>
        public static readonly Color RichPurple = new(0.4470588f, 0, 0.345098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5411765, 0.945098, 0.9960784)
        /// </summary>
        public static readonly Color RobinEggBlue = new(0.5411765f, 0.945098f, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.427451, 0.9294118, 0.9921569)
        /// </summary>
        public static readonly Color RobinSEgg = new(0.427451f, 0.9294118f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5960785, 0.9372549, 0.9764706)
        /// </summary>
        public static readonly Color RobinSEggBlue = new(0.5960785f, 0.9372549f, 0.9764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.5254902, 0.6431373)
        /// </summary>
        public static readonly Color Rosa = new(0.9960784f, 0.5254902f, 0.6431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8117647, 0.3843137, 0.4588235)
        /// </summary>
        public static readonly Color Rose = new(0.8117647f, 0.3843137f, 0.4588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9686275, 0.5294118, 0.6039216)
        /// </summary>
        public static readonly Color RosePink = new(0.9686275f, 0.5294118f, 0.6039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.003921569, 0.2352941)
        /// </summary>
        public static readonly Color RoseRed = new(0.7450981f, 0.003921569f, 0.2352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9647059, 0.4078431, 0.5568628)
        /// </summary>
        public static readonly Color RosyPink = new(0.9647059f, 0.4078431f, 0.5568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6705883, 0.07058824, 0.2235294)
        /// </summary>
        public static readonly Color Rouge = new(0.6705883f, 0.07058824f, 0.2235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04705882, 0.09019608, 0.5764706)
        /// </summary>
        public static readonly Color Royal = new(0.04705882f, 0.09019608f, 0.5764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01960784, 0.01568628, 0.6666667)
        /// </summary>
        public static readonly Color RoyalBlue = new(0.01960784f, 0.01568628f, 0.6666667f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2941177, 0, 0.4313726)
        /// </summary>
        public static readonly Color RoyalPurple = new(0.2941177f, 0, 0.4313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 0.003921569, 0.2784314)
        /// </summary>
        public static readonly Color Ruby = new(0.7921569f, 0.003921569f, 0.2784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6313726, 0.2235294, 0.01960784)
        /// </summary>
        public static readonly Color Russet = new(0.6313726f, 0.2235294f, 0.01960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.2352941, 0.03529412)
        /// </summary>
        public static readonly Color Rust = new(0.6588235f, 0.2352941f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5450981, 0.1921569, 0.01176471)
        /// </summary>
        public static readonly Color RustBrown = new(0.5450981f, 0.1921569f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7686275, 0.3333333, 0.03137255)
        /// </summary>
        public static readonly Color RustOrange = new(0.7686275f, 0.3333333f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6666667, 0.1529412, 0.01568628)
        /// </summary>
        public static readonly Color RustRed = new(0.6666667f, 0.1529412f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8039216, 0.3490196, 0.03529412)
        /// </summary>
        public static readonly Color RustyOrange = new(0.8039216f, 0.3490196f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6862745, 0.1843137, 0.05098039)
        /// </summary>
        public static readonly Color RustyRed = new(0.6862745f, 0.1843137f, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.6980392, 0.03529412)
        /// </summary>
        public static readonly Color Saffron = new(0.9960784f, 0.6980392f, 0.03529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5294118, 0.682353, 0.4509804)
        /// </summary>
        public static readonly Color Sage = new(0.5294118f, 0.682353f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5333334, 0.7019608, 0.4705882)
        /// </summary>
        public static readonly Color SageGreen = new(0.5333334f, 0.7019608f, 0.4705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.4745098, 0.4235294)
        /// </summary>
        public static readonly Color Salmon = new(1, 0.4745098f, 0.4235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.4823529, 0.4862745)
        /// </summary>
        public static readonly Color SalmonPink = new(0.9960784f, 0.4823529f, 0.4862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8862745, 0.7921569, 0.4627451)
        /// </summary>
        public static readonly Color Sand = new(0.8862745f, 0.7921569f, 0.4627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.6470588, 0.3764706)
        /// </summary>
        public static readonly Color SandBrown = new(0.7960784f, 0.6470588f, 0.3764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.8823529, 0.4)
        /// </summary>
        public static readonly Color SandYellow = new(0.9882353f, 0.8823529f, 0.4f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 0.682353, 0.454902)
        /// </summary>
        public static readonly Color Sandstone = new(0.7882353f, 0.682353f, 0.454902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.945098, 0.854902, 0.4784314)
        /// </summary>
        public static readonly Color Sandy = new(0.945098f, 0.854902f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7686275, 0.6509804, 0.3803922)
        /// </summary>
        public static readonly Color SandyBrown = new(0.7686275f, 0.6509804f, 0.3803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.9333333, 0.4509804)
        /// </summary>
        public static readonly Color SandyYellow = new(0.9921569f, 0.9333333f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3607843, 0.5450981, 0.08235294)
        /// </summary>
        public static readonly Color SapGreen = new(0.3607843f, 0.5450981f, 0.08235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1294118, 0.2196078, 0.6705883)
        /// </summary>
        public static readonly Color Sapphire = new(0.1294118f, 0.2196078f, 0.6705883f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7450981, 0.003921569, 0.09803922)
        /// </summary>
        public static readonly Color Scarlet = new(0.7450981f, 0.003921569f, 0.09803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2352941, 0.6, 0.572549)
        /// </summary>
        public static readonly Color Sea = new(0.2352941f, 0.6f, 0.572549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.454902, 0.5843138)
        /// </summary>
        public static readonly Color SeaBlue = new(0.01568628f, 0.454902f, 0.5843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3254902, 0.9882353, 0.6313726)
        /// </summary>
        public static readonly Color SeaGreen = new(0.3254902f, 0.9882353f, 0.6313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5019608, 0.9764706, 0.6784314)
        /// </summary>
        public static readonly Color Seafoam = new(0.5019608f, 0.9764706f, 0.6784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4705882, 0.8196079, 0.7137255)
        /// </summary>
        public static readonly Color SeafoamBlue = new(0.4705882f, 0.8196079f, 0.7137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4784314, 0.9764706, 0.6705883)
        /// </summary>
        public static readonly Color SeafoamGreen = new(0.4784314f, 0.9764706f, 0.6705883f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.09411765, 0.8196079, 0.4823529)
        /// </summary>
        public static readonly Color Seaweed = new(0.09411765f, 0.8196079f, 0.4823529f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2078431, 0.6784314, 0.4196078)
        /// </summary>
        public static readonly Color SeaweedGreen = new(0.2078431f, 0.6784314f, 0.4196078f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5960785, 0.3686275, 0.1686275)
        /// </summary>
        public static readonly Color Sepia = new(0.5960785f, 0.3686275f, 0.1686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.7058824, 0.2980392)
        /// </summary>
        public static readonly Color Shamrock = new(0.003921569f, 0.7058824f, 0.2980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.7568628, 0.3019608)
        /// </summary>
        public static readonly Color ShamrockGreen = new(0.007843138f, 0.7568628f, 0.3019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4980392, 0.372549, 0)
        /// </summary>
        public static readonly Color Shit = new(0.4980392f, 0.372549f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0.345098, 0.01568628)
        /// </summary>
        public static readonly Color ShitBrown = new(0.4823529f, 0.345098f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.5019608, 0)
        /// </summary>
        public static readonly Color ShitGreen = new(0.4588235f, 0.5019608f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9960784, 0.007843138, 0.6352941)
        /// </summary>
        public static readonly Color ShockingPink = new(0.9960784f, 0.007843138f, 0.6352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.7254902, 0.172549)
        /// </summary>
        public static readonly Color SickGreen = new(0.6156863f, 0.7254902f, 0.172549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5803922, 0.6980392, 0.1098039)
        /// </summary>
        public static readonly Color SicklyGreen = new(0.5803922f, 0.6980392f, 0.1098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8156863, 0.8941177, 0.1607843)
        /// </summary>
        public static readonly Color SicklyYellow = new(0.8156863f, 0.8941177f, 0.1607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6627451, 0.3372549, 0.1176471)
        /// </summary>
        public static readonly Color Sienna = new(0.6627451f, 0.3372549f, 0.1176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.772549, 0.7882353, 0.7803922)
        /// </summary>
        public static readonly Color Silver = new(0.772549f, 0.7882353f, 0.7803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.509804, 0.7921569, 0.9882353)
        /// </summary>
        public static readonly Color Sky = new(0.509804f, 0.7921569f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.7333333, 0.9921569)
        /// </summary>
        public static readonly Color SkyBlue = new(0.4588235f, 0.7333333f, 0.9921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3176471, 0.3960784, 0.4470588)
        /// </summary>
        public static readonly Color Slate = new(0.3176471f, 0.3960784f, 0.4470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3568628, 0.4862745, 0.6)
        /// </summary>
        public static readonly Color SlateBlue = new(0.3568628f, 0.4862745f, 0.6f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.5529412, 0.427451)
        /// </summary>
        public static readonly Color SlateGreen = new(0.3960784f, 0.5529412f, 0.427451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3490196, 0.3960784, 0.427451)
        /// </summary>
        public static readonly Color SlateGrey = new(0.3490196f, 0.3960784f, 0.427451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6, 0.8, 0.01568628)
        /// </summary>
        public static readonly Color SlimeGreen = new(0.6f, 0.8f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6745098, 0.7333333, 0.05098039)
        /// </summary>
        public static readonly Color Snot = new(0.6745098f, 0.7333333f, 0.05098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6156863, 0.7568628, 0)
        /// </summary>
        public static readonly Color SnotGreen = new(0.6156863f, 0.7568628f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3921569, 0.5333334, 0.9176471)
        /// </summary>
        public static readonly Color SoftBlue = new(0.3921569f, 0.5333334f, 0.9176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4352941, 0.7607843, 0.4627451)
        /// </summary>
        public static readonly Color SoftGreen = new(0.4352941f, 0.7607843f, 0.4627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.6901961, 0.7529412)
        /// </summary>
        public static readonly Color SoftPink = new(0.9921569f, 0.6901961f, 0.7529412f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6509804, 0.4352941, 0.7098039)
        /// </summary>
        public static readonly Color SoftPurple = new(0.6509804f, 0.4352941f, 0.7098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1176471, 0.972549, 0.4627451)
        /// </summary>
        public static readonly Color Spearmint = new(0.1176471f, 0.972549f, 0.4627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6627451, 0.9764706, 0.4431373)
        /// </summary>
        public static readonly Color SpringGreen = new(0.6627451f, 0.9764706f, 0.4431373f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03921569, 0.372549, 0.2196078)
        /// </summary>
        public static readonly Color Spruce = new(0.03921569f, 0.372549f, 0.2196078f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9490196, 0.6705883, 0.08235294)
        /// </summary>
        public static readonly Color Squash = new(0.9490196f, 0.6705883f, 0.08235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4509804, 0.5215687, 0.5843138)
        /// </summary>
        public static readonly Color Steel = new(0.4509804f, 0.5215687f, 0.5843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3529412, 0.4901961, 0.6039216)
        /// </summary>
        public static readonly Color SteelBlue = new(0.3529412f, 0.4901961f, 0.6039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4352941, 0.509804, 0.5411765)
        /// </summary>
        public static readonly Color SteelGrey = new(0.4352941f, 0.509804f, 0.5411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6784314, 0.6470588, 0.5294118)
        /// </summary>
        public static readonly Color Stone = new(0.6784314f, 0.6470588f, 0.5294118f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3137255, 0.4823529, 0.6117647)
        /// </summary>
        public static readonly Color StormyBlue = new(0.3137255f, 0.4823529f, 0.6117647f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.9647059, 0.4745098)
        /// </summary>
        public static readonly Color Straw = new(0.9882353f, 0.9647059f, 0.4745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9843137, 0.1607843, 0.2627451)
        /// </summary>
        public static readonly Color Strawberry = new(0.9843137f, 0.1607843f, 0.2627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04705882, 0.02352941, 0.9686275)
        /// </summary>
        public static readonly Color StrongBlue = new(0.04705882f, 0.02352941f, 0.9686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.02745098, 0.5372549)
        /// </summary>
        public static readonly Color StrongPink = new(1, 0.02745098f, 0.5372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.8745098, 0.1333333)
        /// </summary>
        public static readonly Color SunYellow = new(1, 0.8745098f, 0.1333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.772549, 0.07058824)
        /// </summary>
        public static readonly Color Sunflower = new(1, 0.772549f, 0.07058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.854902, 0.01176471)
        /// </summary>
        public static readonly Color SunflowerYellow = new(1, 0.854902f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9764706, 0.09019608)
        /// </summary>
        public static readonly Color SunnyYellow = new(1, 0.9764706f, 0.09019608f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9921569, 0.2156863)
        /// </summary>
        public static readonly Color SunshineYellow = new(1, 0.9921569f, 0.2156863f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4117647, 0.5137255, 0.2235294)
        /// </summary>
        public static readonly Color Swamp = new(0.4117647f, 0.5137255f, 0.2235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.454902, 0.5215687, 0)
        /// </summary>
        public static readonly Color SwampGreen = new(0.454902f, 0.5215687f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8196079, 0.6980392, 0.4352941)
        /// </summary>
        public static readonly Color Tan = new(0.8196079f, 0.6980392f, 0.4352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6705883, 0.4941176, 0.2980392)
        /// </summary>
        public static readonly Color TanBrown = new(0.6705883f, 0.4941176f, 0.2980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6627451, 0.7450981, 0.4392157)
        /// </summary>
        public static readonly Color TanGreen = new(0.6627451f, 0.7450981f, 0.4392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.5803922, 0.03137255)
        /// </summary>
        public static readonly Color Tangerine = new(1, 0.5803922f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7254902, 0.6352941, 0.5058824)
        /// </summary>
        public static readonly Color Taupe = new(0.7254902f, 0.6352941f, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3960784, 0.6705883, 0.4862745)
        /// </summary>
        public static readonly Color Tea = new(0.3960784f, 0.6705883f, 0.4862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7411765, 0.972549, 0.6392157)
        /// </summary>
        public static readonly Color TeaGreen = new(0.7411765f, 0.972549f, 0.6392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.007843138, 0.5764706, 0.5254902)
        /// </summary>
        public static readonly Color Teal = new(0.007843138f, 0.5764706f, 0.5254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.5333334, 0.6235294)
        /// </summary>
        public static readonly Color TealBlue = new(0.003921569f, 0.5333334f, 0.6235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.145098, 0.6392157, 0.4352941)
        /// </summary>
        public static readonly Color TealGreen = new(0.145098f, 0.6392157f, 0.4352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1411765, 0.7372549, 0.6588235)
        /// </summary>
        public static readonly Color Tealish = new(0.1411765f, 0.7372549f, 0.6588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.04705882, 0.8627451, 0.4509804)
        /// </summary>
        public static readonly Color TealishGreen = new(0.04705882f, 0.8627451f, 0.4509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7882353, 0.3921569, 0.2313726)
        /// </summary>
        public static readonly Color TerraCotta = new(0.7882353f, 0.3921569f, 0.2313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.4078431, 0.2627451)
        /// </summary>
        public static readonly Color Terracota = new(0.7960784f, 0.4078431f, 0.2627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7921569, 0.4, 0.254902)
        /// </summary>
        public static readonly Color Terracotta = new(0.7921569f, 0.4f, 0.254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0.9490196, 0.854902)
        /// </summary>
        public static readonly Color TiffanyBlue = new(0.4823529f, 0.9490196f, 0.854902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9372549, 0.2509804, 0.1490196)
        /// </summary>
        public static readonly Color Tomato = new(0.9372549f, 0.2509804f, 0.1490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9254902, 0.1764706, 0.003921569)
        /// </summary>
        public static readonly Color TomatoRed = new(0.9254902f, 0.1764706f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.07450981, 0.7333333, 0.6862745)
        /// </summary>
        public static readonly Color Topaz = new(0.07450981f, 0.7333333f, 0.6862745f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.6745098, 0.4901961)
        /// </summary>
        public static readonly Color Toupe = new(0.7803922f, 0.6745098f, 0.4901961f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3803922, 0.8705882, 0.1647059)
        /// </summary>
        public static readonly Color ToxicGreen = new(0.3803922f, 0.8705882f, 0.1647059f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1647059, 0.4941176, 0.09803922)
        /// </summary>
        public static readonly Color TreeGreen = new(0.1647059f, 0.4941176f, 0.09803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.003921569, 0.05882353, 0.8)
        /// </summary>
        public static readonly Color TrueBlue = new(0.003921569f, 0.05882353f, 0.8f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03137255, 0.5803922, 0.01568628)
        /// </summary>
        public static readonly Color TrueGreen = new(0.03137255f, 0.5803922f, 0.01568628f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.7607843, 0.6745098)
        /// </summary>
        public static readonly Color Turquoise = new(0.02352941f, 0.7607843f, 0.6745098f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.6941177, 0.7686275)
        /// </summary>
        public static readonly Color TurquoiseBlue = new(0.02352941f, 0.6941177f, 0.7686275f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01568628, 0.9568627, 0.5372549)
        /// </summary>
        public static readonly Color TurquoiseGreen = new(0.01568628f, 0.9568627f, 0.5372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.7215686, 0.3098039)
        /// </summary>
        public static readonly Color TurtleGreen = new(0.4588235f, 0.7215686f, 0.3098039f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3058824, 0.3176471, 0.5450981)
        /// </summary>
        public static readonly Color Twilight = new(0.3058824f, 0.3176471f, 0.5450981f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03921569, 0.2627451, 0.4784314)
        /// </summary>
        public static readonly Color TwilightBlue = new(0.03921569f, 0.2627451f, 0.4784314f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1921569, 0.4, 0.5411765)
        /// </summary>
        public static readonly Color UglyBlue = new(0.1921569f, 0.4f, 0.5411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4901961, 0.4431373, 0.01176471)
        /// </summary>
        public static readonly Color UglyBrown = new(0.4901961f, 0.4431373f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4784314, 0.5921569, 0.01176471)
        /// </summary>
        public static readonly Color UglyGreen = new(0.4784314f, 0.5921569f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8039216, 0.4588235, 0.5176471)
        /// </summary>
        public static readonly Color UglyPink = new(0.8039216f, 0.4588235f, 0.5176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6431373, 0.2588235, 0.627451)
        /// </summary>
        public static readonly Color UglyPurple = new(0.6431373f, 0.2588235f, 0.627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8156863, 0.7568628, 0.003921569)
        /// </summary>
        public static readonly Color UglyYellow = new(0.8156863f, 0.7568628f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1254902, 0, 0.6941177)
        /// </summary>
        public static readonly Color Ultramarine = new(0.1254902f, 0, 0.6941177f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.09411765, 0.01960784, 0.8588235)
        /// </summary>
        public static readonly Color UltramarineBlue = new(0.09411765f, 0.01960784f, 0.8588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6980392, 0.3921569, 0)
        /// </summary>
        public static readonly Color Umber = new(0.6980392f, 0.3921569f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4588235, 0.03137255, 0.3176471)
        /// </summary>
        public static readonly Color Velvet = new(0.4588235f, 0.03137255f, 0.3176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9568627, 0.1960784, 0.04705882)
        /// </summary>
        public static readonly Color Vermillion = new(0.9568627f, 0.1960784f, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0, 0.003921569, 0.2)
        /// </summary>
        public static readonly Color VeryDarkBlue = new(0, 0.003921569f, 0.2f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1137255, 0.007843138, 0)
        /// </summary>
        public static readonly Color VeryDarkBrown = new(0.1137255f, 0.007843138f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.02352941, 0.1803922, 0.01176471)
        /// </summary>
        public static readonly Color VeryDarkGreen = new(0.02352941f, 0.1803922f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1647059, 0.003921569, 0.2039216)
        /// </summary>
        public static readonly Color VeryDarkPurple = new(0.1647059f, 0.003921569f, 0.2039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8352941, 1, 1)
        /// </summary>
        public static readonly Color VeryLightBlue = new(0.8352941f, 1, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.827451, 0.7137255, 0.5137255)
        /// </summary>
        public static readonly Color VeryLightBrown = new(0.827451f, 0.7137255f, 0.5137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8196079, 1, 0.7411765)
        /// </summary>
        public static readonly Color VeryLightGreen = new(0.8196079f, 1, 0.7411765f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.9568627, 0.9490196)
        /// </summary>
        public static readonly Color VeryLightPink = new(1, 0.9568627f, 0.9490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9647059, 0.8078431, 0.9882353)
        /// </summary>
        public static readonly Color VeryLightPurple = new(0.9647059f, 0.8078431f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8392157, 1, 0.9960784)
        /// </summary>
        public static readonly Color VeryPaleBlue = new(0.8392157f, 1, 0.9960784f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.8117647, 0.9921569, 0.7372549)
        /// </summary>
        public static readonly Color VeryPaleGreen = new(0.8117647f, 0.9921569f, 0.7372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.01176471, 0.2235294, 0.972549)
        /// </summary>
        public static readonly Color VibrantBlue = new(0.01176471f, 0.2235294f, 0.972549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.03921569, 0.8666667, 0.03137255)
        /// </summary>
        public static readonly Color VibrantGreen = new(0.03921569f, 0.8666667f, 0.03137255f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6784314, 0.01176471, 0.8705882)
        /// </summary>
        public static readonly Color VibrantPurple = new(0.6784314f, 0.01176471f, 0.8705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6039216, 0.05490196, 0.9176471)
        /// </summary>
        public static readonly Color Violet = new(0.6039216f, 0.05490196f, 0.9176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.3176471, 0.03921569, 0.7882353)
        /// </summary>
        public static readonly Color VioletBlue = new(0.3176471f, 0.03921569f, 0.7882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9843137, 0.372549, 0.9882353)
        /// </summary>
        public static readonly Color VioletPink = new(0.9843137f, 0.372549f, 0.9882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6470588, 0, 0.3333333)
        /// </summary>
        public static readonly Color VioletRed = new(0.6470588f, 0, 0.3333333f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1176471, 0.5686275, 0.4039216)
        /// </summary>
        public static readonly Color Viridian = new(0.1176471f, 0.5686275f, 0.4039216f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.08235294, 0.1803922, 1)
        /// </summary>
        public static readonly Color VividBlue = new(0.08235294f, 0.1803922f, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1843137, 0.9372549, 0.0627451)
        /// </summary>
        public static readonly Color VividGreen = new(0.1843137f, 0.9372549f, 0.0627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6, 0, 0.9803922)
        /// </summary>
        public static readonly Color VividPurple = new(0.6f, 0, 0.9803922f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6352941, 0.6431373, 0.08235294)
        /// </summary>
        public static readonly Color Vomit = new(0.6352941f, 0.6431373f, 0.08235294f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5372549, 0.6352941, 0.01176471)
        /// </summary>
        public static readonly Color VomitGreen = new(0.5372549f, 0.6352941f, 0.01176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7803922, 0.7568628, 0.04705882)
        /// </summary>
        public static readonly Color VomitYellow = new(0.7803922f, 0.7568628f, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2941177, 0.3411765, 0.8588235)
        /// </summary>
        public static readonly Color WarmBlue = new(0.2941177f, 0.3411765f, 0.8588235f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5882353, 0.3058824, 0.007843138)
        /// </summary>
        public static readonly Color WarmBrown = new(0.5882353f, 0.3058824f, 0.007843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5921569, 0.5411765, 0.5176471)
        /// </summary>
        public static readonly Color WarmGrey = new(0.5921569f, 0.5411765f, 0.5176471f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9843137, 0.3333333, 0.5058824)
        /// </summary>
        public static readonly Color WarmPink = new(0.9843137f, 0.3333333f, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5843138, 0.1803922, 0.5607843)
        /// </summary>
        public static readonly Color WarmPurple = new(0.5843138f, 0.1803922f, 0.5607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7372549, 0.9607843, 0.6509804)
        /// </summary>
        public static readonly Color WashedOutGreen = new(0.7372549f, 0.9607843f, 0.6509804f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.05490196, 0.5294118, 0.8)
        /// </summary>
        public static readonly Color WaterBlue = new(0.05490196f, 0.5294118f, 0.8f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9921569, 0.2745098, 0.3490196)
        /// </summary>
        public static readonly Color Watermelon = new(0.9921569f, 0.2745098f, 0.3490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.227451, 0.8980392, 0.4980392)
        /// </summary>
        public static readonly Color WeirdGreen = new(0.227451f, 0.8980392f, 0.4980392f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9843137, 0.8666667, 0.4941176)
        /// </summary>
        public static readonly Color Wheat = new(0.9843137f, 0.8666667f, 0.4941176f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 1)
        /// </summary>
        public static readonly Color White = new(1, 1, 1);

        /// <summary>
        ///     A formatted XKCD survey colour (0.2156863, 0.4705882, 0.7490196)
        /// </summary>
        public static readonly Color WindowsBlue = new(0.2156863f, 0.4705882f, 0.7490196f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.5019608, 0.003921569, 0.2470588)
        /// </summary>
        public static readonly Color Wine = new(0.5019608f, 0.003921569f, 0.2470588f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.4823529, 0.01176471, 0.1372549)
        /// </summary>
        public static readonly Color WineRed = new(0.4823529f, 0.01176471f, 0.1372549f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.1254902, 0.9764706, 0.5254902)
        /// </summary>
        public static readonly Color Wintergreen = new(0.1254902f, 0.9764706f, 0.5254902f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6588235, 0.4901961, 0.7607843)
        /// </summary>
        public static readonly Color Wisteria = new(0.6588235f, 0.4901961f, 0.7607843f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 1, 0.07843138)
        /// </summary>
        public static readonly Color Yellow = new(1, 1, 0.07843138f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7176471, 0.5803922, 0)
        /// </summary>
        public static readonly Color YellowBrown = new(0.7176471f, 0.5803922f, 0);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7529412, 0.9843137, 0.1764706)
        /// </summary>
        public static readonly Color YellowGreen = new(0.7529412f, 0.9843137f, 0.1764706f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7960784, 0.6156863, 0.02352941)
        /// </summary>
        public static readonly Color YellowOchre = new(0.7960784f, 0.6156863f, 0.02352941f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.6901961, 0.003921569)
        /// </summary>
        public static readonly Color YellowOrange = new(0.9882353f, 0.6901961f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.8901961, 0.4313726)
        /// </summary>
        public static readonly Color YellowTan = new(1, 0.8901961f, 0.4313726f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7843137, 0.9921569, 0.2392157)
        /// </summary>
        public static readonly Color Yellow_Green = new(0.7843137f, 0.9921569f, 0.2392157f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7333333, 0.9764706, 0.05882353)
        /// </summary>
        public static readonly Color Yellowgreen = new(0.7333333f, 0.9764706f, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9803922, 0.9333333, 0.4)
        /// </summary>
        public static readonly Color Yellowish = new(0.9803922f, 0.9333333f, 0.4f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6078432, 0.4784314, 0.003921569)
        /// </summary>
        public static readonly Color YellowishBrown = new(0.6078432f, 0.4784314f, 0.003921569f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.6901961, 0.8666667, 0.08627451)
        /// </summary>
        public static readonly Color YellowishGreen = new(0.6901961f, 0.8666667f, 0.08627451f);

        /// <summary>
        ///     A formatted XKCD survey colour (1, 0.6705883, 0.05882353)
        /// </summary>
        public static readonly Color YellowishOrange = new(1, 0.6705883f, 0.05882353f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.9882353, 0.9882353, 0.5058824)
        /// </summary>
        public static readonly Color YellowishTan = new(0.9882353f, 0.9882353f, 0.5058824f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.682353, 0.5450981, 0.04705882)
        /// </summary>
        public static readonly Color YellowyBrown = new(0.682353f, 0.5450981f, 0.04705882f);

        /// <summary>
        ///     A formatted XKCD survey colour (0.7490196, 0.945098, 0.1568628)
        /// </summary>
        public static readonly Color YellowyGreen = new(0.7490196f, 0.945098f, 0.1568628f);

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
    }
}