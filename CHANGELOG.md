# 1.0.1.4 (unreleased)

- Fixed canopy shadows disappearing instead of becoming dappled. The stand-in that casts them was
  built on a material made at runtime from `Shader.Find("Universal Render Pipeline/Unlit")`, and a
  material made that way gets the all-features-off shader variant - while a build only ships the
  variants something in it actually used. Nothing in this game is an unlit material that casts a
  shadow, so that variant's ShadowCaster pass was almost certainly stripped: the stand-in wrote
  nothing at all, and since the canopies had already been told to stop casting for themselves, the
  shadows simply vanished. The stand-in is now cloned from the material the canopies are already
  being drawn with, which is casting shadows by definition. Its wind animation is switched off, so
  a stand-in cannot sway out of step with the tree it belongs to.
- The stand-ins are also drawn as ordinary shadow-only MeshRenderers now instead of through
  `Graphics.DrawMeshInstanced`, one merged mesh per tree. That is the plain, well-trodden path into
  the shadow map, and one object per tree keeps normal frustum and shadow-distance culling working -
  a single merged mesh for the island would never be culled at all. About 89k triangles across some
  65 objects, depth-only, and the holes make the pass cheaper than the solid canopies were.
- Added `Canopy shade brightness` (0.3-1, default 0.6), which works like the existing shade sliders.
  The shaded side of the canopies was too dark at the fixed values 1.0.1.3 shipped. It drives all
  three shading amounts together: the underside of each disc, the dark band beneath it, and the
  difference between the upper and lower discs of one tree.
- The canopy shade band's colour is now written from the canopy pass rather than from the recolour
  table, so the slider reaches it. The table still holds the material, which is what captures the
  authored colour and puts it back on uninstall.

# 1.0.1.3 (unreleased)

Both of these are about the round tree canopies, and neither is specific to autumn.

- The round canopies have light and shade again. They are drawn by
  `Shader Graphs/ToonWaveInstanceShader`, which has no lighting of any kind - no shade colour, no
  light binding, nothing the sun reaches. The conical evergreens standing next to them are on the
  game's UTS `Toon` shader and do get a real terminator, which is exactly why the two read so
  differently. All the shading these canopies can have is the shader's own colour band, and 1.0.1.2
  set `Canopy colour coverage` to 1 by default, which flattened that band away and left them as
  blocks of flat colour. There are now three layers of shading instead: a ramp down the inside of
  every disc (its underside is 62% of its top, up from 84%), a dark band along the bottom of each
  disc - the shader's one shared colour, so it is a deep shadow brown rather than any tree's own
  colour, since a dark underside reads as shade on every tree where a tree colour would read as the
  wrong tree - and a per-disc brightness from how high the disc sits in its own crown, since the
  discs of a tree overlap and the lower ones are the ones the upper ones would be shading. A small
  deterministic wobble on top keeps a crown from looking like one moulded lump.
- `Canopy colour coverage` is replaced by `Canopy shading` (default 0.5), which sets how much of
  each canopy the dark band takes up. 0 is the flat look; too high and the crowns go dark.

- Canopy shadows are dappled instead of solid. A canopy is a solid ellipsoid, so it lays down one
  filled oval - the island ends up covered in flat dark discs. Nothing in a material can change
  that: the shadow comes from the depth pass, and a hole in a shadow needs a hole in the geometry
  casting it. So the canopies stop casting (their `RenderParams.shadowCastingMode` is turned off by
  reflection, the same handle the existing instanced-shadow pass already uses) and a perforated
  stand-in is drawn into the shadow map from the very same instance matrices. New under
  `[Sun] Dappled canopy shadows` and `Dapple density`, and it applies in both seasons.
- The stand-in is the upper half of an ellipsoid matching the canopy's bounds, built as a height
  field over an evenly spaced grid on the horizontal plane. A UV sphere was the obvious shape and
  it was wrong: its quads project onto the ground wildly unevenly - the ones at the equator are
  edge-on and cast almost nothing, while a handful near the pole cover the whole middle of the
  shadow - so holes cut evenly over that mesh came out as one missing quarter. On a horizontal grid
  one cell is one patch of shadow. Only the top half is built: it is the half the light meets, a
  single layer cannot have a hole in its top that a surface underneath then plugs, and a half
  ellipsoid's silhouette under a low sun is the same stretched oval as the whole one's.
