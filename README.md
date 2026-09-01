# On-Together: Day and Night

Version 1.0.1 for **On-Together**.

> **日本語概要:** On-Together 向けの BepInEx モッドです。リアルタイムの昼夜サイクル、天候・水面・影・照明・環境音、秋テーマとライブ設定メニューを追加します。ソースコードはポートフォリオ目的で公開しており、ゲーム本体・ゲーム由来のアセット・コンパイル済み DLL は含みません。

## Portfolio notes

This repository is a small Unity/BepInEx modding case study. The implementation focuses on
runtime rendering changes that are configurable without restarting the game: lighting and
shadow direction, sky and water colour, scene-light discovery, seasonal recolouring, and
ambient audio. The source is intentionally separated from the proprietary game installation;
build outputs and game assemblies are not committed.

The mod is an independent fan project and is not affiliated with or endorsed by On-Together or
its developers.

## What it adds

- A configurable real-time day and night cycle. The default is 60 minutes; 1440 minutes synchronizes a full game day with a real day.
- Directional shadows for world scenery, characters, lamps, and water surfaces, with the round
  tree canopies casting dappled shadows rather than solid ovals.
- Smooth dawn, dusk, moon, stars, clouds, rain, and a sea that takes its colour from the sky it is under.
- Warm lighting for scene lamps, campfires, hanging lanterns, decorative bulbs, and desk lamps.
- Subtle self-lit phone, laptop, and handheld game-console screens. The console screen gently changes colour with occasional game-like jumps.
- A summer or autumn theme. Autumn repaints the round tree canopies, bushes, flower beds and
  grass tufts - and optionally the lawn and its paths - in gold, orange and red, lays a warm
  cast over the frame, sheds ginkgo and maple leaves that settle and stay on the ground, and
  puts the hanging string lights on a Halloween palette. The conical evergreens stay green.
- Cicada and night-insect ambience with separate volume controls.
- A Chinese, English, and Japanese live settings menu, opened with **F11**.

## Seasons

The **Season** tab in the F11 menu switches between the summer look (the game as authored) and an
autumn one. Autumn is a live switch; nothing needs restarting.

Each round tree takes one of four colours - gold, amber, pumpkin or scarlet - chosen from where it
stands, so the same tree is the same colour every session. The conical evergreens are left green:
pines do not turn.

- **Autumn colour strength** mixes the autumn palette over the summer colours. Lower it for an
  early-autumn look with some green still in the scene.
- **Canopy shade brightness** sets how light the shaded side of the round trees is, the same way
  the other shade sliders work, and **Canopy shading** sets how much of each tree the dark band
  along its underside takes up. The shader these canopies use has no lighting of its own, so
  that band and the ramp inside each disc are where all of their light and shade comes from.
- **Warm filter** lays an amber cast over the whole frame. It fades out at night.
- **Include lawn and paths** can be turned off to recolour only the planting.
- **Leaf fall rate** multiplies the emitters, and **Shedding trees** gives that many canopies an
  emitter of their own, largest trees first - the game itself only sheds leaves at three fixed
  spots on the whole island. Each tree drops the leaf that matches its own colour.
- **Leaves on the ground** keeps fallen leaves lying where they land, building up around wherever
  you are, and **Ground leaf lifetime** is how long they stay - 0 means forever. Set the count to
  0 to turn the layer off. They are flat quads merged into a single draw call, so a few hundred
  costs very little; it is still the first autumn setting to lower on a weak GPU.

## Dappled tree shadows

The island's round tree canopies are solid ellipsoids, so each one casts a single filled oval on the
ground. **Dappled canopy shadows** (in the Sky & Light tab, `[Sun]` in the config file) stops them
casting themselves and draws a perforated copy of each canopy into the shadow map instead, so the
shadow breaks up into dapples of light. **Dapple density** sets how much of it is holes.

This works in both seasons. It costs one extra instanced draw per canopy batch, shadow-only, and the
holes make the shadow pass cheaper rather than dearer. If tree shadows ever vanish entirely, turn it
off - that means the stand-in is not reaching the shadow map on your setup.

## Installation

1. Install BepInEx for On-Together.
2. With a mod manager, install this package normally; no files need to be moved afterwards. For manual installation, copy the included `BepInEx` folder into the game folder and merge it with the one already there.
3. Start the game. Press **F11** for live settings; press **F4** to reload the configuration file.

The mod uses the plugin folder `On-Together_Day_and_Night` and creates the configuration file `cn.ontogether.dayandnight.cfg`.

## Troubleshooting

- If an ID-card portrait was saved at night by the original 1.0.0 build, open avatar customization, make any
  harmless change, and save once. The game stores `IDPhoto.png`, so uninstalling a mod does not regenerate an
  already saved portrait; the fixed build keeps future portrait captures in neutral daytime lighting.
- `Thunder chance per minute = 0` disables Day and Night's lightning flash and thunder clip. The game's separate
  **Storm** ambience toggle has its own rain/thunder effect and is not controlled by this setting.
- The default 4096 shadow atlas and four cascades keep nearby shadows clear across the long world view. If a GPU
  cannot sustain them, lower these two settings from F11; cloud shadows remain off by default.

## Included third-party audio

The bundled cicada and night-insect recordings are CC0. Their source and licence details are in `audio/THIRD_PARTY_AUDIO.md`.
