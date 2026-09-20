# 1.0.2

The autumn release. The island can be repainted in red, orange and gold; leaves fall from twenty
trees instead of the game's three, land, lie where they fall, are carried off by a gust and drift
away down the river; crows call over the lawn in place of the cicadas; the sun sits lower all day
and a harvest moon comes up over the sea; and there is a dry crunch underfoot where they have
piled up.

Alongside it: gusts that cross the island and set the canopies moving, dappled shade under the
round canopies - which turned out to have no lighting of their own at all - water that reflects
the sky and receives shadows, and an F11 menu cut back to the settings worth a decision.

The sections below are the stages this release was built in, kept in full. Most of them are a
record of getting something wrong first, and of what the wrong answer was.

## 1.0.1.8

The constant-gale report was right and my two previous explanations for it were both wrong. The
cause was mine, and it is this: **the game's falling leaves are almost weightless.**

```
gravityModifier = 0.010      one percent of gravity
startSpeed      = 0
Velocity / Force modules     disabled
Noise module                 enabled, strength 0.4
ExternalForces               disabled
```

A leaf falls about five metres in its ten-second life and drifts on a noise field for the whole of
it. So an emission rate is not "leaves per second falling past" - it is "leaves permanently hanging
in the air". The game ships three emitters, about 80 airborne leaves at a time, which reads as a
sparse drift. 1.0.1.5 set the defaults to 40 trees at 4x rate: **2240 leaves, none of which ever
landed.** That is the gale. It had nothing to do with the gust system, which the log confirmed was
running at a 10% duty cycle the whole time - I should have gone on to ask what else could move a
leaf, instead of treating that as the answer.

- Leaf density corrected: base rate per tree 1.2 to 0.4, `Leaf fall rate` 4 to 1.5, `Shedding
  trees` 40 to 20 - about 240 leaves in the air across the island, against the game's own 80.
  Existing configs still holding the old pair are migrated; anyone who tuned their own is left
  alone. The config text now states what the number actually controls.
- `ExternalForcesModule` is left disabled except while a gust is blowing. The vanilla emitters have
  it off deliberately, and leaving it on meant the leaves were no longer behaving as authored even
  between gusts.

- Fixed no leaves ever landing on water. The ground-leaf probe casts downward from beneath a tree
  crown, and the crowns all stand on land, so that ray could never reach water no matter how widely
  it was scattered - the water path was unreachable code. Water now has its own probe near the
  viewer, which is fair enough for a leaf blown onto a pond: it need not have come off the tree
  directly above it. Verified against the scene first this time - the Water layer carries three
  enabled box colliders (`SeaCollider` and two `MD_SeaPlane`), and the probe rejects any hit whose
  normal is not facing up, so it cannot catch a side wall.

- Bundled six crow calls, picked at random per call: Carrion Crow #1-#5 and Crows #1, all CC0 from
  BigSoundBank. The earlier suggestion was wrong on its own terms - those recordings are described
  on the source pages as "probably a magpie or a jackdaw". Crows #1 is a 33-second woodland
  recording rather than a single call, so it plays as a 2.6-second window from a random point in it.

- Trimmed the F11 menu. A panel with a slider for everything is not a panel anybody can find
  anything in, and several of these were shipped because they were easy to expose rather than
  because they were worth deciding about. Gone from the Season tab: canopy shade brightness,
  canopy shading, recolour lawns and paths, water depth tint, sun elevation multiplier, harvest
  moon, leaves land on water, kick up leaves when walking. Gone from Night Lights: the campfire,
  rattan lantern and play-tower sun switches. Every one of them is still there and still works -
  they live in the config file now, which F4 reloads.
- The day-cycle length and the held time of day snap to sensible steps instead of to decimal
  places. A cycle is a whole number of half hours (30 to 1440), and the held time moves in half
  hours and reads as a clock: `18:30`, not `18.5`.
- New: **Always raining**, which holds the rain on and stops it clearing by itself. Stopping the
  rain by hand, with the menu button or F10, turns it off again rather than letting the two fight.
