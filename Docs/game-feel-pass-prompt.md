# Game-feel pass: make AI Arena, Robot Racing and Tech Escape play like Claude Bomb

## Read first

- `Docs/look-audit.md`, especially "Phase 6, where the three games stand". You concluded there that the reviewer said NO on every screen and that the first reason is the screens' design: drawers that read as forms, briefings and debriefs as dashboards, chip strips, two button styles. The owner agrees and has decided: this pass fixes it.
- Play Claude Bomb from the title to the debrief before you change anything, and the three games too, level 1.1 of each.

The look is approved. Do not touch the models, materials, lighting or post. This pass is about what the player does, what is on the screen while they do it, and how the game explains itself.

## The problem, in the owner's words

"There are a lot of forms and texts and I don't feel it's a game. It's overwhelming with the data per screen. Claude Bomb is better in smartness and creativity, with less need of text on the screen to explain stuff or to enable stuff. No need to show things that are common sense."

## Why Claude Bomb feels like a game and the three do not

| | Claude Bomb | The three games today |
|---|---|---|
| What the player does | Touches the device: cuts wires, presses buttons, flips switches. The object is the interface. | Fills in forms beside the world: a 6 by 4 grid of point buttons, 15 BUY/NEVER toggles, 12 colour swatches, a name field, ten coin buttons, Verdict/Reason/Report rows for every message. The world is a picture next to the form. |
| How it teaches | The mechanic is the lesson: you can only win by describing what you see clearly to Claude. LUMA adds one short line now and then. | The screens explain the lesson: a question headline, a teal sub-headline, a LUMA paragraph (often cut mid-sentence), a "You fix" box, "Fix it" in red, then the same idea again in the file. |
| State | Shown by the world: the clock on the device, lamps go green, three strike circles. | Shown by text and numbers: "Attempt 3 of 120 · No finish yet", "This attempt 0 · best 0 · last move +0", "New move chance 0.87", "seed 7", "Speed 1 · 3 moves · Driving.", "Stand-in brain: heading for the banana at (3, 2)", "28,269 tokens left", sensor bars with "clear / wall / slow / off". |
| Words on the play screen | About 40, most of them the chat, which is the gameplay. | 150 to 300 on most play screens (count them in `realism/final/`). |
| Buttons on screen | Manuals, the camera arrows, pause, send. | 5 to 9 at once: Briefing, The file, Settings, Tour, Title screen, Follow, The track, Pause, Stop race now, Hide, plus the main action. |
| The future | Hidden. You find out by playing. | Shown and locked: "OPENS AT 2.1" on seven tags, grey part chips, a ladder of 13 to 20 level buttons on the briefing. |
| Tension | A ticking clock, strikes, hidden information split between you and Claude. | Little: the player sets things up, presses go, watches. |
| Briefing | "Incoming transmission": four short lines of story over the lab, one button. | A dashboard: headline, sub-headline, LUMA, a track or room card, a grid of 7 to 12 chips, the ladder, three buttons. |
| Debrief | One result, stars, LUMA's recap. | A chart, a checklist, a runs table, a medal card, an unlock card, LUMA, four buttons. |

## The rules for this pass

1. **Verbs, not forms.** Every decision the player makes is an action on a thing in the world, the way Claude Bomb's wires are. A form, a grid of toggles or a dropdown on a play screen is a bug. (Ideas per game are below. They are starting points, not orders: propose better ones if you have them.)
2. **The world shows the state.** If the world can show it, the HUD does not say it. A crash is sparks and a shake, not "crashed 1". Points float up from the car. What the robot senses is rays or cones drawn from it on the track, not bars in a panel. Tokens are a battery on the agent's back that drains. A held door is a teal lamp, not a row of text.
3. **No common sense on screen.** Never label what the player can see or already knows: "Walking.", "Driving.", "Speed 0 · 0 moves", "The track: a straight, one right bend, the finish" over a track that shows exactly that, "No finish yet", "YOUR AGENT", "THE SYSTEM · DECIDE EACH ROW", "Tonight's menu" repeated in three places. No coordinates, seeds, probabilities, token counts with commas or internal names ("Stand-in brain") during play.
4. **Show the future only when it arrives.** Remove every "OPENS AT x.y" tag, every greyed-out chip and the level ladder from the play and briefing screens. A part that unlocks arrives as a moment when it unlocks (a new part fitted to the car, a new item on the agent). The ladder lives on the title screen only.
5. **LUMA says one short thing, at the right moment, and it ends.** At most 15 words a line, spoken and subtitled, shown in full (never cut mid-sentence), gone when she is done. She talks when something happens (the first crash, the first finish, the agent sent home, a door breached), not to explain a screen. She never repeats the headline. Her recorded lines and the curriculum stay as they are: when a line is longer than 15 words, it moves to the file and she says a shorter cue that already exists in her lines, or nothing.
6. **One goal on screen.** One short objective line at the top (at most 8 words), not a question plus a sub-headline plus a "Fix it" plus a "You fix".
7. **A briefing is a transmission, not a dashboard.** Claude Bomb's shape: three or four short lines of story over the world, typed in, and one button. No chips, no cards, no ladder, no part grid.
8. **A debrief is a moment, then a door.** One big result (the medal with its stars, the race result, the doors that held), LUMA's one line, one "Next" button and one quiet "Again". The chart, the checks and the runs table go behind a single "Details" control, closed by default.
9. **Three controls at most on a play screen**, plus the main action. Briefing, The file, Settings, Tour and Title screen move into one menu behind Esc and a single icon, as in Claude Bomb (`GameMenu.cs`). The file stays: it is where the full text of the course lives, one icon away.
10. **Tension.** Each game gets a reason to care while it plays, built from what is already there: Robot Racing's attempt budget as a fuel gauge that empties; AI Arena's dinner time as a clock on the market tower with the family waiting at the house; Tech Escape's attacker as a figure on the corridor monitor getting closer while you work. Never punishing, always visible.
11. **Teach by trying.** Let the player do the wrong thing and see it fail before LUMA names it. The level's broken setup (already loaded on every fix-it level) is the puzzle; the world showing it fail is the explanation.

