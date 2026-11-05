# AI Arena: art bible

Three lines, written before the first material of the look-and-feel pass (5 November 2026),
so that every colour, light and effect after it traces back here. The interface keeps the
Maaloomatiia brand palette in `Scripts/Ui/Theme.cs`; this bible is the world's. It sits next
to Robot Racing's circuit (the same diorama style, a warmer hour) and Claude Bomb's lab.

## Style

A toy-market diorama: a paved square in a park, striped stalls under teal awnings, chunky
little robots in their team colours with white heads, all flat-shaded blocks, lit like a
model under a bright late-morning sun. No textures, no outlines; the shapes carry the style,
the light carries the depth, and what matters to the lesson glows: an antenna while it
thinks, a window at dusk, the square an agent can see.

## Palette

The world (`Scripts/World/Look.cs`), each colour named for its job:

| Job | Hex | Name |
|---|---|---|
| plaza | `#CDC1A8` | warm limestone |
| tiles | `#DED5C3` / `#CEC4AF` | pale and mid limestone |
| kerb | `#A89C86` | shadowed stone |
| grass | `#8FBF6E`, verge `#6FA35A` | meadow, moss |
| trees | `#4FA058` / `#3F8C4A`, trunk `#7F5236` | leaf, leaf dark, bark |
| stall wood | `#C08F57` / `#8F6239`, cloth `#E6DAC4` | oak, oak dark, linen |
| awnings | `#16C2B0` / `#F4F6F7` | brand aqua, white |
| the agent, primary | `Theme.Team[n]` | its team colour |
| the agent, secondary | `#F1F4F6` head and hands | shell |
| the agent, dark | `#2B2440` chassis and visor, `#3B3550` wheels | plum black, tyre |
| the agent's lights | `#9BE8FF` eyes, `#FFD27A` antenna and lamps | ice, lamp |
| the unseen | `#B9C2CC` / `#97A3AF` | grey crate (what the agent cannot see) |
| shadow (ambient ground) | `#4A5A6B` | cool slate blue |
| accent: a buy, home | `#24E0C0` teal | the brand's teal |
| danger: sent home | `#D6453D` | brand red |
| warning: out of tokens | `#E09B1A` | amber |

Rules: saturation in one family (meadow, leaf and the team colours mid-saturated; nothing
neon except a glow at a scoring moment); greys are warm stone, never the interface's cool
neutrals; the only pure white is the agent's head and the awning stripes.

## Mood

Late morning on the square. One warm key light (`#FFE3C0`, intensity 1.15, 42 degrees up,
from the camera's right) so the awnings, the stall tops and the agents' heads catch the light
and every shadow side falls cool slate blue; soft shadows at full strength. Gradient ambient:
sky `#8FB6D8`, horizon `#C9C2B4`, ground `#4A5A6B`. A soft blue-to-cream sky, haze from 30 to
120 units. ACES tonemapping, a little bloom on the glowing parts only, a vignette, saturation
+8, contrast +5; depth of field behind briefings and the debrief.

Dusk (the title, the story, the cover): the brand's ink sky with an orange horizon, the sun
low and amber, every stall's lamp lit, the houses' windows warm, the ambient blue-violet.

## What this means for the interface

- Cover screens (title, story) stay dark on `Theme.Ink` with the wordmark, the market beside.
- Everything during play is light on the brand's page white, but the market is always in
  frame: cards next to the square, never over a blank page.
- The HUD on the world is dark glass (`Theme.Ink` at 78 %) with teal keylines and chalk text.
- Moments use the colour of the moment: teal for a buy and ALL HOME, red for SENT HOME, amber
  for OUT OF TOKENS; gold, silver and bronze for the medal.
- The agent's step is shown as LOOK · DECIDE · MOVE: the three words of the loop light one
  after the other on every turn, so the loop the lesson names is the loop on screen.
- Buttons are 12 px rounded rectangles; no pills, no ovals, no circles.