- New: **Rain chance**, replacing the `Clear minutes minimum/maximum` pair. Those two said the same
  thing but you could not read the answer off them - how much of the time it actually rains was a
  sum across four numbers. The length of a shower is still set by the two rain-minute entries; how
  long it stays clear between showers is worked out from them and this, so 0.25 means about one
  hour in four is wet. The old clear-minute entries are gone from the config; anyone who tuned them
  should set the chance instead.
- Audited the menu's Chinese/English/Japanese table: every string the menu can draw has all three,
  and the 41 entries left behind by options removed over the last few versions are gone.

- Fixed a gust putting almost nothing in the air. Three things had gone at once: the per-tree base
  rate had been halved again on top of the 0.4 correction above and the vanilla emitters put on a
  further 0.55 multiplier, which left about a third of the documented 240; the gust force field was
  still being built and aimed every frame but nothing called it any more, so it applied to nothing;
  and `ExternalForcesModule` was pinned off even during a gust, so it could not have applied
  anyway. Rates are back at the documented pair, the field is driven again, and the emitters opt
  into it for the length of a gust.
- A gust now also tears five to twelve leaves off each nearby crown as it arrives, rather than only
  pushing on whatever happened to already be on its way down. At one percent gravity a leaf hangs
  for its full ten seconds, so an emission rate sets how many are *hanging*; the leaves that read
  as "the wind got up" have to be shed at the moment it does.
- Fixed blown leaves travelling in straight parallel lines. The gust velocity was a single constant
  written to every emitter, so every leaf on the island moved at exactly the same speed in exactly
  the same direction. It is a random range per particle now, including a little of it downward, and
  the noise field the game flutters its leaves on is turned up for the length of the gust and put
  back after - which is what makes a blown leaf tumble instead of slide.
- Leaves being rolled along the ground were doing it at a flat 1.4 m/s regardless of leaf or wind.
  They are back to being carried at a speed set by the gust, and each leaf's own `DriftFactor` now
  scales how far it is carried, how hard it swings across the wind and how quickly it picks the
  gust up, so a drift of them no longer moves as one sheet.

- Fixed the river emptying of leaves over a session while the fountains silted up. The status log
  told the whole story: `water=300 ... spawned=0/0/0/0`, the water budget pinned at its ceiling
  with nothing in it moving and nothing able to spawn. A leaf that reaches standing water - a
  fountain basin, a closed pool, the open sea - never leaves it again, so under one shared ceiling
  the still bodies take every slot within the hour and the river, whose leaves do flow away and are
  culled at range, is left with none. Standing water now has its own ceiling of a fifth of the
  budget, and a second ceiling per body of roughly one leaf per four square metres of surface, so a
  two-metre fountain basin takes a handful rather than the same crowd as a pool ten times its size.
- A leaf on standing water now goes waterlogged and sinks after 45 to 105 seconds, fading out over
  three. Nothing is carrying it, so without a clock of its own it is there for the session; with
  one, the still bodies turn over and their slots keep coming back to the water that moves.

- Fixed leaves stranding on the rim of the hot spring. Two causes. The pool is about eight metres
  across and the waterfall only takes the middle 1.2 m of its lip, but the leaves were steered
  towards the lip across the pool's full width, so anything arriving off-centre reached the rim and
  stopped. They are funnelled now: the further down the pool a leaf is, the less room it is given
  either side of the centre, until at the lip itself it is inside the mouth of the fall. Second,
  any leaf whose five candidate steps all failed simply sat there forever - it now notices it has
  stopped moving, opens the search to the full circle, and after twelve seconds of finding nothing
  in any direction accepts that it has run aground and fades out. Only leaves that ought to be
  moving are judged this way; one loitering on still water is meant to go nowhere.

- Fixed airborne leaves and leaves rolling along the ground looking like two separate systems. The
  gust force field's direction is an *acceleration*, not a speed: at 2.2 it ran at, five seconds of
  gust put an airborne leaf past 10 m/s while the ones on the ground were being carried at three.
  The field is down to 0.9 with drag under it, so it settles at a terminal speed instead of
  climbing for the whole gust, and it is now the turbulence rather than the push - the steady push
  comes from the emitters' velocity module, at a speed matched to the ground leaves'.