What does not change: what each level teaches, its checks, its scoring, the platform's progress reports, LUMA's recorded voice, the curriculum text in the file and the exact words the agents read. The checks may now be met by actions instead of form fields; the report sent to the platform must be the same.

## Ideas per game (starting points)

### Robot Racing
- The points table becomes **placing rewards on the track**: drag a chequered "+" flag onto the finish, red "−" cones against the walls, small markers along the way, each with a number you turn like a dial. The car learns from what you placed. The table is what you built, readable in the file.
- The sensors are **fitted to the car** as physical parts (a front eye, side whiskers, a speed gauge) and draw their rays on the track while it drives. Turning one off removes the part and its ray.
- The rule book and the curve stay, but behind "Details". On the main screen: the attempt fuel gauge, the best lap, and the ghost lines of earlier attempts on the track (already built: make them the hero of the training run).
- The pit lane screen is the car on the lift and the parts around it, nothing else.

### AI Arena
- The list becomes **packing the agent**: the 15 items are real produce on a table in the garage; drag what to buy into the basket and what never to buy into a "No" crate. The list the agent will read writes itself on a note pinned to the agent, readable when tapped.
- Sight, memory and brain are **parts you fit to the robot** (a bigger eye, a notebook on its back, a brighter antenna), shown on the robot, no tags with labels.
- Pocket money is **coins you drop into the robot's battery slot**; the battery shows how far it goes.
- Name and colour: picked once on the title, not on every garage screen.
- During the errand: the street view, the agent's thought as a small bubble in plain words ("I see bananas. Buying."), the dinner clock, and the agents' faces at the house when they come home. No cards with token counts during play.

### Tech Escape
- The inbox becomes **sorting the post**: pick up each envelope from the sorter, it opens close to the camera showing the sender and the link, and you stamp it SAFE or SUSPICIOUS, and drop the suspicious ones in the report tray. The reason is a second stamp, picked from three, on the letter itself.
- Every room's fix becomes **an action on the room's prop**: tag the unlisted asset with a label gun, turn the safe's dial, lock the badge gate, move the backup tapes out of reach, flip the one red switch. The rows of chips go.
- The briefing is the corridor seen from the camera with the night-shift transmission over it, and the level's door lit amber. The 12-room grid goes; the corridor already shows the rooms.
- The attack stays as it is (it is the best moment in the three games), with the HUD reduced to the door lamps.

## How to work

1. **Design first, no code.** Write `Docs/game-feel/<game>.md` for each of the three: for every screen of level 1.1 and of one tier-2 level, a "before" list of every word and control on screen (counted), and an "after" sketch: what is in the world, what the player does, the one objective line, what LUMA says and when, what moved to the file or the menu. Then show me the three docs and stop. I approve or change them before you build.
2. **Pilot Robot Racing**, level 1.1 and the tier-2 level you designed, end to end. Stop for my approval with the screenshots below.
3. Then the rest of Robot Racing, then AI Arena, then Tech Escape, stopping after each game.

## How we judge it

Machine checks, on every screen of the screenshot walk, reported in a table per game:
- **Words on screen** (count the visible words of every visible TMP text, leaving out the player's own typed text and Claude's replies): play screens 30 or fewer, briefing 50 or fewer, debrief with Details closed 30 or fewer. Claude Bomb's play screen is the yardstick: count it the same way and report it.
- **Controls on screen**: 3 or fewer on play screens, plus the main action.
- No form controls on a play screen (toggles, dropdowns, button grids, text fields), except the place where the player writes the agent's list in their own words, which is the lesson in AI Arena.
- No "OPENS AT", no ladder, no coordinates, no seeds, no probabilities on any play or briefing screen.
- Every LUMA line on screen is complete and 15 words or fewer.

Then two fresh reviewer agents that have not seen the work:
- **The player test.** Give it only the screenshots of level 1.1 in order, with no explanation, and ask: "This is a game. What is it about, what do you do on each screen, and at which screen did you feel you were filling in a form or reading instructions instead of playing?" It passes when the reviewer can say what to do on every screen and names no form or instruction screens.
- **The Claude Bomb test.** Give it the side-by-sides (Claude Bomb on the left) and ask: "Which one feels more like a game, and why? List what on the right feels like an app, a form or a lesson, most visible first." Fix what it lists and ask again.

Append "Phase 7: game-feel pass" to `Docs/look-audit.md` with the before and after word and control counts per screen, the reviewers' answers, and the screenshots in `games/<id>/Builds/look-audit/game-feel/`.
