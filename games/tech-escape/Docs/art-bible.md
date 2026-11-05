# Tech Escape: art bible

Three lines, written before the first material of the look-and-feel pass (5 November 2026),
so that every colour, light and effect after it traces back here. The interface keeps the
Maaloomatiia brand palette in `Scripts/Ui/Theme.cs`; this bible is the facility's. It sits
next to Robot Racing's circuit and AI Arena's market (the same diorama style) and Claude
Bomb's lab, and it is the dark one of the four.

## Style

A permit office in lockdown, cut open from above like a model: one concrete corridor, twelve
doors, six a side, the exit at the far end, every room a small set behind its door (a rack, a
desk, a screen, and the one prop that says what the room is: a mail sorter, a safe, a glass
cell, a badge gate, a wall of switches, shelves of tapes). Flat-shaded blocks, no textures, no
outlines. The light comes from the facility itself: the strip over every door, the screens,
the rack LEDs, the exit sign; the night outside is a cool blue. What matters to the lesson
glows: a door's state, the row of lamps that says what has been decided in a room, the
attacker's visor and the trail it leaves, the door that gave way.

## Palette

The world (`Scripts/World/Look.cs`), each colour named for its job:

| Job | Hex | Name |
|---|---|---|
| night sky | `#0B1536` top, `#1E3A5F` horizon | ink, lockdown blue |
| ground outside | `#161D2E` | asphalt at night |
| corridor floor | `#3A4356`, lines `#55607A` | polished concrete |
| walls | `#8C96A8`, dark `#6B7489` | concrete under moonlight |
| skirting, door panel, chassis | `#2B2440` | plum black |
| door frame | `#16C2B0` | brand aqua |
| door light: locked / current / held / breached | `#D6453D` / `#E09B1A` / `#16C2B0` / `#FF4B3E` | red, amber, teal, scarlet |
| sign | `#F1F4F6` | shell white |
| rack `#1F2430`, LEDs `#6CC24A` | rack, LED green |
| desk `#C08F57`, chair `#3B3550` | oak, tyre |
| screen | `#9BE8FF`, glowing | ice |
| the room lamp | `#DDE6FF` cool, `#FFD27A` warm on the desk | moon, lamp |
| the config lamps: decided / undecided | `#16C2B0` / `#E09B1A` | teal, amber |
| the attacker | `#3A3556` body with a sheen, `#FF4B3E` visor and bands, a scarlet ring on the floor under it, trail `#FF4B3E` at 60 % | shadow plum, scarlet |
| the alarm | `#FF4B3E` strobe, `#D6453D` pulse | scarlet, brand red |
| chalk (text on glass) | `#E8E2D3` | chalk |
| accent: held, the exit open | `#24E0C0` | the brand's teal |
| danger: through | `#D6453D` | brand red |
| warning: the current door | `#E09B1A` | amber |

Rules: the facility is blue-grey at night, nothing warm but the desks and the lamps; the four
door colours are the only saturated colours on the set and they mean one thing each; the only
pure white is the signs; greys are cool concrete, never the interface's teal-tinted neutrals.

## Mood

Lockdown, every screen. One cool key light (`#5E78A8`, intensity 1.0, 50 degrees up, from
the exit end) so the tops of the walls and the racks catch a little moon and every shadow
side falls into the ink; soft shadows. Gradient ambient: sky `#3C5A8C`, horizon `#46527A`,
ground `#1A2238`. Fog in lockdown blue from 50 to 160 units. Every door's strip is a real
light on the corridor; the room on screen has its own cool ceiling lamp and a warm one on the
desk. ACES tonemapping, bloom on the strips, the screens and the visor (the only things over
white), a vignette, saturation +8, contrast +5; depth of field behind the briefing, the warm-up
and the debrief.

The attack: the strips go scarlet and strobe when a door gives way; the camera chases the
attacker down the corridor; a breach is sparks, a shake, a red pulse and a hit-stop; a held
door is a teal flash and a clack. The exit opening is the one moment the facility goes teal.

## What this means for the interface

- The cover (the title) stays dark on `Theme.Ink` with the wordmark, the facility beside.
- The briefing and the debrief are light on the brand's page white, but the facility is always
  in frame, soft, in a window; the ladder of levels in dark glass over it.
- The room and the attack are the world full-screen: the system's rows on a sheet beside the
  room, the attack's lines in dark glass beside the corridor.
- The HUD on the world is dark glass (`Theme.Ink` at 78 %) with teal keylines and chalk text:
  the level, the room, the twelve doors as lamps, THROUGH and HELD counts.
- Moments use the colour of the moment: scarlet for THROUGH, teal for HELD and THE EXIT OPENS,
  red for THE ATTACK BEGINS; gold, silver and bronze for the medal.
- Buttons are 12 px rounded rectangles; no pills, no ovals, no circles.