- Fixed blown leaves appearing to go in all directions. Unity draws each axis of the velocity
  module from its own random value, so the half-to-one-and-a-half range meant to vary their *speeds*
  was swinging each leaf's *heading* by up to twenty-five degrees instead. It is a narrow range
  now, and the noise boost over a gust is down from 2.4x to 1x; the variety comes from the force
  field, which is the part that is supposed to look turbulent.
- Leaves lifted off the ground are carried a little faster again to meet them in the middle.

- Fixed leaves on the water drifting in single file. Every one of them moved at the same 0.6 m/s
  and was steered back onto the river's centreline, so they queued up along one line. Each leaf now
  holds its own lane across the channel - sliding slowly across it rather than pinned to it - has
  its own pace between 0.34 and 0.88 m/s, takes its step on its own stagger rather than on a shared
  tick, and turns its own way.
- Fixed leaves on closed water setting off downstream. `RiverLeafDirection` returned a heading for
  any position at all, so a leaf on a pond with no outlet, or out at sea, was handed the direction
  of the nearest reach of a river it was not on and marched off into the bank. The current now only
  applies within 4.5 m of the sampled centreline; anywhere else the leaf wanders slowly on a
  heading of its own, which is what a leaf on still water does.

- Raised the floor under the leaf lighting. The main light has a floor beneath it (`Minimum visible
  night light`) so that trees, paths and people stay readable after dark; the leaves had none and
  bottomed out near black, so a leaf sat several stops below the ground it was lying on and read as
  a hole in it. The same setting now floors the leaves, and the moonlit base they fall back to is
  no longer as crushed.

## 1.0.1.7

- Fixed the gust reading as constant wind. The gusts themselves were fine - the log shows them
  50 to 70 seconds apart, four to eight seconds each, about a 10% duty cycle. The problem was that
  the canopies already sway a little all the time at the game's own settings, and a gust that only
  widened that sway was not distinguishable from the resting state. Wind is felt as movement
  getting *faster*, so the shader's wind speed is now driven alongside its strength (the canopies
  spell it `_WindSpeed`, the outlines `_Windspeed`; both are handled). A gust now also arrives in
  0.45 s rather than 1.4 s - fading in over a second and a half read as the weather slowly
  changing rather than as a gust.
- Fixed leaves on the ground appearing not to be lifted. They were: every leaf moved at 1.8 m/s and
  was removed after 2.5 m, which emptied the ground within a second and a half - that does not read
  as leaves being carried off, it reads as them vanishing. Only the loosest third of them are taken
  now, and each one that goes is thrown up into the air on the gust rather than deleted, which is
  the part that actually reads as the wind taking it. The rest shuffle and turn where they lie,
  capped at half a metre.
- The gust clock is now unscaled, like every other clock in this plugin: the game scales time in
  places, and a gust that stalled mid-blow would never have ended.