- Which cells become holes is decided at a quantile of a noise field rather than against a fixed
  threshold, so the fraction kept is exactly `1 - density` whatever the seed rolled. With a fixed
  threshold the same setting kept anywhere from 43% to 65% of the shell depending on the seed, and
  at higher settings it sometimes erased the shadow altogether. The noise is 85% fine and 15%
  coarse, sampled on the horizontal plane: measured over six seeds, an even mix left the largest
  single connected hole at 49% of all the holes - one missing corner, not dapple - where this mix
  leaves it at 20%. Holes are weighted toward the rim, because the middle of a crown is denser than
  its edge.

# 1.0.1.2 (unreleased)

Autumn, second pass, all of it from playing the first one.

- One tree is now one colour. A tree here is not one object: it is a handful of flattened discs,
  and the game does not colour the discs of a tree consistently - measured on the shipped scene,
  41 of the island's ~65 trees carry more than one of the four authored colours. Nobody notices in
  summer, because all four are near-identical yellow-greens sitting over one shared green body.
  Mapping those four straight onto four autumn colours turned single trees into gold-and-red
  confetti. The discs are now grouped by where they stand (anything within 9 m horizontally is one
  tree) and the colour is chosen per tree. Which colour a tree draws comes from a hash of its
  position, so the mix is stable between sessions and neighbours do not march through the palette
  in a stripe. Gold is weighted to appear most often and scarlet least.
- The canopies are no longer brown. They were being coloured by rotating the authored olive greens
  around the colour wheel, which preserves the art's own light and shade - but rotating a dark
  olive lands on brown however the saturation is set, and brown is not autumn. The canopies,
  bushes, grass tufts and flower-bed discs now take absolute colours instead: gold `#F2C63C`,
  amber `#EDA733`, pumpkin `#E87C2E` and scarlet `#D8492C`. The lawn and the paths keep the hue
  rotation, because they cover most of the screen and their authored light-and-shade relationships
  are what stop the island reading as flat.
- `Canopy colour coverage` now defaults to 1. Each canopy is a per-tree cap over one shared body
  colour, and at 1 the cap covers the disc outright, so a tree shows its own colour cleanly rather
  than as a hat over a common brown.
- The conical evergreens are left alone. They are a separate mesh - `MD_TreeBase_03`, 2:1 tall
  against 0.77:1 for the round canopies - drawn by a different instancing manager off the
  `M_Tree_02` material, so they were simply removed from the recolour table. They also get no leaf
  emitters, which follows from the same change: emitters are placed per round canopy.
- A tree drops its own leaf. The leaf atlas is now four tiles in the same order as the four tree
  colours - gold and amber ginkgo, pumpkin and scarlet maple - and an emitter holds the frame of
  the tree it belongs to instead of picking one at random per leaf. The game's own three emitters
  take the colour of whichever tree they stand nearest.
- Leaves come off the whole crown, from under it. They were emitted from a 2.8 m cone placed above
  the canopy, which is why they looked like they were being generated out of the sky. The emitter
  now sits at the underside of the crown and its cone is widened to the tree's own radius.
- `Shedding trees` replaces `Extra shedding trees` and defaults to 40. The game sheds leaves at
  three fixed spots on the whole island; each of the largest N trees now gets an emitter of its
  own, so leaf fall reads as a season rather than as two trees doing something.
- Leaves settle and stay. `Leaves on the ground` (default 400) keeps that many fallen leaves lying
  on the ground; `Ground leaf lifetime` defaults to 0, meaning they never fade. They accumulate
  within 70 m of wherever you are, so the budget is spent where it can be seen, and what you walk
  past stays behind you. They are placed by raycast, aligned to the surface normal, refused on
  anything steeper than about 57 degrees, and drawn as one merged world-space mesh - a GameObject
  per leaf would be hundreds of transforms and draw calls, and a particle system cannot park a
  particle on uneven ground. The layer is unlit and named "LookCare", which the night dimmer skips
  by design, so it follows the day cycle explicitly instead of glowing after dark.
- Added `Warm filter` (default 0.75): an amber cast over the whole autumn frame, multiplied into
  the existing grade and faded out with the daylight so nights keep their cool tone. The value is
  a linear multiplier, which is why it is written so much lower in blue than it reads - at the
  default a white surface comes out `#FFF6DC`.
- The Halloween string lights are pumpkin orange and witch purple only, and they step between the
  two instead of fading. Blending between orange and purple spends most of the cycle on the muddy
  browns and mauves that lie on the line between them; stepping is also what a real Halloween
  light string does.

