using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace OnTogetherDayAndNight
{
    [BepInPlugin("cn.ontogether.dayandnight", "On-Together: Day and Night", PluginVersion)]
    public sealed class DayAndNightPlugin : BaseUnityPlugin
    {
        // Iterations on top of the released 1.0.1 while the autumn update is being tried out.
        // The Thunderstore manifest stays at the last published x.y.z until a release is cut.
        public const string PluginVersion = "1.0.1.4";

        // -------- grading config --------
        private ConfigEntry<bool> _gradingEnabled;
        private ConfigEntry<float> _temperature;
        private ConfigEntry<float> _tint;
        private ConfigEntry<float> _postExposure;
        private ConfigEntry<float> _contrast;
        private ConfigEntry<float> _saturation;
        private ConfigEntry<Color> _dayColorFilter;
        private ConfigEntry<float> _shadowCooling;
        private ConfigEntry<string> _tonemapping;
        private ConfigEntry<bool> _disableFilmGrain;
        private ConfigEntry<bool> _disableDepthOfField;
        private ConfigEntry<bool> _disableMotionBlur;
        private ConfigEntry<bool> _disableBloom;
        private ConfigEntry<bool> _disableVignette;
        private ConfigEntry<bool> _neutralizeSplitToning;

        // -------- sun config --------
        private ConfigEntry<bool> _adjustSun;
        private ConfigEntry<float> _sunElevation;
        private ConfigEntry<float> _sunAzimuth;
        private ConfigEntry<float> _sunIntensity;
        private ConfigEntry<float> _sunShadowStrength;
        private ConfigEntry<float> _sunNeutralize;
        private ConfigEntry<bool> _forceShadowCasting;
        private ConfigEntry<bool> _restoreTreeShadows;
        private ConfigEntry<bool> _hardShadows;
        private ConfigEntry<int> _shadowMapResolution;
        private ConfigEntry<int> _shadowCascadeCount;
        private ConfigEntry<bool> _stabilizeShadowEdges;
        private ConfigEntry<bool> _dappledCanopyShadows;
        private ConfigEntry<float> _canopyShadowGaps;

        // -------- season config --------
        private ConfigEntry<string> _seasonTheme;
        private ConfigEntry<float> _autumnStrength;
        private ConfigEntry<bool> _autumnGroundRecolor;
        private ConfigEntry<float> _canopyShading;
        private ConfigEntry<float> _canopyShadeBrightness;
        private ConfigEntry<float> _autumnWarmFilter;
        private ConfigEntry<float> _leafFallRate;
        private ConfigEntry<int> _leafFallTrees;
        private ConfigEntry<int> _groundLeafLimit;
        private ConfigEntry<float> _groundLeafLifetime;
        private ConfigEntry<bool> _halloweenStringLights;

        // -------- day cycle config --------
        private ConfigEntry<bool> _dayCycleEnabled;
        private ConfigEntry<float> _cycleMinutes;
        private ConfigEntry<float> _noonMinute;
        private ConfigEntry<float> _maxElevation;
        private ConfigEntry<float> _nightBrightness;
        private ConfigEntry<float> _minimumNightLight;
        private ConfigEntry<float> _nightExposureDip;
        private ConfigEntry<float> _shadowDistance;

        // -------- lamp config --------
        private ConfigEntry<bool> _lampsEnabled;
        private ConfigEntry<string> _lampStyle;
        private ConfigEntry<string> _lampKeywords;
        private ConfigEntry<int> _maxLitLamps;
        private ConfigEntry<float> _lampRange;
        private ConfigEntry<float> _lampIntensity;
        private ConfigEntry<float> _haloSize;
        private ConfigEntry<bool> _deckStringLightsEnabled;
        private ConfigEntry<float> _deckStringLightRange;
        private ConfigEntry<float> _deckStringLightIntensity;
        private ConfigEntry<float> _deckStringColorCycleSeconds;
        private ConfigEntry<float> _deskLampRange;
        private ConfigEntry<float> _deskLampIntensity;
        private ConfigEntry<bool> _campfireLightsEnabled;
        private ConfigEntry<bool> _treeLanternLightsEnabled;
        private ConfigEntry<bool> _playTowerSunLightEnabled;
        private ConfigEntry<bool> _deviceScreensGlow;

        // -------- water config --------
        private ConfigEntry<bool> _waterDaylight;
        private ConfigEntry<float> _nightWaterBrightness;
        private ConfigEntry<Color> _dayWaterPatternColor;
        private ConfigEntry<Color> _nightWaterPatternColor;
        private ConfigEntry<float> _waterWaveBrightness;
        private ConfigEntry<float> _nightWaterfallFoamBrightness;
        private ConfigEntry<float> _seaSkyReflection;
        private ConfigEntry<float> _twilightSeaBrightness;
        private ConfigEntry<bool> _sunGlitterEnabled;
        private ConfigEntry<float> _sunGlitterStrength;
        private ConfigEntry<float> _sunGlitterCoverage;
        private ConfigEntry<float> _sunGlitterSpread;
        private ConfigEntry<bool> _waterShadowOverlayEnabled;
        private ConfigEntry<float> _waterShadowOverlayStrength;

        // -------- toon shading config --------
        private ConfigEntry<bool> _toonShading;
        private ConfigEntry<float> _shadeDarkness;
        private ConfigEntry<float> _surfaceShadeStep;
        private ConfigEntry<float> _surfaceNoonShadeStep;
        private ConfigEntry<float> _surfaceSystemShadowLevel;
        private ConfigEntry<float> _playerShadeDarkness;
        private ConfigEntry<float> _playerSystemShadowLevel;
        private ConfigEntry<float> _playerShadeStep;

        // -------- sky config --------
        private ConfigEntry<bool> _replaceSkybox;
        private ConfigEntry<float> _atmosphereThickness;
        private ConfigEntry<float> _skyExposure;
        private ConfigEntry<float> _sunSize;
        private ConfigEntry<Color> _skyTint;
        private ConfigEntry<Color> _groundColor;
        private ConfigEntry<bool> _moonEnabled;
        private ConfigEntry<float> _lunarCycleDays;
        private ConfigEntry<float> _newMoonBrightness;
        private ConfigEntry<float> _moonSize;
        private ConfigEntry<Color> _moonColor;
        private ConfigEntry<bool> _sunsetGlowEnabled;
        private ConfigEntry<float> _sunsetGlowStrength;
        private ConfigEntry<bool> _freezeTime;
        private ConfigEntry<float> _frozenHour;
        private ConfigEntry<int> _twilightVariantOverride;
        private float _lastCyclePhase;
        private int _lastVariantOverride = -1;
        private ConfigEntry<float> _pinkDuskChance;
        private ConfigEntry<float> _redDuskChance;
        private ConfigEntry<float> _twilightWorldTint;
        private ConfigEntry<float> _twilightWidth;
        private ConfigEntry<float> _twilightFalloff;
        private ConfigEntry<bool> _starsEnabled;
        private ConfigEntry<int> _starCount;
        private ConfigEntry<float> _meteorChance;

        // -------- cloud config --------
        private ConfigEntry<bool> _cloudsEnabled;
        private ConfigEntry<int> _cloudCount;
        private ConfigEntry<bool> _cloudShadows;
        private ConfigEntry<float> _cloudSpeed;
        private ConfigEntry<float> _cloudDirection;
        private ConfigEntry<float> _cloudAltitude;
        private ConfigEntry<float> _cloudScale;
        private ConfigEntry<bool> _hideVanillaClouds;

        // -------- weather config --------
        private ConfigEntry<bool> _weatherEnabled;
        private ConfigEntry<bool> _randomRain;
        private ConfigEntry<string> _rainToggleKey;
        private ConfigEntry<float> _clearMinutesMin;
        private ConfigEntry<float> _clearMinutesMax;
        private ConfigEntry<float> _rainMinutesMin;
        private ConfigEntry<float> _rainMinutesMax;
        private ConfigEntry<float> _weatherTransitionSeconds;
        private ConfigEntry<float> _rainLightMultiplier;
        private ConfigEntry<float> _rainExposureDip;
        private ConfigEntry<float> _rainAudioVolume;
        private ConfigEntry<float> _thunderChancePerMinute;
        private ConfigEntry<float> _rainDropSize;
        private ConfigEntry<float> _rainDensity;

        // -------- decor config --------
        private ConfigEntry<bool> _decorDimming;
        private ConfigEntry<float> _nightDecorBrightness;
        private ConfigEntry<string> _decorExcludeKeywords;

        // -------- ambient config --------
        private ConfigEntry<float> _ambientCoolShift;
        private ConfigEntry<float> _ambientIntensity;

        // -------- nature ambience config --------
        private ConfigEntry<bool> _natureAmbienceEnabled;
        private ConfigEntry<float> _noonCicadaVolume;
        private ConfigEntry<float> _nightNatureVolume;
        private ConfigEntry<float> _natureFadeSeconds;
        private ConfigEntry<float> _rainNatureMultiplier;

        // -------- in-game config menu --------
        private ConfigEntry<bool> _configMenuEnabled;
        private ConfigEntry<string> _configMenuKey;
        private ConfigEntry<string> _configMenuLanguage;
        private ConfigEntry<int> _configRevision;

        // -------- state --------
        private GameObject _volumeObject;
        private Volume _volume;
        private VolumeProfile _profile;
        private Material _skyMaterial;
        private Material _originalSkybox;
        private bool _skyApplied;
        private bool _skyRuntimeEnabled;
        private GameObject _moonObject;
        private Material _moonMaterial;
        private Texture2D _moonTexture;
        private Mesh _moonMesh;
        private float _moonPhase;
        private float _moonIllumination = 1f;
        private float _lastMoonTexturePhase = -10f;
        private float _nextMoonPhaseUpdate;
        private bool _ambientCaptured;
        private AmbientMode _origAmbientMode;
        private Color _origAmbientLight;
        private Color _origAmbientSky;
        private Color _origAmbientEquator;
        private Color _origAmbientGround;
        private float _origAmbientIntensity;
        private Coroutine _applyRoutine;
        private Light _sunLight;
        private bool _sunCaptured;
        private Quaternion _origSunRotation;
        private Color _origSunColor;
        private float _origSunIntensity;
        private float _origSunShadowStrength;
        private int _postForcedCount;
        private int _skyboxForcedCount;
        private int _cameraSkyboxForcedCount;
        private float _nextPostCheck;
        private float _nextDayCycleUpdate;
        private Camera _worldCamera;
        private static readonly FieldInfo ProfilePhotoCameraField = typeof(PhotoController).GetField(
            "_photoCamera", BindingFlags.Instance | BindingFlags.NonPublic);
        private Camera _profilePhotoCamera;
        private bool _profilePhotoLookupAttempted;
        private Camera _neutralPhotoRenderCamera;
        private bool _neutralPhotoVolumeEnabled;
        private Quaternion _neutralPhotoSunRotation;
        private Color _neutralPhotoSunColor;
        private float _neutralPhotoSunIntensity;
        private float _neutralPhotoSunShadowStrength;
        private Color _neutralPhotoAmbientLight;
        private Color _neutralPhotoAmbientSky;
        private Color _neutralPhotoAmbientEquator;
        private Color _neutralPhotoAmbientGround;
        private float _neutralPhotoAmbientIntensity;
        private float _neutralPhotoReflectionIntensity;
        private sealed class CameraSkyboxState
        {
            public Skybox Skybox;
            public Material OriginalMaterial;
        }
        private readonly Dictionary<int, CameraSkyboxState> _cameraSkyboxStates =
            new Dictionary<int, CameraSkyboxState>();
        private sealed class ShadowRendererState
        {
            public Renderer Renderer;
            public ShadowCastingMode OriginalMode;
            public bool OriginalReceiveShadows;
        }

        private readonly Dictionary<int, ShadowRendererState> _shadowRendererStates =
            new Dictionary<int, ShadowRendererState>();
        private LightShadows _origSunShadows;
        private float _nextGiUpdate;
        private float _nextShadowRescan;
        private float _nextPlayerShadowRefresh;
        private float _nextSceneFeatureLampRescan;
        private bool _playerDiagnosticsWritten;
        private ColorAdjustments _colorAdjustments;
        private bool _colorGradingModeCaptured;
        private ColorGradingMode _originalColorGradingMode;
        private readonly List<Light> _lampLights = new List<Light>();
        private readonly List<GameObject> _lampHolders = new List<GameObject>();
        private readonly List<Transform> _lampHalos = new List<Transform>();
        private readonly Dictionary<Light, float> _deckStringLightHues = new Dictionary<Light, float>();
        private readonly Dictionary<Renderer, float> _deckStringHaloHues = new Dictionary<Renderer, float>();
        private sealed class SpecialLocalLight
        {
            public float DayIntensity;
            public float NightIntensity;
            public bool ReserveBudget;
        }
        private readonly Dictionary<Light, SpecialLocalLight> _specialLocalLights =
            new Dictionary<Light, SpecialLocalLight>();
        private sealed class CampfireEmissionState
        {
            public GameObject Overlay;
            public Material[] Materials;
            public bool[] Visible;
        }
        private readonly List<CampfireEmissionState> _campfireEmissionStates =
            new List<CampfireEmissionState>();
        private sealed class DeviceScreenState
        {
            public Renderer Source;
            public Renderer OverlayRenderer;
            public GameObject Overlay;
            public Material[] Materials;
            public Light Light;
            public bool HasScreenAnchor;
            public Vector3 ScreenLocalCenter;
            public Vector3 ScreenLocalNormal;
            // MD_ConsoleDeck is the hand-held game console, not the similarly named
            // prop. Its M_DeckScreen material is an independent display surface.
            public bool IsGameConsole;
            public Color DisplayColor;
            public Color TargetDisplayColor;
            public float NextDisplayChangeAt;
        }
        private readonly List<DeviceScreenState> _deviceScreenStates =
            new List<DeviceScreenState>();
        private static readonly Color[] DeckStringPastelColors = new Color[]
        {
            new Color(1.00f, 0.68f, 0.66f, 1f), new Color(0.98f, 0.82f, 0.56f, 1f),
            new Color(0.71f, 0.88f, 0.64f, 1f), new Color(0.57f, 0.82f, 0.88f, 1f),
            new Color(0.65f, 0.70f, 0.96f, 1f), new Color(0.88f, 0.66f, 0.92f, 1f)
        };
        // Halloween palette for the hanging string lights. Read in autumn instead of the pastel
        // rainbow above. Light colours are authored in sRGB; Unity converts them itself.
        // Pumpkin orange and witch purple, and nothing in between. Blending between the two
        // would spend most of the cycle on the muddy browns and mauves that lie on the line
        // between them, so the string steps from one to the other instead of fading - which is
        // also what a real Halloween light string does.
        private static readonly Color[] DeckStringHalloweenColors = new Color[]
        {
            new Color(1.00f, 0.42f, 0.05f, 1f), new Color(0.56f, 0.17f, 0.88f, 1f)
        };
        private Material _haloMaterial;
        private Mesh _haloMesh;
        private float _nextLampUpdate;
        private float _lastDayWeight = 1f;
        private float _lampWeight;

        // ---------------------------------------------------------------
        // Season theme
        // ---------------------------------------------------------------
        // Autumn is expressed as a hue plus saturation/value scales rather than as literal
        // colours. Two reasons. It keeps every material's authored brightness relationship
        // intact, which is what makes the island still read as one painting; and it is immune
        // to the colour-space trap this project sits in - the project renders in Linear, so a
        // literal written through SetColor is taken as already-linear and comes out pale, while
        // a hue rotation of whatever the material already holds lands correctly either way.
        private sealed class SeasonRecolor
        {
            public readonly string Material;
            public readonly string[] Properties;
            public readonly float Hue;
            public readonly float SaturationScale;
            public readonly float ValueScale;
            public readonly bool Ground;
            // Planting is given an absolute colour, because rotating the authored olive greens
            // by hue lands on brown however the scales are set, and brown is not autumn. Ground
            // surfaces keep the hue shift: they cover most of the screen, and preserving their
            // authored light-and-shade relationships is what stops the island reading as flat.
            public readonly bool Absolute;
            public readonly Color Target;

            public SeasonRecolor(string material, string properties, float hue,
                float saturationScale, float valueScale, bool ground)
            {
                Material = material;
                Properties = properties.Split(',');
                Hue = hue;
                SaturationScale = saturationScale;
                ValueScale = valueScale;
                Ground = ground;
                Absolute = false;
                Target = Color.white;
            }

            public SeasonRecolor(string material, string properties, Color target, bool ground)
            {
                Material = material;
                Properties = properties.Split(',');
                Hue = 0f;
                SaturationScale = 1f;
                ValueScale = 1f;
                Ground = ground;
                Absolute = true;
                Target = target;
            }
        }

        // Every entry was read out of the shipped scene rather than guessed. The round canopies
        // (MD_TreeBase_01/02) are drawn by BushInstancing through one shared ToonWaveInstance
        // material, so their body colour lives here while their per-tree cap colour lives in the
        // instancing colour arrays below - changing the WaveTree01..04 materials those canopies
        // still reference does nothing at all, because their renderers are disabled.
        private static readonly SeasonRecolor[] AutumnRecolors = new SeasonRecolor[]
        {
            // The band under every round canopy. Present here so its authored colour is captured
            // and restored; the value actually written is set in ApplyAutumnCanopies, which runs
            // after this and is where the shade-brightness slider is read.
            new SeasonRecolor("ToonWaveInstance", "_Color_3", new Color(0.361f, 0.180f, 0.078f, 1f), false),
            // MD_TreeBase_03 - the conical evergreens - is deliberately absent. Its mesh is
            // 2:1 tall against 0.77:1 for the round canopies, they read as pines, and pines do
            // not turn. Leaving M_Tree_02 alone also keeps them out of the leaf-fall pass.
            // Bushes: potted circle bushes, the hedge lines, and the potted plant leaves that
            // share their material.
            new SeasonRecolor("Green 10", "_BaseColor,_Color", new Color(0.851f, 0.561f, 0.235f, 1f), false),
            new SeasonRecolor("Green 10 2", "_BaseColor,_Color", new Color(0.851f, 0.561f, 0.235f, 1f), false),
            // Grass tufts scattered over the island.
            new SeasonRecolor("M_Grass_01", "_BaseColor,_Color", new Color(0.788f, 0.541f, 0.220f, 1f), false),
            // Flower beds: the green disc under each cluster, then the petals.
            new SeasonRecolor("Flower 3 Green Circle", "_BaseColor,_Color", new Color(0.784f, 0.486f, 0.200f, 1f), false),
            new SeasonRecolor("Flower 2", "_BaseColor,_Color", 0.095f, 1.40f, 1.00f, false),
            new SeasonRecolor("Flower 3 White Circle 1", "_BaseColor,_Color", 0.095f, 1.40f, 1.00f, false),
            new SeasonRecolor("Flower 4", "_BaseColor,_Color", 0.085f, 1.40f, 1.00f, false),
            // Lawn.
            new SeasonRecolor("M_GroundGrass 30", "_BaseColor,_Color", 0.098f, 1.40f, 0.96f, true),
            new SeasonRecolor("M_GroundGrass 10", "_BaseColor,_Color", 0.098f, 1.40f, 0.96f, true),
            // Nature paths crossing it.
            new SeasonRecolor("M_PathNature 1", "_BaseColor,_Color", 0.085f, 1.30f, 1.00f, true),
            new SeasonRecolor("M_PathNature Mid", "_BaseColor,_Color", 0.088f, 1.25f, 1.00f, true),
            new SeasonRecolor("M_PathNature Long", "_BaseColor,_Color", 0.088f, 1.25f, 1.00f, true),
            new SeasonRecolor("M_PathNature Short", "_BaseColor,_Color", 0.088f, 1.25f, 1.00f, true)
        };

        // Four tree colours, written as the sRGB the artist would have picked. The canopies get
        // absolute colours rather than a hue rotation of what was there: rotating the authored
        // olive greens landed on browns, which is not what autumn looks like, and the whole
        // point here is that these four are picked to sit together.
        private static readonly Color[] AutumnCanopyPalette = new Color[]
        {
            new Color(0.949f, 0.776f, 0.235f, 1f),   // 0 gold      #F2C63C   ginkgo
            new Color(0.929f, 0.655f, 0.200f, 1f),   // 1 amber     #EDA733   ginkgo
            new Color(0.910f, 0.486f, 0.180f, 1f),   // 2 pumpkin   #E87C2E   maple
            new Color(0.847f, 0.286f, 0.173f, 1f)    // 3 scarlet   #D8492C   maple
        };
        // Which colour a tree draws, by hash of where it stands. Gold appears most often and
        // scarlet least, so the island reads gold with orange and red through it.
        private static readonly int[] AutumnCanopyWeights = new int[] { 0, 0, 0, 1, 1, 2, 2, 3 };
        // The round canopies are drawn by Shader Graphs/ToonWaveInstanceShader, which has no
        // lighting of its own - no shade colour, no light binding, nothing the sun reaches. The
        // conical evergreens next to them are on the game's UTS "Toon" shader and do get a real
        // terminator, which is exactly why the two read so differently. Everything below is the
        // shading these canopies can have: a vertical ramp inside each disc, a dark band at the
        // bottom of it, and a per-disc brightness that follows how high the disc sits in its own
        // tree. Painted by hand, but it is the only channel this shader offers.
        //
        // All three shading amounts are driven by the Canopy shade brightness slider, between
        // these ends: at its lowest the underside is deep, at its highest it is barely there.
        private const float CanopyLowerScaleDark = 0.50f;
        private const float CanopyLowerScaleLight = 0.98f;
        // Per-disc brightness from the lowest disc of a tree to the highest. The discs of one
        // tree overlap, so the lower ones are the ones the upper ones would be shading.
        private const float CanopyShadeLowDark = 0.58f;
        private const float CanopyShadeLowLight = 0.98f;
        private const float CanopyShadeHigh = 1.08f;
        // The band under every canopy, from deepest to lightest. One shared colour for the whole
        // island, so it is a shadow brown rather than any tree's own colour: a dark underside
        // reads as shade on every tree, where a tree colour would read as the wrong tree.
        private static readonly Color CanopyBandDark = new Color(0.30f, 0.14f, 0.06f, 1f);
        private static readonly Color CanopyBandLight = new Color(0.80f, 0.52f, 0.24f, 1f);
        // A little variation between neighbouring discs, so a crown does not read as one
        // moulded lump of colour.
        private const float CanopyShadeJitter = 0.07f;
        // Discs closer together than this on the horizontal plane are one tree. Measured: at 9 m
        // the island's 373 canopy discs group into roughly 60 trees of about 5 discs each, which
        // matches what a tree looks like in game.
        private const float CanopyClusterRadius = 9f;
        // Sits on top of the grade as a multiplier, so it warms without lifting the exposure.
        // These are linear values, which is why the blue looks so much lower than it reads: a
        // white surface through this filter comes out #FFF4D0, a warm afternoon. Written the way
        // an sRGB literal "looks right" (0.98 blue, say) the filter would be invisible.
        private static readonly Color AutumnWarmFilter = new Color(1.02f, 0.90f, 0.62f, 1f);

        private sealed class CanopyCluster
        {
            public Vector3 Centre;
            public float Radius;
            public float BottomY;
            public float TopY;
            public int Palette;
            public int Members;
        }

        // Saturation ceiling for every autumn colour. Without it the scales push these already
        // saturated greens to a flat 1.0, which drives the blue channel to zero and produces the
        // heavy, muddy orange of a cheap filter - the opposite of this game's pastel painting.
        // Leaving a little blue in keeps the autumn colours in the same family as the art.
        private const float AutumnMaxSaturation = 0.86f;

        private sealed class SeasonMaterialState
        {
            public Material Material;
            public int[] PropertyIds;
            public string[] PropertyNames;
            public Color[] Originals;
        }

        private sealed class SeasonCanopyState
        {
            public BushInstancing Batch;
            public Vector4[] OriginalColor1;
            public Vector4[] OriginalColor2;
            public float[] OriginalThreshold1;
            // Which cluster each instance belongs to, so every disc of one tree takes the same
            // colour. -1 until the clustering pass has run.
            public int[] Cluster;
        }

        private sealed class LeafEmitterState
        {
            public ParticleSystem System;
            public ParticleSystemRenderer Renderer;
            public Material OriginalMaterial;
            public float OriginalRate;
            public bool OriginalSheetEnabled;
            public ParticleSystemAnimationMode OriginalSheetMode;
            public int OriginalTilesX;
            public int OriginalTilesY;
            public ParticleSystemAnimationType OriginalAnimation;
            public ParticleSystem.MinMaxCurve OriginalFrameOverTime;
            public ParticleSystem.MinMaxCurve OriginalStartFrame;
            public int OriginalCycleCount;
            public float OriginalShapeRadius;
            public bool ShapeAdjusted;
            // Copies this plugin made to spread leaf fall over the island; simply destroyed again.
            public bool Cloned;
        }

        private readonly List<SeasonMaterialState> _seasonMaterials = new List<SeasonMaterialState>();
        private readonly HashSet<Material> _seasonSeen = new HashSet<Material>();
        private readonly List<SeasonCanopyState> _seasonCanopies = new List<SeasonCanopyState>();
        private readonly List<CanopyCluster> _canopyClusters = new List<CanopyCluster>();

        // Shadow-only stand-ins for the round canopies. See ApplyCanopyShadows.
        private sealed class CanopyShadowState
        {
            public BushInstancing Batch;
            public ShadowCastingMode OriginalMode;
            public bool Captured;
        }
        private readonly List<CanopyShadowState> _canopyShadows = new List<CanopyShadowState>();
        private readonly List<GameObject> _canopyShadowObjects = new List<GameObject>();
        private readonly List<Mesh> _canopyShadowMeshes = new List<Mesh>();
        private Material _canopyShadowMaterial;
        private bool[][] _canopyDappleMasks;
        private float _canopyDappleMaskGaps = -1f;
        private const int CanopyShadowGrid = 16;
        private const int CanopyDapplePatterns = 8;
        private readonly List<LeafEmitterState> _leafEmitters = new List<LeafEmitterState>();
        private Material _autumnLeafMaterial;
        private Texture2D _autumnLeafAtlas;

        // Leaves that have already landed. Kept as plain data and drawn as one mesh of
        // world-space quads: a GameObject per leaf would be hundreds of transforms and draw
        // calls, and a particle system cannot park a particle on uneven ground.
        private sealed class GroundLeaf
        {
            public Vector3 Position;
            public Vector3 Right;
            public Vector3 Forward;
            public int Frame;
            public float BornAt;
        }
        private readonly List<GroundLeaf> _groundLeaves = new List<GroundLeaf>();
        private readonly List<Vector3> _groundLeafVertices = new List<Vector3>();
        private readonly List<Vector2> _groundLeafUvs = new List<Vector2>();
        private readonly List<int> _groundLeafTriangles = new List<int>();
        private GameObject _groundLeafObject;
        private Mesh _groundLeafMesh;
        private MeshRenderer _groundLeafRenderer;
        private Material _groundLeafMaterial;
        private float _nextGroundLeafSpawn;
        private float _nextGroundLeafRebuild;
        private bool _groundLeafDirty;
        private int _groundLeafRayMask;
        private bool _groundLeafRayMaskReady;
        // Leaves only settle near the viewer, so the budget is spent where it can be seen. Walk
        // somewhere else and they build up there too, while the ones behind you stay put.
        private const float GroundLeafViewerRange = 70f;
        private const float GroundLeafFadeSeconds = 3f;
        // -1 until a season has been applied once, then 0 summer / 1 autumn.
        private int _appliedSeason = -1;
        private float _nextSeasonRescan;
        private int _seasonRescanAttempts;

        private sealed class WaterMaterial
        {
            public Material Material;
            public int[] PropertyIds;
            public string[] PropertyNames;
            public Color[] Originals;
            public Color[] DrivenColors;
            public bool HasReflectionStrength;
            public float OriginalReflectionStrength;
            public bool IsWaterfall;
            // Open water mirrors the sky; a vertical waterfall sheet barely does.
            public bool IsOpenWater;
            // Per-color-property weight for the sky mirror, resolved once at scan time.
            public float[] SkyMirrorWeights;
        }

        private readonly List<WaterMaterial> _waterMaterials = new List<WaterMaterial>();
        private readonly HashSet<Material> _waterSeen = new HashSet<Material>();
        private sealed class WaterParticleState
        {
            public ParticleSystem System;
            public ParticleSystem.MinMaxGradient OriginalStartColor;
        }
        private readonly List<WaterParticleState> _waterParticles = new List<WaterParticleState>();
        private readonly HashSet<int> _waterParticleIds = new HashSet<int>();
        // Last evaluated sky color the sea mirrors, shared with the shadow-receiver overlay.
        private Color _seaSkyMirrorColor = Color.white;
        private Color _seaDrivenBodyColor = new Color(0.2f, 0.4f, 0.5f, 1f);
        private float _lastTwilightGlow;
        private float _lastSunVisibility;
        // Sea plane of the largest horizontal water surface, used by the sun-glitter path.
        private float _seaSurfaceY;
        private Bounds _seaBounds;
        private bool _seaSurfaceValid;
        private float _nextWaterEffectScan;
        private float _origReflectionIntensity = 1f;
        private Material _waterShadowOverlayMaterial;
        private readonly List<GameObject> _waterShadowOverlayObjects = new List<GameObject>();
        private readonly HashSet<int> _waterShadowOverlaySourceIds = new HashSet<int>();

        // Decor materials reuse the WaterMaterial capture/restore shape.
        private readonly List<WaterMaterial> _decorMaterials = new List<WaterMaterial>();
        private readonly HashSet<Material> _decorSeen = new HashSet<Material>();
        private float _nextDecorRescan;

        // Per-renderer facial overlay instances. Skin, hair, glasses and clothes are never
        // touched, and no shared material is ever modified.
        // One overlay slot: what the customization system put there, what we render in its
        // place, and the tint we drive. Cheeks and eyebrows keep the game's own shader, so
        // FaceInstanceInfo remembers which colour property is theirs.
        private sealed class FaceInstanceInfo
        {
            public Material Instance;
            // The tint the overlay would show at full daylight - the value the game itself
            // chose. Brightness is applied on top of this, never in place of it.
            public Color BaseColor;
            public int TintProperty;
            public bool HasSourceTint;
            public int SourceTintProperty;
            public Color LastSourceTint;
            public bool HasSourceTexture;
            public int SourceTextureProperty;
            public int InstanceTextureProperty;
            public Texture LastSourceTexture;
        }

        private sealed class NativeFeatureMaterial
        {
            public int Slot;
            public Material Original;
            public Material Instance;
            public Color BaseColor;
            // Which colour property of the instance carries the tint. Cloned cheek and
            // eyebrow materials keep the game's own graph, whose colour may not be _BaseColor.
            public int TintProperty;
            public FaceInstanceInfo Info;
            // Cheeks and face paint are tinted in place. The customization system rewrites
            // that material's colour and mask whenever the player picks a different one, so
            // copying it froze the first blush shape onto the face.
            public bool InPlace;
            public bool HasWritten;
            public Color LastWritten;
            // Cheeks and face paint take their colour from the decal texture, not from any
            // colour property, so their brightness has to be applied to the texture itself.
            public string TextureProperty;
            public Texture SourceTexture;
            public Texture2D DimTexture;
            public Color32[] SourcePixels;
            public Color32[] Scratch;
            public int Bucket;
        }
        private const int FaceBrightnessSteps = 24;
        private const int FaceDecalMaxSize = 256;
        private sealed class NativeFeatureRenderer
        {
            public Renderer Renderer;
            public Material[] Assigned;
            public readonly List<NativeFeatureMaterial> Materials = new List<NativeFeatureMaterial>();
            // Blinking swaps the eye slot between open and closed materials several times a
            // second. One cached instance per source material stops that from allocating on
            // every blink and lets the new slot inherit the current brightness immediately,
            // instead of flashing at full colour until the next refresh.
            public readonly Dictionary<int, FaceInstanceInfo> Instances =
                new Dictionary<int, FaceInstanceInfo>();
            // The same records looked up by the instance's own id, for the pass that finds one
            // of our materials already sitting in a slot.
            public readonly Dictionary<int, FaceInstanceInfo> ByInstance =
                new Dictionary<int, FaceInstanceInfo>();
            public readonly Dictionary<int, NativeFeatureMaterial> InPlaceEntries =
                new Dictionary<int, NativeFeatureMaterial>();
            public readonly List<Material> Owned = new List<Material>();
            public float Brightness = -1f;
            // Face brightness is eased toward its target rather than snapped. A lamp switching
            // on or off, or a head moving in an idle animation, otherwise steps the value and
            // the overlays visibly jump with it.
            public float SmoothedBrightness = -1f;
            // Loading and customization screens rebuild their preview materials continuously.
            // Swapping in lockstep with that made the preview strobe, so a renderer that keeps
            // overwriting our slots is left alone instead of being fought over.
            public int ConsecutiveReassigns;
            public bool Abandoned;
        }
        private readonly Dictionary<int, NativeFeatureRenderer> _nativeFeatureRenderers =
            new Dictionary<int, NativeFeatureRenderer>();
        private readonly List<Renderer> _faceRenderers = new List<Renderer>();
        private readonly List<Light> _faceLightSources = new List<Light>();
        private readonly List<int> _nativeFeatureCleanupIds = new List<int>();
        private Shader _faceTintShader;
        private float _nextFaceRendererScan;
        private float _nextFaceBrightnessUpdate;
        private float _nextFaceAssignCheck;
        private float _nextFaceStatusLog;
        private int _nativeFeatureLogged;
        private const string FaceInstanceSuffix = " (DayNight Facial Light)";
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int CanopyColor1PropertyId = Shader.PropertyToID("_Color_1");
        private static readonly int CanopyColor2PropertyId = Shader.PropertyToID("_Color_2");
        private static readonly int CanopyThresholdPropertyId = Shader.PropertyToID("_Threshold_1");
        private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
        // Checked in this order. _BaseColor is what the game's own SetFacialColor writes.
        private static readonly string[] FaceTintPropertyNames =
            new string[] { "_BaseColor", "_Color", "_Tint", "_TintColor" };
        private static readonly string[] FaceTexturePropertyNames =
            new string[] { "_MainTex", "_Texture2D", "_BaseMap" };

        // -------- twilight variant state --------
        private int _lastTwilightId = int.MinValue;
        private string _twilightVariantName = "golden dusk";
        private Color _twilightGlowColor = new Color(1f, 0.42f, 0.10f, 1f);
        private Color _twilightSkyTint = new Color(0.82f, 0.44f, 0.18f, 1f);
        private Color _twilightSunColor = new Color(1f, 0.70f, 0.38f, 1f);
        private GameObject _sunsetGlowObject;
        private Material _sunsetGlowMaterial;
        private Texture2D _sunsetGlowTexture;
        private GameObject _duskShadeObject;
        private Material _duskShadeMaterial;
        private GameObject _sunGlitterObject;
        private Material _sunGlitterMaterial;
        private Mesh _sunGlitterMesh;
        private Vector3[] _glitterVertices;
        private Color[] _glitterColors;
        private GameObject _sunObject;
        private Material _sunMaterial;
        private Texture2D _sunTexture;
        // -1 until the procedural skybox disc state has been applied once.
        private int _sunDiskState = -1;

        // -------- celestial visual state --------
        // The light keeps a 6-degree elevation floor for stable shading, while
        // celestial visibility and the moon position follow the real elevation.
        private bool _celestialValid;
        private float _lastSignedElevation;
        private float _lastSunAzimuth;
        private Camera _shadowAaCamera;
        private AntialiasingMode _originalAntialiasing;
        private AntialiasingQuality _originalAntialiasingQuality;
        private bool _shadowAaCaptured;
        private GameObject _starObject;
        private Material _starMaterial;
        private Mesh _starMesh;
        private Texture2D _starTexture;
        private readonly List<Color> _starBaseColors = new List<Color>();
        private readonly List<float> _starTwinklePhases = new List<float>();
        private readonly List<float> _starTwinkleSpeeds = new List<float>();
        private readonly List<float> _starTwinkleAmounts = new List<float>();
        private readonly List<Color> _starAnimatedColors = new List<Color>();
        private float _nextStarTwinkleUpdate;
        private GameObject _meteorObject;
        private Material _meteorMaterial;
        private bool _meteorActive;
        private Vector3 _meteorDir;
        private Vector3 _meteorVelocity;
        private float _meteorStartTime;
        private float _nextMeteorRoll;
        private readonly System.Random _visualRandom = new System.Random();
        private sealed class HiddenCloudRendererState
        {
            public Renderer Renderer;
            public bool OriginalForceRenderingOff;
            public bool OriginalEnabled;
        }
        private readonly Dictionary<int, HiddenCloudRendererState> _hiddenVanillaClouds =
            new Dictionary<int, HiddenCloudRendererState>();
        private sealed class HiddenWorldCloudRootState
        {
            public GameObject Root;
            public bool OriginalActive;
        }
        private readonly Dictionary<int, HiddenWorldCloudRootState> _hiddenWorldCloudRoots =
            new Dictionary<int, HiddenWorldCloudRootState>();
        private sealed class CloudInstancingState
        {
            public CloudInstancing Instancing;
            public bool OriginalEnabled;
        }
        private readonly Dictionary<int, CloudInstancingState> _hiddenCloudInstancers =
            new Dictionary<int, CloudInstancingState>();
        private float _nextVanillaCloudScan;
        private readonly HashSet<int> _cloudSuspectsLogged = new HashSet<int>();
        private readonly HashSet<int> _skyRenderersLogged = new HashSet<int>();

        // -------- cloud state --------
        private const float CloudFieldRadius = 210f;
        private GameObject _cloudRoot;
        private Material _cloudMaterial;
        private readonly List<Transform> _cloudTransforms = new List<Transform>();
        private readonly List<Vector3> _cloudBaseScales = new List<Vector3>();
        private readonly List<Mesh> _cloudMeshList = new List<Mesh>();

        // -------- weather state --------
        private bool _rainTarget;
        private float _rainBlend;
        private float _nextWeatherChange;
        private float _nextWorldProbe;
        private bool _worldSceneReady;
        private KeyCode _weatherToggleKey = KeyCode.F10;
        private readonly System.Random _weatherRandom = new System.Random();
        private GameObject _stormCloudRoot;
        private Material _stormCloudMaterial;
        private readonly List<Transform> _stormCloudTransforms = new List<Transform>();
        private readonly List<Vector3> _stormCloudBaseScales = new List<Vector3>();
        private readonly List<Mesh> _stormCloudMeshes = new List<Mesh>();
        private GameObject _rainObject;
        private ParticleSystem _rainParticles;
        private Material _rainMaterial;
        private Texture2D _rainTexture;
        private bool _rainUsesNativeMaterial;
        private GameObject _splashObject;
        private ParticleSystem _splashParticles;
        private Material _splashMaterial;
        private Texture2D _splashTexture;
        private float _splashAccumulator;
        private bool _splashDiagnosticWritten;
        private GameObject _weatherAudioObject;
        private AudioSource _rainAudioSource;
        private AudioSource _rainDetailAudioSource;
        private AudioSource _thunderAudioSource;
        private AudioClip _rainAudioClip;
        private AudioClip _rainDetailAudioClip;
        private AudioClip _thunderAudioClip;
        private string _weatherAudioPairSignature = "";
        private float _nextWeatherAudioProbe;
        private bool _weatherAudioWarningWritten;
        private float _nextThunderRoll;
        private float _lightningFlash;
        private bool _weatherFogCaptured;
        private bool _originalFogEnabled;
        private FogMode _originalFogMode;
        private Color _originalFogColor;
        private float _originalFogDensity;
        private float _originalFogStart;
        private float _originalFogEnd;

        // -------- licensed nature ambience state --------
        private GameObject _natureAudioObject;
        private AudioSource _cicadaAudioSource;
        private AudioSource _nightNatureAudioSource;
        private AudioClip _cicadaAudioClip;
        private AudioClip _nightNatureAudioClip;
        private Coroutine _natureAudioLoadRoutine;
        private bool _natureAudioLoadAttempted;
        private int _lastNatureAudioPhase = -1;

        // -------- in-game config menu state --------
        private KeyCode _configMenuKeyCode = KeyCode.F11;
        private bool _configMenuVisible;
        private bool _hotConfigDirty;
        private float _hotConfigApplyAt;
        private Rect _configMenuRect = new Rect(36f, 36f, 680f, 700f);
        private Vector2 _configMenuScroll;
        private int _configMenuTab;
        private CursorLockMode _cursorLockBeforeMenu;
        private bool _cursorVisibleBeforeMenu;
        private GUIStyle _menuTitleStyle;
        private GUIStyle _menuSectionStyle;
        private GUIStyle _menuNoteStyle;
        private GUIStyle _menuValueStyle;
        private Font _configMenuFont;
        private bool _ownsConfigMenuFont;
        private readonly HashSet<int> _lampRootIds = new HashSet<int>();
        private readonly HashSet<int> _sceneFeatureLightSourceIds = new HashSet<int>();

        private sealed class ToonMaterial
        {
            public Material Material;
            public bool HadSystemShadows;
            public float OrigSystemShadows;
            public bool HadSystemShadowLevel;
            public float OrigSystemShadowLevel;
            public bool HadBaseColorStep;
            public float OrigBaseColorStep;
            public bool HadUseBaseAs1st;
            public float OrigUseBaseAs1st;
            public bool HadUse1stAs2nd;
            public float OrigUse1stAs2nd;
            public bool PlayerShadowAdjusted;
            public bool SurfaceShadowAdjusted;
            public bool Has1st;
            public Color Orig1st;
            public bool Has2nd;
            public Color Orig2nd;
        }

        private readonly List<ToonMaterial> _toonMaterials = new List<ToonMaterial>();
        private readonly List<ToonMaterial> _surfaceToonEntries = new List<ToonMaterial>();
        private readonly HashSet<Material> _toonSeen = new HashSet<Material>();
        private readonly HashSet<Material> _surfaceToonMaterials = new HashSet<Material>();
        private readonly HashSet<Material> _basketCourtToonMaterials = new HashSet<Material>();

        private void Awake()
        {
            _gradingEnabled = Config.Bind("Grading", "Enabled", true,
                "Master switch for the Day and Night colour grade (white balance, exposure, contrast, shadow tone). " +
                "F6 toggles it live for A/B comparison.");
            _temperature = Config.Bind("Grading", "Temperature", 0f,
                "White balance temperature, -100 (cold) to 100 (warm). Negative values counteract the game's warm cast.");
            _tint = Config.Bind("Grading", "Tint", 10f,
                "White balance green-magenta tint, -100 to 100. Positive counteracts the green cast of the " +
                "grass-dominated daylight scenes.");
            _postExposure = Config.Bind("Grading", "Post exposure", 0f,
                "Exposure adjustment in EV. Zero preserves the clear authored highlights and sun disc.");
            _contrast = Config.Bind("Grading", "Contrast", 14f,
                "Contrast, -100 to 100. Positive values give shadows more presence.");
            _saturation = Config.Bind("Grading", "Saturation", 3f,
                "Saturation, -100 to 100.");
            _dayColorFilter = Config.Bind("Grading", "Day color filter", new Color(1f, 0.98f, 1f, 1f),
                "Final daylight RGB multiplier. Kept very close to white so the grade removes only the green " +
                "bias without laying a gray/magenta veil over the whole image.");
            _shadowCooling = Config.Bind("Grading", "Shadow cooling", 0.08f,
                "0-1. Shifts shadow regions toward a cooler, slightly deeper tone so they read as natural " +
                "shade instead of warm haze.");
            _tonemapping = Config.Bind("Grading", "Tonemapping", "None",
                "None, Neutral or ACES. None preserves crisp pastel highlights and keeps the sun from being " +
                "compressed into a gray disc; Neutral/ACES intentionally roll highlights off.");
            _disableFilmGrain = Config.Bind("Grading", "Disable film grain", true,
                "Removes the game's film grain overlay, which contributes to the washed-out look.");
            _disableDepthOfField = Config.Bind("Grading", "Disable depth of field", true,
                "Removes the game's depth-of-field blur. Sharper distance view and slightly cheaper on the GPU.");
            _disableMotionBlur = Config.Bind("Grading", "Disable motion blur", true,
                "Removes the game's motion blur, which smears the image at sub-60 frame rates.");
            _disableBloom = Config.Bind("Grading", "Disable bloom", true,
                "The game's own bloom (never visible before, since it shipped with post-processing off) washes " +
                "the bright pastel scene into haze, especially at noon.");
            _disableVignette = Config.Bind("Grading", "Disable vignette", true,
                "Removes the game's corner darkening.");
            _neutralizeSplitToning = Config.Bind("Grading", "Neutralize split toning", true,
                "Overrides the game's own shadow/highlight color toning with neutral gray. Try this if the " +
                "image still feels tinted after the white balance change.");

            _replaceSkybox = Config.Bind("Sky", "Replace skybox", true,
                "Replaces the flat sky with a procedural atmosphere: deep blue zenith fading to a pale horizon, " +
                "plus a visible sun disc that follows the scene's directional light. F5 toggles it live.");
            _atmosphereThickness = Config.Bind("Sky", "Atmosphere thickness", 0.7f,
                "0.5-2. Higher values deepen the blue but also whiten the horizon haze.");
            _skyExposure = Config.Bind("Sky", "Exposure", 1.2f,
                "Skybox brightness.");
            _sunSize = Config.Bind("Sky", "Sun size", 0.028f,
                "Apparent size of the daytime sun. The default keeps it compact and consistent through dawn and dusk.");
            _skyTint = Config.Bind("Sky", "Sky tint", new Color(0.5f, 0.5f, 0.5f, 1f),
                "Tint applied to the atmosphere. Neutral gray gives a natural blue sky.");
            _groundColor = Config.Bind("Sky", "Ground color", new Color(0.6f, 0.8f, 0.85f, 1f),
                "Color of the skybox below the horizon. Keep it close to the sea color so the strip " +
                "between the ocean edge and the sky blends in instead of showing a pale band.");
            _moonEnabled = Config.Bind("Sky", "Enable moon phases", true,
                "Replaces the night-time miniature sun disc with a dedicated procedural moon whose lit face " +
                "changes through a lunar cycle.");
            _lunarCycleDays = Config.Bind("Sky", "Lunar cycle game days", 8f,
                "Number of in-game day/night cycles per complete lunar cycle. Eight gives the familiar eight " +
                "recognizable moon phases; with a 30-minute day this is a four-hour lunar cycle.");
            _newMoonBrightness = Config.Bind("Sky", "New moon light multiplier", 0.6f,
                "0-1. Night directional-light brightness at new moon relative to full moon. Full moon always " +
                "uses the existing Night brightness value as its baseline.");
            _moonSize = Config.Bind("Sky", "Moon angular size", 0.075f,
                "Apparent size of the moon in the night sky.");
            _moonColor = Config.Bind("Sky", "Moon color", new Color(0.82f, 0.9f, 1f, 1f),
                "Tint of the illuminated lunar surface and its subtle halo.");
            _sunsetGlowEnabled = Config.Bind("Sky", "Sunset glow", true,
                "Directional dawn/dusk color: a soft glow band hugs the horizon around the sun, so the " +
                "sky blushes on the sun side while the opposite side stays dim, like a real sunset.");
            _sunsetGlowStrength = Config.Bind("Sky", "Sunset glow strength", 1f,
                "0-2. Opacity multiplier for the horizon glow band.");
            _freezeTime = Config.Bind("Sky", "Hold time of day", false,
                "Keeps it at one time of day forever, instead of the clock moving on. Everything else "
                + "still works normally. Turn it off and the day starts moving again.");
            _frozenHour = Config.Bind("Sky", "Time to hold", 18f,
                "Which time to stay at. 12 is midday, 0 and 24 are midnight, 18 is about sunset and "
                + "6 is about sunrise.");
            _twilightVariantOverride = Config.Bind("Sky", "Always use this sunset colour", 0,
                "Normally the colour of each sunset is picked at random. Set this to always get the "
                + "one you want. 0 random, 1 golden, 2 yellow morning, 3 pink, 4 deep red.");
            _pinkDuskChance = Config.Bind("Sky", "Pink twilight chance", 0.05f,
                "0-1. Probability that a twilight rolls a pink sky instead of its usual color. Rolled " +
                "once per twilight, deterministically from the real clock, and colors the glow band, " +
                "sky tint and low-sun light together. A dusk normally comes up golden orange and a " +
                "dawn plain yellow; dawns take this roll at 45% of the configured chance, because the " +
                "haze that makes a sky burn builds up over the day.");
            _redDuskChance = Config.Bind("Sky", "Crimson twilight chance", 0.03f,
                "0-1. Probability of a deep crimson twilight. Checked before the pink roll. Dawns take " +
                "this roll at 30% of the configured chance.");
            _twilightWidth = Config.Bind("Sky", "Twilight width", 18f,
                "2-90 degrees of sun elevation. How far above and below the horizon a dawn or dusk "
                + "reaches at all. Larger makes twilight last longer.");
            _twilightFalloff = Config.Bind("Sky", "Twilight falloff", 3f,
                "0.2-8. Shape of the twilight inside that width. 1 is a straight ramp, so the sky "
                + "starts colouring while the sun is still high. Higher pushes the colour into the "
                + "last few degrees before the horizon and keeps the rest of the day clean.");
            _twilightWorldTint = Config.Bind("Sky", "Twilight world tint", 0.5f,
                "0-1. How strongly the whole frame (ground, buildings, characters, sea) is pulled " +
                "toward the twilight color while the sun crosses the horizon. 0 = only the sky and " +
                "the low sun change color, like before.");
            _starsEnabled = Config.Bind("Sky", "Stars", true,
                "Starfield that fades in at dusk and out at dawn, slowly wheeling across the night sky.");
            _starCount = Config.Bind("Sky", "Star count", 180,
                "40-800. Number of stylized stars. A restrained count fits the game's clean cartoon sky better.");
            _meteorChance = Config.Bind("Sky", "Shooting star chance", 0.035f,
                "0-1. Probability per second (at deep night) that a shooting star streaks across the " +
                "sky. 0.035 is roughly one every half minute.");

            _cloudsEnabled = Config.Bind("Clouds", "Enabled", true,
                "Stylized puffy clouds that drift across the sky. They tint with dawn/dusk, darken " +
                "under the night moon and can throw drifting shadows onto the island.");
            _cloudCount = Config.Bind("Clouds", "Cloud count", 24,
                "1-40. Number of clouds around the camera.");
            _cloudShadows = Config.Bind("Clouds", "Cast cloud shadows", false,
                "Clouds write the shadow map, so soft shadow patches sweep across the ground as they " +
                "drift. Costs some shadow-pass GPU; turn off first if the frame rate dips.");
            _cloudSpeed = Config.Bind("Clouds", "Wind speed", 0.8f,
                "Cloud drift speed in meters per second (0-30). Asset excavation proved every cloud in the " +
                "sky is generated by this mod (the baked PR_CloudBase set is deactivated), so a clearly visible " +
                "drift doubles as proof: a cloud that visibly moves is ours, and they all move.");
            _cloudDirection = Config.Bind("Clouds", "Wind direction", 25f,
                "Compass direction the clouds drift toward, in degrees.");
            _cloudAltitude = Config.Bind("Clouds", "Altitude", 50f,
                "Cloud base height in world units (25-150).");
            _cloudScale = Config.Bind("Clouds", "Cloud scale", 1f,
                "0.3-3. Overall cloud size multiplier.");
            _hideVanillaClouds = Config.Bind("Clouds", "Hide vanilla clouds", true,
                "Hides the original clouds in the main world so they do not overlap the mod's clouds. " +
                "Menu clouds and nearby player weather effects are preserved.");

            _weatherEnabled = Config.Bind("Weather", "Enabled", true,
                "Enables Day and Night rain events, visuals and the game's own rain ambience audio.");
            _randomRain = Config.Bind("Weather", "Random rain", true,
                "Starts and stops rain automatically after randomized clear/rain intervals.");
            _rainToggleKey = Config.Bind("Weather", "Toggle rain key", "F10",
                "Keyboard key that immediately starts or stops rain. F10 avoids FrameCare's F7-F9 keys.");
            _clearMinutesMin = Config.Bind("Weather", "Clear minutes minimum", 8f,
                "Minimum clear-weather time before a random rain event can start.");
            _clearMinutesMax = Config.Bind("Weather", "Clear minutes maximum", 18f,
                "Maximum clear-weather time before a random rain event starts.");
            _rainMinutesMin = Config.Bind("Weather", "Rain minutes minimum", 2.5f,
                "Minimum duration of a random rain event.");
            _rainMinutesMax = Config.Bind("Weather", "Rain minutes maximum", 5.5f,
                "Maximum duration of a random rain event.");
            _weatherTransitionSeconds = Config.Bind("Weather", "Transition seconds", 14f,
                "Seconds used to build or clear the storm instead of popping instantly.");
            _rainLightMultiplier = Config.Bind("Weather", "Rain light multiplier", 0.58f,
                "0.2-1. Directional and ambient light multiplier under full rain cover.");
            _rainExposureDip = Config.Bind("Weather", "Rain exposure dip", -0.18f,
                "Additional post exposure in EV during full rain.");
            _rainAudioVolume = Config.Bind("Weather", "Rain audio volume", 0.62f,
                "0-1. Combined volume of the game's long StormRain and Rain ambience layers.");
            _thunderChancePerMinute = Config.Bind("Weather", "Thunder chance per minute", 0.06f,
                "Average per-minute chance of a brief lightning flash and the game's built-in Thunder clip.");
            _rainDropSize = Config.Bind("Weather", "Rain drop size multiplier", 0.4f,
                "Visible size of raindrops (0.4-1.4). This does not change their fall speed.");
            _rainDensity = Config.Bind("Weather", "Rain density multiplier", 1.66f,
                "Amount of visible rain (0.5-2.0). Higher values may have a small performance cost.");

            _adjustSun = Config.Bind("Sun", "Adjust sun", true,
                "The game parks its directional light at a permanent overhead noon (90 degrees, straight down) " +
                "with shadow strength 0.22, which makes the world look flat. This tilts the sun to a pleasant " +
                "afternoon angle and restores shadow definition. Purely visual, does not affect gameplay.");
            _sunElevation = Config.Bind("Sun", "Elevation", 45f,
                "Sun height above the horizon in degrees. 90 restores the game's original overhead angle.");
            _sunAzimuth = Config.Bind("Sun", "Azimuth", 330f,
                "Compass direction the sunlight comes from, 0-360 degrees.");
            _sunIntensity = Config.Bind("Sun", "Intensity multiplier", 1f,
                "Multiplier on the sun light intensity.");
            _sunShadowStrength = Config.Bind("Sun", "Shadow strength", 0.65f,
                "0-1. How dark sun shadows are. The game's original value is 0.22 (nearly invisible).");
            _shadowDistance = Config.Bind("Sun", "Shadow distance", 220f,
                "Distance in world units within which shadows render, measured from the CAMERA. Zoomed-out " +
                "views put the whole frame beyond a short distance and every shadow in view vanishes at once " +
                "(the giant Circle_Hill cylinders made this obvious). Lower this first if shadow cost is too high.");
            _hardShadows = Config.Bind("Sun", "Hard shadows", false,
                "Uses crisp shadow edges instead of the smoother default. Smooth shadows usually look better on moving characters.");
            _shadowMapResolution = Config.Bind("Sun", "Shadow map resolution", 4096,
                "Main directional-light shadow atlas resolution. 4096 keeps character and tree shadows clear " +
                "across the long 220-unit shadow distance; lower values trade sharpness for performance.");
            _shadowCascadeCount = Config.Bind("Sun", "Shadow cascade count", 4,
                "Directional shadow cascades (1-4). Four preserves nearby shadow detail while the camera also " +
                "covers distant scenery.");
            _stabilizeShadowEdges = Config.Bind("Sun", "Stabilize moving shadow edges", true,
                "Uses high-quality spatial edge smoothing without frame-history blending. This avoids making " +
                "transparent character outlines fade while the character moves.");
            _sunNeutralize = Config.Bind("Sun", "Neutralize warm tint", 0.25f,
                "0-1. Blends the sun color from the game's warm cream toward neutral white.");
            _forceShadowCasting = Config.Bind("Sun", "Force shadow casting", true,
                "The game disables shadow casting on most environment objects (built for its shadowless " +
                "overhead sun). This re-enables it so the tilted sun casts consistent shadows everywhere. " +
                "Turn off if the shadow pass costs too much on your GPU.");
            _restoreTreeShadows = Config.Bind("Sun", "Restore tree shadows", true,
                "Forces confirmed tree crowns and trunks to cast and receive directional shadows. This " +
                "restores crown self-shadowing/back-side depth and lets trunks/crowns project onto the ground. " +
                "Disable only if the additional moving foliage shadow casters are too expensive.");
            _dayCycleEnabled = Config.Bind("Day cycle", "Enabled", true,
                "Real-clock day/night cycle: 'Noon at minute' of every hour is high noon and one full day " +
                "lasts 'Cycle minutes'. With the current defaults (0/60), a full day matches one real hour. " +
                "Overrides the static sun elevation/azimuth while active.");
            _cycleMinutes = Config.Bind("Day cycle", "Cycle minutes", 60f,
                "Length of one full day-night cycle in real minutes (5-1440). 1440 follows a full real day.");
            _noonMinute = Config.Bind("Day cycle", "Noon at minute", 0f,
                "Minute of the hour that counts as high noon.");
            _maxElevation = Config.Bind("Day cycle", "Max sun elevation", 80f,
                "Sun height above the horizon at noon, in degrees (20-80). Higher noon sun keeps shadows short.");
            _nightBrightness = Config.Bind("Day cycle", "Night brightness", 0.58f,
                "0-1. Moonlight intensity relative to daylight, so nights stay cozy instead of pitch black.");
            _minimumNightLight = Config.Bind("Day cycle", "Minimum visible night light", 0.34f,
                "0-1. Main-light floor after moon phase and rain multipliers. Keeps characters, trees and " +
                "paths readable during rainy new-moon nights without removing night color or lamp contrast.");
            _nightExposureDip = Config.Bind("Day cycle", "Night exposure dip", -0.12f,
                "Extra post exposure in EV applied at deep night. This darkens EVERYTHING on screen - " +
                "including water, chalkboards and other glowing unlit materials that ignore the sun.");

            _lampsEnabled = Config.Bind("Lamps", "Enabled", true,
                "EXPERIMENTAL: attaches warm point lights to lamp/lantern objects, lit automatically at " +
                "dusk and night (needs the day cycle). Only works if the game's shaders support additional " +
                "lights - check the ADDITIONAL LIGHTS log line.");
            _lampKeywords = Config.Bind("Lamps", "Name keywords", "lamp,lantern,streetlight,street_light,lamppost",
                "Comma-separated name fragments used to recognize lamp objects. The last run only found 5 " +
                "lamps, so if some street lights stay dark, add a fragment of their object name here and " +
                "press F4. 'lighthouse' is always excluded.");
            _lampStyle = Config.Bind("Lamps", "Style", "Both",
                "Light = real point lights that illuminate surroundings; Halo = a cartoon glow sprite on the " +
                "lamp itself, matching the vanilla art style; Both = the two combined.");
            _maxLitLamps = Config.Bind("Lamps", "Max lit lamps", 48,
                "Only the N nearest local lights (lamps and hanging string bulbs) are active at once. " +
                "Range: 1-256. Halos remain visible on every recognized light.");
            _haloSize = Config.Bind("Lamps", "Halo size", 1.3f,
                "Diameter of the glow sprite in world units.");
            _lampRange = Config.Bind("Lamps", "Range", 9f,
                "Point light range in world units.");
            _lampIntensity = Config.Bind("Lamps", "Intensity", 3.5f,
                "Point light intensity at deep night.");
            _deckStringLightsEnabled = Config.Bind("Lamps", "Colorful hanging string lights", true,
                "Adds point lights and visible halos to the original Tool Shop deck-light bulbs. " +
                "Their colors travel smoothly through a rainbow at night.");
            _deckStringLightRange = Config.Bind("Lamps", "Hanging string light range", 2f,
                "2-12. Point-light range for each hanging bulb.");
            _deckStringLightIntensity = Config.Bind("Lamps", "Hanging string light intensity", 0.2f,
                "0-5. Brightness for each hanging bulb before the shared local-light budget is applied.");
            _deckStringColorCycleSeconds = Config.Bind("Lamps", "Hanging string color cycle seconds", 18f,
                "4-90. Seconds for the hanging bulbs to complete one smooth rainbow cycle.");
            _deskLampRange = Config.Bind("Lamps", "Desk lamp range", 3.0f,
                "1-8. Range of the dedicated player-desk and library-table lamp lights.");
            _deskLampIntensity = Config.Bind("Lamps", "Desk lamp intensity", 0.42f,
                "0-3. Brightness of the dedicated player-desk and library-table lamp lights at deep night.");
            _campfireLightsEnabled = Config.Bind("Lamps", "Permanent campfire lights", true,
                "Adds real orange point lights to the two native CampFire roots. They remain on during the day " +
                "at reduced intensity, reserve two slots in the shared local-light budget, and make the native " +
                "Fire Red/Fire Yellow flame meshes emissive.");
            _treeLanternLightsEnabled = Config.Bind("Lamps", "Wicker hanging lantern lights", true,
                "Adds warm-white light only to the small woven hanging lamps under the large tree and by the canteen. " +
                "Swings and coloured string bulbs are not affected.");
            _playTowerSunLightEnabled = Config.Bind("Lamps", "Play Tower sun lamp", true,
                "Adds a warm-yellow point light to the native MD_PlayTower_PlanetSun ornament.");
            _deviceScreensGlow = Config.Bind("Lamps", "Personal device screen glow", true,
                "Adds a subtle self-lit look to the player's phone, laptop, and handheld game-console screens.");

            _waterDaylight = Config.Bind("Water", "Match water to daylight", true,
                "Water uses unlit materials that ignore the sun and glow at night. This drives the water " +
                "material colors with the day cycle instead: dark and blue-shifted at night, untouched at noon, " +
                "so sea and sky stay in the same palette.");
            _nightWaterBrightness = Config.Bind("Water", "Night water brightness", 0.09f,
                "0-1. Water body brightness at deep night relative to daytime. Rain applies an additional " +
                "body-only reduction so the sea stays below its wave highlights.");
            _dayWaterPatternColor = Config.Bind("Water", "Day water pattern color",
                new Color(0.78f, 0.88f, 0.92f, 1f),
                "RGB color used by sea, fountain and waterfall patterns in full daylight. It blends smoothly " +
                "with the night pattern color through dawn and dusk.");
            _nightWaterPatternColor = Config.Bind("Water", "Night water pattern color",
                new Color(0.25f, 0.33f, 0.42f, 1f),
                "RGB color used by sea, fountain and waterfall patterns at deep night. Adjust it from the F11 " +
                "menu while viewing the water; changes are saved and shown immediately.");
            _waterWaveBrightness = Config.Bind("Water", "Wave brightness above surface", 1.10f,
                "Final brightness multiplier for the selected water-pattern colors. It is independent of dark " +
                "water-body colors so ocean, fountain and waterfall patterns cannot turn black at night.");
            _nightWaterfallFoamBrightness = Config.Bind("Water", "Night waterfall foam brightness", 0.55f,
                "Brightness of actual fountain and waterfall spray particles at deep night. Only the particle " +
                "start color is adjusted; the original particle materials remain untouched.");
            _seaSkyReflection = Config.Bind("Water", "Sea reflects sky color", 0.68f,
                "0-1. How strongly the sea mirrors the color of the sky this plugin is currently drawing. " +
                "It drives the water's horizon color hardest, its shallow and deep body colors less, so a pink " +
                "or golden dusk sky produces a pink or golden sea instead of a grey in-between step. " +
                "Costs nothing: the sky color is computed, not rendered into a reflection texture.");
            _sunGlitterEnabled = Config.Bind("Water", "Sun glitter path", true,
                "Draws the specular track a low sun lays across the water, from the viewer out to the " +
                "horizon. Water material colors are uniform over the whole sea, so this is the only part " +
                "of a sunset that changes with which way you are facing.");
            _sunGlitterStrength = Config.Bind("Water", "Sun glitter strength", 1f,
                "0-2. Opacity multiplier for the sun's track on the water.");
            _sunGlitterCoverage = Config.Bind("Water", "Sun glitter coverage", 52f,
                "10-80 degrees. How far down from the horizon the track reaches, measured on screen "
                + "rather than in metres, so it covers the same share of the water whether you are "
                + "standing on the beach or looking down from the air. Higher covers more sea.");
            _sunGlitterSpread = Config.Bind("Water", "Sun glitter spread", 26f,
                "5-60 degrees. Half-width of the track where it is widest, next to the viewer. It "
                + "narrows toward the sun on its own, the way a real specular path does.");
            _twilightSeaBrightness = Config.Bind("Water", "Twilight sea brightness", 0.66f,
                "0-1. Lowest brightness the sea may reach while the sun is crossing the horizon, relative to " +
                "noon. The plain day-to-night fade darkens the sea long before the sky stops burning, which is " +
                "what turns dawn and dusk water grey. Raising this keeps the sea as luminous as the sky above it.");
            _waterShadowOverlayEnabled = Config.Bind("Water", "Directional shadow overlay", true,
                "Adds a transparent real-shadow receiver over the original horizontal water mesh. The native " +
                "Stylized Water material, including waves, foam and reflections, is left unchanged.");
            _waterShadowOverlayStrength = Config.Bind("Water", "Directional shadow overlay strength", 0.36f,
                "0.02-0.60. Opacity of the additional receiver: lower is subtler, higher makes sun shadows " +
                "on water easier to read.");

            _decorDimming = Config.Bind("Decor", "Dim light-ignoring decor at night", true,
                "Ground flowers, grass tufts and some other props use materials that ignore the sun " +
                "entirely (UTS light-color binding off, or unlit shaders), so they keep full daytime " +
                "brightness and appear to glow in the dark. This drives their colors with the day " +
                "cycle, like the water. Matched materials are listed in the log as DECOR lines.");
            _nightDecorBrightness = Config.Bind("Decor", "Night decor brightness", 0.55f,
                "0-1. Brightness of these props at deep night relative to daytime. These materials " +
                "cannot receive lamp light, so anywhere near a lit lamp they read darker than their " +
                "surroundings - keep this well above the water's night brightness.");
            _decorExcludeKeywords = Config.Bind("Decor", "Exclude keywords", "",
                "Comma-separated material-name fragments that should be left untouched (keep glowing " +
                "at night). Check the DECOR log lines for the exact material names.");

            _toonShading = Config.Bind("Shading", "Boost toon shadow contrast", true,
                "Improves light and shadow definition on scenery and characters while preserving their colors.");
            _shadeDarkness = Config.Bind("Shading", "Shade darkness", 0.66f,
                "Shadowed areas show the base color multiplied by this factor. Lower = darker, higher-contrast " +
                "shadows. 1 restores the (invisible) vanilla look.");
            _surfaceShadeStep = Config.Bind("Shading", "Main island surface shade step", 0.42f,
                "Controls how clearly shadows appear on the main island when the sun is low.");
            _surfaceNoonShadeStep = Config.Bind("Shading", "Main island noon shade step", 0.8f,
                "Controls how clearly soft shadows appear on the main island around noon.");
            _surfaceSystemShadowLevel = Config.Bind("Shading", "Main island system shadow level", -0.12f,
                "Fine adjustment for keeping shadows consistent across grass, paths, and roads (-0.5 to 0.5).");
            _playerShadeDarkness = Config.Bind("Shading", "Player shade darkness", 0.83f,
                "0.3-1. Player-only shade brightness. Kept higher than world shadows so faces and clothing do " +
                "not turn into a flat dark silhouette.");
            _playerSystemShadowLevel = Config.Bind("Shading", "Player system shadow level", -0.05f,
                "Fine adjustment for shadow visibility on characters only (-0.5 to 0.5).");
            _playerShadeStep = Config.Bind("Shading", "Player shade step", 0.45f,
                "Controls how much of a character is covered by the darker shade (0-1). Lower values keep faces brighter.");

            _ambientCoolShift = Config.Bind("Ambient", "Cool shift", 0.16f,
                "0-1. Nudges ambient light (what fills the shadows) toward a cooler blue-gray.");
            _ambientIntensity = Config.Bind("Ambient", "Intensity multiplier", 1f,
                "Multiplier for ambient light intensity.");

            _natureAmbienceEnabled = Config.Bind("Nature ambience", "Enabled", true,
                "Plays quiet CC0 outdoor ambience that follows the Day and Night cycle. " +
                "All bundled sources and license links are documented in audio/THIRD_PARTY_AUDIO.md.");
            _noonCicadaVolume = Config.Bind("Nature ambience", "Noon cicada volume", 0.055f,
                "0-1. Stereo cicada bed near high noon. The source recording is naturally loud, so the " +
                "default is deliberately low enough to sit behind work and conversation audio.");
            _nightNatureVolume = Config.Bind("Nature ambience", "Night insects and frogs volume", 0.06f,
                "0-1. Long rural-night recording with field crickets and occasional frogs at deep night.");
            _natureFadeSeconds = Config.Bind("Nature ambience", "Transition seconds", 12f,
                "Seconds used to fade nature beds in and out. Twilight stays mostly quiet between them.");
            _rainNatureMultiplier = Config.Bind("Nature ambience", "Rain volume multiplier", 0.18f,
                "0-1. Remaining nature-bed volume under full rain, keeping rain from becoming a noisy wall.");

            _seasonTheme = Config.Bind("Season", "Theme", "Summer",
                "Summer or Autumn. Summer is the game as authored. Autumn repaints the round tree " +
                "canopies, bushes, flower beds, grass tufts and - unless the ground switch below is " +
                "off - the lawn and its paths into red, orange and gold, drops leaves far more often, " +
                "and puts the hanging string lights on a Halloween palette.");
            _autumnStrength = Config.Bind("Season", "Autumn colour strength", 1f,
                "0-1. How far the autumn palette is mixed in over the summer colours. 1 is the full " +
                "repaint; lower values leave some green in the scene for an early-autumn look.");
            _autumnGroundRecolor = Config.Bind("Season", "Recolour lawn and paths", true,
                "Includes the island lawn and the nature paths crossing it in the autumn repaint. " +
                "Turn this off to keep the ground green and change only the planting.");
            _canopyShadeBrightness = Config.Bind("Season", "Canopy shade brightness", 0.6f,
                "0.3-1. How light the shaded side of a round tree canopy is, the same way the " +
                "[Shading] shade sliders work for everything else. 1 is almost no shading; lower " +
                "deepens the underside of every canopy, the band beneath it, and the difference " +
                "between the upper and lower discs of one tree.");
            _canopyShading = Config.Bind("Season", "Canopy shading", 0.5f,
                "0-1. How much of each round canopy is taken up by the dark band along its underside. " +
                "The shader that draws these canopies has no lighting of its own, so this band, plus " +
                "the ramp inside each disc, is where all of their light and shade comes from. 0 makes " +
                "them flat blocks of colour; too high and the crowns go dark.");
            _autumnWarmFilter = Config.Bind("Season", "Warm filter", 0.75f,
                "0-1. Strength of a warm amber cast over the whole autumn frame. It works on top of " +
                "the normal colour grade and follows the day cycle, so nights stay cool.");
            _leafFallRate = Config.Bind("Season", "Leaf fall rate", 4f,
                "Multiplier on the falling-leaf emitters in autumn. 1 leaves the game's own three " +
                "exactly as the game has them.");
            _leafFallTrees = Config.Bind("Season", "Shedding trees", 40,
                "Autumn only. The game only sheds leaves at three fixed spots on the whole island, so " +
                "this many tree canopies get an emitter of their own, largest trees first. Each one " +
                "costs a few dozen more particles; 0 disables it and leaves the game's three.");
            _groundLeafLimit = Config.Bind("Season", "Leaves on the ground", 400,
                "Autumn only. How many fallen leaves may lie on the ground at once. They build up " +
                "around wherever you are and stay there. 0 turns the ground layer off. This is the " +
                "one autumn setting worth lowering on a weak GPU, though the leaves are flat quads " +
                "in a single draw call and 400 costs very little.");
            _groundLeafLifetime = Config.Bind("Season", "Ground leaf lifetime", 0f,
                "Seconds a leaf lies on the ground before fading out. 0 means they never fade: the " +
                "layer fills to the limit above and then stays.");
            _halloweenStringLights = Config.Bind("Season", "Halloween string lights", true,
                "Autumn only. Puts the hanging string lights on pumpkin orange, candle amber, witch " +
                "purple, toxic green and blood red instead of the usual pastel rainbow.");

            _dappledCanopyShadows = Config.Bind("Sun", "Dappled canopy shadows", true,
                "The island's round tree canopies are solid ellipsoids, so each one casts a single " +
                "filled oval on the ground. With this on they stop casting themselves and a " +
                "perforated copy of the canopy is drawn into the shadow map in their place, which " +
                "breaks the shadow up into dapples. Costs one extra instanced draw per canopy batch, " +
                "shadow-only, and the holes make the shadow pass cheaper rather than dearer.");
            _canopyShadowGaps = Config.Bind("Sun", "Dapple density", 0.42f,
                "0-1. How much of the perforated canopy is holes. Holes are weighted toward the edge " +
                "of the crown, so the middle of the shadow stays solid the way a real canopy does.");

            _configMenuEnabled = Config.Bind("Interface", "Enable in-game config menu", true,
                "Enables the in-game settings menu. Changes are saved and applied automatically.");
            _configMenuKey = Config.Bind("Interface", "Config menu key", "F11",
                "Keyboard key used to open or close the settings menu. Esc also closes it.");
            _configMenuLanguage = Config.Bind("Interface", "Menu language", "Auto",
                "Language used by the in-game settings menu: Auto, Chinese, English or Japanese.");
            _configRevision = Config.Bind("Internal", "Config revision", 0,
                "Internal migration marker. Do not edit.");

            ApplySafePerformanceDefaults();
            EnsureChineseConfigNotes();
            RefreshWeatherConfig();
            SceneManager.sceneLoaded += OnSceneLoaded;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            ScheduleApply(6f);
            Logger.LogInfo("On-Together: Day and Night " + PluginVersion +
                " loaded. F4: reload config  F5: toggle sky  F6: toggle grading  " +
                _weatherToggleKey + ": toggle rain  " + _configMenuKeyCode + ": config menu");
        }

        private void ApplySafePerformanceDefaults()
        {
            bool changed = false;
            if (_configRevision.Value < 1)
            {
                if (_maxLitLamps.Value == 128)
                    _maxLitLamps.Value = 48;
                if (_cloudShadows.Value)
                    _cloudShadows.Value = false;
                _configRevision.Value = 1;
                changed = true;
            }

            if (_configRevision.Value < 2)
            {
                // The first performance hotfix forced this exact pair, but the 220-unit
                // shadow distance spreads it too thin and makes nearby shadows visibly blocky.
                // Restore the original quality pair only when both hotfix defaults are intact.
                if (_shadowMapResolution.Value == 2048 && _shadowCascadeCount.Value == 2)
                {
                    _shadowMapResolution.Value = 4096;
                    _shadowCascadeCount.Value = 4;
                }
                _configRevision.Value = 2;
                changed = true;
            }

            if (!changed)
                return;
            Config.Save();
            Logger.LogInfo("PERFORMANCE/QUALITY migrated 1.0.0 defaults: shadowAtlas=" +
                _shadowMapResolution.Value + " cascades=" + _shadowCascadeCount.Value +
                " maxLocalLights=" + _maxLitLamps.Value + " cloudShadows=" + _cloudShadows.Value);
        }

        private void OnDestroy()
        {
            if (_configMenuVisible)
                RestoreCursorAfterMenu();
            if (_ownsConfigMenuFont && _configMenuFont != null)
                Destroy(_configMenuFont);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            EndNeutralProfilePhotoRender();
            RestoreSky();
            RestoreAmbient();
            RestoreSun();
            RestoreShadowAntialiasing();
            RestoreShadowCasting();
            RestoreVanillaClouds();
            RestoreWaterParticles();
            ClearWaterShadowOverlays();
            RestoreWeather();
            RestoreNatureAudio();
            ClearLamps();
            RestoreCampfireEmission();
            RestoreDeviceScreenGlow();
            RestoreWaterMaterials();
            RestoreDecorMaterials();
            RestoreSeasonTheme();
            RestoreCanopyShadows();
            if (_canopyShadowMaterial != null)
                Destroy(_canopyShadowMaterial);
            DestroyGroundLeaves();
            RestoreNativeFeatureMaterials();
            RestoreToonShading();
            DestroyMoon();
            DestroySunsetGlow();
            DestroyStars();
            DestroyMeteor();
            if (_starTexture != null)
                Destroy(_starTexture);
            if (_autumnLeafMaterial != null)
                Destroy(_autumnLeafMaterial);
            if (_autumnLeafAtlas != null)
                Destroy(_autumnLeafAtlas);
            ClearClouds();
            if (_cloudMaterial != null)
                Destroy(_cloudMaterial);
            RestoreColorGradingMode();
            if (_volumeObject != null)
                Destroy(_volumeObject);
            if (_profile != null)
                Destroy(_profile);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                // The world streams many small additive scenes. Rebuilding the sky, every material,
                // hundreds of device overlays, clouds and stars for each chunk produced periodic stalls.
                // Existing incremental scans will pick up the newly loaded content without tearing down
                // the stable global state.
                float now = Time.unscaledTime;
                if (_profilePhotoCamera == null)
                    _profilePhotoLookupAttempted = false;
                if (!_sunCaptured || _sunLight == null || !_skyApplied)
                {
                    ScheduleApply(1f);
                    return;
                }
                _nextShadowRescan = now + 0.25f;
                _nextPlayerShadowRefresh = now + 0.25f;
                _nextSceneFeatureLampRescan = now + 0.25f;
                _nextVanillaCloudScan = now + 0.25f;
                _nextDecorRescan = now + 0.25f;
                _nextWaterEffectScan = now + 0.25f;
                // New chunks bring new canopies and leaf emitters with them.
                _nextSeasonRescan = now + 1.5f;
                _seasonRescanAttempts = 0;
                return;
            }

            // Scene loads reset RenderSettings, so previous captures are stale.
            RestoreShadowAntialiasing();
            RestoreCameraSkyboxes();
            RestoreShadowCasting();
            RestoreVanillaClouds();
            ClearWaterShadowOverlays();
            RestoreCampfireEmission();
            RestoreDeviceScreenGlow();
            RestoreNativeFeatureMaterials();
            _weatherFogCaptured = false;
            _skyApplied = false;
            _skyRuntimeEnabled = false;
            _ambientCaptured = false;
            _originalSkybox = null;
            _sunCaptured = false;
            _sunLight = null;
            _worldCamera = null;
            _profilePhotoCamera = null;
            _profilePhotoLookupAttempted = false;
            _playerDiagnosticsWritten = false;
            ClearLamps();
            ClearClouds();
            ClearStormClouds();
            // The canopy batches and the leaf emitters belong to the scene being replaced.
            // _seasonMaterials deliberately survives: those are shared material assets that
            // outlive the scene, and their captured summer colours are the only record of what
            // this plugin has to put back.
            RestoreCanopyShadows();
            _seasonCanopies.Clear();
            _canopyClusters.Clear();
            _leafEmitters.Clear();
            // The leaves lay on geometry that is going away with the scene.
            DestroyGroundLeaves();
            _appliedSeason = -1;
            _seasonRescanAttempts = 0;
            _nextSeasonRescan = 0f;
            _celestialValid = false;
            _worldSceneReady = false;
            _nextWorldProbe = 0f;
            HideCelestialVisuals();
            ScheduleApply(6f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                ReloadConfigFromDisk();
            if (_configMenuEnabled.Value && Input.GetKeyDown(_configMenuKeyCode))
                ToggleConfigMenu();
            if (_configMenuVisible && Input.GetKeyDown(KeyCode.Escape))
                ToggleConfigMenu();
            if (Input.GetKeyDown(KeyCode.F5))
                ToggleSky();
            if (Input.GetKeyDown(KeyCode.F6) && _volume != null)
            {
                _volume.enabled = !_volume.enabled;
                Logger.LogInfo("GRADING " + (_volume.enabled ? "ON" : "OFF"));
            }
            if (_weatherEnabled.Value && Input.GetKeyDown(_weatherToggleKey))
                SetRainTarget(!_rainTarget, true);

            if (_hotConfigDirty && Time.unscaledTime >= _hotConfigApplyAt)
            {
                _hotConfigDirty = false;
                Config.Save();
                EnsureChineseConfigNotes();
                RefreshWeatherConfig();
                RestoreAmbient();
                _ambientCaptured = false;
                ScheduleApply(0.05f);
                Logger.LogInfo("LIVE CONFIG saved and reapplied.");
            }

            UpdateWeather();
            UpdateDayCycle();
            RefreshNativeFeatureMaterials();
            UpdateDeviceScreenLights();
            UpdateNatureAmbience();
            UpdateGroundLeaves();
            UpdateMoonVisual();
            UpdateClouds();
            // Without the day cycle there is no twilight; make sure stale celestial
            // visuals do not stay parked in the sky.
            if (!_dayCycleEnabled.Value)
                HideCelestialVisuals();

            if (_lampsEnabled.Value && Time.unscaledTime >= _nextLampUpdate)
            {
                _nextLampUpdate = Time.unscaledTime + 1.5f;
                ScanForNewLamps(false);
                // Library tables and the player's desk can stream in after the world scene has
                // loaded. They are intentionally excluded from the generic lamp scan, so give
                // their exact feature scan its own periodic, de-duplicated pass.
                if (Time.unscaledTime >= _nextSceneFeatureLampRescan)
                {
                    _nextSceneFeatureLampRescan = Time.unscaledTime + 5f;
                    AddSceneFeatureLights();
                }
                UpdateLampSelection();
            }
            if (_lampHalos.Count > 0 && _lampWeight > 0.01f)
            {
                Camera billboardCamera = Camera.main;
                if (billboardCamera != null)
                {
                    Quaternion facing = billboardCamera.transform.rotation;
                    for (int i = 0; i < _lampHalos.Count; i++)
                    {
                        if (_lampHalos[i] != null)
                            _lampHalos[i].rotation = facing;
                    }
                }
            }
            if (_adjustSun.Value && Time.unscaledTime >= _nextShadowRescan)
            {
                // Catch objects that stream in after the initial pass.
                _nextShadowRescan = Time.unscaledTime + 60f;
                ApplyShadowCasting();
            }
            if (_hideVanillaClouds.Value && Time.unscaledTime >= _nextVanillaCloudScan)
            {
                // Cloud LODs and scene chunks can become active well after the initial scene pass.
                // forceRenderingOff survives LODGroup changes, while this rescan catches streamed renderers.
                _nextVanillaCloudScan = Time.unscaledTime + 5f;
                ApplyVanillaCloudHiding();
            }
            if (_worldSceneReady && Time.unscaledTime >= _nextSeasonRescan)
            {
                // The island streams in additively, so the first apply pass can run before a
                // single canopy or leaf emitter exists. Catch up once they do, then stop:
                // scanning every material and particle system in the scene is not free. The
                // deadline moves whether or not anything was due, so the check itself is not
                // paying for a per-frame theme lookup.
                _nextSeasonRescan = Time.unscaledTime + 8f;
                if (SeasonNeedsRescan())
                {
                    _seasonRescanAttempts++;
                    ApplySeasonTheme();
                    ApplyCanopyShadows();
                }
            }
            if (_decorDimming.Value && _worldSceneReady && Time.unscaledTime >= _nextDecorRescan)
            {
                // Face features (eyes, freckles, face paint) are unlit materials that the game
                // assigns into empty renderer slots only after avatar customization loads, so a
                // one-shot scene scan never sees them and they keep glowing at night. Rescanning
                // is incremental: already-tracked materials are skipped via _decorSeen.
                _nextDecorRescan = Time.unscaledTime + 10f;
                ScanDecorMaterials();
            }
            if (_waterDaylight.Value && _worldSceneReady && Time.unscaledTime >= _nextWaterEffectScan)
            {
                _nextWaterEffectScan = Time.unscaledTime + 10f;
                ScanWaterParticles();
                EnsureWaterShadowOverlays();
            }
            if (_adjustSun.Value && Time.unscaledTime >= _nextPlayerShadowRefresh)
            {
                // Player meshes and their pooled materials are created after the scene pass and can
                // change again on customization. Follow the game's actual player component instead
                // of hoping a one-shot global renderer scan happens at the right moment.
                _nextPlayerShadowRefresh = Time.unscaledTime + 5f;
                ApplyPlayerShadows();
            }
        }

        private void ReloadConfigFromDisk()
        {
            Config.Reload();
            EnsureChineseConfigNotes();
            RefreshWeatherConfig();
            RestoreAmbient();
            _ambientCaptured = false;
            ScheduleApply(0.1f);
            if (!_configMenuEnabled.Value && _configMenuVisible)
                ToggleConfigMenu();
            Logger.LogInfo("Config reloaded from disk, reapplying.");
        }

        private void QueueHotConfigApply()
        {
            _hotConfigDirty = true;
            _hotConfigApplyAt = Time.unscaledTime + 0.25f;
        }

        private void ApplyHotConfigNow()
        {
            _hotConfigDirty = false;
            Config.Save();
            EnsureChineseConfigNotes();
            RefreshWeatherConfig();
            RestoreAmbient();
            _ambientCaptured = false;
            ScheduleApply(0.05f);
            Logger.LogInfo("LIVE CONFIG applied from menu.");
        }

        private void EnsureChineseConfigNotes()
        {
            try
            {
                string path = Config.ConfigFilePath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;
                List<string> lines = new List<string>(File.ReadAllLines(path));
                bool changed = false;
                bool hasHeader = false;
                for (int i = 0; i < Mathf.Min(8, lines.Count); i++)
                {
                    if (lines[i].StartsWith("## 中文提示", StringComparison.Ordinal))
                    {
                        hasHeader = true;
                        break;
                    }
                }
                if (!hasHeader)
                {
                    lines.Insert(Mathf.Min(2, lines.Count),
                        "## 中文提示：游戏内按 F11 打开多语言实时设置；修改配置文件后按 F4 可重新读取。");
                    changed = true;
                }

                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    string line = lines[i].Trim();
                    if (line.Length < 3 || line[0] != '[' || line[line.Length - 1] != ']')
                        continue;
                    string section = line.Substring(1, line.Length - 2);
                    string note = ChineseSectionNote(section);
                    if (note == null)
                        continue;
                    bool present = false;
                    for (int j = i + 1; j < Mathf.Min(lines.Count, i + 5); j++)
                    {
                        if (lines[j] == note || lines[j].StartsWith("## 中文：", StringComparison.Ordinal))
                        {
                            present = true;
                            break;
                        }
                    }
                    if (!present)
                    {
                        lines.Insert(i + 1, note);
                        lines.Insert(i + 1, "");
                        changed = true;
                    }
                }
                if (changed)
                    File.WriteAllLines(path, lines.ToArray());
            }
            catch (Exception exception)
            {
                Logger.LogWarning("Could not add Chinese config notes: " + exception.GetType().Name);
            }
        }

        private static string ChineseSectionNote(string section)
        {
            switch (section)
            {
                case "Ambient": return "## 中文：环境光的冷暖与整体亮度。";
                case "Clouds": return "## 中文：模组卡通云、移动速度、尺寸、阴影与原版主世界云隐藏。";
                case "Day cycle": return "## 中文：按现实时间运行的昼夜循环、周期和日夜亮度。";
                case "Decor": return "## 中文：夜间压暗不受光照影响的花草和装饰材质。";
                case "Grading": return "## 中文：白平衡、曝光、对比度、饱和度与后处理开关。";
                case "Interface": return "## 中文：游戏内实时配置菜单与快捷键。";
                case "Lamps": return "## 中文：夜间路灯点光与卡通光晕。";
                case "Nature ambience": return "## 中文：正午蝉鸣、夜间虫蛙声及雨天音量衰减。";
                case "Season": return "## 中文：季节主题。秋季会把树冠、灌木、花坛、草坪与小路换成红橙黄配色，" +
                    "加大落叶，并把彩灯灯串换成万圣节配色。";
                case "Shading": return "## 中文：主岛与人物的阴影清晰度和明暗层次。";
                case "Sky": return "## 中文：自定义天空、太阳、月相、暮光与星空。";
                case "Sun": return "## 中文：太阳角度、亮度、阴影距离和质量。";
                case "Water": return "## 中文：海面、波纹、瀑布水花在昼夜及雨天中的亮度。";
                case "Weather": return "## 中文：随机或手动雨天、过渡、雨滴、雨声与雷声。F10 手动切换下雨。";
                default: return null;
            }
        }

        private void ToggleConfigMenu()
        {
            _configMenuVisible = !_configMenuVisible;
            if (_configMenuVisible)
            {
                _cursorLockBeforeMenu = Cursor.lockState;
                _cursorVisibleBeforeMenu = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                RestoreCursorAfterMenu();
            }
            Logger.LogInfo("CONFIG MENU " + (_configMenuVisible ? "OPEN" : "CLOSED"));
        }

        private void RestoreCursorAfterMenu()
        {
            Cursor.lockState = _cursorLockBeforeMenu;
            Cursor.visible = _cursorVisibleBeforeMenu;
        }

        private void EnsureConfigMenuStyles()
        {
            if (_menuTitleStyle != null)
                return;
            Font[] loadedFonts = Resources.FindObjectsOfTypeAll<Font>();
            for (int i = 0; i < loadedFonts.Length; i++)
            {
                Font candidate = loadedFonts[i];
                if (candidate != null && candidate.HasCharacter('中') && candidate.HasCharacter('あ'))
                {
                    _configMenuFont = candidate;
                    break;
                }
            }
            if (_configMenuFont == null)
            {
                try
                {
                    _configMenuFont = Font.CreateDynamicFontFromOSFont(new string[]
                    {
                        "Microsoft YaHei UI", "Yu Gothic UI", "Meiryo UI", "Arial Unicode MS"
                    }, 16);
                    _ownsConfigMenuFont = _configMenuFont != null;
                }
                catch (Exception exception)
                {
                    Logger.LogWarning("Chinese config-menu font fallback failed: " + exception.GetType().Name);
                }
            }
            _menuTitleStyle = new GUIStyle(GUI.skin.label);
            _menuTitleStyle.font = _configMenuFont;
            _menuTitleStyle.fontSize = 18;
            _menuTitleStyle.fontStyle = FontStyle.Bold;
            _menuTitleStyle.normal.textColor = new Color(0.94f, 0.97f, 1f, 1f);
            _menuSectionStyle = new GUIStyle(GUI.skin.label);
            _menuSectionStyle.font = _configMenuFont;
            _menuSectionStyle.fontSize = 16;
            _menuSectionStyle.fontStyle = FontStyle.Bold;
            _menuSectionStyle.normal.textColor = new Color(0.72f, 0.88f, 1f, 1f);
            _menuNoteStyle = new GUIStyle(GUI.skin.label);
            _menuNoteStyle.font = _configMenuFont;
            _menuNoteStyle.fontSize = 12;
            _menuNoteStyle.wordWrap = true;
            _menuNoteStyle.normal.textColor = new Color(0.76f, 0.80f, 0.86f, 1f);
            _menuValueStyle = new GUIStyle(GUI.skin.label);
            _menuValueStyle.font = _configMenuFont;
            _menuValueStyle.alignment = TextAnchor.MiddleRight;
            _menuValueStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            if (!_configMenuVisible || !_configMenuEnabled.Value)
                return;
            EnsureConfigMenuStyles();
            _configMenuRect.width = Mathf.Min(680f, Mathf.Max(420f, Screen.width - 40f));
            _configMenuRect.height = Mathf.Min(700f, Mathf.Max(360f, Screen.height - 40f));
            Color previousBackground = GUI.backgroundColor;
            Font previousFont = GUI.skin.font;
            if (_configMenuFont != null)
                GUI.skin.font = _configMenuFont;
            GUI.backgroundColor = new Color(0.34f, 0.43f, 0.58f, 0.98f);
            _configMenuRect = GUILayout.Window(190019, _configMenuRect, DrawConfigMenuWindow,
                "On-Together: Day and Night " + PluginVersion + "  " + MenuText("实时设置"),
                GUILayout.Width(_configMenuRect.width),
                GUILayout.Height(_configMenuRect.height));
            GUI.backgroundColor = previousBackground;
            GUI.skin.font = previousFont;
            _configMenuRect.x = Mathf.Clamp(_configMenuRect.x, 0f, Mathf.Max(0f, Screen.width - _configMenuRect.width));
            _configMenuRect.y = Mathf.Clamp(_configMenuRect.y, 0f, Mathf.Max(0f, Screen.height - _configMenuRect.height));
        }

        private void DrawConfigMenuWindow(int windowId)
        {
            GUILayout.Label(MenuText("修改后会自动保存并立即应用；按 F11 或 Esc 关闭。"), _menuTitleStyle);
            GUILayout.Label(MenuText("这里提供常用设置；更多选项仍可在配置文件中修改，并用 F4 重新读取。"),
                _menuNoteStyle);
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
            GUILayout.Label(MenuText("语言"), GUILayout.Width(80f));
            DrawLanguageButton("Auto", MenuText("自动"));
            DrawLanguageButton("Chinese", "中文");
            DrawLanguageButton("English", "English");
            DrawLanguageButton("Japanese", "日本語");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawMenuTab(0, "常用");
            DrawMenuTab(1, "季节");
            DrawMenuTab(2, "天空与光照");
            DrawMenuTab(3, "天气与云");
            DrawMenuTab(4, "画面");
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            _configMenuScroll = GUILayout.BeginScrollView(_configMenuScroll, false, true,
                GUILayout.Height(Mathf.Max(230f, _configMenuRect.height - 190f)));
            if (_configMenuTab == 0)
                DrawCommonConfigTab();
            else if (_configMenuTab == 1)
                DrawSeasonConfigTab();
            else if (_configMenuTab == 2)
                DrawSkyConfigTab();
            else if (_configMenuTab == 3)
                DrawWeatherConfigTab();
            else
                DrawGradingConfigTab();
            GUILayout.EndScrollView();

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(MenuText("保存并立即应用"), GUILayout.Height(30f)))
                ApplyHotConfigNow();
            if (GUILayout.Button(MenuText("从配置文件重新读取 (F4)"), GUILayout.Height(30f)))
                ReloadConfigFromDisk();
            if (GUILayout.Button(MenuText("关闭"), GUILayout.Height(30f)))
                ToggleConfigMenu();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0f, 0f, _configMenuRect.width, 28f));
        }

        private void DrawMenuTab(int index, string label)
        {
            bool selected = _configMenuTab == index;
            Color previous = GUI.backgroundColor;
            if (selected)
                GUI.backgroundColor = new Color(0.40f, 0.68f, 0.88f, 1f);
            if (GUILayout.Button(MenuText(label), GUILayout.Height(28f)))
            {
                _configMenuTab = index;
                _configMenuScroll = Vector2.zero;
            }
            GUI.backgroundColor = previous;
        }

        private void DrawLanguageButton(string value, string label)
        {
            bool selected = string.Equals(_configMenuLanguage.Value, value, StringComparison.OrdinalIgnoreCase);
            Color previous = GUI.backgroundColor;
            if (selected)
                GUI.backgroundColor = new Color(0.40f, 0.68f, 0.88f, 1f);
            if (GUILayout.Button(label, GUILayout.Height(24f)))
            {
                _configMenuLanguage.Value = value;
                Config.Save();
                EnsureChineseConfigNotes();
            }
            GUI.backgroundColor = previous;
        }

        private int GetMenuLanguage()
        {
            string value = (_configMenuLanguage.Value ?? "Auto").Trim().ToLowerInvariant();
            if (value == "english" || value == "en")
                return 1;
            if (value == "japanese" || value == "ja")
                return 2;
            if (value == "chinese" || value == "zh")
                return 0;
            string system = Application.systemLanguage.ToString().ToLowerInvariant();
            if (system.IndexOf("japanese") >= 0)
                return 2;
            if (system.IndexOf("chinese") >= 0)
                return 0;
            return 1;
        }

        private string MenuText(string chinese)
        {
            int language = GetMenuLanguage();
            if (language == 0)
                return chinese;
            switch (chinese)
            {
                case "实时设置": return language == 1 ? "Live Settings" : "リアルタイム設定";
                case "修改后会自动保存并立即应用；按 F11 或 Esc 关闭。": return language == 1
                    ? "Changes are saved and applied automatically. Press F11 or Esc to close."
                    : "変更は自動で保存・反映されます。F11 または Esc で閉じます。";
                case "这里提供常用设置；更多选项仍可在配置文件中修改，并用 F4 重新读取。": return language == 1
                    ? "Common settings are available here. More options remain in the config file; press F4 to reload it."
                    : "ここでは主な設定を変更できます。詳細設定は設定ファイルで変更し、F4 で再読み込みできます。";
                case "语言": return language == 1 ? "Language" : "言語";
                case "自动": return language == 1 ? "Auto" : "自動";
                case "常用": return language == 1 ? "General" : "基本";
                case "天空与光照": return language == 1 ? "Sky & Lighting" : "空と照明";
                case "天气与云": return language == 1 ? "Weather & Clouds" : "天気と雲";
                case "画面": return language == 1 ? "Visuals" : "画面";
                case "保存并立即应用": return language == 1 ? "Save and Apply" : "保存して適用";
                case "从配置文件重新读取 (F4)": return language == 1 ? "Reload Config File (F4)" : "設定ファイルを再読込 (F4)";
                case "关闭": return language == 1 ? "Close" : "閉じる";
                case "快速调整": return language == 1 ? "Quick Adjustments" : "クイック設定";
                case "启用昼夜天空": return language == 1 ? "Enable Day and Night Sky" : "昼夜の空を有効化";
                case "关闭后恢复原版天空；F5 也可快速对比。": return language == 1
                    ? "Turn this off to restore the original sky. F5 also provides a quick comparison."
                    : "オフにすると元の空へ戻ります。F5 でもすぐに比較できます。";
                case "启用色彩校正": return language == 1 ? "Enable Color Adjustments" : "色調補正を有効化";
                case "控制白平衡、曝光、对比度和阴影色调。": return language == 1
                    ? "Adjusts white balance, exposure, contrast, and shadow tone."
                    : "ホワイトバランス、露出、コントラスト、影の色合いを調整します。";
                case "启用真实时钟昼夜循环": return language == 1 ? "Use Real-Time Day Cycle" : "リアルタイム昼夜サイクル";
                case "保持当前按现实时间推进的昼夜循环。": return language == 1
                    ? "Runs the day and night cycle using the real-world clock."
                    : "現実の時刻に合わせて昼夜が進行します。";
                case "白天太阳尺寸": return language == 1 ? "Daytime Sun Size" : "昼の太陽サイズ";
                case "默认 0.028；日出/日落与白天太阳会在低空阶段平滑交接。": return language == 1
                    ? "Default: 0.028. The sun keeps a consistent size through dawn and dusk."
                    : "初期値は 0.028。日の出と日没でも自然な大きさを保ちます。";
                case "正午太阳高度": return language == 1 ? "Noon Sun Height" : "正午の太陽高度";
                case "正午太阳的最高高度；数值越高，影子越短。": return language == 1
                    ? "Sets the sun's highest point at noon. Higher values create shorter shadows."
                    : "正午の太陽の最高高度です。高いほど影が短くなります。";
                case "一轮昼夜分钟数": return language == 1 ? "Day Length (Minutes)" : "1日の長さ（分）";
                case "现实时间中一整个昼夜循环的长度。": return language == 1
                    ? "Length of one full day and night cycle in real minutes."
                    : "昼夜が一周するまでの現実時間（分）です。";
                case "现实时间中一整个昼夜循环的长度；1440 分钟时与现实一天同步。": return language == 1
                    ? "Length of one full day and night cycle in real minutes. At 1440 minutes it follows the real day."
                    : "昼夜が一周するまでの現実時間です。1440分では現実の1日と同期します。";
                case "雨滴尺寸倍率": return language == 1 ? "Raindrop Size" : "雨粒の大きさ";
                case "默认 0.72；只改变可见雨滴的尺寸。": return language == 1
                    ? "Default: 0.72. Changes the visible size of raindrops without changing fall speed."
                    : "初期値は 0.72。落下速度を変えず、見た目の大きさだけを調整します。";
                case "雨滴密度倍率": return language == 1 ? "Rain Density" : "雨の密度";
                case "默认 1.18；数值越高，雨量越大。": return language == 1
                    ? "Default: 1.18. Higher values create heavier rain and may affect performance."
                    : "初期値は 1.18。高くすると雨が密になり、処理負荷が増える場合があります。";
                case "立即停止下雨": return language == 1 ? "Stop Rain Now" : "雨をすぐ止める";
                case "立即开始下雨": return language == 1 ? "Start Rain Now" : "雨をすぐ降らせる";
                case "天空与太阳": return language == 1 ? "Sky and Sun" : "空と太陽";
                case "使用自定义天空": return language == 1 ? "Use Custom Sky" : "カスタムの空を使用";
                case "启用自定义天空、太阳、月相和星空。": return language == 1
                    ? "Enables the custom sky, sun, moon phases, and stars."
                    : "カスタムの空、太陽、月の満ち欠け、星空を有効にします。";
                case "控制白天太阳在天空中的显示大小；默认 0.028。": return language == 1
                    ? "Controls the apparent size of the daytime sun. Default: 0.028."
                    : "昼の太陽の見た目の大きさを調整します。初期値は 0.028 です。";
                case "大气厚度": return language == 1 ? "Atmosphere" : "大気の濃さ";
                case "越高天空越深蓝，地平线雾白也会更明显。": return language == 1
                    ? "Higher values deepen the blue sky and brighten the horizon haze."
                    : "高くすると空の青が深くなり、地平線の霞も明るくなります。";
                case "天空曝光": return language == 1 ? "Sky Brightness" : "空の明るさ";
                case "只影响天空盒亮度。": return language == 1 ? "Controls the overall brightness of the sky." : "空全体の明るさを調整します。";
                case "晚霞染色强度": return language == 1 ? "Sunset Colour on Scenery" : "夕焼けの景色への色付け";
                case "日出日落时，地面、建筑、人物与海面被染上暖色的程度。": return language == 1
                    ? "How strongly a sunrise or sunset colours the ground, buildings, characters and sea."
                    : "日の出や日没が地面、建物、キャラクター、海を染める強さです。";
                case "月亮尺寸": return language == 1 ? "Moon Size" : "月の大きさ";
                case "控制月亮在天空中的显示大小。": return language == 1 ? "Controls the moon's apparent size in the sky." : "空に見える月の大きさを調整します。";
                case "光照与阴影": return language == 1 ? "Lighting and Shadows" : "照明と影";
                case "夜间月光亮度": return language == 1 ? "Moonlight Brightness" : "月明かりの明るさ";
                case "满月相对日光的亮度。": return language == 1 ? "Brightness of full-moon light relative to daylight." : "満月の光を昼の光と比べた明るさです。";
                case "太阳阴影强度": return language == 1 ? "Sun Shadow Strength" : "太陽の影の濃さ";
                case "越高，投影越深。": return language == 1 ? "Higher values create darker shadows." : "高いほど影が濃くなります。";
                case "正午阴影清晰度": return language == 1 ? "Noon Shadow Clarity" : "正午の影の見やすさ";
                case "调整正午时柔和阴影的可见程度。": return language == 1 ? "Adjusts how clearly soft shadows appear around noon." : "正午付近の柔らかな影の見やすさを調整します。";
                case "稳定移动阴影边缘": return language == 1 ? "Stabilize Moving Shadows" : "動く影の輪郭を安定化";
                case "让太阳保持连续移动，同时减少长阴影边缘的闪烁和波形抖动。": return language == 1 ? "Keeps the sun moving continuously while reducing shimmer along long shadow edges." : "太陽を滑らかに動かしたまま、長い影の輪郭のちらつきを抑えます。";
                case "水面": return language == 1 ? "Water" : "水面";
                case "海面反映天空颜色": return language == 1 ? "Sea Reflects Sky Colour" : "海に空の色を反映";
                case "海面随天空变色的程度，数值越高，粉色或金色的晚霞越会映到海面上。": return language == 1
                    ? "How closely the sea follows the colour of the sky. Higher values carry more of a pink or golden sunset onto the water."
                    : "海が空の色にどれだけ追従するかです。高いほどピンクや金色の夕焼けが海面に映ります。";
                case "时间固定": return language == 1 ? "Time Lock" : "時間の固定";
                case "固定当前时刻": return language == 1 ? "Lock Time of Day" : "時刻を固定する";
                case "将天色固定在下方设定的时刻，其余功能照常运行。关闭后恢复正常的昼夜交替。": return language == 1
                    ? "Holds the sky at the time set below. All other features continue to run normally. Turn this off to resume the day and night cycle."
                    : "空を下で設定した時刻に固定します。他の機能は通常どおり動作します。オフにすると昼夜の移り変わりが再開します。";
                case "固定时刻": return language == 1 ? "Locked Time" : "固定する時刻";
                case "12 时为正午，0 时与 24 时为午夜，18 时前后为日落，6 时前后为日出。": return language == 1
                    ? "12 is noon, 0 and 24 are midnight, sunset falls around 18 and sunrise around 6."
                    : "12 は正午、0 と 24 は真夜中、日没は 18 時前後、日の出は 6 時前後です。";
                case "指定晚霞颜色": return language == 1 ? "Fixed Sunset Colour" : "夕焼けの色を指定";
                case "晚霞颜色默认随机。指定后将固定使用该配色：0 随机，1 金橙，2 晨间明黄，3 粉色，4 深红。": return language == 1
                    ? "Sunset colour is random by default. Choosing one here uses it every time: 0 random, 1 golden, 2 morning yellow, 3 pink, 4 deep red."
                    : "夕焼けの色は既定ではランダムです。ここで選ぶと常にその配色になります。0 ランダム、1 金色、2 朝の黄色、3 ピンク、4 深紅。";
                case "水面太阳倒影": return language == 1 ? "Sun Reflection on Water" : "水面の夕日の映り込み";
                case "在海面上绘制夕阳的反光带，由视点延伸至远处的太阳。": return language == 1
                    ? "Draws the sun's reflection across the sea, running from the viewer out to the sun."
                    : "夕日の反射を海面に描きます。視点から遠くの太陽まで伸びます。";
                case "倒影亮度": return language == 1 ? "Reflection Brightness" : "映り込みの明るさ";
                case "反光带的明显程度。": return language == 1
                    ? "How prominent the reflection appears."
                    : "反射の目立ちやすさです。";
                case "晚霞持续时长": return language == 1 ? "Sunset Duration" : "夕焼けの長さ";
                case "一次日出或日落持续的时间，数值越大持续越久。": return language == 1
                    ? "How long a sunrise or sunset lasts. Higher values make it last longer."
                    : "朝焼けや夕焼けが続く時間です。大きいほど長く続きます。";
                case "晚霞起始时机": return language == 1 ? "Sunset Onset" : "夕焼けの始まり";
                case "数值越小，太阳位置较高时天空即开始变色；数值越大，则接近地平线时才明显染色。": return language == 1
                    ? "Lower values start colouring the sky while the sun is still high. Higher values hold the colour back until the sun is close to the horizon."
                    : "小さいほど太陽が高い段階から空が色づき、大きいほど地平線に近づいてから色づきます。";
                case "倒影范围": return language == 1 ? "Reflection Reach" : "映り込みの範囲";
                case "反光带由远处向视点延伸的范围，数值越大染色的海面越多。在岸边与在高空所占的比例保持一致。": return language == 1
                    ? "How far the reflection extends from the distance back toward the viewer. Higher values colour more of the sea. The proportion stays the same from the shore or from the air."
                    : "反射が遠方から視点側へどこまで伸びるかです。大きいほど海が広く色づきます。岸辺でも上空でも占める割合は変わりません。";
                case "倒影宽度": return language == 1 ? "Reflection Width" : "映り込みの幅";
                case "反光带最宽处的宽度，靠近太阳一端会自动收窄。": return language == 1
                    ? "Width of the reflection at its widest point. The end nearest the sun narrows automatically."
                    : "反射の最も広い部分の幅です。太陽に近い側は自動的に細くなります。";
                case "日出日落海面亮度": return language == 1 ? "Sea Brightness at Sunset" : "朝夕の海の明るさ";
                case "太阳位于地平线附近时海面亮度的下限。过低会使海面先于天空变暗，出现灰蒙的过渡色。": return language == 1
                    ? "Lower limit on sea brightness while the sun is near the horizon. Values that are too low let the sea darken before the sky, producing a grey transition."
                    : "太陽が地平線付近にあるときの海面の明るさの下限です。低すぎると空より先に海が暗くなり、灰色がかった中間色になります。";
                case "夜间水面亮度": return language == 1 ? "Night Water Brightness" : "夜の水面の明るさ";
                case "控制深夜海水主体的亮度。": return language == 1 ? "Controls the main sea color at deep night." : "深夜の海面全体の明るさを調整します。";
                case "水波纹亮度": return language == 1 ? "Water Pattern Brightness" : "水面模様の明るさ";
                case "控制海面、喷泉和瀑布的浅色纹路；不会再跟随深色水体变黑。": return language == 1
                    ? "Controls the light patterns on the sea, fountains, and waterfalls without inheriting dark water colors."
                    : "海、噴水、滝の明るい模様を調整します。暗い水面の色を引き継いで黒くなることはありません。";
                case "白天水波纹颜色": return language == 1 ? "Day Water Pattern Color" : "昼の水面模様の色";
                case "夜间水波纹颜色": return language == 1 ? "Night Water Pattern Color" : "夜の水面模様の色";
                case "白天使用的水纹颜色；昼夜交替时会自动平滑过渡。": return language == 1
                    ? "Water-pattern color used in daylight. It blends smoothly during dawn and dusk."
                    : "昼に使う水面模様の色です。朝夕は滑らかに切り替わります。";
                case "深夜使用的水纹颜色；可一边观察水面一边实时调整。": return language == 1
                    ? "Water-pattern color used at deep night. Adjust it live while viewing the water."
                    : "深夜に使う水面模様の色です。水面を見ながらリアルタイムで調整できます。";
                case "红": return language == 1 ? "Red" : "赤";
                case "绿": return language == 1 ? "Green" : "緑";
                case "蓝": return language == 1 ? "Blue" : "青";
                case "夜间瀑布水花亮度": return language == 1 ? "Night Waterfall Spray" : "夜の滝しぶきの明るさ";
                case "单独控制瀑布顶部和底部的水花亮度。": return language == 1 ? "Separately controls spray and foam at the top and bottom of waterfalls." : "滝の上部と下部にある水しぶきの明るさを個別に調整します。";
                case "降雨": return language == 1 ? "Rain" : "雨";
                case "启用昼夜天气": return language == 1 ? "Enable Day and Night Weather" : "昼夜の天気を有効化";
                case "总开关；关闭会平滑结束当前雨天。": return language == 1 ? "Master switch. Turning it off gently clears the current rain." : "天気機能の主スイッチです。オフにすると現在の雨が自然に止みます。";
                case "随机下雨": return language == 1 ? "Random Rain" : "ランダムな雨";
                case "按配置的晴天/雨天区间自动切换。": return language == 1 ? "Automatically alternates between configured clear and rainy periods." : "設定した晴天・雨天の時間に合わせて自動で切り替えます。";
                case "默认 0.72；只改变雨滴大小，不改变落速。": return language == 1 ? "Default: 0.72. Changes raindrop size without changing fall speed." : "初期値は 0.72。落下速度を変えず、雨粒の大きさだけを調整します。";
                case "天气过渡秒数": return language == 1 ? "Weather Transition (Seconds)" : "天気の切替時間（秒）";
                case "风暴形成与散去的渐变时间。": return language == 1 ? "Time used for storms to build up and clear away." : "雨雲が広がり、また晴れるまでの移行時間です。";
                case "雨天主光倍率": return language == 1 ? "Rainy-Day Light" : "雨天の明るさ";
                case "越低，雨天越暗。": return language == 1 ? "Lower values make rainy weather darker." : "低いほど雨天が暗くなります。";
                case "雨声音量": return language == 1 ? "Rain Volume" : "雨音の音量";
                case "原生雨声层的合成音量。": return language == 1 ? "Controls the combined volume of the game's rain ambience." : "ゲーム内の雨音を合わせた音量です。";
                case "每分钟雷声概率": return language == 1 ? "Thunder Chance per Minute" : "1分ごとの雷の確率";
                case "短闪电与雷声的平均触发概率。": return language == 1 ? "Average chance of a lightning flash and thunder sound." : "稲光と雷鳴が発生する平均確率です。";
                case "云": return language == 1 ? "Clouds" : "雲";
                case "启用昼夜云": return language == 1 ? "Enable Day and Night Clouds" : "昼夜の雲を有効化";
                case "插件生成并随风移动的卡通云。": return language == 1 ? "Adds stylized clouds that move with the wind." : "風に流れる、やわらかな雰囲気の雲を追加します。";
                case "隐藏原版主世界云": return language == 1 ? "Hide Original World Clouds" : "元のワールド雲を非表示";
                case "隐藏主世界原有的云，同时保留菜单和玩家身边的天气效果。": return language == 1 ? "Hides the original world clouds while preserving menu and nearby weather effects." : "ワールドに元からある雲だけを隠し、メニューやプレイヤー周辺の天気表現は残します。";
                case "云投射阴影": return language == 1 ? "Cloud Shadows" : "雲の影";
                case "关闭可降低阴影通道开销。": return language == 1 ? "Turn this off to reduce the cost of cloud shadows." : "オフにすると雲の影による負荷を軽減できます。";
                case "云数量": return language == 1 ? "Cloud Count" : "雲の数";
                case "修改后会自动重建云层。": return language == 1 ? "The cloud field is rebuilt automatically after changes." : "変更後、雲は自動で作り直されます。";
                case "云移动速度": return language == 1 ? "Cloud Speed" : "雲の移動速度";
                case "单位为米/秒。": return language == 1 ? "Measured in metres per second." : "単位はメートル毎秒です。";
                case "云尺寸倍率": return language == 1 ? "Cloud Size" : "雲の大きさ";
                case "所有昼夜云的整体尺寸。": return language == 1 ? "Overall size of all Day and Night clouds." : "昼夜の雲全体の大きさです。";
                case "色彩校正": return language == 1 ? "Color Adjustments" : "色調補正";
                case "F6 仍可做临时 A/B 对比。": return language == 1 ? "F6 still provides a quick before-and-after comparison." : "F6 で補正前後をすぐに比較できます。";
                case "色温": return language == 1 ? "Temperature" : "色温度";
                case "负值偏冷，正值偏暖。": return language == 1 ? "Negative values are cooler; positive values are warmer." : "負の値で寒色、正の値で暖色になります。";
                case "绿色/洋红偏移": return language == 1 ? "Green / Magenta Tint" : "緑・マゼンタ補正";
                case "正值用于抵消草地主导的绿色偏色。": return language == 1 ? "Positive values reduce a strong green cast." : "正の値で強い緑かぶりを抑えます。";
                case "曝光 EV": return language == 1 ? "Exposure" : "露出";
                case "0 保留当前高光与太阳亮度。": return language == 1 ? "Zero preserves the current highlights and sun brightness." : "0 では現在のハイライトと太陽の明るさを保ちます。";
                case "对比度": return language == 1 ? "Contrast" : "コントラスト";
                case "正值增强明暗层次。": return language == 1 ? "Positive values increase light and dark separation." : "正の値で明暗差が強くなります。";
                case "饱和度": return language == 1 ? "Saturation" : "彩度";
                case "0 不改变原始饱和度。": return language == 1 ? "Zero preserves the original saturation." : "0 では元の彩度を保ちます。";
                case "卡通阴影与环境光": return language == 1 ? "Stylized Shadows and Ambient Light" : "影の表現と環境光";
                case "增强卡通阴影层次": return language == 1 ? "Improve Shadow Definition" : "影の立体感を強調";
                case "增强场景和人物的明暗层次。": return language == 1 ? "Improves light and shadow definition on scenery and characters." : "風景とキャラクターの明暗を見やすくします。";
                case "场景阴影亮度": return language == 1 ? "World Shadow Brightness" : "風景の影の明るさ";
                case "越低越暗；1 接近原版不明显的阴影。": return language == 1 ? "Lower values make shadows darker; 1 is close to the original appearance." : "低いほど影が暗くなり、1 で元の見た目に近づきます。";
                case "人物阴影亮度": return language == 1 ? "Character Shadow Brightness" : "キャラクターの影の明るさ";
                case "单独控制人物，避免面部变成黑块。": return language == 1 ? "Controls characters separately to keep faces readable." : "顔が暗くなりすぎないよう、キャラクターだけを個別に調整します。";
                case "环境光冷色偏移": return language == 1 ? "Cooler Ambient Light" : "環境光の寒色補正";
                case "让阴影区域略偏蓝灰。": return language == 1 ? "Gives shaded areas a subtle blue-grey tone." : "影の部分をわずかに青灰色へ寄せます。";
                case "环境光强度": return language == 1 ? "Ambient Light Strength" : "環境光の強さ";
                case "控制填充阴影的整体环境光。": return language == 1 ? "Controls the ambient light that fills shaded areas." : "影を照らす環境光全体の強さを調整します。";
                case "控制阴影在水面上的可见程度；不会影响水波、泡沫或反射。其余水面细节沿用推荐值。 ": return language == 1
                    ? "Controls how visible shadows are on water without changing waves, foam, or reflections. Other water details use the recommended values."
                    : "波、泡、反射を変えず、水面上の影の見え方だけを調整します。その他の水面設定は推奨値を使います。";
                case "水面接收阴影": return language == 1 ? "Shadows on Water" : "水面に影を落とす";
                case "在原水面上加一层透明的真实主光阴影接收层；不会改动波纹、泡沫或反射材质。": return language == 1
                    ? "Adds a transparent layer that receives real sun shadows without changing the original waves, foam, or reflection material."
                    : "元の波、泡、反射マテリアルを変えずに、太陽の影だけを受ける透明レイヤーを水面に追加します。";
                case "水面阴影浓度": return language == 1 ? "Water Shadow Depth" : "水面の影の濃さ";
                case "启用场景灯光": return language == 1 ? "Enable Scene Lights" : "シーン照明を有効化";
                case "启用路灯、灯笼和吊灯串的夜间局部照明。": return language == 1
                    ? "Enables local nighttime lighting for street lamps, hanging lamps, and string lights."
                    : "街灯、吊りランプ、電球の飾りに夜間の局所照明を追加します。";
                case "同时点亮的局部光源": return language == 1 ? "Simultaneous Local Lights" : "同時に点灯する局所ライト";
                case "距离镜头最近的这些灯会实际照亮场景；其余灯仍保留光晕。": return language == 1
                    ? "The closest lights illuminate the scene; the rest keep their visible glow."
                    : "カメラに近いライトだけが実際に周囲を照らし、残りは見た目の光だけを保ちます。";
                case "彩色吊灯串": return language == 1 ? "Color-Changing String Lights" : "色がゆっくり変わる電球飾り";
                case "给工具店甲板的原版吊灯串添加彩色渐变照明和光晕。": return language == 1
                    ? "Adds softly changing colored light and glow to the original string bulbs on the tool-shop deck."
                    : "工具店デッキにある元の電球飾りへ、ゆっくり色が変わる照明と光を追加します。";
                case "篝火全天照明": return language == 1 ? "Campfire Lighting All Day" : "焚き火を一日中照らす";
                case "篝火白天保持低亮度，夜间自动增强；火焰本体也会自发光，并始终占用两个本地光源名额。": return language == 1
                    ? "Keeps campfires faintly lit by day and brighter at night. The flame also glows, and two nearby-light slots are always reserved."
                    : "昼は控えめ、夜は明るく焚き火を照らします。炎も光り、近くのライト枠を常に2つ確保します。";
                case "游乐塔太阳灯": return language == 1 ? "Play-Tower Sun Lamp" : "遊具タワーの太陽ランプ";
                case "为原版太阳挂饰添加暖黄色夜间照明。": return language == 1
                    ? "Adds warm yellow nighttime lighting to the original sun ornament."
                    : "元の太陽の飾りに、暖かな黄色の夜間照明を追加します。";
                case "路灯照明范围": return language == 1 ? "Street-Light Range" : "街灯の照明範囲";
                case "普通路灯与场景灯的影响半径。较大的数值会明显增加光照范围。": return language == 1
                    ? "The radius of ordinary street and scene lights. Larger values noticeably widen their coverage."
                    : "通常の街灯とシーンライトが届く半径です。大きいほど照らす範囲が広がります。";
                case "路灯照明亮度": return language == 1 ? "Street-Light Brightness" : "街灯の明るさ";
                case "普通路灯与场景灯在深夜的亮度。": return language == 1
                    ? "Brightness of ordinary street and scene lights at deep night."
                    : "通常の街灯とシーンライトの深夜の明るさです。";
                case "彩灯照明范围": return language == 1 ? "String-Light Range" : "電球飾りの照明範囲";
                case "每个彩灯灯泡的影响半径。": return language == 1 ? "The radius of each colored string-light bulb." : "各色付き電球が届く半径です。";
                case "彩灯照明亮度": return language == 1 ? "String-Light Brightness" : "電球飾りの明るさ";
                case "每个彩灯灯泡在深夜的亮度。": return language == 1 ? "Brightness of each colored string-light bulb at deep night." : "各色付き電球の深夜の明るさです。";
                case "彩灯变色周期": return language == 1 ? "String-Light Color Cycle" : "電球飾りの色変化周期";
                case "彩灯完成一次平滑变色所需的秒数。": return language == 1
                    ? "Seconds the string lights take to complete one smooth color change."
                    : "電球飾りが一周分、滑らかに色を変えるまでの秒数です。";
                case "书桌台灯照明范围": return language == 1 ? "Desk-Lamp Range" : "デスクライトの照明範囲";
                case "玩家书桌和图书馆桌面台灯的影响半径。": return language == 1
                    ? "The radius of the player-desk and library-table lamps."
                    : "プレイヤーの机と図書館のテーブルライトが届く半径です。";
                case "书桌台灯照明亮度": return language == 1 ? "Desk-Lamp Brightness" : "デスクライトの明るさ";
                case "玩家书桌和图书馆桌面台灯在深夜的亮度。": return language == 1
                    ? "Brightness of the player-desk and library-table lamps at deep night."
                    : "プレイヤーの机と図書館のテーブルライトの深夜の明るさです。";
                case "夜间灯光": return language == 1 ? "Night Lighting" : "夜の照明";
                case "藤编挂灯暖白照明": return language == 1 ? "Warm Woven Hanging Lamps" : "編み込み吊りランプの暖色照明";
                case "照亮大树下和餐厅旁的藤编小挂灯；不会影响秋千、座椅或彩灯串。": return language == 1
                    ? "Lights the small woven lamps below the large tree and beside the canteen; swings, seats, and string bulbs are untouched."
                    : "大きな木の下と食堂横の小さな編み込み吊りランプだけを照らします。ブランコ、座席、電球の飾りは変わりません。";
                case "手机、笔记本与游戏机屏幕微光": return language == 1 ? "Device Screen Glow" : "端末画面の微光";
                case "让手机、笔记本和手持游戏机的屏幕保持轻微可见；游戏机画面会缓慢变色并偶尔跳变。": return language == 1
                    ? "Keeps phone, laptop, and handheld-console screens faintly visible; the console shifts colour with occasional game-like jumps."
                    : "スマホ、ノートPC、携帯ゲーム機の画面をほのかに見せます。ゲーム機の画面はゆっくり色が変わり、ときどきゲームらしく切り替わります。";
                case "环境音": return language == 1 ? "Nature Ambience" : "自然環境音";
                case "启用自然环境音": return language == 1 ? "Enable Nature Ambience" : "自然環境音を有効化";
                case "让蝉鸣和夜间虫鸣随昼夜平滑出现；下雨时会自动降低。 ": return language == 1
                    ? "Fades cicadas and night insects in with the day cycle, then lowers them automatically in rain."
                    : "蝉と夜の虫の音を昼夜に合わせて自然に切り替え、雨では自動的に小さくします。";
                case "正午蝉鸣音量": return language == 1 ? "Noon Cicada Volume" : "正午の蝉の音量";
                case "仅在接近正午时渐入，避免持续干扰。 ": return language == 1
                    ? "Fades in only near noon so it does not play constantly."
                    : "正午付近だけで徐々に聞こえるため、常に鳴り続けません。";
                case "夜间虫鸣音量": return language == 1 ? "Night Insect Volume" : "夜の虫の音量";
                case "夜深后渐入；与蝉鸣独立控制。 ": return language == 1
                    ? "Fades in after nightfall and is controlled independently from cicadas."
                    : "夜が深くなると徐々に聞こえ、蝉とは別に調整できます。";
                case "使树木与建筑的影子能够投在水面上，不影响水波、泡沫与反光。": return language == 1
                    ? "Allows shadows from trees and buildings to fall on the water. Waves, foam and reflections are unaffected."
                    : "木や建物の影が水面に落ちるようにします。波、泡、反射には影響しません。";
                case "水面上阴影的明显程度。": return language == 1
                    ? "How strongly shadows show on the water."
                    : "水面に落ちる影の濃さです。";
                case "人物阴影分界": return language == 1 ? "Character Shade Edge" : "キャラの影の境目";
                case "只作用于角色：阴影带的分界位置，越大受光面越小。": return language == 1
                    ? "Characters only: where the shaded band starts. Higher leaves less of them lit."
                    : "キャラのみ：影の帯が始まる位置です。大きいほど明るい部分が狭くなります。";
                case "只作用于角色：越低人物自身阴影越深，1 接近没有阴影。": return language == 1
                    ? "Characters only: lower makes their own shading darker. 1 is almost no shading."
                    : "キャラのみ：低いほど自身の陰影が濃くなります。1 はほぼ陰影なしです。";
                case "季节": return language == 1 ? "Season" : "季節";
                case "季节主题": return language == 1 ? "Season Theme" : "季節テーマ";
                case "主题": return language == 1 ? "Theme" : "テーマ";
                case "夏季（原版）": return language == 1 ? "Summer (original)" : "夏（オリジナル）";
                case "秋季": return language == 1 ? "Autumn" : "秋";
                case "秋季会把球型树冠、灌木、花坛和草丛换成红橙黄的配色，加大落叶，并把彩灯灯串换成万圣节配色。尖顶的针叶木保持原样。":
                    return language == 1
                    ? "Autumn repaints the round tree canopies, bushes, flower beds and grass tufts in red, orange and gold, sheds leaves far more often, and puts the string lights on a Halloween palette. The conical evergreens are left as they are."
                    : "秋は丸い樹冠・低木・花壇・草むらを赤やオレンジ、金色に塗り替え、落ち葉を大幅に増やし、電飾をハロウィンの配色にします。円錐形の針葉樹はそのままです。";
                case "下面的选项只在秋季主题下生效。": return language == 1
                    ? "The settings below only take effect with the autumn theme."
                    : "以下の設定は秋テーマのときだけ効果があります。";
                case "秋季配色": return language == 1 ? "Autumn Palette" : "秋の配色";
                case "秋色浓度": return language == 1 ? "Autumn Colour Strength" : "秋色の強さ";
                case "秋季配色与原版配色的混合程度。1 为完全换色，较低的数值会保留一些绿意，像初秋。":
                    return language == 1
                    ? "How far the autumn palette is mixed over the summer colours. 1 is the full repaint; lower values keep some green for an early-autumn look."
                    : "秋の配色を夏の色にどれだけ重ねるかです。1 で完全に塗り替え、低くすると緑が残り初秋のようになります。";
                case "树冠暗面亮度": return language == 1 ? "Canopy Shade Brightness" : "樹冠の陰の明るさ";
                case "球型树冠背光面的亮度，与其他物件的阴影亮度滑块是同一种手感。1 接近没有暗面；调低会同时加深每片树冠的下半部、底部的暗带，以及同一棵树上下层树冠之间的明暗差。":
                    return language == 1
                    ? "How light the shaded side of a round canopy is, the same way the other shade sliders work. 1 is almost no shading; lower deepens the underside of each disc, the band beneath it, and the difference between the upper and lower discs of one tree."
                    : "丸い樹冠の影側の明るさです。他の陰影スライダーと同じ感覚で使えます。1 でほぼ陰影なし、下げるほど各樹冠の下側・その下の帯・同じ木の上下の樹冠の明暗差が濃くなります。";
                case "树冠明暗层次": return language == 1 ? "Canopy Shading" : "樹冠の陰影";
                case "球型树冠所用的着色器本身没有光照，它的明暗全部来自底部的暗带和每片树冠内部的上下渐变。0 会让树冠变成一整块平色；过高则树冠整体偏暗。":
                    return language == 1
                    ? "The shader these round canopies use has no lighting of its own, so all of their light and shade comes from the dark band along the underside and the ramp inside each disc. 0 makes them one flat block of colour; too high and the crowns go dark."
                    : "丸い樹冠のシェーダーには照明がないため、陰影はすべて下側の暗い帯と、各樹冠内部のグラデーションから来ています。0 では真っ平らな色になり、上げすぎると樹冠全体が暗くなります。";
                case "树影斑驳": return language == 1 ? "Dappled Tree Shadows" : "木漏れ日の影";
                case "球型树冠的影子原本是一整块实心的椭圆。开启后改由一个带孔洞的树冠形状来投影，影子里会出现细碎的光斑。":
                    return language == 1
                    ? "A round canopy otherwise casts one solid oval of shadow. This casts it from a perforated copy of the canopy instead, so the shadow breaks up into dapples of light."
                    : "丸い樹冠は本来、塗りつぶした楕円の影を落とします。オンにすると穴の空いた樹冠から影を落とし、影の中に木漏れ日ができます。";
                case "光斑密度": return language == 1 ? "Dapple Density" : "木漏れ日の量";
                case "树冠影子里孔洞所占的比例。数值越高，影子越碎、越透光；过高会让树影几乎消失。":
                    return language == 1
                    ? "How much of the canopy shadow is holes. Higher breaks the shadow up more and lets more light through; too high and the shadow all but disappears."
                    : "樹冠の影に空ける穴の割合です。高いほど影が細かく砕け、光がよく抜けます。上げすぎると影がほとんど消えます。";
                case "暖色滤镜强度": return language == 1 ? "Warm Filter" : "暖色フィルター";
                case "给整个画面叠加的暖琥珀色调。夜间会自动淡出，不影响夜晚的冷色。": return language == 1
                    ? "A warm amber cast over the whole frame. It fades out at night, so nights keep their cool tone."
                    : "画面全体に暖かい琥珀色をかけます。夜は自動的に弱まるので、夜の寒色はそのままです。";
                case "包含草坪与小路": return language == 1 ? "Include lawn and paths" : "芝生と小道も含める";
                case "让草坪和岛上的小路一起换成秋季配色。关闭则只改变植被。": return language == 1
                    ? "Repaints the island lawn and the paths crossing it as well. Turn this off to change only the planting."
                    : "島の芝生とその小道も秋の色に塗り替えます。オフにすると植物だけが変わります。";
                case "落叶": return language == 1 ? "Falling Leaves" : "落ち葉";
                case "落叶频率倍率": return language == 1 ? "Leaf Fall Rate" : "落ち葉の量";
                case "落叶发射器的倍率。1 表示原版自带的三处与原版完全一致。": return language == 1
                    ? "Multiplier on the falling-leaf emitters. 1 leaves the game's own three exactly as it has them."
                    : "落ち葉エミッターの倍率です。1 でゲーム本来の三か所はそのままになります。";
                case "落叶的树木数量": return language == 1 ? "Shedding Trees" : "落ち葉を降らせる木の数";
                case "原版全岛只有三处会落叶。这里设置有多少棵树会自己落叶，从最大的树开始。每一棵会多出几十个粒子；0 表示只保留原版那三处。":
                    return language == 1
                    ? "The game only sheds leaves at three fixed spots on the whole island. This many canopies get an emitter of their own, largest trees first. Each costs a few dozen more particles; 0 leaves just the game's three."
                    : "ゲームでは島全体で三か所しか落ち葉が出ません。ここで指定した数の樹冠に、大きい木から順に専用のエミッターを付けます。1 本につき数十個の粒子が増えます。0 なら本来の三か所だけです。";
                case "地面落叶数量上限": return language == 1 ? "Leaves on the Ground" : "地面に積もる葉の数";
                case "地面上最多同时堆积多少片落叶。它们会在你所在的位置附近逐渐积累并留在原地。0 表示关闭。落叶是合并成一次绘制的平面片，400 片的开销很小。":
                    return language == 1
                    ? "How many fallen leaves may lie on the ground at once. They build up around wherever you are and stay there. 0 turns the layer off. They are flat quads merged into a single draw call, so 400 costs very little."
                    : "地面に同時に積もる葉の枚数です。今いる場所の周りに少しずつ積もり、そのまま残ります。0 でオフ。1 回の描画にまとめた平らな板なので、400 枚でも負荷はごくわずかです。";
                case "地面落叶停留秒数": return language == 1 ? "Ground Leaf Lifetime" : "地面の葉が残る秒数";
                case "地面落叶在消失前停留的秒数。0 表示永不消失：堆到上限后就一直留着。": return language == 1
                    ? "Seconds a fallen leaf lies on the ground before fading. 0 means they never fade: the layer fills to the limit and stays."
                    : "落ち葉が消えるまで地面に残る秒数です。0 なら消えません。上限まで積もったあとはそのまま残ります。";
                case "彩灯": return language == 1 ? "String Lights" : "電飾";
                case "万圣节彩灯配色": return language == 1 ? "Halloween string lights" : "ハロウィンの電飾";
                case "把吊灯串在南瓜橙和女巫紫之间切换，而不是原本的柔和彩虹色。": return language == 1
                    ? "Steps the hanging string lights between pumpkin orange and witch purple instead of the usual pastel rainbow."
                    : "吊り下げ電飾を、いつものパステルの虹色ではなくカボチャのオレンジと魔女の紫で切り替えます。";
                case "人物接收投影强度": return language == 1 ? "Shadows Cast on Characters" : "キャラに落ちる影の強さ";
                case "只作用于角色：越低，落在人物身上的投影越明显。": return language == 1
                    ? "Characters only: lower makes shadows falling onto them more obvious."
                    : "キャラのみ：低いほど、キャラに落ちる影がはっきりします。";
                default: return chinese;
            }
        }

        private void DrawCommonConfigTab()
        {
            MenuSection("快速调整");
            MenuToggle(_replaceSkybox, "启用昼夜天空", "关闭后恢复原版天空；F5 也可快速对比。");
            MenuToggle(_gradingEnabled, "启用色彩校正", "控制白平衡、曝光、对比度和阴影色调。");
            MenuToggle(_dayCycleEnabled, "启用真实时钟昼夜循环", "保持当前按现实时间推进的昼夜循环。");
            MenuSlider(_cycleMinutes, "一轮昼夜分钟数", 5f, 1440f, 0,
                "现实时间中一整个昼夜循环的长度；1440 分钟时与现实一天同步。");
            MenuSection("时间固定");
            MenuToggle(_freezeTime, "固定当前时刻",
                "将天色固定在下方设定的时刻，其余功能照常运行。关闭后恢复正常的昼夜交替。");
            MenuSlider(_frozenHour, "固定时刻", 0f, 24f, 1,
                "12 时为正午，0 时与 24 时为午夜，18 时前后为日落，6 时前后为日出。");
            MenuIntSlider(_twilightVariantOverride, "指定晚霞颜色", 0, 4,
                "晚霞颜色默认随机。指定后将固定使用该配色：0 随机，1 金橙，2 晨间明黄，3 粉色，4 深红。");
            GUILayout.Space(8f);
            if (GUILayout.Button(MenuText(_rainTarget ? "立即停止下雨" : "立即开始下雨"), GUILayout.Height(32f)))
                SetRainTarget(!_rainTarget, true);
        }

        private void DrawSeasonConfigTab()
        {
            MenuSection("季节主题");
            GUILayout.BeginHorizontal();
            GUILayout.Label(MenuText("主题"), GUILayout.Width(120f));
            DrawSeasonButton("Summer", "夏季（原版）");
            DrawSeasonButton("Autumn", "秋季");
            GUILayout.EndHorizontal();
            GUILayout.Label(MenuText("秋季会把球型树冠、灌木、花坛和草丛换成红橙黄的配色，加大落叶，" +
                "并把彩灯灯串换成万圣节配色。尖顶的针叶木保持原样。"), _menuNoteStyle);

            bool autumn = IsAutumnTheme();
            if (!autumn)
            {
                GUILayout.Space(6f);
                GUILayout.Label(MenuText("下面的选项只在秋季主题下生效。"), _menuNoteStyle);
            }

            MenuSection("秋季配色");
            MenuSlider(_autumnStrength, "秋色浓度", 0f, 1f, 2,
                "秋季配色与原版配色的混合程度。1 为完全换色，较低的数值会保留一些绿意，像初秋。");
            MenuSlider(_canopyShadeBrightness, "树冠暗面亮度", 0.3f, 1f, 2,
                "球型树冠背光面的亮度，与其他物件的阴影亮度滑块是同一种手感。1 接近没有暗面；" +
                "调低会同时加深每片树冠的下半部、底部的暗带，以及同一棵树上下层树冠之间的明暗差。");
            MenuSlider(_canopyShading, "树冠明暗层次", 0f, 1f, 2,
                "球型树冠所用的着色器本身没有光照，它的明暗全部来自底部的暗带和每片树冠内部的上下渐变。" +
                "0 会让树冠变成一整块平色；过高则树冠整体偏暗。");
            MenuSlider(_autumnWarmFilter, "暖色滤镜强度", 0f, 1f, 2,
                "给整个画面叠加的暖琥珀色调。夜间会自动淡出，不影响夜晚的冷色。");
            MenuToggle(_autumnGroundRecolor, "包含草坪与小路",
                "让草坪和岛上的小路一起换成秋季配色。关闭则只改变植被。");

            MenuSection("落叶");
            MenuSlider(_leafFallRate, "落叶频率倍率", 0f, 20f, 1,
                "落叶发射器的倍率。1 表示原版自带的三处与原版完全一致。");
            MenuIntSlider(_leafFallTrees, "落叶的树木数量", 0, 120,
                "原版全岛只有三处会落叶。这里设置有多少棵树会自己落叶，从最大的树开始。" +
                "每一棵会多出几十个粒子；0 表示只保留原版那三处。");
            MenuIntSlider(_groundLeafLimit, "地面落叶数量上限", 0, 2000,
                "地面上最多同时堆积多少片落叶。它们会在你所在的位置附近逐渐积累并留在原地。" +
                "0 表示关闭。落叶是合并成一次绘制的平面片，400 片的开销很小。");
            MenuSlider(_groundLeafLifetime, "地面落叶停留秒数", 0f, 600f, 0,
                "地面落叶在消失前停留的秒数。0 表示永不消失：堆到上限后就一直留着。");

            MenuSection("彩灯");
            MenuToggle(_halloweenStringLights, "万圣节彩灯配色",
                "把吊灯串在南瓜橙和女巫紫之间切换，而不是原本的柔和彩虹色。");
        }

        private void DrawSeasonButton(string value, string label)
        {
            bool selected = string.Equals(_seasonTheme.Value, value, StringComparison.OrdinalIgnoreCase);
            Color previous = GUI.backgroundColor;
            if (selected)
                GUI.backgroundColor = new Color(0.40f, 0.68f, 0.88f, 1f);
            if (GUILayout.Button(MenuText(label), GUILayout.Height(26f)) && !selected)
            {
                _seasonTheme.Value = value;
                QueueHotConfigApply();
            }
            GUI.backgroundColor = previous;
        }

        private void DrawSkyConfigTab()
        {
            MenuSection("天空与太阳");
            MenuToggle(_replaceSkybox, "使用自定义天空", "启用自定义天空、太阳、月相和星空。");
            MenuSlider(_sunSize, "白天太阳尺寸", 0.01f, 0.08f, 3,
                "控制白天太阳在天空中的显示大小；默认 0.028。");
            MenuSlider(_atmosphereThickness, "大气厚度", 0.1f, 3f, 2,
                "越高天空越深蓝，地平线雾白也会更明显。");
            MenuSlider(_skyExposure, "天空曝光", 0.05f, 8f, 2, "只影响天空盒亮度。");
            MenuSlider(_twilightWorldTint, "晚霞染色强度", 0f, 4f, 2,
                "日出日落时，地面、建筑、人物与海面被染上暖色的程度。");
            MenuSlider(_twilightWidth, "晚霞持续时长", 2f, 90f, 0,
                "一次日出或日落持续的时间，数值越大持续越久。");
            MenuSlider(_twilightFalloff, "晚霞起始时机", 0.2f, 8f, 2,
                "数值越小，太阳位置较高时天空即开始变色；数值越大，则接近地平线时才明显染色。");
            MenuSlider(_moonSize, "月亮尺寸", 0.025f, 0.14f, 3, "控制月亮在天空中的显示大小。");
            MenuSection("光照与阴影");
            MenuSlider(_maxElevation, "正午太阳高度", 20f, 80f, 0,
                "正午太阳的最高高度；数值越高，影子越短。");
            MenuSlider(_nightBrightness, "夜间月光亮度", 0f, 1f, 2, "满月相对日光的亮度。");
            MenuSlider(_sunShadowStrength, "太阳阴影强度", 0f, 1f, 2, "越高，投影越深。");
            MenuSlider(_surfaceNoonShadeStep, "正午阴影清晰度", 0.3f, 1f, 2,
                "调整正午时柔和阴影的可见程度。");
            MenuToggle(_stabilizeShadowEdges, "稳定移动阴影边缘",
                "让太阳保持连续移动，同时减少长阴影边缘的闪烁和波形抖动。");
            MenuToggle(_dappledCanopyShadows, "树影斑驳",
                "球型树冠的影子原本是一整块实心的椭圆。开启后改由一个带孔洞的树冠形状来投影，" +
                "影子里会出现细碎的光斑。");
            MenuSlider(_canopyShadowGaps, "光斑密度", 0f, 0.85f, 2,
                "树冠影子里孔洞所占的比例。数值越高，影子越碎、越透光；过高会让树影几乎消失。");
            MenuSection("水面");
            MenuSlider(_seaSkyReflection, "海面反映天空颜色", 0f, 1f, 2,
                "海面随天空变色的程度，数值越高，粉色或金色的晚霞越会映到海面上。");
            MenuSlider(_twilightSeaBrightness, "日出日落海面亮度", 0f, 2f, 2,
                "太阳位于地平线附近时海面亮度的下限。过低会使海面先于天空变暗，出现灰蒙的过渡色。");
            MenuToggle(_sunGlitterEnabled, "水面太阳倒影",
                "在海面上绘制夕阳的反光带，由视点延伸至远处的太阳。");
            MenuSlider(_sunGlitterStrength, "倒影亮度", 0f, 6f, 2,
                "反光带的明显程度。");
            MenuSlider(_sunGlitterCoverage, "倒影范围", 3f, 89f, 0,
                "反光带由远处向视点延伸的范围，数值越大染色的海面越多。在岸边与在高空所占的比例保持一致。");
            MenuSlider(_sunGlitterSpread, "倒影宽度", 0.5f, 89f, 1,
                "反光带最宽处的宽度，靠近太阳一端会自动收窄。");
            MenuToggle(_waterShadowOverlayEnabled, "水面接收阴影",
                "使树木与建筑的影子能够投在水面上，不影响水波、泡沫与反光。");
            MenuSlider(_waterShadowOverlayStrength, "水面阴影浓度", 0.02f, 0.60f, 2,
                "水面上阴影的明显程度。");
            MenuSection("夜间灯光");
            MenuToggle(_lampsEnabled, "启用场景灯光", "启用路灯、灯笼和吊灯串的夜间局部照明。");
            MenuIntSlider(_maxLitLamps, "同时点亮的局部光源", 1, 256,
                "距离镜头最近的这些灯会实际照亮场景；其余灯仍保留光晕。");
            MenuSlider(_lampRange, "路灯照明范围", 2f, 25f, 1,
                "普通路灯与场景灯的影响半径。较大的数值会明显增加光照范围。");
            MenuSlider(_lampIntensity, "路灯照明亮度", 0f, 8f, 2,
                "普通路灯与场景灯在深夜的亮度。");
            MenuToggle(_deckStringLightsEnabled, "彩色吊灯串", "给工具店甲板的原版吊灯串添加彩色渐变照明和光晕。");
            MenuSlider(_deckStringLightRange, "彩灯照明范围", 2f, 12f, 1,
                "每个彩灯灯泡的影响半径。");
            MenuSlider(_deckStringLightIntensity, "彩灯照明亮度", 0f, 5f, 2,
                "每个彩灯灯泡在深夜的亮度。");
            MenuSlider(_deckStringColorCycleSeconds, "彩灯变色周期", 4f, 90f, 0,
                "彩灯完成一次平滑变色所需的秒数。");
            MenuSlider(_deskLampRange, "书桌台灯照明范围", 1f, 8f, 1,
                "玩家书桌和图书馆桌面台灯的影响半径。");
            MenuSlider(_deskLampIntensity, "书桌台灯照明亮度", 0f, 3f, 2,
                "玩家书桌和图书馆桌面台灯在深夜的亮度。");
            MenuToggle(_campfireLightsEnabled, "篝火全天照明", "篝火白天保持低亮度，夜间自动增强；火焰本体也会自发光，并始终占用两个本地光源名额。");
            MenuToggle(_treeLanternLightsEnabled, "藤编挂灯暖白照明",
                "照亮大树下和餐厅旁的藤编小挂灯；不会影响秋千、座椅或彩灯串。");
            MenuToggle(_playTowerSunLightEnabled, "游乐塔太阳灯", "为原版太阳挂饰添加暖黄色夜间照明。");
            MenuToggle(_deviceScreensGlow, "手机、笔记本与游戏机屏幕微光",
                "让手机、笔记本和手持游戏机的屏幕保持轻微可见；游戏机画面会缓慢变色并偶尔跳变。");
        }

        private void DrawWeatherConfigTab()
        {
            MenuSection("降雨");
            MenuToggle(_weatherEnabled, "启用昼夜天气", "总开关；关闭会平滑结束当前雨天。");
            MenuToggle(_randomRain, "随机下雨", "按配置的晴天/雨天区间自动切换。");
            MenuSlider(_rainDropSize, "雨滴尺寸倍率", 0.4f, 1.4f, 2,
                "默认 0.72；只改变雨滴大小，不改变落速。");
            MenuSlider(_rainDensity, "雨滴密度倍率", 0.5f, 2f, 2,
                "默认 1.18；数值越高，雨量越大。");
            MenuSlider(_weatherTransitionSeconds, "天气过渡秒数", 2f, 60f, 1,
                "风暴形成与散去的渐变时间。");
            MenuSlider(_rainLightMultiplier, "雨天主光倍率", 0.2f, 1f, 2, "越低，雨天越暗。");
            MenuSlider(_rainAudioVolume, "雨声音量", 0f, 1f, 2, "原生雨声层的合成音量。");
            MenuSlider(_thunderChancePerMinute, "每分钟雷声概率", 0f, 1f, 2, "短闪电与雷声的平均触发概率。");
            if (GUILayout.Button(MenuText(_rainTarget ? "立即停止下雨" : "立即开始下雨"), GUILayout.Height(30f)))
                SetRainTarget(!_rainTarget, true);

            MenuSection("云");
            MenuToggle(_cloudsEnabled, "启用昼夜云", "插件生成并随风移动的卡通云。");
            MenuToggle(_hideVanillaClouds, "隐藏原版主世界云", "隐藏主世界原有的云，同时保留菜单和玩家身边的天气效果。");
            MenuToggle(_cloudShadows, "云投射阴影", "关闭可降低阴影通道开销。");
            MenuIntSlider(_cloudCount, "云数量", 1, 40, "修改后会自动重建云层。");
            MenuSlider(_cloudSpeed, "云移动速度", 0f, 30f, 1, "单位为米/秒。");
            MenuSlider(_cloudScale, "云尺寸倍率", 0.3f, 3f, 2, "所有昼夜云的整体尺寸。");

            MenuSection("环境音");
            MenuToggle(_natureAmbienceEnabled, "启用自然环境音", "让蝉鸣和夜间虫鸣随昼夜平滑出现；下雨时会自动降低。 ");
            MenuSlider(_noonCicadaVolume, "正午蝉鸣音量", 0f, 1f, 3, "仅在接近正午时渐入，避免持续干扰。 ");
            MenuSlider(_nightNatureVolume, "夜间虫鸣音量", 0f, 1f, 3, "夜深后渐入；与蝉鸣独立控制。 ");
        }

        private void DrawGradingConfigTab()
        {
            MenuSection("色彩校正");
            MenuToggle(_gradingEnabled, "启用色彩校正", "F6 仍可做临时 A/B 对比。");
            MenuSlider(_temperature, "色温", -100f, 100f, 0, "负值偏冷，正值偏暖。");
            MenuSlider(_tint, "绿色/洋红偏移", -100f, 100f, 0, "正值用于抵消草地主导的绿色偏色。");
            MenuSlider(_postExposure, "曝光 EV", -2f, 2f, 2, "0 保留当前高光与太阳亮度。");
            MenuSlider(_contrast, "对比度", -100f, 100f, 0, "正值增强明暗层次。");
            MenuSlider(_saturation, "饱和度", -100f, 100f, 0, "0 不改变原始饱和度。");
            MenuSection("卡通阴影与环境光");
            MenuToggle(_toonShading, "增强卡通阴影层次", "增强场景和人物的明暗层次。");
            MenuSlider(_shadeDarkness, "场景阴影亮度", 0.3f, 1f, 2, "越低越暗；1 接近原版不明显的阴影。");
            MenuSlider(_playerShadeDarkness, "人物阴影亮度", 0.3f, 1f, 2,
                "只作用于角色：越低人物自身阴影越深，1 接近没有阴影。");
            MenuSlider(_playerShadeStep, "人物阴影分界", 0.05f, 0.8f, 2,
                "只作用于角色：阴影带的分界位置，越大受光面越小。");
            MenuSlider(_playerSystemShadowLevel, "人物接收投影强度", -0.5f, 0.5f, 2,
                "只作用于角色：越低，落在人物身上的投影越明显。");
            MenuSlider(_ambientCoolShift, "环境光冷色偏移", 0f, 1f, 2, "让阴影区域略偏蓝灰。");
            MenuSlider(_ambientIntensity, "环境光强度", 0.2f, 2f, 2, "控制填充阴影的整体环境光。");
        }

        private void MenuSection(string title)
        {
            GUILayout.Space(8f);
            GUILayout.Label(MenuText(title), _menuSectionStyle);
        }

        private void MenuToggle(ConfigEntry<bool> entry, string label, string note)
        {
            bool next = GUILayout.Toggle(entry.Value, MenuText(label));
            if (next != entry.Value)
            {
                entry.Value = next;
                QueueHotConfigApply();
            }
            GUILayout.Label(MenuText(note), _menuNoteStyle);
        }

        private void MenuSlider(ConfigEntry<float> entry, string label, float minimum, float maximum,
            int decimals, string note)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(MenuText(label), GUILayout.Width(250f));
            float next = GUILayout.HorizontalSlider(entry.Value, minimum, maximum, GUILayout.MinWidth(220f));
            float factor = Mathf.Pow(10f, Mathf.Clamp(decimals, 0, 4));
            next = Mathf.Round(next * factor) / factor;
            string format = decimals <= 0 ? "F0" : (decimals == 1 ? "F1" : (decimals == 2 ? "F2" : "F3"));
            GUILayout.Label(next.ToString(format), _menuValueStyle, GUILayout.Width(64f));
            GUILayout.EndHorizontal();
            if (Mathf.Abs(next - entry.Value) > 0.0001f)
            {
                entry.Value = next;
                QueueHotConfigApply();
            }
            GUILayout.Label(MenuText(note), _menuNoteStyle);
        }

        private void MenuIntSlider(ConfigEntry<int> entry, string label, int minimum, int maximum, string note)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(MenuText(label), GUILayout.Width(250f));
            int next = Mathf.RoundToInt(GUILayout.HorizontalSlider(entry.Value, minimum, maximum,
                GUILayout.MinWidth(220f)));
            GUILayout.Label(next.ToString(), _menuValueStyle, GUILayout.Width(64f));
            GUILayout.EndHorizontal();
            if (next != entry.Value)
            {
                entry.Value = next;
                QueueHotConfigApply();
            }
            GUILayout.Label(MenuText(note), _menuNoteStyle);
        }

        private void MenuColor(ConfigEntry<Color> entry, string label, string note)
        {
            Color current = entry.Value;
            GUILayout.BeginHorizontal();
            GUILayout.Label(MenuText(label), GUILayout.Width(250f));
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = current;
            GUILayout.Box("", GUILayout.Width(72f), GUILayout.Height(22f));
            GUI.backgroundColor = previousBackground;
            string hex = ColorUtility.ToHtmlStringRGB(current);
            GUILayout.Label("#" + hex, _menuValueStyle, GUILayout.Width(90f));
            GUILayout.EndHorizontal();

            float red = MenuColorChannel("红", current.r);
            float green = MenuColorChannel("绿", current.g);
            float blue = MenuColorChannel("蓝", current.b);
            Color next = new Color(red, green, blue, 1f);

            if (Mathf.Abs(next.r - current.r) > 0.0001f ||
                Mathf.Abs(next.g - current.g) > 0.0001f ||
                Mathf.Abs(next.b - current.b) > 0.0001f)
            {
                entry.Value = next;
                QueueHotConfigApply();
            }
            GUILayout.Label(MenuText(note), _menuNoteStyle);
        }

        private float MenuColorChannel(string label, float normalized)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(250f);
            GUILayout.Label(MenuText(label), GUILayout.Width(48f));
            float value = GUILayout.HorizontalSlider(normalized * 255f, 0f, 255f, GUILayout.MinWidth(180f));
            value = Mathf.Round(value);
            GUILayout.Label(value.ToString("F0"), _menuValueStyle, GUILayout.Width(48f));
            GUILayout.EndHorizontal();
            return value / 255f;
        }

        private void ScheduleApply(float delay)
        {
            if (_applyRoutine != null)
                StopCoroutine(_applyRoutine);
            _applyRoutine = StartCoroutine(ApplyAfterDelay(delay));
        }

        private IEnumerator ApplyAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            _applyRoutine = null;
            DumpSceneState();
            ApplyGrading();
            ApplySky();
            ApplySun();
            RestoreShadowCasting();
            ApplyShadowCasting();
            _nextShadowRescan = Time.unscaledTime + 60f;
            _nextPlayerShadowRefresh = Time.unscaledTime + 5f;
            ApplyLamps();
            ScanWaterMaterials();
            _nextWaterEffectScan = Time.unscaledTime + 10f;
            _seasonRescanAttempts = 0;
            _nextSeasonRescan = Time.unscaledTime + 4f;
            ApplySeasonTheme();
            ApplyCanopyShadows();
            ScanDecorMaterials();
            ApplyToonShading();
            // Screen glow must be created after the generic Toon pass. Otherwise F4 creates
            // it briefly, then that pass treats it as a normal shadowed surface and darkens it.
            ApplyDeviceScreenGlow();
            ApplyAmbient();
            ClearStormClouds();
            BuildClouds();
            ApplyVanillaCloudHiding();
            _nextVanillaCloudScan = Time.unscaledTime + 5f;
            BuildStars();
            ApplyWeather();
            EnsureNatureAudio();
        }

        private void LateUpdate()
        {
            // The game turns camera post-processing off; keep re-enabling it
            // so the grading volume can take effect.
            if (Time.unscaledTime < _nextPostCheck)
                return;
            _nextPostCheck = Time.unscaledTime + 1f;
            Camera camera = _worldCamera != null ? _worldCamera : Camera.main;
            if (camera == null)
                return;
            if (_worldCamera == null)
                _worldCamera = camera;
            UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
                ApplyShadowAntialiasing(camera, data);
            if (data != null && !data.renderPostProcessing)
            {
                data.renderPostProcessing = true;
                _postForcedCount++;
                if (_postForcedCount == 1 || _postForcedCount % 120 == 0)
                    Logger.LogWarning("Camera post-processing was off again; re-enabled (count " + _postForcedCount + ").");
            }

            // RenderSettings belong to the ACTIVE scene. When the game switches the active
            // scene (moving between additively-loaded islands/areas), the vanilla "Day6 Main"
            // skybox comes back - complete with its painted speckles and clouds, which read
            // as "vanilla clouds that survived every renderer-based hide". Take the skybox
            // back the same way the post-processing flag is retaken.
            if (_replaceSkybox.Value && _skyApplied && _skyRuntimeEnabled && _skyMaterial != null &&
                RenderSettings.skybox != _skyMaterial)
            {
                RenderSettings.skybox = _skyMaterial;
                _skyboxForcedCount++;
                DynamicGI.UpdateEnvironment();
                Logger.LogWarning("Skybox was swapped back by the game (active scene change); " +
                    "re-applied Day and Night sky (count " + _skyboxForcedCount + ").");
            }
        }

        // RenderSettings is not the final authority when a Camera has its own Skybox
        // component. The game also swaps active-scene RenderSettings late in the frame.
        // Enforce the LookCare sky at the last reliable SRP boundary, immediately before
        // each skybox camera renders. This removes the painted clouds in Day6 Main rather
        // than merely hiding the PR_CloudBase geometry below it.
        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == null)
                return;

            bool isProfilePhotoCamera = IsProfilePhotoCamera(camera);
            if (_worldCamera == null && !isProfilePhotoCamera && camera.name == "Main Camera")
                _worldCamera = camera;
            bool isMainCamera = camera == _worldCamera;
            // A global volume otherwise grades transparent sticker/overlay cameras and the
            // persistent ID-photo capture. Enable it only for the actual world camera.
            if (_volume != null)
                _volume.enabled = _gradingEnabled.Value && isMainCamera;

            if (isProfilePhotoCamera)
                BeginNeutralProfilePhotoRender(camera);

            if (!_replaceSkybox.Value || !_skyApplied || !_skyRuntimeEnabled ||
                _skyMaterial == null || !isMainCamera ||
                camera.clearFlags != CameraClearFlags.Skybox)
                return;

            bool renderSettingsChanged = RenderSettings.skybox != _skyMaterial;
            if (renderSettingsChanged)
            {
                RenderSettings.skybox = _skyMaterial;
                _skyboxForcedCount++;
            }

            Skybox cameraSkybox = camera.GetComponent<Skybox>();
            bool cameraOverrideChanged = false;
            string originalCameraMaterial = "<none>";
            if (cameraSkybox != null && cameraSkybox.enabled)
            {
                int id = cameraSkybox.GetInstanceID();
                CameraSkyboxState state;
                if (!_cameraSkyboxStates.TryGetValue(id, out state))
                {
                    state = new CameraSkyboxState();
                    state.Skybox = cameraSkybox;
                    state.OriginalMaterial = cameraSkybox.material;
                    _cameraSkyboxStates.Add(id, state);
                }
                originalCameraMaterial = state.OriginalMaterial != null
                    ? state.OriginalMaterial.name : "<null>";
                if (cameraSkybox.material != _skyMaterial)
                {
                    cameraSkybox.material = _skyMaterial;
                    cameraOverrideChanged = true;
                    _cameraSkyboxForcedCount++;
                }
            }

            if (renderSettingsChanged || cameraOverrideChanged)
            {
                Logger.LogWarning("SKY RENDER GUARD camera=" + camera.name +
                    " renderSettingsForced=" + renderSettingsChanged +
                    " cameraOverrideForced=" + cameraOverrideChanged +
                    " originalCameraSky=" + originalCameraMaterial +
                    " totals=(" + _skyboxForcedCount + "," + _cameraSkyboxForcedCount + ")");
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != null && camera == _neutralPhotoRenderCamera)
                EndNeutralProfilePhotoRender();
        }

        private bool IsProfilePhotoCamera(Camera camera)
        {
            if (camera == null || ProfilePhotoCameraField == null)
                return false;
            if (_profilePhotoCamera == null && !_profilePhotoLookupAttempted)
            {
                _profilePhotoLookupAttempted = true;
                PhotoController controller = UnityEngine.Object.FindFirstObjectByType<PhotoController>(
                    FindObjectsInactive.Include);
                if (controller != null)
                    _profilePhotoCamera = ProfilePhotoCameraField.GetValue(controller) as Camera;
            }
            return camera == _profilePhotoCamera;
        }

        private void BeginNeutralProfilePhotoRender(Camera camera)
        {
            if (_neutralPhotoRenderCamera != null || camera == null)
                return;
            _neutralPhotoRenderCamera = camera;
            if (_volume != null)
            {
                _neutralPhotoVolumeEnabled = _volume.enabled;
                _volume.enabled = false;
            }
            if (_sunLight != null && _sunCaptured)
            {
                _neutralPhotoSunRotation = _sunLight.transform.rotation;
                _neutralPhotoSunColor = _sunLight.color;
                _neutralPhotoSunIntensity = _sunLight.intensity;
                _neutralPhotoSunShadowStrength = _sunLight.shadowStrength;
                _sunLight.transform.rotation = _origSunRotation;
                _sunLight.color = _origSunColor;
                _sunLight.intensity = _origSunIntensity;
                _sunLight.shadowStrength = _origSunShadowStrength;
            }
            _neutralPhotoAmbientLight = RenderSettings.ambientLight;
            _neutralPhotoAmbientSky = RenderSettings.ambientSkyColor;
            _neutralPhotoAmbientEquator = RenderSettings.ambientEquatorColor;
            _neutralPhotoAmbientGround = RenderSettings.ambientGroundColor;
            _neutralPhotoAmbientIntensity = RenderSettings.ambientIntensity;
            _neutralPhotoReflectionIntensity = RenderSettings.reflectionIntensity;
            if (_ambientCaptured)
            {
                RenderSettings.ambientLight = _origAmbientLight;
                RenderSettings.ambientSkyColor = _origAmbientSky;
                RenderSettings.ambientEquatorColor = _origAmbientEquator;
                RenderSettings.ambientGroundColor = _origAmbientGround;
                RenderSettings.ambientIntensity = _origAmbientIntensity;
                RenderSettings.reflectionIntensity = _origReflectionIntensity;
            }
        }

        private void EndNeutralProfilePhotoRender()
        {
            if (_neutralPhotoRenderCamera == null)
                return;
            if (_sunLight != null && _sunCaptured)
            {
                _sunLight.transform.rotation = _neutralPhotoSunRotation;
                _sunLight.color = _neutralPhotoSunColor;
                _sunLight.intensity = _neutralPhotoSunIntensity;
                _sunLight.shadowStrength = _neutralPhotoSunShadowStrength;
            }
            RenderSettings.ambientLight = _neutralPhotoAmbientLight;
            RenderSettings.ambientSkyColor = _neutralPhotoAmbientSky;
            RenderSettings.ambientEquatorColor = _neutralPhotoAmbientEquator;
            RenderSettings.ambientGroundColor = _neutralPhotoAmbientGround;
            RenderSettings.ambientIntensity = _neutralPhotoAmbientIntensity;
            RenderSettings.reflectionIntensity = _neutralPhotoReflectionIntensity;
            if (_volume != null)
                _volume.enabled = _neutralPhotoVolumeEnabled;
            _neutralPhotoRenderCamera = null;
        }

        // ---------------------------------------------------------------
        // Color grading via a high-priority global URP volume
        // ---------------------------------------------------------------
        private void ApplyGrading()
        {
            string mode = (_tonemapping.Value != null ? _tonemapping.Value : "None").Trim().ToLowerInvariant();
            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
            {
                if (!_colorGradingModeCaptured)
                {
                    _originalColorGradingMode = asset.colorGradingMode;
                    _colorGradingModeCaptured = true;
                }
                ColorGradingMode targetMode = mode == "none"
                    ? _originalColorGradingMode : ColorGradingMode.HighDynamicRange;
                if (asset.colorGradingMode != targetMode)
                {
                    Logger.LogInfo("Color grading mode: " + asset.colorGradingMode + " -> " + targetMode +
                        (mode == "none" ? " (preserve authored highlights)" : " (for tonemapping)"));
                    asset.colorGradingMode = targetMode;
                }
            }

            if (_volumeObject == null)
            {
                _volumeObject = new GameObject("LookCare Global Volume");
                DontDestroyOnLoad(_volumeObject);
                _volume = _volumeObject.AddComponent<Volume>();
                _volume.isGlobal = true;
                _volume.priority = 5000f;
            }

            if (_profile != null)
                Destroy(_profile);
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();

            WhiteBalance wb = _profile.Add<WhiteBalance>(true);
            wb.temperature.Override(Mathf.Clamp(_temperature.Value, -100f, 100f));
            wb.tint.Override(Mathf.Clamp(_tint.Value, -100f, 100f));

            ColorAdjustments ca = _profile.Add<ColorAdjustments>(true);
            ca.postExposure.Override(_postExposure.Value);
            ca.contrast.Override(Mathf.Clamp(_contrast.Value, -100f, 100f));
            ca.saturation.Override(Mathf.Clamp(_saturation.Value, -100f, 100f));
            ca.colorFilter.Override(_dayColorFilter.Value);
            ca.hueShift.Override(0f);
            _colorAdjustments = ca;

            if (_disableBloom.Value)
            {
                Bloom bloom = _profile.Add<Bloom>(true);
                bloom.intensity.Override(0f);
            }
            if (_disableVignette.Value)
            {
                Vignette vignette = _profile.Add<Vignette>(true);
                vignette.intensity.Override(0f);
            }

            float cool = Mathf.Clamp01(_shadowCooling.Value);
            LiftGammaGain lgg = _profile.Add<LiftGammaGain>(true);
            lgg.lift.Override(new Vector4(1f - 0.05f * cool, 1f - 0.02f * cool, 1f + 0.06f * cool, -0.015f * cool));

            Tonemapping tm = _profile.Add<Tonemapping>(true);
            if (mode == "aces")
                tm.mode.Override(TonemappingMode.ACES);
            else if (mode == "none")
                tm.mode.Override(TonemappingMode.None);
            else
                tm.mode.Override(TonemappingMode.Neutral);

            if (_disableFilmGrain.Value)
            {
                FilmGrain grain = _profile.Add<FilmGrain>(true);
                grain.intensity.Override(0f);
            }
            if (_disableDepthOfField.Value)
            {
                DepthOfField dof = _profile.Add<DepthOfField>(true);
                dof.mode.Override(DepthOfFieldMode.Off);
            }
            if (_disableMotionBlur.Value)
            {
                MotionBlur blur = _profile.Add<MotionBlur>(true);
                blur.intensity.Override(0f);
            }
            if (_neutralizeSplitToning.Value)
            {
                SplitToning split = _profile.Add<SplitToning>(true);
                split.shadows.Override(new Color(0.5f, 0.5f, 0.5f, 1f));
                split.highlights.Override(new Color(0.5f, 0.5f, 0.5f, 1f));
                split.balance.Override(0f);
            }

            _volume.sharedProfile = _profile;
            _volume.enabled = _gradingEnabled.Value;

            EnsureCameraPostProcessing();
            Logger.LogInfo("GRADING applied: temp=" + _temperature.Value + " exposure=" + _postExposure.Value +
                " contrast=" + _contrast.Value + " shadowCool=" + cool + " tonemap=" + mode);
        }

        private void RestoreColorGradingMode()
        {
            if (!_colorGradingModeCaptured)
                return;
            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
                asset.colorGradingMode = _originalColorGradingMode;
            _colorGradingModeCaptured = false;
        }

        private void EnsureCameraPostProcessing()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;
            if (_worldCamera == null && !IsProfilePhotoCamera(camera))
                _worldCamera = camera;
            UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
            if (data == null)
                return;
            ApplyShadowAntialiasing(camera, data);
            if (!data.renderPostProcessing)
            {
                Logger.LogInfo("Camera post-processing was off; enabling it for the grade.");
                data.renderPostProcessing = true;
            }

            // The camera only evaluates volumes on layers inside its volume mask;
            // make sure ours lives on one of them.
            int mask = data.volumeLayerMask.value;
            if (_volumeObject != null && (mask & (1 << _volumeObject.layer)) == 0)
            {
                bool moved = false;
                for (int layer = 0; layer < 32; layer++)
                {
                    if ((mask & (1 << layer)) != 0)
                    {
                        _volumeObject.layer = layer;
                        Logger.LogInfo("Volume moved to layer " + layer + " (" + LayerMask.LayerToName(layer) +
                            ") to match the camera volume mask.");
                        moved = true;
                        break;
                    }
                }
                if (!moved)
                    Logger.LogWarning("Camera volume mask is empty; no volume can apply to this camera.");
            }
        }

        private void ApplyShadowAntialiasing(Camera camera, UniversalAdditionalCameraData data)
        {
            if (!_stabilizeShadowEdges.Value || !_adjustSun.Value)
            {
                RestoreShadowAntialiasing();
                return;
            }
            if (_shadowAaCaptured && _shadowAaCamera != camera)
                RestoreShadowAntialiasing();
            if (!_shadowAaCaptured)
            {
                _shadowAaCamera = camera;
                _originalAntialiasing = data.antialiasing;
                _originalAntialiasingQuality = data.antialiasingQuality;
                _shadowAaCaptured = true;
            }
            // TAA depends on correct motion vectors. The game's separate transparent outline passes do not
            // provide them, so history blending makes avatar outlines fade whenever the character moves.
            // SMAA remains fully spatial: no ghost history, no transparent-outline loss.
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
        }

        private void RestoreShadowAntialiasing()
        {
            if (!_shadowAaCaptured)
                return;
            if (_shadowAaCamera != null)
            {
                UniversalAdditionalCameraData data =
                    _shadowAaCamera.GetComponent<UniversalAdditionalCameraData>();
                if (data != null)
                {
                    data.antialiasing = _originalAntialiasing;
                    data.antialiasingQuality = _originalAntialiasingQuality;
                }
            }
            _shadowAaCamera = null;
            _shadowAaCaptured = false;
        }

        // ---------------------------------------------------------------
        // Sun: the game parks it at a permanent overhead noon; tilt it
        // ---------------------------------------------------------------
        private void ApplySun()
        {
            if (!_adjustSun.Value)
            {
                RestoreSun();
                return;
            }

            Light sun = RenderSettings.sun != null ? RenderSettings.sun : FindSunLight();
            if (sun == null)
                return;
            if (_sunLight != sun)
            {
                _sunLight = sun;
                _sunCaptured = false;
            }
            if (!_sunCaptured)
            {
                _origSunRotation = sun.transform.rotation;
                _origSunColor = sun.color;
                _origSunIntensity = sun.intensity;
                _origSunShadowStrength = sun.shadowStrength;
                _origSunShadows = sun.shadows;
                _sunCaptured = true;
            }

            if (sun.shadows != LightShadows.None)
                sun.shadows = _hardShadows.Value ? LightShadows.Hard : LightShadows.Soft;

            // The day cycle drives rotation/color/intensity per frame when active.
            if (!_dayCycleEnabled.Value)
            {
                sun.transform.rotation = Quaternion.Euler(
                    Mathf.Clamp(_sunElevation.Value, 5f, 90f), Mathf.Repeat(_sunAzimuth.Value, 360f), 0f);
                sun.color = Color.Lerp(_origSunColor, Color.white, Mathf.Clamp01(_sunNeutralize.Value));
                sun.intensity = _origSunIntensity * Mathf.Clamp(_sunIntensity.Value, 0.3f, 2f);
            }
            sun.shadowStrength = Mathf.Clamp01(_sunShadowStrength.Value);

            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
            {
                // Do not silently truncate the configured range. The old 100-unit ceiling meant the
                // 120-unit default could never take effect, and the large Circle_Hill casters dropped
                // out of the directional shadow map while they were still plainly visible on screen.
                float targetDistance = Mathf.Clamp(_shadowDistance.Value, 10f, 300f);
                asset.shadowDistance = targetDistance;
                int targetResolution = Mathf.Clamp(_shadowMapResolution.Value, 1024, 8192);
                int targetCascades = Mathf.Clamp(_shadowCascadeCount.Value, 1, 4);
                bool resolutionSet = TrySetPipelineMember(asset, "mainLightShadowmapResolution",
                    "m_MainLightShadowmapResolution", targetResolution);
                bool cascadesSet = TrySetPipelineMember(asset, "shadowCascadeCount",
                    "m_ShadowCascadeCount", targetCascades);
                bool softSet = _hardShadows.Value || TrySetPipelineMember(asset, "supportsSoftShadows",
                    "m_SoftShadowsSupported", true);
                bool pipelineSoftQualitySet = _hardShadows.Value || TrySetPipelineMember(asset,
                    "softShadowQuality", "m_SoftShadowQuality", "High");
                bool perLightSoftSet = true;
                if (!_hardShadows.Value)
                {
                    UniversalAdditionalLightData lightData = sun.GetComponent<UniversalAdditionalLightData>();
                    perLightSoftSet = lightData == null || TrySetPipelineMember(lightData,
                        "softShadowQuality", "m_SoftShadowQuality", "High");
                }
                Logger.LogInfo("SHADOW QUALITY distance=" + targetDistance + " resolution=" +
                    targetResolution + "(" + resolutionSet + ") cascades=" + targetCascades +
                    "(" + cascadesSet + ") soft=" + (!_hardShadows.Value) + "(" + softSet +
                    ") pipelineHigh=" + pipelineSoftQualitySet + " perLightHigh=" + perLightSoftSet);
            }

            if (_skyApplied)
                DynamicGI.UpdateEnvironment();
            Logger.LogInfo("SUN adjusted: elevation=" + _sunElevation.Value + " azimuth=" + _sunAzimuth.Value +
                " shadowStrength=" + sun.shadowStrength + " continuousDirection=true temporalStability=" +
                _stabilizeShadowEdges.Value + " originalEuler=" + _origSunRotation.eulerAngles);
        }

        private static bool TrySetPipelineMember(object target, string propertyName, string fieldName, object value)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                PropertyInfo property = target.GetType().GetProperty(propertyName, flags);
                if (property != null)
                {
                    MethodInfo setter = property.GetSetMethod(true);
                    if (setter != null)
                    {
                        setter.Invoke(target, new object[] { ConvertMemberValue(property.PropertyType, value) });
                        return true;
                    }
                }
                FieldInfo field = target.GetType().GetField(fieldName, flags);
                if (field != null)
                {
                    field.SetValue(target, ConvertMemberValue(field.FieldType, value));
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private static object ConvertMemberValue(Type targetType, object value)
        {
            if (targetType.IsEnum)
            {
                string text = value as string;
                if (text != null)
                    return Enum.Parse(targetType, text, true);
                return Enum.ToObject(targetType, value);
            }
            if (value != null && !targetType.IsInstanceOfType(value))
                return Convert.ChangeType(value, targetType);
            return value;
        }

        // ---------------------------------------------------------------
        // Day/night cycle synced to the real clock
        // ---------------------------------------------------------------
        private void UpdateDayCycle()
        {
            if (!_dayCycleEnabled.Value || !_adjustSun.Value || !_sunCaptured || _sunLight == null)
                return;

            // A five-minute cycle moves only 0.06 degrees in 50 ms. Updating the full water,
            // decor, sky, star and shadow-material graph at 20 Hz is visually continuous while
            // removing hundreds of redundant material writes from most rendered frames.
            float updateTime = Time.unscaledTime;
            if (updateTime < _nextDayCycleUpdate)
                return;
            _nextDayCycleUpdate = updateTime + 0.05f;

            DateTime now = DateTime.Now;
            double cycleMinutes = Mathf.Clamp(_cycleMinutes.Value, 5f, 1440f);
            double cycle = cycleMinutes * 60.0;
            // Short cycles retain the existing per-hour noon anchor. At a full 24-hour
            // cycle, anchor noon to real 12:00 so sunrise, noon, sunset and midnight align
            // with the local clock instead of repeating inside each hour.
            double secondsIntoDay = now.TimeOfDay.TotalSeconds;
            double noonAnchor = cycleMinutes >= 720f ? 12.0 * 60.0 * 60.0 : _noonMinute.Value * 60.0;
            double sinceNoon = secondsIntoDay - noonAnchor;
            float phase = (float)(((sinceNoon % cycle) + cycle) % cycle / cycle); // 0 = noon, 0.5 = midnight
            // Testing aid: pin the cycle to one time of day. 12:00 is phase 0, 00:00 is phase 0.5.
            if (_freezeTime.Value)
                phase = Mathf.Repeat((Mathf.Clamp(_frozenHour.Value, 0f, 24f) - 12f) / 24f, 1f);
            _lastCyclePhase = phase;

            float maxElevation = Mathf.Clamp(_maxElevation.Value, 20f, 80f);
            float signedElevation = maxElevation * Mathf.Cos(phase * 2f * Mathf.PI);
            // Smooth |elevation| with a floor so the light never grazes the horizon exactly.
            float elevation = Mathf.Sqrt(signedElevation * signedElevation + 36f);
            float azimuth = Mathf.Repeat(_sunAzimuth.Value + phase * 360f, 360f);
            // 1 in daylight, 0 at night, with a wide twilight so sunsets are visible.
            float dayWeight = Mathf.Clamp01((signedElevation + 8f) / 16f);
            _lastDayWeight = dayWeight;
            _lastSignedElevation = signedElevation;
            _lastSunAzimuth = azimuth;
            _celestialValid = true;
            UpdateMoonPhaseState(false);
            UpdateTwilightVariant();

            // Restore the pre-0.17 sun/moon handoff. The later three-stage weighting tied
            // both the visible moon and the whole directional shadow map to the same delayed
            // moon weight, which caused the observed early-night shadow outage. Keep one
            // continuously active directional light and only soften its shadows at the flip.
            bool nightSide = signedElevation < 0f;
            float lightAzimuth = Mathf.Repeat(nightSide ? azimuth + 180f : azimuth, 360f);
            // Keep the sun fully continuous. Temporal edge stabilization is handled by the camera rather
            // than by stepping the light direction, so short day cycles never look like stop motion.
            _sunLight.transform.rotation = Quaternion.Euler(elevation, lightAzimuth, 0f);
            float handoffFade = Mathf.Clamp01(Mathf.Abs(signedElevation) / 5f);
            _sunLight.shadowStrength = Mathf.Clamp01(_sunShadowStrength.Value) *
                Mathf.Lerp(0.15f, 1f, handoffFade);
            UpdateSurfaceToonThreshold(elevation);

            Color dayColor = Color.Lerp(_origSunColor, Color.white, Mathf.Clamp01(_sunNeutralize.Value));
            float horizonWarmth = 1f - Mathf.Clamp01(signedElevation / 35f);
            // 1 while the sun actually crosses the horizon, 0 above ~14 degrees or at deep night.
            float twilightGlow = TwilightGlowCurve(signedElevation);
            // Keep the single sun mostly white, adding only a restrained dawn/dusk tint and lowering
            // its output near the horizon before it reaches full daylight strength.
            dayColor = Color.Lerp(dayColor, _twilightSunColor, twilightGlow * 0.28f);
            float lowSunBrightness = Mathf.Lerp(0.72f, 1f,
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-2f, 18f, signedElevation)));
            dayColor *= lowSunBrightness;
            dayColor.a = 1f;
            Color moonColor = new Color(0.72f, 0.79f, 0.95f, 1f);
            _sunLight.color = Color.Lerp(moonColor, dayColor, dayWeight);
            _sunLight.color = Color.Lerp(_sunLight.color, new Color(0.76f, 0.82f, 0.90f, 1f),
                _rainBlend * 0.58f);

            float nightLevel = Mathf.Clamp01(_nightBrightness.Value);
            float lunarLight = _moonEnabled.Value
                ? Mathf.Lerp(Mathf.Clamp01(_newMoonBrightness.Value), 1f,
                    Mathf.Sqrt(Mathf.Clamp01(_moonIllumination)))
                : 1f;
            float weatherDirectional = Mathf.Lerp(1f,
                Mathf.Clamp(_rainLightMultiplier.Value, 0.2f, 1f), _rainBlend);
            float minimumNightLight = Mathf.Clamp(_minimumNightLight.Value, 0.15f, 0.7f);
            float nightDirectionalIntensity = Mathf.Max(
                nightLevel * lunarLight * weatherDirectional, minimumNightLight);
            float directionalIntensity = Mathf.Lerp(nightDirectionalIntensity,
                weatherDirectional, dayWeight);
            _sunLight.intensity = _origSunIntensity * Mathf.Clamp(_sunIntensity.Value, 0.3f, 2f) *
                directionalIntensity *
                (1f + _lightningFlash * 1.6f);

            // Post-level night dip: darkens and cools EVERYTHING, including unlit
            // water/decal materials that ignore the sun entirely.
            if (_colorAdjustments != null)
            {
                // Hold the night dip back while the sun is crossing. dayWeight is already at zero
                // eight degrees down, so the full night darkening used to land while the sky was
                // still burning and took the afterglow with it.
                float dip = _nightExposureDip.Value * (1f - dayWeight) *
                    Mathf.Lerp(1f, 0.40f, twilightGlow) +
                    _rainExposureDip.Value * _rainBlend;
                _colorAdjustments.postExposure.Override(_postExposure.Value + dip);
                Color filter = Color.Lerp(new Color(0.88f, 0.91f, 1f, 1f),
                    _dayColorFilter.Value, dayWeight);
                // Pull the whole frame toward the twilight color while the sun crosses the
                // horizon, so the island itself turns golden/pink/crimson, not just the sky.
                // At 0.45 the filter barely moved off white even with the slider at maximum;
                // 0.75 gives the slider a range that actually reads on the ground and buildings.
                Color gentleTwilightFilter = Color.Lerp(Color.white, _twilightGlowColor, 0.75f);
                // LerpUnclamped: Color.Lerp saturates t at 1, so the tint slider hit a hard
                // ceiling at its own maximum however far it was pushed.
                Color twilightFilter = Color.LerpUnclamped(Color.white, gentleTwilightFilter,
                    Mathf.Clamp(_twilightWorldTint.Value, 0f, 4f) * twilightGlow);
                twilightFilter.r = Mathf.Max(0f, twilightFilter.r);
                twilightFilter.g = Mathf.Max(0f, twilightFilter.g);
                twilightFilter.b = Mathf.Max(0f, twilightFilter.b);
                filter *= twilightFilter;
                filter *= Color.Lerp(Color.white, new Color(0.88f, 0.91f, 0.97f, 1f), _rainBlend);
                // Autumn's own warm cast, faded out with the daylight so nights stay cool and
                // the twilight tint above is not fighting an amber wash.
                filter *= GetSeasonColorFilter(dayWeight);
                _colorAdjustments.colorFilter.Override(filter);
            }

            if (_skyMaterial != null && RenderSettings.skybox == _skyMaterial)
            {
                // The glow band brightens the sun side of the horizon; pulling overall sky
                // exposure down a little during the crossing keeps the opposite side dimmer,
                // which is what sells the directional sunset.
                _skyMaterial.SetFloat("_Exposure",
                    Mathf.Clamp(_skyExposure.Value, 0.05f, 8f) * Mathf.Lerp(0.5f, 1f, dayWeight) *
                    (1f - 0.18f * twilightGlow) * Mathf.Lerp(1f, 0.62f, _rainBlend));
                // Thicker atmosphere near sunrise/sunset paints the sky orange, but the
                // procedural sky turns green past ~1.5 thickness, so keep the boost modest
                // and warm the scattering tint instead. Rain flattens the atmosphere so the
                // whole dome reads as one overcast gray instead of a blue gradient.
                float sunsetBoost = 0.30f * horizonWarmth * dayWeight;
                _skyMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(
                    Mathf.Clamp(_atmosphereThickness.Value + sunsetBoost, 0.1f, 3f),
                    0.55f, _rainBlend));
                // Keyed off twilightGlow as well as the daylight-weighted horizon warmth: the old
                // form peaked at 0.35 with the sun exactly on the horizon, so the dome itself
                // barely moved and all the colour had to come from the glow band. The elevation
                // term pulls the warmth back out once the sun is well down, or the whole dome
                // would still read brown while the stars come up.
                float skyWarmth = Mathf.Max(horizonWarmth * dayWeight,
                    twilightGlow * Mathf.Clamp01((signedElevation + 12f) / 14f));
                Color skyTwilightTint = _twilightSkyTint;
                // The procedural sky's own scattering is blue. A tint whose green sits high
                // relative to its red multiplies into that blue and comes out green along the
                // horizon - which is what turned every sunrise's horizon dark green, since the
                // dawn tint is the yellowest of the four. Cap green against red before it
                // reaches the skybox; the glow band stays yellow, and it is additive geometry
                // with no scattering to interact with, so dawn still reads yellow.
                float greenCeiling = skyTwilightTint.r * 0.72f;
                if (skyTwilightTint.g > greenCeiling)
                    skyTwilightTint.g = greenCeiling;
                _skyMaterial.SetColor("_SkyTint", Color.Lerp(_skyTint.Value,
                    skyTwilightTint, skyWarmth * 0.9f));
                _skyMaterial.SetColor("_SkyTint", Color.Lerp(_skyMaterial.GetColor("_SkyTint"),
                    new Color(0.36f, 0.41f, 0.49f, 1f), _rainBlend));
                _skyMaterial.SetColor("_GroundColor",
                    _groundColor.Value * Mathf.Lerp(0.45f, 1f, dayWeight));
            }

            // The water surface brightness is dominated by the skybox reflection, so
            // fade global reflections with the night alongside the material colors.
            // The old -12..20 window put the sea at roughly a third of its daytime level while the
            // sun was still exactly on the horizon and the sky was at its most colorful. Track the
            // sun's own -8..8 daylight window instead, so sea and sky darken together.
            float waterDayWeight = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(-10f, 10f, signedElevation));
            if (_waterDaylight.Value && _ambientCaptured)
                RenderSettings.reflectionIntensity = _origReflectionIntensity * Mathf.Lerp(0.20f, 1f, waterDayWeight) *
                    Mathf.Lerp(1f, 0.72f, _rainBlend);

            // Everything the sea should mirror is already known: this plugin paints the sky.
            // The shadow-receiver overlay reads the same color, so the two layers cannot disagree.
            Color skyMirror = EvaluateSkyMirrorColor(dayWeight, twilightGlow);
            _seaSkyMirrorColor = skyMirror;
            _lastTwilightGlow = twilightGlow;

            // Drive every tracked water body directly. The sea's dawn/dusk color is taken from the
            // sky this plugin is drawing at that moment, not from a generic warm cast: a plain tint
            // over an already darkened blue can only ever produce the grey-brown midpoint.
            if (_waterDaylight.Value)
            {
                float skyMirrorAmount = Mathf.Clamp01(_seaSkyReflection.Value);
                // Grazing sunlight makes water far more mirror-like than an overhead noon sun does,
                // so the sky only takes the surface over during the crossing. At noon it stays a
                // light atmospheric fade over the game's own palette.
                float skyMirrorRamp = Mathf.Lerp(0.25f, 1.35f, twilightGlow);
                float nightWaterBrightness = Mathf.Clamp01(_nightWaterBrightness.Value);
                float waterBrightness = Mathf.Lerp(nightWaterBrightness, 1f, waterDayWeight);
                waterBrightness *= Mathf.Lerp(1f, 0.78f, _rainBlend);
                // Rain should cool the water, not turn it into an almost black mirror.
                waterBrightness = Mathf.Max(waterBrightness,
                    Mathf.Lerp(0.24f, 0.46f, waterDayWeight) * _rainBlend);
                // Hold the sea near daylight luminance for as long as the sky is burning. Without
                // this floor the horizon crossing is the darkest the sea gets outside deep night.
                float twilightLift = twilightGlow * Mathf.Lerp(1f, 0.55f, _rainBlend);
                float liftedBrightness = Mathf.Max(waterBrightness, Mathf.Lerp(waterBrightness,
                    Mathf.Clamp(_twilightSeaBrightness.Value, 0f, 2f), twilightLift));
                // Only the grazing horizon band takes the full lift. Raising deep water with it
                // turns the whole surface into flat, milky paint instead of water with depth.
                float bodyBrightness = Mathf.Lerp(waterBrightness, liftedBrightness, 0.45f);
                Color waterTint = Color.Lerp(new Color(0.64f, 0.74f, 0.96f, 1f), Color.white, waterDayWeight);
                // The night-side cool cast fights the sunset directly; retire it while the sun crosses.
                waterTint = Color.Lerp(waterTint, Color.white, twilightGlow * 0.75f);
                waterTint *= Color.Lerp(Color.white, new Color(0.56f, 0.66f, 0.79f, 1f), _rainBlend);
                float waterTwilight = Mathf.Clamp(_twilightWorldTint.Value, 0f, 4f) * twilightGlow;
                // Deep water at dusk is blue, not warm. A real sea only turns orange where it
                // mirrors the sky at a grazing angle; carrying the sunset hue into the body is
                // what reads as flat purple paint. Lean a saturated navy a little toward the
                // evening's hue - through HSV, so the target itself never desaturates - and hand
                // the warm end to the horizon band alone.
                Color seaBodyTarget = BlendThroughBlue(new Color(0.05f, 0.10f, 0.38f, 1f),
                    skyMirror, 0.14f);
                float horizonMirror = Mathf.Lerp(0.30f, 0.97f,
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(twilightGlow * 2.2f))) *
                    Mathf.Lerp(0.70f, 1f, skyMirrorAmount);
                for (int i = 0; i < _waterMaterials.Count; i++)
                {
                    WaterMaterial water = _waterMaterials[i];
                    if (water.Material == null)
                        continue;
                    if (water.HasReflectionStrength)
                    {
                        float reflectionMultiplier = Mathf.Lerp(0.28f, 1f, waterDayWeight) *
                            Mathf.Lerp(1f, 0.72f, _rainBlend);
                        water.Material.SetFloat("_ReflectionStrength",
                            water.OriginalReflectionStrength * reflectionMultiplier);
                    }
                    // A vertical waterfall sheet mirrors far less sky than open water, but it sits
                    // right next to the pond and river it feeds, so it cannot drift off-palette.
                    float surfaceSkyMirror = water.IsOpenWater ? skyMirrorAmount : skyMirrorAmount * 0.72f;
                    Color[] drivenColors = water.DrivenColors;
                    for (int j = 0; j < water.PropertyIds.Length; j++)
                    {
                        Color original = water.Originals[j];
                        bool highlight = IsWaterHighlightProperty(water.PropertyNames[j]);
                        if (highlight)
                            continue;
                        float weight = water.SkyMirrorWeights[j];
                        float propBrightness = Mathf.Lerp(bodyBrightness, liftedBrightness, weight);
                        Color driven = original * waterTint * propBrightness;
                        // Move toward the live sky color rather than tinting at constant luminance.
                        // The cube falls off fast, so only the horizon channel actually chases the
                        // warm sky; shallow and deep water head for the navy body target instead.
                        if (weight > 0.9f)
                        {
                            // The horizon band is where sea meets sky, so it simply becomes the
                            // sky. Mixing halfway is what produced a grey step, so the crossover
                            // is deliberately fast: past a fifth of the twilight it is nearly all
                            // sky, and the only near-neutral moment left is a bright daytime haze.
                            Color target = skyMirror * (0.96f * Mathf.Lerp(0.45f, 1f, propBrightness));
                            driven = Color.Lerp(driven, target, horizonMirror);
                        }
                        else
                        {
                            float mirror = Mathf.Clamp01(surfaceSkyMirror *
                                Mathf.Lerp(0.5f, 1f, weight) * skyMirrorRamp);
                            if (mirror > 0.001f)
                            {
                                // Body colors are dark, where a plain mix does land on grey, so
                                // these rotate their hue through blue toward the navy target.
                                Color target = seaBodyTarget *
                                    (Mathf.Lerp(0.62f, 1f, weight) * Mathf.Lerp(0.45f, 1f, propBrightness));
                                driven = BlendThroughBlue(driven, target, mirror);
                            }
                            else
                            {
                                driven = ApplyLuminancePreservingTint(driven, _twilightGlowColor,
                                    waterTwilight);
                            }
                        }
                        driven.a = original.a;
                        drivenColors[j] = driven;
                        // The shadow-receiver overlay matches this so a lit surface barely
                        // changes; shallow water is the channel most of the sea reads as.
                        if (water.IsOpenWater && weight > 0.3f && weight < 0.9f)
                            _seaDrivenBodyColor = driven;
                    }
                    // Stylized Water 3's foam/intersection inputs are their own rendered color channels,
                    // not ordinary overlays on _BaseColor. Give them a dedicated readable palette instead
                    // of multiplying the (very dark at night) body color.
                    Color nightPattern = _nightWaterPatternColor.Value;
                    Color dayPattern = _dayWaterPatternColor.Value;
                    Color waveColor = Color.Lerp(nightPattern, dayPattern, waterDayWeight);
                    waveColor *= Mathf.Clamp(_waterWaveBrightness.Value, 0.75f, 1.25f);
                    waveColor.r = Mathf.Clamp01(waveColor.r);
                    waveColor.g = Mathf.Clamp01(waveColor.g);
                    waveColor.b = Mathf.Clamp01(waveColor.b);
                    // Foam and wave crests catch the sky more strongly than the body does, but they
                    // must stay the brightest thing on the surface, so mirror a lightened sky.
                    float foamMirror = surfaceSkyMirror * twilightGlow * 0.7f;
                    if (foamMirror > 0.001f)
                        waveColor = Color.Lerp(waveColor, Color.Lerp(skyMirror, Color.white, 0.4f), foamMirror);
                    else
                        waveColor = ApplyLuminancePreservingTint(waveColor, _twilightGlowColor,
                            waterTwilight * 0.35f);
                    for (int j = 0; j < water.PropertyIds.Length; j++)
                    {
                        bool highlight = IsWaterHighlightProperty(water.PropertyNames[j]);
                        Color driven = highlight ? waveColor : drivenColors[j];
                        driven.a = water.Originals[j].a;
                        water.Material.SetColor(water.PropertyIds[j], driven);
                    }
                }
                UpdateWaterParticles(waterDayWeight, waterTwilight);
                UpdateWaterShadowOverlayMaterial(waterDayWeight);
            }

            // Flowers, grass tufts and other light-ignoring decor keep full daytime
            // brightness at night; dim them alongside the water.
            if (_decorDimming.Value && _decorMaterials.Count > 0)
            {
                float decorBrightness = Mathf.Lerp(Mathf.Clamp01(_nightDecorBrightness.Value), 1f, dayWeight);
                Color decorTint = Color.Lerp(new Color(0.80f, 0.85f, 1f, 1f), Color.white, dayWeight);
                for (int i = 0; i < _decorMaterials.Count; i++)
                {
                    WaterMaterial decor = _decorMaterials[i];
                    if (decor.Material == null)
                        continue;
                    for (int j = 0; j < decor.PropertyIds.Length; j++)
                    {
                        Color original = decor.Originals[j];
                        Color driven = original * decorTint * decorBrightness;
                        driven.a = original.a;
                        decor.Material.SetColor(decor.PropertyIds[j], driven);
                    }
                }
            }

            // Ambient light samples the skybox; refresh it occasionally as the sky changes.
            // During the horizon crossing the sky moves fast, so refresh more often there -
            // this is also what carries the sunset colors into the water reflection.
            if (Time.unscaledTime >= _nextGiUpdate)
            {
                // The dusk sky moves fast and the ambient probe is what carries its color onto
                // everything the sun does not reach, so refresh it harder while it matters.
                _nextGiUpdate = Time.unscaledTime + (twilightGlow > 0.2f ? 2f : 10f);
                DynamicGI.UpdateEnvironment();
            }

            UpdateSunsetGlow(signedElevation, dayWeight);
            UpdateSunGlitter(signedElevation, twilightGlow);
            UpdateSunVisual(signedElevation);
            UpdateStars(dayWeight, phase);
        }

        // The affected objects are not rendered through their scene Renderers.
        // The game disables those Renderers, then submits the mesh batches through
        // Graphics.RenderMeshInstanced. RenderParams defaults to ShadowCastingMode.Off.
        private void ApplyShadowCasting()
        {
            if (!_adjustSun.Value || !_forceShadowCasting.Value)
                return;

            int hills = SetInstancedShadowCasting<CircleMountainInstancing>("_renderParams", "_renderParamsTop");
            int trunks = _restoreTreeShadows.Value
                ? SetInstancedShadowCasting<TrunkInstancing>("_renderParams")
                : 0;
            int streetLamps = SetInstancedShadowCasting<StreetLampInstancing>(
                "_renderParams", "_renderParams1", "_renderParams2");
            Logger.LogInfo("SHADOW INSTANCED casters circleMountainParams=" + hills +
                " trunkParams=" + trunks + " streetLampParams=" + streetLamps);
        }

        private int SetInstancedShadowCasting<T>(params string[] fieldNames) where T : Component
        {
            T[] batches = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            int patched = 0;
            for (int batchIndex = 0; batchIndex < batches.Length; batchIndex++)
            {
                T batch = batches[batchIndex];
                if (batch == null)
                    continue;

                Type type = batch.GetType();
                for (int fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
                {
                    FieldInfo field = type.GetField(fieldNames[fieldIndex],
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field == null || field.FieldType != typeof(RenderParams))
                    {
                        Logger.LogWarning("SHADOW INSTANCED missing RenderParams " +
                            type.FullName + "." + fieldNames[fieldIndex]);
                        continue;
                    }

                    RenderParams parameters = (RenderParams)field.GetValue(batch);
                    if (parameters.shadowCastingMode != ShadowCastingMode.On)
                    {
                        parameters.shadowCastingMode = ShadowCastingMode.On;
                        field.SetValue(batch, parameters);
                        patched++;
                    }
                }
            }
            return patched;
        }

        private static bool IsBasketballCourtRenderer(Renderer renderer)
        {
            if (renderer == null || NormalizeSceneName(renderer.gameObject.name) != "mdbasketfield")
                return false;
            Transform current = renderer.transform.parent;
            while (current != null)
            {
                if (NormalizeSceneName(current.gameObject.name).IndexOf("prbasketfield") >= 0)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool IsBasketballCourtMaterial(Material material)
        {
            return material != null && NormalizeSceneName(material.name).StartsWith("basketcourt");
        }

        private static bool HasStructuralIdentity(Renderer renderer)
        {
            Transform current = renderer.transform;
            while (current != null)
            {
                string name = NormalizeSceneName(current.gameObject.name);
                if (ContainsAny(name, "building", "school", "pillar", "column", "tower", "library",
                    "canteen", "shop", "stadium", "basketfield", "lighthouse", "wall", "roof",
                    "bridge", "dock", "arch", "cliff", "mountain", "rock", "house", "playtower",
                    "circlehill", "waterfallhill"))
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool ContainsAny(string value, params string[] fragments)
        {
            for (int i = 0; i < fragments.Length; i++)
            {
                if (value.IndexOf(fragments[i], StringComparison.Ordinal) >= 0)
                    return true;
            }
            return false;
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null)
                return "<null>";
            string path = transform.gameObject.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.gameObject.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static string GetMaterialSummary(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            string summary = "";
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (i > 0)
                    summary += ",";
                summary += material == null ? "<null>" : material.name + "[" +
                    (material.shader != null ? material.shader.name : "no-shader") + "]";
            }
            return summary;
        }

        private void CaptureShadowState(Renderer renderer)
        {
            int id = renderer.GetInstanceID();
            if (_shadowRendererStates.ContainsKey(id))
                return;
            ShadowRendererState state = new ShadowRendererState();
            state.Renderer = renderer;
            state.OriginalMode = renderer.shadowCastingMode;
            state.OriginalReceiveShadows = renderer.receiveShadows;
            _shadowRendererStates.Add(id, state);
        }

        private void ApplyPlayerShadows()
        {
            PlayerCustomizationController[] players = UnityEngine.Object.FindObjectsByType<PlayerCustomizationController>(
                FindObjectsSortMode.None);
            int rendererCount = 0;
            int castEnabled = 0;
            int receiveEnabled = 0;
            int toonMaterialsApplied = 0;
            float shade = Mathf.Clamp(_playerShadeDarkness.Value, 0.3f, 1f);
            bool writePlayerDiagnostics = !_playerDiagnosticsWritten && players.Length > 0;

            for (int p = 0; p < players.Length; p++)
            {
                PlayerCustomizationController player = players[p];
                if (player == null)
                    continue;
                // Customization prefabs keep dozens of unused renderers under every player. Touch only
                // active choices; newly selected parts are picked up by the next incremental pass.
                SkinnedMeshRenderer[] playerRenderers = player.GetComponentsInChildren<SkinnedMeshRenderer>(false);
                for (int r = 0; r < playerRenderers.Length; r++)
                {
                    SkinnedMeshRenderer renderer = playerRenderers[r];
                    if (renderer == null)
                        continue;
                    rendererCount++;
                    if (renderer.shadowCastingMode != ShadowCastingMode.On)
                    {
                        CaptureShadowState(renderer);
                        renderer.shadowCastingMode = ShadowCastingMode.On;
                        castEnabled++;
                    }
                    if (!renderer.receiveShadows)
                    {
                        CaptureShadowState(renderer);
                        renderer.receiveShadows = true;
                        receiveEnabled++;
                    }

                    if (writePlayerDiagnostics)
                        Logger.LogInfo("PLAYER MAT " + renderer.name +
                            " cast=" + renderer.shadowCastingMode +
                            " receive=" + renderer.receiveShadows + " -> " +
                            GetPlayerMaterialShadowSummary(renderer));
                }
                ScanPlayerFaceMaterials(player);
            }

            if (players.Length > 0)
                _playerDiagnosticsWritten = true;
            if (castEnabled > 0 || receiveEnabled > 0 || writePlayerDiagnostics ||
                players.Length > 0 && rendererCount == 0)
            {
                Logger.LogInfo("PLAYER SHADOWS players=" + players.Length + " renderers=" + rendererCount +
                    " castOn=" + castEnabled + " receiveOn=" + receiveEnabled +
                    " toonMaterials=" + toonMaterialsApplied);
            }
        }

        // The avatar customization system fills renderer material slots well after spawn and can
        // swap them again at any time. Only capture materials that already expose safe channels.
        private void ScanPlayerFaceMaterials(PlayerCustomizationController player)
        {
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>(false);
            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer renderer = renderers[r];
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                    continue;
                // The player's own desk lamp carries a "LookCare Halo" glow quad. Capturing
                // LookCare's own Sprites/Default materials here handed the halo tint to the
                // night dimmer, which turned the lamp glow into a dark disc.
                if (IsLookCareTransform(renderer.transform))
                    continue;
                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material == null || material.shader == null)
                        continue;
                    ApplyPlayerSystemShadows(material);
                    TryCapturePlayerNightMaterial(material);
                }
            }
        }

        // Avatar Toon materials are authored with system shadows off, so a body lit at night
        // reads as one flat block with no cast shadow on it at all. This flips only that one
        // switch: rewriting the shade colours here is what previously turned skin grey, so
        // the shade bands are left exactly as authored.
        private void ApplyPlayerSystemShadows(Material material)
        {
            if (!_toonShading.Value || IsNativeFeatureInstance(material))
                return;
            ToonMaterial entry = TrackToonMaterial(material);
            if (entry == null || entry.Material == null || !entry.HadSystemShadows)
                return;
            // The shade band on avatars comes from the same rule the global toon pass uses:
            // the shade colour is derived from the material's own base colour, which is what
            // keeps shirt, shorts and skin colours inside the shadow. Writing the authored
            // value back instead - it is black, meant for a separate shade map these materials
            // do not use - removed the band completely.
            // Reapplied on every pass rather than once, so the avatar sliders take effect live.
            ApplyPlayerToonMaterial(entry, Mathf.Clamp(_playerShadeDarkness.Value, 0.3f, 1f));
        }

        // The avatar face overlays are unlit on both of their shaders, so the sun and the
        // room lamps never reach them. This pass gives every overlay slot its own material
        // on the game's own transparent shader graph and drives that material's _BaseColor
        // from the light actually arriving at the head, so the overlays brighten under a
        // lamp and darken in the dark exactly like the rest of the avatar.
        private void RefreshNativeFeatureMaterials()
        {
            float now = Time.unscaledTime;
            if (now >= _nextFaceRendererScan)
            {
                _nextFaceRendererScan = now + 1f;
                RebuildFaceRendererList();
            }

            // Checked often, because the customization system reassigns the eye slot on every
            // blink and a once-per-second check left the original colour visible in between.
            // Not every frame though: the customization screen rewrites its preview materials
            // continuously, and swapping in lockstep with it made that preview strobe.
            if (now >= _nextFaceAssignCheck)
            {
                _nextFaceAssignCheck = now + 0.08f;
                for (int i = 0; i < _faceRenderers.Count; i++)
                {
                    Renderer renderer = _faceRenderers[i];
                    if (renderer != null)
                        AssignNativeFeatureMaterials(renderer);
                }
            }

            if (now >= _nextFaceBrightnessUpdate)
            {
                _nextFaceBrightnessUpdate = now + 0.1f;
                UpdateNativeFeatureBrightness();
            }

            if (now >= _nextFaceStatusLog)
            {
                _nextFaceStatusLog = now + 10f;
                float sample = ComputeFaceBrightness(_faceRenderers.Count > 0 && _faceRenderers[0] != null
                    ? _faceRenderers[0].bounds.center : Vector3.zero);
                Logger.LogInfo("FACE LIGHT status faceRenderers=" + _faceRenderers.Count +
                    " tracked=" + _nativeFeatureRenderers.Count +
                    " dayWeight=" + _lastDayWeight.ToString("F2") +
                    " lamps=" + _lampLights.Count +
                    " brightness=" + sample.ToString("F2"));
            }
        }

        private void RebuildFaceRendererList()
        {
            _faceLightSources.Clear();
            Light[] sceneLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < sceneLights.Length; i++)
            {
                Light light = sceneLights[i];
                if (light != null && light.type != LightType.Directional && light.range > 0.01f)
                    _faceLightSources.Add(light);
            }

            _faceRenderers.Clear();
            // Searched by material rather than by hierarchy. PlayerCustomizationController sits
            // on the customization preview model, so scanning its children only ever reached the
            // avatar in the editor screen and never the characters walking around the world.
            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer renderer = renderers[r];
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                    continue;
                // The desk lamp carries a LookCare glow quad; that is not a face overlay.
                if (IsLookCareTransform(renderer.transform))
                    continue;
                // The customization and loading previews rebuild their model continuously, so
                // taking their material slots over produced a strobe no back-off could settle.
                // They also run under their own lighting, where a night tint means nothing.
                Transform root = renderer.transform.root;
                if (root != null && root.name.IndexOf("Customization", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                Material[] materials = renderer.sharedMaterials;
                // Avatars are the scene's skinned meshes. The shadow-band switches were reaching
                // the preview model only, for the same hierarchy reason, so bodies in the world
                // stayed flat.
                bool isAvatar = renderer is SkinnedMeshRenderer;
                bool isFace = false;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material == null)
                        continue;
                    if (!isFace && (IsNativeFeatureInstance(material) || IsNativeFeatureSource(material)))
                    {
                        _faceRenderers.Add(renderer);
                        isFace = true;
                    }
                    if (isAvatar)
                        ApplyPlayerSystemShadows(material);
                }
                if (isAvatar)
                {
                    if (renderer.shadowCastingMode != ShadowCastingMode.On)
                    {
                        CaptureShadowState(renderer);
                        renderer.shadowCastingMode = ShadowCastingMode.On;
                    }
                    if (!renderer.receiveShadows)
                    {
                        CaptureShadowState(renderer);
                        renderer.receiveShadows = true;
                    }
                }
            }

            _nativeFeatureCleanupIds.Clear();
            foreach (KeyValuePair<int, NativeFeatureRenderer> pair in _nativeFeatureRenderers)
            {
                if (pair.Value == null || pair.Value.Renderer == null)
                    _nativeFeatureCleanupIds.Add(pair.Key);
            }
            for (int i = 0; i < _nativeFeatureCleanupIds.Count; i++)
            {
                NativeFeatureRenderer state;
                if (_nativeFeatureRenderers.TryGetValue(_nativeFeatureCleanupIds[i], out state))
                    ReleaseNativeFeatureMaterials(state, false);
                _nativeFeatureRenderers.Remove(_nativeFeatureCleanupIds[i]);
            }
        }

        private void AssignNativeFeatureMaterials(Renderer renderer)
        {
            int rendererId = renderer.GetInstanceID();
            NativeFeatureRenderer state;
            if (_nativeFeatureRenderers.TryGetValue(rendererId, out state))
            {
                if (state.Abandoned)
                    return;
                if (NativeFeatureAssignmentIsCurrent(state))
                {
                    // The swap stuck, so nothing is competing for these slots.
                    state.ConsecutiveReassigns = 0;
                    return;
                }
            }
            else
            {
                state = new NativeFeatureRenderer();
                state.Renderer = renderer;
            }

            Material[] sourceMaterials = renderer.sharedMaterials;
            Material[] assigned = (Material[])sourceMaterials.Clone();
            state.Materials.Clear();
            bool changed = false;
            for (int slot = 0; slot < sourceMaterials.Length; slot++)
            {
                Material source = sourceMaterials[slot];
                // Our own instances come back through this loop whenever another slot changes.
                // Re-cloning them compounded the tint and leaked one material per blink.
                if (IsNativeFeatureInstance(source))
                {
                    NativeFeatureMaterial kept = new NativeFeatureMaterial();
                    kept.Slot = slot;
                    kept.Original = source;
                    kept.Instance = source;
                    FaceInstanceInfo keptInfo;
                    if (state.ByInstance.TryGetValue(source.GetInstanceID(), out keptInfo) && keptInfo != null)
                    {
                        kept.Info = keptInfo;
                        kept.BaseColor = keptInfo.BaseColor;
                        kept.TintProperty = keptInfo.TintProperty;
                    }
                    else
                    {
                        kept.BaseColor = Color.white;
                        kept.TintProperty = BaseColorPropertyId;
                    }
                    state.Materials.Add(kept);
                    continue;
                }
                if (!IsNativeFeatureSource(source))
                    continue;

                FaceInstanceInfo info;
                Material instance = GetFaceInstance(state, source, out info);
                if (instance == null || info == null)
                    continue;
                assigned[slot] = instance;
                changed = true;
                NativeFeatureMaterial entry = new NativeFeatureMaterial();
                entry.Slot = slot;
                entry.Original = source;
                entry.Instance = instance;
                entry.Info = info;
                entry.BaseColor = info.BaseColor;
                entry.TintProperty = info.TintProperty;
                state.Materials.Add(entry);
            }

            if (state.Materials.Count == 0)
            {
                if (_nativeFeatureRenderers.ContainsKey(rendererId))
                    _nativeFeatureRenderers.Remove(rendererId);
                return;
            }

            state.Assigned = assigned;
            if (changed)
            {
                state.ConsecutiveReassigns++;
                // A blink or an outfit change needs one swap, after which the check passes again
                // and this counter resets. Something that writes the original slots straight back
                // fails the check on every single pass instead. The old handling - eight swaps,
                // eight seconds off, repeat - turned that standoff into a permanent strobe on the
                // face, which is what the fixed map NPCs were doing. Stand down for good instead,
                // and say so once. Their overlays then stay at their authored brightness, which
                // is wrong at night but is not a flicker.
                if (state.ConsecutiveReassigns > 25)
                {
                    state.Abandoned = true;
                    Logger.LogWarning("FACE LIGHT stood down on " + renderer.name +
                        ": its face materials keep being restored by something else. Left as " +
                        "authored so they cannot flicker.");
                    return;
                }
                renderer.sharedMaterials = assigned;
            }
            if (!_nativeFeatureRenderers.ContainsKey(rendererId))
                _nativeFeatureRenderers.Add(rendererId, state);
            // A freshly swapped slot must not show a frame of the undimmed original.
            state.Brightness = -1f;
            state.SmoothedBrightness = ComputeFaceBrightness(renderer.bounds.center);
            ApplyFaceBrightness(state, state.SmoothedBrightness);

            if (_nativeFeatureLogged < 20)
            {
                _nativeFeatureLogged++;
                string captured = "";
                for (int i = 0; i < state.Materials.Count; i++)
                {
                    if (i > 0)
                        captured += ", ";
                    NativeFeatureMaterial entry = state.Materials[i];
                    captured += entry.Original.name + "[" + entry.Original.shader.name +
                        (entry.InPlace ? " decal" : " tint") + "]";
                }
                Logger.LogInfo("FACE LIGHT captured renderer=" + renderer.name +
                    " root=" + renderer.transform.root.name +
                    " pos=" + renderer.bounds.center +
                    " brightness=" + state.Brightness.ToString("F2") +
                    " slots=" + state.Materials.Count + " -> " + captured);
            }
        }

        private Material GetFaceInstance(NativeFeatureRenderer state, Material source,
            out FaceInstanceInfo info)
        {
            info = null;
            int sourceId = source.GetInstanceID();
            FaceInstanceInfo cached;
            if (state.Instances.TryGetValue(sourceId, out cached) && cached != null &&
                cached.Instance != null)
            {
                info = cached;
                return cached.Instance;
            }

            // A screen that hands out a brand new material every frame would otherwise make this
            // cache grow without limit. Past this point the slot is left as authored.
            if (state.Owned.Count >= 32)
                return null;

            int sourceTint = 0;
            bool hasSourceTint = TryGetFaceProperty(source, FaceTintPropertyNames, true, out sourceTint);
            int sourceTexture = 0;
            bool hasSourceTexture = TryGetFaceProperty(source, FaceTexturePropertyNames, false,
                out sourceTexture);

            FaceInstanceInfo record = new FaceInstanceInfo();
            Material instance;
            if (hasSourceTint)
            {
                // Cheeks, freckles and one of the eyebrow families carry a real tint on the
                // game's own transparent graph, and the face-marks menu writes the colour the
                // player picked straight into it. Rebuilding them on a stock unlit shader with a
                // white tint is what pinned every cheek to whatever colour its decal happened to
                // be drawn in, whatever the menu said. Cloning keeps the authored shader,
                // texture, keywords and blend state, so at full daylight the overlay is
                // identical to the unmodded game, and the only thing this plugin does to it is
                // scale that colour by the light actually reaching the head.
                instance = new Material(source);
                record.TintProperty = sourceTint;
                record.BaseColor = source.GetColor(sourceTint);
                record.InstanceTextureProperty = sourceTexture;
            }
            else
            {
                // Eyes and mouths ship on Unlit/Transparent, which is fixed function and carries
                // no colour channel at all, so there is nothing on them to drive. URP's stock
                // unlit shader is the one that multiplies the texture by a tint.
                Texture texture = hasSourceTexture ? source.GetTexture(sourceTexture) : null;
                Shader tintShader = GetFaceTintShader();
                if (texture == null || tintShader == null)
                    return null;
                instance = new Material(tintShader);
                instance.SetTexture("_BaseMap", texture);
                if (source.HasProperty("_Tiling"))
                {
                    Vector4 tiling = source.GetVector("_Tiling");
                    if (tiling.x != 0f && tiling.y != 0f)
                        instance.SetTextureScale("_BaseMap", new Vector2(tiling.x, tiling.y));
                }
                if (source.HasProperty("_Offset"))
                {
                    Vector4 offset = source.GetVector("_Offset");
                    instance.SetTextureOffset("_BaseMap", new Vector2(offset.x, offset.y));
                }
                instance.SetColor("_BaseColor", Color.white);
                // Alpha blending is plain render state on this shader, so it does not depend on
                // a shader variant surviving build-time stripping.
                instance.SetFloat("_Surface", 1f);
                instance.SetFloat("_Blend", 0f);
                instance.SetFloat("_AlphaClip", 0f);
                instance.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                instance.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                PreserveDestinationAlpha(instance);
                instance.SetFloat("_ZWrite", 0f);
                instance.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                instance.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                instance.DisableKeyword("_ALPHATEST_ON");
                instance.SetShaderPassEnabled("ShadowCaster", false);
                instance.SetShaderPassEnabled("DepthOnly", false);
                instance.SetShaderPassEnabled("DepthNormals", false);
                instance.renderQueue = source.renderQueue > 0 ? source.renderQueue : 3000;
                record.TintProperty = BaseColorPropertyId;
                record.BaseColor = Color.white;
                record.InstanceTextureProperty = BaseMapPropertyId;
            }

            record.HasSourceTint = hasSourceTint;
            record.SourceTintProperty = sourceTint;
            record.LastSourceTint = hasSourceTint ? source.GetColor(sourceTint) : Color.white;
            record.HasSourceTexture = hasSourceTexture;
            record.SourceTextureProperty = sourceTexture;
            record.LastSourceTexture = hasSourceTexture ? source.GetTexture(sourceTexture) : null;

            instance.name = source.name + FaceInstanceSuffix;
            record.Instance = instance;
            info = record;
            state.Instances[sourceId] = record;
            state.ByInstance[instance.GetInstanceID()] = record;
            state.Owned.Add(instance);
            return instance;
        }

        // Looks a property up on the material's own shader rather than trusting HasProperty, so
        // the colour driven here is one the shader really declares. Vector-typed slots count
        // too: some of the game's graphs expose their tint that way.
        private static bool TryGetFaceProperty(Material material, string[] candidates,
            bool wantColor, out int propertyId)
        {
            propertyId = 0;
            if (material == null || material.shader == null)
                return false;
            Shader shader = material.shader;
            int count = shader.GetPropertyCount();
            for (int c = 0; c < candidates.Length; c++)
            {
                for (int p = 0; p < count; p++)
                {
                    UnityEngine.Rendering.ShaderPropertyType type = shader.GetPropertyType(p);
                    bool matches = wantColor
                        ? (type == UnityEngine.Rendering.ShaderPropertyType.Color ||
                           type == UnityEngine.Rendering.ShaderPropertyType.Vector)
                        : type == UnityEngine.Rendering.ShaderPropertyType.Texture;
                    if (!matches || shader.GetPropertyName(p) != candidates[c])
                        continue;
                    propertyId = shader.GetPropertyNameId(p);
                    return true;
                }
            }
            return false;
        }

        // Resolved from the shaders already loaded by the game so it can never fall back to
        // the missing-shader magenta.
        private Shader GetFaceTintShader()
        {
            if (_faceTintShader != null)
                return _faceTintShader;
            _faceTintShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (_faceTintShader == null)
            {
                Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
                for (int i = 0; i < shaders.Length; i++)
                {
                    if (shaders[i] != null && shaders[i].name == "Universal Render Pipeline/Unlit")
                    {
                        _faceTintShader = shaders[i];
                        break;
                    }
                }
            }
            Logger.LogInfo("FACE LIGHT tint shader=" +
                (_faceTintShader != null ? _faceTintShader.name : "MISSING"));
            return _faceTintShader;
        }

        private static bool IsNativeFeatureSource(Material material)
        {
            if (material == null || material.shader == null || IsNativeFeatureInstance(material))
                return false;
            // Eyes, mouths and eyebrows ship on Unlit/Transparent; cheeks, freckles and face
            // paint ship on the game's transparent shader graph. Both are unlit subtargets,
            // so neither one follows the sun or a lamp. Nothing else is converted.
            string shader = material.shader.name;
            if (!shader.Equals("Unlit/Transparent", StringComparison.OrdinalIgnoreCase) &&
                shader.IndexOf("TransparentShader", StringComparison.OrdinalIgnoreCase) < 0)
                return false;
            string name = material.name.ToLowerInvariant();
            if (name.IndexOf("glass") >= 0)
                return false;
            return name.IndexOf("eye") >= 0 || name.IndexOf("mouth") >= 0 ||
                name.IndexOf("brow") >= 0 || name.IndexOf("cheek") >= 0 ||
                name.IndexOf("freckle") >= 0 || name.IndexOf("blush") >= 0 ||
                name.IndexOf("paint") >= 0 || name.IndexOf("facial") >= 0;
        }

        private static bool IsNativeFeatureInstance(Material material)
        {
            return material != null && material.name.IndexOf(FaceInstanceSuffix, StringComparison.Ordinal) >= 0;
        }

        private void UpdateNativeFeatureBrightness()
        {
            foreach (KeyValuePair<int, NativeFeatureRenderer> pair in _nativeFeatureRenderers)
            {
                NativeFeatureRenderer state = pair.Value;
                if (state == null || state.Renderer == null)
                    continue;
                SyncNativeFeatureSources(state);
                float target = ComputeFaceBrightness(state.Renderer.bounds.center);
                // Called on a 0.1 s tick, so this crosses the whole 0.32-1 range in about 0.8 s.
                // ComputeFaceBrightness takes the strongest lamp in range, so a lamp being
                // switched on or off by the local-light budget moves it in one step; easing
                // turns that step into a fade nobody reads as a flicker.
                if (state.SmoothedBrightness < 0f)
                    state.SmoothedBrightness = target;
                else
                    state.SmoothedBrightness =
                        Mathf.MoveTowards(state.SmoothedBrightness, target, 0.085f);
                ApplyFaceBrightness(state, state.SmoothedBrightness);
            }
        }

        // The overlays are ours to dim, but their colour and shape belong to the customization
        // system, which keeps writing both after we have taken the slot over. Two routes:
        // the face-marks menu writes straight onto our material, because a clone of "M_Cheek13"
        // is still called "...Cheek13..." and the game finds its cheek slot by name; and a head
        // rebuild puts the new colour and decal on the pooled source material instead. Both are
        // picked up here, so the tint we scale is always the one the player actually chose.
        // Without this the cheek stayed at whatever colour it had when the slot was taken.
        private static void SyncNativeFeatureSources(NativeFeatureRenderer state)
        {
            for (int i = 0; i < state.Materials.Count; i++)
            {
                NativeFeatureMaterial entry = state.Materials[i];
                if (entry == null || entry.Instance == null || entry.InPlace)
                    continue;

                // Written onto our own material by someone else.
                if (entry.HasWritten)
                {
                    Color live = entry.Instance.GetColor(entry.TintProperty);
                    if (!ColorsRoughlyEqual(live, entry.LastWritten))
                    {
                        entry.BaseColor = live;
                        if (entry.Info != null)
                            entry.Info.BaseColor = live;
                        // Re-tint on this pass rather than waiting for the brightness to move.
                        state.Brightness = -1f;
                    }
                }

                FaceInstanceInfo info = entry.Info;
                if (info == null || entry.Original == null || entry.Original == entry.Instance)
                    continue;

                if (info.HasSourceTint)
                {
                    Color sourceTint = entry.Original.GetColor(info.SourceTintProperty);
                    if (!ColorsRoughlyEqual(sourceTint, info.LastSourceTint))
                    {
                        info.LastSourceTint = sourceTint;
                        info.BaseColor = sourceTint;
                        entry.BaseColor = sourceTint;
                        state.Brightness = -1f;
                    }
                }

                if (info.HasSourceTexture)
                {
                    Texture sourceTexture = entry.Original.GetTexture(info.SourceTextureProperty);
                    if (sourceTexture != null && sourceTexture != info.LastSourceTexture)
                    {
                        // Cheek shapes are changed in place: the game writes the new decal onto
                        // the material it found rather than swapping the material out, so a
                        // cached instance would otherwise keep the first blush shape forever.
                        info.LastSourceTexture = sourceTexture;
                        entry.Instance.SetTexture(info.InstanceTextureProperty, sourceTexture);
                    }
                }
            }
        }

        // Approximates what the avatar's lit materials receive at that spot: scene ambient,
        // the sun or moon, and every room lamp in range. A face under a lamp at night reads
        // bright here; the same face in an unlit corner reads dark.
        private float ComputeFaceBrightness(Vector3 position)
        {
            // Driven by the mod's own day progress, not by RenderSettings: ambient light is a
            // per-scene setting, so reading it made the customization screen dim correctly while
            // the world scene stayed at full brightness.
            float lit = Mathf.Clamp01(_lastDayWeight);
            // Every non-directional light in the scene counts, not just the street lamps:
            // light strings, desk lamps, screens, campfires and anything else that lights a
            // face all belong here.
            for (int i = 0; i < _faceLightSources.Count; i++)
            {
                Light lamp = _faceLightSources[i];
                if (lamp == null || !lamp.isActiveAndEnabled || lamp.range <= 0.01f)
                    continue;
                float distance = Vector3.Distance(position, lamp.transform.position);
                if (distance >= lamp.range)
                    continue;
                float falloff = 1f - distance / lamp.range;
                // Not lamp.intensity. The local-light budget zeroes every lamp each frame and
                // then re-lights only the ones nearest the camera, so reading the live value
                // meant an NPC's face tracked which lamps happened to be inside that budget.
                // Walking past flipped a lamp in or out and the face stepped with it - that is
                // the flicker. GetLightIntensity returns what the lamp is meant to be putting
                // out, which does not depend on the camera at all.
                float intended = GetLightIntensity(lamp,
                    Mathf.Clamp(_lampIntensity.Value, 0f, 8f) * _lampWeight);
                float lamplight = Mathf.Clamp01(intended * lamp.color.grayscale) * falloff * falloff;
                if (lamplight > lit)
                    lit = lamplight;
            }
            return Mathf.Lerp(0.32f, 1f, lit);
        }

        // Cheeks, freckles and face paint carry their colour in the decal texture: every style
        // ships a differently coloured image while they all share one unused _BaseColor value.
        // Scaling that texture's RGB is the only channel these materials actually react to.
        // Alpha is copied byte for byte, so the decal shape and its soft edges never change.
        private static void UpdateDecalTexture(NativeFeatureMaterial entry, float brightness)
        {
            Material material = entry.Instance;
            if (material == null || entry.TextureProperty == null)
                return;

            Texture current = material.GetTexture(entry.TextureProperty);
            if (current != entry.DimTexture)
            {
                // The player picked a different decal, so start again from the new image.
                if (entry.DimTexture != null)
                    UnityEngine.Object.Destroy(entry.DimTexture);
                entry.DimTexture = null;
                entry.SourceTexture = current;
                entry.Bucket = -1;
                Texture2D copy;
                Color32[] pixels;
                if (!TryCopyTexturePixels(current, FaceDecalMaxSize, out copy, out pixels))
                    return;
                entry.DimTexture = copy;
                entry.SourcePixels = pixels;
                entry.Scratch = new Color32[pixels.Length];
            }

            int bucket = Mathf.Clamp(Mathf.RoundToInt(brightness * FaceBrightnessSteps), 1, FaceBrightnessSteps);
            if (bucket != entry.Bucket)
            {
                entry.Bucket = bucket;
                float scale = (float)bucket / FaceBrightnessSteps;
                for (int i = 0; i < entry.SourcePixels.Length; i++)
                {
                    Color32 source = entry.SourcePixels[i];
                    entry.Scratch[i] = new Color32(
                        (byte)(source.r * scale),
                        (byte)(source.g * scale),
                        (byte)(source.b * scale),
                        source.a);
                }
                entry.DimTexture.SetPixels32(entry.Scratch);
                entry.DimTexture.Apply(false);
            }
            material.SetTexture(entry.TextureProperty, entry.DimTexture);
        }

        // Decal textures are imported non-readable, so the pixels come back through a temporary
        // render target instead of GetPixels on the source asset.
        private static bool TryCopyTexturePixels(Texture source, int maxSize, out Texture2D copy,
            out Color32[] pixels)
        {
            copy = null;
            pixels = null;
            if (source == null)
                return false;
            int width = Mathf.Min(source.width, maxSize);
            int height = Mathf.Min(source.height, maxSize);
            if (width <= 0 || height <= 0)
                return false;

            RenderTexture previous = RenderTexture.active;
            RenderTexture temp = RenderTexture.GetTemporary(width, height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            try
            {
                Graphics.Blit(source, temp);
                RenderTexture.active = temp;
                copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
                copy.name = source.name + FaceInstanceSuffix;
                copy.wrapMode = source.wrapMode;
                copy.filterMode = source.filterMode;
                copy.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                copy.Apply(false);
                pixels = copy.GetPixels32();
                return true;
            }
            catch (Exception)
            {
                if (copy != null)
                    UnityEngine.Object.Destroy(copy);
                copy = null;
                pixels = null;
                return false;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temp);
            }
        }

        // The desk-pet camera mode (overlay preset 4) composites the frame onto the desktop
        // through the window's alpha channel, so whatever the last draw leaves in alpha decides
        // what the player sees through. URP's unlit shader defaults its separate alpha blend to
        // (One, Zero), which means a transparent overlay stamps its own alpha over the surface
        // underneath: the fully transparent materials this plugin puts on a device's non-screen
        // submeshes were writing alpha 0 across the whole laptop, phone or console, and the
        // wallpaper showed straight through it. These overlays only ever sit on top of geometry
        // that already wrote its own alpha, so the correct rule is to leave the destination
        // alpha exactly as it is.
        private static void PreserveDestinationAlpha(Material material)
        {
            if (material == null)
                return;
            if (material.HasProperty("_SrcBlendAlpha"))
                material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.Zero);
            if (material.HasProperty("_DstBlendAlpha"))
                material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
        }

        private static bool ColorsRoughlyEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.02f && Mathf.Abs(a.g - b.g) < 0.02f &&
                Mathf.Abs(a.b - b.b) < 0.02f && Mathf.Abs(a.a - b.a) < 0.02f;
        }

        private static void ApplyFaceBrightness(NativeFeatureRenderer state, float brightness)
        {
            if (state == null)
                return;
            state.Brightness = brightness;
            for (int i = 0; i < state.Materials.Count; i++)
            {
                NativeFeatureMaterial entry = state.Materials[i];
                if (entry == null || entry.Instance == null)
                    continue;
                if (entry.InPlace)
                {
                    UpdateDecalTexture(entry, brightness);
                    continue;
                }
                // Alpha stays authored: only the visible colour follows the light.
                Color tinted = new Color(
                    entry.BaseColor.r * brightness,
                    entry.BaseColor.g * brightness,
                    entry.BaseColor.b * brightness,
                    entry.BaseColor.a);
                entry.Instance.SetColor(entry.TintProperty, tinted);
                // Store what the material actually holds afterwards, not what we asked for, so
                // the next comparison is against the same representation it will read back.
                entry.LastWritten = entry.Instance.GetColor(entry.TintProperty);
                entry.HasWritten = true;
            }
        }

        private static bool NativeFeatureAssignmentIsCurrent(NativeFeatureRenderer state)
        {
            if (state == null || state.Renderer == null || state.Assigned == null)
                return false;
            Material[] current = state.Renderer.sharedMaterials;
            if (current.Length != state.Assigned.Length)
                return false;
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != state.Assigned[i])
                    return false;
            }
            return true;
        }

        private static void ReleaseNativeFeatureMaterials(NativeFeatureRenderer state, bool restoreRenderer)
        {
            if (state == null)
                return;
            if (restoreRenderer && state.Renderer != null)
            {
                Material[] current = state.Renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < state.Materials.Count; i++)
                {
                    NativeFeatureMaterial entry = state.Materials[i];
                    if (entry.InPlace)
                    {
                        if (entry.Instance != null && entry.TextureProperty != null &&
                            entry.SourceTexture != null)
                            entry.Instance.SetTexture(entry.TextureProperty, entry.SourceTexture);
                        continue;
                    }
                    if (entry.Slot < current.Length && current[entry.Slot] == entry.Instance &&
                        entry.Original != entry.Instance)
                    {
                        current[entry.Slot] = entry.Original;
                        changed = true;
                    }
                }
                if (changed)
                    state.Renderer.sharedMaterials = current;
            }
            foreach (KeyValuePair<int, NativeFeatureMaterial> pair in state.InPlaceEntries)
            {
                if (pair.Value != null && pair.Value.DimTexture != null)
                    UnityEngine.Object.Destroy(pair.Value.DimTexture);
            }
            state.InPlaceEntries.Clear();
            for (int i = 0; i < state.Owned.Count; i++)
            {
                if (state.Owned[i] != null)
                    UnityEngine.Object.Destroy(state.Owned[i]);
            }
            state.Owned.Clear();
            state.Instances.Clear();
            state.ByInstance.Clear();
            state.Materials.Clear();
        }

        private void RestoreNativeFeatureMaterials()
        {
            foreach (KeyValuePair<int, NativeFeatureRenderer> pair in _nativeFeatureRenderers)
                ReleaseNativeFeatureMaterials(pair.Value, true);
            _nativeFeatureRenderers.Clear();
            _faceRenderers.Clear();
        }

        private void TryCapturePlayerNightMaterial(Material material)
        {
            if (!_decorDimming.Value || _decorSeen.Contains(material) ||
                _waterSeen.Contains(material) || _surfaceToonMaterials.Contains(material) ||
                _basketCourtToonMaterials.Contains(material))
                return;
            if (IsLightIgnoringMaterial(material))
            {
                // Eyes, mouths, eyebrows, cheeks and species overlays use several custom
                // transparent shader graphs. Replacing or tinting those shared materials can
                // turn the whole decal quad opaque black on different shader variants/GPUs.
                // Their authored unlit appearance is safer than mutating face alpha semantics.
                return;
            }
            // Lit Toon parts can still glow at night through the emissive channel,
            // which no light or exposure ever scales. Dim just that channel.
            if (material.HasProperty("_Emissive_Color"))
            {
                Color emissive = material.GetColor("_Emissive_Color");
                if (emissive.maxColorComponent > 0.02f)
                {
                    WaterMaterial entry = new WaterMaterial();
                    entry.Material = material;
                    entry.PropertyIds = new int[] { Shader.PropertyToID("_Emissive_Color") };
                    entry.PropertyNames = new string[] { "_Emissive_Color" };
                    entry.Originals = new Color[] { emissive };
                    _decorSeen.Add(material);
                    _decorMaterials.Add(entry);
                    Logger.LogInfo("DECOR player emissive=" + material.name +
                        " shader=" + material.shader.name);
                }
            }
        }

        private static string GetPlayerMaterialShadowSummary(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            string summary = "";
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (i > 0)
                    summary += ",";
                if (material == null)
                {
                    summary += "<null>";
                    continue;
                }
                summary += material.name + "[" + (material.shader != null ? material.shader.name : "no-shader");
                if (material.HasProperty("_Set_SystemShadowsToBase"))
                    summary += " sys=" + material.GetFloat("_Set_SystemShadowsToBase");
                if (material.HasProperty("_Tweak_SystemShadowsLevel"))
                    summary += " level=" + material.GetFloat("_Tweak_SystemShadowsLevel");
                if (material.HasProperty("_BaseColor_Step"))
                    summary += " step=" + material.GetFloat("_BaseColor_Step");
                if (material.HasProperty("_Use_BaseAs1st"))
                    summary += " base1=" + material.GetFloat("_Use_BaseAs1st");
                summary += "]";
            }
            return summary;
        }

        private static bool IsMainIslandSurface(Renderer renderer)
        {
            // Do not classify every descendant of IslandGround as terrain. Several authored structures are
            // grouped under that transform; the old ancestor test silently disabled their shadow casting.
            // The actual receiver meshes have stable ground-specific renderer names/materials.
            string name = NormalizeSceneName(renderer.gameObject.name);
            if (IsNumberedGroundName(name, "mdground") || IsNumberedGroundName(name, "prground") ||
                name == "islandground" || name == "prislandmain" || name == "mdislandcollider" ||
                name.IndexOf("schoolground") >= 0 || name.IndexOf("maincirclegrass") >= 0 ||
                name == "roadmeshholder")
                return true;

            if (HasStructuralIdentity(renderer))
                return false;

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                    continue;
                string materialName = NormalizeSceneName(material.name);
                if (materialName.StartsWith("mgroundgrass") || materialName.StartsWith("mgroundstone") ||
                    materialName.StartsWith("mpathnature") || materialName.StartsWith("mbeach"))
                    return true;
            }
            return false;
        }

        private static bool IsNumberedGroundName(string name, string prefix)
        {
            return name.StartsWith(prefix) && name.Length > prefix.Length &&
                char.IsDigit(name[prefix.Length]);
        }

        private static string NormalizeSceneName(string name)
        {
            string lower = name != null ? name.ToLowerInvariant() : "";
            char[] buffer = new char[lower.Length];
            int length = 0;
            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                if (char.IsLetterOrDigit(c))
                    buffer[length++] = c;
            }
            return new string(buffer, 0, length);
        }

        // ---------------------------------------------------------------
        // Water: collect water materials so the day cycle can drive them
        // ---------------------------------------------------------------
        private void ScanWaterMaterials()
        {
            if (!_waterDaylight.Value)
            {
                RestoreWaterMaterials();
                RestoreWaterParticles();
                ClearWaterShadowOverlays();
                return;
            }

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            _surfaceToonMaterials.Clear();
            _basketCourtToonMaterials.Clear();
            // Identify the actual main-island receiver materials before applying the global shade colors.
            // Their authored BaseColor_Step is 0.72-0.80: harmless with near-identical vanilla shade colors,
            // but it classifies almost the whole island as shade once LookCare makes that shade visibly dark.
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;
                bool mainSurface = IsMainIslandSurface(renderer);
                bool basketballCourt = IsBasketballCourtRenderer(renderer);
                if (!mainSurface && !basketballCourt)
                    continue;
                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    if (materials[m] != null && mainSurface)
                        _surfaceToonMaterials.Add(materials[m]);
                    if (materials[m] != null && basketballCourt && IsBasketballCourtMaterial(materials[m]))
                        _basketCourtToonMaterials.Add(materials[m]);
                }
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;

                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material == null || material.shader == null || _waterSeen.Contains(material))
                        continue;

                    // Only the dedicated water shader. Matching by object name proved
                    // disastrous: decorative "Wave"/"Sea*" props share the atlas Toon
                    // materials used by the ground and walls of the whole island.
                    if (material.shader.name.ToLowerInvariant().IndexOf("stylized water") < 0)
                        continue;

                    _waterSeen.Add(material);
                    Shader shader = material.shader;
                    List<int> ids = new List<int>();
                    List<string> names = new List<string>();
                    List<Color> originals = new List<Color>();
                    string propNames = "";
                    int count = shader.GetPropertyCount();
                    for (int p = 0; p < count; p++)
                    {
                        if (shader.GetPropertyType(p) != UnityEngine.Rendering.ShaderPropertyType.Color)
                            continue;
                        int id = shader.GetPropertyNameId(p);
                        ids.Add(id);
                        names.Add(shader.GetPropertyName(p));
                        Color authored = material.GetColor(id);
                        originals.Add(authored);
                        // The authored values decide how the sky mirror lands, so log them.
                        propNames += (propNames.Length > 0 ? "," : "") + shader.GetPropertyName(p) +
                            "=" + ColorUtility.ToHtmlStringRGBA(authored);
                    }
                    if (ids.Count == 0)
                        continue;

                    WaterMaterial water = new WaterMaterial();
                    water.Material = material;
                    water.PropertyIds = ids.ToArray();
                    water.PropertyNames = names.ToArray();
                    water.Originals = originals.ToArray();
                    water.DrivenColors = new Color[water.PropertyIds.Length];
                    water.SkyMirrorWeights = new float[water.PropertyIds.Length];
                    for (int p = 0; p < water.PropertyIds.Length; p++)
                        water.SkyMirrorWeights[p] = SkyMirrorWeight(water.PropertyNames[p]);
                    water.IsWaterfall = material.name.ToLowerInvariant().IndexOf("waterfall") >= 0;
                    water.IsOpenWater = !water.IsWaterfall;
                    water.HasReflectionStrength = material.HasProperty("_ReflectionStrength");
                    if (water.HasReflectionStrength)
                        water.OriginalReflectionStrength = material.GetFloat("_ReflectionStrength");
                    _waterMaterials.Add(water);
                    Logger.LogInfo("WATER material=" + material.name + " shader=" + material.shader.name +
                        " colors=[" + propNames + "] reflection=" +
                        (water.HasReflectionStrength ? water.OriginalReflectionStrength.ToString("F2") : "n/a"));
                    // How far the horizon color reaches back toward the viewer is a shader float,
                    // and that distance is the only way to widen the warm band on a dusk sea
                    // without tinting the water body itself, which turns the whole surface purple.
                    // Dump the floats so the right knob can be identified.
                    string floatNames = "";
                    for (int p = 0; p < count; p++)
                    {
                        UnityEngine.Rendering.ShaderPropertyType type = shader.GetPropertyType(p);
                        if (type != UnityEngine.Rendering.ShaderPropertyType.Float &&
                            type != UnityEngine.Rendering.ShaderPropertyType.Range)
                            continue;
                        string floatName = shader.GetPropertyName(p);
                        floatNames += (floatNames.Length > 0 ? "," : "") + floatName + "=" +
                            material.GetFloat(shader.GetPropertyNameId(p)).ToString("0.###");
                    }
                    Logger.LogInfo("WATER floats " + material.name + " [" + floatNames + "]");
                }
            }
            // The sun-glitter path needs the plane it lies on. Take the widest horizontal water
            // surface: the main sea dwarfs the pond and the fountain basins.
            float widestSea = 0f;
            _seaSurfaceValid = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare") ||
                    !HasStylizedWaterMaterial(renderer) || !IsHorizontalWaterSurface(renderer))
                    continue;
                Bounds bounds = renderer.bounds;
                float span = Mathf.Max(bounds.size.x, bounds.size.z);
                if (span <= widestSea)
                    continue;
                widestSea = span;
                _seaSurfaceY = bounds.max.y;
                _seaBounds = bounds;
                _seaSurfaceValid = true;
            }

            ScanWaterParticles();
            EnsureWaterShadowOverlays();
            Logger.LogInfo("WATER tracked materials: " + _waterMaterials.Count +
                " seaPlaneY=" + (_seaSurfaceValid ? _seaSurfaceY.ToString("F2") : "n/a") +
                " seaSpan=" + widestSea.ToString("F0"));
        }

        private static bool IsWaterHighlightProperty(string propertyName)
        {
            string name = propertyName != null ? propertyName.ToLowerInvariant() : "";
            return name.IndexOf("foam") >= 0 || name.IndexOf("intersection") >= 0 ||
                name.IndexOf("wave") >= 0 || name.IndexOf("crest") >= 0;
        }

        // How much of each Stylized Water color channel the viewer actually reads as reflected sky.
        // The horizon color owns the grazing angles that fill most of the screen when looking out to
        // sea, which is exactly where a real ocean is a mirror. Returning 0 leaves a channel on the
        // older warm-cast path.
        private static float SkyMirrorWeight(string propertyName)
        {
            string name = propertyName != null ? propertyName.ToLowerInvariant() : "";
            if (name.IndexOf("horizon") >= 0)
                return 1f;
            if (name.IndexOf("shallow") >= 0)
                return 0.42f;
            if (name.IndexOf("base") >= 0 || name.IndexOf("deep") >= 0 || name.IndexOf("fog") >= 0)
                return 0.32f;
            return 0f;
        }

        // The plugin paints the sky itself, so the color the sea should mirror is already known:
        // it follows from the day weight, the rolled twilight variant and the configured sky tint.
        // Evaluating it costs nothing, where a planar reflection camera costs a second scene render
        // every frame - and the game's water meshes carry no WaterObject for one to attach to.
        private Color EvaluateSkyMirrorColor(float dayWeight, float twilightGlow)
        {
            dayWeight = Mathf.Clamp01(dayWeight);
            twilightGlow = Mathf.Clamp01(twilightGlow);
            // A pale hazy blue, not a deep one. A real horizon is washed out by distance, and a
            // saturated blue here cancels against the warm dusk color on the way past it, which
            // puts a grey step in the middle of every sunrise and sunset.
            Color daySky = Color.Lerp(new Color(0.74f, 0.81f, 0.88f, 1f), _skyTint.Value, 0.22f);
            Color nightSky = new Color(0.055f, 0.075f, 0.145f, 1f) *
                Mathf.Lerp(0.75f, 1.25f, Mathf.Clamp01(_moonIllumination));
            Color sky = Color.Lerp(nightSky, daySky, dayWeight);
            // Dawn and dusk reuse the exact variant colors that drive the glow band and the sun
            // disc, so the sea can never disagree with the sky directly above it.
            Color duskSky = Color.Lerp(_twilightGlowColor, _twilightSunColor, 0.30f) *
                Mathf.Lerp(0.62f, 1f, dayWeight);
            sky = Color.Lerp(sky, duskSky, twilightGlow);
            // Rain flattens the dome into one overcast grey, and there is no sunset left to mirror.
            sky = Color.Lerp(sky,
                new Color(0.42f, 0.46f, 0.52f, 1f) * Mathf.Lerp(0.35f, 1f, dayWeight), _rainBlend);
            sky.a = 1f;
            return sky;
        }

        private static float ColorLuminance(Color color)
        {
            return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        }

        // Straight RGB interpolation between the sea's cyan and a warm dusk sky passes exactly
        // through grey at the halfway point, which is the muddy in-between color the day/night
        // fade used to show. Rotating the hue instead keeps the water saturated the whole way,
        // and forcing the rotation through increasing hue takes it cyan -> blue -> violet ->
        // pink -> orange: the route a real sea takes at dusk, never through green.
        private static Color BlendThroughBlue(Color from, Color to, float amount)
        {
            amount = Mathf.Clamp01(amount);
            if (amount <= 0.001f)
                return from;
            float fromHue, fromSaturation, fromValue;
            float toHue, toSaturation, toValue;
            Color.RGBToHSV(ClampColor(from), out fromHue, out fromSaturation, out fromValue);
            Color.RGBToHSV(ClampColor(to), out toHue, out toSaturation, out toValue);
            // A fully desaturated endpoint carries no usable hue; hold the other one instead.
            if (toSaturation < 0.02f)
                toHue = fromHue;
            else if (fromSaturation < 0.02f)
                fromHue = toHue;
            float hue = Mathf.Repeat(fromHue + Mathf.Repeat(toHue - fromHue, 1f) * amount, 1f);
            Color result = Color.HSVToRGB(hue,
                Mathf.Lerp(fromSaturation, toSaturation, amount),
                Mathf.Lerp(fromValue, toValue, amount));
            result.a = from.a;
            return result;
        }

        private static Color ClampColor(Color color)
        {
            return new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g),
                Mathf.Clamp01(color.b), color.a);
        }

        private static Color ApplyLuminancePreservingTint(Color color, Color tint, float amount)
        {
            amount = Mathf.Clamp01(amount);
            if (amount <= 0.001f)
                return color;
            float luminance = ColorLuminance(color);
            float tintLuminance = Mathf.Max(0.0001f, ColorLuminance(tint));
            float scale = luminance / tintLuminance;
            Color target = new Color(
                Mathf.Clamp01(tint.r * scale), Mathf.Clamp01(tint.g * scale),
                Mathf.Clamp01(tint.b * scale), color.a);
            Color result = Color.Lerp(color, target, amount);
            result.a = color.a;
            return result;
        }

        private void ScanWaterParticles()
        {
            ParticleSystem[] systems = UnityEngine.Object.FindObjectsByType<ParticleSystem>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int added = 0;
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem system = systems[i];
                if (system == null || _waterParticleIds.Contains(system.GetInstanceID()))
                    continue;
                string path = GetTransformPath(system.transform);
                string lower = path.ToLowerInvariant();
                if (lower.IndexOf("fountain") < 0 && lower.IndexOf("waterfall") < 0)
                    continue;
                ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                if (renderer == null)
                    continue;
                Material material = renderer.sharedMaterial;
                string materialName = material != null ? material.name.ToLowerInvariant() : "";
                if (materialName.IndexOf("fx waterfall") < 0 && materialName.IndexOf("bubbles") < 0)
                    continue;
                WaterParticleState state = new WaterParticleState();
                state.System = system;
                state.OriginalStartColor = system.main.startColor;
                _waterParticles.Add(state);
                _waterParticleIds.Add(system.GetInstanceID());
                added++;
                Logger.LogInfo("WATER PARTICLE path=" + path + " material=" +
                    (material != null ? material.name : "<null>") + " shader=" +
                    (material != null && material.shader != null ? material.shader.name : "<null>"));
            }
            if (added > 0)
                Logger.LogInfo("WATER PARTICLES added=" + added + " total=" + _waterParticles.Count);
        }

        private void UpdateWaterParticles(float dayWeight, float twilight)
        {
            float nightBrightness = Mathf.Clamp01(_nightWaterfallFoamBrightness.Value);
            float brightness = Mathf.Lerp(nightBrightness, 1f, dayWeight);
            Color multiplier = Color.Lerp(new Color(0.68f, 0.78f, 0.95f, 1f), Color.white, dayWeight);
            multiplier *= brightness;
            multiplier = ApplyLuminancePreservingTint(multiplier, _twilightGlowColor, twilight * 0.45f);
            for (int i = _waterParticles.Count - 1; i >= 0; i--)
            {
                WaterParticleState state = _waterParticles[i];
                if (state.System == null)
                {
                    _waterParticles.RemoveAt(i);
                    continue;
                }
                ParticleSystem.MainModule main = state.System.main;
                main.startColor = MultiplyParticleGradient(state.OriginalStartColor, multiplier);
            }
        }

        private static ParticleSystem.MinMaxGradient MultiplyParticleGradient(
            ParticleSystem.MinMaxGradient original, Color multiplier)
        {
            ParticleSystem.MinMaxGradient driven = original;
            switch (original.mode)
            {
                case ParticleSystemGradientMode.Color:
                    driven.color = MultiplyParticleColor(original.color, multiplier);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    driven.colorMin = MultiplyParticleColor(original.colorMin, multiplier);
                    driven.colorMax = MultiplyParticleColor(original.colorMax, multiplier);
                    break;
                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.RandomColor:
                    driven.gradient = MultiplyParticleColorGradient(original.gradient, multiplier);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    driven.gradientMin = MultiplyParticleColorGradient(original.gradientMin, multiplier);
                    driven.gradientMax = MultiplyParticleColorGradient(original.gradientMax, multiplier);
                    break;
            }
            return driven;
        }

        private static Gradient MultiplyParticleColorGradient(Gradient original, Color multiplier)
        {
            if (original == null)
                return null;
            GradientColorKey[] colorKeys = original.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
                colorKeys[i].color = MultiplyParticleColor(colorKeys[i].color, multiplier);
            Gradient driven = new Gradient();
            driven.mode = original.mode;
            driven.SetKeys(colorKeys, original.alphaKeys);
            return driven;
        }

        private static Color MultiplyParticleColor(Color color, Color multiplier)
        {
            return new Color(color.r * multiplier.r, color.g * multiplier.g,
                color.b * multiplier.b, color.a);
        }

        private void RestoreWaterParticles()
        {
            for (int i = 0; i < _waterParticles.Count; i++)
            {
                WaterParticleState state = _waterParticles[i];
                if (state.System == null)
                    continue;
                ParticleSystem.MainModule main = state.System.main;
                main.startColor = state.OriginalStartColor;
            }
            _waterParticles.Clear();
            _waterParticleIds.Clear();
        }

        // Stylized Water 3 in this build has no compiled variant that both receives URP
        // directional shadows and keeps its wave/foam pass. Do not toggle its shadow keyword:
        // that provably removes the waves. Instead, draw a very low-opacity URP Lit receiver
        // on the exact same horizontal mesh. It samples the real main-light shadow map while
        // the native water below remains responsible for animation, foam and reflections.
        private void EnsureWaterShadowOverlays()
        {
            if (!_waterShadowOverlayEnabled.Value)
            {
                ClearWaterShadowOverlays();
                return;
            }
            if (_waterShadowOverlayMaterial == null && !CreateWaterShadowOverlayMaterial())
                return;

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int added = 0;
            int skippedVertical = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer source = renderers[i];
                if (source == null || source.gameObject.name.StartsWith("LookCare"))
                    continue;
                int sourceId = source.GetInstanceID();
                if (_waterShadowOverlaySourceIds.Contains(sourceId) || !HasStylizedWaterMaterial(source))
                    continue;

                MeshFilter filter = source.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                    continue;
                if (!IsHorizontalWaterSurface(source))
                {
                    skippedVertical++;
                    continue;
                }

                GameObject overlay = new GameObject("LookCare Water Shadow Receiver");
                overlay.layer = source.gameObject.layer;
                overlay.transform.SetParent(source.transform, false);
                // Water meshes in this game are level. Raising only six millimetres avoids z-fighting
                // without separating the receiver visibly from animated native water underneath.
                overlay.transform.localPosition = Vector3.up * 0.006f;
                MeshFilter overlayFilter = overlay.AddComponent<MeshFilter>();
                overlayFilter.sharedMesh = filter.sharedMesh;
                MeshRenderer overlayRenderer = overlay.AddComponent<MeshRenderer>();
                overlayRenderer.sharedMaterial = _waterShadowOverlayMaterial;
                overlayRenderer.shadowCastingMode = ShadowCastingMode.Off;
                overlayRenderer.receiveShadows = true;
                overlayRenderer.lightProbeUsage = LightProbeUsage.Off;
                overlayRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                overlayRenderer.sortingOrder = 20;

                _waterShadowOverlayObjects.Add(overlay);
                _waterShadowOverlaySourceIds.Add(sourceId);
                added++;
            }
            if (added > 0 || skippedVertical > 0)
                Logger.LogInfo("WATER SHADOW receivers added=" + added + " total=" +
                    _waterShadowOverlayObjects.Count + " vertical-skipped=" + skippedVertical);
        }

        private bool CreateWaterShadowOverlayMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Logger.LogWarning("WATER SHADOW unavailable: Universal Render Pipeline/Lit was not loaded.");
                return false;
            }
            Material material = new Material(shader);
            material.name = "LookCare Water Directional Shadow Receiver";
            material.renderQueue = 3100;
            SetMaterialFloat(material, "_Surface", 1f);
            SetMaterialFloat(material, "_Blend", 0f);
            SetMaterialFloat(material, "_SrcBlend", 5f); // SrcAlpha
            SetMaterialFloat(material, "_DstBlend", 10f); // OneMinusSrcAlpha
            SetMaterialFloat(material, "_ZWrite", 0f);
            SetMaterialFloat(material, "_Cull", 0f);
            SetMaterialFloat(material, "_ReceiveShadows", 1f);
            SetMaterialFloat(material, "_SpecularHighlights", 0f);
            SetMaterialFloat(material, "_EnvironmentReflections", 0f);
            SetMaterialFloat(material, "_Metallic", 0f);
            SetMaterialFloat(material, "_Smoothness", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_RECEIVE_SHADOWS_OFF");
            _waterShadowOverlayMaterial = material;
            UpdateWaterShadowOverlayMaterial(_lastDayWeight);
            return true;
        }

        private void UpdateWaterShadowOverlayMaterial(float waterDayWeight)
        {
            if (_waterShadowOverlayMaterial == null)
                return;
            float alpha = Mathf.Clamp(_waterShadowOverlayStrength.Value, 0.02f, 0.60f);
            // Keep lit water close to its native reflected color. In a shadow, URP attenuates
            // this receiver through the same directional shadow map used by ground and props.
            Color night = new Color(0.16f, 0.24f, 0.36f, alpha);
            Color day = new Color(0.52f, 0.67f, 0.79f, alpha);
            Color receiverTint = Color.Lerp(night, day, Mathf.Clamp01(waterDayWeight));
            // A fixed blue-grey layer over a sunset sea greys it straight back out, so the
            // receiver follows the same sky color the water mirrors and thins out while the
            // sun crosses the horizon.
            float twilightGlow = Mathf.Clamp01(_lastTwilightGlow);
            receiverTint = Color.Lerp(receiverTint, _seaSkyMirrorColor, twilightGlow * 0.8f);
            // Match the water's own driven color, so lit water is barely altered and the layer
            // shows up as what it is for: the shadow. A veil that darkens lit water also splits
            // the palette, because only horizontal surfaces carry it - the vertical waterfall
            // sheet gets no receiver and would otherwise sit brighter than the pond it feeds.
            receiverTint = Color.Lerp(receiverTint, _seaDrivenBodyColor * 1.25f, 0.55f);
            // Directional shadows are barely there under moonlight, so the layer has nothing
            // left to show at night; keeping it on only tints the water and breaks that match.
            alpha *= Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(waterDayWeight)) *
                Mathf.Lerp(1f, 0.62f, twilightGlow);
            receiverTint = Color.Lerp(receiverTint, new Color(0.28f, 0.36f, 0.47f, alpha),
                _rainBlend * 0.35f);
            receiverTint.a = alpha;
            SetMaterialColor(_waterShadowOverlayMaterial, "_BaseColor", receiverTint);
            SetMaterialColor(_waterShadowOverlayMaterial, "_BaseMapColor", receiverTint);
        }

        private static bool HasStylizedWaterMaterial(Renderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null && material.shader != null &&
                    material.shader.name.IndexOf("stylized water", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static bool IsHorizontalWaterSurface(Renderer renderer)
        {
            Bounds bounds = renderer.bounds;
            float horizontalSpan = Mathf.Max(bounds.size.x, bounds.size.z);
            return horizontalSpan > 0.25f && bounds.size.y <= horizontalSpan * 0.20f;
        }

        private void ClearWaterShadowOverlays()
        {
            for (int i = 0; i < _waterShadowOverlayObjects.Count; i++)
            {
                if (_waterShadowOverlayObjects[i] != null)
                    Destroy(_waterShadowOverlayObjects[i]);
            }
            _waterShadowOverlayObjects.Clear();
            _waterShadowOverlaySourceIds.Clear();
            if (_waterShadowOverlayMaterial != null)
                Destroy(_waterShadowOverlayMaterial);
            _waterShadowOverlayMaterial = null;
        }

        private void RestoreWaterMaterials()
        {
            for (int i = 0; i < _waterMaterials.Count; i++)
            {
                WaterMaterial water = _waterMaterials[i];
                if (water.Material == null)
                    continue;
                for (int j = 0; j < water.PropertyIds.Length; j++)
                    water.Material.SetColor(water.PropertyIds[j], water.Originals[j]);
                if (water.HasReflectionStrength)
                    water.Material.SetFloat("_ReflectionStrength", water.OriginalReflectionStrength);
            }
            RestoreWaterParticles();
        }

        // ---------------------------------------------------------------
        // Decor: ground flowers, grass tufts and other props whose
        // materials ignore the sun entirely and therefore glow at night.
        // Identified by mechanism (UTS light-color binding off, or unlit
        // shader families), never by object name.
        // ---------------------------------------------------------------
        // ---------------------------------------------------------------
        // Season theme: summer as authored, or an autumn repaint
        // ---------------------------------------------------------------
        // The amber wash that makes an autumn afternoon read as one. Multiplied into the grade
        // rather than replacing it, so every other colour control still does what it says.
        // Keyed off _appliedSeason rather than the config string: this runs every frame and the
        // string comparison allocates.
        private Color GetSeasonColorFilter(float dayWeight)
        {
            if (_appliedSeason != 1)
                return Color.white;
            float amount = Mathf.Clamp01(_autumnWarmFilter.Value) * Mathf.Clamp01(dayWeight);
            if (amount <= 0.001f)
                return Color.white;
            return Color.Lerp(Color.white, AutumnWarmFilter, amount);
        }

        private bool IsAutumnTheme()
        {
            string value = _seasonTheme.Value;
            if (value == null)
                return false;
            value = value.Trim();
            return value.Equals("Autumn", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("秋", StringComparison.Ordinal) ||
                value.Equals("秋季", StringComparison.Ordinal);
        }

        // Runs from the main apply pass, just before the decor and toon passes, because both of
        // those read the colours that are written here: the decor dimmer captures a material's
        // colour once and then drives it every frame from that capture, and the toon pass
        // derives its shade bands from the live base colour. Restoring first and re-writing from
        // the stored originals every time keeps this idempotent, so repeated F11 tweaks cannot
        // compound the shift.
        private void ApplySeasonTheme()
        {
            bool autumn = IsAutumnTheme();
            int desired = autumn ? 1 : 0;

            // The decor dimmer's captured "original" colours belong to the season they were
            // taken in. Drop them so the pass that follows re-captures the new ones, otherwise
            // the first night would paint summer greens back over the autumn palette.
            if (_appliedSeason == 1 || desired == 1)
            {
                RestoreDecorMaterials();
                _decorMaterials.Clear();
                _decorSeen.Clear();
            }

            if (_appliedSeason != desired)
                ClearGroundLeaves();
            RestoreSeasonMaterials();
            float strength = Mathf.Clamp01(_autumnStrength.Value);
            int recolored = 0;
            if (autumn && strength > 0.001f)
                recolored = ApplyAutumnMaterials(strength);
            int canopies = ApplyAutumnCanopies(autumn && strength > 0.001f, strength);
            int emitters = ApplyLeafFall(autumn);

            _appliedSeason = desired;
            int[] paletteCounts = new int[AutumnCanopyPalette.Length];
            for (int i = 0; i < _canopyClusters.Count; i++)
                paletteCounts[_canopyClusters[i].Palette]++;
            Logger.LogInfo("SEASON theme=" + (autumn ? "Autumn" : "Summer") +
                " strength=" + strength.ToString("F2") +
                " ground=" + (_autumnGroundRecolor.Value ? "on" : "off") +
                " materials=" + recolored + "/" + _seasonMaterials.Count +
                " canopyBatches=" + canopies + " trees=" + _canopyClusters.Count +
                " [gold=" + paletteCounts[0] + " amber=" + paletteCounts[1] +
                " pumpkin=" + paletteCounts[2] + " scarlet=" + paletteCounts[3] + "]" +
                " leafEmitters=" + emitters);
        }

        // Materials are matched by name because the objects that carry them are not reliable
        // handles: the round canopies, bushes, grass tufts and string lights all have their
        // renderers switched off by the game's own instancing managers and are redrawn from
        // matrices, so a renderer walk reaches the ones nothing draws. FindObjectsOfTypeAll
        // reaches the material assets themselves, whether or not any live renderer points at
        // them.
        private int ApplyAutumnMaterials(float strength)
        {
            bool ground = _autumnGroundRecolor.Value;
            Material[] loaded = Resources.FindObjectsOfTypeAll<Material>();
            int written = 0;
            for (int i = 0; i < loaded.Length; i++)
            {
                Material material = loaded[i];
                if (material == null || material.shader == null)
                    continue;
                SeasonRecolor recolor = FindSeasonRecolor(material.name);
                if (recolor == null)
                    continue;
                SeasonMaterialState state = CaptureSeasonMaterial(material, recolor);
                if (state == null || (recolor.Ground && !ground))
                    continue;
                for (int p = 0; p < state.PropertyIds.Length; p++)
                {
                    Color autumnColor = recolor.Absolute
                        ? Color.Lerp(state.Originals[p], ToSceneColor(recolor.Target),
                            Mathf.Clamp01(strength))
                        : AutumnShift(state.Originals[p], recolor.Hue,
                            recolor.SaturationScale, recolor.ValueScale, strength);
                    material.SetColor(state.PropertyIds[p], autumnColor);
                }
                written++;
            }
            return written;
        }

        private static SeasonRecolor FindSeasonRecolor(string materialName)
        {
            if (string.IsNullOrEmpty(materialName))
                return null;
            for (int i = 0; i < AutumnRecolors.Length; i++)
            {
                string target = AutumnRecolors[i].Material;
                // Runtime copies come through as "Name (Instance)", so accept that suffix but
                // nothing else - "Green 10" must not swallow "Green 10 2", which is a different
                // material on different objects.
                if (materialName == target ||
                    (materialName.Length > target.Length + 1 &&
                     materialName.StartsWith(target + " (", StringComparison.Ordinal)))
                    return AutumnRecolors[i];
            }
            return null;
        }

        private SeasonMaterialState CaptureSeasonMaterial(Material material, SeasonRecolor recolor)
        {
            if (_seasonSeen.Contains(material))
            {
                for (int i = 0; i < _seasonMaterials.Count; i++)
                {
                    if (_seasonMaterials[i].Material == material)
                        return _seasonMaterials[i];
                }
                return null;
            }

            List<int> ids = new List<int>();
            List<string> names = new List<string>();
            List<Color> originals = new List<Color>();
            for (int p = 0; p < recolor.Properties.Length; p++)
            {
                string property = recolor.Properties[p].Trim();
                if (property.Length == 0 || !material.HasProperty(property))
                    continue;
                ids.Add(Shader.PropertyToID(property));
                names.Add(property);
                originals.Add(material.GetColor(property));
            }
            if (ids.Count == 0)
            {
                Logger.LogWarning("SEASON material " + material.name + " (" + material.shader.name +
                    ") has none of the properties " + string.Join(",", recolor.Properties) +
                    "; left as authored.");
                return null;
            }

            SeasonMaterialState state = new SeasonMaterialState();
            state.Material = material;
            state.PropertyIds = ids.ToArray();
            state.PropertyNames = names.ToArray();
            state.Originals = originals.ToArray();
            _seasonSeen.Add(material);
            _seasonMaterials.Add(state);
            string summary = "";
            for (int i = 0; i < names.Count; i++)
                summary += (i > 0 ? "," : "") + names[i] + "=#" +
                    ColorUtility.ToHtmlStringRGB(originals[i]);
            Logger.LogInfo("SEASON captured " + material.name + " [" + material.shader.name + "] " + summary);
            return state;
        }

        private void RestoreSeasonMaterials()
        {
            for (int i = 0; i < _seasonMaterials.Count; i++)
            {
                SeasonMaterialState state = _seasonMaterials[i];
                if (state == null || state.Material == null)
                    continue;
                for (int p = 0; p < state.PropertyIds.Length; p++)
                    state.Material.SetColor(state.PropertyIds[p], state.Originals[p]);
            }
        }

        // Autumn as a hue plus scales rather than a literal colour. See the note on
        // SeasonRecolor: this keeps each material's authored brightness relationships and is
        // correct whether the value read back is linear or gamma.
        private static Color AutumnShift(Color source, float hue, float saturationScale,
            float valueScale, float strength)
        {
            float h, sat, val;
            Color.RGBToHSV(source, out h, out sat, out val);
            // A black or fully unsaturated slot has no hue to rotate; leave it exactly as it is
            // rather than inventing a colour the artist never put there.
            if (sat < 0.02f || val < 0.02f)
                return source;
            Color shifted = Color.HSVToRGB(Mathf.Repeat(hue, 1f),
                Mathf.Clamp(sat * saturationScale, 0f, AutumnMaxSaturation),
                Mathf.Clamp01(val * valueScale));
            shifted.a = source.a;
            return Color.Lerp(source, shifted, Mathf.Clamp01(strength));
        }

        // The round canopies (MD_TreeBase_01/02) are the island's signature planting, and their
        // colour is not on any material: BushInstancing hands the shader a per-instance colour
        // array, one entry per tree, and only the cap above _Threshold_1 uses it. The four
        // colours in that array are the four tree families - and because the arrays feed
        // RenderParams.matProps by reference, rewriting them is all it takes.
        // The round canopies (MD_TreeBase_01/02) are the island's signature planting, and their
        // colour is not on any material: BushInstancing hands the shader a per-instance colour
        // array, one entry per disc, and the cap above _Threshold_1 uses it. Because the arrays
        // feed RenderParams.matProps by reference, rewriting them is all it takes.
        //
        // One tree is several discs, and the game does not colour them consistently - measured
        // on the shipped scene, 41 of the island's ~65 trees carry more than one of the four
        // authored colours. Nobody notices in summer, because all four are near-identical
        // yellow-greens sitting above one shared green body. Mapping those four straight onto
        // four autumn colours turned single trees into gold-and-red confetti. So the discs are
        // grouped by where they stand and the colour is chosen per tree, not per disc.
        private int ApplyAutumnCanopies(bool autumn, float strength)
        {
            BushInstancing[] batches = UnityEngine.Object.FindObjectsByType<BushInstancing>(
                FindObjectsSortMode.None);
            // Nothing to do only when there is also nothing already captured: a restore has to
            // still run for batches captured before, even if the scene no longer reports any.
            if (batches.Length == 0 && _seasonCanopies.Count == 0)
                return 0;

            for (int b = 0; b < batches.Length; b++)
                CaptureCanopyBatch(batches[b]);
            BuildCanopyClusters();

            float shading = Mathf.Clamp01(_canopyShading.Value);
            float brightness = Mathf.InverseLerp(0.3f, 1f,
                Mathf.Clamp(_canopyShadeBrightness.Value, 0.3f, 1f));
            float lowerScale = Mathf.Lerp(CanopyLowerScaleDark, CanopyLowerScaleLight, brightness);
            float shadeLow = Mathf.Lerp(CanopyShadeLowDark, CanopyShadeLowLight, brightness);
            // The band's colour is written here rather than from the recolour table, so the
            // slider reaches it. The table still holds the material, which is what captures the
            // authored colour and puts it back.
            if (autumn)
            {
                Color band = ToSceneColor(Color.Lerp(CanopyBandDark, CanopyBandLight, brightness));
                for (int i = 0; i < _seasonCanopies.Count; i++)
                {
                    Material material = _seasonCanopies[i].Batch != null
                        ? _seasonCanopies[i].Batch.ToonInstanceMat : null;
                    if (material != null && material.HasProperty("_Color_3"))
                        material.SetColor("_Color_3", band);
                }
            }
            int applied = 0;
            for (int b = 0; b < _seasonCanopies.Count; b++)
            {
                SeasonCanopyState state = _seasonCanopies[b];
                if (state == null || state.Batch == null || state.Batch.MatProp == null)
                    continue;
                MaterialPropertyBlock block = state.Batch.MatProp;
                if (!autumn)
                {
                    block.SetVectorArray(CanopyColor1PropertyId, state.OriginalColor1);
                    block.SetVectorArray(CanopyColor2PropertyId, state.OriginalColor2);
                    block.SetFloatArray(CanopyThresholdPropertyId, state.OriginalThreshold1);
                    applied++;
                    continue;
                }

                int count = CanopyCount(state.Batch, state);
                Matrix4x4[] matrices = state.Batch.BushBatches.Matrices;
                Vector4[] color1 = (Vector4[])state.OriginalColor1.Clone();
                Vector4[] color2 = (Vector4[])state.OriginalColor2.Clone();
                float[] threshold = (float[])state.OriginalThreshold1.Clone();
                for (int i = 0; i < count; i++)
                {
                    int index = state.Cluster != null && i < state.Cluster.Length ? state.Cluster[i] : -1;
                    CanopyCluster cluster = index >= 0 && index < _canopyClusters.Count
                        ? _canopyClusters[index] : null;
                    int palette = cluster != null ? cluster.Palette : 0;

                    // How high this disc sits in its own tree, and a stable wobble on top so
                    // neighbouring discs are not identical.
                    float shade = 1f;
                    if (cluster != null && matrices != null && i < matrices.Length)
                    {
                        float discY = matrices[i].m13;
                        float span = cluster.TopY - cluster.BottomY;
                        float height = span > 0.01f
                            ? Mathf.Clamp01((discY - cluster.BottomY) / span) : 0.5f;
                        shade = Mathf.Lerp(shadeLow, CanopyShadeHigh, height);
                        shade *= 1f + (DiscJitter(b, i) * 2f - 1f) * CanopyShadeJitter;
                    }

                    Color tree = AutumnCanopyPalette[palette];
                    Color cap = ToSceneColor(new Color(tree.r * shade, tree.g * shade,
                        tree.b * shade, 1f));
                    float lowerShade = shade * lowerScale;
                    Color lower = ToSceneColor(new Color(tree.r * lowerShade, tree.g * lowerShade,
                        tree.b * lowerShade, 1f));
                    color1[i] = BlendCanopyColor(state.OriginalColor1[i], cap, strength);
                    color2[i] = BlendCanopyColor(state.OriginalColor2[i], lower, strength);
                    // Below this the shader drops to _Color_3, which is one colour for the whole
                    // island - so it is set to a deep shadow brown rather than to a tree colour.
                    // A dark band under every crown is what a canopy actually looks like, and it
                    // being shared reads as shadow instead of as a mismatched tree.
                    threshold[i] = Mathf.Lerp(0.02f, 0.55f, shading);
                }
                block.SetVectorArray(CanopyColor1PropertyId, color1);
                block.SetVectorArray(CanopyColor2PropertyId, color2);
                block.SetFloatArray(CanopyThresholdPropertyId, threshold);
                applied++;
            }
            return applied;
        }

        // Greedy horizontal grouping across every batch at once, because one tree can mix discs
        // from both meshes. Cheap enough at this size (about 370 discs) to redo on each apply.
        private void BuildCanopyClusters()
        {
            _canopyClusters.Clear();
            float radiusSquared = CanopyClusterRadius * CanopyClusterRadius;
            List<Vector3> sums = new List<Vector3>();
            for (int b = 0; b < _seasonCanopies.Count; b++)
            {
                SeasonCanopyState state = _seasonCanopies[b];
                if (state == null || state.Batch == null || state.Batch.BushBatches == null)
                    continue;
                Matrix4x4[] matrices = state.Batch.BushBatches.Matrices;
                if (matrices == null)
                    continue;
                int count = Mathf.Min(CanopyCount(state.Batch, state), matrices.Length);
                if (state.Cluster == null || state.Cluster.Length < count)
                    state.Cluster = new int[Mathf.Max(count, 1)];
                for (int i = 0; i < count; i++)
                {
                    Matrix4x4 matrix = matrices[i];
                    Vector3 position = matrix.GetColumn(3);
                    float discRadius = matrix.GetColumn(0).magnitude * 2f;
                    float discHeight = matrix.GetColumn(1).magnitude * 1.5f;
                    int found = -1;
                    for (int c = 0; c < _canopyClusters.Count; c++)
                    {
                        float dx = _canopyClusters[c].Centre.x - position.x;
                        float dz = _canopyClusters[c].Centre.z - position.z;
                        if (dx * dx + dz * dz < radiusSquared)
                        {
                            found = c;
                            break;
                        }
                    }
                    if (found < 0)
                    {
                        CanopyCluster cluster = new CanopyCluster();
                        cluster.Centre = position;
                        cluster.Radius = discRadius;
                        cluster.BottomY = position.y - discHeight;
                        cluster.TopY = position.y + discHeight;
                        cluster.Members = 1;
                        _canopyClusters.Add(cluster);
                        sums.Add(position);
                        found = _canopyClusters.Count - 1;
                    }
                    else
                    {
                        CanopyCluster cluster = _canopyClusters[found];
                        cluster.Members++;
                        sums[found] = sums[found] + position;
                        cluster.Centre = sums[found] / cluster.Members;
                        cluster.Radius = Mathf.Max(cluster.Radius, discRadius);
                        cluster.BottomY = Mathf.Min(cluster.BottomY, position.y - discHeight);
                        cluster.TopY = Mathf.Max(cluster.TopY, position.y + discHeight);
                    }
                    state.Cluster[i] = found;
                }
            }

            // Colour by position, not by index: a deterministic hash of where the tree stands
            // keeps the mix stable across sessions and stops neighbours from marching through
            // the palette in a visible stripe.
            for (int c = 0; c < _canopyClusters.Count; c++)
            {
                CanopyCluster cluster = _canopyClusters[c];
                int hx = Mathf.RoundToInt(cluster.Centre.x * 0.37f);
                int hz = Mathf.RoundToInt(cluster.Centre.z * 0.37f);
                int hash = (hx * 73856093) ^ (hz * 19349663);
                if (hash < 0)
                    hash = -(hash + 1);
                cluster.Palette = AutumnCanopyWeights[hash % AutumnCanopyWeights.Length];
            }
        }

        // Stable 0-1 wobble per disc. Deterministic, so a crown looks the same every session.
        private static float DiscJitter(int batch, int index)
        {
            int hash = (batch * 6151 + index * 31337) ^ 0x5f3759df;
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash ^= hash >> 16;
            return (hash & 0xFFFF) / 65535f;
        }

        private static Vector4 BlendCanopyColor(Vector4 source, Color target, float strength)
        {
            Color original = new Color(source.x, source.y, source.z, source.w);
            Color blended = Color.Lerp(original, target, Mathf.Clamp01(strength));
            return new Vector4(blended.r, blended.g, blended.b, source.w);
        }

        // Colours written from script are handed to the GPU unconverted, so in a Linear project
        // an sRGB literal comes out pale and washed. Everything authored in this file is written
        // as sRGB and converted here. (Material colours read back with GetColor are already
        // linear, which is why the hue-shift path never needs this.)
        private static Color ToSceneColor(Color authoredSrgb)
        {
            return QualitySettings.activeColorSpace == ColorSpace.Linear
                ? authoredSrgb.linear : authoredSrgb;
        }

        private static int CanopyCount(BushInstancing batch, SeasonCanopyState state)
        {
            int count = batch.BushBatches != null ? batch.BushBatches.Count : 0;
            count = Mathf.Min(count, state.OriginalColor1.Length);
            count = Mathf.Min(count, state.OriginalColor2.Length);
            return Mathf.Max(0, Mathf.Min(count, state.OriginalThreshold1.Length));
        }

        private SeasonCanopyState CaptureCanopyBatch(BushInstancing batch)
        {
            if (batch == null || batch.BushBatches == null || batch.MatProp == null)
                return null;
            for (int i = 0; i < _seasonCanopies.Count; i++)
            {
                if (_seasonCanopies[i].Batch == batch)
                    return _seasonCanopies[i];
            }
            BushBatches data = batch.BushBatches;
            if (data.Color1 == null || data.Color2 == null || data.Threshold1 == null)
                return null;
            SeasonCanopyState state = new SeasonCanopyState();
            state.Batch = batch;
            state.OriginalColor1 = (Vector4[])data.Color1.Clone();
            state.OriginalColor2 = (Vector4[])data.Color2.Clone();
            state.OriginalThreshold1 = (float[])data.Threshold1.Clone();
            _seasonCanopies.Add(state);
            Logger.LogInfo("SEASON canopy batch on " + batch.gameObject.name + " discs=" + data.Count +
                " mesh=" + (batch.BushMesh != null ? batch.BushMesh.name : "<none>") +
                " material=" + (batch.ToonInstanceMat != null ? batch.ToonInstanceMat.name : "<none>"));
            return state;
        }

        // True while the scene still holds season work this plugin has not been able to do -
        // the canopies or the leaf emitters were not loaded yet when the apply pass ran. Capped
        // so a scene that genuinely has neither (the menu, a small area) stops being rescanned.
        private bool SeasonNeedsRescan()
        {
            if (_seasonRescanAttempts >= 12)
                return false;
            if (_appliedSeason != (IsAutumnTheme() ? 1 : 0))
                return true;
            // The canopies and their shadow stand-ins are not seasonal, so this has to be checked
            // in summer too.
            if (_canopyClusters.Count == 0)
                return true;
            if (_dappledCanopyShadows.Value && _adjustSun.Value && _canopyShadowObjects.Count == 0)
                return true;
            if (!IsAutumnTheme())
                return false;
            return _leafEmitters.Count == 0;
        }

        // ---------------------------------------------------------------
        // Dappled canopy shadows
        // ---------------------------------------------------------------
        // A round canopy is a solid ellipsoid, so it lays down one filled oval of shadow - the
        // island ends up covered in flat dark discs. Nothing about the material can change that:
        // the shadow comes from the depth pass, and a hole in the shadow needs a hole in the
        // geometry that casts it.
        //
        // So the canopies stop casting (their RenderParams.shadowCastingMode is turned off by
        // reflection, the same handle ApplyShadowCasting already uses on the other instancing
        // managers) and a perforated stand-in is drawn into the shadow map instead, from the very
        // same instance matrices. The stand-in is a generated ellipsoid matching the canopy's
        // bounds with a fraction of its quads dropped. The holes are chosen from a smooth noise
        // field sampled on the horizontal plane only, so a hole in the top of the shell lines up
        // with a hole in the bottom and light actually passes through; and they are weighted
        // toward the rim, because the middle of a crown really is denser than its edge.
        private void ApplyCanopyShadows()
        {
            RestoreCanopyShadows();
            if (!_dappledCanopyShadows.Value || !_adjustSun.Value)
                return;
            if (_seasonCanopies.Count == 0 || _canopyClusters.Count == 0)
                return;
            if (!EnsureCanopyShadowMaterial())
                return;

            float gaps = Mathf.Clamp(_canopyShadowGaps.Value, 0f, 0.85f);
            EnsureDappleMasks(gaps);

            // Gather the discs of each tree, so one tree becomes one stand-in object with its own
            // bounds. A single merged mesh for the whole island would never be culled.
            List<List<Matrix4x4>> discs = new List<List<Matrix4x4>>();
            List<List<Bounds>> discBounds = new List<List<Bounds>>();
            List<List<int>> discPatterns = new List<List<int>>();
            for (int i = 0; i < _canopyClusters.Count; i++)
            {
                discs.Add(new List<Matrix4x4>());
                discBounds.Add(new List<Bounds>());
                discPatterns.Add(new List<int>());
            }

            int pattern = 0;
            for (int b = 0; b < _seasonCanopies.Count; b++)
            {
                SeasonCanopyState state = _seasonCanopies[b];
                if (state == null || state.Batch == null || state.Batch.BushMesh == null ||
                    state.Batch.BushBatches == null || state.Cluster == null)
                    continue;
                Matrix4x4[] matrices = state.Batch.BushBatches.Matrices;
                if (matrices == null)
                    continue;
                Bounds bounds = state.Batch.BushMesh.bounds;
                int count = Mathf.Min(CanopyCount(state.Batch, state),
                    Mathf.Min(matrices.Length, state.Cluster.Length));
                for (int i = 0; i < count; i++)
                {
                    int cluster = state.Cluster[i];
                    if (cluster < 0 || cluster >= discs.Count)
                        continue;
                    discs[cluster].Add(matrices[i]);
                    discBounds[cluster].Add(bounds);
                    discPatterns[cluster].Add(pattern++);
                }

                // Stop the canopy casting for itself. Same handle ApplyShadowCasting already uses
                // on the other instancing managers - the MeshRenderers are disabled, so nothing
                // reachable through Renderer.shadowCastingMode applies.
                FieldInfo field = typeof(BushInstancing).GetField("_renderParams",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null || field.FieldType != typeof(RenderParams))
                {
                    Logger.LogWarning("CANOPY SHADOW BushInstancing._renderParams not found; " +
                        "dappling unavailable.");
                    RestoreCanopyShadows();
                    return;
                }
                CanopyShadowState captured = new CanopyShadowState();
                captured.Batch = state.Batch;
                RenderParams parameters = (RenderParams)field.GetValue(state.Batch);
                captured.OriginalMode = parameters.shadowCastingMode;
                captured.Captured = true;
                parameters.shadowCastingMode = ShadowCastingMode.Off;
                field.SetValue(state.Batch, parameters);
                _canopyShadows.Add(captured);
            }

            int objects = 0;
            int triangles = 0;
            for (int c = 0; c < discs.Count; c++)
            {
                if (discs[c].Count == 0)
                    continue;
                Vector3 origin = _canopyClusters[c].Centre;
                Mesh mesh = BuildCanopyShadowMesh(discs[c], discBounds[c], discPatterns[c], origin);
                if (mesh == null)
                    continue;
                GameObject holder = new GameObject("LookCare Canopy Shadow");
                holder.transform.position = origin;
                holder.transform.rotation = Quaternion.identity;
                MeshFilter filter = holder.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = holder.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _canopyShadowMaterial;
                // Never drawn to the camera - it only exists to write the shadow map.
                renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                _canopyShadowObjects.Add(holder);
                _canopyShadowMeshes.Add(mesh);
                objects++;
                triangles += (int)(mesh.GetIndexCount(0) / 3);
            }

            Logger.LogInfo("CANOPY SHADOW dappled trees=" + objects + " batches=" + _canopyShadows.Count +
                " triangles=" + triangles + " gaps=" + gaps.ToString("F2") +
                " material=" + (_canopyShadowMaterial != null ? _canopyShadowMaterial.shader.name : "<none>"));
        }

        // Cloned from the material the canopies are already being drawn with, not built from a
        // shader looked up by name. A material made at runtime from Shader.Find gets the
        // all-features-off variant, and a build only ships the variants something in it actually
        // used - this project has nothing unlit casting shadows, so a fresh URP/Unlit had no
        // usable ShadowCaster pass and the stand-in wrote nothing at all, which took the canopy
        // shadows away entirely. The canopy's own material is casting shadows by definition.
        private bool EnsureCanopyShadowMaterial()
        {
            if (_canopyShadowMaterial != null)
                return true;
            Material donor = null;
            for (int i = 0; i < _seasonCanopies.Count; i++)
            {
                if (_seasonCanopies[i] != null && _seasonCanopies[i].Batch != null &&
                    _seasonCanopies[i].Batch.ToonInstanceMat != null)
                {
                    donor = _seasonCanopies[i].Batch.ToonInstanceMat;
                    break;
                }
            }
            if (donor == null)
            {
                Logger.LogWarning("CANOPY SHADOW no canopy material to clone; dappling unavailable.");
                return false;
            }
            _canopyShadowMaterial = new Material(donor);
            _canopyShadowMaterial.name = "LookCare Canopy Shadow";
            _canopyShadowMaterial.enableInstancing = false;
            // The canopy shader animates its vertices in the wind. The stand-in is a different
            // mesh, so it would sway out of step with the tree it belongs to.
            if (_canopyShadowMaterial.HasProperty("_WindStrength"))
                _canopyShadowMaterial.SetFloat("_WindStrength", 0f);
            if (_canopyShadowMaterial.HasProperty("_CastShadows"))
                _canopyShadowMaterial.SetFloat("_CastShadows", 1f);
            // A single-sided shell must not be culled away when its winding faces from the light.
            if (_canopyShadowMaterial.HasProperty("_Cull"))
                _canopyShadowMaterial.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            return true;
        }

        private void RestoreCanopyShadows()
        {
            for (int i = 0; i < _canopyShadows.Count; i++)
            {
                CanopyShadowState state = _canopyShadows[i];
                if (state.Batch == null || !state.Captured)
                    continue;
                FieldInfo field = typeof(BushInstancing).GetField("_renderParams",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null || field.FieldType != typeof(RenderParams))
                    continue;
                RenderParams parameters = (RenderParams)field.GetValue(state.Batch);
                parameters.shadowCastingMode = state.OriginalMode;
                field.SetValue(state.Batch, parameters);
            }
            _canopyShadows.Clear();
            for (int i = 0; i < _canopyShadowObjects.Count; i++)
            {
                if (_canopyShadowObjects[i] != null)
                    Destroy(_canopyShadowObjects[i]);
            }
            _canopyShadowObjects.Clear();
            for (int i = 0; i < _canopyShadowMeshes.Count; i++)
            {
                if (_canopyShadowMeshes[i] != null)
                    Destroy(_canopyShadowMeshes[i]);
            }
            _canopyShadowMeshes.Clear();
        }

        // One tree's worth of perforated shells, merged, in coordinates relative to the tree.
        private Mesh BuildCanopyShadowMesh(List<Matrix4x4> discs, List<Bounds> bounds,
            List<int> patterns, Vector3 origin)
        {
            const int grid = CanopyShadowGrid;
            const int stride = grid + 1;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int d = 0; d < discs.Count; d++)
            {
                bool[] mask = _canopyDappleMasks[patterns[d] % _canopyDappleMasks.Length];
                Matrix4x4 matrix = discs[d];
                Bounds discBounds = bounds[d];
                int baseIndex = vertices.Count;
                for (int z = 0; z <= grid; z++)
                {
                    for (int x = 0; x <= grid; x++)
                    {
                        float nx = ((float)x / grid) * 2f - 1f;
                        float nz = ((float)z / grid) * 2f - 1f;
                        float flat = nx * nx + nz * nz;
                        float ny = flat < 1f ? Mathf.Sqrt(1f - flat) : 0f;
                        Vector3 unit = new Vector3(nx, ny, nz);
                        Vector3 local = discBounds.center + Vector3.Scale(unit, discBounds.extents);
                        vertices.Add(matrix.MultiplyPoint3x4(local) - origin);
                        Vector3 normal = matrix.MultiplyVector(unit);
                        normals.Add(normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up);
                    }
                }
                for (int z = 0; z < grid; z++)
                {
                    for (int x = 0; x < grid; x++)
                    {
                        if (!mask[z * grid + x])
                            continue;
                        int a = baseIndex + z * stride + x;
                        int b = a + 1;
                        int c = a + stride;
                        int e = c + 1;
                        triangles.Add(a);
                        triangles.Add(c);
                        triangles.Add(b);
                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(e);
                    }
                }
            }

            if (triangles.Count == 0)
                return null;
            Mesh mesh = new Mesh();
            mesh.name = "LookCare Canopy Shadow";
            if (vertices.Count > 65000)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // A handful of hole patterns, reused across the island's ~370 discs. Building a noise
        // field per disc would be several hundred thousand operations for variation nobody can
        // see; eight is plenty for no two neighbouring crowns to match.
        private void EnsureDappleMasks(float gaps)
        {
            if (_canopyDappleMasks != null && Mathf.Abs(_canopyDappleMaskGaps - gaps) < 0.001f)
                return;
            _canopyDappleMasks = new bool[CanopyDapplePatterns][];
            for (int p = 0; p < CanopyDapplePatterns; p++)
                _canopyDappleMasks[p] = BuildDappleMask(gaps, p);
            _canopyDappleMaskGaps = gaps;
        }

        // Which cells of the horizontal grid keep their geometry.
        //
        // The grid is laid out on the horizontal plane rather than over a UV sphere, because a
        // sphere's quads project onto the ground wildly unevenly - the ones at its equator are
        // edge-on and cast almost nothing, while a handful near the pole cover the whole middle
        // of the shadow - so holes cut evenly over a sphere came out as one missing quarter. Here
        // one cell is one patch of shadow.
        private static bool[] BuildDappleMask(float gaps, int seed)
        {
            const int grid = CanopyShadowGrid;
            const int fineSize = 56;
            const int coarseSize = 8;
            float[,] coarse = BuildNoiseField(coarseSize, 2, seed * 7919 + 104729);
            float[,] fine = BuildNoiseField(fineSize, 1, seed * 6151 + 15485863);

            float[] weights = new float[grid * grid];
            bool[] inside = new bool[grid * grid];
            List<float> insideWeights = new List<float>();
            for (int z = 0; z < grid; z++)
            {
                for (int x = 0; x < grid; x++)
                {
                    float nx = ((x + 0.5f) / grid) * 2f - 1f;
                    float nz = ((z + 0.5f) / grid) * 2f - 1f;
                    float radius = Mathf.Sqrt(nx * nx + nz * nz);
                    int cell = z * grid + x;
                    inside[cell] = radius <= 1f;
                    if (!inside[cell])
                        continue;
                    float u = (nx + 1f) * 0.5f;
                    float v = (nz + 1f) * 0.5f;
                    // Almost all fine detail, with just enough of the coarse field to give the
                    // crown some large-scale thick-and-thin. Measured over six seeds: at an even
                    // mix the largest single connected hole was 49% of all the holes - one
                    // missing corner, not dapple. At this mix it is 20%.
                    float noise = SampleNoiseField(coarse, coarseSize, u, v) * 0.15f +
                        SampleNoiseField(fine, fineSize, u, v) * 0.85f;
                    // Divided rather than compared against a scaled threshold, so the bias
                    // survives the quantile: the middle of a crown is denser than its rim.
                    weights[cell] = noise / Mathf.Lerp(0.88f, 1.14f, Mathf.Clamp01(radius));
                    insideWeights.Add(weights[cell]);
                }
            }

            // Cut at a quantile rather than at an absolute value. Against a noise field a fixed
            // threshold is wildly unstable - measured, the same setting kept anywhere from 43% to
            // 65% of the shell depending on the seed, and sometimes erased the shadow outright.
            float threshold = float.MinValue;
            if (gaps > 0f && insideWeights.Count > 0)
            {
                float[] sorted = insideWeights.ToArray();
                Array.Sort(sorted);
                int cut = Mathf.Clamp(Mathf.RoundToInt(gaps * sorted.Length), 0, sorted.Length - 1);
                threshold = sorted[cut];
            }

            bool[] mask = new bool[grid * grid];
            for (int cell = 0; cell < mask.Length; cell++)
                mask[cell] = inside[cell] && weights[cell] >= threshold;
            return mask;
        }

        private static float[,] BuildNoiseField(int size, int smoothingPasses, int seed)
        {
            System.Random random = new System.Random(seed);
            float[,] field = new float[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    field[x, y] = (float)random.NextDouble();
            for (int pass = 0; pass < smoothingPasses; pass++)
            {
                float[,] blurred = new float[size, size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float total = 0f;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                total += field[(x + dx + size) % size, (y + dy + size) % size];
                        blurred[x, y] = total / 9f;
                    }
                }
                field = blurred;
            }
            return field;
        }

        private static float SampleNoiseField(float[,] field, int size, float u, float v)
        {
            float x = Mathf.Clamp01(u) * (size - 1);
            float y = Mathf.Clamp01(v) * (size - 1);
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = Mathf.Min(x0 + 1, size - 1);
            int y1 = Mathf.Min(y0 + 1, size - 1);
            float fx = x - x0;
            float fy = y - y0;
            float top = Mathf.Lerp(field[x0, y0], field[x1, y0], fx);
            float bottom = Mathf.Lerp(field[x0, y1], field[x1, y1], fx);
            return Mathf.Lerp(top, bottom, fy);
        }

        private void RestoreSeasonTheme()
        {
            RestoreSeasonMaterials();
            ApplyAutumnCanopies(false, 0f);
            RestoreLeafFall();
            ClearGroundLeaves();
            _appliedSeason = -1;
        }

        // ---------------------------------------------------------------
        // Autumn leaf fall
        // ---------------------------------------------------------------
        // The game already sheds leaves, but only from three fixed spots on the whole island,
        // and both of its leaf materials are one pointed almond silhouette tinted flat - the
        // texture is pure white with the shape in its alpha, so the colour is entirely the
        // material's. Autumn keeps those emitters and gives them a four-frame sheet instead:
        // two ginkgo fans in gold and butter, two maple leaves in orange and scarlet, each with
        // its colour baked into the atlas so the shape and the colour cannot come apart.
        private const int LeafTileSize = 128;
        private const float ClonedLeafBaseRate = 1.2f;
        private static readonly float[] MapleLobeAngles =
            new float[] { 0f, 0.95f, -0.95f, 2.30f, -2.30f };
        private static readonly float[] MapleLobeLengths =
            new float[] { 0.95f, 0.86f, 0.86f, 0.68f, 0.68f };
        // One tile per tree colour, in the same order as AutumnCanopyPalette, so an emitter can
        // simply hold its tree's palette index and drop the matching leaf. Slightly deeper than
        // the canopy: a leaf that has left the tree has dried a little.
        private const float LeafTileDarken = 0.92f;
        private static readonly Color[] LeafTileColors = new Color[]
        {
            new Color(0.949f * LeafTileDarken, 0.776f * LeafTileDarken, 0.235f * LeafTileDarken, 1f),
            new Color(0.929f * LeafTileDarken, 0.655f * LeafTileDarken, 0.200f * LeafTileDarken, 1f),
            new Color(0.910f * LeafTileDarken, 0.486f * LeafTileDarken, 0.180f * LeafTileDarken, 1f),
            new Color(0.847f * LeafTileDarken, 0.286f * LeafTileDarken, 0.173f * LeafTileDarken, 1f)
        };

        private int ApplyLeafFall(bool autumn)
        {
            RestoreLeafFall();
            if (!autumn)
                return 0;

            ParticleSystem[] systems = UnityEngine.Object.FindObjectsByType<ParticleSystem>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            List<ParticleSystem> sources = new List<ParticleSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
                ParticleSystem system = systems[i];
                if (system == null || system.gameObject.name.StartsWith("LookCare"))
                    continue;
                ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                if (renderer == null || renderer.sharedMaterial == null)
                    continue;
                if (!renderer.sharedMaterial.name.StartsWith("falling leaves",
                        StringComparison.OrdinalIgnoreCase))
                    continue;
                sources.Add(system);
            }
            if (sources.Count == 0)
            {
                Logger.LogInfo("SEASON no falling-leaf emitters found in this scene.");
                return 0;
            }

            if (!EnsureAutumnLeafMaterial(sources[0]))
                return 0;

            float rate = Mathf.Clamp(_leafFallRate.Value, 0f, 20f);
            for (int i = 0; i < sources.Count; i++)
            {
                // The game's own three sit at fixed spots. Give each of them the leaf of
                // whichever tree it stands under, so a gold tree never drops a scarlet leaf.
                int palette = NearestClusterPalette(sources[i].transform.position);
                TrackLeafEmitter(sources[i], rate, false, palette, 0f);
            }

            int copies = AddTreeLeafEmitters(sources[0], rate);
            Logger.LogInfo("SEASON leaf fall emitters=" + sources.Count + " copies=" + copies +
                " rate=x" + rate.ToString("F2") + " trees=" + _canopyClusters.Count);
            return _leafEmitters.Count;
        }

        // The game sheds leaves at three fixed spots on the whole island, which is why autumn
        // read as "a couple of trees are doing something". One emitter per tree instead, biggest
        // trees first, placed at the underside of the canopy rather than above it - leaves that
        // start above the crown look like they are being generated out of thin air.
        private int AddTreeLeafEmitters(ParticleSystem template, float rate)
        {
            int wanted = Mathf.Clamp(_leafFallTrees.Value, 0, 120);
            if (wanted <= 0 || template == null || _canopyClusters.Count == 0)
                return 0;

            List<CanopyCluster> ordered = new List<CanopyCluster>(_canopyClusters);
            ordered.Sort(delegate(CanopyCluster left, CanopyCluster right)
            {
                return right.Members.CompareTo(left.Members);
            });

            int added = 0;
            for (int i = 0; i < ordered.Count && added < wanted; i++)
            {
                CanopyCluster cluster = ordered[i];
                Vector3 spot = new Vector3(cluster.Centre.x, cluster.BottomY, cluster.Centre.z);
                GameObject clone = Instantiate(template.gameObject, spot,
                    template.transform.rotation, template.transform.parent);
                clone.name = "LookCare Autumn Leaf Fall";
                clone.SetActive(true);
                ParticleSystem system = clone.GetComponent<ParticleSystem>();
                if (system == null)
                {
                    Destroy(clone);
                    continue;
                }
                // The template's cone is 2.8 m across, which is a fraction of a full canopy.
                // Widening it to the tree makes the leaves come off the whole crown.
                TrackLeafEmitter(system, rate, true, cluster.Palette,
                    Mathf.Clamp(cluster.Radius, 2f, 16f));
                added++;
            }
            return added;
        }

        private int NearestClusterPalette(Vector3 position)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < _canopyClusters.Count; i++)
            {
                float dx = _canopyClusters[i].Centre.x - position.x;
                float dz = _canopyClusters[i].Centre.z - position.z;
                float distance = dx * dx + dz * dz;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }
            return best >= 0 ? _canopyClusters[best].Palette : 0;
        }

        private bool EnsureAutumnLeafMaterial(ParticleSystem template)
        {
            if (_autumnLeafMaterial != null)
                return true;
            ParticleSystemRenderer renderer = template.GetComponent<ParticleSystemRenderer>();
            Material source = renderer != null ? renderer.sharedMaterial : null;
            if (source == null)
                return false;
            if (_autumnLeafAtlas == null)
                _autumnLeafAtlas = CreateAutumnLeafAtlas();
            // Cloned from the game's own leaf material so the blend mode, render queue and
            // shader variant are ones this build definitely has, rather than a shader looked up
            // by name that may have been stripped.
            _autumnLeafMaterial = new Material(source);
            _autumnLeafMaterial.name = "LookCare Autumn Leaves";
            if (_autumnLeafMaterial.HasProperty("_BaseMap"))
                _autumnLeafMaterial.SetTexture("_BaseMap", _autumnLeafAtlas);
            if (_autumnLeafMaterial.HasProperty("_MainTex"))
                _autumnLeafMaterial.SetTexture("_MainTex", _autumnLeafAtlas);
            // The colour now lives in the atlas, so the tint has to stop multiplying one in.
            // White is white in either colour space, so this needs no conversion.
            if (_autumnLeafMaterial.HasProperty("_BaseColor"))
                _autumnLeafMaterial.SetColor("_BaseColor", Color.white);
            if (_autumnLeafMaterial.HasProperty("_Color"))
                _autumnLeafMaterial.SetColor("_Color", Color.white);
            return true;
        }

        private void TrackLeafEmitter(ParticleSystem system, float rate, bool cloned, int frame,
            float shapeRadius)
        {
            ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
                return;
            ParticleSystem.TextureSheetAnimationModule sheet = system.textureSheetAnimation;
            ParticleSystem.EmissionModule emission = system.emission;
            ParticleSystem.ShapeModule shape = system.shape;

            LeafEmitterState state = new LeafEmitterState();
            state.System = system;
            state.Renderer = renderer;
            state.Cloned = cloned;
            state.OriginalMaterial = renderer.sharedMaterial;
            state.OriginalRate = emission.rateOverTimeMultiplier;
            state.OriginalSheetEnabled = sheet.enabled;
            state.OriginalSheetMode = sheet.mode;
            state.OriginalTilesX = sheet.numTilesX;
            state.OriginalTilesY = sheet.numTilesY;
            state.OriginalAnimation = sheet.animation;
            state.OriginalFrameOverTime = sheet.frameOverTime;
            state.OriginalStartFrame = sheet.startFrame;
            state.OriginalCycleCount = sheet.cycleCount;
            state.OriginalShapeRadius = shape.radius;
            _leafEmitters.Add(state);

            // Copies do not inherit the template's rate. The three emitters the game ships run
            // at 1, 2 and 5 leaves a second, and a copy that picked up the 5 would put a couple
            // of hundred particles under every tree it was placed on. A copy is one tree quietly
            // shedding, so it gets its own modest base rate and the user's multiplier on top.
            emission.rateOverTimeMultiplier = (cloned ? ClonedLeafBaseRate : state.OriginalRate) * rate;
            if (shapeRadius > 0f)
            {
                shape.radius = shapeRadius;
                state.ShapeAdjusted = true;
            }
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Grid;
            sheet.numTilesX = 2;
            sheet.numTilesY = 2;
            sheet.animation = ParticleSystemAnimationType.WholeSheet;
            // Frame held, not animated: a leaf that walked through the sheet would change
            // species and colour as it fell. One tree drops one kind of leaf.
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            int clamped = Mathf.Clamp(frame, 0, LeafTileColors.Length - 1);
            sheet.startFrame = new ParticleSystem.MinMaxCurve(clamped, clamped + 0.999f);
            sheet.cycleCount = 1;
            renderer.sharedMaterial = _autumnLeafMaterial;
        }

        // ---------------------------------------------------------------
        // Leaves that have landed
        // ---------------------------------------------------------------
        private void UpdateGroundLeaves()
        {
            int limit = _groundLeafLimit != null ? Mathf.Clamp(_groundLeafLimit.Value, 0, 2000) : 0;
            if (_appliedSeason != 1 || limit <= 0 || _autumnLeafAtlas == null)
            {
                if (_groundLeaves.Count > 0)
                    ClearGroundLeaves();
                return;
            }

            float now = Time.unscaledTime;
            float lifetime = Mathf.Clamp(_groundLeafLifetime.Value, 0f, 3600f);

            if (lifetime > 0f)
            {
                for (int i = _groundLeaves.Count - 1; i >= 0; i--)
                {
                    if (now - _groundLeaves[i].BornAt >= lifetime)
                    {
                        _groundLeaves.RemoveAt(i);
                        _groundLeafDirty = true;
                    }
                }
            }
            else if (_groundLeaves.Count > limit)
            {
                // The limit was lowered from the menu; drop the oldest until it fits.
                _groundLeaves.RemoveRange(0, _groundLeaves.Count - limit);
                _groundLeafDirty = true;
            }

            if (now >= _nextGroundLeafSpawn && _groundLeaves.Count < limit)
            {
                _nextGroundLeafSpawn = now + 0.3f;
                int budget = Mathf.Min(4, limit - _groundLeaves.Count);
                for (int i = 0; i < budget; i++)
                {
                    if (TrySpawnGroundLeaf(now))
                        _groundLeafDirty = true;
                }
            }

            // A fading layer changes shape every frame, so it is rebuilt on a timer; a permanent
            // one only when a leaf was added or removed.
            if ((_groundLeafDirty || lifetime > 0f) && now >= _nextGroundLeafRebuild)
            {
                _nextGroundLeafRebuild = now + 0.25f;
                _groundLeafDirty = false;
                RebuildGroundLeafMesh(now, lifetime);
            }

            if (_groundLeafMaterial != null)
            {
                // Unlit, and named "LookCare", so the decor dimmer deliberately skips it - the
                // day cycle has to be applied here or the ground would glow at night.
                float lit = Mathf.Lerp(0.40f, 1f, Mathf.Clamp01(_lastDayWeight));
                _groundLeafMaterial.SetColor(BaseColorPropertyId, new Color(lit, lit, lit, 1f));
            }
        }

        private bool TrySpawnGroundLeaf(float now)
        {
            if (_canopyClusters.Count == 0)
                return false;
            Camera camera = _worldCamera != null ? _worldCamera : Camera.main;
            if (camera == null)
                return false;
            Vector3 viewer = camera.transform.position;

            CanopyCluster cluster = null;
            float rangeSquared = GroundLeafViewerRange * GroundLeafViewerRange;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                CanopyCluster candidate =
                    _canopyClusters[UnityEngine.Random.Range(0, _canopyClusters.Count)];
                float dx = candidate.Centre.x - viewer.x;
                float dz = candidate.Centre.z - viewer.z;
                if (dx * dx + dz * dz <= rangeSquared)
                {
                    cluster = candidate;
                    break;
                }
            }
            if (cluster == null)
                return false;

            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            // sqrt keeps the scatter even over the disc instead of crowding the middle.
            float radius = Mathf.Sqrt(UnityEngine.Random.value) * Mathf.Max(2.5f, cluster.Radius * 1.4f);
            Vector3 origin = new Vector3(
                cluster.Centre.x + Mathf.Cos(angle) * radius,
                cluster.BottomY + 2f,
                cluster.Centre.z + Mathf.Sin(angle) * radius);

            RaycastHit hit;
            if (!Physics.Raycast(origin, Vector3.down, out hit, 30f, GetGroundLeafRayMask(),
                    QueryTriggerInteraction.Ignore))
                return false;
            // A leaf does not stay on a cliff or a wall.
            if (hit.normal.y < 0.55f)
                return false;

            Quaternion rotation =
                Quaternion.AngleAxis(UnityEngine.Random.value * 360f, hit.normal) *
                Quaternion.FromToRotation(Vector3.up, hit.normal);
            float size = UnityEngine.Random.Range(0.10f, 0.16f);
            GroundLeaf leaf = new GroundLeaf();
            leaf.Position = hit.point + hit.normal * 0.02f;
            leaf.Right = rotation * Vector3.right * size;
            leaf.Forward = rotation * Vector3.forward * size;
            leaf.Frame = cluster.Palette;
            leaf.BornAt = now;
            _groundLeaves.Add(leaf);
            return true;
        }

        private int GetGroundLeafRayMask()
        {
            if (_groundLeafRayMaskReady)
                return _groundLeafRayMask;
            _groundLeafRayMaskReady = true;
            int mask = Physics.DefaultRaycastLayers;
            string[] excluded = new string[]
            {
                "Player", "DeskRender", "IgnorePlayer", "PlayerRender", "Water", "UI"
            };
            for (int i = 0; i < excluded.Length; i++)
            {
                int layer = LayerMask.NameToLayer(excluded[i]);
                if (layer >= 0)
                    mask &= ~(1 << layer);
            }
            _groundLeafRayMask = mask;
            return mask;
        }

        private void RebuildGroundLeafMesh(float now, float lifetime)
        {
            EnsureGroundLeafObject();
            if (_groundLeafMesh == null)
                return;

            _groundLeafVertices.Clear();
            _groundLeafUvs.Clear();
            _groundLeafTriangles.Clear();
            for (int i = 0; i < _groundLeaves.Count; i++)
            {
                GroundLeaf leaf = _groundLeaves[i];
                float scale = 1f;
                if (lifetime > 0f)
                {
                    float remaining = lifetime - (now - leaf.BornAt);
                    if (remaining < GroundLeafFadeSeconds)
                        scale = Mathf.Clamp01(remaining / GroundLeafFadeSeconds);
                    if (scale <= 0.02f)
                        continue;
                }
                Vector3 right = leaf.Right * scale;
                Vector3 forward = leaf.Forward * scale;
                int baseIndex = _groundLeafVertices.Count;
                _groundLeafVertices.Add(leaf.Position - right - forward);
                _groundLeafVertices.Add(leaf.Position + right - forward);
                _groundLeafVertices.Add(leaf.Position - right + forward);
                _groundLeafVertices.Add(leaf.Position + right + forward);
                // Same tile layout the atlas was written with: frame 0 top-left, then right,
                // then the bottom row. Texture rows run bottom-up.
                float u0 = (leaf.Frame % 2) * 0.5f;
                float v0 = leaf.Frame < 2 ? 0.5f : 0f;
                _groundLeafUvs.Add(new Vector2(u0, v0));
                _groundLeafUvs.Add(new Vector2(u0 + 0.5f, v0));
                _groundLeafUvs.Add(new Vector2(u0, v0 + 0.5f));
                _groundLeafUvs.Add(new Vector2(u0 + 0.5f, v0 + 0.5f));
                _groundLeafTriangles.Add(baseIndex);
                _groundLeafTriangles.Add(baseIndex + 2);
                _groundLeafTriangles.Add(baseIndex + 1);
                _groundLeafTriangles.Add(baseIndex + 1);
                _groundLeafTriangles.Add(baseIndex + 2);
                _groundLeafTriangles.Add(baseIndex + 3);
            }

            _groundLeafMesh.Clear();
            if (_groundLeafVertices.Count == 0)
                return;
            _groundLeafMesh.SetVertices(_groundLeafVertices);
            _groundLeafMesh.SetUVs(0, _groundLeafUvs);
            _groundLeafMesh.SetTriangles(_groundLeafTriangles, 0);
            _groundLeafMesh.RecalculateBounds();
        }

        private void EnsureGroundLeafObject()
        {
            if (_groundLeafObject != null || _autumnLeafAtlas == null)
                return;
            Shader shader = GetFaceTintShader();
            if (shader == null)
                return;

            _groundLeafMaterial = new Material(shader);
            _groundLeafMaterial.name = "LookCare Ground Leaves";
            _groundLeafMaterial.SetTexture("_BaseMap", _autumnLeafAtlas);
            _groundLeafMaterial.SetColor("_BaseColor", Color.white);
            _groundLeafMaterial.SetFloat("_Surface", 1f);
            _groundLeafMaterial.SetFloat("_Blend", 0f);
            _groundLeafMaterial.SetFloat("_AlphaClip", 0f);
            _groundLeafMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _groundLeafMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            PreserveDestinationAlpha(_groundLeafMaterial);
            _groundLeafMaterial.SetFloat("_ZWrite", 0f);
            _groundLeafMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _groundLeafMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _groundLeafMaterial.DisableKeyword("_ALPHATEST_ON");
            _groundLeafMaterial.SetShaderPassEnabled("ShadowCaster", false);
            // After everything opaque but before the water, so a leaf lies on the ground and is
            // still covered by the sea where the two meet.
            _groundLeafMaterial.renderQueue = 2900;

            _groundLeafObject = new GameObject("LookCare Ground Leaves");
            _groundLeafObject.transform.position = Vector3.zero;
            _groundLeafObject.transform.rotation = Quaternion.identity;
            _groundLeafMesh = new Mesh();
            _groundLeafMesh.name = "LookCare Ground Leaves";
            _groundLeafMesh.MarkDynamic();
            MeshFilter filter = _groundLeafObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _groundLeafMesh;
            _groundLeafRenderer = _groundLeafObject.AddComponent<MeshRenderer>();
            _groundLeafRenderer.sharedMaterial = _groundLeafMaterial;
            _groundLeafRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _groundLeafRenderer.receiveShadows = false;
            Logger.LogInfo("SEASON ground leaf layer created.");
        }

        private void ClearGroundLeaves()
        {
            _groundLeaves.Clear();
            _groundLeafDirty = false;
            if (_groundLeafMesh != null)
                _groundLeafMesh.Clear();
        }

        private void DestroyGroundLeaves()
        {
            ClearGroundLeaves();
            if (_groundLeafObject != null)
                Destroy(_groundLeafObject);
            if (_groundLeafMesh != null)
                Destroy(_groundLeafMesh);
            if (_groundLeafMaterial != null)
                Destroy(_groundLeafMaterial);
            _groundLeafObject = null;
            _groundLeafMesh = null;
            _groundLeafMaterial = null;
            _groundLeafRenderer = null;
        }

        private void RestoreLeafFall()
        {
            for (int i = 0; i < _leafEmitters.Count; i++)
            {
                LeafEmitterState state = _leafEmitters[i];
                if (state == null)
                    continue;
                if (state.Cloned)
                {
                    if (state.System != null)
                        Destroy(state.System.gameObject);
                    continue;
                }
                if (state.System == null)
                    continue;
                ParticleSystem.EmissionModule emission = state.System.emission;
                emission.rateOverTimeMultiplier = state.OriginalRate;
                ParticleSystem.TextureSheetAnimationModule sheet = state.System.textureSheetAnimation;
                sheet.numTilesX = state.OriginalTilesX;
                sheet.numTilesY = state.OriginalTilesY;
                sheet.mode = state.OriginalSheetMode;
                sheet.animation = state.OriginalAnimation;
                sheet.frameOverTime = state.OriginalFrameOverTime;
                sheet.startFrame = state.OriginalStartFrame;
                sheet.cycleCount = state.OriginalCycleCount;
                sheet.enabled = state.OriginalSheetEnabled;
                if (state.ShapeAdjusted)
                {
                    ParticleSystem.ShapeModule shape = state.System.shape;
                    shape.radius = state.OriginalShapeRadius;
                }
                if (state.Renderer != null && state.OriginalMaterial != null)
                    state.Renderer.sharedMaterial = state.OriginalMaterial;
            }
            _leafEmitters.Clear();
        }

        // Four tiles, laid out the way Unity's grid sheet numbers them: frame 0 top-left, then
        // right, then the bottom row. Texture rows run bottom-up, so the top row is written
        // last. RGB is a flat leaf colour everywhere in the tile and only the alpha carries the
        // shape - the same trick the game's own leaf texture uses, and the reason a filtered
        // edge never fringes toward black.
        private static Texture2D CreateAutumnLeafAtlas()
        {
            int tile = LeafTileSize;
            int size = tile * 2;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "LookCare Autumn Leaf Atlas";
            texture.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[size * size];
            for (int frame = 0; frame < 4; frame++)
            {
                int tileX = frame % 2;
                int tileY = frame < 2 ? 1 : 0;
                bool ginkgo = frame < 2;
                Color color = LeafTileColors[frame];
                for (int y = 0; y < tile; y++)
                {
                    for (int x = 0; x < tile; x++)
                    {
                        float coverage = 0f;
                        for (int sy = 0; sy < 3; sy++)
                        {
                            for (int sx = 0; sx < 3; sx++)
                            {
                                float u = ((x + (sx + 0.5f) / 3f) / tile) * 2f - 1f;
                                float v = ((y + (sy + 0.5f) / 3f) / tile) * 2f - 1f;
                                coverage += ginkgo ? GinkgoCoverage(u, v) : MapleCoverage(u, v);
                            }
                        }
                        coverage /= 9f;
                        int index = (tileY * tile + y) * size + tileX * tile + x;
                        pixels[index] = new Color32(
                            (byte)Mathf.RoundToInt(Mathf.Clamp01(color.r) * 255f),
                            (byte)Mathf.RoundToInt(Mathf.Clamp01(color.g) * 255f),
                            (byte)Mathf.RoundToInt(Mathf.Clamp01(color.b) * 255f),
                            (byte)Mathf.RoundToInt(Mathf.Clamp01(coverage) * 255f));
                    }
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        // A fan with its apex at the stalk and a notch cut into the middle of the outer edge,
        // which is the one silhouette everybody recognizes a ginkgo by.
        private static float GinkgoCoverage(float u, float v)
        {
            if (Mathf.Abs(u) < 0.055f && v > -0.92f && v < -0.45f)
                return 1f;
            float dy = v + 0.55f;
            if (dy <= 0f)
                return 0f;
            float radius = Mathf.Sqrt(u * u + dy * dy);
            float angle = Mathf.Atan2(u, dy);
            if (Mathf.Abs(angle) > 0.82f)
                return 0f;
            float notch = 0.28f * Mathf.Exp(-(angle * angle) / (2f * 0.10f * 0.10f));
            return radius <= 1.35f * (1f - notch) ? 1f : 0f;
        }

        // Five lobes on tapered arms around a small central blob, plus a stalk.
        private static float MapleCoverage(float u, float v)
        {
            const float centerY = -0.08f;
            if (Mathf.Abs(u) < 0.05f && v > -1f && v < centerY)
                return 1f;
            float px = u;
            float py = v - centerY;
            if (px * px + py * py < 0.17f * 0.17f)
                return 1f;
            for (int i = 0; i < MapleLobeAngles.Length; i++)
            {
                float length = MapleLobeLengths[i];
                float dx = Mathf.Sin(MapleLobeAngles[i]);
                float dy = Mathf.Cos(MapleLobeAngles[i]);
                float t = (px * dx + py * dy) / length;
                if (t < 0f || t > 1f)
                    continue;
                float ox = px - dx * length * t;
                float oy = py - dy * length * t;
                if (Mathf.Sqrt(ox * ox + oy * oy) <= 0.30f * (1f - 0.88f * t) + 0.015f)
                    return 1f;
            }
            return 0f;
        }

        private void ScanDecorMaterials()
        {
            if (!_decorDimming.Value)
            {
                RestoreDecorMaterials();
                return;
            }

            string[] excludes = (_decorExcludeKeywords.Value != null ? _decorExcludeKeywords.Value : "")
                .ToLowerInvariant().Split(',');
            for (int k = 0; k < excludes.Length; k++)
                excludes[k] = excludes[k].Trim();

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int added = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;
                if (renderer.GetComponentInParent<PlayerCustomizationController>() != null)
                    continue;
                // Particles (fire, sparkles) are supposed to glow; UI lives under canvases.
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer) &&
                    !(renderer is SpriteRenderer))
                    continue;
                if (renderer.GetComponentInParent<Canvas>() != null)
                    continue;

                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material == null || material.shader == null || _decorSeen.Contains(material) ||
                        _waterSeen.Contains(material) || _surfaceToonMaterials.Contains(material) ||
                        _basketCourtToonMaterials.Contains(material))
                        continue;
                    if (!IsLightIgnoringMaterial(material))
                        continue;

                    string materialName = material.name.ToLowerInvariant();
                    bool excluded = false;
                    for (int k = 0; k < excludes.Length; k++)
                    {
                        if (excludes[k].Length > 0 && materialName.IndexOf(excludes[k]) >= 0)
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded)
                        continue;

                    WaterMaterial entry = CaptureMaterialColors(material);
                    if (entry == null)
                        continue;
                    _decorSeen.Add(material);
                    _decorMaterials.Add(entry);
                    added++;
                    Logger.LogInfo("DECOR material=" + material.name + " shader=" +
                        material.shader.name + " sample=" + GetTransformPath(renderer.transform));
                }
            }
            if (added > 0)
                Logger.LogInfo("DECOR light-ignoring materials tracked: " + _decorMaterials.Count +
                    " (+" + added + " new). They now follow the day cycle; add name fragments to " +
                    "[Decor] Exclude keywords to keep specific ones glowing.");
        }

        private static bool IsLightIgnoringMaterial(Material material)
        {
            // UTS materials with the light-color binding off show their base color at full
            // brightness whatever the sun or moon does - these are the night-glowing props.
            if (material.HasProperty("_Is_LightColor_Base"))
                return material.GetFloat("_Is_LightColor_Base") < 0.5f;
            string shaderName = material.shader.name.ToLowerInvariant();
            // "Shader Graphs/TransparentShader" is the game's plain unlit transparent
            // graph (used for overlay decals like avatar face features); it has neither
            // "unlit" in its name nor any UTS light-binding property.
            return shaderName.IndexOf("unlit") >= 0 || shaderName.StartsWith("sprites/") ||
                shaderName == "shader graphs/transparentshader";
        }

        private static WaterMaterial CaptureMaterialColors(Material material)
        {
            Shader shader = material.shader;
            List<int> ids = new List<int>();
            List<string> names = new List<string>();
            List<Color> originals = new List<Color>();
            int count = shader.GetPropertyCount();
            for (int p = 0; p < count; p++)
            {
                if (shader.GetPropertyType(p) != UnityEngine.Rendering.ShaderPropertyType.Color)
                    continue;
                int id = shader.GetPropertyNameId(p);
                ids.Add(id);
                names.Add(shader.GetPropertyName(p));
                originals.Add(material.GetColor(id));
            }
            if (ids.Count == 0)
                return null;
            WaterMaterial entry = new WaterMaterial();
            entry.Material = material;
            entry.PropertyIds = ids.ToArray();
            entry.PropertyNames = names.ToArray();
            entry.Originals = originals.ToArray();
            return entry;
        }

        private void RestoreDecorMaterials()
        {
            for (int i = 0; i < _decorMaterials.Count; i++)
            {
                WaterMaterial decor = _decorMaterials[i];
                if (decor.Material == null)
                    continue;
                for (int j = 0; j < decor.PropertyIds.Length; j++)
                    decor.Material.SetColor(decor.PropertyIds[j], decor.Originals[j]);
            }
        }

        // ---------------------------------------------------------------
        // Toon shading: the game's UTS materials use shade colors nearly
        // identical to their base colors, so shadows are invisible however
        // strong the light's shadows are. Derive real shade colors instead.
        // ---------------------------------------------------------------
        private void ApplyToonShading()
        {
            if (!_toonShading.Value)
            {
                RestoreToonShading();
                return;
            }

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;
                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    if (IsNativeFeatureInstance(materials[m]))
                        continue;
                    TrackToonMaterial(materials[m]);
                }
            }

            float shade = Mathf.Clamp(_shadeDarkness.Value, 0.3f, 1f);
            float surfaceStep = GetAdaptiveSurfaceShadeStep(GetCurrentLightElevation());
            int surfaceMaterialsAdjusted = 0;
            int basketCourtMaterialsAdjusted = 0;
            for (int i = 0; i < _toonMaterials.Count; i++)
            {
                ToonMaterial entry = _toonMaterials[i];
                if (entry != null && _basketCourtToonMaterials.Contains(entry.Material))
                {
                    ApplyBasketCourtToonMaterial(entry, shade);
                    basketCourtMaterialsAdjusted++;
                }
                else if (entry != null && _surfaceToonMaterials.Contains(entry.Material))
                {
                    ApplySurfaceToonMaterial(entry, shade);
                    surfaceMaterialsAdjusted++;
                }
                else
                {
                    ApplyToonMaterial(entry, shade);
                }
            }
            Logger.LogInfo("TOON SHADING materials=" + _toonMaterials.Count + " shadeFactor=" + shade +
                " surfaceMaterials=" + surfaceMaterialsAdjusted +
                " basketCourtMaterials=" + basketCourtMaterialsAdjusted + " surfaceStep=" + surfaceStep +
                " surfaceStepRange=" + Mathf.Clamp(_surfaceShadeStep.Value, 0.05f, 0.8f) + "-" +
                Mathf.Clamp(_surfaceNoonShadeStep.Value, 0.05f, 0.8f) + " surfaceSystem=" +
                Mathf.Clamp(_surfaceSystemShadowLevel.Value, -0.5f, 0.5f));
        }

        private ToonMaterial TrackToonMaterial(Material material)
        {
            if (material == null || material.shader == null)
                return null;
            if (_toonSeen.Contains(material))
            {
                for (int i = 0; i < _toonMaterials.Count; i++)
                {
                    if (_toonMaterials[i].Material == material)
                        return _toonMaterials[i];
                }
                return null;
            }

            bool supportsSystemShadows = material.HasProperty("_Set_SystemShadowsToBase");
            bool supportsToonShadeColors = material.HasProperty("_1st_ShadeColor") ||
                material.HasProperty("_2nd_ShadeColor");
            if (material.shader.name != "Toon" && !supportsSystemShadows && !supportsToonShadeColors)
                return null;

            _toonSeen.Add(material);
            ToonMaterial entry = new ToonMaterial();
            entry.Material = material;
            if (supportsSystemShadows)
            {
                entry.HadSystemShadows = true;
                entry.OrigSystemShadows = material.GetFloat("_Set_SystemShadowsToBase");
            }
            if (material.HasProperty("_Tweak_SystemShadowsLevel"))
            {
                entry.HadSystemShadowLevel = true;
                entry.OrigSystemShadowLevel = material.GetFloat("_Tweak_SystemShadowsLevel");
            }
            if (material.HasProperty("_BaseColor_Step"))
            {
                entry.HadBaseColorStep = true;
                entry.OrigBaseColorStep = material.GetFloat("_BaseColor_Step");
            }
            if (material.HasProperty("_Use_BaseAs1st"))
            {
                entry.HadUseBaseAs1st = true;
                entry.OrigUseBaseAs1st = material.GetFloat("_Use_BaseAs1st");
            }
            if (material.HasProperty("_Use_1stAs2nd"))
            {
                entry.HadUse1stAs2nd = true;
                entry.OrigUse1stAs2nd = material.GetFloat("_Use_1stAs2nd");
            }
            if (material.HasProperty("_1st_ShadeColor"))
            {
                entry.Has1st = true;
                entry.Orig1st = material.GetColor("_1st_ShadeColor");
            }
            if (material.HasProperty("_2nd_ShadeColor"))
            {
                entry.Has2nd = true;
                entry.Orig2nd = material.GetColor("_2nd_ShadeColor");
            }
            _toonMaterials.Add(entry);
            return entry;
        }

        private static void ApplyToonMaterial(ToonMaterial entry, float shade)
        {
            if (entry == null || entry.Material == null)
                return;
            Material material = entry.Material;
            if (entry.HadSystemShadows)
                material.SetFloat("_Set_SystemShadowsToBase", 1f);
            if (!entry.Has1st)
                return;

            float shade2 = shade * 0.85f;
            Color baseColor = material.HasProperty("_BaseColor")
                ? material.GetColor("_BaseColor") : Color.white;
            material.SetColor("_1st_ShadeColor", new Color(
                baseColor.r * shade, baseColor.g * shade, baseColor.b * shade, entry.Orig1st.a));
            if (entry.Has2nd)
                material.SetColor("_2nd_ShadeColor", new Color(
                    baseColor.r * shade2, baseColor.g * shade2, baseColor.b * shade2, entry.Orig2nd.a));
        }

        private void ApplyPlayerToonMaterial(ToonMaterial entry, float shade)
        {
            ApplyToonMaterial(entry, shade);
            if (entry == null || entry.Material == null)
                return;

            Material material = entry.Material;
            if (entry.HadSystemShadowLevel)
                material.SetFloat("_Tweak_SystemShadowsLevel",
                    Mathf.Clamp(_playerSystemShadowLevel.Value, -0.5f, 0.5f));
            if (entry.HadBaseColorStep)
                material.SetFloat("_BaseColor_Step", Mathf.Clamp(_playerShadeStep.Value, 0.05f, 0.8f));
            // UTS normally permits separate untextured shade maps. Reusing BaseMap in both shade bands keeps
            // printed shirts, facial details and other avatar customization visible under real-time shadows.
            if (entry.HadUseBaseAs1st)
                material.SetFloat("_Use_BaseAs1st", 1f);
            if (entry.HadUse1stAs2nd)
                material.SetFloat("_Use_1stAs2nd", 1f);
            entry.PlayerShadowAdjusted = true;
        }

        private void ApplySurfaceToonMaterial(ToonMaterial entry, float shade)
        {
            ApplyToonMaterial(entry, shade);
            if (entry == null || entry.Material == null || !entry.HadBaseColorStep)
                return;

            // Grass is authored at -0.12 while the separate nature-path materials are authored at 0. Align
            // their shadow-map response so one physical cast shadow cannot stop at the material boundary.
            if (entry.HadSystemShadowLevel)
            {
                entry.Material.SetFloat("_Tweak_SystemShadowsLevel", Mathf.Min(entry.OrigSystemShadowLevel,
                    Mathf.Clamp(_surfaceSystemShadowLevel.Value, -0.5f, 0.5f)));
            }
            entry.Material.SetFloat("_BaseColor_Step", Mathf.Min(entry.OrigBaseColorStep,
                GetAdaptiveSurfaceShadeStep(GetCurrentLightElevation())));
            entry.SurfaceShadowAdjusted = true;
            if (!_surfaceToonEntries.Contains(entry))
                _surfaceToonEntries.Add(entry);
        }

        private void ApplyBasketCourtToonMaterial(ToonMaterial entry, float shade)
        {
            ApplyToonMaterial(entry, shade);
            if (entry == null || entry.Material == null)
                return;

            // Basket Court is authored with BaseColor_Step=0. The generic surface rule deliberately preserves
            // lower authored values, so the material never entered the system-shadow band and could not display
            // projections even with receiveShadows=true. It needs the same live threshold as the island surface.
            if (entry.HadSystemShadows)
                entry.Material.SetFloat("_Set_SystemShadowsToBase", 1f);
            if (entry.HadSystemShadowLevel)
                entry.Material.SetFloat("_Tweak_SystemShadowsLevel",
                    Mathf.Clamp(_surfaceSystemShadowLevel.Value, -0.5f, 0.5f));
            if (entry.HadBaseColorStep)
                entry.Material.SetFloat("_BaseColor_Step",
                    GetAdaptiveSurfaceShadeStep(GetCurrentLightElevation()));
            entry.SurfaceShadowAdjusted = true;
            if (!_surfaceToonEntries.Contains(entry))
                _surfaceToonEntries.Add(entry);
        }

        private float GetCurrentLightElevation()
        {
            if (_sunLight == null)
                return Mathf.Clamp(_sunElevation.Value, 5f, 90f);
            float elevation = Mathf.Repeat(_sunLight.transform.eulerAngles.x, 360f);
            if (elevation > 180f)
                elevation = 360f - elevation;
            if (elevation > 90f)
                elevation = 180f - elevation;
            return Mathf.Abs(elevation);
        }

        private float GetAdaptiveSurfaceShadeStep(float elevation)
        {
            float low = Mathf.Clamp(_surfaceShadeStep.Value, 0.05f, 0.8f);
            float high = Mathf.Clamp(_surfaceNoonShadeStep.Value, low, 0.8f);
            float maxElevation = Mathf.Clamp(_maxElevation.Value, 20f, 80f);
            float heightWeight = Mathf.InverseLerp(6f, maxElevation, Mathf.Abs(elevation));
            return Mathf.Lerp(low, high, heightWeight);
        }

        private void UpdateSurfaceToonThreshold(float elevation)
        {
            if (!_toonShading.Value || _surfaceToonEntries.Count == 0)
                return;
            float step = GetAdaptiveSurfaceShadeStep(elevation);
            for (int i = 0; i < _surfaceToonEntries.Count; i++)
            {
                ToonMaterial entry = _surfaceToonEntries[i];
                if (entry == null || entry.Material == null || !entry.HadBaseColorStep)
                    continue;
                entry.Material.SetFloat("_BaseColor_Step", _basketCourtToonMaterials.Contains(entry.Material)
                    ? step : Mathf.Min(entry.OrigBaseColorStep, step));
            }
        }

        private void RestoreToonShading()
        {
            for (int i = 0; i < _toonMaterials.Count; i++)
            {
                ToonMaterial entry = _toonMaterials[i];
                if (entry.Material == null)
                    continue;
                if (entry.HadSystemShadows)
                    entry.Material.SetFloat("_Set_SystemShadowsToBase", entry.OrigSystemShadows);
                if ((entry.PlayerShadowAdjusted || entry.SurfaceShadowAdjusted) && entry.HadSystemShadowLevel)
                    entry.Material.SetFloat("_Tweak_SystemShadowsLevel", entry.OrigSystemShadowLevel);
                if ((entry.PlayerShadowAdjusted || entry.SurfaceShadowAdjusted) && entry.HadBaseColorStep)
                    entry.Material.SetFloat("_BaseColor_Step", entry.OrigBaseColorStep);
                if (entry.PlayerShadowAdjusted && entry.HadUseBaseAs1st)
                    entry.Material.SetFloat("_Use_BaseAs1st", entry.OrigUseBaseAs1st);
                if (entry.PlayerShadowAdjusted && entry.HadUse1stAs2nd)
                    entry.Material.SetFloat("_Use_1stAs2nd", entry.OrigUse1stAs2nd);
                if (entry.Has1st)
                    entry.Material.SetColor("_1st_ShadeColor", entry.Orig1st);
                if (entry.Has2nd)
                    entry.Material.SetColor("_2nd_ShadeColor", entry.Orig2nd);
            }
            _toonMaterials.Clear();
            _surfaceToonEntries.Clear();
            _toonSeen.Clear();
            _surfaceToonMaterials.Clear();
            _basketCourtToonMaterials.Clear();
        }

        // ---------------------------------------------------------------
        // Lamp lights: warm point lights on lamp/lantern props at night
        // ---------------------------------------------------------------
        private void ApplyLamps()
        {
            ClearLamps();
            ApplyCampfireEmission();
            if (!_lampsEnabled.Value)
                return;

            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
            {
                Logger.LogInfo("ADDITIONAL LIGHTS mode=" + asset.additionalLightsRenderingMode +
                    " maxCount=" + asset.maxAdditionalLightsCount);
                if (asset.additionalLightsRenderingMode == LightRenderingMode.Disabled)
                {
                    FieldInfo modeField = typeof(UniversalRenderPipelineAsset).GetField(
                        "m_AdditionalLightsRenderingMode", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (modeField != null)
                    {
                        modeField.SetValue(asset, LightRenderingMode.PerPixel);
                        Logger.LogInfo("Additional lights were Disabled; switched to PerPixel (experimental).");
                    }
                }
                if (asset.maxAdditionalLightsCount < _maxLitLamps.Value)
                    asset.maxAdditionalLightsCount = _maxLitLamps.Value;
            }

            ScanForNewLamps(true);
            if (_deckStringLightsEnabled.Value)
                AddDeckStringLights(wantLight: true, wantHalo: true);
            AddSceneFeatureLights();
        }

        private void ScanForNewLamps(bool initialScan)
        {
            if (!_lampsEnabled.Value || _lampHolders.Count >= 256)
                return;

            string style = (_lampStyle.Value != null ? _lampStyle.Value : "Both").Trim().ToLowerInvariant();
            bool wantLight = style != "halo";
            bool wantHalo = style != "light";

            string[] keywords = (_lampKeywords.Value != null ? _lampKeywords.Value : "lamp,lantern")
                .ToLowerInvariant().Split(',');
            for (int k = 0; k < keywords.Length; k++)
                keywords[k] = keywords[k].Trim();

            int added = 0;
            Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.gameObject.name.StartsWith("LookCare") ||
                    HasExcludedLampAncestor(candidate))
                    continue;
                string name = candidate.gameObject.name.ToLowerInvariant();
                if (!MatchesLampName(name, keywords))
                    continue;

                // StreetLamps is the collection containing PR_StreetLamp 1..30,
                // not one enormous lamp. The old ancestor walk collapsed the
                // whole island collection into a single light.
                if (IsLampCollection(candidate, keywords))
                    continue;

                Transform root = candidate;
                while (root.parent != null &&
                    MatchesLampName(root.parent.gameObject.name.ToLowerInvariant(), keywords) &&
                    !IsLampCollection(root.parent, keywords))
                    root = root.parent;
                int rootId = root.GetInstanceID();
                if (_lampRootIds.Contains(rootId))
                    continue;
                _lampRootIds.Add(rootId);
                AddLampRoot(root, wantLight, wantHalo);
                added++;

                if (_lampHolders.Count >= 256)
                    break;
            }
            if (initialScan)
                Logger.LogInfo("LAMPS found=" + _lampHolders.Count + " style=" + style);
            else if (added > 0)
                Logger.LogInfo("LAMPS late-added=" + added + " total=" + _lampHolders.Count);
        }

        private void AddLampRoot(Transform root, bool wantLight, bool wantHalo)
        {
            Bounds bounds;
            if (!TryGetLampBounds(root, out bounds))
                bounds = new Bounds(root.position, new Vector3(1f, 3f, 1f));
            Vector3 headPosition;
            if (!TryGetLampHeadPosition(root, out headPosition))
            {
                float lift = Mathf.Min(Mathf.Max(0.4f, bounds.extents.y * 0.65f),
                    Mathf.Max(0.06f, bounds.extents.y * 0.9f));
                headPosition = new Vector3(bounds.center.x, bounds.center.y + lift, bounds.center.z);
            }
            GameObject holder = new GameObject("LookCare Lamp");
            holder.transform.position = headPosition;
            holder.transform.SetParent(root, true);
            _lampHolders.Add(holder);

            float sizeScale = Mathf.Clamp(bounds.extents.magnitude / 1.4f, 0.3f, 1f);
            if (wantLight)
            {
                Light light = holder.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = Mathf.Clamp(_lampRange.Value, 2f, 25f) * Mathf.Max(sizeScale, 0.45f);
                light.intensity = 0f;
                light.color = new Color(1f, 0.8f, 0.55f, 1f);
                light.shadows = LightShadows.None;
                _lampLights.Add(light);
            }
            if (wantHalo)
            {
                Transform halo = CreateHalo(holder.transform, sizeScale);
                if (halo != null)
                    _lampHalos.Add(halo);
            }
        }

        private void AddDeckStringLights(bool wantLight, bool wantHalo)
        {
            DeckLightInstancing[] batches = UnityEngine.Object.FindObjectsByType<DeckLightInstancing>(
                FindObjectsSortMode.None);
            int added = 0;
            for (int batchIndex = 0; batchIndex < batches.Length; batchIndex++)
            {
                DeckLightInstancing batch = batches[batchIndex];
                if (batch == null || batch.DeckLightBatches == null || batch.DeckLightBatches.Matrices == null)
                    continue;

                int count = Mathf.Min(batch.DeckLightBatches.Count, batch.DeckLightBatches.Matrices.Length);
                for (int i = 0; i < count && _lampHolders.Count < 256; i++)
                {
                    Matrix4x4 matrix = batch.DeckLightBatches.Matrices[i];
                    Vector3 position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
                    if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
                        continue;

                    GameObject holder = new GameObject("LookCare Deck String Light");
                    holder.transform.position = position;
                    holder.transform.SetParent(batch.transform, true);
                    _lampHolders.Add(holder);
                    float hue = Mathf.Repeat((i + batchIndex * 0.37f) * 0.127f, 1f);
                    if (wantLight)
                    {
                        Light light = holder.AddComponent<Light>();
                        light.type = LightType.Point;
                        light.range = Mathf.Clamp(_deckStringLightRange.Value, 2f, 12f);
                        light.intensity = 0f;
                        light.shadows = LightShadows.None;
                        _lampLights.Add(light);
                        _deckStringLightHues.Add(light, hue);
                    }
                    if (wantHalo)
                    {
                        Transform halo = CreateHalo(holder.transform, 0.42f);
                        if (halo != null)
                        {
                            _lampHalos.Add(halo);
                            Renderer renderer = halo.GetComponent<Renderer>();
                            if (renderer != null)
                                _deckStringHaloHues.Add(renderer, hue);
                        }
                    }
                    added++;
                }
            }
            Logger.LogInfo("DECK STRING LIGHTS bulbs=" + added + " batches=" + batches.Length);
        }

        // These are exact scene features established from the level data. The wicker lantern
        // rule uses its material/shape as a fallback too, so a second canteen copy with a
        // different object name is not silently missed.
        private void AddSceneFeatureLights()
        {
            int campfires = 0;
            int paperLanterns = 0;
            int libraryDeskLamps = 0;
            int playerDeskLamps = 0;
            int sunLamps = 0;

            if (_campfireLightsEnabled.Value)
            {
                Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform source = transforms[i];
                    if (source == null || source.gameObject.name != "CampFire" || IsLookCareTransform(source))
                        continue;
                    Bounds bounds;
                    Vector3 position = TryGetLampBounds(source, out bounds)
                        ? bounds.center + Vector3.up * Mathf.Clamp(bounds.extents.y * 0.35f, 0.18f, 0.65f)
                        : source.position + Vector3.up * 0.45f;
                    if (!TryClaimSceneFeatureLight(source))
                        continue;
                    AddSpecialLight(source, "LookCare Campfire Light", position,
                        new Color(1f, 0.42f, 0.16f, 1f), 8f, 0.38f, 2.35f, reserveBudget: true);
                    campfires++;
                }
            }

            if (_treeLanternLightsEnabled.Value)
            {
                Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer lantern = renderers[i];
                    if (lantern == null || lantern.gameObject.name.StartsWith("LookCare") ||
                        !IsWickerHangingLantern(lantern))
                        continue;
                    if (!TryClaimSceneFeatureLight(lantern.transform))
                        continue;
                    AddSpecialLight(lantern.transform, "LookCare Wicker Lantern", lantern.bounds.center,
                        new Color(1f, 0.84f, 0.66f, 1f), 4.0f, 0f, 1.05f, reserveBudget: false);
                    paperLanterns++;
                }
            }

            // These two lamp families are not generic scene lamps. The library has a named
            // cone mesh at each actual emitter, while the player's desk lamp is a separate
            // MD_Lamp_01 below Player_Desk. Keeping them on their own restrained profile
            // prevents the generic 9 m / 3.5 light from turning every table into a yellow pool.
            Renderer[] sceneRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < sceneRenderers.Length; i++)
            {
                Renderer cone = sceneRenderers[i];
                if (cone == null || cone.gameObject.name != "MD_TableLightCone" ||
                    !HasAncestorNamed(cone.transform, "PR_LibraryLamp"))
                    continue;
                if (!TryClaimSceneFeatureLight(cone.transform))
                    continue;
                AddDeskLampLight(cone.transform, "LookCare Library Desk Lamp", cone.bounds.center);
                libraryDeskLamps++;
            }

            Transform[] allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform source = allTransforms[i];
                if (source == null || source.gameObject.name != "MD_Lamp_01" ||
                    !HasAncestorNamed(source, "Player_Desk") || IsLookCareTransform(source))
                    continue;
                if (!TryClaimSceneFeatureLight(source))
                    continue;
                Bounds bounds;
                Vector3 position = TryGetLampBounds(source, out bounds)
                    ? bounds.center + Vector3.up * Mathf.Clamp(bounds.extents.y * 0.24f, 0.08f, 0.24f)
                    : source.position + Vector3.up * 0.16f;
                AddDeskLampLight(source, "LookCare Player Desk Lamp", position);
                playerDeskLamps++;
            }

            if (_playTowerSunLightEnabled.Value)
            {
                Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform source = transforms[i];
                    if (source == null || source.gameObject.name != "MD_PlayTower_PlanetSun" ||
                        IsLookCareTransform(source))
                        continue;
                    Bounds bounds;
                    Vector3 position = TryGetLampBounds(source, out bounds) ? bounds.center : source.position;
                    if (!TryClaimSceneFeatureLight(source))
                        continue;
                    AddSpecialLight(source, "LookCare Play Tower Sun Light", position,
                        new Color(1f, 0.70f, 0.28f, 1f), 5.5f, 0f, 1.35f, reserveBudget: false);
                    sunLamps++;
                }
            }

            int added = campfires + paperLanterns + libraryDeskLamps + playerDeskLamps + sunLamps;
            if (added > 0)
                Logger.LogInfo("SCENE FEATURE LIGHTS campfires=" + campfires + " paper-lanterns=" +
                    paperLanterns + " library-desk-lamps=" + libraryDeskLamps + " player-desk-lamps=" +
                    playerDeskLamps + " play-tower-suns=" + sunLamps);
        }

        private void AddDeskLampLight(Transform source, string holderName, Vector3 position)
        {
            // Ivory rather than saturated yellow; range is just enough to cover a desk surface.
            float nightIntensity = Mathf.Clamp(_deskLampIntensity.Value, 0f, 3f);
            AddSpecialLight(source, holderName, position, new Color(1f, 0.90f, 0.78f, 1f),
                Mathf.Clamp(_deskLampRange.Value, 1f, 8f), nightIntensity * 0.17f, nightIntensity,
                reserveBudget: false);
        }

        private bool TryClaimSceneFeatureLight(Transform source)
        {
            return source != null && _sceneFeatureLightSourceIds.Add(source.GetInstanceID());
        }

        private static bool IsWickerHangingLantern(Renderer renderer)
        {
            if (renderer.gameObject.name == "MD_THFiller")
                return true;
            // The visible lantern mesh is a short woven cylinder. Restrict the fallback to
            // the two verified feature roots and exclude the much larger wicker swing seats.
            if (!HasAncestorNamed(renderer.transform, "PR_Tree_01_06") &&
                !HasAncestorNamed(renderer.transform, "PR_BuildingCanteen"))
                return false;
            Material[] materials = renderer.sharedMaterials;
            bool wicker = false;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null && material.name.StartsWith("Wicker Texture 2", StringComparison.OrdinalIgnoreCase))
                {
                    wicker = true;
                    break;
                }
            }
            if (!wicker)
                return false;
            Bounds bounds = renderer.bounds;
            return bounds.size.x <= 0.60f && bounds.size.y <= 0.90f && bounds.size.z <= 0.60f;
        }

        private void AddSpecialLight(Transform parent, string holderName, Vector3 position, Color color,
            float range, float dayIntensity, float nightIntensity, bool reserveBudget)
        {
            GameObject holder = new GameObject(holderName);
            holder.transform.position = position;
            holder.transform.SetParent(GetNearestActiveAncestor(parent), true);
            _lampHolders.Add(holder);
            Light light = holder.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            _lampLights.Add(light);
            SpecialLocalLight state = new SpecialLocalLight();
            state.DayIntensity = dayIntensity;
            state.NightIntensity = nightIntensity;
            state.ReserveBudget = reserveBudget;
            _specialLocalLights.Add(light, state);
        }

        // The shipped Fire Red/Fire Yellow materials do expose emissive property names, but the
        // player build has no active emissive shader variant for them. Draw the exact native fire
        // mesh a second time with a very low-alpha URP Unlit material instead. It preserves the
        // original red/yellow texture and merely lifts it at night; it never replaces the flame
        // with a solid red silhouette or alters any shared game material.
        private void ApplyCampfireEmission()
        {
            RestoreCampfireEmission();
            if (!_campfireLightsEnabled.Value)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Logger.LogWarning("CAMPFIRE EMISSION unavailable: Universal Render Pipeline/Unlit was not loaded.");
                return;
            }

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int overlays = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || !HasAncestorNamed(renderer.transform, "CampFire") ||
                    (renderer.gameObject.name != "MD_Tent Fire 1" && renderer.gameObject.name != "MD_Tent Fire 2"))
                    continue;
                Material[] original = renderer.sharedMaterials;
                MeshFilter sourceFilter = renderer.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null || original.Length == 0)
                    continue;

                Material[] overlayMaterials = new Material[original.Length];
                bool[] visible = new bool[original.Length];
                bool hasVisibleFire = false;
                for (int m = 0; m < original.Length; m++)
                {
                    Material material = original[m];
                    string materialName = material != null ? material.name : "";
                    bool isFire = material != null &&
                        (materialName.IndexOf("Fire Red", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         materialName.IndexOf("Fire Yellow", StringComparison.OrdinalIgnoreCase) >= 0);
                    overlayMaterials[m] = CreateCampfireOverlayMaterial(shader, material, isFire);
                    visible[m] = isFire;
                    hasVisibleFire |= isFire;
                }
                if (!hasVisibleFire)
                {
                    for (int m = 0; m < overlayMaterials.Length; m++)
                        if (overlayMaterials[m] != null)
                            Destroy(overlayMaterials[m]);
                    continue;
                }

                GameObject overlay = new GameObject("LookCare Campfire Emissive Mesh");
                overlay.layer = renderer.gameObject.layer;
                overlay.transform.SetParent(renderer.transform, false);
                overlay.transform.localScale = Vector3.one * 1.003f;
                MeshFilter overlayFilter = overlay.AddComponent<MeshFilter>();
                overlayFilter.sharedMesh = sourceFilter.sharedMesh;
                MeshRenderer overlayRenderer = overlay.AddComponent<MeshRenderer>();
                overlayRenderer.sharedMaterials = overlayMaterials;
                overlayRenderer.shadowCastingMode = ShadowCastingMode.Off;
                overlayRenderer.receiveShadows = false;
                CampfireEmissionState state = new CampfireEmissionState();
                state.Overlay = overlay;
                state.Materials = overlayMaterials;
                state.Visible = visible;
                _campfireEmissionStates.Add(state);
                overlays++;
            }
            UpdateCampfireEmission();
            Logger.LogInfo("CAMPFIRE EMISSION overlays=" + overlays);
        }

        private static Material CreateCampfireOverlayMaterial(Shader shader, Material source, bool visible)
        {
            Material material = new Material(shader);
            material.name = visible
                ? "LookCare " + (source != null ? source.name : "Fire") + " Unlit Flame"
                : "LookCare Campfire Hidden Submesh";
            material.renderQueue = 3100;
            SetMaterialFloat(material, "_Surface", 1f);
            SetMaterialFloat(material, "_Blend", 0f);
            SetMaterialFloat(material, "_SrcBlend", 5f); // SrcAlpha
            SetMaterialFloat(material, "_DstBlend", 10f); // OneMinusSrcAlpha
            SetMaterialFloat(material, "_ZWrite", 0f);
            SetMaterialFloat(material, "_Cull", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            PreserveDestinationAlpha(material);
            Texture texture = GetCampfireSourceTexture(source);
            SetMaterialTexture(material, "_BaseMap", texture);
            SetMaterialTexture(material, "_MainTex", texture);
            SetMaterialColor(material, "_BaseColor", visible ? Color.white : new Color(0f, 0f, 0f, 0f));
            SetMaterialColor(material, "_Color", visible ? Color.white : new Color(0f, 0f, 0f, 0f));
            return material;
        }

        private static Texture GetCampfireSourceTexture(Material source)
        {
            if (source == null)
                return null;
            string[] properties = new string[]
            {
                "_BaseMap", "_Texture2D", "_BaseColorMap", "_MainTex"
            };
            for (int i = 0; i < properties.Length; i++)
            {
                if (!source.HasProperty(properties[i]))
                    continue;
                Texture texture = source.GetTexture(properties[i]);
                if (texture != null)
                    return texture;
            }
            return null;
        }

        private void UpdateCampfireEmission()
        {
            if (_campfireEmissionStates.Count == 0)
                return;
            float nightFactor = 1f - Mathf.Clamp01(_lastDayWeight);
            float flicker = 0.90f + 0.10f * Mathf.Sin(Time.unscaledTime * 7.3f);
            float intensity = Mathf.Lerp(0.78f, 1.15f, nightFactor) * flicker;
            for (int i = _campfireEmissionStates.Count - 1; i >= 0; i--)
            {
                CampfireEmissionState state = _campfireEmissionStates[i];
                if (state == null || state.Overlay == null)
                {
                    _campfireEmissionStates.RemoveAt(i);
                    continue;
                }
                for (int m = 0; m < state.Materials.Length; m++)
                {
                    Material material = state.Materials[m];
                    if (material == null || !state.Visible[m])
                        continue;
                    bool yellow = material.name.IndexOf("Yellow", StringComparison.OrdinalIgnoreCase) >= 0;
                    Color emission = yellow
                        ? new Color(1.0f, 0.86f, 0.56f, 0.48f) * intensity
                        : new Color(1.0f, 0.64f, 0.36f, 0.32f) * intensity;
                    emission.a = yellow ? 0.48f : 0.32f;
                    SetMaterialColor(material, "_BaseColor", emission);
                    SetMaterialColor(material, "_Color", emission);
                }
            }
        }

        private void RestoreCampfireEmission()
        {
            for (int i = 0; i < _campfireEmissionStates.Count; i++)
            {
                CampfireEmissionState state = _campfireEmissionStates[i];
                if (state == null)
                    continue;
                if (state.Overlay != null)
                    Destroy(state.Overlay);
                if (state.Materials == null)
                    continue;
                for (int m = 0; m < state.Materials.Length; m++)
                {
                    if (state.Materials[m] != null)
                        Destroy(state.Materials[m]);
                }
            }
            _campfireEmissionStates.Clear();
        }

        private static bool HasAncestorNamed(Transform transform, string ancestorName)
        {
            Transform current = transform != null ? transform.parent : null;
            while (current != null)
            {
                if (current.gameObject.name == ancestorName)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static Transform GetNearestActiveAncestor(Transform transform)
        {
            Transform current = transform;
            while (current != null && !current.gameObject.activeInHierarchy)
                current = current.parent;
            return current != null ? current : transform;
        }

        private static bool IsLampCollection(Transform transform, string[] keywords)
        {
            int matchingChildren = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (!MatchesLampName(transform.GetChild(i).gameObject.name.ToLowerInvariant(), keywords))
                    continue;
                matchingChildren++;
                if (matchingChildren > 1)
                    return true;
            }
            return false;
        }

        private static bool HasExcludedLampAncestor(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                string name = current.gameObject.name;
                if (name.ToLowerInvariant().IndexOf("lighthouse") >= 0)
                    return true;
                // These are handled below by AddSceneFeatureLights with a deliberately short,
                // ivory desk-light profile. Do not let the generic lamp matcher add a second
                // large, saturated yellow point light to the same table.
                if (name.StartsWith("PR_LibraryLamp", StringComparison.Ordinal) ||
                    (name == "MD_Lamp_01" && HasAncestorNamed(current, "Player_Desk")))
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool TryGetLampHeadPosition(Transform root, out Vector3 position)
        {
            position = Vector3.zero;
            // 1) The prop may carry an authored light child (e.g. SpotLight_LampPost):
            //    its transform marks the exact emitter position.
            Light[] lights = root.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.gameObject.name.StartsWith("LookCare"))
                    continue;
                position = light.transform.position;
                return true;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Renderer highest = null;
            int rendererCount = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;
                rendererCount++;
                if (highest == null || renderer.bounds.center.y > highest.bounds.center.y)
                    highest = renderer;
            }
            if (highest == null)
                return false;

            // 2) Multi-part lamp: the highest-centered renderer is the head/shade.
            if (rendererCount > 1)
            {
                position = highest.bounds.center;
                return true;
            }

            // 3) Single mesh: centroid of the top vertex band lands inside the shade even
            //    when it arches sideways off the base. Skip statically batched renderers -
            //    their shared mesh spans many unrelated objects.
            MeshFilter filter = highest.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null && filter.sharedMesh.isReadable &&
                !highest.isPartOfStaticBatch)
            {
                try
                {
                    Vector3[] vertices = filter.sharedMesh.vertices;
                    if (vertices.Length >= 8)
                    {
                        Transform meshTransform = highest.transform;
                        float minY = float.MaxValue;
                        float maxY = float.MinValue;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            float y = meshTransform.TransformPoint(vertices[i]).y;
                            if (y < minY)
                                minY = y;
                            if (y > maxY)
                                maxY = y;
                        }
                        float bandFloor = maxY - Mathf.Max(0.03f, (maxY - minY) * 0.28f);
                        Vector3 sum = Vector3.zero;
                        int bandCount = 0;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            Vector3 world = meshTransform.TransformPoint(vertices[i]);
                            if (world.y >= bandFloor)
                            {
                                sum += world;
                                bandCount++;
                            }
                        }
                        if (bandCount >= 3)
                        {
                            position = sum / bandCount;
                            return true;
                        }
                    }
                }
                catch
                {
                    // Non-readable mesh data despite the flag; fall through to the bounds path.
                }
            }
            return false;
        }

        private static bool TryGetLampBounds(Transform root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            bounds = new Bounds(root.position, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;
                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            return found;
        }

        private static bool MatchesLampName(string name, string[] keywords)
        {
            if (name.IndexOf("lighthouse") >= 0)
                return false;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i].Length > 0 && name.IndexOf(keywords[i]) >= 0)
                    return true;
            }
            return false;
        }

        private Transform CreateHalo(Transform parent, float sizeScale)
        {
            if (_haloMesh == null)
                _haloMesh = CreateQuadMesh();
            if (_haloMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    Logger.LogWarning("Sprites/Default shader missing; lamp halos unavailable.");
                    return null;
                }
                _haloMaterial = new Material(shader);
                _haloMaterial.mainTexture = CreateGlowTexture();
                _haloMaterial.color = new Color(1f, 0.86f, 0.62f, 0f);
            }

            GameObject halo = new GameObject("LookCare Halo");
            halo.transform.position = parent.position;
            halo.transform.SetParent(parent, true);
            halo.transform.localScale = Vector3.one *
                (Mathf.Clamp(_haloSize.Value, 0.2f, 5f) * sizeScale);
            MeshFilter filter = halo.AddComponent<MeshFilter>();
            filter.sharedMesh = _haloMesh;
            MeshRenderer renderer = halo.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _haloMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return halo.transform;
        }

        private static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "LookCare Halo Quad";
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f)
            };
            mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Texture2D CreateGlowTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size - 0.5f;
                    float dy = (y + 0.5f) / size - 0.5f;
                    float radial = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    float alpha = Mathf.Clamp01(1f - radial);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
                }
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

        private void UpdateLampSelection()
        {
            UpdateCampfireEmission();
            Camera camera = Camera.main;
            if (camera == null)
                return;

            // Fade lamps in through dusk; fully lit at deep night.
            _lampWeight = 1f - Mathf.Clamp01((_lastDayWeight - 0.15f) / 0.45f);
            _lampWeight = Mathf.Max(_lampWeight, _rainBlend * 0.55f);
            if (_haloMaterial != null)
                _haloMaterial.color = new Color(1f, 0.86f, 0.62f, 0.85f * _lampWeight);
            UpdateDeckStringLightColors();

            if (_lampLights.Count == 0)
                return;
            float normalLampIntensity = Mathf.Clamp(_lampIntensity.Value, 0f, 8f) * _lampWeight;
            int budget = Mathf.Clamp(_maxLitLamps.Value, 1, 256);
            Vector3 cameraPosition = camera.transform.position;

            for (int i = _lampLights.Count - 1; i >= 0; i--)
            {
                Light light = _lampLights[i];
                if (light == null)
                {
                    _lampLights.RemoveAt(i);
                    _deckStringLightHues.Remove(light);
                    _specialLocalLights.Remove(light);
                    continue;
                }
                light.intensity = 0f;
            }

            // Campfires reserve their two slots so "permanent" means both day/night and
            // independent of which ordinary lamp happens to be nearest to the camera.
            int availableBudget = budget;
            foreach (KeyValuePair<Light, SpecialLocalLight> pair in _specialLocalLights)
            {
                Light light = pair.Key;
                SpecialLocalLight state = pair.Value;
                if (light == null || state == null || !state.ReserveBudget || availableBudget <= 0)
                    continue;
                float specialIntensity = GetSpecialLightIntensity(state);
                if (specialIntensity <= 0.01f)
                    continue;
                light.intensity = specialIntensity;
                availableBudget--;
            }
            if (availableBudget <= 0)
                return;

            float[] bestDistances = new float[availableBudget];
            Light[] bestLights = new Light[availableBudget];
            for (int i = 0; i < availableBudget; i++)
                bestDistances[i] = float.MaxValue;

            for (int i = 0; i < _lampLights.Count; i++)
            {
                Light light = _lampLights[i];
                if (light == null)
                    continue;
                SpecialLocalLight special;
                if (_specialLocalLights.TryGetValue(light, out special) && special.ReserveBudget)
                    continue;
                float requestedIntensity = GetLightIntensity(light, normalLampIntensity);
                if (requestedIntensity <= 0.01f)
                    continue;

                float distance = (light.transform.position - cameraPosition).sqrMagnitude;
                for (int slot = 0; slot < availableBudget; slot++)
                {
                    if (distance < bestDistances[slot])
                    {
                        for (int shiftIndex = availableBudget - 1; shiftIndex > slot; shiftIndex--)
                        {
                            bestDistances[shiftIndex] = bestDistances[shiftIndex - 1];
                            bestLights[shiftIndex] = bestLights[shiftIndex - 1];
                        }
                        bestDistances[slot] = distance;
                        bestLights[slot] = light;
                        break;
                    }
                }
            }

            for (int i = 0; i < availableBudget; i++)
            {
                if (bestLights[i] != null)
                    bestLights[i].intensity = GetLightIntensity(bestLights[i], normalLampIntensity);
            }
        }

        private float GetLightIntensity(Light light, float normalLampIntensity)
        {
            SpecialLocalLight special;
            if (_specialLocalLights.TryGetValue(light, out special) && special != null)
                return GetSpecialLightIntensity(special);
            if (_deckStringLightHues.ContainsKey(light))
                return Mathf.Clamp(_deckStringLightIntensity.Value, 0f, 5f) * _lampWeight;
            return normalLampIntensity;
        }

        private float GetSpecialLightIntensity(SpecialLocalLight light)
        {
            return Mathf.Lerp(light.DayIntensity, light.NightIntensity, _lampWeight);
        }

        private void UpdateDeckStringLightColors()
        {
            if (_deckStringLightHues.Count == 0 && _deckStringHaloHues.Count == 0)
                return;

            float cycleSeconds = Mathf.Clamp(_deckStringColorCycleSeconds.Value, 4f, 90f);
            float cycle = Time.unscaledTime / cycleSeconds;
            // Resolved once for the whole pass: this runs over every bulb every frame, and the
            // theme lookup allocates.
            bool halloween = _halloweenStringLights.Value && IsAutumnTheme();
            Color[] palette = halloween ? DeckStringHalloweenColors : DeckStringPastelColors;
            foreach (KeyValuePair<Light, float> pair in _deckStringLightHues)
            {
                if (pair.Key != null)
                    pair.Key.color = GetDeckStringLightColor(palette, cycle + pair.Value, halloween);
            }
            foreach (KeyValuePair<Renderer, float> pair in _deckStringHaloHues)
            {
                if (pair.Key == null)
                    continue;
                Color color = GetDeckStringLightColor(palette, cycle + pair.Value, halloween);
                color.a = 0.9f * _lampWeight;
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", color);
                pair.Key.SetPropertyBlock(block);
            }
        }

        private static Color GetDeckStringLightColor(Color[] palette, float phase, bool step)
        {
            float wrapped = Mathf.Repeat(phase, 1f) * palette.Length;
            int from = Mathf.FloorToInt(wrapped) % palette.Length;
            if (step)
                return palette[from];
            int to = (from + 1) % palette.Length;
            float blend = Mathf.SmoothStep(0f, 1f, wrapped - Mathf.Floor(wrapped));
            return Color.Lerp(palette[from], palette[to], blend);
        }

        private void ClearLamps()
        {
            for (int i = 0; i < _lampHolders.Count; i++)
            {
                if (_lampHolders[i] != null)
                    Destroy(_lampHolders[i]);
            }
            _lampHolders.Clear();
            _lampLights.Clear();
            _lampHalos.Clear();
            _deckStringLightHues.Clear();
            _deckStringHaloHues.Clear();
            _specialLocalLights.Clear();
            _lampRootIds.Clear();
            _sceneFeatureLightSourceIds.Clear();
            _nextSceneFeatureLampRescan = 0f;
        }

        private void RestoreShadowCasting()
        {
            foreach (KeyValuePair<int, ShadowRendererState> pair in _shadowRendererStates)
            {
                ShadowRendererState state = pair.Value;
                if (state == null || state.Renderer == null)
                    continue;
                state.Renderer.shadowCastingMode = state.OriginalMode;
                state.Renderer.receiveShadows = state.OriginalReceiveShadows;
            }
            _shadowRendererStates.Clear();
        }

        private void RestoreSun()
        {
            if (_sunCaptured && _sunLight != null)
            {
                _sunLight.transform.rotation = _origSunRotation;
                _sunLight.color = _origSunColor;
                _sunLight.intensity = _origSunIntensity;
                _sunLight.shadowStrength = _origSunShadowStrength;
                _sunLight.shadows = _origSunShadows;
            }
            _sunCaptured = false;
        }

        // ---------------------------------------------------------------
        // Procedural moon: a separate billboard instead of reusing the sun
        // ---------------------------------------------------------------
        private void UpdateMoonPhaseState(bool forceTexture)
        {
            if (!_moonEnabled.Value)
            {
                _moonIllumination = 1f;
                return;
            }
            if (!forceTexture && Time.unscaledTime < _nextMoonPhaseUpdate)
                return;
            _nextMoonPhaseUpdate = Time.unscaledTime + 10f;

            double gameDayMinutes = Mathf.Clamp(_cycleMinutes.Value, 5f, 1440f);
            double lunarDays = Mathf.Clamp(_lunarCycleDays.Value, 2f, 64f);
            double totalMinutes = DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerMinute;
            double rawPhase = totalMinutes / (gameDayMinutes * lunarDays);
            _moonPhase = (float)(rawPhase - Math.Floor(rawPhase)); // 0=new, 0.5=full
            _moonIllumination = 0.5f - 0.5f * Mathf.Cos(_moonPhase * Mathf.PI * 2f);

            float phaseDelta = Mathf.Abs(_moonPhase - _lastMoonTexturePhase);
            if (_lastMoonTexturePhase >= 0f)
                phaseDelta = Mathf.Min(phaseDelta, 1f - phaseDelta);
            if (forceTexture || _moonTexture == null || phaseDelta >= 0.002f)
            {
                RebuildMoonTexture(_moonPhase);
                _lastMoonTexturePhase = _moonPhase;
            }
        }

        private void EnsureMoon()
        {
            if (!_moonEnabled.Value)
            {
                if (_moonObject != null)
                    _moonObject.SetActive(false);
                return;
            }
            if (_moonObject != null)
            {
                UpdateMoonPhaseState(false);
                return;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Logger.LogWarning("Sprites/Default shader missing; procedural moon unavailable.");
                return;
            }

            _moonMesh = CreateQuadMesh();
            _moonMesh.name = "LookCare Moon Quad";
            _moonMaterial = new Material(shader);
            _moonMaterial.name = "LookCare Moon Material";
            _moonMaterial.renderQueue = 3000;

            _moonObject = new GameObject("LookCare Moon");
            DontDestroyOnLoad(_moonObject);
            MeshFilter filter = _moonObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _moonMesh;
            MeshRenderer renderer = _moonObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _moonMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = -1000;
            _moonObject.SetActive(false);
            UpdateMoonPhaseState(true);
        }

        private void RebuildMoonTexture(float phase)
        {
            const int size = 192;
            if (_moonTexture == null)
            {
                _moonTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                _moonTexture.name = "LookCare Procedural Moon";
                _moonTexture.wrapMode = TextureWrapMode.Clamp;
                _moonTexture.filterMode = FilterMode.Bilinear;
            }

            Color[] pixels = new Color[size * size];
            float angle = phase * Mathf.PI * 2f;
            Vector3 lightDirection = new Vector3(Mathf.Sin(angle), 0f, -Mathf.Cos(angle));
            Color litColor = _moonColor.Value;
            Color darkColor = new Color(0.055f, 0.075f, 0.14f, 1f);
            Vector3[] craters = new Vector3[]
            {
                new Vector3(-0.24f, 0.22f, 0.13f), new Vector3(0.18f, 0.3f, 0.09f),
                new Vector3(0.27f, -0.12f, 0.15f), new Vector3(-0.08f, -0.27f, 0.1f),
                new Vector3(-0.34f, -0.04f, 0.07f), new Vector3(0.04f, 0.05f, 0.06f)
            };
            const float discRadius = 0.72f;

            for (int y = 0; y < size; y++)
            {
                float py = ((y + 0.5f) / size * 2f - 1f);
                for (int x = 0; x < size; x++)
                {
                    float px = ((x + 0.5f) / size * 2f - 1f);
                    float radius = Mathf.Sqrt(px * px + py * py);
                    int index = y * size + x;
                    if (radius <= discRadius)
                    {
                        float nx = px / discRadius;
                        float ny = py / discRadius;
                        float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - nx * nx - ny * ny));
                        float lightDot = nx * lightDirection.x + ny * lightDirection.y +
                            nz * lightDirection.z;
                        float lit = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.025f, 0.055f, lightDot));
                        float craterShade = 1f;
                        for (int c = 0; c < craters.Length; c++)
                        {
                            float dx = nx - craters[c].x;
                            float dy = ny - craters[c].y;
                            float distance = Mathf.Sqrt(dx * dx + dy * dy);
                            if (distance < craters[c].z)
                                craterShade *= Mathf.Lerp(0.72f, 1f, distance / craters[c].z);
                        }
                        float limb = 0.78f + 0.22f * nz;
                        Color bright = new Color(litColor.r * limb * craterShade,
                            litColor.g * limb * craterShade, litColor.b * limb * craterShade, 1f);
                        Color color = Color.Lerp(darkColor, bright, lit);
                        float edge = Mathf.Clamp01((discRadius - radius) / 0.018f);
                        color.a = edge * Mathf.Lerp(0.2f, 1f, lit);
                        pixels[index] = color;
                    }
                    else if (radius < 1f)
                    {
                        float halo = Mathf.Clamp01((1f - radius) / (1f - discRadius));
                        halo = halo * halo * halo * 0.13f * _moonIllumination;
                        pixels[index] = new Color(litColor.r, litColor.g, litColor.b, halo);
                    }
                    else
                    {
                        pixels[index] = Color.clear;
                    }
                }
            }
            _moonTexture.SetPixels(pixels);
            _moonTexture.Apply(false, false);
            if (_moonMaterial != null)
                _moonMaterial.mainTexture = _moonTexture;
        }

        private void UpdateMoonVisual()
        {
            if (!_moonEnabled.Value || RenderSettings.skybox != _skyMaterial || _sunLight == null)
            {
                if (_moonObject != null)
                    _moonObject.SetActive(false);
                return;
            }
            EnsureMoon();
            if (_moonObject == null || _moonMaterial == null)
                return;

            // Fade on the moon's own elevation, not on daylight. Keying it to dayWeight meant the
            // moon did not start to appear until the sun was nearly three degrees down and was not
            // fully opaque until eight - and at low opacity against a still-bright twilight sky it
            // stayed invisible, so by the time it could be seen it was already well up and seemed
            // to pop into existence in mid-air. It now rises from the horizon like the sun sets.
            float moonElevation = -_lastSignedElevation;
            float nightVisibility = _celestialValid
                ? Mathf.Clamp01((moonElevation + 2f) / 5f) * (1f - _rainBlend)
                : (1f - Mathf.Clamp01(_lastDayWeight * 3f)) * (1f - _rainBlend);
            if (nightVisibility <= 0.01f)
            {
                _moonObject.SetActive(false);
                return;
            }
            Camera camera = Camera.main;
            if (camera == null)
            {
                _moonObject.SetActive(false);
                return;
            }

            float distance = Mathf.Clamp(camera.farClipPlane * 0.72f, 80f, 500f);
            // The moon rises opposite the setting sun and must also stay below the
            // horizon mirror of the real sun elevation - not follow the floored light.
            Vector3 moonDirection = _celestialValid
                ? CelestialDirection(-_lastSignedElevation, _lastSunAzimuth)
                : -_sunLight.transform.forward;
            _moonObject.transform.position = camera.transform.position + moonDirection * distance;
            _moonObject.transform.rotation = camera.transform.rotation;
            float scale = distance * Mathf.Clamp(_moonSize.Value, 0.025f, 0.16f);
            _moonObject.transform.localScale = new Vector3(scale, scale, 1f);
            _moonMaterial.color = new Color(1f, 1f, 1f, nightVisibility);
            _moonObject.SetActive(true);
        }

        private void SetProceduralSunDisk(bool visible)
        {
            if (_skyMaterial == null)
                return;
            // Called every cycle tick. Shader keyword changes force a variant lookup, so only
            // touch the material when the state actually differs.
            int wanted = visible ? 1 : 0;
            if (_sunDiskState == wanted)
                return;
            _sunDiskState = wanted;
            _skyMaterial.DisableKeyword("_SUNDISK_NONE");
            _skyMaterial.DisableKeyword("_SUNDISK_SIMPLE");
            _skyMaterial.DisableKeyword("_SUNDISK_HIGH_QUALITY");
            if (visible)
            {
                _skyMaterial.SetFloat("_SunDisk", 2f);
                _skyMaterial.EnableKeyword("_SUNDISK_HIGH_QUALITY");
            }
            else
            {
                _skyMaterial.SetFloat("_SunDisk", 0f);
                _skyMaterial.EnableKeyword("_SUNDISK_NONE");
            }
        }

        // ---------------------------------------------------------------
        // Twilight variants: most dawns/dusks are orange, some roll pink
        // or deep crimson. One deterministic roll per half-day so a color
        // holds through its whole twilight and F4 reloads do not reroll.
        // ---------------------------------------------------------------
        private void UpdateTwilightVariant()
        {
            int halfDayId;
            if (_freezeTime.Value)
            {
                // Frozen time still advances the real clock, so the normal bucket would keep
                // rerolling the variant underneath a sun that is not moving. Derive the bucket
                // from the frozen phase instead: one fixed dusk and one fixed dawn.
                halfDayId = _lastCyclePhase < 0.5f ? 0 : 1;
            }
            else
            {
                double cycleMinutes = Mathf.Clamp(_cycleMinutes.Value, 5f, 1440f);
                double totalCycles = (DateTime.Now.Ticks / (double)TimeSpan.TicksPerMinute -
                    _noonMinute.Value) / cycleMinutes;
                // Half-day buckets flip at noon and midnight, so one bucket covers a whole
                // dusk (noon->midnight) or dawn (midnight->noon) without changing mid-event.
                halfDayId = (int)Math.Floor(totalCycles * 2.0);
            }
            int variantOverride = _twilightVariantOverride.Value;
            if (halfDayId == _lastTwilightId && variantOverride == _lastVariantOverride)
                return;
            _lastTwilightId = halfDayId;
            _lastVariantOverride = variantOverride;

            uint hash = (uint)halfDayId;
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            hash *= 2654435761u;
            float roll = (hash & 0xFFFFFF) / (float)0x1000000;

            // Even buckets run noon to midnight, so the sun is descending: that half-day's
            // twilight is a dusk. Odd buckets run midnight to noon and give a dawn.
            bool dusk = (((halfDayId % 2) + 2) % 2) == 0;

            // Mornings are the tamer ones. The dust and haze that make skies burn red build up
            // over the day and settle overnight, so dawn mostly comes up plain gold and rolls
            // the dramatic variants far less often than dusk does.
            float redChance = Mathf.Clamp01(_redDuskChance.Value) * (dusk ? 1f : 0.30f);
            float pinkChance = Mathf.Clamp01(_pinkDuskChance.Value) * (dusk ? 1f : 0.45f);
            if (variantOverride >= 1 && variantOverride <= 4)
            {
                // Forced from the menu for testing; skip the roll entirely.
                dusk = variantOverride != 2;
                roll = variantOverride == 4 ? -1f : (variantOverride == 3 ? 0f : 1f);
                redChance = variantOverride == 4 ? 1f : 0f;
                pinkChance = variantOverride == 3 ? 1f : 0f;
            }
            if (roll < redChance)
            {
                _twilightVariantName = "crimson";
                // Saturated on purpose. The previous values were pale enough that an "orange"
                // dusk landed on peach and read as pink, which made every twilight look alike.
                _twilightGlowColor = new Color(1f, 0.20f, 0.12f, 1f);
                _twilightSkyTint = new Color(0.74f, 0.25f, 0.18f, 1f);
                _twilightSunColor = new Color(1f, 0.48f, 0.32f, 1f);
            }
            else if (roll < redChance + pinkChance)
            {
                _twilightVariantName = "pink";
                _twilightGlowColor = new Color(1f, 0.38f, 0.56f, 1f);
                _twilightSkyTint = new Color(0.76f, 0.36f, 0.50f, 1f);
                _twilightSunColor = new Color(1f, 0.62f, 0.62f, 1f);
            }
            else if (dusk)
            {
                _twilightVariantName = "golden dusk";
                _twilightGlowColor = new Color(1f, 0.42f, 0.10f, 1f);
                _twilightSkyTint = new Color(0.82f, 0.44f, 0.18f, 1f);
                _twilightSunColor = new Color(1f, 0.70f, 0.38f, 1f);
            }
            else
            {
                // Yellow rather than orange: a clearly higher hue, so a morning never gets
                // mistaken for an evening.
                _twilightVariantName = "yellow dawn";
                _twilightGlowColor = new Color(1f, 0.75f, 0.22f, 1f);
                _twilightSkyTint = new Color(0.84f, 0.58f, 0.30f, 1f);
                _twilightSunColor = new Color(1f, 0.90f, 0.62f, 1f);
            }
            Logger.LogInfo("TWILIGHT halfDay=" + halfDayId + " " + (dusk ? "dusk" : "dawn") +
                " variant=" + _twilightVariantName + " roll=" + roll.ToString("0.00") +
                " redChance=" + redChance.ToString("0.00") +
                " pinkChance=" + pinkChance.ToString("0.00"));
        }

        // ---------------------------------------------------------------
        // Sunset glow: a soft billboard band on the horizon around the
        // sun. The procedural skybox alone cannot color one side of the
        // sky without tinting all of it, so the directional part of the
        // sunset comes from this band plus a small global exposure dip.
        // ---------------------------------------------------------------
        private void EnsureSunsetGlow()
        {
            if (_sunsetGlowObject != null)
                return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Logger.LogWarning("Sprites/Default shader missing; sunset glow unavailable.");
                return;
            }
            if (_sunsetGlowTexture == null)
                _sunsetGlowTexture = CreateGlowTexture();
            if (_haloMesh == null)
                _haloMesh = CreateQuadMesh();

            _sunsetGlowMaterial = new Material(shader);
            _sunsetGlowMaterial.name = "LookCare Sunset Glow";
            _sunsetGlowMaterial.mainTexture = _sunsetGlowTexture;
            _sunsetGlowMaterial.renderQueue = 2995;

            _sunsetGlowObject = new GameObject("LookCare Sunset Glow");
            DontDestroyOnLoad(_sunsetGlowObject);
            MeshFilter filter = _sunsetGlowObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _haloMesh;
            MeshRenderer renderer = _sunsetGlowObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _sunsetGlowMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = -1001;
            _sunsetGlowObject.SetActive(false);

            _duskShadeMaterial = new Material(shader);
            _duskShadeMaterial.name = "LookCare Dusk Shade";
            _duskShadeMaterial.mainTexture = _sunsetGlowTexture;
            _duskShadeMaterial.renderQueue = 2994;
            _duskShadeObject = new GameObject("LookCare Dusk Shade");
            DontDestroyOnLoad(_duskShadeObject);
            MeshFilter shadeFilter = _duskShadeObject.AddComponent<MeshFilter>();
            shadeFilter.sharedMesh = _haloMesh;
            MeshRenderer shadeRenderer = _duskShadeObject.AddComponent<MeshRenderer>();
            shadeRenderer.sharedMaterial = _duskShadeMaterial;
            shadeRenderer.shadowCastingMode = ShadowCastingMode.Off;
            shadeRenderer.receiveShadows = false;
            shadeRenderer.sortingOrder = -1002;
            _duskShadeObject.SetActive(false);
        }

        private void UpdateSunsetGlow(float signedElevation, float dayWeight)
        {
            float strength = 0f;
            if (_sunsetGlowEnabled.Value && _celestialValid && _skyApplied &&
                RenderSettings.skybox == _skyMaterial)
            {
                strength = TwilightGlowCurve(signedElevation) *
                    Mathf.Clamp(_sunsetGlowStrength.Value, 0f, 6f);
            strength *= 1f - _rainBlend;
            }
            Camera camera = Camera.main;
            if (strength <= 0.02f || camera == null)
            {
                if (_sunsetGlowObject != null)
                    _sunsetGlowObject.SetActive(false);
                if (_duskShadeObject != null)
                    _duskShadeObject.SetActive(false);
                return;
            }

            EnsureSunsetGlow();
            if (_sunsetGlowObject == null || _sunsetGlowMaterial == null)
                return;
            // The LIGHT flips to the moon side after sunset, so the glow must follow the
            // stored REAL sun azimuth instead of the light direction.
            Vector3 toSun = CelestialDirection(0f, _lastSunAzimuth + 180f);

            // Pulled in from 0.6 of the far plane: the band below is far wider now, and at the
            // old distance its corners fell outside the far clip plane and were cut off.
            float distance = Mathf.Clamp(camera.farClipPlane * 0.45f, 60f, 320f);
            Vector3 eye = camera.transform.position;
            _sunsetGlowObject.transform.position = eye + toSun * distance +
                Vector3.up * (distance * 0.10f);
            // Upright band facing back at the camera; a full billboard would tilt the
            // horizon line whenever the camera pitches.
            _sunsetGlowObject.transform.rotation = Quaternion.LookRotation(toSun, Vector3.up);
            // Roughly 110 degrees of horizon and 34 degrees of altitude. The previous 77 x 19
            // band was the "small patch near the sun" that was easy to miss entirely.
            // Opacity saturates at strength ~1.43, so past that the band keeps growing instead.
            // Height only: widening it as well would put the corners outside the far clip plane.
            float bandHeight = 1.15f * Mathf.Lerp(1f, 1.9f,
                Mathf.Clamp01((strength - 1.43f) / 4.57f));
            _sunsetGlowObject.transform.localScale =
                new Vector3(distance * 2.9f, distance * bandHeight, 1f);
            float bandOverdrive = Mathf.Max(1f, 0.70f * strength);
            _sunsetGlowMaterial.color = new Color(
                _twilightGlowColor.r * bandOverdrive, _twilightGlowColor.g * bandOverdrive,
                _twilightGlowColor.b * bandOverdrive, Mathf.Clamp01(0.70f * strength));
            _sunsetGlowObject.SetActive(true);

            // Anti-solar side: a deep blue-purple band so the sky opposite the sun visibly
            // darkens into night while the sun side burns.
            if (_duskShadeObject != null && _duskShadeMaterial != null)
            {
                _duskShadeObject.transform.position = eye - toSun * distance +
                    Vector3.up * (distance * 0.10f);
                _duskShadeObject.transform.rotation = Quaternion.LookRotation(-toSun, Vector3.up);
                _duskShadeObject.transform.localScale = new Vector3(distance * 2.9f, distance * 1.15f, 1f);
                _duskShadeMaterial.color = new Color(0.10f, 0.13f, 0.28f, 0.50f * Mathf.Clamp01(strength));
                _duskShadeObject.SetActive(true);
            }
        }

        // ---------------------------------------------------------------
        // Sun glitter: the specular track a low sun lays across the water,
        // running from the viewer out to the horizon. Material colors are
        // uniform over the whole sea, so this is the only part of the
        // sunset that varies with which way you are looking - without it
        // the dusk lives entirely in the sky and the sea reads as one flat
        // repainted surface.
        // ---------------------------------------------------------------
        private void EnsureSunGlitter()
        {
            if (_sunGlitterObject != null)
                return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return;
            if (_sunGlitterMesh == null)
                _sunGlitterMesh = BuildGlitterMesh();

            _sunGlitterMaterial = new Material(shader);
            _sunGlitterMaterial.name = "LookCare Sun Glitter";
            // Every UV sits on one white texel; the shape is entirely vertex colour.
            _sunGlitterMaterial.mainTexture = Texture2D.whiteTexture;
            // After the water (3000) and its shadow receiver (3100), but still depth-tested,
            // so island terrain and the boat deck occlude the path exactly as they should.
            _sunGlitterMaterial.renderQueue = 3150;

            _sunGlitterObject = new GameObject("LookCare Sun Glitter");
            DontDestroyOnLoad(_sunGlitterObject);
            MeshFilter filter = _sunGlitterObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _sunGlitterMesh;
            MeshRenderer renderer = _sunGlitterObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _sunGlitterMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            _sunGlitterObject.SetActive(false);
        }

        // Distance at which a horizontal ray leaves an axis-aligned box, ignoring height.
        // Bounds.IntersectRay reports where a ray enters, which is not what is wanted here.
        private static float HorizontalExitDistance(Vector3 origin, Vector3 direction, Bounds bounds)
        {
            float exit = float.MaxValue;
            if (Mathf.Abs(direction.x) > 0.00001f)
            {
                float side = direction.x > 0f ? bounds.max.x : bounds.min.x;
                float distance = (side - origin.x) / direction.x;
                if (distance > 0f)
                    exit = Mathf.Min(exit, distance);
            }
            if (Mathf.Abs(direction.z) > 0.00001f)
            {
                float side = direction.z > 0f ? bounds.max.z : bounds.min.z;
                float distance = (side - origin.z) / direction.z;
                if (distance > 0f)
                    exit = Mathf.Min(exit, distance);
            }
            return exit;
        }

        private void UpdateSunGlitter(float signedElevation, float twilightGlow)
        {
            float strength = 0f;
            if (_sunGlitterEnabled.Value && _celestialValid && _seaSurfaceValid &&
                _waterDaylight.Value)
            {
                strength = twilightGlow * Mathf.Clamp(_sunGlitterStrength.Value, 0f, 6f) *
                    (1f - _rainBlend);
                // Once the sun is well down there is nothing left to reflect; fade with the
                // afterglow instead of cutting the path off mid-frame.
                strength *= Mathf.Clamp01((signedElevation + 10f) / 8f);
            }
            Camera camera = Camera.main;
            if (strength <= 0.02f || camera == null)
            {
                if (_sunGlitterObject != null && _sunGlitterObject.activeSelf)
                    _sunGlitterObject.SetActive(false);
                return;
            }

            // The visible light flips to the moon side after sunset, so use the stored real
            // sun azimuth, the same way the horizon glow band does.
            Vector3 toSun = CelestialDirection(0f, _lastSunAzimuth + 180f);
            Vector3 flat = new Vector3(toSun.x, 0f, toSun.z);
            if (flat.sqrMagnitude < 0.0001f)
            {
                if (_sunGlitterObject != null && _sunGlitterObject.activeSelf)
                    _sunGlitterObject.SetActive(false);
                return;
            }
            flat.Normalize();

            EnsureSunGlitter();
            if (_sunGlitterObject == null || _sunGlitterMaterial == null || _sunGlitterMesh == null)
                return;

            Vector3 eye = camera.transform.position;
            float height = Mathf.Max(eye.y - _seaSurfaceY, 0.6f);
            // Cover the water to its farthest visible point, so no strip of bare sea is ever left
            // between the track and the sun. Whichever ends first wins: the far clip plane, or the
            // edge of the water mesh - a flat sea has no true horizon, so the edge of the mesh is
            // what actually reads as one.
            float clipLimit = camera.farClipPlane * 0.95f;
            float waterLimit = HorizontalExitDistance(eye, flat, _seaBounds);
            float maxDistance = Mathf.Min(clipLimit, waterLimit);
            if (maxDistance < 5f)
            {
                _sunGlitterObject.SetActive(false);
                return;
            }
            // Only fade the far end when the far clip plane cut it short. When the water itself is
            // what ends, the track has to run right off the edge - fading there would put back the
            // bare strip this exists to remove.
            bool fadeFarEnd = clipLimit < waterLimit;

            float coverage = Mathf.Clamp(_sunGlitterCoverage.Value, 3f, 89f) * Mathf.Deg2Rad;
            float spread = Mathf.Clamp(_sunGlitterSpread.Value, 0.5f, 89f) * Mathf.Deg2Rad;
            const float minimumSpread = 1.4f * Mathf.Deg2Rad;
            float nearDistance = Mathf.Clamp(height * 0.55f, 0.8f, maxDistance * 0.4f);
            float ratio = maxDistance / nearDistance;

            for (int ring = 0; ring < GlitterRings; ring++)
            {
                float step = ring / (float)(GlitterRings - 1);
                // Log spacing, because screen position goes as atan(height / distance): even
                // spacing piles every ring into the far sliver and leaves the near water bare.
                float distance = nearDistance * Mathf.Pow(ratio, step);
                float angleBelowHorizon = Mathf.Atan(height / distance);
                float down = Mathf.Clamp01(angleBelowHorizon / coverage);
                float alpha = Mathf.Pow(1f - down, 0.9f);
                if (fadeFarEnd)
                    alpha *= 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.88f, 1f, step));
                float halfAngle = Mathf.Max(minimumSpread, spread * Mathf.Pow(down, 0.65f));
                float halfWidth = distance * Mathf.Tan(halfAngle);
                for (int column = 0; column < GlitterColumns; column++)
                {
                    int index = ring * GlitterColumns + column;
                    _glitterVertices[index] = new Vector3(
                        GlitterColumnOffset[column] * halfWidth, 0f, distance);
                    _glitterColors[index] = new Color(1f, 1f, 1f,
                        alpha * GlitterColumnAlpha[column]);
                }
            }
            _sunGlitterMesh.vertices = _glitterVertices;
            _sunGlitterMesh.colors = _glitterColors;
            _sunGlitterMesh.RecalculateBounds();

            _sunGlitterObject.transform.position = new Vector3(eye.x, _seaSurfaceY + 0.05f, eye.z);
            // Local +Z runs at the sun, local +X across the track; the mesh lies in that plane.
            _sunGlitterObject.transform.rotation = Quaternion.LookRotation(flat, Vector3.up);
            _sunGlitterObject.transform.localScale = Vector3.one;
            Color glow = Color.Lerp(_twilightGlowColor, Color.white, 0.25f);
            // Alpha cannot exceed 1, so beyond that point strength has to keep going through
            // the colour instead - the render target is HDR, so it does keep brightening.
            float overdrive = Mathf.Max(1f, strength);
            _sunGlitterMaterial.color = new Color(glow.r * overdrive, glow.g * overdrive,
                glow.b * overdrive, Mathf.Clamp01(strength));
            _sunGlitterObject.SetActive(true);
        }

        private void HideCelestialVisuals()
        {
            if (_sunsetGlowObject != null && _sunsetGlowObject.activeSelf)
                _sunsetGlowObject.SetActive(false);
            if (_sunObject != null && _sunObject.activeSelf)
                _sunObject.SetActive(false);
            if (_sunGlitterObject != null && _sunGlitterObject.activeSelf)
                _sunGlitterObject.SetActive(false);
            if (_duskShadeObject != null && _duskShadeObject.activeSelf)
                _duskShadeObject.SetActive(false);
            if (_starObject != null && _starObject.activeSelf)
                _starObject.SetActive(false);
            if (_meteorObject != null && _meteorObject.activeSelf)
                _meteorObject.SetActive(false);
            _meteorActive = false;
        }

        private void DestroySunsetGlow()
        {
            if (_sunsetGlowObject != null)
                Destroy(_sunsetGlowObject);
            if (_sunsetGlowMaterial != null)
                Destroy(_sunsetGlowMaterial);
            if (_sunsetGlowTexture != null)
                Destroy(_sunsetGlowTexture);
            if (_duskShadeObject != null)
                Destroy(_duskShadeObject);
            if (_duskShadeMaterial != null)
                Destroy(_duskShadeMaterial);
            if (_sunGlitterObject != null)
                Destroy(_sunGlitterObject);
            if (_sunGlitterMaterial != null)
                Destroy(_sunGlitterMaterial);
            if (_sunGlitterMesh != null)
                Destroy(_sunGlitterMesh);
            if (_sunObject != null)
                Destroy(_sunObject);
            if (_sunMaterial != null)
                Destroy(_sunMaterial);
            if (_sunTexture != null)
                Destroy(_sunTexture);
            _sunObject = null;
            _sunMaterial = null;
            _sunTexture = null;
            _sunsetGlowObject = null;
            _sunsetGlowMaterial = null;
            _sunsetGlowTexture = null;
            _duskShadeObject = null;
            _duskShadeMaterial = null;
            _sunGlitterObject = null;
            _sunGlitterMaterial = null;
            _sunGlitterMesh = null;
        }

        // ---------------------------------------------------------------
        // Celestial direction helpers used by the moon, stars, and horizon glow.
        // ---------------------------------------------------------------
        // How much of a twilight is happening, from the sun's elevation. A linear ramp over a wide
        // window had the sky already colouring while the sun was still high, which stretched every
        // dusk out until it stopped reading as an event. The cube keeps the same reach for a faint
        // afterglow but puts nearly all of the colour in the last few degrees:
        // 18 deg -> 0.00, 12 -> 0.04, 9 -> 0.12, 6 -> 0.30, 3 -> 0.58, 0 -> 1.00.
        private float TwilightGlowCurve(float signedElevation)
        {
            float width = Mathf.Clamp(_twilightWidth.Value, 2f, 90f);
            float span = 1f - Mathf.Clamp01(Mathf.Abs(signedElevation) / width);
            return Mathf.Pow(span, Mathf.Clamp(_twilightFalloff.Value, 0.2f, 8f));
        }

        private static Vector3 CelestialDirection(float elevationDeg, float azimuthDeg)
        {
            float elevation = elevationDeg * Mathf.Deg2Rad;
            float azimuth = azimuthDeg * Mathf.Deg2Rad;
            float cosElevation = Mathf.Cos(elevation);
            return new Vector3(cosElevation * Mathf.Sin(azimuth), Mathf.Sin(elevation),
                cosElevation * Mathf.Cos(azimuth));
        }

        private void EnsureSunVisual()
        {
            if (_sunObject != null)
                return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Logger.LogWarning("Sprites/Default shader missing; procedural sun unavailable.");
                return;
            }
            if (_sunTexture == null)
                _sunTexture = CreateSunTexture();

            _sunMaterial = new Material(shader);
            _sunMaterial.name = "LookCare Sun Material";
            _sunMaterial.mainTexture = _sunTexture;
            _sunMaterial.renderQueue = 3000;

            _sunObject = new GameObject("LookCare Sun");
            DontDestroyOnLoad(_sunObject);
            MeshFilter filter = _sunObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _haloMesh != null ? _haloMesh : (_haloMesh = CreateQuadMesh());
            MeshRenderer renderer = _sunObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _sunMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = -1000;
            _sunObject.SetActive(false);
        }

        // The specular track on the water.
        //
        // A textured quad cannot work here. Its UV maps linearly to distance, but on a flat sea a
        // point's screen position is atan(height / distance) - so from the shore almost the whole
        // visible sea falls in the first few percent of the quad, and from the air it falls in the
        // last half. One texture profile is therefore correct at exactly one camera height and
        // invisible at the other, which is why pushing the track out to the far plane made it
        // disappear from the beach.
        //
        // The geometry is spaced by screen angle instead: rings at log-spaced distances, shaped
        // and faded by how far below the horizon each ring lands. One mesh, one draw call, alpha
        // from vertex colour, no texture fetch.
        private const int GlitterRings = 26;
        private const int GlitterColumns = 5;
        private static readonly float[] GlitterColumnOffset = { -1f, -0.5f, 0f, 0.5f, 1f };
        private static readonly float[] GlitterColumnAlpha = { 0f, 0.55f, 1f, 0.55f, 0f };

        private Mesh BuildGlitterMesh()
        {
            int vertexCount = GlitterRings * GlitterColumns;
            _glitterVertices = new Vector3[vertexCount];
            _glitterColors = new Color[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                uv[i] = new Vector2(0.5f, 0.5f);
            int[] triangles = new int[(GlitterRings - 1) * (GlitterColumns - 1) * 6];
            int index = 0;
            for (int ring = 0; ring < GlitterRings - 1; ring++)
            {
                for (int column = 0; column < GlitterColumns - 1; column++)
                {
                    int corner = ring * GlitterColumns + column;
                    triangles[index++] = corner;
                    triangles[index++] = corner + GlitterColumns;
                    triangles[index++] = corner + 1;
                    triangles[index++] = corner + 1;
                    triangles[index++] = corner + GlitterColumns;
                    triangles[index++] = corner + GlitterColumns + 1;
                }
            }
            Mesh mesh = new Mesh();
            mesh.name = "LookCare Sun Glitter";
            mesh.MarkDynamic();
            mesh.vertices = _glitterVertices;
            mesh.uv = uv;
            mesh.colors = _glitterColors;
            mesh.triangles = triangles;
            return mesh;
        }

        private static Texture2D CreateSunTexture()
        {
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size - 0.5f;
                    float dy = (y + 0.5f) / size - 0.5f;
                    float radius = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                    // A solid disc inside a wide corona. The skybox disc used to supply a strong
                    // bloom around the sun and this billboard replaced it, so the corona has to
                    // carry that weight on its own - the game's own Bloom is off by default.
                    // Mathf.SmoothStep(a, b, t) returns a value BETWEEN a and b - it is not GLSL's
                    // smoothstep(edge0, edge1, x). Passing the edges directly returned ~0.8 for
                    // every pixel and drew the sun as a solid white square.
                    float disc = 1f - Mathf.SmoothStep(0f, 1f,
                        Mathf.InverseLerp(0.23f, 0.30f, radius));
                    // Reaches zero exactly at radius 1, so the corona never meets the quad edge
                    // and no straight seam shows along the billboard's border.
                    float corona = Mathf.Pow(Mathf.Clamp01(1f - radius), 2f) * 0.75f;
                    // Composite the corona under the disc rather than taking max() of the two.
                    // max() switched terms where the disc had already fallen to zero but the
                    // corona had not yet risen, and that kink hugging the bright core is what
                    // read as a dark outline around the sun.
                    float alpha = Mathf.Clamp01(disc + corona * (1f - disc));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

        private void UpdateSunVisual(float signedElevation)
        {
            bool active = _replaceSkybox.Value && _skyApplied && RenderSettings.skybox == _skyMaterial;
            // The procedural skybox disc tracks the directional light, and that light is pinned to a
            // 6 degree elevation floor so shading stays stable near the horizon. The disc therefore
            // could never reach the horizon at all: it stopped six degrees up and was then switched
            // off outright at elevation zero, which is why the sun vanished in mid-air. Keep the
            // skybox disc permanently off and draw the sun as its own billboard on the real signed
            // elevation, the same way the moon already works.
            SetProceduralSunDisk(false);

            float visibility = 0f;
            if (active && _celestialValid)
            {
                // Hold full brightness right down to the horizon, then fade out over the few
                // degrees below it. Starting the fade while the sun was still up left it a dim
                // smudge exactly when it should have been the brightest thing in the frame.
                visibility = Mathf.Clamp01((signedElevation + 4.5f) / 5.5f) *
                    (1f - Mathf.Clamp01(_rainBlend * 2.2f));
            }
            _lastSunVisibility = visibility;
            Camera camera = Camera.main;
            if (visibility <= 0.01f || camera == null)
            {
                if (_sunObject != null && _sunObject.activeSelf)
                    _sunObject.SetActive(false);
                return;
            }

            EnsureSunVisual();
            if (_sunObject == null || _sunMaterial == null)
                return;

            float distance = Mathf.Clamp(camera.farClipPlane * 0.72f, 80f, 500f);
            Vector3 toSun = CelestialDirection(signedElevation, _lastSunAzimuth + 180f);
            _sunObject.transform.position = camera.transform.position + toSun * distance;
            _sunObject.transform.rotation = camera.transform.rotation;
            // The quad now carries the whole corona, not just the disc, so it needs more room
            // around the core than the bare disc did.
            float scale = distance * Mathf.Clamp(_sunSize.Value, 0.01f, 0.12f) * 6.5f;
            _sunObject.transform.localScale = new Vector3(scale, scale, 1f);
            // Deepen toward the twilight color as it drops, like the light it casts.
            float lowSun = 1f - Mathf.Clamp01(signedElevation / 18f);
            Color disc = Color.Lerp(new Color(1f, 0.97f, 0.90f, 1f), _twilightSunColor, lowSun * 0.85f);
            _sunMaterial.color = new Color(disc.r, disc.g, disc.b, visibility);
            _sunObject.SetActive(true);
        }

        // ---------------------------------------------------------------
        // Stars and shooting stars
        // ---------------------------------------------------------------
        private void BuildStars()
        {
            DestroyStars();
            if (!_starsEnabled.Value)
                return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return;
            if (_starTexture == null)
                _starTexture = CreateCartoonStarAtlas();

            int count = Mathf.Clamp(_starCount.Value, 40, 800);
            List<Vector3> vertices = new List<Vector3>(count * 4);
            List<Vector2> uvs = new List<Vector2>(count * 4);
            List<Color> colors = new List<Color>(count * 4);
            List<int> triangles = new List<int>(count * 6);
            _starBaseColors.Clear();
            _starTwinklePhases.Clear();
            _starTwinkleSpeeds.Clear();
            _starTwinkleAmounts.Clear();
            _starAnimatedColors.Clear();
            System.Random random = new System.Random(52341);
            Vector3 previousDirection = Vector3.up;
            for (int i = 0; i < count; i++)
            {
                Vector3 direction;
                // Roughly one star in six forms a loose two/three-star group with its predecessor. Sparse
                // clusters read like hand-placed cartoon constellations instead of uniform procedural noise.
                if (i > 0 && i % 6 != 1 && i % 6 <= 2)
                {
                    Vector3 tangentRight = Mathf.Abs(previousDirection.y) > 0.97f
                        ? Vector3.right : Vector3.Normalize(Vector3.Cross(Vector3.up, previousDirection));
                    Vector3 tangentUp = Vector3.Cross(previousDirection, tangentRight);
                    float offsetX = ((float)random.NextDouble() * 2f - 1f) * 0.07f;
                    float offsetY = ((float)random.NextDouble() * 2f - 1f) * 0.05f;
                    direction = Vector3.Normalize(previousDirection + tangentRight * offsetX + tangentUp * offsetY);
                    if (direction.y < 0.055f)
                        direction.y = 0.055f;
                    direction.Normalize();
                }
                else
                {
                    // Uniform-area hemisphere point (y uniform == area uniform on a sphere).
                    float y = 0.06f + (float)random.NextDouble() * 0.94f;
                    float phi = (float)random.NextDouble() * Mathf.PI * 2f;
                    float ringRadius = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                    direction = new Vector3(ringRadius * Mathf.Cos(phi), y, ringRadius * Mathf.Sin(phi));
                    previousDirection = direction;
                }
                Vector3 right = Mathf.Abs(direction.y) > 0.99f
                    ? Vector3.right : Vector3.Normalize(Vector3.Cross(Vector3.up, direction));
                Vector3 up = Vector3.Cross(direction, right);
                float rotation = (float)random.NextDouble() * Mathf.PI * 2f;
                Vector3 rotatedRight = right * Mathf.Cos(rotation) + up * Mathf.Sin(rotation);
                Vector3 rotatedUp = -right * Mathf.Sin(rotation) + up * Mathf.Cos(rotation);

                double shapeRoll = random.NextDouble();
                int shape = shapeRoll < 0.62 ? 0 : (shapeRoll < 0.82 ? 1 : (shapeRoll < 0.95 ? 2 : 3));
                float starSize = 0.0016f + (float)random.NextDouble() * 0.0026f;
                if (shape == 1 || shape == 2)
                    starSize *= 1.45f;
                else if (shape == 3)
                    starSize *= 1.8f;
                float brightness = 0.42f + (float)random.NextDouble() * (float)random.NextDouble() * 0.58f;
                double hueRoll = random.NextDouble();
                Color color;
                if (hueRoll < 0.58)
                    color = new Color(0.86f, 0.93f, 1f, brightness);
                else if (hueRoll < 0.84)
                    color = new Color(1f, 0.98f, 0.86f, brightness);
                else
                    color = new Color(0.92f, 0.82f, 1f, brightness);

                int baseIndex = vertices.Count;
                vertices.Add(direction - rotatedRight * starSize - rotatedUp * starSize);
                vertices.Add(direction + rotatedRight * starSize - rotatedUp * starSize);
                vertices.Add(direction - rotatedRight * starSize + rotatedUp * starSize);
                vertices.Add(direction + rotatedRight * starSize + rotatedUp * starSize);
                float cellX = (shape % 2) * 0.5f;
                float cellY = (shape / 2) * 0.5f;
                const float inset = 0.012f;
                uvs.Add(new Vector2(cellX + inset, cellY + inset));
                uvs.Add(new Vector2(cellX + 0.5f - inset, cellY + inset));
                uvs.Add(new Vector2(cellX + inset, cellY + 0.5f - inset));
                uvs.Add(new Vector2(cellX + 0.5f - inset, cellY + 0.5f - inset));
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                _starBaseColors.Add(color);
                _starTwinklePhases.Add((float)random.NextDouble() * Mathf.PI * 2f);
                _starTwinkleSpeeds.Add(0.45f + (float)random.NextDouble() * 1.35f);
                _starTwinkleAmounts.Add(shape == 0 ? 0.12f : 0.28f + (float)random.NextDouble() * 0.22f);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }

            _starMesh = new Mesh();
            _starMesh.name = "LookCare Stars";
            _starMesh.SetVertices(vertices);
            _starMesh.SetUVs(0, uvs);
            _starMesh.SetColors(colors);
            _starMesh.SetTriangles(triangles, 0);
            _starMesh.RecalculateBounds();
            _starMesh.MarkDynamic();

            _starMaterial = new Material(shader);
            _starMaterial.name = "LookCare Star Material";
            _starMaterial.mainTexture = _starTexture;
            _starMaterial.renderQueue = 2993;

            _starObject = new GameObject("LookCare Stars");
            DontDestroyOnLoad(_starObject);
            MeshFilter filter = _starObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _starMesh;
            MeshRenderer renderer = _starObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _starMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = -1003;
            _starObject.SetActive(false);
            Logger.LogInfo("STARS built count=" + count);
        }

        private static Texture2D CreateCartoonStarAtlas()
        {
            const int size = 128;
            const int cellSize = size / 2;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "LookCare Cartoon Star Atlas";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
            {
                int cellY = y / cellSize;
                float py = ((y % cellSize) + 0.5f) / cellSize * 2f - 1f;
                for (int x = 0; x < size; x++)
                {
                    int cellX = x / cellSize;
                    int shape = cellY * 2 + cellX;
                    float px = ((x % cellSize) + 0.5f) / cellSize * 2f - 1f;
                    float ax = Mathf.Abs(px);
                    float ay = Mathf.Abs(py);
                    float radius = Mathf.Sqrt(px * px + py * py);
                    float alpha;
                    if (shape == 0)
                    {
                        // Small, clean cel-shaded dot with a feathered one-pixel rim.
                        alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.34f, 0.64f, radius));
                    }
                    else if (shape == 1)
                    {
                        // Rounded diamond: the most common large cartoon sparkle.
                        float diamond = ax + ay;
                        alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.34f, 0.88f, diamond));
                    }
                    else if (shape == 2)
                    {
                        // Four-point starburst, kept chunky rather than photorealistically sharp.
                        float core = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.16f, 0.38f, radius));
                        float horizontal = (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.045f, 0.16f, ay))) *
                            (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.94f, ax)));
                        float vertical = (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.045f, 0.16f, ax))) *
                            (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.94f, ay)));
                        alpha = Mathf.Max(core, Mathf.Max(horizontal, vertical) * 0.9f);
                    }
                    else
                    {
                        // Rare five-lobed storybook star for recognizable variation among the dots.
                        float angle = Mathf.Atan2(py, px) - Mathf.PI * 0.5f;
                        float lobe = 0.5f + 0.5f * Mathf.Cos(angle * 5f);
                        float boundary = Mathf.Lerp(0.34f, 0.76f, lobe);
                        alpha = 1f - Mathf.SmoothStep(0f, 1f,
                            Mathf.InverseLerp(boundary - 0.12f, boundary + 0.06f, radius));
                    }
                    // Explicit cell-edge padding prevents bilinear sampling from leaking neighboring shapes.
                    if (ax > 0.97f || ay > 0.97f)
                        alpha = 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
                }
            }
            texture.Apply(false, false);
            return texture;
        }

        private void UpdateStarTwinkle()
        {
            if (_starMesh == null || Time.unscaledTime < _nextStarTwinkleUpdate)
                return;
            _nextStarTwinkleUpdate = Time.unscaledTime + 0.12f;
            _starAnimatedColors.Clear();
            float time = Time.unscaledTime;
            for (int i = 0; i < _starBaseColors.Count; i++)
            {
                Color color = _starBaseColors[i];
                float wave = 0.5f + 0.5f * Mathf.Sin(_starTwinklePhases[i] + time * _starTwinkleSpeeds[i]);
                color.a *= Mathf.Lerp(1f - _starTwinkleAmounts[i], 1f, wave);
                _starAnimatedColors.Add(color);
                _starAnimatedColors.Add(color);
                _starAnimatedColors.Add(color);
                _starAnimatedColors.Add(color);
            }
            _starMesh.SetColors(_starAnimatedColors);
        }

        private void UpdateStars(float dayWeight, float phase)
        {
            float weight = 0f;
            if (_starsEnabled.Value && _skyApplied && RenderSettings.skybox == _skyMaterial)
                weight = Mathf.Clamp01((0.22f - dayWeight) / 0.22f) * (1f - _rainBlend);
            Camera camera = Camera.main;
            if (weight <= 0.01f || camera == null || _starObject == null)
            {
                if (_starObject != null)
                    _starObject.SetActive(false);
                if (_meteorObject != null)
                    _meteorObject.SetActive(false);
                _meteorActive = false;
                return;
            }

            float distance = Mathf.Clamp(camera.farClipPlane * 0.68f, 60f, 400f);
            _starObject.transform.position = camera.transform.position;
            // Slow wheel tied to the cycle so the sky is not frozen.
            _starObject.transform.rotation = Quaternion.Euler(0f, phase * 90f + 20f, 0f);
            _starObject.transform.localScale = Vector3.one * distance;
            UpdateStarTwinkle();
            if (_starMaterial != null)
                _starMaterial.color = new Color(1f, 1f, 1f, weight);
            _starObject.SetActive(true);

            UpdateMeteor(weight, camera);
        }

        private void EnsureMeteor()
        {
            if (_meteorObject != null)
                return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return;
            if (_sunsetGlowTexture == null)
                _sunsetGlowTexture = CreateGlowTexture();
            if (_haloMesh == null)
                _haloMesh = CreateQuadMesh();
            _meteorMaterial = new Material(shader);
            _meteorMaterial.name = "LookCare Meteor Material";
            _meteorMaterial.mainTexture = _sunsetGlowTexture;
            _meteorMaterial.renderQueue = 2997;
            _meteorObject = new GameObject("LookCare Meteor");
            DontDestroyOnLoad(_meteorObject);
            MeshFilter filter = _meteorObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _haloMesh;
            MeshRenderer renderer = _meteorObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _meteorMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = -999;
            _meteorObject.SetActive(false);
        }

        private void UpdateMeteor(float starWeight, Camera camera)
        {
            float now = Time.unscaledTime;
            if (!_meteorActive)
            {
                if (starWeight < 0.55f || now < _nextMeteorRoll)
                    return;
                _nextMeteorRoll = now + 1f;
                if (_visualRandom.NextDouble() >= Mathf.Clamp01(_meteorChance.Value))
                    return;
                EnsureMeteor();
                if (_meteorObject == null)
                    return;
                float azimuth = (float)_visualRandom.NextDouble() * 360f;
                float elevation = 25f + (float)_visualRandom.NextDouble() * 40f;
                _meteorDir = CelestialDirection(elevation, azimuth);
                // Tangential velocity, biased downward with a random sideways component.
                Vector3 east = Vector3.Normalize(Vector3.Cross(Vector3.up, _meteorDir));
                Vector3 downTangent = Vector3.Normalize(_meteorDir * _meteorDir.y - Vector3.up);
                Vector3 tangent = Vector3.Normalize(downTangent +
                    east * ((float)_visualRandom.NextDouble() * 1.6f - 0.8f));
                _meteorVelocity = tangent * (30f * Mathf.Deg2Rad);
                _meteorStartTime = now;
                _meteorActive = true;
            }

            const float duration = 0.9f;
            float age = now - _meteorStartTime;
            if (age >= duration || _meteorObject == null || _meteorMaterial == null)
            {
                _meteorActive = false;
                if (_meteorObject != null)
                    _meteorObject.SetActive(false);
                return;
            }
            Vector3 direction = Vector3.Normalize(_meteorDir + _meteorVelocity * age);
            float distance = Mathf.Clamp(camera.farClipPlane * 0.62f, 60f, 380f);
            _meteorObject.transform.position = camera.transform.position + direction * distance;
            Vector3 tangentNow = _meteorVelocity - direction * Vector3.Dot(_meteorVelocity, direction);
            if (tangentNow.sqrMagnitude < 0.000001f)
                tangentNow = Vector3.down;
            _meteorObject.transform.rotation = Quaternion.LookRotation(direction,
                Vector3.Normalize(tangentNow));
            _meteorObject.transform.localScale = new Vector3(distance * 0.010f, distance * 0.115f, 1f);
            float lifeFraction = age / duration;
            float flare = Mathf.Sin(lifeFraction * Mathf.PI);
            _meteorMaterial.color = new Color(0.86f, 0.92f, 1f, 0.9f * flare * starWeight);
            _meteorObject.SetActive(true);
        }

        private void DestroyStars()
        {
            if (_starObject != null)
                Destroy(_starObject);
            if (_starMaterial != null)
                Destroy(_starMaterial);
            if (_starMesh != null)
                Destroy(_starMesh);
            _starObject = null;
            _starMaterial = null;
            _starMesh = null;
            _starBaseColors.Clear();
            _starTwinklePhases.Clear();
            _starTwinkleSpeeds.Clear();
            _starTwinkleAmounts.Clear();
            _starAnimatedColors.Clear();
        }

        private void DestroyMeteor()
        {
            if (_meteorObject != null)
                Destroy(_meteorObject);
            if (_meteorMaterial != null)
                Destroy(_meteorMaterial);
            _meteorObject = null;
            _meteorMaterial = null;
            _meteorActive = false;
        }

        private void DestroyMoon()
        {
            if (_moonObject != null)
                Destroy(_moonObject);
            if (_moonMaterial != null)
                Destroy(_moonMaterial);
            if (_moonTexture != null)
                Destroy(_moonTexture);
            if (_moonMesh != null)
                Destroy(_moonMesh);
            _moonObject = null;
            _moonMaterial = null;
            _moonTexture = null;
            _moonMesh = null;
        }

        // ---------------------------------------------------------------
        // Procedural sky with separate sun and moon discs
        // ---------------------------------------------------------------
        private void ApplySky()
        {
            if (!_replaceSkybox.Value)
            {
                RestoreSky();
                return;
            }

            if (_skyMaterial == null)
            {
                Shader shader = Shader.Find("Skybox/Procedural");
                if (shader == null)
                {
                    Logger.LogWarning("Skybox/Procedural shader not found; sky replacement skipped.");
                    return;
                }
                _skyMaterial = new Material(shader);
                _skyMaterial.name = "LookCare Procedural Sky";
            }

            // The real-clock cycle updates the same atmospheric sun disc each frame. Without that cycle,
            // retain the authored atmospheric sun continuously.
            SetProceduralSunDisk(!_dayCycleEnabled.Value);
            _skyMaterial.SetFloat("_AtmosphereThickness", Mathf.Clamp(_atmosphereThickness.Value, 0.1f, 3f));
            _skyMaterial.SetFloat("_Exposure", Mathf.Clamp(_skyExposure.Value, 0.05f, 8f));
            _skyMaterial.SetColor("_SkyTint", _skyTint.Value);
            _skyMaterial.SetColor("_GroundColor", _groundColor.Value);

            if (!_skyApplied)
            {
                _originalSkybox = RenderSettings.skybox;
                _skyApplied = true;
            }
            RenderSettings.skybox = _skyMaterial;
            _skyRuntimeEnabled = true;
            EnsureMoon();
            UpdateMoonPhaseState(true);

            if (RenderSettings.sun == null)
            {
                Light sun = FindSunLight();
                if (sun != null)
                {
                    RenderSettings.sun = sun;
                    Logger.LogInfo("Sun light assigned for the sky disc: " + sun.name);
                }
            }
            DynamicGI.UpdateEnvironment();
            Logger.LogInfo("SKY replaced (F5 toggles). thickness=" + _atmosphereThickness.Value +
                " exposure=" + _skyExposure.Value);
        }

        private void ToggleSky()
        {
            if (!_skyApplied)
            {
                ApplySky();
                return;
            }
            _skyRuntimeEnabled = !_skyRuntimeEnabled;
            RenderSettings.skybox = _skyRuntimeEnabled ? _skyMaterial : _originalSkybox;
            if (!_skyRuntimeEnabled)
                RestoreCameraSkyboxes();
            if (!_skyRuntimeEnabled && _moonObject != null)
                _moonObject.SetActive(false);
            DynamicGI.UpdateEnvironment();
            Logger.LogInfo("SKY " + (_skyRuntimeEnabled ? "custom" : "original"));
        }

        private void RestoreSky()
        {
            _skyRuntimeEnabled = false;
            RestoreCameraSkyboxes();
            if (_skyApplied && RenderSettings.skybox == _skyMaterial)
            {
                RenderSettings.skybox = _originalSkybox;
                DynamicGI.UpdateEnvironment();
            }
            if (_moonObject != null)
                _moonObject.SetActive(false);
            _skyApplied = false;
        }

        private void RestoreCameraSkyboxes()
        {
            foreach (KeyValuePair<int, CameraSkyboxState> pair in _cameraSkyboxStates)
            {
                CameraSkyboxState state = pair.Value;
                if (state != null && state.Skybox != null)
                    state.Skybox.material = state.OriginalMaterial;
            }
            _cameraSkyboxStates.Clear();
        }

        private static Light FindSunLight()
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light best = null;
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null || light.type != LightType.Directional || !light.enabled)
                    continue;
                if (best == null || light.intensity > best.intensity)
                    best = light;
            }
            return best;
        }

        // ---------------------------------------------------------------
        // Weather: randomized/manual rain, storm cover, rain particles,
        // fog/light grading and the game's own Rain/Thunder ambience clips.
        // ---------------------------------------------------------------
        private void RefreshWeatherConfig()
        {
            KeyCode parsed;
            if (!Enum.TryParse<KeyCode>(_rainToggleKey.Value, true, out parsed))
            {
                parsed = KeyCode.F10;
                Logger.LogWarning("Unknown rain toggle key '" + _rainToggleKey.Value + "'; using F10.");
            }
            _weatherToggleKey = parsed;
            if (!Enum.TryParse<KeyCode>(_configMenuKey.Value, true, out parsed))
            {
                parsed = KeyCode.F11;
                Logger.LogWarning("Unknown config menu key '" + _configMenuKey.Value + "'; using F11.");
            }
            _configMenuKeyCode = parsed;
            if (_nextWeatherChange <= 0f)
                ScheduleWeatherChange(false);
        }

        private void ApplyWeather()
        {
            RefreshWeatherConfig();
            if (!_weatherEnabled.Value)
                SetRainTarget(false, false);
            if (_rainTarget)
            {
                EnsureStormClouds();
                EnsureRainParticles();
                EnsureWeatherAudio();
            }
            Logger.LogInfo("WEATHER enabled=" + _weatherEnabled.Value + " random=" + _randomRain.Value +
                " target=" + (_rainTarget ? "rain" : "clear") + " toggle=" + _weatherToggleKey +
                " nextChange=" + Mathf.Max(0f, _nextWeatherChange - Time.unscaledTime).ToString("F0") + "s");
        }

        private void SetRainTarget(bool raining, bool manual)
        {
            if (!_weatherEnabled.Value)
                raining = false;
            if (_rainTarget == raining && !manual)
                return;
            _rainTarget = raining;
            ScheduleWeatherChange(raining);
            if (raining)
            {
                EnsureStormClouds();
                EnsureRainParticles();
                EnsureWeatherAudio();
            }
            Logger.LogInfo("WEATHER " + (raining ? "RAIN START" : "RAIN STOP") +
                (manual ? " (manual)" : " (random)") + "; next transition in " +
                Mathf.Max(0f, _nextWeatherChange - Time.unscaledTime).ToString("F0") + "s");
        }

        private void ScheduleWeatherChange(bool raining)
        {
            float minMinutes = raining ? _rainMinutesMin.Value : _clearMinutesMin.Value;
            float maxMinutes = raining ? _rainMinutesMax.Value : _clearMinutesMax.Value;
            minMinutes = Mathf.Max(0.2f, minMinutes);
            maxMinutes = Mathf.Max(minMinutes, maxMinutes);
            float minutes = Mathf.Lerp(minMinutes, maxMinutes, (float)_weatherRandom.NextDouble());
            _nextWeatherChange = Time.unscaledTime + minutes * 60f;
        }

        private void UpdateWeather()
        {
            float now = Time.unscaledTime;
            if (now >= _nextWorldProbe)
            {
                _nextWorldProbe = now + 3f;
                _worldSceneReady = GameObject.Find("Island") != null;
            }

            if (_weatherEnabled.Value && _randomRain.Value && _worldSceneReady &&
                now >= _nextWeatherChange)
                SetRainTarget(!_rainTarget, false);

            bool effectiveRain = _weatherEnabled.Value && _worldSceneReady && _rainTarget;
            float transition = Mathf.Clamp(_weatherTransitionSeconds.Value, 2f, 60f);
            _rainBlend = Mathf.MoveTowards(_rainBlend, effectiveRain ? 1f : 0f,
                Time.unscaledDeltaTime / transition);
            _lightningFlash = Mathf.MoveTowards(_lightningFlash, 0f, Time.unscaledDeltaTime * 4.5f);

            if (_rainBlend > 0.001f)
            {
                EnsureStormClouds();
                EnsureRainParticles();
                EnsureWeatherAudio();
            }
            UpdateStormClouds();
            UpdateRainParticles();
            UpdateWeatherAudioAndThunder();
            UpdateWeatherFogAndAmbient();
            UpdateWeatherCloudColors();
            if (!_dayCycleEnabled.Value)
                ApplyWeatherWithoutDayCycle();
        }

        private void ApplyWeatherWithoutDayCycle()
        {
            if (_sunCaptured && _sunLight != null)
            {
                Color clearColor = Color.Lerp(_origSunColor, Color.white, Mathf.Clamp01(_sunNeutralize.Value));
                _sunLight.color = Color.Lerp(clearColor, new Color(0.66f, 0.72f, 0.82f, 1f),
                    _rainBlend * 0.72f);
                _sunLight.intensity = _origSunIntensity * Mathf.Clamp(_sunIntensity.Value, 0.3f, 2f) *
                    Mathf.Lerp(1f, Mathf.Clamp(_rainLightMultiplier.Value, 0.2f, 1f), _rainBlend) *
                    (1f + _lightningFlash * 1.6f);
            }
            if (_colorAdjustments != null)
            {
                _colorAdjustments.postExposure.Override(_postExposure.Value +
                    _rainExposureDip.Value * _rainBlend);
                _colorAdjustments.colorFilter.Override(_dayColorFilter.Value *
                    Color.Lerp(Color.white, new Color(0.72f, 0.80f, 0.92f, 1f), _rainBlend) *
                    GetSeasonColorFilter(1f));
            }
            if (_skyMaterial != null && RenderSettings.skybox == _skyMaterial)
            {
                _skyMaterial.SetFloat("_Exposure", Mathf.Clamp(_skyExposure.Value, 0.05f, 8f) *
                    Mathf.Lerp(1f, 0.52f, _rainBlend));
                _skyMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(
                    Mathf.Clamp(_atmosphereThickness.Value, 0.1f, 3f), 0.55f, _rainBlend));
                _skyMaterial.SetColor("_SkyTint", Color.Lerp(_skyTint.Value,
                    new Color(0.36f, 0.41f, 0.49f, 1f), _rainBlend));
                SetProceduralSunDisk(_rainBlend < 0.12f);
            }
        }

        private void EnsureStormClouds()
        {
            if (_stormCloudRoot != null || _cloudMaterial == null)
                return;
            _stormCloudMaterial = new Material(_cloudMaterial);
            _stormCloudMaterial.name = "LookCare Storm Cloud Material";
            SetMaterialFloat(_stormCloudMaterial, "_Is_LightColor_Base", 0f);
            SetMaterialFloat(_stormCloudMaterial, "_Is_LightColor_1st_Shade", 0f);
            SetMaterialFloat(_stormCloudMaterial, "_Is_LightColor_2nd_Shade", 0f);
            SetMaterialColor(_stormCloudMaterial, "_BaseColor", new Color(0.47f, 0.53f, 0.62f, 1f));
            SetMaterialColor(_stormCloudMaterial, "_Color", new Color(0.47f, 0.53f, 0.62f, 1f));
            SetMaterialColor(_stormCloudMaterial, "_1st_ShadeColor", new Color(0.29f, 0.34f, 0.43f, 1f));
            SetMaterialColor(_stormCloudMaterial, "_2nd_ShadeColor", new Color(0.19f, 0.24f, 0.34f, 1f));

            _stormCloudRoot = new GameObject("LookCare Storm Clouds");
            System.Random random = new System.Random(118021);
            Camera camera = Camera.main;
            Vector3 center = camera != null ? camera.transform.position : Vector3.zero;
            // A random radial scatter left large blue windows between clouds. Use a jittered,
            // camera-centred lattice instead: every part of the visible dome has at least one
            // broad cloud above it, while the jitter/rotation keep the formation from reading
            // as a grid. Puffy meshes stay irregular; only their coverage is stratified.
            const int gridSize = 8;
            const float spacing = 48f;
            const int count = gridSize * gridSize;
            for (int i = 0; i < count; i++)
            {
                Mesh mesh = CreateCloudMesh(random);
                _stormCloudMeshes.Add(mesh);
                GameObject cloud = new GameObject("LookCare Storm Cloud " + i);
                cloud.transform.SetParent(_stormCloudRoot.transform, false);
                int gridX = i % gridSize;
                int gridZ = i / gridSize;
                float offsetX = (gridX - (gridSize - 1) * 0.5f) * spacing;
                float offsetZ = (gridZ - (gridSize - 1) * 0.5f) * spacing;
                offsetX += ((float)random.NextDouble() - 0.5f) * 18f;
                offsetZ += ((float)random.NextDouble() - 0.5f) * 18f;
                cloud.transform.position = new Vector3(center.x + offsetX,
                    46f + (float)random.NextDouble() * 16f,
                    center.z + offsetZ);
                cloud.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                float scale = 3.8f + (float)random.NextDouble() * 1.2f;
                Vector3 baseScale = new Vector3(scale * 1.42f, scale * 0.56f, scale * 1.42f);
                cloud.transform.localScale = Vector3.zero;
                _stormCloudTransforms.Add(cloud.transform);
                _stormCloudBaseScales.Add(baseScale);
                MeshFilter filter = cloud.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = cloud.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _stormCloudMaterial;
                renderer.shadowCastingMode = _cloudShadows.Value ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            _stormCloudRoot.SetActive(false);
            Logger.LogInfo("WEATHER storm cloud cover built=" + count +
                " layout=" + gridSize + "x" + gridSize + " spacing=" + spacing);
        }

        private void UpdateStormClouds()
        {
            if (_stormCloudRoot == null)
                return;
            if (_rainBlend <= 0.005f)
            {
                _stormCloudRoot.SetActive(false);
                return;
            }
            _stormCloudRoot.SetActive(true);
            float formation = Mathf.SmoothStep(0f, 1f, _rainBlend);
            float stormLight = Mathf.Lerp(0.40f, 1f, _lastDayWeight);
            Color stormBase = new Color(0.43f, 0.48f, 0.56f, 1f) * stormLight;
            Color stormFirst = new Color(0.27f, 0.31f, 0.38f, 1f) * stormLight;
            Color stormSecond = new Color(0.18f, 0.22f, 0.29f, 1f) * stormLight;
            stormBase.a = stormFirst.a = stormSecond.a = 1f;
            SetMaterialColor(_stormCloudMaterial, "_BaseColor", stormBase);
            SetMaterialColor(_stormCloudMaterial, "_Color", stormBase);
            SetMaterialColor(_stormCloudMaterial, "_1st_ShadeColor", stormFirst);
            SetMaterialColor(_stormCloudMaterial, "_2nd_ShadeColor", stormSecond);
            Camera camera = Camera.main;
            Vector3 center = camera != null ? camera.transform.position : Vector3.zero;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            float speed = Mathf.Max(0.18f, Mathf.Clamp(_cloudSpeed.Value, 0f, 30f) * 0.55f);
            Vector3 wind = Quaternion.Euler(0f, Mathf.Repeat(_cloudDirection.Value + 8f, 360f), 0f) *
                Vector3.forward * (speed * dt);
            for (int i = 0; i < _stormCloudTransforms.Count; i++)
            {
                Transform cloud = _stormCloudTransforms[i];
                if (cloud == null)
                    continue;
                Vector3 position = cloud.position + wind;
                float dx = position.x - center.x;
                float dz = position.z - center.z;
                const float halfSpan = 192f;
                const float span = halfSpan * 2f;
                if (dx > halfSpan) dx -= span;
                else if (dx < -halfSpan) dx += span;
                if (dz > halfSpan) dz -= span;
                else if (dz < -halfSpan) dz += span;
                cloud.position = new Vector3(center.x + dx, position.y, center.z + dz);
                cloud.localScale = _stormCloudBaseScales[i] * formation;
            }
        }

        private void EnsureRainParticles()
        {
            if (_rainObject != null)
                return;

            // The shipped fx_slow rain / fx_rain & thunder systems both use the material
            // "fx rain 3": an untextured URP transparent unlit material at alpha 0.227.
            // Clone that exact material, then explicitly drive its tint from LookCare's
            // daylight, moonlight, storm dimming and lightning state.
            Material nativeMaterial = FindNativeRainMaterial();
            _rainUsesNativeMaterial = nativeMaterial != null;
            if (_rainUsesNativeMaterial)
            {
                _rainMaterial = new Material(nativeMaterial);
                _rainMaterial.name = "LookCare Rain Material (native fx rain 3)";
            }
            else
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    return;
                _rainTexture = CreateRainStreakTexture();
                _rainMaterial = new Material(shader);
                _rainMaterial.name = "LookCare Rain Material (soft fallback)";
                _rainMaterial.mainTexture = _rainTexture;
            }

            _rainObject = new GameObject("LookCare Rain");
            DontDestroyOnLoad(_rainObject);
            // A slight tilt off vertical sells wind-driven rain without a wind zone.
            _rainObject.transform.rotation = Quaternion.Euler(84f, 0f, 0f);
            _rainParticles = _rainObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = _rainParticles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            // Match the native systems' 0.4-0.5 second life and 0.08-0.12 size instead
            // of drawing the old 50-metre opaque blue-white streaks.
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.62f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(25f, 31f);
            main.gravityModifier = 0.16f;
            float dropSize = Mathf.Clamp(_rainDropSize.Value, 0.4f, 1.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.075f * dropSize, 0.115f * dropSize);
            main.maxParticles = 2600;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 1f, 1f, 0.82f), new Color(1f, 1f, 1f, 0.98f));
            ParticleSystem.EmissionModule emission = _rainParticles.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _rainParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient rainFade = new Gradient();
            rainFade.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.15f, 0f), new GradientAlphaKey(1f, 0.10f),
                    new GradientAlphaKey(0.68f, 0.78f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = rainFade;
            ParticleSystem.ShapeModule shape = _rainParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            // The shape box is in LOCAL space and the whole system is rotated ~90 degrees
            // to point +Z (the emission direction) downward. That swaps the local Y and Z
            // axes in world space: (70,2,70) was a 2m-THIN VERTICAL CURTAIN of rain slicing
            // through the camera - rain over only a strip of the sky. (70,70,2) is the
            // intended horizontal 70x70m emission sheet, 2m thick.
            shape.scale = new Vector3(70f, 70f, 2f);
            ParticleSystemRenderer renderer = _rainObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = _rainMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            // The native renderer uses velocityScale 0.05-0.10 and no artificial long
            // length. This produces short translucent dashes instead of luminous needles.
            renderer.velocityScale = 0.045f;
            renderer.lengthScale = 0f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            _rainParticles.Play();
            _rainObject.SetActive(false);
            EnsureSplashParticles();
            Logger.LogInfo("WEATHER rain visual material=" +
                (_rainUsesNativeMaterial ? "native fx rain 3" : "soft brightness-driven fallback") +
                " shader=" + (_rainMaterial.shader != null ? _rainMaterial.shader.name : "<none>"));
        }

        private static Material FindNativeRainMaterial()
        {
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null && NormalizeSceneName(material.name) == "fxrain3")
                    return material;
            }
            return null;
        }

        private static Texture2D CreateRainStreakTexture()
        {
            // Fallback only. The native effect has no blue ink rim: it is a feathered,
            // semi-transparent white particle whose apparent brightness comes from light.
            const int width = 24;
            const int height = 80;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "LookCare Rain Streak";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < height; y++)
            {
                float v = (y + 0.5f) / height;
                float vertical = Mathf.Sin(v * Mathf.PI);
                vertical = Mathf.Pow(Mathf.Clamp01(vertical), 0.55f);
                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs((x + 0.5f) / width - 0.5f) * 2f;
                    float horizontal = Mathf.Clamp01(1f - dx);
                    horizontal = horizontal * horizontal * (3f - 2f * horizontal);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, horizontal * vertical * 0.62f));
                }
            }
            texture.Apply(false, false);
            return texture;
        }

        private void UpdateRainParticles()
        {
            if (_rainObject == null || _rainParticles == null)
                return;
            if (_rainBlend <= 0.005f || !_worldSceneReady)
            {
                _rainObject.SetActive(false);
                if (_splashObject != null)
                    _splashObject.SetActive(false);
                _splashAccumulator = 0f;
                return;
            }
            Camera camera = Camera.main;
            if (camera == null)
                return;
            _rainObject.SetActive(true);
            // At a 0.45-0.62s native-style lifetime, a 22m-high sheet killed most drops
            // before they entered the view. Twelve metres keeps the field overhead while
            // allowing the short streaks to cross the camera and reach nearby surfaces.
            _rainObject.transform.position = camera.transform.position + Vector3.up * 12f;
            if (!_rainParticles.isPlaying)
                _rainParticles.Play();
            ParticleSystem.MainModule main = _rainParticles.main;
            float dropSize = Mathf.Clamp(_rainDropSize.Value, 0.4f, 1.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.075f * dropSize, 0.115f * dropSize);
            ParticleSystem.EmissionModule emission = _rainParticles.emission;
            emission.rateOverTime = 620f * Mathf.Clamp(_rainDensity.Value, 0.5f, 2f) * _rainBlend;
            UpdatePrecipitationBrightness();
            EmitRainSplashes(camera);
        }

        private void EnsureSplashParticles()
        {
            if (_splashObject != null)
                return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return;

            _splashTexture = CreateSplashTexture();
            _splashMaterial = new Material(shader);
            _splashMaterial.name = "LookCare Rain Splash Material";
            _splashMaterial.mainTexture = _splashTexture;

            _splashObject = new GameObject("LookCare Rain Splashes");
            DontDestroyOnLoad(_splashObject);
            _splashParticles = _splashObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = _splashParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.32f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.24f, 0.46f);
            main.maxParticles = 320;
            ParticleSystem.EmissionModule emission = _splashParticles.emission;
            emission.enabled = false;
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = _splashParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve splashSize = new AnimationCurve(
                new Keyframe(0f, 0.35f), new Keyframe(0.16f, 1f), new Keyframe(1f, 0.72f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, splashSize);
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _splashParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f),
                    new GradientAlphaKey(0.55f, 0.55f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            ParticleSystemRenderer renderer = _splashObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = _splashMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            _splashParticles.Play();
            _splashObject.SetActive(false);
        }

        private static Texture2D CreateSplashTexture()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "LookCare Rain Splash";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            // A small cluster of separate droplets, rather than the old three-pronged jet
            // and impact ring. Each emitted sprite reads as water breaking into pixels.
            Vector2[] centers = new Vector2[]
            {
                new Vector2(0.00f, -0.08f), new Vector2(-0.46f, 0.27f),
                new Vector2(0.43f, 0.31f), new Vector2(-0.15f, 0.67f),
                new Vector2(0.56f, -0.30f), new Vector2(-0.62f, -0.37f)
            };
            float[] radii = new float[] { 0.17f, 0.11f, 0.12f, 0.08f, 0.07f, 0.065f };
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(
                        ((x + 0.5f) / size) * 2f - 1f,
                        ((y + 0.5f) / size) * 2f - 1f);
                    float alpha = 0f;
                    for (int dot = 0; dot < centers.Length; dot++)
                    {
                        float fill = Mathf.Clamp01(1f - Vector2.Distance(point, centers[dot]) / radii[dot]);
                        alpha = Mathf.Max(alpha, fill * fill * (3f - 2f * fill));
                    }
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texture.Apply(false, false);
            return texture;
        }

        private void UpdatePrecipitationBrightness()
        {
            // Sprites/Default is unlit, and even the native lit material benefits from a
            // conservative tint at deep night. This multiplier makes every active drop and
            // splash follow the same day/night/storm/lightning state as the rest of the frame.
            float dayBrightness = Mathf.Lerp(0.66f, 1f, _lastDayWeight);
            dayBrightness *= Mathf.Lerp(1f, 0.82f, _rainBlend);
            dayBrightness *= 1f + _lightningFlash * 0.55f;
            Color nightTint = new Color(0.66f, 0.74f, 0.88f, 1f);
            Color tint = Color.Lerp(nightTint, Color.white, _lastDayWeight) * dayBrightness;
            tint.a = _rainUsesNativeMaterial ? Mathf.Lerp(0.32f, 0.23f, _lastDayWeight) : 0.42f;
            if (_rainMaterial != null)
            {
                _rainMaterial.color = tint;
                SetMaterialColor(_rainMaterial, "_BaseColor", tint);
                SetMaterialColor(_rainMaterial, "_Color", tint);
            }
            if (_splashMaterial != null)
            {
                Color splashTint = tint;
                splashTint.a = 0.52f;
                _splashMaterial.color = splashTint;
            }
        }

        private void EmitRainSplashes(Camera camera)
        {
            if (_splashObject == null || _splashParticles == null || _rainBlend < 0.12f)
                return;
            _splashObject.SetActive(true);
            if (!_splashParticles.isPlaying)
                _splashParticles.Play();

            _splashAccumulator += Mathf.Min(Time.unscaledDeltaTime, 0.1f) *
                Mathf.Lerp(8f, 24f, _rainBlend);
            int attempts = Mathf.Min(5, Mathf.FloorToInt(_splashAccumulator));
            _splashAccumulator -= attempts;
            for (int i = 0; i < attempts; i++)
            {
                float angle = (float)_weatherRandom.NextDouble() * Mathf.PI * 2f;
                float radius = Mathf.Sqrt((float)_weatherRandom.NextDouble()) * 18f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 origin = camera.transform.position + offset + Vector3.up * 24f;
                RaycastHit hit;
                if (!Physics.Raycast(origin, Vector3.down, out hit, 180f, ~0, QueryTriggerInteraction.Ignore))
                    continue;
                if (hit.collider == null || hit.collider.GetComponentInParent<Canvas>() != null)
                    continue;
                string hitName = NormalizeSceneName(hit.collider.gameObject.name);
                if (hit.normal.y < 0.42f || hitName.IndexOf("player") >= 0 || hitName.IndexOf("avatar") >= 0)
                    continue;

                ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams();
                emit.position = hit.point + hit.normal * 0.035f;
                emit.velocity = hit.normal * Mathf.Lerp(0.18f, 0.42f, (float)_weatherRandom.NextDouble());
                emit.startLifetime = Mathf.Lerp(0.19f, 0.31f, (float)_weatherRandom.NextDouble());
                emit.startSize = Mathf.Lerp(0.26f, 0.48f, (float)_weatherRandom.NextDouble());
                emit.startColor = Color.white;
                emit.rotation = (float)_weatherRandom.NextDouble() * Mathf.PI * 2f;
                _splashParticles.Emit(emit, 1);
                if (!_splashDiagnosticWritten)
                {
                    _splashDiagnosticWritten = true;
                    Logger.LogInfo("WEATHER splash first-hit path=" + GetTransformPath(hit.collider.transform) +
                        " point=" + hit.point.ToString("F1") + " normalY=" + hit.normal.y.ToString("F2"));
                }
            }
        }

        private void EnsureWeatherAudio()
        {
            if (_weatherAudioObject == null)
            {
                _weatherAudioObject = new GameObject("LookCare Weather Audio");
                DontDestroyOnLoad(_weatherAudioObject);
                _rainAudioSource = _weatherAudioObject.AddComponent<AudioSource>();
                _rainAudioSource.loop = true;
                _rainAudioSource.playOnAwake = false;
                _rainAudioSource.spatialBlend = 0f;
                _rainAudioSource.ignoreListenerPause = true;
                _rainDetailAudioSource = _weatherAudioObject.AddComponent<AudioSource>();
                _rainDetailAudioSource.loop = true;
                _rainDetailAudioSource.playOnAwake = false;
                _rainDetailAudioSource.spatialBlend = 0f;
                _rainDetailAudioSource.ignoreListenerPause = true;
                _thunderAudioSource = _weatherAudioObject.AddComponent<AudioSource>();
                _thunderAudioSource.loop = false;
                _thunderAudioSource.playOnAwake = false;
                _thunderAudioSource.spatialBlend = 0f;
                _thunderAudioSource.ignoreListenerPause = true;
            }
            if ((_rainAudioClip == null || _rainDetailAudioClip == null || _thunderAudioClip == null) &&
                Time.unscaledTime >= _nextWeatherAudioProbe)
            {
                _nextWeatherAudioProbe = Time.unscaledTime + 10f;
                if (_rainAudioClip == null || _rainDetailAudioClip == null)
                    ResolveBuiltInRainLayers();
                if (_thunderAudioClip == null)
                    _thunderAudioClip = FindBuiltInAudioClip("Thunder");
            }
            if (_rainAudioSource != null && _rainAudioSource.clip != _rainAudioClip)
                _rainAudioSource.clip = _rainAudioClip;
            if (_rainDetailAudioSource != null && _rainDetailAudioSource.clip != _rainDetailAudioClip)
                _rainDetailAudioSource.clip = _rainDetailAudioClip;
        }

        private void ResolveBuiltInRainLayers()
        {
            AudioClip longestStorm = null;
            AudioClip longestRain = null;
            AudioClip[] loaded = Resources.FindObjectsOfTypeAll<AudioClip>();
            for (int i = 0; i < loaded.Length; i++)
            {
                AudioClip clip = loaded[i];
                if (clip == null)
                    continue;
                if (string.Equals(clip.name, "StormRain", StringComparison.OrdinalIgnoreCase) &&
                    (longestStorm == null || clip.length > longestStorm.length))
                    longestStorm = clip;
                else if (string.Equals(clip.name, "Rain", StringComparison.OrdinalIgnoreCase) &&
                    (longestRain == null || clip.length > longestRain.length))
                    longestRain = clip;
            }

            if (longestStorm == null)
                longestStorm = Resources.Load<AudioClip>("StormRain");
            if (longestRain == null)
                longestRain = Resources.Load<AudioClip>("Rain");

            _rainAudioClip = longestStorm != null ? longestStorm : longestRain;
            _rainDetailAudioClip = longestRain != null ? longestRain : _rainAudioClip;
            if (_rainAudioClip == null)
                return;

            string signature = _rainAudioClip.name + "(" + _rainAudioClip.length.ToString("F1") + "s) + " +
                (_rainDetailAudioClip != null
                    ? _rainDetailAudioClip.name + "(" + _rainDetailAudioClip.length.ToString("F1") + "s)"
                    : "none");
            if (!string.Equals(signature, _weatherAudioPairSignature, StringComparison.Ordinal))
            {
                _weatherAudioPairSignature = signature;
                Logger.LogInfo("WEATHER AUDIO rain layers=" + signature);
            }
        }

        private AudioClip FindBuiltInAudioClip(string exactName)
        {
            AudioClip[] loaded = Resources.FindObjectsOfTypeAll<AudioClip>();
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] != null && string.Equals(loaded[i].name, exactName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogInfo("WEATHER AUDIO found loaded clip=" + loaded[i].name);
                    return loaded[i];
                }
            }
            AudioClip resource = Resources.Load<AudioClip>(exactName);
            if (resource != null)
            {
                Logger.LogInfo("WEATHER AUDIO loaded resource=" + resource.name);
                return resource;
            }
            if (!_weatherAudioWarningWritten)
            {
                _weatherAudioWarningWritten = true;
                Logger.LogWarning("Built-in weather AudioClip '" + exactName +
                    "' was confirmed in resources.assets but is not loaded by this scene yet; " +
                    "LookCare will keep probing without substituting non-game audio.");
            }
            return null;
        }

        private void UpdateWeatherAudioAndThunder()
        {
            if (_weatherAudioObject == null)
                return;
            float volume = Mathf.Clamp01(_rainAudioVolume.Value) * _rainBlend;
            bool hasDetail = _rainDetailAudioSource != null && _rainDetailAudioClip != null;
            _rainAudioSource.volume = volume * (hasDetail ? 0.68f : 1f);
            if (volume > 0.01f && _rainAudioClip != null)
            {
                if (!_rainAudioSource.isPlaying)
                {
                    _rainAudioSource.pitch = 1f;
                    if (_rainAudioClip.length > 1f)
                        _rainAudioSource.time = (float)_weatherRandom.NextDouble() * (_rainAudioClip.length - 0.1f);
                    _rainAudioSource.Play();
                }
            }
            else if (_rainAudioSource.isPlaying)
            {
                _rainAudioSource.Stop();
            }

            if (hasDetail)
            {
                _rainDetailAudioSource.volume = volume * 0.32f;
                if (volume > 0.01f)
                {
                    if (!_rainDetailAudioSource.isPlaying)
                    {
                        // A tiny pitch offset plus a random phase prevents even a fallback duplicate
                        // clip from exposing its loop point at the same moment as the base layer.
                        _rainDetailAudioSource.pitch = ReferenceEquals(_rainDetailAudioClip, _rainAudioClip)
                            ? 0.973f : 0.992f;
                        if (_rainDetailAudioClip.length > 1f)
                            _rainDetailAudioSource.time = (float)_weatherRandom.NextDouble() *
                                (_rainDetailAudioClip.length - 0.1f);
                        _rainDetailAudioSource.Play();
                    }
                }
                else if (_rainDetailAudioSource.isPlaying)
                {
                    _rainDetailAudioSource.Stop();
                }
            }

            float now = Time.unscaledTime;
            float configuredThunderChance = Mathf.Clamp(_thunderChancePerMinute.Value, 0f, 12f);
            if (configuredThunderChance <= 0f)
            {
                // Zero is a hard off switch, including an already playing one-shot and any
                // flash left over from the previous setting.
                _lightningFlash = 0f;
                _nextThunderRoll = now + 1f;
                if (_thunderAudioSource != null && _thunderAudioSource.isPlaying)
                    _thunderAudioSource.Stop();
                return;
            }
            if (_rainBlend >= 0.82f && now >= _nextThunderRoll)
            {
                _nextThunderRoll = now + 1f;
                float chance = configuredThunderChance / 60f;
                if (_weatherRandom.NextDouble() < chance)
                {
                    _lightningFlash = 1f;
                    if (_thunderAudioClip != null)
                        _thunderAudioSource.PlayOneShot(_thunderAudioClip, volume * 0.9f);
                    Logger.LogInfo("WEATHER lightning/thunder");
                }
            }
        }

        // Phone, laptop, and handheld-console screens are separate material slots on the
        // actual player meshes. The console is MD_ConsoleDeck / M_DeckScreen; MD_Console
        // is a different prop and deliberately remains untouched.
        // The shipped Toon shader has no usable runtime emission variant, so this draws only
        // the confirmed screen submeshes a second time with a subtle URP Unlit overlay. It
        // neither changes shared game materials nor lights the nearby scene.
        private void ApplyDeviceScreenGlow()
        {
            RestoreDeviceScreenGlow();
            if (!_deviceScreensGlow.Value)
                return;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Logger.LogWarning("DEVICE SCREEN GLOW unavailable: Universal Render Pipeline/Unlit was not loaded.");
                return;
            }

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int overlays = 0;
            int screenSlots = 0;
            int meshOverlays = 0;
            int skinnedOverlays = 0;
            int skipped = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;
                string objectName = renderer.gameObject.name;
                bool phone = objectName == "MD_Cellphone" || objectName == "MD_CellphoneBrowse";
                bool laptop = objectName == "MD_Laptop" || objectName == "MD_GroundLaptop_S" ||
                    objectName == "MD_LaptopGround" || objectName == "MD_BedLaptop" ||
                    objectName == "MD_FloaterLaptop";
                bool gameConsole = objectName == "MD_ConsoleDeck";
                if (!phone && !laptop && !gameConsole)
                    continue;

                Material[] originals = renderer.sharedMaterials;
                if (originals == null || originals.Length == 0)
                    continue;
                bool[] visibleSlots = new bool[originals.Length];
                bool hasScreen = false;
                for (int m = 0; m < originals.Length; m++)
                {
                    Material original = originals[m];
                    if (original == null)
                        continue;
                    string materialName = original.name;
                    bool isScreen = materialName.StartsWith("M_LaptopScreen", StringComparison.OrdinalIgnoreCase) ||
                        (phone && materialName.StartsWith("White 5 - no secondary", StringComparison.OrdinalIgnoreCase)) ||
                        (gameConsole && materialName.StartsWith("M_DeckScreen", StringComparison.OrdinalIgnoreCase));
                    visibleSlots[m] = isScreen;
                    hasScreen |= isScreen;
                }
                if (!hasScreen)
                    continue;

                DeviceScreenState state = new DeviceScreenState();
                if (!TryCreateDeviceScreenOverlay(renderer, originals, visibleSlots, shader, state))
                {
                    skipped++;
                    continue;
                }
                state.Source = renderer;
                state.IsGameConsole = gameConsole;
                if (gameConsole)
                {
                    state.DisplayColor = GetGameConsoleScreenColor(0);
                    state.TargetDisplayColor = state.DisplayColor;
                    state.NextDisplayChangeAt = Time.unscaledTime + UnityEngine.Random.Range(1.8f, 3.4f);
                    SetDeviceScreenOverlayColor(state, state.DisplayColor);
                }
                CreateDeviceScreenLight(state);
                _deviceScreenStates.Add(state);
                overlays++;
                for (int m = 0; m < visibleSlots.Length; m++)
                    if (visibleSlots[m])
                        screenSlots++;
                if (renderer is SkinnedMeshRenderer)
                    skinnedOverlays++;
                else
                    meshOverlays++;
            }
            Logger.LogInfo("DEVICE SCREEN GLOW overlays=" + overlays + " slots=" + screenSlots +
                " mesh=" + meshOverlays + " skinned=" + skinnedOverlays + " skipped=" + skipped);
        }

        private static bool TryCreateDeviceScreenOverlay(Renderer source, Material[] originals, bool[] visibleSlots,
            Shader shader, DeviceScreenState state)
        {
            Material[] materials = new Material[originals.Length];
            for (int i = 0; i < originals.Length; i++)
                materials[i] = CreateDeviceScreenOverlayMaterial(shader, originals[i], visibleSlots[i]);

            GameObject overlay = new GameObject("LookCare Device Screen Overlay");
            overlay.layer = source.gameObject.layer;
            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            SkinnedMeshRenderer sourceSkin = source as SkinnedMeshRenderer;
            if (sourceFilter != null && sourceFilter.sharedMesh != null)
            {
                state.HasScreenAnchor = TryGetScreenMeshAnchor(sourceFilter.sharedMesh, visibleSlots,
                    out state.ScreenLocalCenter, out state.ScreenLocalNormal);
                overlay.transform.SetParent(source.transform, false);
                overlay.transform.localScale = Vector3.one * 1.001f;
                MeshFilter filter = overlay.AddComponent<MeshFilter>();
                filter.sharedMesh = sourceFilter.sharedMesh;
                MeshRenderer renderer = overlay.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                state.OverlayRenderer = renderer;
            }
            else if (sourceSkin != null && sourceSkin.sharedMesh != null)
            {
                overlay.transform.SetParent(source.transform.parent, false);
                overlay.transform.localPosition = source.transform.localPosition;
                overlay.transform.localRotation = source.transform.localRotation;
                overlay.transform.localScale = source.transform.localScale * 1.001f;
                SkinnedMeshRenderer renderer = overlay.AddComponent<SkinnedMeshRenderer>();
                renderer.sharedMesh = sourceSkin.sharedMesh;
                renderer.bones = sourceSkin.bones;
                renderer.rootBone = sourceSkin.rootBone;
                renderer.localBounds = sourceSkin.localBounds;
                renderer.updateWhenOffscreen = sourceSkin.updateWhenOffscreen;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                state.OverlayRenderer = renderer;
            }
            else
            {
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] != null)
                        Destroy(materials[i]);
                Destroy(overlay);
                return false;
            }

            state.Overlay = overlay;
            state.Materials = materials;
            if (state.OverlayRenderer != null)
                state.OverlayRenderer.enabled = source.enabled && source.gameObject.activeInHierarchy;
            return true;
        }

        private static Material CreateDeviceScreenOverlayMaterial(Shader shader, Material source, bool visible)
        {
            Material material = new Material(shader);
            material.name = visible ? "LookCare Device Screen Unlit" : "LookCare Device Screen Hidden Submesh";
            material.renderQueue = 3100;
            SetMaterialFloat(material, "_Surface", 1f);
            SetMaterialFloat(material, "_Blend", 0f);
            SetMaterialFloat(material, "_SrcBlend", 5f); // SrcAlpha
            SetMaterialFloat(material, "_DstBlend", 10f); // OneMinusSrcAlpha
            SetMaterialFloat(material, "_ZWrite", 0f);
            SetMaterialFloat(material, "_Cull", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            PreserveDestinationAlpha(material);
            Texture texture = GetScreenOverlayTexture(source);
            SetMaterialTexture(material, "_BaseMap", visible ? texture : null);
            SetMaterialTexture(material, "_MainTex", visible ? texture : null);
            // Clearly readable as a screen, but still cool white rather than a solid neon card.
            Color color = visible ? new Color(0.70f, 0.86f, 1f, 0.56f) : new Color(0f, 0f, 0f, 0f);
            SetMaterialColor(material, "_BaseColor", color);
            SetMaterialColor(material, "_Color", color);
            return material;
        }

        private static bool TryGetScreenMeshAnchor(Mesh mesh, bool[] visibleSlots, out Vector3 center,
            out Vector3 normal)
        {
            center = Vector3.zero;
            normal = Vector3.zero;
            if (mesh == null || visibleSlots == null || mesh.vertexCount == 0 || !mesh.isReadable)
                return false;
            try
            {
                Vector3[] vertices = mesh.vertices;
                int triangleCount = 0;
                for (int slot = 0; slot < visibleSlots.Length && slot < mesh.subMeshCount; slot++)
                {
                    if (!visibleSlots[slot])
                        continue;
                    int[] triangles = mesh.GetTriangles(slot);
                    for (int i = 0; i + 2 < triangles.Length; i += 3)
                    {
                        int a = triangles[i];
                        int b = triangles[i + 1];
                        int c = triangles[i + 2];
                        if (a < 0 || b < 0 || c < 0 || a >= vertices.Length || b >= vertices.Length || c >= vertices.Length)
                            continue;
                        Vector3 first = vertices[a];
                        Vector3 second = vertices[b];
                        Vector3 third = vertices[c];
                        center += (first + second + third) / 3f;
                        normal += Vector3.Cross(second - first, third - first);
                        triangleCount++;
                    }
                }
                if (triangleCount == 0 || normal.sqrMagnitude < 0.000001f)
                    return false;
                center /= triangleCount;
                normal.Normalize();
                return true;
            }
            catch
            {
                center = Vector3.zero;
                normal = Vector3.zero;
                return false;
            }
        }

        private static void CreateDeviceScreenLight(DeviceScreenState state)
        {
            if (state == null || state.Source == null)
                return;
            GameObject holder = new GameObject("LookCare Device Screen Light");
            holder.transform.position = state.Source.bounds.center;
            Light light = holder.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = state.IsGameConsole ? state.DisplayColor : new Color(0.72f, 0.86f, 1f, 1f);
            light.range = state.IsGameConsole ? 0.62f : 0.75f;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            state.Light = light;
        }

        private void UpdateDeviceScreenLights()
        {
            if (_deviceScreenStates.Count == 0)
                return;
            float nightAmount = 1f - Mathf.Clamp01(_lastDayWeight);
            float intensity = Mathf.Lerp(0.025f, 0.16f, nightAmount);
            for (int i = _deviceScreenStates.Count - 1; i >= 0; i--)
            {
                DeviceScreenState state = _deviceScreenStates[i];
                if (state == null || state.Light == null || state.Source == null)
                {
                    _deviceScreenStates.RemoveAt(i);
                    continue;
                }
                bool active = state.Source.enabled && state.Source.gameObject.activeInHierarchy;
                if (state.OverlayRenderer != null)
                    state.OverlayRenderer.enabled = active;
                state.Light.enabled = active;
                if (!active)
                    continue;
                Vector3 position = state.Source.bounds.center;
                if (state.HasScreenAnchor)
                {
                    Vector3 normal = state.Source.transform.TransformDirection(state.ScreenLocalNormal).normalized;
                    // Place the light just in front of the actual screen plane. This is what
                    // keeps landscape-held phones from lighting through their back or staying
                    // at the device's geometric centre.
                    position = state.Source.transform.TransformPoint(state.ScreenLocalCenter) + normal * 0.035f;
                }
                state.Light.transform.position = position;
                if (state.IsGameConsole)
                {
                    UpdateGameConsoleScreen(state);
                    state.Light.color = state.DisplayColor;
                    // A very small colour spill makes the display feel alive without
                    // turning a hand-held console into a flashlight.
                    state.Light.intensity = intensity * 0.55f;
                }
                else
                    state.Light.intensity = intensity;
            }
        }

        private static Color GetGameConsoleScreenColor(int index)
        {
            Color[] colours =
            {
                new Color(0.54f, 0.88f, 1.00f, 0.64f),
                new Color(0.58f, 0.98f, 0.72f, 0.64f),
                new Color(0.82f, 0.68f, 1.00f, 0.64f),
                new Color(1.00f, 0.77f, 0.48f, 0.64f),
                new Color(1.00f, 0.64f, 0.80f, 0.64f)
            };
            return colours[Mathf.Abs(index) % colours.Length];
        }

        private static void SetDeviceScreenOverlayColor(DeviceScreenState state, Color color)
        {
            if (state == null || state.Materials == null)
                return;
            for (int i = 0; i < state.Materials.Length; i++)
            {
                Material material = state.Materials[i];
                if (material == null || material.name.IndexOf("Device Screen Unlit", StringComparison.Ordinal) < 0)
                    continue;
                SetMaterialColor(material, "_BaseColor", color);
                SetMaterialColor(material, "_Color", color);
            }
        }

        private static void UpdateGameConsoleScreen(DeviceScreenState state)
        {
            float now = Time.unscaledTime;
            if (now >= state.NextDisplayChangeAt)
            {
                Color next = GetGameConsoleScreenColor(UnityEngine.Random.Range(0, 5));
                // Roughly one out of three changes is an abrupt in-game scene/menu switch;
                // the others ease to the next colour like an animated game display.
                if (UnityEngine.Random.value < 0.33f)
                    state.DisplayColor = next;
                state.TargetDisplayColor = next;
                state.NextDisplayChangeAt = now + UnityEngine.Random.Range(2.2f, 5.5f);
            }
            state.DisplayColor = Color.Lerp(state.DisplayColor, state.TargetDisplayColor,
                1f - Mathf.Exp(-3.5f * Time.unscaledDeltaTime));
            SetDeviceScreenOverlayColor(state, state.DisplayColor);
        }

        private static Texture GetScreenOverlayTexture(Material material)
        {
            if (material == null)
                return Texture2D.whiteTexture;
            if (material.HasProperty("_BaseMap"))
                return material.GetTexture("_BaseMap") ?? Texture2D.whiteTexture;
            if (material.HasProperty("_MainTex"))
                return material.GetTexture("_MainTex") ?? Texture2D.whiteTexture;
            return Texture2D.whiteTexture;
        }

        private void RestoreDeviceScreenGlow()
        {
            for (int i = 0; i < _deviceScreenStates.Count; i++)
            {
                DeviceScreenState state = _deviceScreenStates[i];
                if (state == null)
                    continue;
                if (state.Light != null)
                    Destroy(state.Light.gameObject);
                if (state.Overlay != null)
                    Destroy(state.Overlay);
                if (state.Materials == null)
                    continue;
                for (int m = 0; m < state.Materials.Length; m++)
                {
                    if (state.Materials[m] != null)
                        Destroy(state.Materials[m]);
                }
            }
            _deviceScreenStates.Clear();
        }

        // ---------------------------------------------------------------
        // Licensed nature ambience: subtle CC0 field recordings loaded
        // from the plugin's audio directory and crossfaded by sun height.
        // ---------------------------------------------------------------
        private void EnsureNatureAudio()
        {
            if (!_natureAmbienceEnabled.Value)
                return;
            if (_natureAudioObject == null)
            {
                _natureAudioObject = new GameObject("LookCare Nature Ambience");
                DontDestroyOnLoad(_natureAudioObject);
                _cicadaAudioSource = CreateNatureAudioSource(_natureAudioObject, "Noon Cicadas");
                _nightNatureAudioSource = CreateNatureAudioSource(_natureAudioObject, "Night Insects and Frogs");
            }
            if (_natureAudioLoadAttempted || _natureAudioLoadRoutine != null)
                return;
            _natureAudioLoadAttempted = true;
            _natureAudioLoadRoutine = StartCoroutine(LoadNatureAudioClips());
        }

        private static AudioSource CreateNatureAudioSource(GameObject holder, string sourceName)
        {
            AudioSource source = holder.AddComponent<AudioSource>();
            source.name = sourceName;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.volume = 0f;
            return source;
        }

        private IEnumerator LoadNatureAudioClips()
        {
            string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string audioDirectory = Path.Combine(pluginDirectory, "audio");
            yield return StartCoroutine(LoadNatureAudioClip(
                Path.Combine(audioDirectory, "noon_cicadas_cc0.ogg"),
                delegate(AudioClip clip) { _cicadaAudioClip = clip; }));
            yield return StartCoroutine(LoadNatureAudioClip(
                Path.Combine(audioDirectory, "night_crickets_frogs_cc0.ogg"),
                delegate(AudioClip clip) { _nightNatureAudioClip = clip; }));

            if (_cicadaAudioSource != null)
                _cicadaAudioSource.clip = _cicadaAudioClip;
            if (_nightNatureAudioSource != null)
                _nightNatureAudioSource.clip = _nightNatureAudioClip;
            _natureAudioLoadRoutine = null;
            Logger.LogInfo("NATURE AUDIO ready cicadas=" + DescribeAudioClip(_cicadaAudioClip) +
                " night=" + DescribeAudioClip(_nightNatureAudioClip));
        }

        private IEnumerator LoadNatureAudioClip(string path, Action<AudioClip> assign)
        {
            if (!File.Exists(path))
            {
                Logger.LogWarning("NATURE AUDIO missing file=" + path);
                yield break;
            }

            string uri = new Uri(path).AbsoluteUri;
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS))
            {
                DownloadHandlerAudioClip handler = request.downloadHandler as DownloadHandlerAudioClip;
                if (handler != null)
                    handler.streamAudio = true;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Logger.LogWarning("NATURE AUDIO load failed file=" + Path.GetFileName(path) +
                        " error=" + request.error);
                    yield break;
                }
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    clip.name = "LookCare " + Path.GetFileNameWithoutExtension(path);
                    assign(clip);
                }
            }
        }

        private static string DescribeAudioClip(AudioClip clip)
        {
            return clip != null ? clip.name + "(" + clip.length.ToString("F1") + "s)" : "missing";
        }

        private void UpdateNatureAmbience()
        {
            if (_natureAmbienceEnabled.Value)
                EnsureNatureAudio();
            if (_natureAudioObject == null)
                return;

            float worldWeight = _natureAmbienceEnabled.Value && _worldSceneReady ? 1f : 0f;
            float noonWeight = 0f;
            float nightWeight = 0f;
            if (_celestialValid && _dayCycleEnabled.Value)
            {
                noonWeight = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01((_lastSignedElevation - 18f) / 28f));
                nightWeight = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01((-_lastSignedElevation - 6f) / 14f));
            }
            else if (!_dayCycleEnabled.Value)
            {
                noonWeight = 1f;
            }

            float rainScale = Mathf.Lerp(1f, Mathf.Clamp01(_rainNatureMultiplier.Value), _rainBlend);
            float cicadaTarget = Mathf.Clamp01(_noonCicadaVolume.Value) * noonWeight * worldWeight * rainScale;
            float nightTarget = Mathf.Clamp01(_nightNatureVolume.Value) * nightWeight * worldWeight * rainScale;
            float fadeSeconds = Mathf.Clamp(_natureFadeSeconds.Value, 2f, 40f);
            float blend = 1f - Mathf.Exp(-Mathf.Min(Time.unscaledDeltaTime, 0.1f) * 4.6f / fadeSeconds);

            UpdateNatureSource(_cicadaAudioSource, cicadaTarget, blend);
            UpdateNatureSource(_nightNatureAudioSource, nightTarget, blend);

            if (_worldSceneReady && (_cicadaAudioClip != null || _nightNatureAudioClip != null))
            {
                int phase = cicadaTarget > 0.001f ? 0 : (nightTarget > 0.001f ? 2 : 1);
                if (phase != _lastNatureAudioPhase)
                {
                    _lastNatureAudioPhase = phase;
                    Logger.LogInfo("NATURE AUDIO phase=" + (phase == 0 ? "noon-cicadas" :
                        (phase == 2 ? "night-insects-frogs" : "quiet-twilight")) +
                        " elevation=" + _lastSignedElevation.ToString("F1") +
                        " rain=" + _rainBlend.ToString("F2"));
                }
            }
        }

        private void UpdateNatureSource(AudioSource source, float targetVolume, float blend)
        {
            if (source == null)
                return;
            source.volume = Mathf.Lerp(source.volume, targetVolume, blend);
            if (targetVolume > 0.0005f && source.clip != null && !source.isPlaying)
            {
                if (source.clip.length > 1f)
                    source.time = (float)_weatherRandom.NextDouble() * (source.clip.length - 0.1f);
                source.Play();
            }
            else if (targetVolume <= 0.0005f && source.volume <= 0.0006f && source.isPlaying)
            {
                source.Stop();
            }
        }

        private void RestoreNatureAudio()
        {
            if (_natureAudioLoadRoutine != null)
                StopCoroutine(_natureAudioLoadRoutine);
            _natureAudioLoadRoutine = null;
            if (_natureAudioObject != null)
                Destroy(_natureAudioObject);
            _natureAudioObject = null;
            _cicadaAudioSource = null;
            _nightNatureAudioSource = null;
            if (_cicadaAudioClip != null)
                Destroy(_cicadaAudioClip);
            if (_nightNatureAudioClip != null)
                Destroy(_nightNatureAudioClip);
            _cicadaAudioClip = null;
            _nightNatureAudioClip = null;
            _natureAudioLoadAttempted = false;
            _lastNatureAudioPhase = -1;
        }

        private void CaptureWeatherFog()
        {
            if (_weatherFogCaptured)
                return;
            _weatherFogCaptured = true;
            _originalFogEnabled = RenderSettings.fog;
            _originalFogMode = RenderSettings.fogMode;
            _originalFogColor = RenderSettings.fogColor;
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogStart = RenderSettings.fogStartDistance;
            _originalFogEnd = RenderSettings.fogEndDistance;
        }

        private void UpdateWeatherFogAndAmbient()
        {
            CaptureWeatherFog();
            if (_rainBlend > 0.001f)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = Color.Lerp(_originalFogColor,
                    new Color(0.39f, 0.47f, 0.58f, 1f), _rainBlend);
                RenderSettings.fogDensity = Mathf.Lerp(Mathf.Max(0f, _originalFogDensity), 0.0042f, _rainBlend);
            }
            else
            {
                RestoreWeatherFogValues();
            }
            if (_ambientCaptured)
            {
                float weatherAmbient = Mathf.Lerp(1f, 0.72f, _rainBlend) *
                    (1f + _lightningFlash * 1.1f);
                RenderSettings.ambientIntensity = _origAmbientIntensity *
                    Mathf.Clamp(_ambientIntensity.Value, 0.2f, 2f) * weatherAmbient;
            }
        }

        private void RestoreWeatherFogValues()
        {
            if (!_weatherFogCaptured)
                return;
            RenderSettings.fog = _originalFogEnabled;
            RenderSettings.fogMode = _originalFogMode;
            RenderSettings.fogColor = _originalFogColor;
            RenderSettings.fogDensity = _originalFogDensity;
            RenderSettings.fogStartDistance = _originalFogStart;
            RenderSettings.fogEndDistance = _originalFogEnd;
        }

        private void RestoreWeatherFog()
        {
            RestoreWeatherFogValues();
            _weatherFogCaptured = false;
        }

        private void UpdateWeatherCloudColors()
        {
            if (_cloudMaterial == null)
                return;
            Color baseColor = Color.Lerp(new Color(0.99f, 0.995f, 1f, 1f),
                new Color(0.56f, 0.62f, 0.72f, 1f), _rainBlend);
            Color firstShade = Color.Lerp(new Color(0.84f, 0.88f, 0.97f, 1f),
                new Color(0.34f, 0.40f, 0.51f, 1f), _rainBlend);
            Color secondShade = Color.Lerp(new Color(0.74f, 0.80f, 0.93f, 1f),
                new Color(0.22f, 0.28f, 0.39f, 1f), _rainBlend);
            SetMaterialColor(_cloudMaterial, "_BaseColor", baseColor);
            SetMaterialColor(_cloudMaterial, "_Color", baseColor);
            SetMaterialColor(_cloudMaterial, "_1st_ShadeColor", firstShade);
            SetMaterialColor(_cloudMaterial, "_2nd_ShadeColor", secondShade);
        }

        private void ClearStormClouds()
        {
            if (_stormCloudRoot != null)
                Destroy(_stormCloudRoot);
            _stormCloudRoot = null;
            _stormCloudTransforms.Clear();
            _stormCloudBaseScales.Clear();
            for (int i = 0; i < _stormCloudMeshes.Count; i++)
            {
                if (_stormCloudMeshes[i] != null)
                    Destroy(_stormCloudMeshes[i]);
            }
            _stormCloudMeshes.Clear();
            if (_stormCloudMaterial != null)
                Destroy(_stormCloudMaterial);
            _stormCloudMaterial = null;
        }

        private void RestoreWeather()
        {
            _rainTarget = false;
            _rainBlend = 0f;
            _lightningFlash = 0f;
            RestoreWeatherFog();
            ClearStormClouds();
            if (_rainObject != null) Destroy(_rainObject);
            if (_rainMaterial != null) Destroy(_rainMaterial);
            if (_rainTexture != null) Destroy(_rainTexture);
            if (_splashObject != null) Destroy(_splashObject);
            if (_splashMaterial != null) Destroy(_splashMaterial);
            if (_splashTexture != null) Destroy(_splashTexture);
            _rainObject = null;
            _rainParticles = null;
            _rainMaterial = null;
            _rainTexture = null;
            _rainUsesNativeMaterial = false;
            _splashObject = null;
            _splashParticles = null;
            _splashMaterial = null;
            _splashTexture = null;
            _splashAccumulator = 0f;
            _splashDiagnosticWritten = false;
            if (_weatherAudioObject != null) Destroy(_weatherAudioObject);
            _weatherAudioObject = null;
            _rainAudioSource = null;
            _rainDetailAudioSource = null;
            _thunderAudioSource = null;
            _rainAudioClip = null;
            _rainDetailAudioClip = null;
            _thunderAudioClip = null;
            _weatherAudioPairSignature = "";
            _weatherAudioWarningWritten = false;
            UpdateWeatherCloudColors();
        }

        // ---------------------------------------------------------------
        // Clouds: procedural puffy meshes drifting with the wind. They
        // use the game's own Toon shader with light-color binding ON, so
        // they warm at dusk and darken under the moon automatically, and
        // as real opaque casters their shadows sweep across the island.
        // ---------------------------------------------------------------
        private void BuildClouds()
        {
            ClearClouds();
            if (!_cloudsEnabled.Value)
                return;
            if (_cloudMaterial == null)
            {
                // Clone a live scene Toon material rather than instantiating the bare shader:
                // a fresh material has no shader_feature keywords enabled and that variant may
                // be stripped from the build (same trap as the skybox _SUNDISK keywords).
                Material donor = FindCloudDonorMaterial();
                if (donor != null)
                {
                    _cloudMaterial = new Material(donor);
                }
                else
                {
                    Shader toon = Shader.Find("Toon");
                    if (toon == null)
                    {
                        Logger.LogWarning("No Toon material or shader found; clouds unavailable.");
                        return;
                    }
                    _cloudMaterial = new Material(toon);
                }
                _cloudMaterial.name = "LookCare Cloud Material";
                SetMaterialTexture(_cloudMaterial, "_MainTex", Texture2D.whiteTexture);
                SetMaterialTexture(_cloudMaterial, "_BaseMap", Texture2D.whiteTexture);
                SetMaterialTexture(_cloudMaterial, "_1st_ShadeMap", Texture2D.whiteTexture);
                SetMaterialTexture(_cloudMaterial, "_2nd_ShadeMap", Texture2D.whiteTexture);
                SetMaterialTexture(_cloudMaterial, "_ClippingMask", Texture2D.whiteTexture);
                SetMaterialFloat(_cloudMaterial, "_Use_BaseAs1st", 1f);
                SetMaterialFloat(_cloudMaterial, "_Use_1stAs2nd", 1f);
                SetMaterialColor(_cloudMaterial, "_BaseColor", new Color(0.99f, 0.995f, 1f, 1f));
                SetMaterialColor(_cloudMaterial, "_Color", new Color(0.99f, 0.995f, 1f, 1f));
                SetMaterialColor(_cloudMaterial, "_1st_ShadeColor", new Color(0.84f, 0.88f, 0.97f, 1f));
                SetMaterialColor(_cloudMaterial, "_2nd_ShadeColor", new Color(0.74f, 0.80f, 0.93f, 1f));
                // Kill donor extras that make no sense on a cloud.
                SetMaterialColor(_cloudMaterial, "_HighColor", Color.black);
                SetMaterialColor(_cloudMaterial, "_RimLightColor", Color.black);
                SetMaterialColor(_cloudMaterial, "_MatCapColor", Color.black);
                SetMaterialColor(_cloudMaterial, "_Emissive_Color", Color.black);
                // Track the sun/moon color so clouds blush at dusk and dim at night.
                SetMaterialFloat(_cloudMaterial, "_Is_LightColor_Base", 1f);
                SetMaterialFloat(_cloudMaterial, "_Is_LightColor_1st_Shade", 1f);
                SetMaterialFloat(_cloudMaterial, "_Is_LightColor_2nd_Shade", 1f);
                SetMaterialFloat(_cloudMaterial, "_Set_SystemShadowsToBase", 0f);
                SetMaterialFloat(_cloudMaterial, "_BaseColor_Step", 0.42f);
                SetMaterialFloat(_cloudMaterial, "_ShadeColor_Step", 0.25f);
                SetMaterialFloat(_cloudMaterial, "_Outline_Width", 0f);
                Logger.LogInfo("CLOUD material donor=" + (donor != null ? donor.name : "<bare Toon shader>"));
            }

            _cloudRoot = new GameObject("LookCare Clouds");
            // Fixed seed: the same cloud shapes every session, only the drift changes.
            System.Random random = new System.Random(978231);
            int count = Mathf.Clamp(_cloudCount.Value, 1, 40);
            float altitude = Mathf.Clamp(_cloudAltitude.Value, 25f, 150f);
            float scaleBase = Mathf.Clamp(_cloudScale.Value, 0.3f, 3f);
            Camera camera = Camera.main;
            Vector3 center = camera != null ? camera.transform.position : Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                Mesh mesh = CreateCloudMesh(random);
                _cloudMeshList.Add(mesh);
                GameObject cloud = new GameObject("LookCare Cloud " + i);
                cloud.transform.SetParent(_cloudRoot.transform, false);
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = Mathf.Sqrt((float)random.NextDouble()) * CloudFieldRadius;
                cloud.transform.position = new Vector3(center.x + Mathf.Cos(angle) * radius,
                    altitude + ((float)random.NextDouble() - 0.5f) * 14f,
                    center.z + Mathf.Sin(angle) * radius);
                cloud.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                Vector3 baseScale = Vector3.one *
                    (scaleBase * (0.8f + (float)random.NextDouble() * 0.9f));
                cloud.transform.localScale = baseScale;
                MeshFilter filter = cloud.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = cloud.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = _cloudMaterial;
                renderer.shadowCastingMode = _cloudShadows.Value
                    ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                _cloudTransforms.Add(cloud.transform);
                _cloudBaseScales.Add(baseScale);
            }
            Logger.LogInfo("CLOUDS built=" + count + " altitude=" + altitude + " shadows=" +
                _cloudShadows.Value + " windSpeed=" + _cloudSpeed.Value + " windDir=" +
                _cloudDirection.Value);
        }

        private void UpdateClouds()
        {
            int count = _cloudTransforms.Count;
            if (count == 0)
                return;
            float clearVisibility = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.05f, 0.72f, _rainBlend));
            if (_cloudRoot != null)
                _cloudRoot.SetActive(clearVisibility > 0.005f);
            Camera camera = Camera.main;
            Vector3 center = camera != null ? camera.transform.position : Vector3.zero;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            Vector3 wind = Quaternion.Euler(0f, Mathf.Repeat(_cloudDirection.Value, 360f), 0f) *
                Vector3.forward * (Mathf.Clamp(_cloudSpeed.Value, 0f, 30f) * dt);
            for (int i = 0; i < count; i++)
            {
                Transform cloud = _cloudTransforms[i];
                if (cloud == null)
                    continue;
                if (i < _cloudBaseScales.Count)
                    cloud.localScale = _cloudBaseScales[i] * clearVisibility;
                Vector3 position = cloud.position + wind;
                // Keep the field centered on the camera: a cloud drifting out one side
                // re-enters from the other, so cover never runs out.
                float dx = position.x - center.x;
                float dz = position.z - center.z;
                if (dx > CloudFieldRadius)
                    dx -= CloudFieldRadius * 2f;
                else if (dx < -CloudFieldRadius)
                    dx += CloudFieldRadius * 2f;
                if (dz > CloudFieldRadius)
                    dz -= CloudFieldRadius * 2f;
                else if (dz < -CloudFieldRadius)
                    dz += CloudFieldRadius * 2f;
                cloud.position = new Vector3(center.x + dx, position.y, center.z + dz);
            }
        }

        private void ClearClouds()
        {
            if (_cloudRoot != null)
                Destroy(_cloudRoot);
            _cloudRoot = null;
            _cloudTransforms.Clear();
            _cloudBaseScales.Clear();
            for (int i = 0; i < _cloudMeshList.Count; i++)
            {
                if (_cloudMeshList[i] != null)
                    Destroy(_cloudMeshList[i]);
            }
            _cloudMeshList.Clear();
        }

        private void ApplyVanillaCloudHiding()
        {
            if (!_hideVanillaClouds.Value)
            {
                RestoreVanillaClouds();
                return;
            }
            int hidden = 0;
            int rootsHidden = 0;
            int instancersHidden = 0;

            // This is the actual renderer for the static two-lobe vanilla clouds.
            // CloudInstancing.Start disables its source MeshRenderers, then Update bypasses
            // every Renderer flag and submits the cached matrices through
            // Graphics.RenderMeshInstanced. Disabling PR_CloudBase or its renderers alone
            // therefore cannot stop the visible batch.
            CloudInstancing[] instancers =
                UnityEngine.Object.FindObjectsByType<CloudInstancing>(FindObjectsSortMode.None);
            for (int i = 0; i < instancers.Length; i++)
            {
                CloudInstancing instancing = instancers[i];
                if (instancing == null || IsLookCareTransform(instancing.transform))
                    continue;
                int id = instancing.GetInstanceID();
                if (!_hiddenCloudInstancers.ContainsKey(id))
                {
                    CloudInstancingState state = new CloudInstancingState();
                    state.Instancing = instancing;
                    state.OriginalEnabled = instancing.enabled;
                    _hiddenCloudInstancers.Add(id, state);
                }
                if (instancing.enabled)
                {
                    string meshName = instancing.CloudMesh != null ? instancing.CloudMesh.name : "<null>";
                    string materialName = instancing.ToonInstanceMat != null
                        ? instancing.ToonInstanceMat.name : "<null>";
                    int instanceCount = instancing.CloudBatches != null
                        ? instancing.CloudBatches.Count : -1;
                    instancing.enabled = false;
                    instancersHidden++;
                    Logger.LogInfo("WORLD CLOUD INSTANCER disabled path=" +
                        GetTransformPath(instancing.transform) + " instances=" + instanceCount +
                        " mesh=" + meshName + " material=" + materialName);
                }
            }

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsLookCareTransform(renderer.transform))
                    continue;
                if (!IsVanillaCloudRenderer(renderer))
                {
                    LogCloudSuspect(renderer);
                    if (!IsSkyAltitudeCloud(renderer))
                        continue;
                }
                int id = renderer.GetInstanceID();
                bool wasHidden = renderer.forceRenderingOff && !renderer.enabled;
                if (!_hiddenVanillaClouds.ContainsKey(id))
                {
                    HiddenCloudRendererState state = new HiddenCloudRendererState();
                    state.Renderer = renderer;
                    state.OriginalForceRenderingOff = renderer.forceRenderingOff;
                    state.OriginalEnabled = renderer.enabled;
                    _hiddenVanillaClouds.Add(id, state);
                }
                // Belt and braces: forceRenderingOff alone is not reliable for statically
                // batched scene geometry (the batch keeps drawing the sub-range), which is
                // exactly what baked cloud prefabs are. Renderer.enabled=false does remove
                // batched sub-ranges; the 5s rescan re-disables anything a LODGroup re-enables.
                renderer.forceRenderingOff = true;
                renderer.enabled = false;
                if (!wasHidden)
                    hidden++;
            }

            // Deactivate the cloud prefab roots outright as well: CloudBatches (menu-scene
            // moving layer) and the world-scene PR_CloudBase prefabs. An inactive root beats
            // both LODGroups and static batching. Do not touch MD_Cloud, fx_weather cloud or
            // Player descendants: those are menu and player-local ambience props.
            Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (transform == null)
                    continue;
                string normalizedRoot = NormalizeSceneName(transform.gameObject.name);
                if ((normalizedRoot != "cloudbatches" && !normalizedRoot.StartsWith("prcloudbase")) ||
                    IsLookCareTransform(transform))
                    continue;
                GameObject root = transform.gameObject;
                int id = root.GetInstanceID();
                if (!_hiddenWorldCloudRoots.ContainsKey(id))
                {
                    HiddenWorldCloudRootState state = new HiddenWorldCloudRootState();
                    state.Root = root;
                    state.OriginalActive = root.activeSelf;
                    _hiddenWorldCloudRoots.Add(id, state);
                }
                if (root.activeSelf)
                {
                    Logger.LogInfo("WORLD CLOUD ROOT hidden path=" + GetTransformPath(root.transform));
                    root.SetActive(false);
                    rootsHidden++;
                }
            }
            if (hidden > 0 || rootsHidden > 0 || instancersHidden > 0)
                Logger.LogInfo("WORLD CLOUDS renderersHidden=" + hidden + " rootsHidden=" + rootsHidden +
                    " instancersHidden=" + instancersHidden +
                    " trackedRenderers=" + _hiddenVanillaClouds.Count +
                    " trackedRoots=" + _hiddenWorldCloudRoots.Count +
                    " trackedInstancers=" + _hiddenCloudInstancers.Count);
        }

        // Evidence trail: any renderer that mentions "cloud" in its name or materials but
        // was NOT matched by the hider gets logged once, so a stray authored cloud that
        // slips past the current rules shows up in the log with its full path.
        private void LogCloudSuspect(Renderer renderer)
        {
            int id = renderer.GetInstanceID();
            if (_cloudSuspectsLogged.Contains(id))
                return;
            bool suspect = NormalizeSceneName(renderer.gameObject.name).IndexOf("cloud") >= 0;
            Material[] materials = renderer.sharedMaterials;
            if (!suspect)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (material != null && material.name.ToLowerInvariant().IndexOf("cloud") >= 0)
                    {
                        suspect = true;
                        break;
                    }
                }
            }
            if (!suspect)
                return;
            _cloudSuspectsLogged.Add(id);
            string materialNames = "";
            for (int i = 0; i < materials.Length; i++)
            {
                if (i > 0)
                    materialNames += ",";
                materialNames += materials[i] != null ? materials[i].name : "<null>";
            }
            Logger.LogInfo("CLOUD SUSPECT not hidden path=" + GetTransformPath(renderer.transform) +
                " materials=" + materialNames);
        }

        // Mechanism-based cloud catch: authored sky clouds are the only wide, flat
        // renderers that FLOAT — their entire bounds sit high above the terrain.
        // Anything attached to the ground (trees on hills, towers, the lighthouse)
        // has a low bounds.min.y however tall it gets, so it can never match.
        // Everything floating high is also logged once as evidence.
        private bool IsSkyAltitudeCloud(Renderer renderer)
        {
            if (renderer.GetComponentInParent<Canvas>() != null)
                return false;
            Bounds bounds = renderer.bounds;
            float width = Mathf.Max(bounds.size.x, bounds.size.z);
            if (bounds.min.y < 30f || width < 6f)
                return false;
            bool hide = bounds.min.y >= 40f && width >= 10f && bounds.size.y <= width * 0.7f;
            if (_skyRenderersLogged.Add(renderer.GetInstanceID()))
            {
                Material[] materials = renderer.sharedMaterials;
                string materialNames = "";
                for (int i = 0; i < materials.Length; i++)
                {
                    if (i > 0)
                        materialNames += ",";
                    materialNames += materials[i] != null ? materials[i].name : "<null>";
                }
                Logger.LogInfo("SKY RENDERER " + (hide ? "auto-hidden" : "observed") +
                    " path=" + GetTransformPath(renderer.transform) +
                    " minY=" + bounds.min.y.ToString("F0") +
                    " size=(" + bounds.size.x.ToString("F0") + "," + bounds.size.y.ToString("F0") +
                    "," + bounds.size.z.ToString("F0") + ") materials=" + materialNames);
            }
            return hide;
        }

        private static bool IsVanillaCloudRenderer(Renderer renderer)
        {
            if (renderer == null || IsLookCareTransform(renderer.transform))
                return false;
            // The authored sky clouds come in two shapes: the CloudBatches root (menu-scene
            // moving layer) and the world-scene PR_CloudBase/MD_CloudBase prefabs, whose
            // renderers all use the M_Clouds / M_Clouds Dark materials. Match both the
            // hierarchy names and the material family so streamed LOD copies are caught too.
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null &&
                    material.name.StartsWith("M_Clouds", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            Transform current = renderer.transform;
            while (current != null)
            {
                string normalized = NormalizeSceneName(current.gameObject.name);
                if (normalized == "cloudbatches" || normalized.IndexOf("cloudbase") >= 0)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static bool IsLookCareTransform(Transform transform)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.gameObject.name.StartsWith("LookCare", StringComparison.Ordinal))
                    return true;
                current = current.parent;
            }
            return false;
        }

        private void RestoreVanillaClouds()
        {
            foreach (KeyValuePair<int, CloudInstancingState> pair in _hiddenCloudInstancers)
            {
                CloudInstancingState state = pair.Value;
                if (state != null && state.Instancing != null)
                    state.Instancing.enabled = state.OriginalEnabled;
            }
            _hiddenCloudInstancers.Clear();
            foreach (KeyValuePair<int, HiddenCloudRendererState> pair in _hiddenVanillaClouds)
            {
                HiddenCloudRendererState state = pair.Value;
                if (state != null && state.Renderer != null)
                {
                    state.Renderer.forceRenderingOff = state.OriginalForceRenderingOff;
                    state.Renderer.enabled = state.OriginalEnabled;
                }
            }
            _hiddenVanillaClouds.Clear();
            foreach (KeyValuePair<int, HiddenWorldCloudRootState> pair in _hiddenWorldCloudRoots)
            {
                HiddenWorldCloudRootState state = pair.Value;
                if (state != null && state.Root != null)
                    state.Root.SetActive(state.OriginalActive);
            }
            _hiddenWorldCloudRoots.Clear();
        }

        private static Mesh CreateCloudMesh(System.Random random)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            int lobeCount = 3 + random.Next(4);
            for (int lobe = 0; lobe < lobeCount; lobe++)
            {
                Vector3 lobeCenter;
                float lobeRadius;
                if (lobe == 0)
                {
                    lobeCenter = Vector3.zero;
                    lobeRadius = 6f + (float)random.NextDouble() * 4f;
                }
                else
                {
                    float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                    float offset = 4f + (float)random.NextDouble() * 7f;
                    lobeCenter = new Vector3(Mathf.Cos(angle) * offset,
                        (float)random.NextDouble() * 1.4f,
                        Mathf.Sin(angle) * offset * 0.7f);
                    lobeRadius = 3f + (float)random.NextDouble() * 4f;
                }
                AppendCloudLobe(vertices, normals, uvs, triangles, lobeCenter, lobeRadius);
            }

            // Flat cartoon underside.
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertex = vertices[i];
                if (vertex.y < -1.2f)
                {
                    vertex.y = -1.2f;
                    vertices[i] = vertex;
                    normals[i] = Vector3.down;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "LookCare Cloud";
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AppendCloudLobe(List<Vector3> vertices, List<Vector3> normals,
            List<Vector2> uvs, List<int> triangles, Vector3 center, float radius)
        {
            const int rings = 7;
            const int segments = 10;
            int baseIndex = vertices.Count;
            for (int r = 0; r <= rings; r++)
            {
                float theta = Mathf.PI * r / rings;
                float y = Mathf.Cos(theta);
                float ringRadius = Mathf.Sin(theta);
                for (int s = 0; s < segments; s++)
                {
                    float phi = Mathf.PI * 2f * s / segments;
                    Vector3 unit = new Vector3(ringRadius * Mathf.Cos(phi), y, ringRadius * Mathf.Sin(phi));
                    vertices.Add(center + Vector3.Scale(unit, new Vector3(radius, radius * 0.5f, radius)));
                    normals.Add(unit);
                    uvs.Add(new Vector2(0.5f, 0.5f));
                }
            }
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int next = (s + 1) % segments;
                    int a = baseIndex + r * segments + s;
                    int b = baseIndex + r * segments + next;
                    int c = baseIndex + (r + 1) * segments + s;
                    int d = baseIndex + (r + 1) * segments + next;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(d);
                    triangles.Add(c);
                }
            }
        }

        private Material FindCloudDonorMaterial()
        {
            for (int i = 0; i < _toonMaterials.Count; i++)
            {
                ToonMaterial candidate = _toonMaterials[i];
                if (candidate != null && candidate.Material != null && candidate.Has1st &&
                    candidate.Material.shader != null && candidate.Material.shader.name == "Toon")
                    return candidate.Material;
            }
            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name.StartsWith("LookCare"))
                    continue;
                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material != null && material.shader != null &&
                        material.shader.name == "Toon" && material.HasProperty("_1st_ShadeColor"))
                        return material;
                }
            }
            return null;
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        private static void SetMaterialTexture(Material material, string propertyName, Texture value)
        {
            if (material.HasProperty(propertyName))
                material.SetTexture(propertyName, value);
        }

        private static void SetMaterialColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
                material.SetColor(propertyName, value);
        }

        // ---------------------------------------------------------------
        // Ambient (the light that fills shadowed areas)
        // ---------------------------------------------------------------
        private void ApplyAmbient()
        {
            if (!_ambientCaptured)
            {
                _origAmbientMode = RenderSettings.ambientMode;
                _origAmbientLight = RenderSettings.ambientLight;
                _origAmbientSky = RenderSettings.ambientSkyColor;
                _origAmbientEquator = RenderSettings.ambientEquatorColor;
                _origAmbientGround = RenderSettings.ambientGroundColor;
                _origAmbientIntensity = RenderSettings.ambientIntensity;
                _origReflectionIntensity = RenderSettings.reflectionIntensity;
                _ambientCaptured = true;
            }

            float shift = Mathf.Clamp01(_ambientCoolShift.Value);
            float intensity = Mathf.Clamp(_ambientIntensity.Value, 0.2f, 2f);
            Color coolTone = new Color(0.66f, 0.72f, 0.82f, 1f);

            if (_origAmbientMode == AmbientMode.Flat)
            {
                RenderSettings.ambientLight = CoolAmbient(_origAmbientLight, coolTone, shift) * intensity;
            }
            else if (_origAmbientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = CoolAmbient(_origAmbientSky, coolTone, shift) * intensity;
                RenderSettings.ambientEquatorColor = CoolAmbient(_origAmbientEquator, coolTone, shift) * intensity;
                RenderSettings.ambientGroundColor = CoolAmbient(_origAmbientGround, coolTone, shift) * intensity;
            }
            else
            {
                RenderSettings.ambientIntensity = _origAmbientIntensity * intensity;
            }
            Logger.LogInfo("AMBIENT applied: mode=" + _origAmbientMode + " coolShift=" + shift +
                " intensity=" + intensity);
        }

        private static Color CoolAmbient(Color original, Color coolTone, float shift)
        {
            Color target = coolTone * original.grayscale;
            return Color.Lerp(original, target, shift);
        }

        private void RestoreAmbient()
        {
            if (!_ambientCaptured)
                return;
            RenderSettings.ambientLight = _origAmbientLight;
            RenderSettings.ambientSkyColor = _origAmbientSky;
            RenderSettings.ambientEquatorColor = _origAmbientEquator;
            RenderSettings.ambientGroundColor = _origAmbientGround;
            RenderSettings.ambientIntensity = _origAmbientIntensity;
            RenderSettings.reflectionIntensity = _origReflectionIntensity;
        }

        // ---------------------------------------------------------------
        // Diagnostics: dump the scene's lighting/grading setup to the log
        // ---------------------------------------------------------------
        private void DumpSceneState()
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int pointLights = 0;
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null)
                    continue;
                if (light.type != LightType.Directional)
                {
                    pointLights++;
                    continue;
                }
                Logger.LogInfo("LIGHT dir name=" + light.name + " color=" + light.color +
                    " intensity=" + light.intensity + " euler=" + light.transform.eulerAngles +
                    " shadows=" + light.shadows + " strength=" + light.shadowStrength);
            }
            Logger.LogInfo("LIGHT other point/spot lights in scene: " + pointLights);

            Camera camera = Camera.main;
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetComponent<UniversalAdditionalCameraData>();
                Logger.LogInfo("CAMERA name=" + camera.name +
                    " post=" + (data != null ? data.renderPostProcessing.ToString() : "n/a") +
                    " volumeMask=" + (data != null ? data.volumeLayerMask.value.ToString() : "n/a") +
                    " forcedPost=" + _postForcedCount);
            }

            // In Linear color space every hand-written color this plugin pushes to a material is
            // taken as already-linear, so it displays lighter and less saturated than the literal
            // suggests. That is the difference between an orange twilight and a peach one.
            Logger.LogInfo("COLORSPACE active=" + QualitySettings.activeColorSpace);

            Material skybox = RenderSettings.skybox;
            Logger.LogInfo("RENDERSETTINGS skybox=" + (skybox != null ? skybox.name + " (" + skybox.shader.name + ")" : "none") +
                " ambientMode=" + RenderSettings.ambientMode +
                " ambientSky=" + RenderSettings.ambientSkyColor +
                " ambientIntensity=" + RenderSettings.ambientIntensity +
                " sun=" + (RenderSettings.sun != null ? RenderSettings.sun.name : "none"));

            Volume[] volumes = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                Volume volume = volumes[i];
                if (volume == null || volume.gameObject == _volumeObject)
                    continue;
                VolumeProfile profile = volume.sharedProfile;
                string overrides = "";
                if (profile != null)
                {
                    for (int j = 0; j < profile.components.Count; j++)
                        overrides += (j > 0 ? "," : "") + profile.components[j].GetType().Name;
                }
                Logger.LogInfo("VOLUME name=" + volume.name + " global=" + volume.isGlobal +
                    " priority=" + volume.priority + " weight=" + volume.weight + " overrides=" + overrides);
            }
        }
    }
}
