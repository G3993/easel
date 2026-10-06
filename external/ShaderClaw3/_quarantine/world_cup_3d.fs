/*{
  "CATEGORIES": [
    "3D",
    "Generator",
    "Audio Reactive",
    "Sports"
  ],
  "DESCRIPTION": "World Cup neon 3D — all 48 national crests as extruded neon pixel-slabs on a black void with a synthwave grid floor and reflection. Auto-cycles every team, or pick two for a head-to-head VERSUS. Audio reactive.",
  "INPUTS": [
    {
      "NAME": "uMode",
      "LABEL": "Mode",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Cycle All",
        "Versus"
      ],
      "DEFAULT": 0
    },
    {
      "NAME": "uTeamA",
      "LABEL": "Team A (left)",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        17,
        18,
        19,
        20,
        21,
        22,
        23,
        24,
        25,
        26,
        27,
        28,
        29,
        30,
        31,
        32,
        33,
        34,
        35,
        36,
        37,
        38,
        39,
        40,
        41,
        42,
        43,
        44,
        45,
        46,
        47
      ],
      "LABELS": [
        "Algeria",
        "Argentina",
        "Australia",
        "Austria",
        "Belgium",
        "Bosnia",
        "Brazil",
        "Cabo Verde",
        "Canada",
        "Colombia",
        "Cote dIvoire",
        "Croatia",
        "Curacao",
        "Czechia",
        "DR Congo",
        "Ecuador",
        "Egypt",
        "England",
        "France",
        "Germany",
        "Ghana",
        "Haiti",
        "Iran",
        "Iraq",
        "Japan",
        "Jordan",
        "Mexico",
        "Morocco",
        "Netherlands",
        "New Zealand",
        "Norway",
        "Panama",
        "Paraguay",
        "Portugal",
        "Qatar",
        "Saudi Arabia",
        "Scotland",
        "Senegal",
        "South Africa",
        "South Korea",
        "Spain",
        "Sweden",
        "Switzerland",
        "Tunisia",
        "Turkiye",
        "USA",
        "Uruguay",
        "Uzbekistan"
      ],
      "DEFAULT": 6
    },
    {
      "NAME": "uTeamB",
      "LABEL": "Team B (right)",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        17,
        18,
        19,
        20,
        21,
        22,
        23,
        24,
        25,
        26,
        27,
        28,
        29,
        30,
        31,
        32,
        33,
        34,
        35,
        36,
        37,
        38,
        39,
        40,
        41,
        42,
        43,
        44,
        45,
        46,
        47
      ],
      "LABELS": [
        "Algeria",
        "Argentina",
        "Australia",
        "Austria",
        "Belgium",
        "Bosnia",
        "Brazil",
        "Cabo Verde",
        "Canada",
        "Colombia",
        "Cote dIvoire",
        "Croatia",
        "Curacao",
        "Czechia",
        "DR Congo",
        "Ecuador",
        "Egypt",
        "England",
        "France",
        "Germany",
        "Ghana",
        "Haiti",
        "Iran",
        "Iraq",
        "Japan",
        "Jordan",
        "Mexico",
        "Morocco",
        "Netherlands",
        "New Zealand",
        "Norway",
        "Panama",
        "Paraguay",
        "Portugal",
        "Qatar",
        "Saudi Arabia",
        "Scotland",
        "Senegal",
        "South Africa",
        "South Korea",
        "Spain",
        "Sweden",
        "Switzerland",
        "Tunisia",
        "Turkiye",
        "USA",
        "Uruguay",
        "Uzbekistan"
      ],
      "DEFAULT": 1
    },
    {
      "NAME": "uExtrude",
      "LABEL": "Extrude Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.6,
      "DEFAULT": 0.26,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "uGlowSize",
      "LABEL": "Glow Size",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "uHoldTime",
      "LABEL": "Seconds / Team",
      "TYPE": "float",
      "MIN": 0.6,
      "MAX": 8,
      "DEFAULT": 2.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uSpinSpeed",
      "LABEL": "Spin Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uSpinAmt",
      "LABEL": "Spin Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.2,
      "DEFAULT": 0.32,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uFlicker",
      "LABEL": "Neon Flicker",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uNeon",
      "LABEL": "Neon Intensity",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 4,
      "DEFAULT": 1.7,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "uFloorOn",
      "LABEL": "Floor Reflection",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Off",
        "On"
      ],
      "DEFAULT": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "uGridOn",
      "LABEL": "Grid Floor",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Off",
        "On"
      ],
      "DEFAULT": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "uZoom",
      "LABEL": "Camera Zoom",
      "TYPE": "float",
      "MIN": 0.35,
      "MAX": 2.2,
      "DEFAULT": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "uCamHeight",
      "LABEL": "Camera Height",
      "TYPE": "float",
      "MIN": -0.4,
      "MAX": 0.6,
      "DEFAULT": 0.06,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "uVignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "uRemoveBg",
      "LABEL": "Remove Background",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Off",
        "On"
      ],
      "DEFAULT": 0,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "uShowScore",
      "LABEL": "Show Score",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Off",
        "On"
      ],
      "DEFAULT": 1
    },
    {
      "NAME": "uScoreA",
      "LABEL": "Score A (left)",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 20,
      "DEFAULT": 0
    },
    {
      "NAME": "uScoreB",
      "LABEL": "Score B (right)",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 20,
      "DEFAULT": 0
    },
    {
      "NAME": "uGoal",
      "LABEL": "GOAL! (trigger anim)",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "uScanline",
      "LABEL": "Scanlines",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.3
    }
  ]
}*/

