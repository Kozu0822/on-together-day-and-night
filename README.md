# On-Together: Day and Night

Bring the island to life from sunrise to starlight.

**Day and Night** is a BepInEx mod for **On-Together** that adds a configurable real-time
day/night cycle, dynamic atmosphere, seasonal visuals, and ambient sound. Open the in-game menu
at any time to make the island feel like your own.

> **日本語:** On-Together 向けの昼夜・季節・環境演出モッドです。F11 の設定メニューから、時間の流れ、空や水面、照明、秋の景観、環境音をその場で調整できます。

![Four moments on the island: summer day, autumn day, night, and dawn](https://raw.githubusercontent.com/Kozu0822/on-together-day-and-night/main/images/day-night-cycle.png)

## Highlights

- Watch the world shift naturally through dawn, daytime, sunset, moonlight, stars, clouds, and rain.
- Set the day length to suit your play style—from a fast in-game cycle to one full real-world day.
- See the sky reflected in the sea, with scene lighting that warms up after dark.
- Give the island an autumn makeover: red and gold foliage, leaves that fall, settle where they
  land and drift away on the water, crows over the lawn, a harvest moon, and optional Halloween lights.
- Feel a gust cross the island now and then, setting the canopies swaying and scattering the fallen leaves.
- Add daytime cicadas and nighttime insects, each with its own volume control.
- Tune lighting, shadows, weather, water, ambience, and seasonal settings live with the F11 menu.

## Installation

1. Install BepInEx for On-Together.
2. Install the package through a mod manager, or copy its included `BepInEx` folder into the game directory.
3. Start the game. Press **F11** to open the live settings menu; press **F4** to reload the configuration file.

The mod uses the plugin folder `On-Together_Day_and_Night` and creates the configuration file
`cn.ontogether.dayandnight.cfg`.

## Troubleshooting

- If an ID-card portrait was saved at night by the original 1.0.0 build, open avatar customization,
  make any harmless change, and save once. Existing portraits are stored by the game and are not
  regenerated when the mod is removed.
- Set `Thunder chance per minute` to `0` to disable Day and Night's lightning and thunder. The game's
  separate **Storm** ambience setting is independent.
- If shadows are too demanding for your GPU, lower the shadow atlas size or cascade count in the F11 menu.

## Development

This repository contains the mod source, build scripts, icon, and separately licensed ambient audio.
It does not include the game, game assets, game assemblies, build output, or a compiled DLL.

Day and Night is an independent fan project and is not affiliated with or endorsed by On-Together
or its developers.

## Third-party audio

The bundled cicada, night-insect, crow and leaf-step recordings are CC0. Their sources and
licence details are in
[`audio/THIRD_PARTY_AUDIO.md`](audio/THIRD_PARTY_AUDIO.md).
