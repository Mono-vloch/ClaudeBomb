# Tech Escape: the game-feel design

Level 1.1 "What is worth protecting?" (the Risk Room) and level 2.4 "What is allowed through?"
(Firewall Control). Shared rules (menu, LUMA, briefing, debrief, measuring): [README](README.md).
Every word of every screen today, verbatim: [inventory/tech-escape.md](inventory/tech-escape.md);
screenshots in `games/tech-escape/Builds/look-audit/game-feel/before/`.

## The game in one paragraph

Today each room is a camera feed with a drawer beside it: a "You fix" paragraph, then rows of
word chips (Owner, Sensitivity, If leaked, If changed, If gone on every asset; Action and Reason
on every rule), half of them below the drawer's fold, and the room's own prop stands in the feed
doing nothing. After: **the fix is done on the room's prop.** In the Risk Room the player pins
assets to the rolling board, clips an owner's badge to each card and sets three magnets for the
three ways it can be lost; the unlisted asset is a folder on the clerk's desk that has to be
found. In Firewall Control the player throws levers on the console, hangs a reason tag on each
lever left open and slides the modules into order. Then the facility runs the attack, which stays
as it is, the best moment in the three games, with its HUD cut down to the door lamps.

## Found while counting

- **2.4's planned mistake is below the fold.** Rule 5, "Everything else, from anywhere to
  anywhere", the allow-all the level is about, sits under the drawer's edge: a player who does not
  scroll never sees it.
- **1.1's hidden asset is a button.** The thing to find, "+ Working copy of applicants' documents on
  the shared drive", is offered by name as a button at the bottom of the drawer, with the two decoys.
  Finding it takes a scroll, not a look round the room.
- **LUMA's attack line can never be read.** On "THE ATTACK BEGINS" it is still typing when the banner
  goes (12 of 15 words on 1.1, three words cut mid-word on 2.4).
- The folded drawer's only word renders broken ("Ope" over "n"); both file sheets are cut at the
  bottom edge mid-sentence. Both go with the drawer and are fixed in the file.

## Forms today, actions after

| Today (the room's drawer) | After (the room's prop) |
|---|---|
| Risk Room: five asset rows, five chips each (Owner, Sensitivity, If leaked, If changed, If gone) | The rolling board of risk cards. Each asset is a card with a pictogram (a database drum, an ID folder, a globe, an envelope, a tape). On each card: an owner's badge clipped on (a photo and a first name: Dana, Omar, Lina, Nour, or no badge for "Nobody"), a coloured corner for sensitivity (green Public, amber Internal, red Confidential), and three magnets under it for leaked, changed and gone, each turned to one, two or three bars (Low, Medium, High). Tap a card to bring it close: its name and its one line ("Every application and decision, in the main database.") show there, and its badge, corner and magnets are set there. |
| "Something not on the list? Add it" with three buttons | Find it in the room: the working copy is a folder of applicants' papers on the clerk's desk, stamped SHARED DRIVE; the lobby printer stands by the door; the canteen menu is on the notice board. Pick one up and pin it to the board. A decoy can be pinned (and unpinned): that is the check "every asset in the facility is on the register". |
| Firewall Control: five rules, each with ▲ ▼, an Action chip and a Reason chip | The console of switches. Each rule is a lever module in a rack, read top to bottom (the first that matches wins). Lever up: allow, teal lamp. Lever down: block and log, red lamp and a paper tape that prints a line when something is blocked. Each open lever has a reason tag hanging from it: tap to swap it ("Needed by the service", "So nothing breaks", none). Drag a module up or down the rack to change the order. The modules carry short labels ("WEB 443 → PORTAL", "STAFF → ADMIN", "PUBLIC 3389 → ADMIN", "DATA → BACKUP", "ANY → ANY"); tap one for its full rule. |
| Hide / Open, Briefing, The file | The menu (Esc or the icon). |
| "YOU FIX" paragraph, "THE SYSTEM · DECIDE EACH ROW", "Fix it: …" | The objective line and LUMA's one sentence. The paragraphs move to the file. |

The other ten rooms get their verb the same way in the build after this pilot (the prompt's
ideas: tag the asset with a label gun, turn the safe's dial, lock the badge gate, move the tapes
out of reach); they are not designed here.

## Tension: the corridor monitor

On the wall of every room, a small monitor shows the corridor camera. While the player works,
the hooded attacker moves along the corridor, door by door, towards this room, and stops outside
it, trying the handle now and then. It never attacks before "Run the attack"; it is there to be
seen. When the attack runs, the monitor is where it starts.

## Level 1.1 (Risk Room)

