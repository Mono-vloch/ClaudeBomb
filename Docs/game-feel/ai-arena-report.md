# AI Arena: the game-feel pass

8 November 2026, branch `game-feel-ai-arena` (a worktree from `game-feel-pass` at 932b511), from
`Docs/game-feel-pass-prompt.md` and the approved design `Docs/game-feel/ai-arena.md`, with the
owner's answers: dropping produce writes only the plain part of the list (the catch and the rule
for an empty view stay in the player's own words, because the checks read them there); the thought
bubble shows Claude's first sentence without coordinates and the whole thought is one tap away;
the story is four 3D shots, first play only, skippable. Carried over from Robot Racing's Phase 7:
every control the player sets has a short name plate on the object; no floating white 3D text,
every world label on the same dark plate; the question is the objective line; a number that is
the lesson stays in the world; the podium and the ribbon medal.

## What changed

1. **Packing the agent (`World/Market.Garage.cs`, `Ui/SetupScreen.cs`).** The garage is the agent
   at a market table by its house. On the table: the errand's produce and up to three
   look-alikes, its basket, the "No" crate, the family's phone, the coins, and from 1.3 the
   trays of parts. Produce dragged (or tapped) into the basket writes "Buy apples." on the note
   pinned to its chest; into the crate, "Never buy tomatoes."; back on the table takes it off.
   The world shows what the note says (`Core/NoteText.cs` reads the text the way the garage
   shows it, and rewrites only the plain sentences), so a list typed by hand lays the produce
   out the same way. The note opens full size on a tap: the one text field, the lesson. The
   phone opens the message on a tap and closes again. Coins tapped into the battery on the
   agent's hip light its ring of notches as far as the money lasts at this list's cost; a tap on
   the battery takes one back. No physics: picking by projection to the screen, dragging along
   the table's plane, drop places by screen distance. "Send": the basket flies onto its back, it
   turns and walks off.
2. **The parts are fitted to the agent.** Sight: three lenses on the tray (small to wide), the
   fitted one a ring on its eye and the sight square on the cobbles. Memory: a stack of three
   cards, a stack of ten, or the notebook; the fitted one rides on the agent (the glass cards
   behind its head, the notebook at its side), a tap on it takes it off. Brain: three bulbs
   (teal HAIKU, cream SONNET, gold OPUS); the fitted one on the antenna with its name on a plate
   over it. Effort: the dimmer at the antenna's foot, three clicks. Each tray has its name plate
   (Sight, Memory, Brain, Effort), the crate "No", the coins "Coins", the battery "Battery".
   Locked parts are not shown.
   Arrivals: "NEW: SIGHT / MEMORY / BRAIN", "EVERYTHING OPEN" with LUMA's line, first visit only.
3. **The round (`Ui/ArenaScreen.cs`, `World/Market.Round.cs`).** The street behind the agent
   pulled back so the market shows ahead (`MarketCamera.followBack` 1.9 m, was 1.12). One
   objective line (the message's gist, at most eight words), the clock at the top (six to seven,
   no digits, amber in the last five turns) and on a post on the square, the home window at the
   top right with one face per agent (crossed when sent home, lit when home; a tap follows that
   agent). On the agent: the battery draining, its thought in a bubble (the first sentence, the
   coordinates and the brain's name taken out; the whole thought one tap away). The loop on the
   agent: it looks on LOOK, the antenna pulses on DECIDE, it walks on MOVE; on 1.1 three small
   glyphs (an eye, a bulb, a foot) light in turn under the clock. Three controls: the menu, the
   camera (street, the agent's eye, the market from above), Call home. Moments: 3, 2, 1, AGENT
   OUT, SENT HOME with "Nadia bought a lemon.", OUT OF TOKENS; at the end the result stays until
   Debrief (HOME with "5 on the list in the basket", SENT HOME, CALLED HOME, ALL HOME for a
   class). The drawer, the cards, the turn counter, the stats and the view tabs went.
4. **The briefing is a transmission (`Ui/LessonScreen.cs`).** The house at dusk in the home
   camera's feed (the agent at the door, the window lit), and on the right the family's message
   typed in under "Mom · 6:00 PM", two short lines of the errand's story, "Pack the agent". The
   question, the kicker card, LUMA's intro, the part chips and the ladder went (to the file and
   the title).
5. **The debrief is a moment, then a door (`Ui/ResultsScreen.cs`).** Dusk at the family's door;
   the agent on its podium step with its basket on its back and what it bought tipped out in
   front of it (a red glow under anything that ruined dinner); the medal rising on its ribbon
   behind it and the stars popping; LUMA's one line (the takeaway's first sentence); Next, Again,
   Details. On 1.5 the cost per item, the lesson's number, stands on a plate beside the agent
   ("$0.0008 an item" in the walk). Details holds the checks, the agents' figures and costs, the lists, the
   stars in all and the rank, the takeaway and the question for the manager, the if-then plans
   on the capstone, and the file. The report to the platform is built from the same checks and
   the same strings as before.