// AUTO-GENERATED — 48 World Cup team crests, 32x32 bitmaps as per-row 32-bit masks.
#define NTEAM 48
// Row lookup: returns vec2(hi16, lo16) of the 32-bit row mask.
// WebGL1-safe: constant comparisons only (no const arrays / dynamic indexing).
vec2 logoRow(int t, int yi){
    if(t==0){
        if(yi==2) return vec2(0.0,128.0);
        if(yi==3) return vec2(0.0,32764.0);
        if(yi==4) return vec2(0.0,32864.0);
        if(yi==5) return vec2(1.0,16380.0);
        if(yi==6) return vec2(2.0,16432.0);
        if(yi==7) return vec2(12908.0,40956.0);
        if(yi==8) return vec2(1281.0,8248.0);
        if(yi==9) return vec2(14190.0,16440.0);
        if(yi==10) return vec2(5408.0,36984.0);
        if(yi==11) return vec2(5423.0,12400.0);
        if(yi==12) return vec2(0.0,26416.0);
        if(yi==13) return vec2(0.0,3968.0);
        if(yi==14) return vec2(40.0,8128.0);
        if(yi==15) return vec2(16.0,8128.0);
        if(yi==16) return vec2(553.0,8132.0);
        if(yi==17) return vec2(257.0,36748.0);
        if(yi==18) return vec2(449.0,34572.0);
        if(yi==19) return vec2(240.0,49176.0);
        if(yi==20) return vec2(120.0,0.0);
        if(yi==21) return vec2(63.0,50944.0);
        if(yi==22) return vec2(31.0,61312.0);
        if(yi==23) return vec2(15.0,57344.0);
        if(yi==25) return vec2(40.0,18944.0);
        if(yi==26) return vec2(41.0,2048.0);
        if(yi==27) return vec2(42.0,19008.0);
        if(yi==28) return vec2(45.0,52160.0);
        if(yi==29) return vec2(2.0,24672.0);
    }
    else if(t==1){
        if(yi==2) return vec2(321.0,17024.0);
        if(yi==3) return vec2(128.0,33024.0);
        if(yi==4) return vec2(321.0,17024.0);
        if(yi==6) return vec2(511.0,65408.0);
        if(yi==7) return vec2(2047.0,65504.0);
        if(yi==9) return vec2(2047.0,65504.0);
        if(yi==10) return vec2(511.0,65408.0);
        if(yi==11) return vec2(448.0,3968.0);
        if(yi==12) return vec2(480.0,16256.0);
        if(yi==13) return vec2(511.0,16256.0);
        if(yi==14) return vec2(479.0,15232.0);
        if(yi==15) return vec2(428.0,13696.0);
        if(yi==16) return vec2(308.0,9856.0);
        if(yi==17) return vec2(311.0,9856.0);
        if(yi==18) return vec2(263.0,8320.0);
        if(yi==19) return vec2(311.0,9856.0);
        if(yi==20) return vec2(2359.0,9872.0);
        if(yi==21) return vec2(6455.0,9880.0);
        if(yi==22) return vec2(511.0,16256.0);
        if(yi==23) return vec2(1279.0,16160.0);
        if(yi==24) return vec2(3196.0,7728.0);
        if(yi==25) return vec2(319.0,64640.0);
        if(yi==26) return vec2(783.0,61632.0);
        if(yi==27) return vec2(67.0,49664.0);
        if(yi==28) return vec2(200.0,4864.0);
        if(yi==29) return vec2(27.0,55296.0);
    }
    else if(t==2){
        if(yi==2) return vec2(15.0,0.0);
        if(yi==3) return vec2(31.0,7168.0);
        if(yi==4) return vec2(159.0,16128.0);
        if(yi==5) return vec2(414.0,32640.0);
        if(yi==6) return vec2(798.0,32704.0);
        if(yi==7) return vec2(1854.0,32736.0);
        if(yi==8) return vec2(3646.0,31216.0);
        if(yi==9) return vec2(3132.0,30832.0);
        if(yi==10) return vec2(7292.0,31800.0);
        if(yi==11) return vec2(7292.0,15384.0);
        if(yi==12) return vec2(15608.0,15884.0);
        if(yi==13) return vec2(15608.0,7948.0);
        if(yi==14) return vec2(16368.0,8068.0);
        if(yi==15) return vec2(16352.0,4032.0);
        if(yi==16) return vec2(16320.0,2016.0);
        if(yi==17) return vec2(16256.0,1008.0);
        if(yi==18) return vec2(7936.0,508.0);
        if(yi==19) return vec2(7.0,61692.0);
        if(yi==20) return vec2(31.0,64632.0);
        if(yi==21) return vec2(511.0,65080.0);
        if(yi==22) return vec2(4095.0,65280.0);
        if(yi==23) return vec2(4095.0,65408.0);
        if(yi==24) return vec2(2032.0,3968.0);
        if(yi==25) return vec2(896.0,8064.0);
        if(yi==26) return vec2(0.0,65408.0);
        if(yi==27) return vec2(127.0,65280.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==3){
        if(yi==2) return vec2(123.0,56832.0);
        if(yi==3) return vec2(72.0,20992.0);
        if(yi==4) return vec2(121.0,53760.0);
        if(yi==5) return vec2(72.0,20992.0);
        if(yi==6) return vec2(120.0,24064.0);
        if(yi==9) return vec2(992.0,1984.0);
        if(yi==10) return vec2(1009.0,36800.0);
        if(yi==11) return vec2(1017.0,57280.0);
        if(yi==12) return vec2(25.0,38912.0);
        if(yi==13) return vec2(1021.0,49088.0);
        if(yi==14) return vec2(1023.0,65472.0);
        if(yi==15) return vec2(24.0,6144.0);
        if(yi==16) return vec2(1019.0,57280.0);
        if(yi==17) return vec2(1019.0,57280.0);
        if(yi==18) return vec2(24.0,6144.0);
        if(yi==19) return vec2(1019.0,57280.0);
        if(yi==20) return vec2(1021.0,49088.0);
        if(yi==21) return vec2(990.0,31680.0);
        if(yi==22) return vec2(959.0,64960.0);
        if(yi==23) return vec2(886.0,28352.0);
        if(yi==24) return vec2(749.0,46912.0);
        if(yi==25) return vec2(731.0,56128.0);
        if(yi==26) return vec2(219.0,56064.0);
        if(yi==27) return vec2(93.0,47616.0);
        if(yi==28) return vec2(14.0,28672.0);
        if(yi==29) return vec2(3.0,49152.0);
    }
    else if(t==4){
        if(yi==2) return vec2(2.0,16384.0);
        if(yi==3) return vec2(5.0,40960.0);
        if(yi==4) return vec2(14.0,28672.0);
        if(yi==6) return vec2(7.0,57344.0);
        if(yi==8) return vec2(255.0,65280.0);
        if(yi==9) return vec2(128.0,21760.0);
        if(yi==10) return vec2(128.0,21760.0);
        if(yi==11) return vec2(128.0,21760.0);
        if(yi==12) return vec2(128.0,21760.0);
        if(yi==13) return vec2(135.0,21760.0);
        if(yi==14) return vec2(143.0,21760.0);
        if(yi==15) return vec2(159.0,21760.0);
        if(yi==16) return vec2(191.0,21760.0);
        if(yi==17) return vec2(255.0,21760.0);
        if(yi==18) return vec2(255.0,21760.0);
        if(yi==19) return vec2(255.0,21760.0);
        if(yi==20) return vec2(255.0,21760.0);
        if(yi==21) return vec2(255.0,21760.0);
        if(yi==22) return vec2(255.0,21760.0);
        if(yi==23) return vec2(127.0,22016.0);
        if(yi==24) return vec2(63.0,21504.0);
        if(yi==25) return vec2(159.0,22784.0);
        if(yi==26) return vec2(79.0,20992.0);
        if(yi==27) return vec2(167.0,58624.0);
        if(yi==28) return vec2(80.0,2560.0);
        if(yi==29) return vec2(43.0,54272.0);
    }
    else if(t==5){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(451.0,50048.0);
        if(yi==6) return vec2(783.0,61632.0);
        if(yi==7) return vec2(1567.0,63584.0);
        if(yi==8) return vec2(3103.0,63536.0);
        if(yi==9) return vec2(3267.0,49968.0);
        if(yi==10) return vec2(6624.0,920.0);
        if(yi==11) return vec2(6607.0,64920.0);
        if(yi==12) return vec2(13247.0,65244.0);
        if(yi==13) return vec2(13247.0,65228.0);
        if(yi==14) return vec2(13247.0,65228.0);
        if(yi==15) return vec2(14207.0,63980.0);
        if(yi==16) return vec2(14207.0,63980.0);
        if(yi==17) return vec2(13247.0,63692.0);
        if(yi==18) return vec2(12415.0,57356.0);
        if(yi==19) return vec2(12543.0,49164.0);
        if(yi==20) return vec2(6207.0,33560.0);
        if(yi==21) return vec2(6623.0,3992.0);
        if(yi==22) return vec2(3564.0,8112.0);
        if(yi==23) return vec2(3318.0,7984.0);
        if(yi==24) return vec2(1656.0,15968.0);
        if(yi==25) return vec2(796.0,14528.0);
        if(yi==26) return vec2(448.0,896.0);
        if(yi==27) return vec2(248.0,7936.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==6){
        if(yi==2) return vec2(2.0,16384.0);
        if(yi==3) return vec2(145.0,35072.0);
        if(yi==4) return vec2(97.0,34304.0);
        if(yi==5) return vec2(9314.0,17956.0);
        if(yi==6) return vec2(6288.0,2328.0);
        if(yi==7) return vec2(6151.0,57368.0);
        if(yi==8) return vec2(9246.0,30756.0);
        if(yi==9) return vec2(126.0,32256.0);
        if(yi==10) return vec2(480.0,1920.0);
        if(yi==11) return vec2(2032.0,4064.0);
        if(yi==12) return vec2(952.0,7616.0);
        if(yi==13) return vec2(408.0,6528.0);
        if(yi==14) return vec2(384.0,384.0);
        if(yi==15) return vec2(443.0,56704.0);
        if(yi==16) return vec2(10.0,17408.0);
        if(yi==17) return vec2(59.0,50176.0);
        if(yi==18) return vec2(394.0,17792.0);
        if(yi==19) return vec2(395.0,56704.0);
        if(yi==20) return vec2(384.0,384.0);
        if(yi==21) return vec2(408.0,6528.0);
        if(yi==22) return vec2(440.0,7552.0);
        if(yi==23) return vec2(496.0,3968.0);
        if(yi==24) return vec2(224.0,1792.0);
        if(yi==25) return vec2(126.0,32256.0);
        if(yi==26) return vec2(62.0,31744.0);
        if(yi==27) return vec2(14.0,28672.0);
        if(yi==28) return vec2(3.0,49152.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==7){
        if(yi==2) return vec2(1023.0,65472.0);
        if(yi==3) return vec2(1023.0,65472.0);
        if(yi==4) return vec2(1023.0,65472.0);
        if(yi==5) return vec2(780.0,12480.0);
        if(yi==6) return vec2(1007.0,48832.0);
        if(yi==7) return vec2(911.0,47296.0);
        if(yi==8) return vec2(1004.0,16064.0);
        if(yi==9) return vec2(1023.0,65472.0);
        if(yi==10) return vec2(896.0,53184.0);
        if(yi==11) return vec2(895.0,8384.0);
        if(yi==12) return vec2(1017.0,51392.0);
        if(yi==13) return vec2(831.0,59584.0);
        if(yi==14) return vec2(775.0,62656.0);
        if(yi==15) return vec2(771.0,64192.0);
        if(yi==16) return vec2(798.0,16320.0);
        if(yi==17) return vec2(775.0,14528.0);
        if(yi==18) return vec2(771.0,61632.0);
        if(yi==19) return vec2(770.0,57536.0);
        if(yi==20) return vec2(770.0,16576.0);
        if(yi==21) return vec2(770.0,16576.0);
        if(yi==22) return vec2(898.0,16832.0);
        if(yi==23) return vec2(962.0,17344.0);
        if(yi==24) return vec2(994.0,18368.0);
        if(yi==25) return vec2(498.0,20352.0);
        if(yi==26) return vec2(250.0,24320.0);
        if(yi==27) return vec2(62.0,31744.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==8){
        if(yi==2) return vec2(1.0,32768.0);
        if(yi==3) return vec2(17.0,34816.0);
        if(yi==4) return vec2(27.0,55296.0);
        if(yi==5) return vec2(31.0,63488.0);
        if(yi==6) return vec2(1055.0,63520.0);
        if(yi==7) return vec2(7711.0,63608.0);
        if(yi==8) return vec2(7967.0,63736.0);
        if(yi==9) return vec2(8095.0,63992.0);
        if(yi==10) return vec2(16351.0,64508.0);
        if(yi==11) return vec2(16383.0,65532.0);
        if(yi==12) return vec2(8191.0,65528.0);
        if(yi==13) return vec2(4095.0,65520.0);
        if(yi==15) return vec2(2276.0,37496.0);
        if(yi==16) return vec2(5418.0,46344.0);
        if(yi==17) return vec2(7470.0,55048.0);
        if(yi==18) return vec2(5354.0,38264.0);
        if(yi==20) return vec2(6624.0,1944.0);
        if(yi==21) return vec2(7408.0,3896.0);
        if(yi==22) return vec2(3687.0,58992.0);
        if(yi==23) return vec2(3855.0,61680.0);
        if(yi==24) return vec2(1823.0,63712.0);
        if(yi==25) return vec2(911.0,61888.0);
        if(yi==26) return vec2(391.0,57728.0);
        if(yi==28) return vec2(3.0,49152.0);
        if(yi==29) return vec2(7.0,57344.0);
    }
    else if(t==9){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(248.0,7936.0);
        if(yi==5) return vec2(449.0,33664.0);
        if(yi==6) return vec2(796.0,14528.0);
        if(yi==7) return vec2(1662.0,32352.0);
        if(yi==8) return vec2(3326.0,32560.0);
        if(yi==9) return vec2(3582.0,32688.0);
        if(yi==10) return vec2(6654.0,32664.0);
        if(yi==11) return vec2(6392.0,7960.0);
        if(yi==12) return vec2(13025.0,34636.0);
        if(yi==13) return vec2(12295.0,57356.0);
        if(yi==14) return vec2(13215.0,63948.0);
        if(yi==15) return vec2(2015.0,64480.0);
        if(yi==16) return vec2(2015.0,64480.0);
        if(yi==17) return vec2(13263.0,62412.0);
        if(yi==18) return vec2(13263.0,62412.0);
        if(yi==19) return vec2(15343.0,63436.0);
        if(yi==20) return vec2(6631.0,59288.0);
        if(yi==21) return vec2(6624.0,1944.0);
        if(yi==22) return vec2(3279.0,62256.0);
        if(yi==23) return vec2(3103.0,63536.0);
        if(yi==24) return vec2(1631.0,64096.0);
        if(yi==25) return vec2(783.0,61632.0);
        if(yi==26) return vec2(451.0,50048.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(62.0,31744.0);
        if(yi==29) return vec2(14.0,28672.0);
    }
    else if(t==10){
        if(yi==2) return vec2(1023.0,65472.0);
        if(yi==3) return vec2(1023.0,65472.0);
        if(yi==4) return vec2(1023.0,63936.0);
        if(yi==5) return vec2(1023.0,65216.0);
        if(yi==6) return vec2(1023.0,65216.0);
        if(yi==7) return vec2(1016.0,32448.0);
        if(yi==8) return vec2(1008.0,3264.0);
        if(yi==9) return vec2(992.0,448.0);
        if(yi==10) return vec2(835.0,49088.0);
        if(yi==11) return vec2(839.0,57280.0);
        if(yi==12) return vec2(839.0,57280.0);
        if(yi==13) return vec2(899.0,49088.0);
        if(yi==14) return vec2(832.0,32704.0);
        if(yi==15) return vec2(832.0,8128.0);
        if(yi==16) return vec2(864.0,51648.0);
        if(yi==17) return vec2(1008.0,6336.0);
        if(yi==18) return vec2(1016.0,5312.0);
        if(yi==19) return vec2(988.0,8384.0);
        if(yi==20) return vec2(1004.0,192.0);
        if(yi==21) return vec2(976.0,24768.0);
        if(yi==22) return vec2(960.0,49600.0);
        if(yi==23) return vec2(963.0,33216.0);
        if(yi==24) return vec2(966.0,960.0);
        if(yi==25) return vec2(492.0,1920.0);
        if(yi==26) return vec2(508.0,3968.0);
        if(yi==27) return vec2(124.0,7680.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==11){
        if(yi==2) return vec2(63.0,64512.0);
        if(yi==3) return vec2(120.0,7680.0);
        if(yi==4) return vec2(250.0,24320.0);
        if(yi==5) return vec2(506.0,24448.0);
        if(yi==6) return vec2(3979.0,53744.0);
        if(yi==7) return vec2(3978.0,20976.0);
        if(yi==8) return vec2(3978.0,20976.0);
        if(yi==9) return vec2(2040.0,8160.0);
        if(yi==10) return vec2(1146.0,24096.0);
        if(yi==11) return vec2(1146.0,56864.0);
        if(yi==12) return vec2(635.0,24128.0);
        if(yi==13) return vec2(1018.0,24512.0);
        if(yi==14) return vec2(968.0,5056.0);
        if(yi==15) return vec2(459.0,54144.0);
        if(yi==16) return vec2(456.0,21376.0);
        if(yi==17) return vec2(507.0,57216.0);
        if(yi==18) return vec2(186.0,7424.0);
        if(yi==19) return vec2(187.0,56576.0);
        if(yi==20) return vec2(248.0,7936.0);
        if(yi==21) return vec2(235.0,55040.0);
        if(yi==22) return vec2(108.0,13824.0);
        if(yi==23) return vec2(40.0,5120.0);
        if(yi==24) return vec2(48.0,3072.0);
        if(yi==25) return vec2(16.0,2048.0);
        if(yi==26) return vec2(16.0,2048.0);
        if(yi==27) return vec2(8.0,4096.0);
        if(yi==28) return vec2(4.0,8192.0);
        if(yi==29) return vec2(3.0,49152.0);
    }
    else if(t==12){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(775.0,57536.0);
        if(yi==7) return vec2(1560.0,6240.0);
        if(yi==8) return vec2(3105.0,33840.0);
        if(yi==9) return vec2(3138.0,16944.0);
        if(yi==10) return vec2(6274.0,16664.0);
        if(yi==11) return vec2(6401.0,32920.0);
        if(yi==12) return vec2(12544.0,140.0);
        if(yi==13) return vec2(12863.0,64588.0);
        if(yi==14) return vec2(12960.0,1356.0);
        if(yi==15) return vec2(12841.0,9292.0);
        if(yi==16) return vec2(12845.0,46156.0);
        if(yi==17) return vec2(12969.0,9548.0);
        if(yi==18) return vec2(12847.0,62540.0);
        if(yi==19) return vec2(12576.0,1164.0);
        if(yi==20) return vec2(6431.0,63640.0);
        if(yi==21) return vec2(6296.0,6424.0);
        if(yi==22) return vec2(3151.0,62000.0);
        if(yi==23) return vec2(3108.0,9264.0);
        if(yi==24) return vec2(1555.0,51296.0);
        if(yi==25) return vec2(768.0,192.0);
        if(yi==26) return vec2(463.0,62336.0);
        if(yi==27) return vec2(255.0,65280.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==13){
        if(yi==2) return vec2(511.0,65408.0);
        if(yi==3) return vec2(256.0,128.0);
        if(yi==4) return vec2(2047.0,65504.0);
        if(yi==5) return vec2(1530.0,49056.0);
        if(yi==6) return vec2(1532.0,32672.0);
        if(yi==7) return vec2(1528.0,49056.0);
        if(yi==8) return vec2(1496.0,7072.0);
        if(yi==9) return vec2(1448.0,14752.0);
        if(yi==10) return vec2(1396.0,28832.0);
        if(yi==11) return vec2(1460.0,1952.0);
        if(yi==12) return vec2(1396.0,4000.0);
        if(yi==13) return vec2(1492.0,15776.0);
        if(yi==14) return vec2(1460.0,928.0);
        if(yi==15) return vec2(1452.0,7584.0);
        if(yi==16) return vec2(1496.0,1952.0);
        if(yi==17) return vec2(1440.0,1952.0);
        if(yi==18) return vec2(1444.0,59296.0);
        if(yi==19) return vec2(1500.0,53152.0);
        if(yi==20) return vec2(1528.0,53152.0);
        if(yi==21) return vec2(1529.0,50080.0);
        if(yi==22) return vec2(760.0,30528.0);
        if(yi==23) return vec2(380.0,65152.0);
        if(yi==24) return vec2(189.0,64768.0);
        if(yi==25) return vec2(95.0,64000.0);
        if(yi==26) return vec2(35.0,50176.0);
        if(yi==27) return vec2(24.0,6144.0);
        if(yi==28) return vec2(6.0,24576.0);
        if(yi==29) return vec2(3.0,49152.0);
    }
    else if(t==14){
        if(yi==2) return vec2(23.0,59392.0);
        if(yi==3) return vec2(31.0,63488.0);
        if(yi==4) return vec2(153.0,39168.0);
        if(yi==5) return vec2(479.0,64384.0);
        if(yi==6) return vec2(1006.0,30656.0);
        if(yi==7) return vec2(495.0,63360.0);
        if(yi==8) return vec2(500.0,12160.0);
        if(yi==9) return vec2(506.0,24448.0);
        if(yi==10) return vec2(490.0,22400.0);
        if(yi==11) return vec2(477.0,48000.0);
        if(yi==12) return vec2(446.0,32128.0);
        if(yi==13) return vec2(889.0,40640.0);
        if(yi==14) return vec2(759.0,61248.0);
        if(yi==15) return vec2(1774.0,30560.0);
        if(yi==16) return vec2(1755.0,56160.0);
        if(yi==17) return vec2(1750.0,27488.0);
        if(yi==18) return vec2(1750.0,27488.0);
        if(yi==19) return vec2(1755.0,56160.0);
        if(yi==20) return vec2(1774.0,30560.0);
        if(yi==21) return vec2(1911.0,61152.0);
        if(yi==22) return vec2(953.0,40384.0);
        if(yi==23) return vec2(1022.0,32704.0);
        if(yi==24) return vec2(511.0,65408.0);
        if(yi==25) return vec2(255.0,65280.0);
        if(yi==26) return vec2(63.0,64512.0);
        if(yi==27) return vec2(15.0,61440.0);
        if(yi==28) return vec2(3.0,49152.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==15){
        if(yi==2) return vec2(511.0,65408.0);
        if(yi==3) return vec2(273.0,128.0);
        if(yi==4) return vec2(511.0,65408.0);
        if(yi==5) return vec2(2047.0,65504.0);
        if(yi==6) return vec2(2047.0,65504.0);
        if(yi==7) return vec2(1599.0,64608.0);
        if(yi==8) return vec2(1823.0,63712.0);
        if(yi==9) return vec2(1932.0,12768.0);
        if(yi==10) return vec2(1996.0,13280.0);
        if(yi==11) return vec2(1743.0,13152.0);
        if(yi==12) return vec2(1615.0,12896.0);
        if(yi==13) return vec2(1871.0,13024.0);
        if(yi==14) return vec2(1993.0,9184.0);
        if(yi==15) return vec2(1996.0,992.0);
        if(yi==16) return vec2(1614.0,4704.0);
        if(yi==17) return vec2(1615.0,12896.0);
        if(yi==18) return vec2(1615.0,12896.0);
        if(yi==19) return vec2(1615.0,12896.0);
        if(yi==20) return vec2(1615.0,12896.0);
        if(yi==21) return vec2(1612.0,12896.0);
        if(yi==22) return vec2(1660.0,15968.0);
        if(yi==23) return vec2(831.0,64704.0);
        if(yi==24) return vec2(927.0,63936.0);
        if(yi==25) return vec2(231.0,59136.0);
        if(yi==26) return vec2(249.0,40704.0);
        if(yi==27) return vec2(62.0,31744.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==16){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(768.0,63680.0);
        if(yi==7) return vec2(1547.0,30048.0);
        if(yi==8) return vec2(3094.0,688.0);
        if(yi==9) return vec2(3116.0,368.0);
        if(yi==10) return vec2(7262.0,29624.0);
        if(yi==11) return vec2(7358.0,64344.0);
        if(yi==12) return vec2(15734.0,29612.0);
        if(yi==13) return vec2(16108.0,332.0);
        if(yi==14) return vec2(15834.0,652.0);
        if(yi==15) return vec2(15285.0,29964.0);
        if(yi==16) return vec2(14186.0,64012.0);
        if(yi==17) return vec2(16085.0,21516.0);
        if(yi==18) return vec2(15786.0,43020.0);
        if(yi==19) return vec2(15189.0,20492.0);
        if(yi==20) return vec2(8191.0,65528.0);
        if(yi==21) return vec2(6144.0,24.0);
        if(yi==22) return vec2(3072.0,48.0);
        if(yi==23) return vec2(3072.0,48.0);
        if(yi==24) return vec2(1536.0,96.0);
        if(yi==25) return vec2(768.0,192.0);
        if(yi==26) return vec2(448.0,896.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==17){
        if(yi==2) return vec2(4095.0,65520.0);
        if(yi==3) return vec2(2558.0,32656.0);
        if(yi==4) return vec2(2430.0,27536.0);
        if(yi==5) return vec2(3967.0,58352.0);
        if(yi==6) return vec2(3840.0,6128.0);
        if(yi==7) return vec2(3840.0,2160.0);
        if(yi==8) return vec2(3951.0,59376.0);
        if(yi==9) return vec2(3875.0,61680.0);
        if(yi==10) return vec2(4095.0,65520.0);
        if(yi==11) return vec2(2558.0,32656.0);
        if(yi==12) return vec2(2430.0,27536.0);
        if(yi==13) return vec2(3967.0,58352.0);
        if(yi==14) return vec2(3840.0,6128.0);
        if(yi==15) return vec2(3840.0,2160.0);
        if(yi==16) return vec2(3951.0,59376.0);
        if(yi==17) return vec2(3875.0,61680.0);
        if(yi==18) return vec2(4095.0,65520.0);
        if(yi==19) return vec2(2558.0,32656.0);
        if(yi==20) return vec2(2430.0,27536.0);
        if(yi==21) return vec2(3967.0,58352.0);
        if(yi==22) return vec2(3840.0,6128.0);
        if(yi==23) return vec2(1792.0,2272.0);
        if(yi==24) return vec2(1903.0,59360.0);
        if(yi==25) return vec2(295.0,61824.0);
        if(yi==26) return vec2(510.0,32640.0);
        if(yi==27) return vec2(126.0,32256.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==18){
        if(yi==2) return vec2(10.0,20480.0);
        if(yi==3) return vec2(4.0,8192.0);
        if(yi==4) return vec2(10.0,20480.0);
        if(yi==6) return vec2(0.0,1024.0);
        if(yi==7) return vec2(0.0,11264.0);
        if(yi==8) return vec2(0.0,16128.0);
        if(yi==9) return vec2(496.0,6656.0);
        if(yi==10) return vec2(920.0,16256.0);
        if(yi==11) return vec2(892.0,16128.0);
        if(yi==12) return vec2(742.0,15872.0);
        if(yi==13) return vec2(479.0,16128.0);
        if(yi==14) return vec2(447.0,65408.0);
        if(yi==15) return vec2(377.0,65408.0);
        if(yi==16) return vec2(375.0,65408.0);
        if(yi==17) return vec2(111.0,65280.0);
        if(yi==18) return vec2(95.0,65024.0);
        if(yi==19) return vec2(95.0,64512.0);
        if(yi==20) return vec2(31.0,63488.0);
        if(yi==21) return vec2(28.0,28672.0);
        if(yi==22) return vec2(8.0,8192.0);
        if(yi==23) return vec2(8.0,8192.0);
        if(yi==24) return vec2(14.0,14336.0);
        if(yi==26) return vec2(25.0,38912.0);
        if(yi==27) return vec2(8.0,34816.0);
        if(yi==28) return vec2(25.0,38912.0);
        if(yi==29) return vec2(8.0,34816.0);
    }
    else if(t==19){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(455.0,58240.0);
        if(yi==6) return vec2(796.0,14528.0);
        if(yi==7) return vec2(1648.0,3680.0);
        if(yi==8) return vec2(3264.0,816.0);
        if(yi==9) return vec2(3459.0,33200.0);
        if(yi==10) return vec2(6401.0,49304.0);
        if(yi==11) return vec2(6913.0,32984.0);
        if(yi==12) return vec2(4621.0,45128.0);
        if(yi==13) return vec2(13853.0,47212.0);
        if(yi==14) return vec2(13343.0,63532.0);
        if(yi==15) return vec2(13359.0,62508.0);
        if(yi==16) return vec2(13399.0,59948.0);
        if(yi==17) return vec2(13355.0,54316.0);
        if(yi==18) return vec2(13909.0,43628.0);
        if(yi==19) return vec2(4651.0,54344.0);
        if(yi==20) return vec2(6915.0,49368.0);
        if(yi==21) return vec2(6402.0,16536.0);
        if(yi==22) return vec2(3461.0,41392.0);
        if(yi==23) return vec2(3296.0,1840.0);
        if(yi==24) return vec2(48.0,3072.0);
        if(yi==25) return vec2(924.0,14784.0);
        if(yi==26) return vec2(455.0,58240.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(55.0,60416.0);
        if(yi==29) return vec2(7.0,57344.0);
    }
    else if(t==20){
        if(yi==2) return vec2(1023.0,0.0);
        if(yi==3) return vec2(521.0,0.0);
        if(yi==4) return vec2(553.0,0.0);
        if(yi==5) return vec2(521.0,0.0);
        if(yi==6) return vec2(969.0,0.0);
        if(yi==7) return vec2(585.0,0.0);
        if(yi==8) return vec2(292.0,32768.0);
        if(yi==9) return vec2(146.0,16384.0);
        if(yi==10) return vec2(73.0,8192.0);
        if(yi==11) return vec2(36.0,38912.0);
        if(yi==12) return vec2(18.0,17920.0);
        if(yi==13) return vec2(33.0,12544.0);
        if(yi==14) return vec2(76.0,52352.0);
        if(yi==15) return vec2(82.0,4736.0);
        if(yi==16) return vec2(166.0,6464.0);
        if(yi==17) return vec2(174.0,7488.0);
        if(yi==18) return vec2(320.0,49312.0);
        if(yi==19) return vec2(337.0,58016.0);
        if(yi==20) return vec2(337.0,58016.0);
        if(yi==21) return vec2(320.0,49312.0);
        if(yi==22) return vec2(174.0,7488.0);
        if(yi==23) return vec2(166.0,6464.0);
        if(yi==24) return vec2(82.0,4736.0);
        if(yi==25) return vec2(72.0,50304.0);
        if(yi==26) return vec2(38.0,6400.0);
        if(yi==27) return vec2(25.0,58880.0);
        if(yi==28) return vec2(6.0,6144.0);
        if(yi==29) return vec2(1.0,57344.0);
    }
    else if(t==21){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(768.0,192.0);
        if(yi==7) return vec2(1663.0,65120.0);
        if(yi==8) return vec2(3136.0,560.0);
        if(yi==9) return vec2(3136.0,560.0);
        if(yi==10) return vec2(6208.0,536.0);
        if(yi==11) return vec2(6271.0,65048.0);
        if(yi==12) return vec2(12384.0,524.0);
        if(yi==13) return vec2(12368.0,524.0);
        if(yi==14) return vec2(72.0,512.0);
        if(yi==15) return vec2(12356.0,524.0);
        if(yi==16) return vec2(12354.0,524.0);
        if(yi==17) return vec2(65.0,512.0);
        if(yi==18) return vec2(12352.0,33292.0);
        if(yi==19) return vec2(12352.0,16908.0);
        if(yi==20) return vec2(6208.0,8728.0);
        if(yi==21) return vec2(6176.0,5144.0);
        if(yi==22) return vec2(3088.0,2096.0);
        if(yi==23) return vec2(3080.0,4144.0);
        if(yi==24) return vec2(1540.0,8288.0);
        if(yi==25) return vec2(771.0,49344.0);
        if(yi==26) return vec2(448.0,896.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==22){
        if(yi==2) return vec2(511.0,65408.0);
        if(yi==3) return vec2(256.0,128.0);
        if(yi==4) return vec2(256.0,128.0);
        if(yi==5) return vec2(511.0,65408.0);
        if(yi==6) return vec2(509.0,49024.0);
        if(yi==7) return vec2(510.0,32640.0);
        if(yi==8) return vec2(511.0,65408.0);
        if(yi==9) return vec2(256.0,128.0);
        if(yi==10) return vec2(256.0,128.0);
        if(yi==11) return vec2(511.0,65408.0);
        if(yi==12) return vec2(256.0,128.0);
        if(yi==13) return vec2(256.0,128.0);
        if(yi==14) return vec2(259.0,49280.0);
        if(yi==15) return vec2(265.0,36992.0);
        if(yi==16) return vec2(280.0,6272.0);
        if(yi==17) return vec2(312.0,7296.0);
        if(yi==18) return vec2(256.0,128.0);
        if(yi==19) return vec2(321.0,33408.0);
        if(yi==20) return vec2(355.0,50816.0);
        if(yi==21) return vec2(355.0,50816.0);
        if(yi==22) return vec2(321.0,33408.0);
        if(yi==23) return vec2(256.0,128.0);
        if(yi==24) return vec2(312.0,7296.0);
        if(yi==25) return vec2(152.0,6400.0);
        if(yi==26) return vec2(73.0,37376.0);
        if(yi==27) return vec2(35.0,50176.0);
        if(yi==28) return vec2(24.0,6144.0);
        if(yi==29) return vec2(7.0,57344.0);
    }
    else if(t==23){
        if(yi==2) return vec2(255.0,65280.0);
        if(yi==3) return vec2(128.0,256.0);
        if(yi==4) return vec2(441.0,13696.0);
        if(yi==5) return vec2(811.0,38080.0);
        if(yi==6) return vec2(634.0,37952.0);
        if(yi==7) return vec2(512.0,64.0);
        if(yi==8) return vec2(767.0,65344.0);
        if(yi==9) return vec2(767.0,65344.0);
        if(yi==10) return vec2(767.0,65344.0);
        if(yi==11) return vec2(767.0,65344.0);
        if(yi==12) return vec2(767.0,65344.0);
        if(yi==13) return vec2(767.0,65344.0);
        if(yi==14) return vec2(512.0,64.0);
        if(yi==15) return vec2(544.0,32832.0);
        if(yi==16) return vec2(812.0,43200.0);
        if(yi==17) return vec2(302.0,64640.0);
        if(yi==18) return vec2(384.0,1408.0);
        if(yi==19) return vec2(128.0,256.0);
        if(yi==20) return vec2(223.0,64256.0);
        if(yi==21) return vec2(95.0,64000.0);
        if(yi==22) return vec2(111.0,62976.0);
        if(yi==23) return vec2(47.0,62464.0);
        if(yi==24) return vec2(55.0,60416.0);
        if(yi==25) return vec2(27.0,55296.0);
        if(yi==26) return vec2(13.0,45056.0);
        if(yi==27) return vec2(6.0,24576.0);
        if(yi==28) return vec2(3.0,49152.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==24){
        if(yi==2) return vec2(4095.0,65520.0);
        if(yi==3) return vec2(4095.0,65520.0);
        if(yi==4) return vec2(3087.0,40944.0);
        if(yi==5) return vec2(4086.0,12272.0);
        if(yi==6) return vec2(3611.0,45040.0);
        if(yi==7) return vec2(4093.0,47088.0);
        if(yi==8) return vec2(3846.0,48112.0);
        if(yi==9) return vec2(4095.0,31728.0);
        if(yi==10) return vec2(3969.0,64496.0);
        if(yi==11) return vec2(4095.0,64496.0);
        if(yi==12) return vec2(4074.0,63472.0);
        if(yi==13) return vec2(4052.0,57584.0);
        if(yi==14) return vec2(4010.0,44016.0);
        if(yi==15) return vec2(3926.0,20016.0);
        if(yi==16) return vec2(4094.0,60976.0);
        if(yi==17) return vec2(4088.0,57904.0);
        if(yi==18) return vec2(4095.0,65520.0);
        if(yi==19) return vec2(4044.0,10224.0);
        if(yi==20) return vec2(4023.0,45040.0);
        if(yi==21) return vec2(3974.0,12272.0);
        if(yi==22) return vec2(4023.0,44528.0);
        if(yi==23) return vec2(1975.0,41440.0);
        if(yi==24) return vec2(2047.0,65504.0);
        if(yi==25) return vec2(511.0,65408.0);
        if(yi==26) return vec2(511.0,65408.0);
        if(yi==27) return vec2(127.0,65024.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==25){
        if(yi==2) return vec2(4095.0,65520.0);
        if(yi==3) return vec2(2268.0,34992.0);
        if(yi==4) return vec2(2730.0,43696.0);
        if(yi==5) return vec2(2698.0,51888.0);
        if(yi==6) return vec2(2732.0,43152.0);
        if(yi==7) return vec2(4095.0,65520.0);
        if(yi==9) return vec2(2815.0,65360.0);
        if(yi==10) return vec2(2815.0,65360.0);
        if(yi==11) return vec2(2815.0,65360.0);
        if(yi==12) return vec2(2815.0,65360.0);
        if(yi==13) return vec2(2815.0,65360.0);
        if(yi==14) return vec2(2814.0,32592.0);
        if(yi==15) return vec2(2802.0,20304.0);
        if(yi==16) return vec2(2808.0,8016.0);
        if(yi==17) return vec2(2808.0,8016.0);
        if(yi==18) return vec2(2800.0,3920.0);
        if(yi==19) return vec2(2812.0,16208.0);
        if(yi==20) return vec2(2813.0,48976.0);
        if(yi==21) return vec2(2943.0,65232.0);
        if(yi==22) return vec2(3519.0,64944.0);
        if(yi==23) return vec2(1743.0,62304.0);
        if(yi==24) return vec2(883.0,52928.0);
        if(yi==25) return vec2(413.0,47488.0);
        if(yi==26) return vec2(230.0,26368.0);
        if(yi==27) return vec2(56.0,7168.0);
        if(yi==28) return vec2(14.0,28672.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==26){
        if(yi==2) return vec2(32.0,0.0);
        if(yi==3) return vec2(96.0,0.0);
        if(yi==4) return vec2(240.0,0.0);
        if(yi==5) return vec2(247.0,57344.0);
        if(yi==6) return vec2(248.0,6144.0);
        if(yi==7) return vec2(379.0,50688.0);
        if(yi==8) return vec2(444.0,12544.0);
        if(yi==9) return vec2(732.0,3200.0);
        if(yi==10) return vec2(878.0,8832.0);
        if(yi==11) return vec2(438.0,29248.0);
        if(yi==12) return vec2(731.0,31040.0);
        if(yi==13) return vec2(879.0,64800.0);
        if(yi==14) return vec2(1463.0,57504.0);
        if(yi==15) return vec2(1502.0,57504.0);
        if(yi==16) return vec2(1517.0,49312.0);
        if(yi==17) return vec2(1403.0,49312.0);
        if(yi==18) return vec2(1207.0,49440.0);
        if(yi==19) return vec2(655.0,16704.0);
        if(yi==20) return vec2(601.0,12864.0);
        if(yi==21) return vec2(323.0,41600.0);
        if(yi==22) return vec2(290.0,33920.0);
        if(yi==23) return vec2(8099.0,34296.0);
        if(yi==25) return vec2(7914.0,11912.0);
        if(yi==26) return vec2(4649.0,17112.0);
        if(yi==27) return vec2(4648.0,36520.0);
        if(yi==28) return vec2(4649.0,17032.0);
        if(yi==29) return vec2(7914.0,11912.0);
    }
    else if(t==27){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(56.0,7168.0);
        if(yi==4) return vec2(224.0,1792.0);
        if(yi==5) return vec2(384.0,384.0);
        if(yi==6) return vec2(768.0,192.0);
        if(yi==7) return vec2(519.0,57408.0);
        if(yi==8) return vec2(526.0,28736.0);
        if(yi==9) return vec2(1564.0,14432.0);
        if(yi==10) return vec2(1056.0,1056.0);
        if(yi==11) return vec2(1077.0,44064.0);
        if(yi==12) return vec2(1074.0,19488.0);
        if(yi==13) return vec2(1083.0,56352.0);
        if(yi==14) return vec2(1080.0,7200.0);
        if(yi==15) return vec2(1053.0,47136.0);
        if(yi==16) return vec2(1549.0,45152.0);
        if(yi==17) return vec2(527.0,61504.0);
        if(yi==18) return vec2(778.0,45248.0);
        if(yi==19) return vec2(394.0,45440.0);
        if(yi==20) return vec2(234.0,46848.0);
        if(yi==21) return vec2(58.0,48128.0);
        if(yi==22) return vec2(10.0,45056.0);
        if(yi==23) return vec2(10.0,45056.0);
        if(yi==24) return vec2(10.0,45056.0);
        if(yi==25) return vec2(10.0,45056.0);
        if(yi==26) return vec2(10.0,61440.0);
        if(yi==27) return vec2(15.0,49152.0);
        if(yi==28) return vec2(3.0,0.0);
        if(yi==29) return vec2(3.0,0.0);
    }
    else if(t==28){
        if(yi==2) return vec2(31.0,63488.0);
        if(yi==3) return vec2(112.0,3584.0);
        if(yi==4) return vec2(463.0,62336.0);
        if(yi==5) return vec2(831.0,64704.0);
        if(yi==6) return vec2(1790.0,32608.0);
        if(yi==7) return vec2(1276.0,48928.0);
        if(yi==8) return vec2(1500.0,7072.0);
        if(yi==9) return vec2(1452.0,14752.0);
        if(yi==10) return vec2(1396.0,28832.0);
        if(yi==11) return vec2(1460.0,1952.0);
        if(yi==12) return vec2(1396.0,4000.0);
        if(yi==13) return vec2(1524.0,15776.0);
        if(yi==14) return vec2(1524.0,928.0);
        if(yi==15) return vec2(1516.0,7584.0);
        if(yi==16) return vec2(1496.0,1952.0);
        if(yi==17) return vec2(1456.0,1952.0);
        if(yi==18) return vec2(1444.0,59296.0);
        if(yi==19) return vec2(1500.0,53152.0);
        if(yi==20) return vec2(1528.0,53152.0);
        if(yi==21) return vec2(1529.0,50080.0);
        if(yi==22) return vec2(1528.0,30624.0);
        if(yi==23) return vec2(1148.0,65056.0);
        if(yi==24) return vec2(1917.0,65248.0);
        if(yi==25) return vec2(287.0,63616.0);
        if(yi==26) return vec2(451.0,50048.0);
        if(yi==27) return vec2(120.0,7680.0);
        if(yi==28) return vec2(14.0,28672.0);
        if(yi==29) return vec2(3.0,49152.0);
    }
    else if(t==29){
        if(yi==2) return vec2(5.0,20480.0);
        if(yi==3) return vec2(63.0,21504.0);
        if(yi==4) return vec2(250.0,55808.0);
        if(yi==5) return vec2(510.0,60032.0);
        if(yi==6) return vec2(789.0,60096.0);
        if(yi==7) return vec2(1546.0,31424.0);
        if(yi==8) return vec2(3076.0,40656.0);
        if(yi==9) return vec2(1.0,8144.0);
        if(yi==10) return vec2(0.0,28632.0);
        if(yi==11) return vec2(4096.0,2552.0);
        if(yi==12) return vec2(12288.0,13304.0);
        if(yi==13) return vec2(12288.0,1912.0);
        if(yi==14) return vec2(12528.0,3192.0);
        if(yi==15) return vec2(7104.0,4604.0);
        if(yi==16) return vec2(7944.0,1932.0);
        if(yi==17) return vec2(7792.0,3596.0);
        if(yi==18) return vec2(8164.0,12.0);
        if(yi==19) return vec2(8072.0,12.0);
        if(yi==20) return vec2(8115.0,4.0);
        if(yi==21) return vec2(8166.0,16384.0);
        if(yi==22) return vec2(4092.0,32784.0);
        if(yi==23) return vec2(3453.0,9264.0);
        if(yi==24) return vec2(1375.0,18784.0);
        if(yi==25) return vec2(349.0,53952.0);
        if(yi==26) return vec2(347.0,65408.0);
        if(yi==27) return vec2(90.0,65280.0);
        if(yi==28) return vec2(22.0,64512.0);
        if(yi==29) return vec2(6.0,28672.0);
    }
    else if(t==30){
        if(yi==2) return vec2(1023.0,65472.0);
        if(yi==3) return vec2(3999.0,63984.0);
        if(yi==4) return vec2(2862.0,29904.0);
        if(yi==5) return vec2(3357.0,47280.0);
        if(yi==6) return vec2(2821.0,41168.0);
        if(yi==7) return vec2(2078.0,30736.0);
        if(yi==9) return vec2(4093.0,49136.0);
        if(yi==10) return vec2(4093.0,49136.0);
        if(yi==11) return vec2(4093.0,49136.0);
        if(yi==12) return vec2(4093.0,49136.0);
        if(yi==13) return vec2(4093.0,49136.0);
        if(yi==14) return vec2(1.0,32768.0);
        if(yi==15) return vec2(4095.0,65520.0);
        if(yi==16) return vec2(4095.0,65520.0);
        if(yi==17) return vec2(1.0,32768.0);
        if(yi==18) return vec2(4093.0,49136.0);
        if(yi==19) return vec2(4093.0,49136.0);
        if(yi==20) return vec2(4093.0,49136.0);
        if(yi==21) return vec2(4093.0,49136.0);
        if(yi==22) return vec2(4093.0,49136.0);
        if(yi==23) return vec2(2045.0,49120.0);
        if(yi==24) return vec2(2045.0,49120.0);
        if(yi==25) return vec2(509.0,49024.0);
        if(yi==26) return vec2(509.0,49024.0);
        if(yi==27) return vec2(125.0,48640.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==31){
        if(yi==2) return vec2(1170.0,29296.0);
        if(yi==3) return vec2(2805.0,21840.0);
        if(yi==4) return vec2(2709.0,21840.0);
        if(yi==5) return vec2(3735.0,22384.0);
        if(yi==6) return vec2(2709.0,21776.0);
        if(yi==7) return vec2(2709.0,21776.0);
        if(yi==9) return vec2(4033.0,33776.0);
        if(yi==10) return vec2(2022.0,51168.0);
        if(yi==11) return vec2(499.0,36736.0);
        if(yi==12) return vec2(123.0,40448.0);
        if(yi==13) return vec2(4095.0,65520.0);
        if(yi==14) return vec2(2032.0,4064.0);
        if(yi==15) return vec2(112.0,3584.0);
        if(yi==16) return vec2(4080.0,61424.0);
        if(yi==17) return vec2(2032.0,45024.0);
        if(yi==18) return vec2(112.0,60928.0);
        if(yi==19) return vec2(4087.0,4080.0);
        if(yi==20) return vec2(2037.0,4064.0);
        if(yi==21) return vec2(119.0,3584.0);
        if(yi==22) return vec2(4091.0,8176.0);
        if(yi==23) return vec2(2044.0,16352.0);
        if(yi==24) return vec2(30.0,30720.0);
        if(yi==25) return vec2(15.0,61440.0);
        if(yi==26) return vec2(31.0,63488.0);
        if(yi==27) return vec2(51.0,52224.0);
        if(yi==28) return vec2(7.0,57344.0);
        if(yi==29) return vec2(14.0,28672.0);
    }
    else if(t==32){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(797.0,51392.0);
        if(yi==7) return vec2(1541.0,21600.0);
        if(yi==8) return vec2(3597.0,56432.0);
        if(yi==9) return vec2(3332.0,21680.0);
        if(yi==10) return vec2(6784.0,344.0);
        if(yi==11) return vec2(6465.0,33432.0);
        if(yi==12) return vec2(12417.0,33036.0);
        if(yi==13) return vec2(12319.0,63500.0);
        if(yi==14) return vec2(12303.0,61452.0);
        if(yi==15) return vec2(12295.0,57356.0);
        if(yi==16) return vec2(12291.0,49164.0);
        if(yi==17) return vec2(12295.0,57356.0);
        if(yi==18) return vec2(12302.0,28684.0);
        if(yi==19) return vec2(14344.0,4108.0);
        if(yi==20) return vec2(6176.0,1048.0);
        if(yi==21) return vec2(6224.0,2584.0);
        if(yi==22) return vec2(3232.0,1328.0);
        if(yi==23) return vec2(3392.0,688.0);
        if(yi==24) return vec2(1664.0,352.0);
        if(yi==25) return vec2(768.0,192.0);
        if(yi==26) return vec2(448.0,896.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==33){
        if(yi==2) return vec2(127.0,65024.0);
        if(yi==3) return vec2(24.0,6144.0);
        if(yi==4) return vec2(9.0,36864.0);
        if(yi==5) return vec2(9.0,36864.0);
        if(yi==6) return vec2(15.0,61440.0);
        if(yi==7) return vec2(120.0,7680.0);
        if(yi==8) return vec2(4226.0,16648.0);
        if(yi==9) return vec2(4381.0,48264.0);
        if(yi==10) return vec2(6496.0,1688.0);
        if(yi==11) return vec2(7745.0,33400.0);
        if(yi==12) return vec2(4929.0,33480.0);
        if(yi==13) return vec2(4385.0,33928.0);
        if(yi==14) return vec2(4512.0,1416.0);
        if(yi==15) return vec2(4269.0,46344.0);
        if(yi==16) return vec2(4269.0,46344.0);
        if(yi==17) return vec2(6061.0,46568.0);
        if(yi==18) return vec2(7328.0,1336.0);
        if(yi==19) return vec2(6305.0,34072.0);
        if(yi==20) return vec2(4257.0,34056.0);
        if(yi==21) return vec2(4273.0,36104.0);
        if(yi==22) return vec2(136.0,4352.0);
        if(yi==23) return vec2(103.0,58880.0);
        if(yi==24) return vec2(16.0,2048.0);
        if(yi==25) return vec2(15.0,61440.0);
        if(yi==26) return vec2(9.0,36864.0);
        if(yi==27) return vec2(9.0,36864.0);
        if(yi==28) return vec2(24.0,6144.0);
        if(yi==29) return vec2(127.0,65024.0);
    }
    else if(t==34){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(768.0,192.0);
        if(yi==7) return vec2(1536.0,96.0);
        if(yi==8) return vec2(3840.0,240.0);
        if(yi==9) return vec2(3971.0,49648.0);
        if(yi==10) return vec2(8140.0,46072.0);
        if(yi==11) return vec2(8179.0,8184.0);
        if(yi==12) return vec2(16353.0,34812.0);
        if(yi==13) return vec2(16368.0,38908.0);
        if(yi==14) return vec2(16327.0,62460.0);
        if(yi==15) return vec2(16338.0,18428.0);
        if(yi==16) return vec2(16336.0,43004.0);
        if(yi==17) return vec2(16369.0,46076.0);
        if(yi==18) return vec2(16353.0,2044.0);
        if(yi==19) return vec2(16380.0,10236.0);
        if(yi==20) return vec2(8177.0,20472.0);
        if(yi==21) return vec2(8188.0,49144.0);
        if(yi==22) return vec2(4095.0,65520.0);
        if(yi==23) return vec2(4086.0,18416.0);
        if(yi==24) return vec2(2027.0,22496.0);
        if(yi==25) return vec2(994.0,22464.0);
        if(yi==26) return vec2(491.0,18304.0);
        if(yi==27) return vec2(235.0,24320.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==35){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(58.0,48128.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(768.0,192.0);
        if(yi==7) return vec2(1551.0,61536.0);
        if(yi==8) return vec2(3088.0,3120.0);
        if(yi==9) return vec2(2083.0,33296.0);
        if(yi==10) return vec2(6212.0,280.0);
        if(yi==11) return vec2(4224.0,392.0);
        if(yi==12) return vec2(12672.0,460.0);
        if(yi==13) return vec2(8696.0,484.0);
        if(yi==14) return vec2(8452.0,228.0);
        if(yi==15) return vec2(8194.0,20.0);
        if(yi==16) return vec2(8192.0,20.0);
        if(yi==17) return vec2(9949.0,49652.0);
        if(yi==18) return vec2(8788.0,17404.0);
        if(yi==19) return vec2(14045.0,50172.0);
        if(yi==20) return vec2(4693.0,1016.0);
        if(yi==21) return vec2(6741.0,50168.0);
        if(yi==22) return vec2(2048.0,1008.0);
        if(yi==23) return vec2(3074.0,304.0);
        if(yi==24) return vec2(1604.0,2144.0);
        if(yi==25) return vec2(824.0,15552.0);
        if(yi==26) return vec2(449.0,65408.0);
        if(yi==27) return vec2(241.0,65280.0);
        if(yi==28) return vec2(60.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==36){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(448.0,896.0);
        if(yi==6) return vec2(783.0,61632.0);
        if(yi==7) return vec2(1567.0,63584.0);
        if(yi==8) return vec2(3327.0,65328.0);
        if(yi==9) return vec2(3200.0,304.0);
        if(yi==10) return vec2(6312.0,5400.0);
        if(yi==11) return vec2(4737.0,33100.0);
        if(yi==12) return vec2(13987.0,17772.0);
        if(yi==13) return vec2(13955.0,33132.0);
        if(yi==14) return vec2(13979.0,57708.0);
        if(yi==15) return vec2(13963.0,33132.0);
        if(yi==16) return vec2(13963.0,57708.0);
        if(yi==17) return vec2(13991.0,34156.0);
        if(yi==18) return vec2(13955.0,33132.0);
        if(yi==19) return vec2(13954.0,33132.0);
        if(yi==20) return vec2(13898.0,53868.0);
        if(yi==21) return vec2(4899.0,1224.0);
        if(yi==22) return vec2(6544.0,2456.0);
        if(yi==23) return vec2(3273.0,4912.0);
        if(yi==24) return vec2(1636.0,9824.0);
        if(yi==25) return vec2(819.0,52416.0);
        if(yi==26) return vec2(392.0,4480.0);
        if(yi==27) return vec2(227.0,50944.0);
        if(yi==28) return vec2(56.0,7168.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==37){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(248.0,7936.0);
        if(yi==5) return vec2(451.0,50048.0);
        if(yi==6) return vec2(796.0,14528.0);
        if(yi==7) return vec2(1569.0,34400.0);
        if(yi==8) return vec2(3182.0,16688.0);
        if(yi==9) return vec2(3072.0,18352.0);
        if(yi==10) return vec2(6256.0,37912.0);
        if(yi==11) return vec2(6145.0,42712.0);
        if(yi==12) return vec2(12867.0,9612.0);
        if(yi==13) return vec2(12353.0,4716.0);
        if(yi==14) return vec2(12545.0,18764.0);
        if(yi==15) return vec2(12481.0,9388.0);
        if(yi==16) return vec2(12575.0,4652.0);
        if(yi==17) return vec2(12772.0,2604.0);
        if(yi==18) return vec2(12292.0,4652.0);
        if(yi==19) return vec2(14345.0,4684.0);
        if(yi==20) return vec2(6162.0,2648.0);
        if(yi==21) return vec2(6180.0,4760.0);
        if(yi==22) return vec2(3140.0,8880.0);
        if(yi==23) return vec2(3108.0,10544.0);
        if(yi==24) return vec2(1620.0,8800.0);
        if(yi==25) return vec2(778.0,8384.0);
        if(yi==26) return vec2(453.0,896.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==38){
        if(yi==2) return vec2(511.0,65408.0);
        if(yi==3) return vec2(511.0,65408.0);
        if(yi==4) return vec2(511.0,65408.0);
        if(yi==5) return vec2(496.0,3968.0);
        if(yi==6) return vec2(503.0,61312.0);
        if(yi==7) return vec2(263.0,57472.0);
        if(yi==8) return vec2(382.0,32384.0);
        if(yi==9) return vec2(124.0,15872.0);
        if(yi==10) return vec2(504.0,8064.0);
        if(yi==11) return vec2(511.0,65408.0);
        if(yi==12) return vec2(466.0,19328.0);
        if(yi==13) return vec2(466.0,19328.0);
        if(yi==14) return vec2(448.0,896.0);
        if(yi==15) return vec2(208.0,2944.0);
        if(yi==16) return vec2(232.0,5888.0);
        if(yi==17) return vec2(244.0,12032.0);
        if(yi==18) return vec2(1272.0,7712.0);
        if(yi==19) return vec2(3710.0,32368.0);
        if(yi==20) return vec2(7986.0,19704.0);
        if(yi==21) return vec2(3996.0,14832.0);
        if(yi==22) return vec2(1998.0,29664.0);
        if(yi==23) return vec2(999.0,59328.0);
        if(yi==24) return vec2(499.0,53120.0);
        if(yi==25) return vec2(1017.0,40896.0);
        if(yi==26) return vec2(508.0,16256.0);
        if(yi==27) return vec2(127.0,65024.0);
        if(yi==28) return vec2(31.0,63488.0);
        if(yi==29) return vec2(3.0,49152.0);
    }
    else if(t==39){
        if(yi==2) return vec2(127.0,65024.0);
        if(yi==3) return vec2(64.0,512.0);
        if(yi==4) return vec2(65.0,33280.0);
        if(yi==5) return vec2(70.0,25088.0);
        if(yi==6) return vec2(73.0,37376.0);
        if(yi==7) return vec2(70.0,25088.0);
        if(yi==8) return vec2(17.0,34816.0);
        if(yi==9) return vec2(33.0,33792.0);
        if(yi==10) return vec2(76.0,12800.0);
        if(yi==11) return vec2(162.0,17664.0);
        if(yi==12) return vec2(338.0,19072.0);
        if(yi==13) return vec2(320.0,640.0);
        if(yi==14) return vec2(171.0,54528.0);
        if(yi==15) return vec2(81.0,35328.0);
        if(yi==16) return vec2(34.0,17408.0);
        if(yi==17) return vec2(14.0,28672.0);
        if(yi==18) return vec2(72.0,4608.0);
        if(yi==19) return vec2(66.0,16896.0);
        if(yi==20) return vec2(65.0,33280.0);
        if(yi==21) return vec2(64.0,512.0);
        if(yi==22) return vec2(127.0,65024.0);
        if(yi==25) return vec2(19.0,41984.0);
        if(yi==26) return vec2(40.0,37888.0);
        if(yi==27) return vec2(57.0,35840.0);
        if(yi==28) return vec2(40.0,37888.0);
        if(yi==29) return vec2(40.0,41984.0);
    }
    else if(t==40){
        if(yi==2) return vec2(2.0,16384.0);
        if(yi==3) return vec2(1.0,32768.0);
        if(yi==4) return vec2(1.0,32768.0);
        if(yi==5) return vec2(2.0,16384.0);
        if(yi==7) return vec2(511.0,65408.0);
        if(yi==8) return vec2(1018.0,24512.0);
        if(yi==9) return vec2(1004.0,14272.0);
        if(yi==10) return vec2(884.0,11968.0);
        if(yi==11) return vec2(570.0,23616.0);
        if(yi==12) return vec2(893.0,48832.0);
        if(yi==13) return vec2(866.0,18112.0);
        if(yi==14) return vec2(879.0,63168.0);
        if(yi==15) return vec2(875.0,54976.0);
        if(yi==16) return vec2(878.0,30400.0);
        if(yi==17) return vec2(877.0,46784.0);
        if(yi==18) return vec2(877.0,46784.0);
        if(yi==19) return vec2(878.0,30400.0);
        if(yi==20) return vec2(879.0,63168.0);
        if(yi==21) return vec2(563.0,52288.0);
        if(yi==22) return vec2(1020.0,16320.0);
        if(yi==23) return vec2(575.0,64576.0);
        if(yi==24) return vec2(1022.0,32704.0);
        if(yi==25) return vec2(509.0,49024.0);
        if(yi==26) return vec2(509.0,49024.0);
        if(yi==27) return vec2(126.0,32256.0);
        if(yi==28) return vec2(15.0,61440.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==41){
        if(yi==2) return vec2(3.0,49152.0);
        if(yi==3) return vec2(6.0,24576.0);
        if(yi==4) return vec2(13.0,45056.0);
        if(yi==5) return vec2(13.0,45056.0);
        if(yi==6) return vec2(46.0,29696.0);
        if(yi==7) return vec2(87.0,59904.0);
        if(yi==8) return vec2(426.0,21888.0);
        if(yi==9) return vec2(383.0,65152.0);
        if(yi==10) return vec2(383.0,65152.0);
        if(yi==11) return vec2(256.0,128.0);
        if(yi==12) return vec2(382.0,32384.0);
        if(yi==13) return vec2(382.0,32384.0);
        if(yi==14) return vec2(382.0,32384.0);
        if(yi==15) return vec2(382.0,32384.0);
        if(yi==16) return vec2(256.0,128.0);
        if(yi==17) return vec2(256.0,128.0);
        if(yi==18) return vec2(382.0,32384.0);
        if(yi==19) return vec2(382.0,32384.0);
        if(yi==20) return vec2(382.0,32384.0);
        if(yi==21) return vec2(446.0,32128.0);
        if(yi==22) return vec2(222.0,31488.0);
        if(yi==23) return vec2(110.0,30208.0);
        if(yi==24) return vec2(54.0,27648.0);
        if(yi==25) return vec2(26.0,22528.0);
        if(yi==26) return vec2(12.0,12288.0);
        if(yi==27) return vec2(6.0,24576.0);
        if(yi==28) return vec2(3.0,49152.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==42){
        if(yi==3) return vec2(768.0,0.0);
        if(yi==4) return vec2(1152.0,0.0);
        if(yi==5) return vec2(2112.0,0.0);
        if(yi==6) return vec2(1087.0,0.0);
        if(yi==7) return vec2(768.0,32768.0);
        if(yi==8) return vec2(1055.0,51168.0);
        if(yi==9) return vec2(2064.0,144.0);
        if(yi==10) return vec2(4112.0,65160.0);
        if(yi==11) return vec2(8712.0,2180.0);
        if(yi==12) return vec2(10500.0,27572.0);
        if(yi==13) return vec2(4226.0,2196.0);
        if(yi==14) return vec2(81.0,11996.0);
        if(yi==15) return vec2(33.0,10240.0);
        if(yi==16) return vec2(34.0,10240.0);
        if(yi==17) return vec2(68.0,0.0);
        if(yi==18) return vec2(74.0,48736.0);
        if(yi==19) return vec2(147.0,33296.0);
        if(yi==20) return vec2(298.0,48080.0);
        if(yi==21) return vec2(582.0,33296.0);
        if(yi==22) return vec2(642.0,6880.0);
        if(yi==23) return vec2(6401.0,2560.0);
        if(yi==24) return vec2(8704.0,51584.0);
        if(yi==25) return vec2(9216.0,0.0);
        if(yi==26) return vec2(6144.0,0.0);
    }
    else if(t==43){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(240.0,3840.0);
        if(yi==5) return vec2(455.0,58240.0);
        if(yi==6) return vec2(792.0,6336.0);
        if(yi==7) return vec2(1571.0,33888.0);
        if(yi==8) return vec2(3139.0,16944.0);
        if(yi==9) return vec2(3203.0,57648.0);
        if(yi==10) return vec2(6403.0,32920.0);
        if(yi==11) return vec2(6403.0,32920.0);
        if(yi==12) return vec2(16383.0,65532.0);
        if(yi==13) return vec2(8199.0,57348.0);
        if(yi==14) return vec2(16376.0,8188.0);
        if(yi==15) return vec2(12307.0,51212.0);
        if(yi==16) return vec2(16356.0,26620.0);
        if(yi==17) return vec2(14400.0,12828.0);
        if(yi==18) return vec2(16325.0,15356.0);
        if(yi==19) return vec2(13250.0,15308.0);
        if(yi==20) return vec2(4549.0,15240.0);
        if(yi==21) return vec2(6336.0,13080.0);
        if(yi==22) return vec2(2148.0,26128.0);
        if(yi==23) return vec2(3123.0,52272.0);
        if(yi==24) return vec2(1560.0,6240.0);
        if(yi==25) return vec2(783.0,61632.0);
        if(yi==26) return vec2(448.0,896.0);
        if(yi==27) return vec2(240.0,3840.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==44){
        if(yi==2) return vec2(15.0,61440.0);
        if(yi==3) return vec2(63.0,64512.0);
        if(yi==4) return vec2(255.0,65280.0);
        if(yi==5) return vec2(511.0,65408.0);
        if(yi==6) return vec2(1023.0,65472.0);
        if(yi==7) return vec2(2047.0,65504.0);
        if(yi==8) return vec2(4095.0,65520.0);
        if(yi==9) return vec2(4094.0,1008.0);
        if(yi==10) return vec2(8189.0,57848.0);
        if(yi==11) return vec2(8191.0,63736.0);
        if(yi==12) return vec2(16383.0,64636.0);
        if(yi==13) return vec2(16319.0,64572.0);
        if(yi==14) return vec2(16319.0,64572.0);
        if(yi==15) return vec2(15367.0,64572.0);
        if(yi==16) return vec2(16159.0,64572.0);
        if(yi==17) return vec2(15887.0,64572.0);
        if(yi==18) return vec2(16111.0,64572.0);
        if(yi==19) return vec2(16383.0,64636.0);
        if(yi==20) return vec2(8191.0,63736.0);
        if(yi==21) return vec2(8189.0,57848.0);
        if(yi==22) return vec2(4094.0,1008.0);
        if(yi==23) return vec2(4095.0,65520.0);
        if(yi==24) return vec2(2047.0,65504.0);
        if(yi==25) return vec2(1023.0,65472.0);
        if(yi==26) return vec2(511.0,65408.0);
        if(yi==27) return vec2(255.0,65280.0);
        if(yi==28) return vec2(63.0,64512.0);
        if(yi==29) return vec2(15.0,61440.0);
    }
    else if(t==45){
        if(yi==2) return vec2(2047.0,65504.0);
        if(yi==3) return vec2(1052.0,13088.0);
        if(yi==4) return vec2(1224.0,4896.0);
        if(yi==5) return vec2(1225.0,37664.0);
        if(yi==6) return vec2(1097.0,37664.0);
        if(yi==7) return vec2(1039.0,37664.0);
        if(yi==8) return vec2(1166.0,12832.0);
        if(yi==9) return vec2(1228.0,28768.0);
        if(yi==10) return vec2(1993.0,61920.0);
        if(yi==11) return vec2(1993.0,37856.0);
        if(yi==12) return vec2(1529.0,40864.0);
        if(yi==13) return vec2(1273.0,40736.0);
        if(yi==14) return vec2(1212.0,15648.0);
        if(yi==15) return vec2(1182.0,31008.0);
        if(yi==16) return vec2(1175.0,59680.0);
        if(yi==17) return vec2(1171.0,51488.0);
        if(yi==18) return vec2(1170.0,18720.0);
        if(yi==19) return vec2(1170.0,18720.0);
        if(yi==20) return vec2(1682.0,18784.0);
        if(yi==21) return vec2(914.0,18880.0);
        if(yi==22) return vec2(402.0,18816.0);
        if(yi==23) return vec2(210.0,19200.0);
        if(yi==24) return vec2(114.0,19968.0);
        if(yi==25) return vec2(50.0,19456.0);
        if(yi==26) return vec2(26.0,22528.0);
        if(yi==27) return vec2(14.0,28672.0);
        if(yi==28) return vec2(6.0,24576.0);
        if(yi==29) return vec2(3.0,49152.0);
    }
    else if(t==46){
        if(yi==2) return vec2(330.0,21120.0);
        if(yi==3) return vec2(132.0,8448.0);
        if(yi==4) return vec2(330.0,21120.0);
        if(yi==6) return vec2(1023.0,65472.0);
        if(yi==7) return vec2(512.0,64.0);
        if(yi==8) return vec2(551.0,58432.0);
        if(yi==9) return vec2(598.0,27200.0);
        if(yi==10) return vec2(549.0,42048.0);
        if(yi==11) return vec2(597.0,43584.0);
        if(yi==12) return vec2(551.0,58432.0);
        if(yi==13) return vec2(597.0,43584.0);
        if(yi==14) return vec2(548.0,9280.0);
        if(yi==15) return vec2(599.0,59968.0);
        if(yi==16) return vec2(548.0,9280.0);
        if(yi==17) return vec2(551.0,42048.0);
        if(yi==18) return vec2(518.0,8256.0);
        if(yi==19) return vec2(519.0,41024.0);
        if(yi==20) return vec2(512.0,64.0);
        if(yi==21) return vec2(633.0,40512.0);
        if(yi==22) return vec2(514.0,16448.0);
        if(yi==23) return vec2(629.0,44608.0);
        if(yi==24) return vec2(261.0,41088.0);
        if(yi==25) return vec2(306.0,19584.0);
        if(yi==26) return vec2(129.0,33024.0);
        if(yi==27) return vec2(112.0,3584.0);
        if(yi==28) return vec2(14.0,28672.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    else if(t==47){
        if(yi==2) return vec2(1.0,32768.0);
        if(yi==3) return vec2(7.0,57344.0);
        if(yi==4) return vec2(31.0,63488.0);
        if(yi==5) return vec2(127.0,65024.0);
        if(yi==6) return vec2(511.0,65408.0);
        if(yi==7) return vec2(2047.0,65504.0);
        if(yi==8) return vec2(8068.0,11768.0);
        if(yi==9) return vec2(16311.0,44540.0);
        if(yi==10) return vec2(16311.0,44540.0);
        if(yi==11) return vec2(16262.0,11772.0);
        if(yi==12) return vec2(16311.0,44540.0);
        if(yi==13) return vec2(8119.0,44536.0);
        if(yi==14) return vec2(8119.0,41464.0);
        if(yi==15) return vec2(8191.0,65528.0);
        if(yi==16) return vec2(4095.0,65520.0);
        if(yi==17) return vec2(4092.0,16368.0);
        if(yi==18) return vec2(2043.0,57312.0);
        if(yi==19) return vec2(2038.0,28640.0);
        if(yi==20) return vec2(1005.0,47040.0);
        if(yi==21) return vec2(1002.0,22464.0);
        if(yi==22) return vec2(490.0,22400.0);
        if(yi==23) return vec2(237.0,46848.0);
        if(yi==24) return vec2(118.0,28160.0);
        if(yi==25) return vec2(59.0,56320.0);
        if(yi==26) return vec2(28.0,14336.0);
        if(yi==27) return vec2(15.0,61440.0);
        if(yi==28) return vec2(7.0,57344.0);
        if(yi==29) return vec2(1.0,32768.0);
    }
    return vec2(0.0);
}
vec3 teamCol(int t){
    if(t==0) return vec3(0.157,0.863,0.471);
    if(t==1) return vec3(0.431,0.784,1.000);
    if(t==2) return vec3(1.000,0.824,0.157);
    if(t==3) return vec3(1.000,0.275,0.353);
    if(t==4) return vec3(1.000,0.804,0.000);
    if(t==5) return vec3(1.000,0.824,0.157);
    if(t==6) return vec3(0.235,0.922,0.510);
    if(t==7) return vec3(0.235,0.549,1.000);
    if(t==8) return vec3(1.000,0.275,0.353);
    if(t==9) return vec3(1.000,0.882,0.157);
    if(t==10) return vec3(1.000,0.549,0.157);
    if(t==11) return vec3(1.000,0.275,0.353);
    if(t==12) return vec3(0.235,0.549,1.000);
    if(t==13) return vec3(0.353,0.588,1.000);
    if(t==14) return vec3(0.353,0.784,1.000);
    if(t==15) return vec3(1.000,0.824,0.157);
    if(t==16) return vec3(1.000,0.275,0.353);
    if(t==17) return vec3(0.353,0.588,1.000);
    if(t==18) return vec3(0.353,0.588,1.000);
    if(t==19) return vec3(1.000,0.824,0.157);
    if(t==20) return vec3(1.000,0.824,0.157);
    if(t==21) return vec3(0.353,0.588,1.000);
    if(t==22) return vec3(0.235,0.922,0.510);
    if(t==23) return vec3(0.235,0.922,0.510);
    if(t==24) return vec3(1.000,0.275,0.431);
    if(t==25) return vec3(1.000,0.275,0.353);
    if(t==26) return vec3(0.235,0.922,0.510);
    if(t==27) return vec3(1.000,0.275,0.353);
    if(t==28) return vec3(1.000,0.549,0.157);
    if(t==29) return vec3(0.784,0.863,1.000);
    if(t==30) return vec3(0.353,0.588,1.000);
    if(t==31) return vec3(0.353,0.588,1.000);
    if(t==32) return vec3(1.000,0.275,0.353);
    if(t==33) return vec3(0.235,0.922,0.510);
    if(t==34) return vec3(0.667,0.157,0.353);
    if(t==35) return vec3(0.235,0.922,0.510);
    if(t==36) return vec3(0.353,0.588,1.000);
    if(t==37) return vec3(0.235,0.922,0.510);
    if(t==38) return vec3(0.235,0.922,0.510);
    if(t==39) return vec3(0.353,0.588,1.000);
    if(t==40) return vec3(1.000,0.824,0.157);
    if(t==41) return vec3(1.000,0.824,0.157);
    if(t==42) return vec3(1.000,0.275,0.353);
    if(t==43) return vec3(1.000,0.275,0.353);
    if(t==44) return vec3(1.000,0.275,0.353);
    if(t==45) return vec3(0.353,0.588,1.000);
    if(t==46) return vec3(0.353,0.706,1.000);
    if(t==47) return vec3(0.235,0.922,0.510);
    return vec3(1.0);
}

// ════════════════════════════════════════════════════════════════════════
//  WORLD CUP — NEON 3D CRESTS
//  48 national crests as extruded neon pixel-slabs on a black void with a
//  synthwave grid floor + reflection. Auto-cycles through every team, or
//  pick two for a head-to-head VERSUS face-off. Audio reactive.
// ════════════════════════════════════════════════════════════════════════

#define PI  3.14159265359
#define TAU 6.28318530718

// ── globals set in main() ────────────────────────────────────────────────
float gPulse;    // bass-driven pulse 0..~1
float gHigh;     // treble shimmer
float gFlicker;  // neon-sign flicker multiplier (1.0 = steady)

// ── hashes ───────────────────────────────────────────────────────────────
float hsh11(float n){ return fract(sin(n*127.1)*43758.5453); }
float hsh21(vec2 p){ return fract(sin(dot(p,vec2(127.1,311.7)))*43758.5453); }

// ── neon-sign flicker: mostly steady, rare dropouts + micro shimmer ──────
float neonFlicker(float t){
    float micro = 0.94 + 0.06*sin(t*42.0);
    float seed  = hsh11(floor(t*22.0));
    float dip   = (seed > 0.90) ? mix(0.30, 0.8, hsh11(floor(t*22.0)+3.0)) : 1.0;
    float buzz  = (hsh11(floor(t*9.0)) > 0.96) ? 0.6 : 1.0;
    return micro*dip*buzz;
}

// ════════ 3x5 PIXEL FONT — 0-9, V(10) S(11) -(12) G(13) O(14) A(15) L(16) !(17)
// each glyph = 5 rows, each row 3 bits (bit2=left col)
int frowAt(int i){
    if(i==0) return 7;
    if(i==1) return 5;
    if(i==2) return 5;
    if(i==3) return 5;
    if(i==4) return 7;
    if(i==5) return 2;
    if(i==6) return 6;
    if(i==7) return 2;
    if(i==8) return 2;
    if(i==9) return 7;
    if(i==10) return 7;
    if(i==11) return 1;
    if(i==12) return 7;
    if(i==13) return 4;
    if(i==14) return 7;
    if(i==15) return 7;
    if(i==16) return 1;
    if(i==17) return 7;
    if(i==18) return 1;
    if(i==19) return 7;
    if(i==20) return 5;
    if(i==21) return 5;
    if(i==22) return 7;
    if(i==23) return 1;
    if(i==24) return 1;
    if(i==25) return 7;
    if(i==26) return 4;
    if(i==27) return 7;
    if(i==28) return 1;
    if(i==29) return 7;
    if(i==30) return 7;
    if(i==31) return 4;
    if(i==32) return 7;
    if(i==33) return 5;
    if(i==34) return 7;
    if(i==35) return 7;
    if(i==36) return 1;
    if(i==37) return 2;
    if(i==38) return 2;
    if(i==39) return 2;
    if(i==40) return 7;
    if(i==41) return 5;
    if(i==42) return 7;
    if(i==43) return 5;
    if(i==44) return 7;
    if(i==45) return 7;
    if(i==46) return 5;
    if(i==47) return 7;
    if(i==48) return 1;
    if(i==49) return 7;
    if(i==50) return 5;
    if(i==51) return 5;
    if(i==52) return 5;
    if(i==53) return 5;
    if(i==54) return 2;
    if(i==55) return 7;
    if(i==56) return 4;
    if(i==57) return 7;
    if(i==58) return 1;
    if(i==59) return 7;
    if(i==60) return 0;
    if(i==61) return 0;
    if(i==62) return 7;
    if(i==63) return 0;
    if(i==64) return 0;
    if(i==65) return 7;
    if(i==66) return 4;
    if(i==67) return 5;
    if(i==68) return 5;
    if(i==69) return 7;
    if(i==70) return 7;
    if(i==71) return 5;
    if(i==72) return 5;
    if(i==73) return 5;
    if(i==74) return 7;
    if(i==75) return 2;
    if(i==76) return 5;
    if(i==77) return 7;
    if(i==78) return 5;
    if(i==79) return 5;
    if(i==80) return 4;
    if(i==81) return 4;
    if(i==82) return 4;
    if(i==83) return 4;
    if(i==84) return 7;
    if(i==85) return 2;
    if(i==86) return 2;
    if(i==87) return 2;
    if(i==88) return 0;
    if(i==89) return 2;
    return 0;
}
float glyphPix(int g, vec2 uv){          // uv in [-1,1]
    if(abs(uv.x) > 1.0 || abs(uv.y) > 1.0) return 0.0;
    int cx = int((uv.x*0.5 + 0.5)*3.0);
    int cy = int((0.5 - uv.y*0.5)*5.0);
    if(cx<0) cx=0; if(cx>2) cx=2;   // int clamp (ES 1.0 has none)
    if(cy<0) cy=0; if(cy>4) cy=4;
    int row = frowAt(g*5 + cy);
    return (mod(floor(float(row)/exp2(float(2-cx))), 2.0) >= 0.5) ? 1.0 : 0.0;
}
float glyphAt(vec2 p, vec2 ctr, vec2 hsz, int g){
    return glyphPix(g, (p - ctr)/hsz);
}
float numAt(vec2 p, vec2 ctr, vec2 hsz, int val){   // 1-2 digits, centered
    if(val<0) val=0; if(val>99) val=99;   // int clamp (ES 1.0 has none)
    if(val < 10) return glyphAt(p, ctr, hsz, val);
    float gw = hsz.x*1.35;
    int tens = val/10;
    return max(glyphAt(p, ctr-vec2(gw,0.0), hsz, tens),
               glyphAt(p, ctr+vec2(gw,0.0), hsz, val - tens*10));
}

// ── crest bitmap lookup ──────────────────────────────────────────────────
// team t, local coords uv in roughly [-1,1]; returns 1.0 on a black crest pixel
float logoBit(int t, vec2 uv){
    vec2 z = uv * 0.92;                       // slight zoom past the padding
    float px = (z.x*0.5 + 0.5) * 32.0;
    float py = (0.5 - z.y*0.5) * 32.0;        // flip Y (bitmap is top-down)
    if(px < 0.0 || px >= 32.0 || py < 0.0 || py >= 32.0) return 0.0;
    int xi = int(px), yi = int(py);
    vec2 row = logoRow(t, yi);                 // (hi16, lo16)
    float half16 = (xi < 16) ? row.y : row.x;
    float bit    = float((xi < 16) ? xi : xi - 16);
    return (mod(floor(half16/exp2(bit)), 2.0) >= 0.5) ? 1.0 : 0.0;
}

// 4-tap coverage for edge anti-aliasing
float logoCov(int t, vec2 uv, float aa){
    float s = logoBit(t, uv + vec2(-aa,-aa));
    s += logoBit(t, uv + vec2( aa,-aa));
    s += logoBit(t, uv + vec2(-aa, aa));
    s += logoBit(t, uv + vec2( aa, aa));
    return s * 0.25;
}

// ── one extruded neon crest card ─────────────────────────────────────────
// p: screen pt (y up). c: center. s: half-height scale. yaw: rotation.
// returns rgb = neon emissive, a = solid coverage
vec4 card(vec2 p, vec2 c, float s, float yaw, int t, vec3 col){
    vec2 q = (p - c) / s;
    if(abs(q.y) > 1.25 || abs(q.x) > 1.8) return vec4(0.0);

    float sa = sin(yaw), ca = cos(yaw);
    float caa = max(abs(ca), 0.14);
    float t2 = uExtrude * (0.5 + 0.9*gPulse);   // half-thickness, breathes with bass

    const int L = 14;
    float hd = -1.0; float ucoord = 0.0;
    // march depth layers front -> back, first crest hit is the visible surface
    for(int i=0; i<L; i++){
        float f  = float(i)/float(L-1);
        float dz = mix(-t2, t2, f);
        float u  = (q.x - dz*sa)/caa;
        if(logoBit(t, vec2(u, q.y)) > 0.5){ hd = f; ucoord = u; break; }
    }
    if(hd < 0.0) return vec4(0.0);

    float cov = logoCov(t, vec2(ucoord, q.y), 0.018);
    cov = max(cov, 0.4);

    float depthShade = mix(1.0, 0.26, hd);                 // side walls fall off dark
    float sheen      = 0.62 + 0.45*smoothstep(-1.0, 1.0, q.y);

    vec3 e = col * (uNeon * 1.35 * depthShade);
    // white-hot core on the front face
    e += vec3(1.0) * uNeon * 0.55 * depthShade * smoothstep(0.5, 0.0, hd);
    e *= sheen;
    e *= (1.0 + gPulse*1.8 + gHigh*0.9);
    return vec4(e, cov);
}

// ── soft neon halo around a crest silhouette ─────────────────────────────
float halo(vec2 p, vec2 c, float s, int t){
    vec2 q = (p - c)/s;
    if(abs(q.x) > 1.8 || abs(q.y) > 1.8) return 0.0;
    float r = 0.045 + 0.16*uGlowSize;
    float g = 0.0;
    const int K = 10;
    for(int i=0; i<K; i++){
        float a = float(i)/float(K) * TAU;
        vec2 d = vec2(cos(a), sin(a));
        g += logoBit(t, q + d*r);
        g += logoBit(t, q + d*r*0.5);
    }
    return g/float(K*2);
}

// ── GOAL confetti — team-colored bits raining down, neon flicker ─────────
vec3 confetti(vec2 p, float amt, vec3 cA, vec3 cB){
    if(amt <= 0.001) return vec3(0.0);
    vec3 e = vec3(0.0);
    for(int l=0; l<3; l++){
        float fl    = float(l);
        float scale = 16.0 + fl*9.0;
        float speed = 0.55 + 0.30*fl;
        vec2 q = p * scale;
        q.x += sin(p.y*3.0 + fl*2.0 + TIME)*0.6;   // drift sway
        q.y += TIME * speed * scale * 0.16;          // fall downward
        vec2 cell = floor(q);
        vec2 f    = fract(q) - 0.5;
        float h = hsh21(cell + fl*41.0);
        if(h > 0.5){
            float rot = (h-0.5)*6.0 + TIME*4.0*sign(h-0.75);
            float cs = cos(rot), sn = sin(rot);
            vec2 fp = mat2(cs,-sn,sn,cs)*f;
            float piece = step(abs(fp.x),0.20)*step(abs(fp.y),0.34);
            float fk = 0.55 + 0.45*sin(TIME*26.0*h + h*60.0);   // per-bit flicker
            vec3 cc = mix(cA, cB, step(0.5, fract(h*7.3)));
            e += cc * piece * fk * (0.7 + 0.5*fl);
        }
    }
    return e * amt * 1.4;
}

// ── "GOAL!" wordmark (G O A L !) ─────────────────────────────────────────
float goalWord(vec2 uv, vec2 ctr, float sz){
    vec2 gh = vec2(0.5,0.85)*sz;
    float sp = 1.5*sz;
    float m = 0.0;
    m = max(m, glyphAt(uv, ctr+vec2(-2.0*sp,0.0), gh, 13));   // G
    m = max(m, glyphAt(uv, ctr+vec2(-1.0*sp,0.0), gh, 14));   // O
    m = max(m, glyphAt(uv, ctr+vec2( 0.0*sp,0.0), gh, 15));   // A
    m = max(m, glyphAt(uv, ctr+vec2( 1.0*sp,0.0), gh, 16));   // L
    m = max(m, glyphAt(uv, ctr+vec2( 2.0*sp,0.0), gh, 17));   // !
    return m;
}

// ── GOAL celebration — a triggerable looping animation (held = it plays) ─
//   arc per loop: burst-in → confetti rain + shockwave → fade. "GOAL!" punches.
vec3 goalCelebration(vec2 uv, float amt, vec3 cA, vec3 cB){
    if(amt <= 0.001) return vec3(0.0);
    float gp  = fract(TIME*0.42);                               // ~2.4s celebration loop
    float env = smoothstep(0.0,0.05,gp) * (1.0 - smoothstep(0.55,1.0,gp));
    vec3 e = vec3(0.0);
    // confetti rains (keeps a little baseline so it never fully empties)
    e += confetti(uv, amt*max(env,0.30), cA, cB);
    // shockwave ring bursting from the title
    float rr   = length(uv - vec2(0.0,0.30));
    float wave = exp(-pow((rr - gp*1.7)*5.0, 2.0)) * env;
    e += mix(cA,cB,0.5) * wave * 2.0;
    // "GOAL!" punches in (scale-in) and flickers like neon
    float sz   = mix(0.10, 0.075, smoothstep(0.0,0.18,gp));
    float g    = goalWord(uv, vec2(0.0,0.30), sz);
    float fk   = 0.6 + 0.4*sin(TIME*30.0);
    vec3  tcol = mix(vec3(1.0), mix(cA,cB,0.5+0.5*sin(TIME*6.0)), 0.45);
    e += g * tcol * (1.6 + env*1.6) * fk;
    return e * amt;
}

// ── VS + live score between the two crests (versus mode) ─────────────────
vec3 vsOverlay(vec2 uv, float cardY, vec3 cA, vec3 cB){
    vec3 e = vec3(0.0);
    // "VS" in the gap, white-hot, sharing the neon flicker
    vec2 gh = vec2(0.034, 0.058);
    vec2 vc = vec2(0.0, cardY + 0.02);
    float vs = max(glyphAt(uv, vc - vec2(0.044,0.0), gh, 10),
                   glyphAt(uv, vc + vec2(0.044,0.0), gh, 11));
    e += vec3(1.0) * vs * 1.8 * gFlicker;
    // score row below, each number in its team's color
    if(uShowScore > 0.5){
        vec2 sh = vec2(0.028, 0.05);
        vec2 sc = vec2(0.0, cardY - 0.34);
        e += cA * numAt(uv, sc - vec2(0.11,0.0), sh, int(uScoreA)) * 1.7;
        e += cB * numAt(uv, sc + vec2(0.11,0.0), sh, int(uScoreB)) * 1.7;
        e += vec3(0.9) * glyphAt(uv, sc, vec2(0.026,0.05), 12) * 1.2;
    }
    return e;
}

// ── synthwave grid floor ─────────────────────────────────────────────────
vec3 gridFloor(vec2 uv, float horizon, vec3 tint){
    if(uv.y >= horizon) return vec3(0.0);
    float d = horizon - uv.y;
    float persp = 1.0/(d + 0.05);
    float gz = (d*persp)*0.55 - TIME*0.9;            // receding lines scroll toward viewer
    float gx = uv.x*persp*0.85;
    float lz = abs(fract(gz) - 0.5);
    float lx = abs(fract(gx) - 0.5);
    float lineZ = smoothstep(0.49, 0.5, lz);
    float lineX = smoothstep(0.47, 0.5, lx);
    float grid = max(lineZ, lineX);
    float fade = exp(-d*2.2) * smoothstep(0.0, 0.08, d);  // fade at horizon + into distance
    vec3 gcol = mix(vec3(0.20,0.35,0.9), tint, 0.5);
    return gcol * grid * fade * (0.5 + 1.6*gPulse) * 0.9;
}

// ── all crests for the current mode (emissive only) ──────────────────────
vec3 crests(vec2 p, float cardY){
    vec3 e = vec3(0.0);
    if(uMode < 0.5){
        // ── CYCLE ── advance through every team with a hidden edge-on flip
        float hold = max(uHoldTime, 0.4);
        float gt = TIME/hold;
        int idx  = int(mod(floor(gt), float(NTEAM)));
        float ph = fract(gt);
        float yaw = sin(TIME*uSpinSpeed)*uSpinAmt;
        int showIdx = idx;
        float flipFrac = 0.22;
        if(ph > 1.0 - flipFrac){
            float k = (ph - (1.0-flipFrac))/flipFrac;     // 0..1
            float sp = k*PI;
            if(k > 0.5){ showIdx = int(mod(float(idx+1), float(NTEAM))); sp = k*PI - PI; }
            yaw += sp;                                     // half-turn flip, swap hidden at edge-on
        }
        vec3 col = teamCol(showIdx);
        float sc = 0.25 * uZoom;
        vec4 s = card(p, vec2(0.0, cardY), sc, yaw, showIdx, col);
        e += s.rgb;
        e += col * halo(p, vec2(0.0, cardY), sc, showIdx) * uNeon * 1.3 * (1.0 + gPulse*1.4 + gHigh*0.8);
    } else {
        // ── VERSUS ── two crests angled toward each other
        int tA = clamp(uTeamA, 0, NTEAM-1);
        int tB = clamp(uTeamB, 0, NTEAM-1);
        vec3 cA = teamCol(tA), cB = teamCol(tB);
        float off = 0.40 * uZoom;
        float sc  = 0.26 * uZoom;
        float bob = sin(TIME*1.3)*0.02;
        // crests spin/wobble on the Y axis around an inward-facing tilt
        float spin = sin(TIME*uSpinSpeed)*uSpinAmt;
        vec2 pa = vec2(-off, cardY + bob);
        vec2 pb = vec2( off, cardY - bob);
        e += card(p, pa, sc,  0.28 + spin, tA, cA).rgb;
        e += card(p, pb, sc, -0.28 - spin, tB, cB).rgb;
        e += cA * halo(p, pa, sc, tA) * uNeon * 1.15 * (1.0 + gPulse*1.4 + gHigh*0.8);
        e += cB * halo(p, pb, sc, tB) * uNeon * 1.15 * (1.0 + gPulse*1.4 + gHigh*0.8);
        // clash glow in the middle
        vec2 m = p - vec2(0.0, cardY);
        float cl = exp(-dot(m,m)*7.0);
        e += mix(cA, cB, 0.5) * cl * (0.45 + gPulse*0.9) * (0.65 + 0.35*sin(TIME*5.0));
    }
    return e * gFlicker;          // whole sign flickers together like neon
}

void main(){
    vec2 R  = RENDERSIZE;
    vec2 uv = (gl_FragCoord.xy - 0.5*R)/R.y;     // centered, y up, aspect by height

    gPulse = clamp(audioBass, 0.0, 1.0) * audioReact;
    gHigh  = clamp(audioHigh, 0.0, 1.0) * audioReact;
    gFlicker = mix(1.0, neonFlicker(TIME), uFlicker);

    float cardY   = 0.10 + uCamHeight;
    float horizon = -0.30 + uCamHeight*0.6;

    // team colors on screen (for tint, confetti, score)
    int idxC = int(mod(floor(TIME/max(uHoldTime,0.4)), float(NTEAM)));
    int iA = clamp(uTeamA, 0, NTEAM-1);
    int iB = clamp(uTeamB, 0, NTEAM-1);
    vec3 colA = (uMode < 0.5) ? teamCol(idxC) : teamCol(iA);
    vec3 colB = (uMode < 0.5) ? teamCol(idxC) : teamCol(iB);
    vec3 tint = mix(colA, colB, 0.5);

    bool bg = (uRemoveBg < 0.5);   // background on unless "remove background"

    // ── background (skipped when background removed) ──
    vec3 col = vec3(0.0);
    if(bg){
        // faint nebula glow behind the crest
        float neb = exp(-length(uv - vec2(0.0, cardY))*1.6);
        col += tint * neb * 0.10 * (0.5 + 2.0*gPulse);
        if(uGridOn > 0.5) col += gridFloor(uv, horizon, tint);
        // floor reflection of the crests
        if(uFloorOn > 0.5 && uv.y < horizon){
            vec2 ruv = vec2(uv.x, 2.0*horizon - uv.y);
            float rf = exp(-(horizon - uv.y)*3.2) * 0.45;
            col += crests(ruv, cardY) * rf;
        }
    }

    // ── crests ──
    col += crests(uv, cardY);

    // ── VS + live score (versus mode) ──
    if(uMode > 0.5) col += vsOverlay(uv, cardY, colA, colB);

    // ── GOAL celebration (triggerable animation) ──
    col += goalCelebration(uv, uGoal, colA, colB);

    // ── post ──
    // soft bloom-ish lift
    col += col*col*0.18;
    // chromatic shimmer on the treble
    // tonemap
    col = col/(1.0+col);
    col = pow(col, vec3(0.85));
    // scanlines
    col *= 1.0 - uScanline*0.12*(0.5+0.5*sin(gl_FragCoord.y*1.4 + TIME*2.0));
    // vignette (only with a background)
    if(uRemoveBg < 0.5){
        float vig = smoothstep(1.45, 0.35, length(uv*vec2(R.x/R.y,1.0)));
        col *= mix(1.0, vig, uVignette);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    // background: tint the darkest end (the black void) toward bgColor
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    col = uc;

    // alpha: opaque normally; with background removed, key on luminance so only
    // the neon crests / text / confetti show on a transparent canvas
    float alpha = 1.0;
    if(uRemoveBg > 0.5){
        float lum = max(col.r, max(col.g, col.b));
        alpha = smoothstep(0.015, 0.16, lum);
    }
    gl_FragColor = vec4(col, alpha);
}