### Title

**Before: 78 words, 22 controls.** Wordmark 1 · "Sound on" 2 · "Games menu" 2 · "TECH ESCAPE · GAME 04"
4 · "Nothing Gets Through" 3 · "Fix what is misconfigured. Then the facility runs the attack." 10 · LUMA 1
+ "I'm LUMA, your coach. Nineteen levels in twelve rooms. Press Start: the first door is the Risk Room."
18 · "TIER 1 · WHAT IS AT RISK" 6 + five nodes 5 · "TIER 2 · PROVE THE CONTROLS WORK" 6 + ten nodes 10 ·
"TIER 3 · THE FINAL EXIT" 5 + four nodes 4 · "Start" 1. Controls: Sound on, Games menu, 19 nodes, Start.

**After: about 40 words, the map + Start + the menu.** As today, the level map the only ladder in
the game, the tier names shortened to "TIER 1", "TIER 2", "TIER 3" (full names to the file),
"Sound on" and "Games menu" in the menu. LUMA: "Press Start: the first door is the Risk Room."
(9)

### Briefing (a transmission)

**Before: 142 words, 21 controls.** "LEVEL 1.1 OF 3.4 · RISK ROOM · THE BRIEFING" 8 · "What is worth protecting?" 4
· "You cannot protect what you have not listed, and nobody protects what nobody owns." 14 · LUMA 1 + "Welcome
to the Harbor Permits Office. Before anything else: what does this service hold, who owns each thing, and
what would its loss cost?" 24 · "YOU FIX" 2 + "The asset register: for each system, what it holds, who owns it,
how sensitive, and the impact of its loss in each of the three ways." 26 · "FIX IT · AN ASSET NOBODY LISTED"
6 · "THE FACILITY, ROOM BY ROOM" 5 + twelve room chips 24 · "Enter the room" 3 · "The file" 2 · "Title screen"
2 · "THE LADDER" 2 + nineteen steps 19. Controls: Enter the room, The file, Title screen, 18 ladder steps.

**After: about 40 words, 1 control (Skip) + the button.** The lockdown corridor from the ceiling
camera (REC, CAM 00, the clock), the Risk Room's door lit amber, the others dark. On the right:

> **Night shift** · 23:47
> Welcome to the Harbor Permits Office.
> Lockdown: every door sealed until the attack is run.
> You cannot protect what you have not listed, and nobody protects what nobody owns.

Button: "Enter the Risk Room". The first line is LUMA's, the third the kicker, the second new. The
"You fix" box, the twelve room chips (the corridor shows the rooms) and the ladder go.

### The room

**Before: 145 words (+90 below the fold), 14 controls (+18 below).** The feed "REC CAM 01 / RISK ROOM
23:47:04 PERMIT OFFICE / LOCKDOWN" 9 · LUMA 1 + "Fix it: An asset nobody listed. The ready register misses the
working copy of applicants' documents kept on the shared drive, outside the main system. The facility's
attack takes exactly that one." 32 (fades) · drawer: "LEVEL 1.1 · RISK ROOM" 4, "Hide" 1, "What is worth
protecting?" 4, "Fix it: An asset nobody listed" 6, "Briefing" 1, "The file" 2, "RUN THE ATTACK" 3, "YOU FIX" 2 + 26,
"THE SYSTEM · DECIDE EACH ROW" 5, two asset rows of name, detail and ten chip labels and values 49 · below the
fold three more rows and "SOMETHING NOT ON THE LIST? · ADD IT" with "+ Working copy of applicants' documents on
the shared drive", "+ The lobby printer", "+ The canteen menu" 90. Controls: Hide, Briefing, The file, RUN THE
ATTACK, 25 chips, 3 add buttons.

**After: about 20 words at rest, 2 controls (menu, camera) + "Run the attack".** The Risk Room
through its camera feed, full width: the board on the back wall with its five cards, the clerk's
desk, the printer by the door, the notice board, and the corridor monitor with the attacker.
- Objective: "Put every asset on the board." (6)
- Camera: one control steps up to the board and back to the room.
- At rest the board shows pictograms, badges with first names and magnets: 5 to 6 words.
- A card brought close shows its name and its line (about 12 words) while it is close.
- Teach by trying: the board as handed over can be attacked at once. The attacker walks out of the
  monitor to the Risk Room, takes the folder off the desk, and leaves: THROUGH. LUMA: "The attack
  takes whatever is not on the list." (9)
