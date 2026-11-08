# Phase 7c: Tech Escape on the game-feel screens

8 November 2026, branch `game-feel-tech-escape` (a worktree off `game-feel-pass` at 932b511),
from `Docs/game-feel-pass-prompt.md` and the design in [tech-escape.md](tech-escape.md) with the
owner's three answers (console labels in plain words with the port after; the attacker walks
to the door and never attacks early; the first attack on an unfixed room is a free rehearsal).
All nineteen levels run on the new screens. Only `games/tech-escape/`, this file and the design
doc's answers changed; the kit, the server and the other games are untouched. Frames and counts:
`games/tech-escape/Builds/look-audit/game-feel/` (not in git).

Round 4 comes first below: Charbel's answers to the round-3 report, the Higgsfield files, the
new counts, the web size and the fourth player test. Round 3, then rounds 1 and 2, follow as
they were reported; where a later round changed something, the later section is the one that
holds.

## Round 4: the owner's answers and the Higgsfield files (8 November)

What Charbel answered after the round-3 report, and what the build does now:

1. **Values only where they are read.** From the room camera the knobs show no values; each
   card on the board has one lamp at its corner, teal when the row is set right and amber when
   not. "Right" is the room's own judgement (`Room.RowRight` in `Core/Facility.cs`): the row
   as the player has it is tried in the setup that works, and it is right when nothing more
   gets through or fails, so any answer the checks accept turns the lamp teal, not only the one
   in the setup. The board of lamps on the wall and the lamps at the top of the screen follow
   the same judgement. The card brought close shows every value ("Owner: Dana, clerk") and the
   card's lamp by its title; the knob just turned shows its value on the board until the next
   one is turned.
2. **Light knobs on the open card**: a light face and a dark pointer from the centre to the
   rim, at the end of each row.
3. **Finding what nobody listed (1.1).** The working copy of applicants' documents is a small
   network drive, labelled SHARED, on a crate on the floor of the room, out of the camera's
   view as the room opens. The player turns the security camera by dragging across the room
   or with the arrow keys (36 degrees each way, 22 down); two faint chevrons at the edges of
   the feed say it turns, and fade once it has. Anything not yet on the list glints, a small
   star and a chime, when the camera passes it. A tap flies the drive up to the board, where
   it becomes the card, while the camera turns back. The printer and the canteen menu, the
   decoys, glint and fly the same way (in the code; the walk taps only the drive). No other
   level has an unlisted thing to find.
4. **The attack pays off both ways.** A door that gives way: the attacker goes in, comes back
   out with the drive in his hand (lit teal, so it reads down the dark corridor) and runs off
   with it. A door that holds: he tries the handle twice more, the door shaking in its frame,
   stands a moment, gives up and walks away down the corridor. Then the end held on the door, as
   before. No body or role for the player.