# 1.0.1.1 (unreleased)

- Cheeks follow the face-marks menu again. The plugin lights the avatar's unlit face overlays by
  giving each slot its own material and scaling that material's colour by the light reaching the
  head. Every overlay went onto URP's stock unlit shader with a white tint, which is right for
  eyes and mouths - they ship on `Unlit/Transparent`, a fixed-function shader with no colour
  channel at all, so white is the only honest answer. Cheeks do not: they ship on the game's own
  transparent graph, they *do* have a tint, and `SetFacialColor` writes the colour the player
  picked straight into it. Forcing that to white left the decal showing whatever colour it
  happened to be drawn in, and re-forcing it ten times a second meant a new pick was erased
  before the next frame. Overlays that carry a real tint are now cloned from the source material
  instead of rebuilt, so they keep the game's shader, texture, keywords and blend state and are
  pixel-identical to the unmodded game at full daylight. Their colour, and the cheek decal shape,
  are re-read continuously from both places the game writes them: onto our own material (a clone
  of `M_Cheek13` is still called `...Cheek13...`, so the game finds it by name) and onto the
  pooled source material when a head is rebuilt.
- Fixed devices going see-through in the desk-pet camera mode. That mode composites the frame
  onto the desktop through the window's alpha channel, so whatever the last draw leaves in alpha
  decides what shows through. URP's unlit shader defaults its *separate alpha blend* to
  `(One, Zero)`, meaning a transparent overlay stamps its own alpha over whatever is underneath -
  and the screen-glow overlay covers the whole device mesh, with a fully transparent material on
  every submesh that is not the screen. Those submeshes were writing alpha 0 across the entire
  laptop, phone or console, and the wallpaper showed straight through it. All of this plugin's
  overlay materials now blend alpha as `(Zero, One)`: they leave the destination alpha exactly as
  the geometry underneath left it. The campfire flame overlay and the face overlays carried the
  same latent bug and were fixed with it.

- Added an autumn theme, selectable against the existing summer look from a new **Season** tab in
  the F11 menu (`[Season] Theme` in the config file). Autumn repaints the round tree canopies,
  bushes, flower beds and grass tufts in gold, orange, scarlet and deep red; optionally the lawn
  and the nature paths crossing it; sheds leaves far more often; and puts the hanging string
  lights on a Halloween palette.
- The autumn palette is expressed as a hue plus saturation and value scales applied to whatever
  colour each material already holds, not as literal replacement colours. That keeps every
  material's authored brightness relationships, so the island still reads as one painting, and it
  sidesteps the colour-space trap this project sits in: the game renders in Linear, so a literal
  written through `SetColor` is taken as already-linear and comes out pale and washed, while a
  hue rotation of the existing value lands correctly either way. `Autumn colour strength` mixes
  the result back over the summer colours for an early-autumn look.
- Recolouring the round canopies needed the per-instance colour arrays, not the materials. The
  canopies (`MD_TreeBase_01/02`) still reference `WaveTree01`..`WaveTree04`, but those renderers
  are switched off by `BushInstancing` in the game's `OptimizationManager` and the geometry is
  redrawn from matrices through one shared `ToonWaveInstance` material, with each tree's colour
  coming from a `Vector4` array in a `MaterialPropertyBlock`. Writing the `WaveTree` materials
  does nothing at all. The four colours in that array are the four tree families - 194 trees, 109,
  62, and the eight cherry trees - so they are ranked by how many trees carry them and repainted
  gold, orange, scarlet and deep red in that order, which makes the island read gold with orange
  and red through it and turns the cherry trees the deep red of a maple rather than leaving them
  pink. Only the cap above `_Threshold_1` uses that per-tree colour, so `Canopy colour spread`
  lowers the threshold to grow the cap over the shared body underneath.
- Autumn leaves are ginkgo and maple instead of one almond silhouette. The game's leaf texture is
  pure white with the shape in its alpha and the colour entirely in the material, so both of its
  emitters drop one shape in one flat colour. Autumn swaps in a generated four-frame sheet - two
  ginkgo fans in gold and butter, two maple leaves in orange and scarlet - with each colour baked
  into the atlas so shape and colour cannot come apart, and each leaf picks one frame when it
  spawns and stays on it.