- Bundled `autumn_leaf_step.ogg` (Feet in Leaves #1, Joseph SARDIN, CC0 via BigSoundBank). Source,
  licence and checksum are recorded in `audio/THIRD_PARTY_AUDIO.md`.
- The wind is synthesised when no `wind_gust` file is supplied. BigSoundBank has no sudden gust -
  its fourteen wind recordings are all long steady ambiences - and a generated bed has a real
  advantage anyway: it is driven straight off the gust envelope, in both volume and pitch, so the
  sound arrives with the wind instead of being a recording faded up underneath it. Filtered noise
  with a slow swell and a cross-faded seamless loop; measured across one-pole bands it falls
  100 / 57 / 47 / 31 / 18 / 4 from below 80 Hz to above 8 kHz. A `wind_gust` file still takes
  precedence if one is present.
- The generated clip is filled through a PCM reader callback rather than `AudioClip.SetData`: in
  this Unity version SetData's only overload takes a `ReadOnlySpan`, which the C# 5 compiler this
  plugin is built with cannot resolve, and referencing the assembly that defines it collides with
  the compiler's own mscorlib.

## 1.0.1.6

- The autumn sun sits lower. `[Season] Sun elevation multiplier` (default 0.76) scales the noon
  height, which is most of what makes autumn light read as autumn: longer shadows all day, and
  everything lit more from the side than from above. It scales the configured noon elevation rather
  than the live angle, so the whole arc comes down together and dawn and dusk stay where the clock
  puts them.
- Added the harvest moon: autumn nights get a deep golden moon a third again as large.
- Leaves land on water. They settle on the sea and the ponds as well, lying flat however the
  surface is angled, riding a slow swell and drifting on a wandering current - a gust pushes them
  much less than one caught on grass, and they are allowed to travel far further before they count
  as gone. They share the ground-leaf limit.
- Walking through fallen leaves kicks up two or three scraps, thrown backwards from alternating
  sides of the player with enough lift to arc over and drop. Gated on there actually being leaves
  underfoot, so crossing bare ground does nothing.

- Autumn swaps the cicadas for crows. The cicada bed fades out entirely in autumn, and an
  occasional crow calls through the day instead - spread around a configured rate rather than on a
  metronome, and quiet after dark.
- Added three optional sound slots, all silent unless the file is present: `autumn_leaf_step` for
  the crunch underfoot, `wind_gust` for the wind during a gust (its volume rides the gust envelope
  directly), and `crow_call`. The loader now takes `.ogg`, `.wav` or `.mp3` and resolves each by
  base name, so a file can be dropped in without converting it first. A footstep plays a third of a
  second from a random point in its clip, so no two footfalls sound the same out of one recording.

## 1.0.1.5

- Canopy shadows are no longer made of squares. The perforated shell is a grid, so every hole was
  an axis-aligned rectangle and the dapples read as pixel art. The grid points are now pushed off
  the lattice by up to about three quarters of a cell before the shell is built - shared between
  the cells that meet at each point, so the shell stays closed and it is the lattice that moves,
  not each cell on its own. The grid also went from 16 to 18, which makes each dapple smaller.
- Falling leaves are lit. Their material is `Universal Render Pipeline/Unlit`, which reads neither
  vertex colour nor the particle's own start colour, so a shared material could only ever have one
  brightness for the whole island - which is why they stayed bright at night. Each emitter now
  carries the light of the tree it belongs to on a per-renderer property block.
- Leaves on the ground are lit per leaf, so one lying under a lamp is lit and the one beside it is
  not. They previously followed the day cycle alone and stayed dark under a lamp at night. This
  needed the ground layer to move off URP's unlit shader, which ignores vertex colour, onto
  `Sprites/Default`, which multiplies the texture by it; the light for each leaf is refreshed a
  third at a time and pushed to the mesh as colours only, without rebuilding the geometry. The
  brightness comes from `ComputeFaceBrightness`, the same function that lights the avatars' face
  overlays, so it already accounts for the day cycle and every lamp in range.
- Added `[Season] Water depth tint` (default 0.5): the sea and ponds go a deeper, colder blue in
  autumn. The horizon band takes only a third of it - where sea meets sky it has to keep following
  the sky, or dusk lands on the grey this sea was tuned out of two versions ago.

- Added gusts of wind, under `[Weather]` and on in both seasons. Every so often - about every 50
  seconds by default - a gust crosses the island from a new direction for four to nine seconds,
  rising quickly and dying away slowly. Three things move with it:
  - The canopies lean and sway. The canopy shader animates its own vertices from `_WindStrength`,
    so the gust is that value going up across every material that has one. That deliberately
    includes the outline materials: an outline that does not sway with the canopy it outlines comes
    away from it.
  - Falling leaves are blown sideways and lifted a little, through a single
    `ParticleSystemForceField` parked on the camera that every leaf emitter is opted into. Far
    simpler than reaching into each particle system's velocity module, and it leaves their authored
    motion intact.
  - Leaves already on the ground skitter along and spin as they go, each at its own rate, and are
    carried off once they have travelled a couple of metres. New ones keep arriving, so a gust
    reshuffles the ground layer rather than emptying it.

## 1.0.1.4

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

## 1.0.1.3

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

## 1.0.1.2

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

## 1.0.1.1

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
