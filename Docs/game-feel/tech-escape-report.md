# Phase 7c: Tech Escape on the game-feel screens

8 November 2026, branch `game-feel-tech-escape` (a worktree off `game-feel-pass` at 932b511),
from `Docs/game-feel-pass-prompt.md` and the design in [tech-escape.md](tech-escape.md) with the
owner's three answers (console labels in plain words with the port after; the attacker walks
to the door and never attacks early; the first attack on an unfixed room is a free rehearsal).
All nineteen levels run on the new screens. Only `games/tech-escape/`, this file and the design
doc's answers changed; the kit, the server and the other games are untouched. Frames and counts:
`games/tech-escape/Builds/look-audit/game-feel/` (not in git).

## What changed

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
| a dated, signed record (2.1, 2.2, 2.3, 2.5, 2.6, 2.9) | the pad under the console |
| the room-wide decisions (1.4 guesses, 2.3 crossings, 2.7 the sign-in, 3.1 severity, 3.4 trigger and cause) | the console's switches, named at rest |

The platform report is unchanged in form: the same `id:result` string, stars and check list
(`ResultsScreen`), from the same checks on the same state. A rehearsal reports nothing.

## Words and controls, before and after

Counted the same way as the inventory (`inventory/tech-escape.md`) and Robot Racing's walk:
every visible word with a letter or a digit, every button the player can press; the world's
own labels count when the camera sees them (the token and switch plates), the kit's door
signs and wall stencils do not (signage, as in the inventory). Every number below is from the
last walk, all nineteen levels, `Builds/look-audit/game-feel/final/counts.jsonl`.

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

## Web

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

All nineteen levels run on the new screens and meet every machine check of the brief on every
screen of the walk. The player test, round 2, tells the whole of level 1.1 back from the
pictures and names what to do on ten of thirteen screens; the three it could not read are
fixed in the build whose frames are in `final/` (the loose things now pulse with a light of
their own, the objective moves on once the owner is set, the attack ends held on the door),
and no reviewer has seen those frames yet. Both reviewers prefer the new attack and debrief to
anything before; both still read the rooms as forms in 3D (a board of dials, a rack of rules)
and the briefing as a lesson's opening, and the Claude Bomb test names the missing pressure:
nothing counts down, and "Run the attack" waits like a Submit button. Those are the owner's
calls below.

Known in the last frames, not fixed: in the Operations Center (2.7, 3.1, 3.4) the wall has no
free place in the feed, so the corridor monitor stands partly behind the console; 1.5's
console had to shrink to clear the bin, and its values are small until the camera comes close;
a card brought close shows 21 to 60 words while it is close (its full line, from the file).

## Waiting for Charbel

1. **Pressure in the rooms.** Both reviewers ask for it. Today the attacker waits at the door
   on the monitor and never attacks early (your answer to the design's question 2). A
   countdown to midnight that runs the attack by itself, or the attacker trying the handle
   harder as time goes, would change that answer. Placeholder: none; the clock on the feed is a
   wall clock.
2. **Objects instead of switches.** Both reviewers read the boards of dials and the rack of
   rules as forms. The design's rooms (badges clipped on, magnets, lever modules, tags) were
   built from the kit's own steel and lenses, so they share one look: a dark plate with dials.
   Per-room objects (owner badges with faces, colour-corner cards, cables between labelled
   sockets, a key cabinet with empty hooks) would need models. If you want them, the first two
   I would ask Higgsfield for: (a) a set of four staff ID badges on lanyards, front-facing,
   photo and first name (Dana, Omar, Lina, Nour), flat card about 9 by 6 cm, PBR, under 3k
   triangles each; (b) a wall-mounted patch panel with five labelled sockets and five short
   cables with plugs, 60 by 30 cm, PBR, under 8k triangles. Placeholder: the plates and dials.
3. **The briefing as an alarm.** The Claude Bomb test wants the transmission to name the
   threat, the deadline and the player's role, and the maxim off it. The three lines are your
   approved design (the level's own sentences); a rewrite is yours to approve. Placeholder: as
   designed.
4. **The podium's second and third steps.** The player test asks why a one-player game has
   them; Robot Racing's podium has them too. Keep both alike, or drop the side steps in both?
5. **No Claude Bomb debrief frame on disk.** The side-by-side for the debrief uses Claude
   Bomb's title, so that pair is not like for like.

## Files

- Code: `games/tech-escape/Assets/Scripts/Core/Script.cs`, `World/RoomRig.cs`,
  `World/Building.Stage.cs`, `Ui/GameMenu.cs`, `Ui/Hud.cs` (new); `Ui/App.cs`, `AttackScreen.cs`,
  `CameraFeed.cs`, `DemoDriver.cs`, `FileSheet.cs`, `LessonScreen.cs`, `Luma.cs`,
  `ResultsScreen.cs`, `RoomScreen.cs`, `TitleScreen.cs`, `UiKit.cs`, `World/Building.cs`,
  `World/CameraRig.cs`, `World/SceneCamera.cs` (changed); `Resources/LumaVoice/` (71
  recordings; the 38 old intro and live recordings no screen says any more are removed).
- Frames (not in git): `games/tech-escape/Builds/look-audit/game-feel/final/` (the last walk,
  118 pictures, `counts.jsonl`, `side-by-sides/`), `reviewers/` (what each reviewer saw),
  `claude-bomb/` (the reference frames), `platform/`.
- Docs: this report; `Docs/game-feel/tech-escape.md` (the owner's answers recorded);
  `games/tech-escape/README.md`.