- `Leaf fall rate` multiplies the game's own emitters. Because the game sheds leaves at only three
  fixed spots on the whole island, `Extra shedding trees` also copies the emitter onto that many
  well-separated canopies so leaves fall across the island; each copy costs a few dozen more
  particles and 0 turns it off.
- The season pass runs immediately before the decor and toon passes, because both read the
  colours it writes: the decor dimmer captures a material's colour once and then drives it every
  frame from that capture, and the toon pass derives its shade bands from the live base colour.
  Switching season drops the decor captures so they are retaken, otherwise the first nightfall
  painted summer greens back over the autumn palette.
- The island streams in additively, so the first apply pass can run before a single canopy or
  leaf emitter exists. A capped catch-up pass picks them up once they load and then stops.

# 1.0.1

- Removed the dark outline around the sun. Its alpha was `max(disc, corona)`, and the two terms
  crossed over where the disc had already fallen to zero but the corona had only reached 0.17 - a
  kink in the profile sitting right against the bright core, which reads as a dark ring. The corona
  is now composited under the disc (`disc + corona * (1 - disc)`), which is continuous: peak
  curvature over the whole profile drops from 0.026 to 0.003.
- Strengthened the sun's corona. The skybox disc used to supply a wide bloom and the billboard
  replaced it, but the corona was only 0.17 opaque at the disc edge, so the sun lost its glow - the
  game's own Bloom is off by default, so nothing else was filling in. The corona now starts at 0.37
  at the disc edge and decays smoothly, reaching exactly zero at the quad edge so no straight seam
  shows along the billboard's border, and the billboard is wider to give it room.

- Fixed the sun drawing as a solid white square. Its texture was built with
  `Mathf.SmoothStep(0.16f, 0.21f, radius)`, which reads like GLSL's `smoothstep(edge0, edge1, x)` but
  is not: Unity's overload returns a value *between* the first two arguments, so every pixel came out
  at roughly 0.8 alpha and the whole quad was opaque. Correct form is
  `Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge0, edge1, x))`, which is what every other generated
  texture in this plugin already used.
- Enlarged the sun's disc within its billboard so the visible core lands near the angular size the
  old skybox disc had, rather than a small dot inside a large corona.

- Dusk and dawn now have separate palettes. A dusk comes up golden orange, a dawn plain yellow at a
  clearly higher hue, so a morning is never mistaken for an evening. Dawns also roll the dramatic
  crimson and pink variants far less often - 30% and 45% of the configured chance - because the haze
  that makes a sky burn builds up over the day.
- Fixed nearly every twilight reading as pink. The variant colours were pale enough that "orange"
  landed on peach: as displayed, the old orange had saturation 0.40 against pink's 0.28, so the two
  were barely distinguishable and the peach read as pink. The palettes are now decisively saturated.
- The sun reaches the horizon. It was drawn by the procedural skybox's sun disc, which tracks the
  directional light - and that light is pinned to a 6 degree elevation floor so shading stays stable.
  The disc could therefore never descend below 6 degrees, and was switched off outright at elevation
  zero, so it vanished in mid-air every evening. The sun is now its own billboard on the real signed
  elevation, like the moon, and sets into the water while fading through haze.
- The moon rises from the horizon instead of appearing already high up. Its fade was keyed to
  daylight, so it did not begin to show until the sun was nearly three degrees down and was not fully
  opaque until eight - and at low opacity against a still-bright twilight sky it stayed invisible
  until it was well up. It now fades on its own elevation, crossing with the setting sun at the
  horizon: at sun elevation zero both are half visible on opposite sides of the sky.
- The active colour space is written to the log as a `COLORSPACE` line. In Linear space every
  hand-written colour this plugin sets is taken as already-linear and displays lighter and less
  saturated than the literal suggests, which is the difference between an orange twilight and a
  peach one.

- The dusk sea no longer reads as flat lavender paint. 1.0.1 carried the sunset hue into the deep and
  shallow water as well as the horizon, and everything converged on one purple. Deep water now stays a
  saturated navy that only leans toward the evening's colour; the warm end belongs to the horizon band,
  where sea actually meets sky.
- Fixed the grey step reappearing in the horizon band. A saturated cyan mixed halfway into a warm sky is
  grey no matter how bright it is, so the horizon crossover is now fast - past a fifth of the twilight the
  band is nearly all sky - and the daytime horizon colour is a pale haze rather than a deep blue. Across all
  three twilight variants the sea's minimum saturation is now 0.72 on the body channels.