- LUMA, entering the room: "An asset nobody listed." (4, the planned mistake's name) once the
  room has been looked at for a few seconds, then nothing until something happens. When the
  folder is pinned: "Watch which door it goes through." (6) as the "Run the attack" lever lights.
- Gone: the drawer and everything in it. The "You fix" paragraph and LUMA's full line go to the
  file; Briefing and The file to the menu.

### The attack

**Before: 34 words early (LUMA cut, typing), 47 at the moment and the end, 0 controls then "See
the debrief".** "THE FACILITY RUNS THE ATTACK" 5 · "Level 1.1 · Risk Room" 4 · "THE DOORS" 2 + twelve
lamps · "THROUGH 0 · HELD 1" 4 · "WHAT IT TRIED" 3 + "HELD Risk Room" 3 · the banner "NOTHING GOT THROUGH" 3 +
"The register held: nothing unlisted to take." 7 · LUMA 1 + "The attack takes whatever is not on the list.
Watch which door it goes through." 15 · "See the debrief" 3.

**After: about 15 words, 0 controls, then "Debrief".** As today: the camera flies over the
facility, the attacker runs the corridor, the doors hold or break. The HUD is the row of twelve
door lamps at the top, nothing else. The banners stay: "THE ATTACK BEGINS" (3), then "HELD" or
"THROUGH" with a short line ("The register held.", 3). LUMA's line comes after the first banner,
whole: "Watch which door it goes through." (6). At the end, the summary as LUMA's line ("The
register held: nothing unlisted to take.", 7) and one button, "Debrief". The "What it tried"
column moves to the debrief's Details.

### Debrief

**Before: 108 words, 5 controls** (117 on the unfixed variant: "NO MEDAL YET", four red lamps).
"Debrief · Level 1.1" 3 · "Risk Room: What is worth protecting?" 6 · "THE ATTACK" 2 + "Held · Risk Room" 3 +
"The register held: nothing unlisted to take." 7 · LUMA 1 + the takeaway 14 · "RISK ROOM · THROUGH 0 · HELD 1"
6 · "THE MEDAL" 2, "GOLD" 1, "3 of 3 stars · 3 in all · Trainee" 8 · "THE CHECKS" 2 + four checks 20 · "UNLOCKED ·
LEVEL 1.2" 3 + "Risk Room: How likely, how bad?" 6 + "3 of 3 stars banked · 3 in all · Trainee" 9 · "Next: 1.2 How
likely, how bad?" 6 · "This room again" 3 · "The file" 2 · "Title screen" 2 · "Games menu" 2.

**After: about 25 words, 1 control (Details) + "Next", "Again".** The corridor outside the Risk
Room: the door shut with its lamp teal, the attacker turning away (or, if it got through, the
door open and red). The medal with its stars, "GOLD" (1). LUMA: "You cannot protect what you have
not listed, and nobody protects what nobody owns." (14). "Next" and "Again". Details: the
attack's lines, the checks, the stars in all and the rank. The unlock card goes; The file, Title
screen and Games menu are in the menu.

## Level 2.4 (Firewall Control)

### Briefing

**Before: 137 words, 21 controls.** As 1.1's, with "What is allowed through?", the kicker "Allow by
name, block the rest, log the block.", LUMA's 35-word intro, "YOU FIX" + "The rules on each line from the
Network Maze: action, reason, and their order." 16, "FIX IT · ALLOW EVERYTHING, AT THE BOTTOM" 7, the room
chips, the ladder.

**After: about 35 words.** The corridor, Firewall Control's door lit amber. On the right:

> **Night shift** · Firewall Control
> A rule set that ends with "allow everything" is no rule set.
> Allow by name, block the rest, log the block.

Button: "Enter Firewall Control". (LUMA's first sentence and the kicker, verbatim.)

### The room

**Before: 158 words (+13 below the fold), 20 controls (+4 below).** The feed 9 · LUMA 1 + "Fix it: Allow
everything, at the bottom. The ready rules allow the right things and end with an allow-all "so nothing
breaks"; remote administration is open to the public side. The facility runs the attack through the open
administration port." 40 (fades) · the drawer head as 1.1's with "Fix it: Allow everything, at the bottom" 7,
"YOU FIX" + 14, "THE SYSTEM · ORDER AND DECIDE" 5 · four rule rows of a number, the rule's full name (6 to 11
words), ▲ ▼, "Action Allow", "Reason Needed by the service" or "So nothing breaks" · below the fold rule 5
"Everything else, from anywhere to anywhere", "Allow", "So nothing breaks". Controls: Hide, Briefing, The
file, RUN THE ATTACK, 10 arrows, 10 chips.

**After: about 25 words at rest, 2 controls (menu, camera) + "Run the attack".** The console
close, full width, the corridor monitor on the wall beside it. Five lever modules in the rack,
all up, the last one ANY → ANY with its tag "So nothing breaks", and PUBLIC 3389 → ADMIN up with
the same tag.
- Objective: "Close what should never be open." (6)
- At rest: five short labels (17 words) and the reason tags on open levers.
- The verbs: throw a lever (allow or block and log), tap a tag to swap it, drag a module up or down.
- Teach by trying: run it as handed over; the attacker comes through the public side on 3389 and
  the PUBLIC 3389 → ADMIN lamp flashes as it passes: THROUGH. LUMA: "The attack tries the
  administration port from the public side." (10, the first half of her live line)
- LUMA, entering: "A rule set that ends with "allow everything" is no rule set." (11) only if it was not
  in the transmission just before; otherwise nothing until something happens.
- Gone: the drawer, the arrows, the chips, the numbers 1 to 5 (the rack's order is the number).

### The attack and the debrief

As 1.1's. The attack's "Before": 25 words early (LUMA cut mid-word), 49 at the moment and the end;
the variant through the open port, 55. The debrief's "Before": 115 words (123 on the variant, "NO
MEDAL YET" with two checks of five teal). After: the attack with the door lamps only, the blocked
lever's tape printing its log line on the monitor as the attacker is turned away; the debrief with
Firewall Control's door, the medal, LUMA's "Allow by name, block the rest, log the block." (9), Next,
Again, Details.

## How the checks are met

| Check | Met by |
|---|---|
| 1.1 Every asset in the facility is on the register; the unlisted one was found | The folder pinned to the board; the printer and the menu not pinned. Stored as `added` is today. |
| 1.1 Each has an owner; an impact in the three terms | A badge on every card; the three magnets set on every card. Stored as the chips' picks. |
| 2.4 No allow-all; administration unreachable from the public side | ANY → ANY and PUBLIC 3389 → ADMIN levers down. |
| 2.4 Each allow has a reason | A "Needed by the service" tag on every open lever. |
| 2.4 Blocks are logged, by the last rule | ANY → ANY down and at the bottom of the rack. |
| 2.4 The service still works | WEB, STAFF and DATA levers up. |

## Open questions, answered (8 November 2026)

1. **Short labels on the console.** Plain words first, the port after: "Internet → Admin ·
   Remote Desktop (3389)"; the full rule on tap (the card brought close) and in the file.
2. **The attacker on the monitor.** It walks to the room's door while the player works, and
   never attacks early.
3. **Teach by trying.** The first attack on an unfixed room is a free rehearsal: no debrief,
   not counted in the score or the platform report; the next run counts.

### After the round-2 report (8 November 2026)

4. **The close-up.** A card brought close is shown face-on and flat, one row at a time,
   everything readable. Every room's close-up is checked.
5. **Values on the controls.** Every control shows its current value on its face ("Owner:
   Omar", "Sensitivity: High"), as on Robot Racing's console.
6. **Models.** Yes to the staff ID badges and the patch panel with cables; Charbel gets them
   made. The current props stay until the files arrive.
7. **No countdown.** When the attacker on the monitor reaches the room's door, he stops there
   and tries the handle, with a sound, until the player is ready.
8. **The main action** is "Lift the lockdown" (was "Run the attack").
9. **The briefing** is the alarm and the story only. The motto moves to LUMA's line on the
   debrief.
10. **The podium** has a single step. Robot Racing's podium gets the same, after the merge.
11. **Fixes asked**: the Operations Center monitor behind the console, and 1.5's small console.

### After the round-3 report (8 November 2026)

12. **Knob values** show on the card brought close and on the knob just turned, nowhere else.
    From the room camera each row on the board shows its lamp, teal when it is right and amber
    when not, and no values. The card brought close may go over 30 words: it is the reading
    moment.
13. **The badges and the patch panel** go where proposed: the badges on 2.1's turnstile
    accounts and on 3.1's roles, the patch panel as 2.4's firewall rack. Charbel gets the
    files made.
14. **Finding what nobody listed.** On levels about something nobody listed, the missing item is
    in the room (a drive under the desk, a device on a crate). It glints when the camera passes
    it; tapping it flies it to the board. The room's camera lets the player look around enough
    to find it.
15. **The attack pays off both ways.** If the door gives way, the attacker grabs the item and
    runs off with it; if it holds, he rattles the handle, gives up and walks away. No body or
    role for the player.
16. **The knobs on the open card** are light on dark, readable at a glance. LUMA's strip stays
    as it is: it is shared, and is fixed once after the merge.

The builds that follow these answers are reported in [tech-escape-report.md](tech-escape-report.md).