5. **The staff badges** (Higgsfield's two printed sheets). Both sheets are spelled right,
   "HARBOR PERMITS OFFICE" and every name and role included. Sheet B is used: its Sami badge says
   LEFT JUNE as the game does, and its large names and coloured role bands read from the room
   camera. Each badge is cropped into its own texture and clipped on a thin card at the left of
   each of 2.1's six turnstile accounts and 3.1's three roles.
6. **The patch panel** (Higgsfield, Tripo). Split into a geometry-only model and 1K maps by the
   same cleanup as the other heroes (`Tools/glb_maps.py`, a rewrite of the realism pass's lost
   tool) and built as a prefab by `Tech Escape > Build patch panel`. The model is a 2U panel
   full of blue, yellow and red cables hanging in front, not five sockets; hung above the rules
   its cables would cover them, so it is the rack's bottom unit, 0.74 m wide under 2.4's five
   rule modules, cables hanging below. 8,998 triangles, no emission map.
7. **LUMA's strip** is left as it is: it is shared and is fixed once after the merge.

Two bugs found on the way and fixed: the room camera kept the prop in the middle of the frame
by shifting its projection, so a turned camera only skewed the picture around the board; the
point it keeps in the middle now turns with the player's look. And a card found in the room
told the screen before the board was rebuilt, so the lamps at the top lagged one step.

### Words and controls, round 4

From the last walk (`Builds/look-audit/game-feel/final/counts.jsonl`, all nineteen levels,
130 pictures). With the values off the knobs the rooms are back within the brief:

| Level | Briefing | Room as handed over | Card close | Room set right | Debrief |
|---|---|---|---|---|---|
| 1.1 | 44 | 20 | 54 | 26 | 18 |
| 1.2 | 43 | 24 | 55 | 24 | – |
| 1.3 | 40 | 20 | 55 | 20 | 19 |
| 1.4 | 41 | 23 | 43 | 23 | – |
| 1.5 | 41 | 19 | 72 | 19 | – |
| 2.1 | 42 | 21 | 35 | 21 | – |
| 2.2 | 41 | 28 | 43 | 28 | – |
| 2.3 | 44 | 22 | 28 | 22 | – |
| 2.4 | 43 | 30 | 42 | 30 | 13 |
| 2.5 | 41 | 20 | 38 | 20 | – |
| 2.6 | 41 | 19 | 26 | 19 | – |
| 2.7 | 42 | 28 | 32 | 28 | – |
| 2.8 | 42 | 23 | 23 | 23 | – |
| 2.9 | 39 | 23 | 27 | 23 | – |
| 2.10 | 41 | 23 | 43 | 23 | – |
| 3.1 | 43 | 17 | 21 | 17 | – |
| 3.2 | 39 | 19 | 27 | 19 | – |
| 3.3 | 43 | 24 | 40 | 24 | – |
| 3.4 | 37 | 25 | 48 | 25 | – |

Every room within 30 words and 3 controls plus Lift the lockdown, as handed over and set
right; every briefing within 50; every debrief within 30; the eleven LUMA lines on the walk's
screens whole and 15 words or fewer; no flags. The card brought close goes over 30 on most
levels (21 to 72 words, the board around it included), as agreed: it is the reading moment.
The badges' printed words are not counted (they are pictures, not text the walk can read).

### Web, round 4

| | Round 3 | Round 4 |
|---|---|---|
| Transferred (the platform's page in headless Chrome, port 8086) | 42.4 MB | 43.2 MB |
| Load (software GL) | 4.5 s | 4.9 s |

Under the 45 MB limit with 1.8 MB in hand; the patch panel and the badges cost 0.8 MB. The
game loads on the platform (`Builds/look-audit/game-feel/platform/platform-tech-escape-round4.png`).

### The player test, round 4

A fresh agent, the same prompt, only 1.1's eighteen screens from the last walk (its answer in
full: `Builds/look-audit/game-feel/reviewers/player-round4/answer.md`). It told the level back
whole: the lockdown at the Harbor Permits Office, the board in the Risk Room, the one asset
nobody listed, the rehearsal in which the intruder steals it, the shared drive found, added,
owned and rated, the door holding, three stars.

- **What it read as play**: finding the drive ("the one real 'aha, there it is' moment"), the
  attacker running off with "something glowing" ("the best moment so far, because something
  was taken"), and the door that "visibly refuses him" before he walks away.
- **What it read as a form or instructions**: the briefing ("the mission verb is 'list'"), the
  card brought close with its question marks ("this is the form"), Details ("a report card").
  The board as handed over is "closest to play", but Lift the lockdown is lit from the start
  "like a Submit button".
- **What it could not tell**: whether the drive in mid-air was carried, dragged or a glitch
  (the flight caught in a still), and whether the hooded figure was the player or the
  intruder (the chase camera sits over his shoulder).
- **What it saw as broken, and where it stands**:
  - No values on the board's knobs. As asked: the lamps carry the state, and the values are on
    the card brought close.
  - The board's cards stick out past the whiteboard's frame, and the open card cuts the cards
    beside it. True in the frames.
  - "Take it off the board" in salmon reads like a delete button.
  - LUMA's "Watch which door it goes through" comes while the player is still in the room.
    That is the line she says once the drive is owned, before the attack.
  - Framing: door signs cut at the frame's edges, flat ceiling lights, the medal small on a
    dark step, the HUD's corner marks over props, a dark wedge under the Details panel.
- **Its changes**: slot the drive into the board by hand and set the owner by handing it to a
  person, with the ratings coming from what the rehearsal showed; make the threat real, with the
  player walking as the agent and a countdown; hide Lift the lockdown until the board is
  complete; a bigger, brighter medal. The countdown and the player's body go against your
  answers (no countdown; no body or role), so they are not taken up. Hiding Lift the lockdown
  until the board is complete would undo the rehearsal, which needs the room as handed over.

None of this was acted on after the test: the build committed is the one in the frames.

## Round 3: the owner's answers (8 November)

What Charbel answered after the round-2 report, and what the build does now:

1. **The close-up is a flat card, face-on** (`World/RoomRig.cs`). Tapping a card no longer
   tilts its plates toward the camera. A new card comes up in front of the board, square to
   the camera: the full name at the top, the file's line under it, then one row per decision,
   each reading "Owner: Omar, supervisor" with a lamp at its start and a knob at its end. All
   rows of a card share one text size, the size its longest possible line needs, so nothing
   jumps when a row is turned. The camera stands as far back as fits the whole card with a
   margin. When furniture stands between, the camera comes closer and the card shrinks with
   it: 44 % on 1.5 (the bin), 85 % on 2.2 and 2.3, full size elsewhere. While it is open, the
   loose things and the lamp's cone and dust step aside. Checked in every room of the last
   walk: nineteen card close-ups and the six console close-ups, all face-on, nothing cut off.
2. **Every control shows its value on its face**, as on Robot Racing's console: "Omar",
   "High", "Off" on the knob at rest, and "Label: Value" on each row of the close-up. The
   knobs were drawn twice their size until now (Unity's built-in cylinder mesh is two units
   across, not one) and overlapped each other; they now sit in their housings, 7.6 cm discs,
   and the housings are matte so the lamp no longer flares on them.
3. **The console is a card.** The room's one-off decisions (Sign-in rules on 1.4, The summary
   on 1.5, Crossings on 2.3, The sign-in on 2.7, Declaration on 3.1, Trigger and cause on 3.4)
   are the last card of the grid, the same size as the others, brought close the same way.
   This ends 1.5's shrunken console and the Operations Center console that stood in front of
   the monitor.
4. **The Operations Center monitor.** The corridor feed plays on one of the screen wall's own
   screens. The grid slides sideways, by as little as it can, so that one screen stays clear:
   the grid moved 25 cm on 2.7, 5 cm on 3.1 and 45 cm on 3.4, and the feed's screen is 0 %
   covered in all three (it was 41 % on 2.7). Elsewhere the monitor on the back wall is a little smaller, so it clears 2.3's
   board of lamps and stays inside 1.4's feed.
5. **No countdown.** The attacker on the monitor walks the corridor to this room's door and
   stops there. He tries the handle, with a rattle, every 3.5 to 5.5 seconds, until the player
   lifts the lockdown. He never attacks early.
6. **"Lift the lockdown"** replaces "Run the attack": the button, the objectives ("Set its
   owner. Then lift the lockdown.", "Fix the room. Then lift the lockdown.") and the briefing.
7. **The briefing is the alarm and the story only**: "Intruder alarm", the room, then three
   lines. For 1.1: "23:47. An intruder alarm at the Harbor Permits Office." / "Every door is
   sealed. Someone is in the corridor." / "Before you lift the lockdown, list what the Risk
   Room protects." Nineteen new alarms, no motto in any.
8. **The motto moved to LUMA's line on the debrief**: as many whole sentences of the level's
   takeaway as fit in 15 words ("You cannot protect what you have not listed, and nobody
   protects what nobody owns."). The eight debrief lines whose words changed were recorded
   again, and 3.4's is new (take-19).
9. **The podium has one step**, with no number, the medal rising on its front. Robot Racing's
   podium is left as it is in this worktree; it gets the same change after the merge.
10. **Higgsfield**: the staff ID badges and the patch panel with cables are approved and
    coming from Charbel. The current props stay until the files arrive.

The words this costs: the values on the knobs at rest count as words when the camera sees
them, so the rooms with many knobs go over the brief's 30 (table below). That is the owner's
rule against the brief's limit; see Waiting for Charbel.

### Words and controls, round 3

From round 3's walk (`Builds/look-audit/game-feel/final-round3/counts.jsonl`, all nineteen levels),
counted as in rounds 1 and 2: every visible word, the world's labels included when the camera
sees them. The values on the knob faces now count, as they are on screen.

| Level | Briefing | Room as handed over (round 2) | Card close | Room set right | Debrief |
|---|---|---|---|---|---|
| 1.1 | 44 | 47 (22) | 75 | 55 | 18 |
| 1.2 | 43 | 48 (24) | 79 | 46 | – |
| 1.3 | 40 | 47 (20) | 82 | 48 | 19 |
| 1.4 | 41 | 36 (23) | 56 | 39 | – |
| 1.5 | 41 | 51 (30) | 104 | 51 | – |
| 2.1 | 42 | 33 (21) | 44 | 33 | – |
| 2.2 | 41 | 43 (28) | 51 | 43 | – |
| 2.3 | 44 | 32 (30) | 32 | 32 | – |
| 2.4 | 43 | 44 (30) | 50 | 42 | 13 |
| 2.5 | 41 | 29 (20) | 47 | 31 | – |
| 2.6 | 41 | 24 (19) | 31 | 25 | – |
| 2.7 | 42 | 39 (30) | 38 | 39 | – |
| 2.8 | 42 | 29 (23) | 26 | 29 | – |
| 2.9 | 39 | 30 (23) | 33 | 31 | – |
| 2.10 | 41 | 41 (23) | 61 | 41 | – |
| 3.1 | 43 | 20 (18) | 24 | 26 | – |
| 3.2 | 39 | 25 (19) | 33 | 27 | – |
| 3.3 | 43 | 27 (24) | 43 | 38 | – |
| 3.4 | 37 | 68 (30) | 88 | 68 | – |

- **Within the brief**: every briefing (37 to 44 words, limit 50); every debrief (13 to 19,
  limit 30); every play screen at 3 controls plus Lift the lockdown; all eleven LUMA lines
  on the walk's screens whole and 15 words or fewer; no flags (OPENS AT, ladder, coordinates,
  seed or probability) on any play or briefing screen.
- **Over the brief's 30 words**: the room as handed over on twelve levels, and set right on
  fifteen, up to 68 on 3.4. The difference from round 2 is the knob values: 1.1 counts 25
  more words and has 25 knobs. The rooms with few knobs (2.6, 2.8, 3.1, 3.2) stay within 30.
- **Card close** counts the card and whatever of the board shows around it (1.5's 104 is
  its card plus the knob values of the cards beside it). The menu now counts 89 words (51) and the file 517
  (457): the menu itself is unchanged; the room's knob values show behind its veil, and the
  file's page on the room was rewritten for this round.

### Web, round 3

| | Round 2 | Round 3 |
|---|---|---|
| Transferred (the platform's page in headless Chrome, port 8086) | 42.0 MB | 42.4 MB |
| Load (software GL) | 4.1 to 5.4 s | 4.5 s |

Under the 45 MB limit with 2.6 MB in hand. The game loads and runs on the platform
(`Builds/look-audit/game-feel/platform/platform-tech-escape-round3.png`).

### The player test, round 3

A fresh agent, the same prompt, only 1.1's thirteen screens from the last walk (its answer in
full: `Builds/look-audit/game-feel/reviewers/player-round3/answer.md`). It told the level back
whole: the alarm at the Harbor Permits Office, the board in the Risk Room that must list what
the office protects, each item's owner and how bad a leak, a change or a loss would be, the
rehearsal where the attacker takes what nobody wrote down, the shared drive found and added,
the lockdown lifted, the door holding, three stars. "The corridor scenes look good; the parts
where you actually act look like a spreadsheet."

- **What it read as play**: the rehearsal, the real attack ("the best moment of the level"),
  the medal.
- **What it read as a form or instructions**: the briefing ("homework, not a mission"), the
  card brought close ("a database record with knobs on it"), Details ("a teacher's marking
  sheet"). The four board screens read as "a form dressed up as a prop".
- **What it could not tell**: what to click in the room as handed over and where the missing
  thing is; what to do differently after the rehearsal; how the shared drive got onto the
  board (the pin happens between two screenshots); what Camera does.
- **What it saw as broken, and where it stands**:
  - The knob values on the board are too small to read at the room camera's distance.
    True in the frames.
  - The row knobs on the open card are dark on dark. True: the notch shows, the disc barely.
  - LUMA's strip is see-through and the scene shows through it.
  - "Watch which door it goes through" stays up while the player is told to lift the
    lockdown. That is the line LUMA says once the owner is set, before the attack.
  - Smaller framing points: the dot bars at the top over the door frame and window; the
    Details panel over the medal; a door sign cut at the edge of the briefing's corridor
    picture; the close camera cutting the monitor at the left edge.
- **Its three changes**: search the room with the hands and carry the missing thing to the
  board; set the dials on the plates with labels that read, and drop the card, the briefing
  card and the Details panel; make the attack pay off both ways (the attacker running off with
  the item, or reacting when the door holds) and give the player a body or a role.

None of this was acted on after the test: the build committed is the one in the frames.

## What changed in rounds 1 and 2

1. **Verbs, not forms: every row is a thing on the room's prop** (`World/RoomRig.cs`). The
   drawer of chips is gone from all twelve rooms. An asset, an account, a message, a rule or a
   step is a token on the hero: a card on the rolling board, an envelope in the sorter's
   pigeonholes, a badge at the turnstile, a ticket on the bench, a lever module in the
   firewall's rack. Each token carries its short name on a dark plate (the same plate as Robot
   Racing's). Each decision is a switch on its token: a lever for two choices, a dial for
   more, with a lamp that is amber while the decision is open, teal once made, red for the
   choice that closes or flags (Block and log, Suspicious, Remove, Must change). Tap a token to
   bring it close: its full name and its line show on a plate under the grid, and each switch
   shows its name and its value; tap a switch to turn it. Rows that must be ordered are a
   rack: drag a module up or down and it swaps places as it passes. The room's one-off
   decisions sit on a console beside the grid with their names at rest; a record that must be
   signed has a pad under it. What is not on the list yet lies about the room (the folder of
   applicants' papers on a crate, stamped SHARED DRIVE; the lobby printer; the canteen menu on
   the wall): tap it to pin it to the board, Unpin on the card to send it back. Everything
   writes the same `RoomState` the chips wrote, so the checks and the platform report read the
   same values. No model was bought: tokens and switches are built from the kit's own
   materials (the door's steel and trim, the lamp's lens, the sign plate).
2. **The world shows the state.** The board of lamps on the wall follows the tokens; the
   levers and dials show the setup as handed over (the firewall's five levers all up, the
   allow-all's lamp lit; the risk matrix's dials all at the same notch).
3. **The corridor monitor** (`Building.Stage.cs`): a second camera at the corridor's entrance
   renders the corridor, one frame in four, onto a monitor on each room's back wall. While the
   player works the attacker walks the corridor door by door toward this room, waits outside it
   and tries the handle now and then; it never attacks before "Run the attack".
4. **The rehearsal.** The first attack on a room with a planned mistake, as handed over and
   untouched, runs as a rehearsal: the chase, the door giving way, "REHEARSAL · NOT COUNTED"
   under the door lamps, nothing recorded or reported, and "Back to the room" instead of the
   debrief. Back in the room the objective reads "Fix it, then run the attack again.", LUMA
   says what the attack took ("The attack takes whatever is not on the list.") and the thing
   the mistake lives in pulses teal. The next run counts.
5. **The briefing is a transmission**: the corridor from the ceiling camera with this level's
   door lit amber, "Night shift" and the room, three short lines typed in (one line of story,
   the level's own sentences), one button ("Enter the Risk Room"). The YOU FIX box, the twelve
   room chips and the ladder are gone.
6. **One line, one menu.** An objective of at most eight words at the top left
   (`Core/Script.cs`); Esc or the icon opens the menu (`Ui/GameMenu.cs`: the briefing, the
   file, this level again, sound, LUMA's voice, the title, the games menu). Briefing, The file,
   Hide/Open, Title screen and Games menu left the screens. The play screen keeps Camera (up to
   the prop and back) and Run the attack.
7. **LUMA says one short thing when something happens** (`Ui/Luma.cs`): the planned mistake's
   name a few seconds after the room is entered ("An asset nobody listed."), the first sentence
   of her live line after the attack's first banner, the attack's summary at its end, the
   first sentence of the takeaway on the debrief. Shown whole, never typed mid-sentence, 15
   words or fewer, recorded with the other lines (71 recordings; the old intro and live
   recordings that no screen says any more were removed). Her full lines and the curriculum
   are in the file.
8. **The attack** keeps its chase, sparks and banners; its HUD is the row of twelve door lamps,
   nothing else. The "What it tried" column moved to the debrief's Details.
9. **The debrief is a moment, then a door**: the corridor outside the room, its door shut and
   teal (or hanging open and scarlet), the attacker turning away down the corridor, a podium
   before the door with the medal rising on the step it earned and its stars popping one by
   one (Robot Racing's podium, built from the kit's steel and lens), LUMA's one line, Next and
   a quiet Again. The attack's lines, the checks, the stars in all and the rank, the question
   for work, the capstone's if-then plans and the file sit behind Details.
10. **The title**: the facility from the corridor camera, one big Start / Continue and a small
    Levels that opens the facility map (twelve room cards in the corridor's order, each level
    a step with its stars): the one ladder in the game.
11. **Two bugs from the pilot, checked**: 2.4's allow-all rule is the fifth module of the rack,
    on screen with the other four, no scrolling anywhere; the attack's first banner waits until
    LUMA has finished (`Luma.Busy`), and her line now comes after the banner, whole.

## How the checks are met

Unchanged in `Rooms.cs`; the actions write the same picks, order, added set and signature:

| Check | Met by |
|---|---|
| 1.1 every asset on the register; the unlisted one found | the folder pinned to the board (a decoy pinned fails the check, as before) |
| 1.1 an owner and an impact in the three terms | the Owner dial and the Leaked, Changed, Gone dials on every card |
| 2.4 no allow-all; administration unreachable from the public side | the Anything → Anywhere and Internet → Admin levers thrown to Block + log |
| 2.4 each allow has a reason; blocks logged by the last rule; the service still works | the Reason dials; the allow-all module dragged to the bottom; the three service modules left on Allow |
| a dated, signed record (2.1, 2.2, 2.3, 2.5, 2.6, 2.9) | the pad beside the grid |
| the room-wide decisions (1.4 guesses, 1.5 the summary, 2.3 crossings, 2.7 the sign-in, 3.1 severity, 3.4 trigger and cause) | the console card, last in the grid (round 3) |

The platform report is unchanged in form: the same `id:result` string, stars and check list
(`ResultsScreen`), from the same checks on the same state. A rehearsal reports nothing.

## Words and controls, before and after (round 2)

Counted the same way as the inventory (`inventory/tech-escape.md`) and Robot Racing's walk:
every visible word with a letter or a digit, every button the player can press; the world's
own labels count when the camera sees them (the token and switch plates), the kit's door
signs and wall stencils do not (signage, as in the inventory). Every number below is from the
round-2 walk, all nineteen levels, `Builds/look-audit/game-feel/final-round2/counts.jsonl`.

| Screen | Before: words / controls | After: words / controls | Limit |
|---|---|---|---|
| Title | 78 / 22 | 35 / 3 (Start, Levels, the menu) | – |
| The facility map (Levels) | – (it was the title) | 48 / 21 (19 levels, Close, the menu) | – |
| Briefing 1.1 | 142 / 21 | 44 / 3 (Skip, the menu, Enter) | 50 |
| Room 1.1, as handed over | 145 (+90 below the fold) / 14 (+18) | 22 / 3 (Camera, the menu, Run the attack) | 30, 3 + main |
| Room 1.1, a card brought close | – | 47 (the card's full line; while it is close) | – |
| Room 1.1, set right | – | 25 / 3 | 30, 3 + main |
| Rehearsal 1.1 (the attack, the end) | – | 11, 18 / 1, 2 | 30 |
| Attack 1.1 (early, the end) | 34, 47 / 0, 1 | 7, 9 / 1, 2 | 30 |
| Debrief 1.1 | 108 / 5 | 21 / 4 (Details, Again, Next, the menu) | 30 |
| Debrief 1.1, Details open | – | 95 / 6 | – |
| Briefing 2.4 | 137 / 21 | 36 / 3 | 50 |
| Room 2.4, as handed over | 158 (+13 below the fold) / 20 (+4) | 30 / 3 | 30, 3 + main |
| Room 2.4, a module brought close | – | 44 (its full rule) | – |
| Attack 2.4 (early, the end) | 25, 49 / 0, 1 | 0, 10 / 1, 2 | 30 |
| Debrief 2.4 | 115 / 5 | 16 / 4 | 30 |
| The menu | – | 51 / 11 | – |
| The file | ~470 | 457 / 5 | – |

All nineteen levels:

| Level | Briefing | Room as handed over | Card close | Room set right | Debrief |
|---|---|---|---|---|---|
| 1.1 | 44 | 22 | 47 | 25 | 21 |
| 1.2 | 39 | 24 | 52 | 24 | – |
| 1.3 | 44 | 20 | 52 | 20 | 12 |
| 1.4 | 45 | 23 | 40 | 23 | – |
| 1.5 | 32 | 30 | 60 | 30 | – |
| 2.1 | 45 | 21 | 34 | 21 | – |
| 2.2 | 44 | 28 | 45 | 28 | – |
| 2.3 | 43 | 30 | 37 | 30 | – |
| 2.4 | 36 | 30 | 44 | 30 | 16 |
| 2.5 | 44 | 20 | 33 | 20 | – |
| 2.6 | 44 | 19 | 25 | 19 | – |
| 2.7 | 39 | 30 | 34 | 30 | – |
| 2.8 | 39 | 23 | 26 | 23 | – |
| 2.9 | 36 | 23 | 29 | 23 | – |
| 2.10 | 46 | 23 | 39 | 23 | – |
| 3.1 | 37 | 18 | 21 | 24 | – |
| 3.2 | 39 | 19 | 26 | 19 | – |
| 3.3 | 46 | 24 | 39 | 24 | – |
| 3.4 | 36 | 30 | 49 | 30 | – |

Every play screen is within 30 words and 3 controls plus the main action; every briefing within
50; every debrief within 30 with Details closed. A card brought close shows its full name and
line on purpose (it is the file's words on the thing, for as long as it is close: 21 to 60
words). Every LUMA line on every screen is whole and 15 words or fewer (eleven different lines
on the walk's screens). No OPENS AT, ladder, coordinates, seed or probability on any play or
briefing screen; the level map (19 steps) is behind Levels on the title only. The prop fills
30 to 31 % of the frame in every room, as in the realism pass.

## Web (round 2)

| | Realism pass (7 Nov) | Game-feel pass (8 Nov) |
|---|---|---|
| Transferred | 44.5 MB | 42.0 MB |
| Load (headless Chrome, software GL, the worktree's own server on port 8086) | 3.7 s | 4.1 to 5.4 s (four measurements this session) |
| data / wasm on disk | 34.8 / 9.3 MB | 32.2 / 9.4 MB |

Under the 45 MB limit with 3 MB in hand (the thirty-eight old recordings of LUMA's intro
and live lines went; the seventy-one short cues weigh less). Loads and runs on the platform
(`Builds/look-audit/game-feel/platform/platform-tech-escape.png`). The web build's size was
measured by the platform's own page in headless Chrome, as the earlier passes measured it.

## The reviewers, round 1

Two fresh agents, given only the pictures (`Builds/look-audit/game-feel/reviewers/`).

**The player test** (level 1.1's thirteen screens in order). It read the game right: "a
security-training game: night shift at the Harbor Permits Office, every door locked; on the
board are the office's information assets, each with dials for owner, sensitivity and how
bad if changed, leaked or gone; Run the attack sends a hooded intruder through the building
that takes anything not on the board; the first run is a practice, it fails because a Shared
drive was never listed; you add it, run again, the door holds, three stars". It could say
what to do on eleven of thirteen screens. It could not on two: the room as handed over ("the
order says *Put every asset on the board* but five are already on it, and the only things to
touch are 25 dials with no labels") and the camera's close-up (it ended behind the office
chair). Felt like a form: the room (03, "a 5×5 grid of fields made of knobs"), the open card
(07, "a spreadsheet row with knobs instead of dropdowns"), the new card (08). Instructions:
the title's LUMA line and the briefing. Play: the attack ("the one payoff moment where it felt
like a game") and the medal. Broken, as it saw it: the open card's labels piled on each other;
the close-up behind the chair; the clock jumping back between screens and touching the menu
icon; a red bar floating by the monitor; eleven red dots in the attack's strip reading as
eleven failures; the hooded figure crouched at the podium "like it's stealing the medal"; the
Details panel see-through; asset, board, register, list and card used for one thing.

**The Claude Bomb test** (seven side-by-sides). RIGHT on the attack ("a hooded intruder at a
lit door in a red corridor... looks like GTA or a heist game; the best frame on either side")
and the debrief ("unmistakably a game's end screen"); LEFT on the title, the two rooms and the
open card; EVEN on the briefing. Its tells, most visible first: the room puzzles as forms and
tables (five plates of five dials, the firewall as a rule table with column headers when a
module is open); no stakes on screen (no countdown, strikes or progress); the briefing as a
lesson slide; the maxim said twice; web-style button pairs; the menu icon. What already reads
as a game: the attack, the podium, the security-camera framing, the corridor monitor with the
intruder in it, full-screen 3D with no sidebar. Broken: the plates float rather than mount;
2.4's third rule in a smaller font than the others; the crate's levers poking between the
modules; the open module's labels tiny and tilted; the debrief's door sign cut by the frame;
a dark red smear under LUMA's line reading as blood (the attacker's trail). Two of my pairs
were not like for like (Claude Bomb's title mid-fade stood in for its briefing, its briefing
for a debrief); round 2 pairs the briefing with its "Incoming transmission" and, Claude Bomb
having no debrief frame on disk, the debrief with its title.

## What changed after round 1

| Reviewer said | Done |
|---|---|
| 1.1's objective does not match the board; nothing to find | "Find what is missing from the board."; the loose things (the folder, the printer, the menu on the wall) glint now and then; after pinning: "Set its owner. Then run the attack." |
| The open card's labels pile up; tapping a card does not come closer | Tapping a card or module brings the camera to it along the room camera's own line, the card grows by half, its switch names and values stagger, its full name and line stand under it |
| The close-up ends behind the chair | The camera closes in along the room camera's line of sight, so nothing the feed does not show can come between |
| No progress or stakes on screen | A row of lamps at the top of the room, one per row on the prop and one for the console and the pad, amber until set, teal once set (the attack's door lamps, Claude Bomb's module dots) |
| The rack's third rule in a smaller font; levers poking through; plates floating | One text size for every module; the modules mounted on a dark rack panel with two rails, 2 cm apart |
| Eleven red dots read as failures | Doors not yet reached are dim; amber this room, teal held, scarlet breached |
| The clock jumps back and touches the menu icon; a red bar by the monitor | One clock for the night, whatever the screen, clear of the icon; the monitor's REC lamp was a deep box sticking out of the screen, now flat |
| The intruder crouching at the medal; the red smear; the door's sign cut; Details see-through | The intruder already walking away down the corridor; no trail on the debrief; the camera higher; Details opaque, Close clear of the menu icon |
| Medal: a flat disc, no stars | The stars had been culled (their triangles faced the wall); now three stars in an arc over the disc, lit as earned |

Not done, as beyond the pass or the owner's to decide: a countdown in the rooms (the design
says the attacker is never punishing and never attacks early); per-room physical objects in
place of the generic levers and dials (see Waiting for Charbel); the briefing's third line,
the level's maxim, approved in the design.

## The reviewers, round 2

**The player test** (a fresh agent, 1.1's thirteen screens after the fixes). It told the whole
level back: "make a whiteboard list of the office's information complete and correct before
an attacker is let loose; run a practice attack; a hooded figure walks into the Risk Room and
steals the one thing that wasn't on the board; add the Shared drive, give it an owner, run
the real attack; the door holds; gold medal". It now names what to do on ten of thirteen
screens. It could not tell on three: how to find the missing thing in the room as handed over
("nothing in the room points to it"), whether the owner was set on the new card (the
objective still said "Set its owner"), and what the close-up was for. Felt like a form: the
room (03, "a table of rows and columns dressed up as a whiteboard") and above all the open
card (07, "a five-field record with knobs instead of dropdown menus"). Instructions: the title
and the briefing. Cut-scenes rather than play: the two attacks ("good contrast between the
door left open and the door that held, but I only watch"). Broken, as it saw it: the open
card still overlapping its neighbours; the knobs unlabelled at rest; LUMA's attack line said
while the owner was being set; the overview after the attack dark and tiny, the win and the
loss looking alike; the takeaway said three times; the podium's second and third steps in a
one-player game; a feed bracket poking above Run the attack; LUMA's strip touching the
Camera button.

Acted on before the last build: the loose things in the room pulse with a small lamp of their
own (the folder, the printer, the menu: one of them is the asset); the objective moves on to
"Run the attack." once the owner is set and LUMA's "Watch which door it goes through." comes
then; the attack ends held on the door (shut and teal, or hanging open and scarlet) instead
of the overview; Details no longer repeats the takeaway and spells out "stars in all"; the
feed's lower brackets go where the buttons are; LUMA's strip sits clear of the Camera button.

**The Claude Bomb test, round 2** (a fresh agent, the side-by-sides with the briefing paired
against Claude Bomb's "Incoming transmission"). RIGHT on the attack ("a real game cutscene and
the best screen on either side") and the debrief (by default: Claude Bomb has no debrief frame
on disk, so its title stood in); LEFT on the title (narrowly), the briefing (clearly: "the left
is an incoming radio message with a threat, a deadline and a role; the right is the start
screen of a course module"), the two rooms ("the right has the better full-screen room and
less interface, but what you play is a whiteboard of labelled tiles, the clock counts up, and
nothing is at risk") and the open module (decisively). Its tells, most visible first: the
firewall box as a rule table (arrows, dots, port numbers in brackets); the close-up as a form
and a glossary; the title as a landing page; the maxim on the briefing and the debrief; the
briefing's welcome with no threat, deadline or role; the 1.1 board as an asset checklist;
"Run the attack" as a Submit button; the dots at the top as a page indicator; no pressure.
What reads as a game: the attack, the podium and medal, full-screen 3D with no sidebar, the
camera framing and the corridor monitor, the lighting and props, the firewall box's five status
lamps, the objective line. Its three changes: a countdown with consequences instead of the
Submit button (the attack comes when the clock runs out or when a lever is pulled); every table
an object (cables between labelled sockets, a key cabinet with empty hooks and name tags; the
words off the panels); open with the intruder, not the maxim (the title, the briefing as an
alarm).

Acted on before the last build: the open module's full rule reads above the rack instead of
over the next module, the module's switch names and values at the console's size; the title
smaller, clear of the camera frame; the debrief's LUMA line bottom left and narrow, clear of the
podium; the medal's lamp gone (it threw a yellow pool on the floor). Not acted on, as the
owner's call (see Waiting for Charbel): the countdown, the objects in place of rule rows and
dials, the briefing rewritten as an alarm, the maxim off the briefing.

## Where Tech Escape stands

All nineteen levels run on the new screens with Charbel's round-4 answers and the Higgsfield
files in. Every machine check of the brief holds on every screen of the last walk again: the
knob values are off the board, so every room is within 30 words. On 1.1 the unlisted drive is
found by turning the camera, glints as the camera passes it and flies to the board; the
attack ends with the attacker running off with the drive, or giving up at a door that held.
2.1 and 3.1 carry the staff badges, 2.4's rules stand on the patch panel, and the web build is
43.2 MB.

The player test, round 4, tells the level back whole and calls two of the new moments the
best in it: finding the drive, and the attacker running off with it. It still reads the card
brought close and Details as forms and the briefing as instructions, and it asks for
hands-on actions in place of the card's rows.

Known in the last frames, not fixed:
- On 2.9 the corridor monitor covers the C of the wall's BACKUP lettering (the stencils are
  not among the things the monitor keeps clear of).
- LUMA's strip is see-through: left for the shared fix after the merge, as asked.
- The open card's lamp and the board's lamps are small from the room camera.

## Waiting for Charbel

1. **The lamps tell the answer.** Teal when right means a player can turn a knob until its
   card's lamp goes teal, before any attack: the lamps judge each row with the room's own
   checks. That is what was asked ("teal when right, amber when not"). If you meant teal once a
   row is set, whatever its value, it is one line (`RoomScreen.Changed` and `RoomRig.ShowRight`
   would use `Decided` again). Placeholder: teal when right.
2. **The patch panel is the rack's bottom unit, not the rack.** The model came as a 2U panel
   full of cables; above the rules its cables would cover them, so it sits below. A panel with
   five labelled sockets and one cable per rule (the first spec) would let each rule be its
   own cable; that would be a new model. Placeholder: the panel under the five modules.
3. **No Claude Bomb debrief frame on disk**, and the Claude Bomb test was not run again in
   rounds 3 and 4; you asked for the player test.

After the merge, not waiting on anything: Robot Racing's podium gets the single step, and
LUMA's strip is fixed once for all games.

## Files

- Code: `games/tech-escape/Assets/Scripts/Core/Script.cs`, `World/RoomRig.cs`,
  `World/Building.Stage.cs`, `Ui/GameMenu.cs`, `Ui/Hud.cs` (new); `Core/Facility.cs`,
  `Ui/App.cs`, `AttackScreen.cs`, `CameraFeed.cs`, `DemoDriver.cs`, `FileSheet.cs`,
  `LessonScreen.cs`, `Luma.cs`, `ResultsScreen.cs`, `RoomScreen.cs`, `Sfx.cs`, `TitleScreen.cs`,
  `UiKit.cs`, `World/AttackerRig.cs`, `World/Building.cs`, `World/CameraRig.cs`,
  `World/SceneCamera.cs`, `Editor/FacilityKit.Prefabs.cs` (changed); `Resources/LumaVoice/` (72
  recordings).
- The Higgsfield files: `Assets/Models/Hero/PatchPanel/` (geometry and maps),
  `Assets/Resources/Facility/Hero/PatchPanel.prefab`, `Assets/Resources/Badges/` (six badges),
  `Tools/glb_maps.py` (the cleanup tool), recorded in `games/tech-escape/Docs/credits.md`.
- Frames (not in git), under `games/tech-escape/Builds/look-audit/game-feel/`:
  - `final/`: the last walk, round 4, 130 pictures and `counts.jsonl`.
  - `final-round3/`, `final-round2/`: the earlier rounds' walks (round 2's with `side-by-sides/`).
  - `reviewers/`: what each reviewer saw; `player-round3/answer.md` and
    `player-round4/answer.md` are the last two answers.
  - `claude-bomb/`: the reference frames.
  - `platform/`: the game on the platform, rounds 2, 3 and 4.
- Docs: this report; `Docs/game-feel/tech-escape.md` (the owner's answers recorded, rounds 2
  to 4); `games/tech-escape/README.md`.