- Twilight lasts from 20 degrees of sun elevation instead of 14, so dawn and dusk are roughly 40% longer.
- The horizon glow band covers about 110 degrees of horizon and 34 degrees of altitude, up from 77 x 19,
  at higher opacity. The old band was the "small patch near the sun" that was easy to miss entirely. The
  anti-solar band grew to match, and both were pulled closer in so their corners stay inside the far clip
  plane at the new size.
- The whole sky dome now warms at dawn and dusk, not just the band around the sun: the twilight sky tints
  carry real saturation and drive the skybox up to 0.77 weight, fading back out once the sun is well down.
- Added the sun-glitter path: the specular track a low sun lays across the water, running from the viewer
  out to the horizon. Water material colours are uniform over the entire sea, so this is the only part of a
  sunset that changes with which way you are facing. Configurable under Water.
- Fixed the waterfall sitting off-palette from the pond and river it feeds. Only horizontal surfaces carry
  the shadow-receiver overlay, so at night that layer darkened the pond and river while the vertical
  waterfall sheet kept its own brightness. The overlay now fades out at night, where there are no
  directional shadows left to show, and follows the water's own driven colour so lit water is barely
  altered - it is a shadow receiver, not a veil over the whole sea.

- Fixed the grey sea at dawn and dusk. The water body used to fade from day blue to night blue on a plain
  interpolation with a warm tint layered on top; a warm tint over an already darkened blue lands on grey, which
  is exactly the colour that showed while the sky was pink or gold. The sea now moves toward the live sky colour
  by rotating its hue through blue and violet instead of crossing straight through grey.
- The sea holds a configurable minimum brightness while the sun crosses the horizon (`Twilight sea brightness`).
  It used to reach about a third of its noon level at the exact moment the sky was at its most colourful.
- Added `Sea reflects sky color`: the sea now follows the colour of the sky this mod is drawing, strongest on the
  horizon band, weaker on shallow and deep water. Crimson, pink, and orange dusks each produce their own sea.
- The water shadow-receiver overlay follows the same sky colour and thins out at dawn and dusk, instead of
  laying a fixed blue-grey over the sunset.
- Removed the `Real water reflections` planar reflection renderer. It never ran: the game's sea meshes carry no
  Stylized Water `WaterObject` for it to attach to, so it only logged a warning every scan. Making it run would
  have cost a second full scene render per frame for colour information the mod already computes for free.
- The sun holds full brightness right down to the horizon and fades out over the few degrees below
  it, instead of starting to dim while it was still well up and reaching the waterline as a smudge.
- The night exposure dip is held back while the sun is crossing. `dayWeight` is already zero eight
  degrees down, so the full night darkening used to land while the sky was still burning and took
  the afterglow with it.

- The twilight world tint reaches much further. Its internal ceiling was 0.45, so even at the
  slider's maximum the ground and buildings barely moved off their daytime colour; it is now 0.75.
  The ambient probe also refreshes every 2 seconds instead of 4 during a twilight, since that probe
  is what carries the sky's colour onto everything the sun does not reach.
- Widened the sun-glitter path on the water and raised its opacity. The water's own material colours
  have to stay blue - carrying the sunset hue into them is what turned the sea lavender - so the warm
  part of a dusk sea comes from here, and a narrow streak covered too little of it.
- The glitter path now reaches the waterline instead of stopping in open sea. It was a radial blob
  capped at 320 m, so its bright centre sat 160 m out - from any height that reads as a coloured disc
  floating mid-ocean with a strip of untouched water between it and the sun. It now uses a wedge
  texture that peaks at its far end and narrows as it gets there, and it runs to whichever comes
  first: the far clip plane or the edge of the water mesh. A flat sea has no true horizon, so the
  mesh's edge is what actually reads as one, and ending the track there puts the bright end exactly
  on the visible waterline.
- Closed the dark band between the sea and the sky. Below the horizon the skybox draws its ground
  colour, and that strip is what shows between the far edge of the water mesh and the sky. It was a
  fixed configured colour that could not track a sea which now recolours all cycle, so it cut the
  water off from the sunset. It now follows the sea's own driven horizon colour.
- The water shader's float properties are dumped to the log as `WATER floats` lines. How far the
  horizon colour reaches back toward the viewer is one of them, and that distance is the only way
  left to widen the warm band on a dusk sea without tinting the water body itself.

