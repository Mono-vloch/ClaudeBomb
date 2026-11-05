# Robot Racing: art bible

Three lines, written before the first material of the look-and-feel pass (5 November 2026), so
that every colour, light and effect after it traces back here. The interface keeps the
Maaloomatiia brand palette in `Scripts/Ui/Theme.cs`; this bible is the world's, and it sits
next to Claude Bomb's lab (cool blue room, warm lamps, one accent) and LUMA (silver, teal
lights) without copying them.

## Style

A toy-circuit diorama: clean flat-shaded blocks and one chunky little robot car, lit like a
model-railway layout under a late-afternoon sun. No textures, no outlines; the shapes carry
the style, the light carries the depth, and everything that earns points glows teal.

## Palette

The world (`Scripts/World/Look.cs`), each colour named for its job:

| Job | Hex | Name |
|---|---|---|
| asphalt | `#3F4654` | slate |
| asphalt line, start line | `#E8E2D3` | chalk |
| grass | `#8FBF6E` | meadow |
| grass in shade, the verge | `#6FA35A` | moss |
| kerb | `#D6453D` / `#F4F6F7` | brand red / white |
| barrier | `#D9D3C5` / `#BDB5A4` | warm concrete, light and dark |
| tree | `#4FA058` / `#3F8C4A`, trunk `#7F5236` | leaf, leaf dark, bark |
| stand | `#E6DFCF` / `#C7BFAD` | limestone, light and dark |
| the agent, primary | `#16C2B0` | brand aqua |
| the agent, secondary | `#F1F4F6` | shell |
| the agent, dark | `#2B2440` cabin, `#2A2E36` wheels | plum black, tyre |
| the agent's lights | `#9BE8FF` eyes, `#FFD27A` antenna | ice, lamp |
| shadow (ambient ground) | `#4A5A6B` | cool slate blue |
| accent: reward, first finish | `#24E0C0` teal, `#E09B1A` amber | the brand's teal and amber |
| danger: a crash, the wrong reward | `#D6453D` | brand red |
| oil | `#23262E` with a blue sheen | the real track's trap |

Rules: saturation stays in one family (meadow, leaf and aqua are all mid-saturated; nothing
neon except the glow on a scoring moment); greys are warm (concrete, limestone), never the
interface's cool neutrals; the only pure white is the kerb and the shell.

## Mood

Late afternoon on the circuit. One warm key light (`#FFE3C0`, intensity 1.15, 40 degrees up,
coming in across the track from the stand side) so the kerbs, the barrier tops and the car's
nose catch the light and every shadow side falls cool slate blue; soft shadows at full
strength. The ambient is a gradient: sky `#8FB6D8`, horizon `#C9C2B4`, ground `#4A5A6B`. The
sky is a soft blue-to-cream gradient and the haze deepens with distance (fog from 30 to 120
units in the sky's horizon colour). ACES tonemapping, a little bloom on the glowing parts only,
a vignette, saturation +8, contrast +5; depth of field behind menus and briefings.

Dusk (the title, the cover): the same track under the brand's ink sky (`#0B1536`) with an
orange horizon (`#F0915A`), the sun low and amber (`#FFA868`, 16 degrees), the finish arch,
the antenna and the stand's lamps lit, the ambient blue-violet. The lockdown night of Tech
Escape and the dusk market of AI Arena are the sister moods; this one is warmer than both.

## What this means for the interface

- Cover screens (title) stay dark on `Theme.Ink` with the wordmark, the world beside them.
- Everything during play is light on the brand's page white, but the world is always in
  frame: cards sit next to the circuit, never over a blank page.
- The HUD on the world is dark glass (`Theme.Ink` at 72 %) with teal keylines and chalk text,
  so it reads over grass and asphalt alike.
- Banners for the big moments use the accent of the moment: teal for a finish, amber for a
  bonus, red for a crash; gold, silver and bronze for the medal.
- Buttons are 12 px rounded rectangles; no pills, no ovals.