6. **The story (`Ui/StoryScreen.cs`)** is four shots of the world, once: the house at dusk and
   the phone lighting up on the step ("Every evening, the family sends a message about
   dinner."); the agent at the door with its note ("That list is the agent's job
   description."); it walks into the market and looks around ("Once it leaves, there is no
   phone."); it buys a tomato and is sent home ("Say exactly what you mean."). A tap moves on,
   Skip ends it. The six flat beats, their captions and the ladder went; the full story lines
   are in the file.
7. **Errand 1.6 (`Ui/DecideScreen.cs`)** is the kitchen table: the five messages as notes in a
   pile, the top one readable on a dark plate; drag it to the agent, the box or the bin
   ("Agent", "Automation", "Neither"); a placed note comes back with a tap. "Who does each task?",
   LUMA's "Mark each task: an agent, a simple automation, or neither." (recorded), "Run them".
   The hints that gave the answer away went to the file. Its debrief has the same shape as the
   others (`Ui/DecideDebrief.cs`).
8. **The title (`Ui/TitleScreen.cs`)**: the player's own agent in the evening square; tap it to
   paint it the next colour, tap its name tag (a dark glass plate at its feet) to rename it.
   The errand map stays (the only ladder); Start or Continue; the menu icon; an amber lamp by
   the menu only when Claude is not connected, which opens the reason. Sound, the voice, free
   play, the story and the games menu moved to the menu.
9. **One menu (`Ui/GameMenu.cs`)**, Esc or the icon, as Claude Bomb's: Resume, The briefing, The
   file, This errand again, The story, Free play, Sound, LUMA's voice, Speed, Turns, The class
   (the roster, `Ui/SettingsSheet.cs`), Title screen, Games menu. The file (`Ui/FileSheet.cs`) is
   dark now and holds the message and its catch, the ready and broken lists with their reasons,
   LUMA's full lines, the takeaway, the question, how to play, and the story.
10. **LUMA** says one short thing when something happens (`Ui/Luma.cs`): the cues are sentences
    of her own lines, recorded with `say` like the rest (`LumaVoiceTools.GenerateBatch`), shown
    whole and never typed mid-sentence. The tour overlay went (its four lines are the garage's
    cues on the first visit: the phone, the note, a coin, the send).

## Words and controls, before and after

Counted the same way as Robot Racing's Phase 7: the "before" from the design doc's inventory (7
November 2026, the web build), the "after" from the final walk's `counts.jsonl`
(`games/ai-arena/Builds/look-audit/game-feel/final/`, on the stand-in brain, twelve turns an
errand). A word is a whitespace-separated token with a letter or a digit; left out, as the brief
asks, are the player's own words (the note on the agent, a field) and Claude's (the thought bubble,
the whole thought), which the walk counts apart. A control is anything that can be pressed or typed
in; a tap on a thing that only shows state (a face at the window, the bubble) is not counted, nor
is a tap anywhere to move the story on. The menu icon counts as a control on every screen.

| Screen | Before: words / controls | After: words / controls | Limit |
|---|---|---|---|
| Title | 59 / 12 | 29 / 11 | – |
| The story (six beats, now four shots) | 44 to 186 / 2 | 7 to 11 / 1 | – |
| Briefing 1.1 | 91 / 9 | 39 / 3 | 50 |
| Garage 1.1 (the list apart) | 144 / 29 | 9 / 2 | 30, 3 + main |
| Garage 1.1, the phone open | – | 26 / 3 | 30 |
| The round 1.1: agents out, the street, the agent's eye, the market, the end | 79 to 98 / 9 | 14 to 27 / 2 to 3 | 30, 3 + main |
| Debrief 1.1 | 115 / 6 | 14 / 4 | 30 |
| Debrief 1.1, Details open | – | 133 / 6 | – |
| Briefing 1.2 | – | 48 / 3 | 50 |
| Garage 1.2 (broken, then fixed) | – | 17 to 25 / 2 | 30, 3 + main |
| The round 1.2 (a moment: sent home) | – | 8 to 17 / 2 to 3 | 30, 3 + main |
| Debrief 1.2 | – | 10 / 4 | 30 |
| Briefing 1.3, garage 1.3 | – | 50 / 3 · 22 / 2 | 50 · 30 |
| Briefing 1.4, garage 1.4 | – | 47 / 3 · 16 / 2 | 50 · 30 |
| Briefing 1.5 | 94 / 9 | 45 / 3 | 50 |
| Garage 1.5 (broken, then fixed) | 147 / 33 | 23 / 2 | 30, 3 + main |
| The round 1.5 | 101 / 9 | 9 to 23 / 2 to 3 | 30, 3 + main |
| Debrief 1.5 | 126 / 6 | 22 / 4 | 30 |
| Debrief 1.5, Details open | – | 171 / 6 | – |
| Briefing 1.6 | – | 40 / 3 | 50 |
| Decide 1.6 (the pile, then all placed) | 184 / 19 | 9 to 26 / 1 to 2 | 30, 3 + main |
| A task of 1.6 (the rule against the agent) | – | 9 / 3 | 30, 3 + main |
| Debrief 1.6 | – | 12 / 4 | 30 |
| Briefing 1.7, garage 1.7 | – | 46 / 3 · 23 to 25 / 2 | 50 · 30 |
| The menu | – | 67 / 28 | – |

Flags: none on any screen
LUMA lines cut or over 15 words: none
LUMA lines counted: 32

What the machine checks say, on every screen of the final walk:

- **Words:** every play screen within 30, every briefing within 50 (1.3's at 50 exactly), every
  debrief with Details closed within 30. The garage of 1.1 holds 9 words, its round 14 to 27.
- **Controls:** 3 or fewer on every play screen plus the main action (the menu, Camera, Call home;
  Send, Debrief, Run them). The title keeps its 11 (the seven errand stops, Start, the menu, the
  agent's name tag, the Claude lamp), the menu its 28.
- **Form controls on a play screen:** none. The one text field is the note, opened full size on a
  tap: the place where the player writes the agent's list, which the brief allows.
- **Flags:** no OPENS AT, no ladder, no coordinates (the bubble's are stripped; the walk checks
  Claude's words for them too), no seeds, no probabilities on any play or briefing screen.
- **LUMA:** 32 lines on screen across the walk, every one whole and 15 words or fewer.
- **The agent in the frame:** 15 to 16 % in the garage (the table takes the rest), 18 % on the
  street (the camera pulled back so the market shows ahead; the realism pass had it at 30 %).

The garage's before counted 144 words and 29 controls (the drawer: a name field, ten swatches, the
list, the build-from-parts mode, ten coins, four tags); after, 9 words and 2 controls plus Send,
the decisions made on the table. The 1.6 decide screen went from 184 words and 19 controls to 9 to
26 and 1 to 2.

## The reviewers, round 1

Two fresh agents, given only the pictures of the first walk (`games/ai-arena/Builds/look-audit/
game-feel/run1/`, before the fixes below), as the brief asks.

**The player test** (1.1's fourteen frames, title to debrief): it said what the game is about
("You are the person at home, not the robot. You write a shopping list for an autonomous agent,
hand it a basket, and send it into a market where you can only watch through cameras and, at
most, switch it off. The lesson is that the list is all the control you get") and what to do on
every screen but the fourth story shot (no caption in that frame: the walk shot it before the
line). Form or instructions by its account: the briefing ("a text message with a button"), the
phone ("the same text again"), the title ("a menu"); and it was "told apples, nothing else five
times in three wordings" (the story's note, the message, the objective, the phone, the note).
Play: the round's five frames ("a pinned objective, a clock, camera modes with REC and LIVE
overlays, a map view, a Call home button"), and the debrief ("a proper game beat"). Liked most:
the look, the home-cam and agent-cam framing, the robot thinking in a bubble, the podium. Liked
least: the repetition, the stand-in agent "wandering" ("reads like a log line"), three stars with
no shown reason. Broken, as it saw it: the title lit errand 7 (the machine's saved progress);
a pink blob beside the robot's head on every dusk frame (the memory's glass cards); no caption
on shot 4; the robot's note hidden behind the basket; "AGENTS OUT" for one agent; the bubble
still up at the end; the face icon "turns red" (its colour); the podium's third step hidden
and the robot beside it. Its summary: "across fourteen screens I made no decision that changed
anything": on 1.1 the ready list is packed already, which is the errand's design ("nothing to
fix: watch the loop"), and the walk never dragged anything.

**The Claude Bomb test** (eight side-by-sides): RIGHT on the briefing ("that is a game briefing")
and on the round's look ("a third-person follow camera behind a robot carrying a basket, a
clock on the HUD, stalls ahead: that is a game camera"), the debrief by default; LEFT on the
title ("the stepper is an onboarding wizard"), the 1.1 garage ("a large empty brown slab with
three tiny vegetables... a sorting activity, not yet a scene"), the agent's eye (a lamp post in
the frame), 1.5's garage ("every meaning is also spelled out on a black plaque: a settings panel
rebuilt in 3D"), and the decide table ("a classification worksheet", "near-black and unlit"). The
tells, most visible first: the title's stepper and tagline; the flat teal buttons in the corners;
the word-signs on the table; LUMA's bar; rules restated on the briefing; the thought as a debug
log; the menu icon and the status dots; the "Agent" tag; the debrief as text plus buttons. Its
three changes: start in the world and stay there (no title panel, the errands as doors on the
street); replace every plaque and corner button with a prop and an action; make the round end
on a moment on the robot's body (home, the door opens, the basket tipped out, the robot on the
podium step). Broken: the glowing phone ("a lighting artefact"), the note cut behind the
basket and the Sight plaque, OPUS and Effort colliding at the antenna, the lamp post, the red
rectangle on the cobbles (the agent's home stone), the decide table's grid.

**Acted on before round 2:** the walk starts from a fresh machine (errand 1 lit, "Start") and
puts the progress back; the memory cards show only on levels about memory; the agent stands
beside the table's near end, whole, the table two metres and the produce bigger; the crate's
walls lower so what is in it shows; the phone's glow at a third; the tray plates lie low behind
their trays and the bulb's name moved off the Effort plate; the stand-in brain speaks plainly
("I see an apple. Going for it.", "Nothing on my list in view. Walking on.") and no longer walks
through forbidden stalls on its way, and reads "except" as a warning; "AGENT OUT" for one agent;
the bubble goes with the round; the face at home is framed teal; the agent stands on its podium
step turned so its basket shows, the medal rising behind it; the stalls in front of the door
cleared for the briefing, the story and the debrief; the story's note reads "Get fruit." so the
tomato lands; the fourth shot taken while it happens. Kept, as the brief keeps them: the errand
map on the title, the three small buttons plus one main action, LUMA's one line, and the name
plates on the things the player sets (Robot Racing's rule).

## The reviewers, round 2

The same two tests, fresh agents, on the third walk's frames (`run3/`, after the fixes above).

**The player test** (1.1's fourteen frames): the game came out right again ("You run a home that
sends a robot shopping agent to market... You pack the robot with a written list and supplies,
then it goes out alone while you can only watch and pull the plug. The lesson is clearly about AI
agents: they act on their instructions alone"). It knew what to do on thirteen of fourteen screens;
the fourth story shot still had no line (its caption had faded before the walk's shot). Liked
most: "the off switch as your one control", the agent cam's "I see an apple" and "Nothing on my
list in view", the town and the robot. Form or instructions: the briefing ("a web-page layout"),
the title ("landing page"), the phone ("read this closely"), the table ("a form dressed up in 3D:
every object has a printed label... Send works like Submit"), the story ("rules as a slideshow").
Play: agent out, the street, the agent cam, the overhead view ("the display looks like a real
game, the thought bubbles are charming, and 'you can only press the off switch' creates real
tension"). Least: the shopping "never appears on screen: no goods on the stalls, no apple I could
see", the round's end with no outcome, three stars not explained. Broken: the edge of the world
between houses, the light going night, day, night, Skip beside Mom's name, the feed's place name
over the agent's feet, "Batter" clipped, the message hiding the table, "I see an apple" with no
apple in view, the loop's icons unlabelled, the podium's empty steps, the medal "a torch or
lollipop".

**The Claude Bomb test** (the side-by-sides): RIGHT on seven of eight: the briefing ("the right
takes the left's security-feed look... gives you someone handing out the mission"), the setup
("a 3D packing scene... the goal in a quest bar"), the play ("a third-person camera behind the
robot walking a market street with its basket, and the world fills the frame"), the agent's eye
("the most game-like frame in the set"), the debrief ("reads straight away as a results screen"),
1.5's setup, and 1.6 ("only because it's 3D; the most lesson-like frame"). LEFT on the title only
("laid out like a landing page... seven numbered boxes"). In round 1 it preferred the right on two
or three. What still reads as an app, most visible first: the word plates on the table (Battery,
Coins, No, Effort, OPUS, Sight, Memory, Brain, Agent, Rule, Neither); 1.6 as a sorting quiz; the
navy panels of the title and the briefing; the flat buttons; the stepper; LUMA's takeaways; the
bubbles "like tooltips"; OPUS, Effort, Memory as settings words. Its three changes: physical parts
that snap in with no plates; sellers, shoppers and goods in the market; the title and the briefing
over the world. Broken: "Batter", the No plate behind the produce, the note cut, "I see an apple"
with no apple, the "?" markers (the agent's-eye view of stalls it cannot see), the pink home stone
"like a debug overlay", the robot's colour under dusk and day, the medal's pole behind its head,
"simple automation" in LUMA's line and "Rule" on the box, the bin crowding "Run them".

**Found on the way: the stalls had no goods.** Both reviewers' "empty market" was a real bug from
the realism pass, in two parts, both in the kit's merge step (`MarketKit.Merge`): the combined
goods mesh was run through `MeshUtility.Optimize`, which on Unity 6000.3 empties it, and its index
format was switched from 32 to 16 bits after combining, which drops its triangles. So every
`goods_<item>_crate_0` and `_heap_0` mesh was saved with 0 vertices and 0 triangles (the kit's
preview rendered them from memory before saving, so the previews looked right). Optimize is gone,
the index format is chosen before combining, and **AI Arena > Rebuild market goods**
(`Editor/MarketKit.Goods.cs`) rebuilt the 15 goods prefabs and their 30 meshes from the same recipe
(1,040 to 30,630 triangles each); nothing else of the kit was rebuilt, and the food material the
rebuild rewrites was restored as it was. The stalls now show their crates heaped with the item.

**Acted on after round 2:** the round's result stays up until Debrief ("HOME" with "1 on the list in
the basket", "SENT HOME", "CALLED HOME"); the debrief tips the basket out on the podium step in
front of the agent, a red glow under anything that ruined dinner; the medal stands clear of the
agent's head; the feed's place name at the bottom right; the message card under the table; the No
plate over the produce; the battery plate beside the pack; LUMA's phone cue cut to two of her own
sentences ("Read it closely. The agent will never see it.") so the screen keeps within thirty
words; the story's last line stays for its shot; the box on 1.6 reads "Automation", as LUMA says.
Kept, and left to the owner: the name plates on the things the player sets (Robot Racing's rule,
which both reviewers would remove), the errand map and the navy panel on the title (the brief keeps
the ladder there), the briefing as a transmission panel, the dusk for the briefing and the debrief
and the day for the garage and the round (as designed; the player test found the change confusing),
the agent's home stone.

## Web

AI Arena: 35.8 MB transferred (was 32.8 after the realism pass: the recorded cues, the new code,
and the goods meshes that were empty before), 3.9 s until the loader hides, no console errors;
measured with a headless Chrome on software rendering against the platform's local server run from
this worktree (the same method as the realism pass). Under the 45 MB cap. The build is
`site/games/ai-arena/` in this worktree (not committed: `site/games/*/` is ignored).

## Where the pictures are

All in `games/ai-arena/Builds/look-audit/game-feel/` of this worktree (ignored by git, like Robot
Racing's): `final/` the last walk, 51 frames with `counts.jsonl`, and `final/side-by-sides/` (Claude
Bomb on the left); `run1/` and `run3/` the frames the two rounds of reviewers saw, with their
side-by-sides; `goods-check/` errands 1.1 and 1.2 just after the goods fix. The final frames came
after the round-2 fixes and the goods fix and have not been shown to a reviewer.

## Waiting for Charbel

- **A clock tower model.** The market clock is built in code (a post by the fountain's corner
  with a dark face, lit hands, a lamp). A modelled tower would sit better among the kit's houses.
- **The name plates.** Both reviewers, both rounds, read the dark plates on the table (No, Coins,
  Battery, Sight, Memory, Brain, Effort, OPUS) as a labelled diagram and would remove them; Robot
  Racing's rule, which you set, says every control the player sets has one. Kept; your call.
- **The title and the briefing as panels.** Both reviewers read the title (wordmark, tagline, the
  seven-stop errand map, Start) as a landing page and the briefing's right half as a text column.
  The brief keeps the ladder on the title and the briefing as a transmission; Robot Racing has the
  same shape. Kept.
- **Dusk and day.** The briefing and the debrief are at dusk (the family's house at dinner time),
  the garage and the round by day, as in the design; the player test found the change confusing.
  One light for the whole errand (day, with the house's lamp on at the end) is a small change if
  you want it.
- **The family at the window.** The design has Mom at the door on the debrief; there is no family
  model, so the house's door is lit (a warm lamp) and nobody stands in it. A Higgsfield figure at
  the door, or a lit window texture for the kit's houses, would finish the shot.

## Where AI Arena stands

All seven errands run on the new screens and meet the brief's machine checks on every screen of
the final walk (the table above). The player test, both rounds, says what the game is about and
what to do on every screen but one story shot; the Claude Bomb test preferred the new screens on
two or three of eight in round 1 and seven of eight in round 2. What both still hold against it
is mostly what the brief and Robot Racing's rules keep (the name plates, the errand map on the
title, the transmission panel, the buttons for the main actions, LUMA's one line), and what the
game is ("nobody steers": on 1.1 the list is packed already and the player watches). The empty
market both reviewers saw was a kit bug, now fixed.

Not checked by the walk, and worth a look by hand: a live run on Claude (the walk uses the stand-in
brain; the smoke test made one real Claude call through Claude Code and read its move); the class
mode with several agents (the window's faces, picking one); free play; dragging by hand (the walk
acts through the same calls the drags make); 1.3, 1.4 and 1.7 played through (their briefings and
garages are walked). The final frames came after the last reviewer round.

The look audit's Phase 7 section for AI Arena is not written: this report is it, for you to merge.