- Added a time-of-day freeze for testing: a toggle plus an hour slider on the F11 Common tab, and
  a twilight-variant override so a specific sky can be forced instead of waited for. While frozen,
  the variant roll is derived from the frozen hour rather than the real clock, so it no longer
  reshuffles underneath a sun that is not moving.
- Twilight no longer starts while the sun is still high. It was a linear ramp over an 18 degree
  window, so the sky was already 40% coloured with the sun 12 degrees up and every dusk dragged on
  until it stopped reading as an event. The same window now uses a cubic curve, which puts nearly
  all of the colour in the last few degrees: 12 degrees up is 0.04 instead of 0.40, 6 degrees up is
  0.30 instead of 0.70.
- Cut the pink and crimson twilight chances hard: pink 0.18 to 0.05, crimson 0.10 to 0.03. A dusk is
  now golden 92% of the time and a dawn plain yellow 97% of the time.
- Fixed the horizon going dark green at sunrise. The dawn sky tint was the yellowest of the four, and
  a tint whose green sits high against its red multiplies into the procedural sky's blue scattering
  and comes out green along the horizon. The dawn tint is now orange-leaning, the skybox tint's green
  is capped against its red as a guard, and the atmosphere-thickness ceiling came down from 1.5 to
  1.30. The glow band stays yellow - it is additive geometry with no scattering to interact with, so
  a dawn still reads yellow.

- The time-of-day hold is now a normal feature rather than a testing aid: hold the sky at any hour,
  and optionally always get the same sunset colour instead of a random one.
- Rewrote the F11 setting names and descriptions in plain language across Chinese, English and
  Japanese. Nothing in the menu refers to depth buffers, sample counts, screen angles or shader
  files any more. Seven strings that had no translation at all - five of them older than this
  release - now have one; all 166 menu strings are covered.
- Fixed the blush on fixed map NPCs flickering. Face brightness took the strongest lamp in range
  by reading that lamp's live intensity - but the local-light budget zeroes every lamp each frame
  and then re-lights only the ones nearest the camera. An NPC's face therefore tracked which lamps
  happened to be inside that budget, and simply walking past flipped one in or out and stepped the
  face with it. It now reads what each lamp is meant to be emitting, which does not depend on the
  camera at all. Brightness is also eased toward its target rather than snapped.
- Fixed the package needing to be moved into the plugins folder by hand after installing with a
  mod manager. A manager places a package by matching the folders inside the zip against an
  install-rule tree rooted at `BepInEx` - `BepInEx/plugins`, `BepInEx/patchers`, `BepInEx/config`
  and so on. The zip shipped a bare top-level `plugins/` folder, which matches nothing in that
  tree, so the manager had no rule for it and left it where it fell. The zip now ships the full
  `BepInEx/plugins/On-Together_Day_and_Night/` path, which both mod managers and a manual drag of
  the `BepInEx` folder place correctly.
- The packaging script now takes the version from the manifest instead of a hard-coded 1.0.0, and
  refuses to build a package that would fail on upload: it checks the manifest name, version
  format, description length and dependency strings, that icon.png is exactly 256x256, and that
  the zip contains nothing a mod manager has no rule for.


# 1.0.0

First formal release of **On-Together: Day and Night**.

- Real-time configurable day and night cycle, including 1440-minute real-day synchronization.
- Dynamic sky, weather, water shading and reflections, shadows, scene lights, and nature ambience.
- F11 live settings menu in Chinese, English, and Japanese.
- Handheld game-console screen glow with subtle colour changes.
- Fixed Thunderstore/manual ZIP paths so `plugins/On-Together_Day_and_Night` extracts as real folders.
- Kept the procedural sky and grading off sticker/overlay and ID-photo capture cameras.
- Kept every face feature on its original shader and added renderer-local tint copies that follow real directional
  occlusion, night brightness, rain, and nearby lamps without changing texture alpha or shared materials.
- Reduced movement stalls by avoiding full rebuilds on streamed additive scenes, scanning only active player parts,
  and matching device-screen overlays to their source renderer state.
- Restored the 4096 shadow atlas and four cascades after the lower preset proved visibly blurry; retained the safer
  48 simultaneous local-light limit and disabled cloud-shadow default.
- Moved release staging outside `BepInEx/plugins`, preventing BepInEx from finding a duplicate packaged DLL.
- A thunder chance of zero now immediately cancels both the mod's flash and any active thunder one-shot.
