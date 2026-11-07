# Robot Racing: "before" inventory of words and controls

Captured 7 Nov 2026 from the local web build (http://localhost:8085, headless Chrome at 1600 x 900, branch realism-pass, fresh profile on every run). Screenshots: `games/robot-racing/Builds/look-audit/game-feel/before/`. Raw frames: `/private/tmp/claude-501/-Users-cdaoud-Desktop-maaloomatiia-games/93c6a380-3a2f-41bd-ab0e-2242c69e5aa5/scratchpad/rr-runs/`.

**Word rule:** a whitespace-separated token with at least one letter or digit ("·", "/", "–" alone are not words; "1.1", "0", "+0", "-100", "0/5", "19:42:14" are). The logo wordmark (an image of the word "maaloomatiia") is counted once on the title. Each element's text is copied as it appears on screen; "·" stands for the dot separators. CUT = cut on screen (ellipsis, half hidden, or still typing). Words below a scroll fold are listed but counted apart.

**Speed note:** the default training speed is Max: a whole training finishes in one frame, and the race runs out under the countdown banners. So the training and race frames were also taken at 1x, 10x or 100x (set in the game's own Settings sheet); each screen says which.


---

## Level 1.1 "Who is driving?"


### 1.1-01-title: Title

Screenshot: `before/1.1-01-title.jpg` (from `rr-runs/1/02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Wordmark (logo image, top left) | maaloomatiia | 1 |
| 2 | Button | Games menu | 2 |
| 3 | Button (toggle) | Sound on | 2 |
| 4 | Camera-feed frame: top label (red dot) | LIVE CAM 3 / THE GRID | 5 |
| 5 | Camera-feed frame: running clock | 19:42:14 | 1 |
| 6 | Camera-feed frame: bottom caption | MAALOOMATIIA RACEWAY / EVENING PRACTICE | 4 |
| 7 | Eyebrow | ROBOT RACING · GAME 03 | 4 |
| 8 | Title | Nobody Drives | 2 |
| 9 | Tagline | You set the points. It teaches itself to race. | 9 |
| 10 | LUMA strip: name | LUMA | 1 |
| 11 | LUMA strip: line | I'm LUMA, your coach. Thirteen levels in three tiers. Press Start: the robot is on the grid. | 17 |
| 12 | Level map: tier 1 label | TIER 1 · HOW A MACHINE LEARNS FROM REWARD | 8 |
| 13 | Level map: tier 1 nodes (1.1 teal with halo = next) | 1.1 1.2 1.3 | 3 |
| 14 | Level map: tier 2 label | TIER 2 · DESIGN HOW IT LEARNS | 6 |
| 15 | Level map: tier 2 nodes | 2.1 2.2 2.3 2.4 2.5 2.6 | 6 |
| 16 | Level map: tier 3 label | TIER 3 · DOES IT REALLY WORK? | 6 |
| 17 | Level map: tier 3 nodes | 3.1 3.2 3.3 3.4 | 4 |
| 18 | Button (main) | Start | 1 |
| 19 | Button | Free play | 2 |

**Words: 84**.

**Controls (17):** Games menu; Sound on (toggle); 13 level-map nodes 1.1 to 3.4 (each starts its level; none locked); **Start (main)**; Free play.

**Notes:**
- State as numbers: "Thirteen levels in three tiers" (LUMA); 3 star dots under each node, all empty (fresh profile); the clock runs (19:42:14 here).
- Future shown: the whole campaign, 13 levels in 3 named tiers, all open to click.
- LUMA: "I'm LUMA, your coach. Thirteen levels in three tiers. Press Start: the robot is on the grid." 17 words, complete, stays.
- Right half: the car on the grid seen through a camera-feed frame (brackets, LIVE, CAM 3, clock, caption).


### 1.1-02-briefing: Briefing

Screenshot: `before/1.1-02-briefing.jpg` (from `rr-runs/2/03.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Panel eyebrow | LEVEL 1.1 OF 3.4 · THE BRIEFING | 6 |
| 2 | Panel title | Who is driving? | 3 |
| 3 | Kicker (teal) | Nobody. It senses, moves, gets points, and goes again. | 9 |
| 4 | LUMA strip: name | LUMA | 1 |
| 5 | LUMA strip: line | Hi, I'm LUMA. Nobody drives this robot. Press run and watch it crash, then crash later, then finish. | 18 |
| 6 | Track box: eyebrow | THE TRACK · ONE BEND | 4 |
| 7 | Track box: note | A straight, one right bend, the finish. | 7 |
| 8 | Track box: map (tiles only) | (no words) | 0 |
| 9 | Parts eyebrow | YOUR ROBOT, PART BY PART · THIS LEVEL OPENS | 8 |
| 10 | Part chip (lit teal: opened by this level) | The robot | 2 |
| 11 | Part chip (dim: not open) | What it senses | 3 |
| 12 | Part chip (dim) | What it may do | 4 |
| 13 | Part chip (dim) | The points | 2 |
| 14 | Part chip (dim) | New moves | 2 |
| 15 | Part chip (dim) | The knobs | 2 |
| 16 | Part chip (dim) | Tracks and measure | 3 |
| 17 | Button (main) | Go to the pit lane | 5 |
| 18 | Button | The file | 2 |
| 19 | Button | Title screen | 2 |
| 20 | Ladder strip: eyebrow | THE LADDER | 2 |
| 21 | Ladder strip: steps (1.1 teal = current, rest grey) | 1.1 1.2 1.3 2.1 2.2 2.3 2.4 2.5 2.6 3.1 3.2 3.3 3.4 | 13 |
| 22 | Emblem, bottom right (logo mark) | (no words) | 0 |

**Words: 98**.

**Controls (15):** **Go to the pit lane (main)**; The file; Title screen; 12 ladder steps 1.2 to 3.4 (each jumps to that level; the current 1.1 is not a button).

**Notes:**
- State: "LEVEL 1.1 OF 3.4"; the ladder marks 1.1 as current.
- Future shown: the 13-step ladder; 6 of the 7 part chips dim (not open yet). The chips are labels, not buttons.
- LUMA: "Hi, I'm LUMA. Nobody drives this robot. Press run and watch it crash, then crash later, then finish." 18 words, complete, stays. It says "Press run"; the pit lane's button says "TRAIN IT".


### 1.1-03-the-file: The file (opened from the briefing; extra)

Screenshot: `before/1.1-03-the-file.jpg` (from `rr-runs/13/f01.jpg + f04.jpg (scrolled)`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Sheet eyebrow | THE FILE | 2 |
| 2 | Sheet title | Who is driving? | 3 |
| 3 | Button | Close | 1 |
| 4 | Section head | LEVEL 1.1 · WHO IS DRIVING? | 5 |
| 5 | Section body | Nobody. It senses, moves, gets points, and goes again. | 9 |
| 6 | Section head | THE IDEA | 2 |
| 7 | Section body, paragraph 1 | Nobody drives. Every moment the robot senses, chooses a move, makes it, and gets points. It starts knowing nothing and moves at random; the moves that earned points it makes more often. That is all. | 35 |
| 8 | Section body, paragraph 2 | The five parts: the robot, the track, what it senses, what it does, the points. Watch them on the run. | 20 |
| 9 | Section head | THE TRACK | 2 |
| 10 | Section body | One bend: A straight, one right bend, the finish. | 9 |
| 11 | Section head | WATCH THE PRO | 3 |
| 12 | Section body | A ready robot on a short track. The finish pays, a crash costs, every cell closer to the finish pays a little. Watch attempt 1, then attempt 100. | 28 |
| 13 | Section head | FIX THE BROKEN BOT · NO POINTS AT ALL | 8 |
| 14 | Section body | Every move earns zero. Attempt 100 looks like attempt 1, because the points are the only thing it learns from. Turn the finish points on and run again. | 28 |
| 15 | Section head | THE TAKEAWAY | 2 |
| 16 | Section body | The robot learns from the points, and from nothing else. | 10 |
| 17 | Section head | ASK BACK AT WORK | 4 |
| 18 | Section body | What do we reward, and what do we hope for? | 10 |
| 19 | Section head | HOW THE PIT LANE WORKS | 5 |
| 20 | Section body, paragraph 1 | What it senses: the sensors, front, left, right, speed, bend ahead. A sensor that is off is a situation the robot cannot tell apart from another. What it may do: speed up, hold, slow down, turn left, turn right, and a top speed it must respect. The points table: the finish, a crash, each step toward the finish, each step away, each moment, a speed bonus. The robot learns from these and from nothing else. | 75 |
| 21 | Section body, paragraph 2 (below the fold) | New moves: the chance it tries something new, how that chance falls over the attempts, and the floor it never goes below. The knobs: how fast it rewrites its rule book, how many attempts it gets, and the seed (the same luck twice). The tracks: the ones it trains on, and a test track it never trains on. The measure: the number you judge it by. *(below the fold)* | 66 |
| 22 | Section body, paragraph 3 (below the fold) | A setting that is locked opens at a later level; until then every robot gets the same, so only this level's idea is in play. The attempt budget is the robot's time: it learns from the points, over the attempts, on the tracks, with these settings and nothing else. *(below the fold)* | 49 |

**Words: 261** visible without scrolling; **376** with the 115 below the fold.

**Controls (2):** **Close (main)**; the dimmed background (click off the sheet closes it); scroll (mouse wheel) for the last two paragraphs.

**Notes:**
- Not on the task list; included because the file is the game's text prop. Opened by "The file" on the briefing (also on the pit lane and the debrief).
- The briefing stays visible, dimmed, at the sheet's left and top edges (cut words such as "Who is drivin"): not counted.
- The only place the words "Watch the pro" and "Fix the broken bot" appear; the run has no screens by those names.
- Light sheet (white) over a dark game; 9 section heads in teal capitals.
- No LUMA strip.


### 1.1-04-pit-lane: Pit lane (setup)

Screenshot: `before/1.1-04-pit-lane.jpg` (from `rr-runs/3/01.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Car tag SENSES (grey, locked): head | SENSES · OPENS AT 2.1 | 4 |
| 2 | Car tag SENSES: value | Front, Left, Right, Speed | 4 |
| 3 | Car tag MOVES (locked): head | MOVES · OPENS AT 2.2 | 4 |
| 4 | Car tag MOVES: value | 5 of 5 · top speed 2 | 6 |
| 5 | Car tag TRACKS (locked): head | TRACKS · OPENS AT 3.1 | 4 |
| 6 | Car tag TRACKS: value | One bend | 2 |
| 7 | Car tag MEASURE (locked): head | MEASURE · OPENS AT 3.2 | 4 |
| 8 | Car tag MEASURE: value | Finish rate | 2 |
| 9 | Car tag POINTS (locked): head | POINTS · OPENS AT 2.3 | 4 |
| 10 | Car tag POINTS: value | finish 0 · crash 0 · toward 0 | 6 |
| 11 | Car tag KNOBS (locked): head | KNOBS · OPENS AT 3.3 | 4 |
| 12 | Car tag KNOBS: value | rewrite 0.30 · 120 attempts | 4 |
| 13 | Car tag NEW MOVES (locked): head | NEW MOVES · OPENS AT 2.5 | 5 |
| 14 | Car tag NEW MOVES: value | 0.9, falls fast | 3 |
| 15 | Drawer eyebrow | LEVEL 1.1 · THE PIT LANE | 5 |
| 16 | Drawer button | Hide | 1 |
| 17 | Drawer title | Who is driving? | 3 |
| 18 | Drawer track eyebrow | THE TRACK · ONE BEND | 4 |
| 19 | Drawer track note | A straight, one right bend, the finish. | 7 |
| 20 | Drawer track map (tiles only) | (no words) | 0 |
| 21 | Drawer status line | 120 attempts · 5 laps on the test track | 8 |
| 22 | Drawer button | Briefing | 1 |
| 23 | Drawer button | The file | 2 |
| 24 | Drawer button | Settings | 1 |
| 25 | Drawer button (main) | TRAIN IT | 2 |
| 26 | LUMA strip over the car: name | LUMA | 1 |
| 27 | LUMA strip: line | Fix it: No points at all. Every move earns zero. Attempt 100 looks like attempt 1, because the points are the only thing it learns from. Turn the finish points on and run again. | 34 |

**Words: 125**.

**Controls (5):** Hide (folds the drawer); Briefing; The file; Settings; **TRAIN IT (main)**.

**Notes:**
- State as numbers: "120 attempts · 5 laps on the test track"; tag values "finish 0 · crash 0 · toward 0", "rewrite 0.30 · 120 attempts", "5 of 5 · top speed 2", "0.9, falls fast".
- Future shown: all 7 car tags are greyed and say OPENS AT (2.1, 2.2, 3.1, 3.2, 2.3, 3.3, 2.5). Level 1.1 opens nothing to set: the drawer stops under its buttons.
- LUMA: "Fix it: No points at all. Every move earns zero. Attempt 100 looks like attempt 1, because the points are the only thing it learns from. Turn the finish points on and run again." 34 words, complete. It fades about 2.5 s after it finishes typing; the pit lane then shows 90 words.
- LUMA says to turn the finish points on, but POINTS is locked here (OPENS AT 2.3): the player cannot do it. (In the code, Lesson.Apply, which would reset locked settings, is never called, so the no-points robot is what trains.)
- The 7 tags are glass cards with leader lines to parts of the car; the sensor rays are drawn on the ground.


### 1.1-05-pit-settings: Settings sheet over the pit lane (opened by the player)

Screenshot: `before/1.1-05-pit-settings.jpg` (from `rr-runs/5/s01.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Sheet eyebrow | SETTINGS | 1 |
| 2 | Button (main, closes) | Done | 1 |
| 3 | Label | Training speed | 2 |
| 4 | Segmented option | 1x | 1 |
| 5 | Segmented option | 10x | 1 |
| 6 | Segmented option | 100x | 1 |
| 7 | Segmented option (selected = default) | Max | 1 |
| 8 | Label | Sound | 1 |
| 9 | Toggle button | On | 1 |

**Words: 10**; plus **90** on the pit lane behind, dimmed, without its LUMA strip (screen 1.1-04 minus 35 LUMA words) = **100** on screen.

**Controls (6):** **Done (main)**; 1x; 10x; 100x; Max; On (sound toggle); (the dimmed background closes the sheet).

**Notes:**
- Opened by the pit lane's Settings button; the run screen's Settings opens the same sheet.
- Default training speed is Max: 120 attempts are done in one frame, so the training phase shows only as the ATTEMPT 1 moment. To show training (1.1-07, 1.1-09) I chose 1x here.
- No LUMA strip.


### 1.1-06-run-attempt-1: Run: training starts (ATTEMPT 1 moment), default speed

Screenshot: `before/1.1-06-run-attempt-1.jpg` (from `rr-runs/6/a1.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 1.1 | 2 |
| 2 | HUD 1: big line | Attempt 1 of 120 | 4 |
| 3 | HUD 1: small line | No finish yet | 3 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 0 | 1 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | – | 0 |
| 10 | Moment banner (small, teal, center of the world) | ATTEMPT 1 | 2 |
| 11 | Moment banner: line under | it starts knowing nothing | 4 |
| 12 | LUMA strip: name | LUMA | 1 |
| 13 | LUMA strip: line (still typing; CUT) | Nobody is steering. The moves are random at first; the ones that **CUT** | 12 |
| 14 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 15 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 16 | Bottom bar: button | Pause | 1 |
| 17 | Bottom bar: button | Stop, race now | 3 |
| 18 | Bottom bar: button | Settings | 1 |
| 19 | Drawer eyebrow | LEVEL 1.1 · THE RUN | 4 |
| 20 | Drawer button | Hide | 1 |
| 21 | Drawer mission line | Training on one bend: 120 attempts, seed 7 | 8 |
| 22 | Drawer points line | This attempt 0 · best 0 · last move +0 | 8 |
| 23 | Drawer explore line | New move chance 0.90 | 4 |
| 24 | Drawer book count | 0 situations | 2 |
| 25 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 26 | Chart (empty) | (no words) | 0 |
| 27 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 28 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 29 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Speed · stopped | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 33 | Move eyebrow | THE MOVE | 2 |
| 34 | Move line 1 | Hold · +0 | 2 |
| 35 | Move line 2 | Speed 0 · 0 moves | 4 |
| 36 | Move line 3 | Driving. | 1 |
| 37 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 38 | Rule book (empty) | (no words) | 0 |

**Words: 103**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- State: "Attempt 1 of 120", "seed 7", "New move chance 0.90", "0 situations", REWARD/BEST/LAP.
- No main action: the robot drives itself; the only forward control is "Stop, race now".
- LUMA (live line, typing, CUT here): full lead "Nobody is steering. The moves are random at first; the ones that earned points come back more often." (18 words). At the default Max speed the training ends when this moment closes and the race line replaces it; in my default-speed runs it never finished typing.
- The moment banner shows for about 1.2 s.


### 1.1-07-run-training: Run: training (at 1x, attempt 13)

Screenshot: `before/1.1-07-run-training.jpg` (from `rr-runs/5/r04.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 1.1 | 2 |
| 2 | HUD 1: big line | Attempt 13 of 120 | 4 |
| 3 | HUD 1: small line | No finish yet | 3 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 0 | 1 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | – | 0 |
| 10 | LUMA strip: name | LUMA | 1 |
| 11 | LUMA strip: line | Nobody is steering. The moves are random at first; the ones that earned points come back more often. | 18 |
| 12 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 13 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 14 | Bottom bar: button | Pause | 1 |
| 15 | Bottom bar: button | Stop, race now | 3 |
| 16 | Bottom bar: button | Settings | 1 |
| 17 | Drawer eyebrow | LEVEL 1.1 · THE RUN | 4 |
| 18 | Drawer button | Hide | 1 |
| 19 | Drawer mission line | Training on one bend: 120 attempts, seed 7 | 8 |
| 20 | Drawer points line | This attempt 0 · best 0 · last move +0 | 8 |
| 21 | Drawer explore line | New move chance 0.75 | 4 |
| 22 | Drawer book count | 9 situations | 2 |
| 23 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 24 | Chart axis, top | 10 | 1 |
| 25 | Chart axis, bottom | 0 | 1 |
| 26 | Chart x label | 120 attempts | 2 |
| 27 | Chart legend | Run 1 | 2 |
| 28 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 29 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Speed · slow | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 34 | Move eyebrow | THE MOVE | 2 |
| 35 | Move line 1 | Speed up · +0 | 3 |
| 36 | Move line 2 | Speed 1 · 1 moves | 4 |
| 37 | Move line 3 | Driving. | 1 |
| 38 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 39 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed slow · Speed up | 10 |
| 40 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Speed up | 10 |
| 41 | Drawer: rule book row 3 | front wall, left wall, right clear, speed stopped · Speed up | 10 |
| 42 | Drawer: rule book row 4 | front wall, left wall, right clear, speed fast · Speed up | 10 |
| 43 | Drawer: rule book row 5 | front wall, left clear, right wall, speed stopped · Speed up | 10 |
| 44 | Drawer: rule book row 6 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 45 | Drawer: rule book row 7 | front 1 cell, left wall, right wall, speed fast · Speed up | 11 |
| 46 | Drawer: rule book row 8 | front 2 cells, left wall, right wall, speed fast · Speed up | 11 |
| 47 | Drawer: rule book row 9 | front clear, left wall, right wall, speed stopped · Speed up | 10 |

**Words: 203**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- Speed set to 1x in Settings (default Max never shows this state).
- State: "Attempt 13 of 120", "seed 7", "New move chance 0.75", "9 situations"; the curve flat at 0.
- No main action: the robot drives itself; the only forward control is "Stop, race now".
- LUMA: "Nobody is steering. The moves are random at first; the ones that earned points come back more often." 18 words, complete; stays until the race line replaces it.
- Every rule says "Speed up": every move earned 0. The rule book grows by one row per new situation; it scrolls, and its word count changes with every attempt.


### 1.1-08-race-countdown: Race: countdown ("2"), default speed

Screenshot: `before/1.1-08-race-countdown.jpg` (from `rr-runs/6/a2.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 1.1 | 2 |
| 2 | HUD 1: big line | Lap 1 of 5 | 4 |
| 3 | HUD 1: small line | Finished 0, crashed 0, wall touches 0.0 a lap, lap time 0 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0/0 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | – | 0 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 0 mv | 2 |
| 10 | Countdown banner (big, teal, frame flash) | 2 | 1 |
| 11 | LUMA strip: name | LUMA | 1 |
| 12 | LUMA strip: line | The race: 5 laps on one bend with the rule book as it is. No new moves, no learning. | 19 |
| 13 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 14 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 15 | Bottom bar: button | Pause | 1 |
| 16 | Bottom bar: button | Stop, race now | 3 |
| 17 | Bottom bar: button | Settings | 1 |
| 18 | Drawer eyebrow | LEVEL 1.1 · THE RUN | 4 |
| 19 | Drawer button | Hide | 1 |
| 20 | Drawer mission line | The race on one bend: 0/5 laps | 7 |
| 21 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 22 | Drawer book count | 16 situations | 2 |
| 23 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 24 | Chart (blank in this frame) | (no words) | 0 |
| 25 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 26 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 27 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 28 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 29 | Drawer: sensor row (name, bar, reading) | Speed · stopped | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 31 | Move eyebrow | THE MOVE | 2 |
| 32 | Move line 1 | Speed 0 · 0 moves | 4 |
| 33 | Move line 2 | Driving. | 1 |
| 34 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 35 | Rule book (blank in this frame) | (no words) | 0 |

**Words: 110**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- The countdown is 4 big banners: "3", "2", "1", then "GO" with "5 laps, the rule book as it is" (see 2.3-09).
- HUD small line CUT with an ellipsis: "... lap time 0 m..." (the full text ends "lap time 0 moves").
- Chart and rule book are blank while the countdown pauses the run (they are not redrawn while paused), though the drawer says "16 situations".
- LUMA: "The race: 5 laps on one bend with the rule book as it is. No new moves, no learning." 19 words, complete.
- The race unpauses before the countdown banners finish, so at default speed the 5 laps are over while "1"/"GO" are still up.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 1.1-09-race: Race in progress (lap 3, at 1x)

Screenshot: `before/1.1-09-race.jpg` (from `rr-runs/5/q02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 1.1 | 2 |
| 2 | HUD 1: big line | Lap 3 of 5 | 4 |
| 3 | HUD 1: small line | Finished 0, crashed 2, wall touches 4.0 a lap, lap time 0 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0/2 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | – | 0 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 0 mv | 2 |
| 10 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 11 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 12 | Bottom bar: button | Pause | 1 |
| 13 | Bottom bar: button | Stop, race now | 3 |
| 14 | Bottom bar: button | Settings | 1 |
| 15 | Drawer eyebrow | LEVEL 1.1 · THE RUN | 4 |
| 16 | Drawer button | Hide | 1 |
| 17 | Drawer mission line | The race on one bend: 0/5 laps | 7 |
| 18 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 19 | Drawer book count | 9 situations | 2 |
| 20 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 21 | Chart axis, top | 10 | 1 |
| 22 | Chart axis, bottom | 0 | 1 |
| 23 | Chart x label | 120 attempts | 2 |
| 24 | Chart legend | Run 1 | 2 |
| 25 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 26 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 27 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 28 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 29 | Drawer: sensor row (name, bar, reading) | Speed · stopped | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 31 | Move eyebrow | THE MOVE | 2 |
| 32 | Move line 1 | Speed 0 · 0 moves | 4 |
| 33 | Move line 2 | Driving. | 1 |
| 34 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 35 | Drawer: rule book row 1 (highlighted: the current situation) | front 2 cells, left wall, right wall, speed fast · Speed up | 11 |
| 36 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Speed up | 10 |
| 37 | Drawer: rule book row 3 | front wall, left wall, right clear, speed stopped · Speed up | 10 |
| 38 | Drawer: rule book row 4 | front wall, left wall, right clear, speed fast · Speed up | 10 |
| 39 | Drawer: rule book row 5 | front wall, left clear, right wall, speed stopped · Speed up | 10 |
| 40 | Drawer: rule book row 6 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 41 | Drawer: rule book row 7 | front 1 cell, left wall, right wall, speed fast · Speed up | 11 |
| 42 | Drawer: rule book row 8 | front clear, left wall, right wall, speed stopped · Speed up | 10 |
| 43 | Drawer: rule book row 9 | front clear, left wall, right wall, speed slow · Speed up | 10 |

**Words: 188**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- Reached from 1x training with "Stop, race now" at attempt 13; the race kept 1x.
- State: HUD "Lap 3 of 5", REWARD "0/2" (finished/laps); the drawer still says "0/5 laps" (it only updates when the phase changes).
- HUD small line CUT ("lap time 0 m...").
- LUMA's race line has faded by this frame (no LUMA strip).
- At 1x each lap end shows a small banner, e.g. "LAP 4: CRASHED" (red, 2 words + number); at 10x and Max there are none.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 1.1-10-race-over-banner: Race over: BACK TO THE PIT LANE moment, default speed

Screenshot: `before/1.1-10-race-over-banner.jpg` (from `rr-runs/6/a4.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 1.1 | 2 |
| 2 | HUD 1: big line | Lap 5 of 5 | 4 |
| 3 | HUD 1: small line | Finished 0, crashed 5, wall touches 4.0 a lap, lap time 0 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0/5 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | – | 0 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 4 mv | 2 |
| 10 | Moment banner (big, amber; text wider than its card) | BACK TO THE PIT LANE | 5 |
| 11 | Moment banner: line under | most laps did not finish | 5 |
| 12 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 13 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 14 | Bottom bar: button | Pause | 1 |
| 15 | Bottom bar: button | Stop, race now | 3 |
| 16 | Bottom bar: button | Settings | 1 |
| 17 | Button (main, accent) | See the debrief | 3 |
| 18 | Drawer eyebrow | LEVEL 1.1 · THE RUN | 4 |
| 19 | Drawer button | Hide | 1 |
| 20 | Drawer mission line | Done. | 1 |
| 21 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 22 | Drawer book count | 16 situations | 2 |
| 23 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 24 | Chart axis, top | 10 | 1 |
| 25 | Chart axis, bottom | 0 | 1 |
| 26 | Chart x label | 120 attempts | 2 |
| 27 | Chart legend | Run 1 | 2 |
| 28 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 29 | Drawer: sensor row (name, bar, reading) | Front · wall | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Right · clear | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 34 | Move eyebrow | THE MOVE | 2 |
| 35 | Move line 1 | Speed 2 · 4 moves | 4 |
| 36 | Move line 2 | Crashed. | 1 |
| 37 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 38 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed slow · Speed up | 10 |
| 39 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Speed up | 10 |
| 40 | Drawer: rule book row 3 | front wall, left wall, right wall, speed slow · Speed up | 10 |
| 41 | Drawer: rule book row 4 | front wall, left wall, right clear, speed stopped · Speed up | 10 |
| 42 | Drawer: rule book row 5 | front wall, left wall, right clear, speed slow · Speed up | 10 |
| 43 | Drawer: rule book row 6 | front wall, left wall, right clear, speed fast · Speed up | 10 |
| 44 | Drawer: rule book row 7 | front wall, left 1 cell, right clear, speed stopped · Speed up | 11 |
| 45 | Drawer: rule book row 8 | front wall, left clear, right wall, speed stopped · Speed up | 10 |
| 46 | Drawer: rule book row 9 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 47 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 48 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed slow · Speed up | 11 |

**Words: 216**.

**Controls (7):** View toggle segment: Follow (selected); View toggle segment: The track; Pause (disabled, looks active); Stop, race now (disabled, looks active); Settings; Hide (drawer); **See the debrief (main)**.

**Notes:**
- State: "Lap 5 of 5", REWARD "0/5", "Done.", "16 situations".
- The banner text "BACK TO THE PIT LANE" is wider than its card: it runs over the card's left edge and keyline.
- The banner says "back to the pit lane" while the one lit button is "See the debrief".
- Rule book: 11 of 16 rows visible, 5 below the fold of its scroll list (not counted).
- HUD small line CUT ("lap time 0 m..."). No LUMA strip (the race line has faded).


### 1.1-11-run-done: Run done (after the banner), default speed

Screenshot: `before/1.1-11-run-done.jpg` (from `rr-runs/6/a6.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 1.1 | 2 |
| 2 | HUD 1: big line | Lap 5 of 5 | 4 |
| 3 | HUD 1: small line | Finished 0, crashed 5, wall touches 4.0 a lap, lap time 0 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0/5 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | – | 0 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 4 mv | 2 |
| 10 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 11 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 12 | Bottom bar: button | Pause | 1 |
| 13 | Bottom bar: button | Stop, race now | 3 |
| 14 | Bottom bar: button | Settings | 1 |
| 15 | Button (main, accent) | See the debrief | 3 |
| 16 | Drawer eyebrow | LEVEL 1.1 · THE RUN | 4 |
| 17 | Drawer button | Hide | 1 |
| 18 | Drawer mission line | Done. | 1 |
| 19 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 20 | Drawer book count | 16 situations | 2 |
| 21 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 22 | Chart axis, top | 10 | 1 |
| 23 | Chart axis, bottom | 0 | 1 |
| 24 | Chart x label | 120 attempts | 2 |
| 25 | Chart legend | Run 1 | 2 |
| 26 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 27 | Drawer: sensor row (name, bar, reading) | Front · wall | 2 |
| 28 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 29 | Drawer: sensor row (name, bar, reading) | Right · clear | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 32 | Move eyebrow | THE MOVE | 2 |
| 33 | Move line 1 | Speed 2 · 4 moves | 4 |
| 34 | Move line 2 | Crashed. | 1 |
| 35 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 36 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed slow · Speed up | 10 |
| 37 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Speed up | 10 |
| 38 | Drawer: rule book row 3 | front wall, left wall, right wall, speed slow · Speed up | 10 |
| 39 | Drawer: rule book row 4 | front wall, left wall, right clear, speed stopped · Speed up | 10 |
| 40 | Drawer: rule book row 5 | front wall, left wall, right clear, speed slow · Speed up | 10 |
| 41 | Drawer: rule book row 6 | front wall, left wall, right clear, speed fast · Speed up | 10 |
| 42 | Drawer: rule book row 7 | front wall, left 1 cell, right clear, speed stopped · Speed up | 11 |
| 43 | Drawer: rule book row 8 | front wall, left clear, right wall, speed stopped · Speed up | 10 |
| 44 | Drawer: rule book row 9 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 45 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 46 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed slow · Speed up | 11 |

**Words: 206**.

**Controls (7):** View toggle segment: Follow (selected); View toggle segment: The track; Pause (disabled, looks active); Stop, race now (disabled, looks active); Settings; Hide (drawer); **See the debrief (main)**.

**Notes:**
- Same as 1.1-10 without the banner: the run screen waits here for "See the debrief".
- Pause and "Stop, race now" are disabled but look the same as before.
- Rule book: 11 of 16 rows visible. HUD small line CUT. No LUMA strip.


### 1.1-12-debrief-questions: Debrief questions (top of the list)

Screenshot: `before/1.1-12-debrief-questions.jpg` (from `rr-runs/6/d02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Card title | Debrief · Level 1.1 · Before the checks | 6 |
| 2 | Question 1: prompt | The one making the decisions. | 5 |
| 3 | Question 1: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 4 | Question 2: prompt | The world it drives in. | 5 |
| 5 | Question 2: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 6 | Question 3: prompt | What it knows about this moment. | 6 |
| 7 | Question 3: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 8 | Question 4: prompt | Speed up, slow down, turn. | 5 |
| 9 | Question 4: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 10 | Question 5: prompt | A number that says how good or bad the move was. | 11 |
| 11 | Question 5: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 12 | Question 6: prompt | What does the robot know at the start? | 8 |
| 13 | Question 6: 3 choice chips in a row | Nothing · The shape of the track · Which moves are good | 10 |
| 14 | Question 7: prompt (last visible line above the fold) | Why did the no-points robot never improve? | 7 |
| 15 | Question 7: choice 1 (below the fold) | Every move earned zero, so there was nothing to learn from *(below the fold)* | 11 |
| 16 | Question 7: choice 2 (below the fold) | The track was too long *(below the fold)* | 5 |
| 17 | Question 7: choice 3 (below the fold) | It needed more sensors *(below the fold)* | 4 |
| 18 | Button (main) | To the checks | 3 |

**Words: 126** visible without scrolling; **146** with the 20 below the fold.

**Controls (32):** 31 choice chips (questions 1-5: 5 each; question 6: 3; question 7: 3, below the fold); **To the checks (main)**; scroll (the list).

**Notes:**
- 7 questions in a scrolling list; the 3 choices of question 7 are below the fold (in 1.1-13).
- Nothing forces an answer: "To the checks" works with none picked (unpicked counts as wrong).
- Left: the car in a soft window (no words). No LUMA strip.
- The first five questions are one item each of "the five parts", with the same 5 chips repeated 5 times (25 chips).


### 1.1-13-debrief-questions-scrolled: Debrief questions (scrolled to the end)

Screenshot: `before/1.1-13-debrief-questions-scrolled.jpg` (from `rr-runs/14/d01.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Card title | Debrief · Level 1.1 · Before the checks | 6 |
| 2 | Question 2: prompt (half hidden under the list's top edge; CUT) | The world it drives in. **CUT** | 5 |
| 3 | Question 2: 5 choice chips | The robot · The track · What it senses · What it does · The points | 12 |
| 4 | Question 3: prompt | What it knows about this moment. | 6 |
| 5 | Question 3: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 6 | Question 4: prompt | Speed up, slow down, turn. | 5 |
| 7 | Question 4: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 8 | Question 5: prompt | A number that says how good or bad the move was. | 11 |
| 9 | Question 5: 5 choice chips in a row | The robot · The track · What it senses · What it does · The points | 12 |
| 10 | Question 6: prompt | What does the robot know at the start? | 8 |
| 11 | Question 6: 3 choice chips in a row | Nothing · The shape of the track · Which moves are good | 10 |
| 12 | Question 7: prompt | Why did the no-points robot never improve? | 7 |
| 13 | Question 7: choice 1 (one per line) | Every move earned zero, so there was nothing to learn from | 11 |
| 14 | Question 7: choice 2 (one per line) | The track was too long | 5 |
| 15 | Question 7: choice 3 (one per line) | It needed more sensors | 4 |
| 16 | Button (main) | To the checks | 3 |

**Words: 129**.

**Controls (27):** 26 choice chips visible (questions 2-7); question 1's 5 chips are scrolled out; **To the checks (main)**; scroll.

**Notes:**
- Scrolled to the end: question 1 is gone above; question 2's prompt is half hidden (CUT).
- Total questions content (both frames): 146 words, 31 chips.


### 1.1-14-debrief: Debrief (checks, medal, next)

Screenshot: `before/1.1-14-debrief.jpg` (from `rr-runs/7/e03.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Page title | Debrief · Level 1.1 | 3 |
| 2 | Chart card: title | POINTS PER ATTEMPT · EVERY RUN OF THIS LEVEL | 8 |
| 3 | Chart: axis top | 10 | 1 |
| 4 | Chart: axis bottom | 0 | 1 |
| 5 | Chart: x label | 120 attempts | 2 |
| 6 | Chart: legend | Run 1 | 2 |
| 7 | Window caption over the 3D car | ONE BEND · THE TEST TRACK · 0 OF 5 LAPS FINISHED | 10 |
| 8 | Medal card: eyebrow | THE MEDAL | 2 |
| 9 | Medal card: medal | SILVER | 1 |
| 10 | Medal card: 3 star dots (2 lit) | (no words) | 0 |
| 11 | Medal card: line | 2 of 3 stars · 2 in all · Trainee | 8 |
| 12 | Checks card: eyebrow | THE CHECKS | 2 |
| 13 | Check row (teal lamp) | Named the five parts | 4 |
| 14 | Check row (teal lamp) | Knows nothing at the start | 5 |
| 15 | Check row (red lamp) | Found why it never improved | 5 |
| 16 | Check row (red lamp) | Trained it to a finish | 5 |
| 17 | Runs card: eyebrow | THE RUNS · FIRST FINISH, SETTLED, THE RACE | 7 |
| 18 | Runs card: line 1 | Run 1 · first finish never · settled never of 120 | 9 |
| 19 | Runs card: line 2 | race 0/5 · 1.0 crashes, 4.0 touches, 0 moves a lap | 10 |
| 20 | LUMA strip: name | LUMA | 1 |
| 21 | LUMA strip: line (the takeaway) | The robot learns from the points, and from nothing else. | 10 |
| 22 | Unlock card: eyebrow | UNLOCKED FOR LEVEL 1.2 | 4 |
| 23 | Unlock card: what opens | The attempts | 2 |
| 24 | Unlock card: line | 2 of 3 stars banked · 2 in all · Trainee | 9 |
| 25 | Button (main) | Next: 1.2 Is it learning, or just lucky? | 8 |
| 26 | Button | Train again | 2 |
| 27 | Button | The file | 2 |
| 28 | Button | Title screen | 2 |
| 29 | Button | Games menu | 2 |

**Words: 127**.

**Controls (5):** **Next: 1.2 Is it learning, or just lucky? (main)**; Train again; The file; Title screen; Games menu.

**Notes:**
- State as numbers: "0 OF 5 LAPS FINISHED", "2 of 3 stars · 2 in all · Trainee" (twice), "first finish never · settled never of 120", "race 0/5 · 1.0 crashes, 4.0 touches, 0 moves a lap". The curve is flat at 0.
- Medal SILVER from my answers: questions 1-6 picked, question 7 left blank (its check is red). "Trained it to a finish" stayed red: with the points locked, the robot never finished in any of my 1.1 runs.
- Future shown: "UNLOCKED FOR LEVEL 1.2 · The attempts" and the Next button with 1.2's title.
- LUMA: "The robot learns from the points, and from nothing else." 10 words, complete, stays.
- The medal pops in over the 3D window with the stars lighting one by one (frame taken after the reveal).


---

## Level 2.3 "What earns points?"


### 2.3-01-briefing: Briefing (reached from the title's level map, node 2.3)

Screenshot: `before/2.3-01-briefing.jpg` (from `rr-runs/8/b02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Panel eyebrow | LEVEL 2.3 OF 3.4 · THE BRIEFING | 6 |
| 2 | Panel title | What earns points? | 3 |
| 3 | Kicker (teal, 2 lines) | Points only at the end leave the robot in the dark. Small points on the way light the path. | 19 |
| 4 | LUMA strip: name | LUMA | 1 |
| 5 | LUMA strip: line | Now you write the points table. Points only at the finish leave the robot in the dark for hundreds of attempts. | 21 |
| 6 | Track box: eyebrow | THE TRACK · THE HAIRPIN | 4 |
| 7 | Track box: note | A long way round with a tight turn back. | 9 |
| 8 | Track box: map (tiles only) | (no words) | 0 |
| 9 | Parts eyebrow | YOUR ROBOT, PART BY PART · THIS LEVEL OPENS | 8 |
| 10 | Part chip (white: open) | The robot | 2 |
| 11 | Part chip (dim) | What it senses | 3 |
| 12 | Part chip (dim) | What it may do | 4 |
| 13 | Part chip (lit teal: opened by this level) | The points | 2 |
| 14 | Part chip (dim) | New moves | 2 |
| 15 | Part chip (dim) | The knobs | 2 |
| 16 | Part chip (dim) | Tracks and measure | 3 |
| 17 | Button (main) | Go to the pit lane | 5 |
| 18 | Button | The file | 2 |
| 19 | Button | Title screen | 2 |
| 20 | Ladder strip: eyebrow | THE LADDER | 2 |
| 21 | Ladder strip: steps (2.3 teal = current, rest grey) | 1.1 1.2 1.3 2.1 2.2 2.3 2.4 2.5 2.6 3.1 3.2 3.3 3.4 | 13 |
| 22 | Emblem, bottom right (logo mark) | (no words) | 0 |

**Words: 113**.

**Controls (15):** **Go to the pit lane (main)**; The file; Title screen; 12 ladder steps (all but 2.3).

**Notes:**
- Title screen first: the same as 1.1-01 (I clicked map node 2.3 instead of Start).
- State: "LEVEL 2.3 OF 3.4"; ladder 1.1-2.2 grey (not done: fresh profile, jumped in).
- Future shown: ladder; 5 part chips dim (not open), The points lit, The robot white (open).
- LUMA: "Now you write the points table. Points only at the finish leave the robot in the dark for hundreds of attempts." 21 words, complete, stays. It repeats the kicker's idea in other words.


### 2.3-02-pit-lane: Pit lane as the level opens it (the broken setup)

Screenshot: `before/2.3-02-pit-lane.jpg` (from `rr-runs/9/p02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Car tag SENSES (grey, locked): head | SENSES · OPENS AT 2.1 | 4 |
| 2 | Car tag SENSES: value | Front, Left, Right, Speed | 4 |
| 3 | Car tag MOVES (locked): head | MOVES · OPENS AT 2.2 | 4 |
| 4 | Car tag MOVES: value | 5 of 5 · top speed 2 | 6 |
| 5 | Car tag TRACKS (locked): head | TRACKS · OPENS AT 3.1 | 4 |
| 6 | Car tag TRACKS: value | The hairpin | 2 |
| 7 | Car tag MEASURE (locked): head | MEASURE · OPENS AT 3.2 | 4 |
| 8 | Car tag MEASURE: value | Finish rate | 2 |
| 9 | Car tag POINTS (teal, open): head | POINTS | 1 |
| 10 | Car tag POINTS: value | finish 100 · crash 0 · toward 0 | 6 |
| 11 | Car tag KNOBS (locked): head | KNOBS · OPENS AT 3.3 | 4 |
| 12 | Car tag KNOBS: value | rewrite 0.30 · 250 attempts | 4 |
| 13 | Car tag NEW MOVES (locked): head | NEW MOVES · OPENS AT 2.5 | 5 |
| 14 | Car tag NEW MOVES: value | 0.9, falls fast | 3 |
| 15 | Drawer eyebrow | LEVEL 2.3 · THE PIT LANE | 5 |
| 16 | Drawer button | Hide | 1 |
| 17 | Drawer title | What earns points? | 3 |
| 18 | Drawer track eyebrow | THE TRACK · THE HAIRPIN | 4 |
| 19 | Drawer track note | A long way round with a tight turn back. | 9 |
| 20 | Drawer track map (tiles only) | (no words) | 0 |
| 21 | Drawer status line | 250 attempts · 5 laps on the test track | 8 |
| 22 | Drawer button | Briefing | 1 |
| 23 | Drawer button | The file | 2 |
| 24 | Drawer button | Settings | 1 |
| 25 | Drawer button (main) | TRAIN IT | 2 |
| 26 | Points table: eyebrow | THE POINTS TABLE | 3 |
| 27 | Points table row: label | The finish | 2 |
| 28 | Points table row: 4 options | 0 · 50 · 100 · 200 | 4 |
| 29 | Points table row: label | A crash | 2 |
| 30 | Points table row: 4 options | 0 · -20 · -100 · -200 | 4 |
| 31 | Points table row: label | Each moment | 2 |
| 32 | Points table row: 4 options | 0 · -0.2 · -1 · -3 | 4 |
| 33 | Points table row: label | Step toward | 2 |
| 34 | Points table row: 4 options | 0 · 1 · 2 · 5 | 4 |
| 35 | Points table row: label | Step away | 2 |
| 36 | Points table row: 4 options | 0 · -1 · -2 · -5 | 4 |
| 37 | Points table row: label | Speed bonus | 2 |
| 38 | Points table row: 3 options | 0 · 1 · 3 | 3 |
| 39 | LUMA strip over the car: name | LUMA | 1 |
| 40 | LUMA strip: line | Fix it: Finish points only. 100 at the finish and nothing else. The curve stays flat far past the budget: in the dark, a random walk rarely reaches the end. Add the small on-the-way points and compare attempts to the first finish. | 42 |

**Words: 170**.

**Controls (28):** Hide; Briefing; The file; Settings; **TRAIN IT (main)**; Points table: 23 option buttons in 6 segmented rows (The finish 4, A crash 4, Each moment 4, Step toward 4, Step away 4, Speed bonus 3).

**Notes:**
- Selected as opened: finish 100, every other row 0.
- State as numbers: "250 attempts · 5 laps on the test track"; tag "finish 100 · crash 0 · toward 0"; the table's 23 numbers.
- Future shown: 6 of 7 car tags greyed with OPENS AT (2.1, 2.2, 3.1, 3.2, 3.3, 2.5); POINTS is teal (open).
- LUMA: "Fix it: Finish points only. 100 at the finish and nothing else. The curve stays flat far past the budget: in the dark, a random walk rarely reaches the end. Add the small on-the-way points and compare attempts to the first finish." 42 words, 3 lines, complete in this frame; it fades about 2.5 s after typing (about 8 s on screen in all).
- The POINTS tag shows only finish, crash and toward; each moment, step away and speed bonus are not on the car.


### 2.3-03-pit-lane-fixed: Pit lane after my fix (crash -100, step toward 2, step away -2)

Screenshot: `before/2.3-03-pit-lane-fixed.jpg` (from `rr-runs/9/p04.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Car tag SENSES (grey, locked): head | SENSES · OPENS AT 2.1 | 4 |
| 2 | Car tag SENSES: value | Front, Left, Right, Speed | 4 |
| 3 | Car tag MOVES (locked): head | MOVES · OPENS AT 2.2 | 4 |
| 4 | Car tag MOVES: value | 5 of 5 · top speed 2 | 6 |
| 5 | Car tag TRACKS (locked): head | TRACKS · OPENS AT 3.1 | 4 |
| 6 | Car tag TRACKS: value | The hairpin | 2 |
| 7 | Car tag MEASURE (locked): head | MEASURE · OPENS AT 3.2 | 4 |
| 8 | Car tag MEASURE: value | Finish rate | 2 |
| 9 | Car tag POINTS (teal, open): head | POINTS | 1 |
| 10 | Car tag POINTS: value (CUT with an ellipsis) | finish 100 · crash -100 · tow... **CUT** | 5 |
| 11 | Car tag KNOBS (locked): head | KNOBS · OPENS AT 3.3 | 4 |
| 12 | Car tag KNOBS: value | rewrite 0.30 · 250 attempts | 4 |
| 13 | Car tag NEW MOVES (locked): head | NEW MOVES · OPENS AT 2.5 | 5 |
| 14 | Car tag NEW MOVES: value | 0.9, falls fast | 3 |
| 15 | Drawer eyebrow | LEVEL 2.3 · THE PIT LANE | 5 |
| 16 | Drawer button | Hide | 1 |
| 17 | Drawer title | What earns points? | 3 |
| 18 | Drawer track eyebrow | THE TRACK · THE HAIRPIN | 4 |
| 19 | Drawer track note | A long way round with a tight turn back. | 9 |
| 20 | Drawer track map (tiles only) | (no words) | 0 |
| 21 | Drawer status line | 250 attempts · 5 laps on the test track | 8 |
| 22 | Drawer button | Briefing | 1 |
| 23 | Drawer button | The file | 2 |
| 24 | Drawer button | Settings | 1 |
| 25 | Drawer button (main) | TRAIN IT | 2 |
| 26 | Points table: eyebrow | THE POINTS TABLE | 3 |
| 27 | Points table row: label | The finish | 2 |
| 28 | Points table row: 4 options | 0 · 50 · 100 · 200 | 4 |
| 29 | Points table row: label | A crash | 2 |
| 30 | Points table row: 4 options | 0 · -20 · -100 · -200 | 4 |
| 31 | Points table row: label | Each moment | 2 |
| 32 | Points table row: 4 options | 0 · -0.2 · -1 · -3 | 4 |
| 33 | Points table row: label | Step toward | 2 |
| 34 | Points table row: 4 options | 0 · 1 · 2 · 5 | 4 |
| 35 | Points table row: label | Step away | 2 |
| 36 | Points table row: 4 options | 0 · -1 · -2 · -5 | 4 |
| 37 | Points table row: label | Speed bonus | 2 |
| 38 | Points table row: 3 options | 0 · 1 · 3 | 3 |

**Words: 126**.

**Controls (28):** Hide; Briefing; The file; Settings; **TRAIN IT (main)**; Points table: 23 option buttons in 6 segmented rows (The finish 4, A crash 4, Each moment 4, Step toward 4, Step away 4, Speed bonus 3).

**Notes:**
- The player's own setup: finish 100, crash -100, each moment 0, step toward 2, step away -2, speed bonus 0 (the "pro" values from the file).
- Once crash reads -100, the POINTS tag value is cut: "finish 100 · crash -100 · tow...".
- LUMA's strip has faded: no LUMA words.
- A changed setting makes the car "pop"; no words appear.


### 2.3-04-run-attempt-1: Run: training starts (ATTEMPT 1 moment)

Screenshot: `before/2.3-04-run-attempt-1.jpg` (from `rr-runs/9/r01.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Attempt 1 of 250 | 4 |
| 3 | HUD 1: small line | No finish yet | 3 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 0 | 1 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | – | 0 |
| 10 | Moment banner (small, teal) | ATTEMPT 1 | 2 |
| 11 | Moment banner: line under | it starts knowing nothing | 4 |
| 12 | LUMA strip: name | LUMA | 1 |
| 13 | LUMA strip: line (still typing; CUT) | Compare attempts to the first finish, table by table. The debrief puts every ta **CUT** | 14 |
| 14 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 15 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 16 | Bottom bar: button | Pause | 1 |
| 17 | Bottom bar: button | Stop, race now | 3 |
| 18 | Bottom bar: button | Settings | 1 |
| 19 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 20 | Drawer button | Hide | 1 |
| 21 | Drawer mission line | Training on the hairpin: 250 attempts, seed 7 | 8 |
| 22 | Drawer points line | This attempt 0 · best 0 · last move +0 | 8 |
| 23 | Drawer explore line | New move chance 0.90 | 4 |
| 24 | Drawer book count | 0 situations | 2 |
| 25 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 26 | Chart (empty) | (no words) | 0 |
| 27 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 28 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 29 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Speed · stopped | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 33 | Move eyebrow | THE MOVE | 2 |
| 34 | Move line 1 | Hold · +0 | 2 |
| 35 | Move line 2 | Speed 0 · 0 moves | 4 |
| 36 | Move line 3 | Driving. | 1 |
| 37 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 38 | Rule book (empty) | (no words) | 0 |

**Words: 105**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- State: "Attempt 1 of 250", "seed 7", "New move chance 0.90", "0 situations".
- LUMA (typing, CUT here): full line "Compare attempts to the first finish, table by table. The debrief puts every table you tried side by side." (19 words, see 2.3-05). At Max the first-finish line (said during the one-frame training) replaces it before it completes.
- The debrief in fact shows one curve per run of this level (only one run here); there is no table-by-table comparison of the points tables.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 2.3-05-run-training: Run: training (at 1x, attempt 8)

Screenshot: `before/2.3-05-run-training.jpg` (from `rr-runs/10/r02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Attempt 8 of 250 | 4 |
| 3 | HUD 1: small line | No finish yet | 3 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | -86 | 1 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | – | 0 |
| 10 | LUMA strip: name | LUMA | 1 |
| 11 | LUMA strip: line | Compare attempts to the first finish, table by table. The debrief puts every table you tried side by side. | 19 |
| 12 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 13 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 14 | Bottom bar: button | Pause | 1 |
| 15 | Bottom bar: button | Stop, race now | 3 |
| 16 | Bottom bar: button | Settings | 1 |
| 17 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 18 | Drawer button | Hide | 1 |
| 19 | Drawer mission line | Training on the hairpin: 250 attempts, seed 7 | 8 |
| 20 | Drawer points line | This attempt 0 · best -86 · last move +0 | 8 |
| 21 | Drawer explore line | New move chance 0.81 | 4 |
| 22 | Drawer book count | 8 situations | 2 |
| 23 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 24 | Chart axis, top | 0 | 1 |
| 25 | Chart axis, bottom | -100 | 1 |
| 26 | Chart x label | 250 attempts | 2 |
| 27 | Chart legend | Run 1 | 2 |
| 28 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 29 | Drawer: sensor row (name, bar, reading) | Front · wall | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Right · clear | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Speed · stopped | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 34 | Move eyebrow | THE MOVE | 2 |
| 35 | Move line 1 | Turn left · +0 | 3 |
| 36 | Move line 2 | Speed 0 · 1 moves | 4 |
| 37 | Move line 3 | Driving. | 1 |
| 38 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 39 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed stopped · Speed up | 10 |
| 40 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Hold | 9 |
| 41 | Drawer: rule book row 3 | front wall, left wall, right clear, speed stopped · Speed up | 10 |
| 42 | Drawer: rule book row 4 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 43 | Drawer: rule book row 5 | front wall, left clear, right wall, speed stopped · Hold | 9 |
| 44 | Drawer: rule book row 6 | front 1 cell, left wall, right wall, speed fast · Speed up | 11 |
| 45 | Drawer: rule book row 7 | front clear, left wall, right wall, speed slow · Slow down | 10 |
| 46 | Drawer: rule book row 8 | front clear, left wall, right wall, speed fast · Speed up | 10 |

**Words: 190**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- Speed set to 1x in Settings (pit lane), after my fix.
- State: "Attempt 8 of 250", BEST "-86", "New move chance 0.81", "8 situations"; the curve near -100.
- LUMA: "Compare attempts to the first finish, table by table. The debrief puts every table you tried side by side." 19 words, complete.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 2.3-06-run-first-finish: Run: FIRST FINISH moment (at 10x)

Screenshot: `before/2.3-06-run-first-finish.jpg` (from `rr-runs/11/f02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Attempt 26 of 250 | 4 |
| 3 | HUD 1: small line | First finish at 23 | 4 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | +6 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 120 | 1 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 6 mv | 2 |
| 10 | Moment banner (big, amber, frame flash) | FIRST FINISH | 2 |
| 11 | Moment banner: line under | attempt 23 | 2 |
| 12 | LUMA strip: name | LUMA | 1 |
| 13 | LUMA strip: line (still typing; CUT) | First finish at attempt 23. One fini **CUT** | 7 |
| 14 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 15 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 16 | Bottom bar: button | Pause | 1 |
| 17 | Bottom bar: button | Stop, race now | 3 |
| 18 | Bottom bar: button | Settings | 1 |
| 19 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 20 | Drawer button | Hide | 1 |
| 21 | Drawer mission line | Training on the hairpin: 250 attempts, seed 7 | 8 |
| 22 | Drawer points line | This attempt 6 · best 120 · last move +4 | 8 |
| 23 | Drawer explore line | New move chance 0.62 | 4 |
| 24 | Drawer book count | 12 situations | 2 |
| 25 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 26 | Chart axis, top | 120 | 1 |
| 27 | Chart marker label (amber) | first finish | 2 |
| 28 | Chart axis, bottom | -100 | 1 |
| 29 | Chart x label | 250 attempts | 2 |
| 30 | Chart legend | Run 1 | 2 |
| 31 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 32 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 34 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 35 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 36 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 37 | Move eyebrow | THE MOVE | 2 |
| 38 | Move line 1 | Speed up · +4 | 3 |
| 39 | Move line 2 | Speed 2 · 2 moves | 4 |
| 40 | Move line 3 | Driving. | 1 |
| 41 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 42 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed slow · Speed up | 10 |
| 43 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Turn right | 10 |
| 44 | Drawer: rule book row 3 | front wall, left wall, right 2 cells, speed fast · Turn right | 11 |
| 45 | Drawer: rule book row 4 | front wall, left wall, right clear, speed stopped · Turn right | 10 |
| 46 | Drawer: rule book row 5 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 47 | Drawer: rule book row 6 | front wall, left clear, right wall, speed stopped · Turn right | 10 |
| 48 | Drawer: rule book row 7 | front 1 cell, left wall, right wall, speed fast · Slow down | 11 |
| 49 | Drawer: rule book row 8 | front 2 cells, left wall, right wall, speed slow · Speed up | 11 |
| 50 | Drawer: rule book row 9 | front 2 cells, left wall, right wall, speed fast · Speed up | 11 |
| 51 | Drawer: rule book row 10 | front clear, left wall, right wall, speed stopped · Speed up | 10 |
| 52 | Drawer: rule book row 11 | front clear, left wall, right wall, speed fast · Hold | 9 |

**Words: 221**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- Speed 10x so the moment comes mid-training; at default Max it shows after the training has already ended (banners queue).
- State: "Attempt 26 of 250", "First finish at 23", REWARD "+6", BEST "120", LAP "6 mv", "New move chance 0.62", "12 situations".
- LUMA (typing, CUT here): full line "First finish at attempt 23. One finish proves nothing: watch whether the curve stays up." (15 words; seen complete in rr-runs/11/f03.jpg).
- Rule book: 11 of 12 rows visible.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 2.3-07-run-curve-settled: Run: THE CURVE SETTLED moment (at 100x)

Screenshot: `before/2.3-07-run-curve-settled.jpg` (from `rr-runs/9/r02.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Attempt 225 of 250 | 4 |
| 3 | HUD 1: small line | Settled at 131 | 3 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | +6 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 120 | 1 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 6 mv | 2 |
| 10 | Moment banner (big, teal, frame flash) | THE CURVE SETTLED | 3 |
| 11 | Moment banner: line under | attempt 131: most attempts finish now | 6 |
| 12 | LUMA strip: name | LUMA | 1 |
| 13 | LUMA strip: line | The curve settled at attempt 131: most attempts finish now. That is learning, not luck. | 15 |
| 14 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 15 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 16 | Bottom bar: button | Pause | 1 |
| 17 | Bottom bar: button | Stop, race now | 3 |
| 18 | Bottom bar: button | Settings | 1 |
| 19 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 20 | Drawer button | Hide | 1 |
| 21 | Drawer mission line | Training on the hairpin: 250 attempts, seed 7 | 8 |
| 22 | Drawer points line | This attempt 6 · best 120 · last move +4 | 8 |
| 23 | Drawer explore line | New move chance 0.05 | 4 |
| 24 | Drawer book count | 17 situations | 2 |
| 25 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 26 | Chart axis, top | 120 | 1 |
| 27 | Chart marker label (amber) | first finish | 2 |
| 28 | Chart marker label (teal, drawn over the curve) | settled | 1 |
| 29 | Chart axis, bottom | -100 | 1 |
| 30 | Chart x label | 250 attempts | 2 |
| 31 | Chart legend | Run 1 | 2 |
| 32 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 33 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 34 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 35 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 36 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 37 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 38 | Move eyebrow | THE MOVE | 2 |
| 39 | Move line 1 | Speed up · +4 | 3 |
| 40 | Move line 2 | Speed 2 · 2 moves | 4 |
| 41 | Move line 3 | Driving. | 1 |
| 42 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 43 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed fast · Slow down | 10 |
| 44 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Turn right | 10 |
| 45 | Drawer: rule book row 3 | front wall, left wall, right 2 cells, speed slow · Turn right | 11 |
| 46 | Drawer: rule book row 4 | front wall, left wall, right 2 cells, speed fast · Turn right | 11 |
| 47 | Drawer: rule book row 5 | front wall, left wall, right clear, speed stopped · Turn right | 10 |
| 48 | Drawer: rule book row 6 | front wall, left 1 cell, right clear, speed stopped · Hold | 10 |
| 49 | Drawer: rule book row 7 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 50 | Drawer: rule book row 8 | front wall, left clear, right wall, speed stopped · Turn left | 10 |
| 51 | Drawer: rule book row 9 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 52 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 53 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed slow · Hold | 10 |

**Words: 235**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- Speed 100x. State: "Attempt 225 of 250", "Settled at 131", "New move chance 0.05", "17 situations".
- The chart label "settled" is drawn over the curve and is partly hard to read.
- LUMA: "The curve settled at attempt 131: most attempts finish now. That is learning, not luck." 15 words, complete. The banner's line repeats LUMA's first half.
- Rule book: 11 of 17 rows visible.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 2.3-08-race-start: Race: on the grid (lap 1)

Screenshot: `before/2.3-08-race-start.jpg` (from `rr-runs/10/r04.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Lap 1 of 5 | 4 |
| 3 | HUD 1: small line | Finished 0, crashed 0, wall touches 0.0 a lap, lap time 0 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 0/0 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | – | 0 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 0 mv | 2 |
| 10 | LUMA strip: name | LUMA | 1 |
| 11 | LUMA strip: line | The race: 5 laps on the hairpin with the rule book as it is. | 14 |
| 12 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 13 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 14 | Bottom bar: button | Pause | 1 |
| 15 | Bottom bar: button | Stop, race now | 3 |
| 16 | Bottom bar: button | Settings | 1 |
| 17 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 18 | Drawer button | Hide | 1 |
| 19 | Drawer mission line | The race on the hairpin: 0/5 laps | 7 |
| 20 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 21 | Drawer book count | 17 situations | 2 |
| 22 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 23 | Chart axis, top | 120 | 1 |
| 24 | Chart marker label (amber) | first finish | 2 |
| 25 | Chart marker label (teal, over the curve) | settled | 1 |
| 26 | Chart axis, bottom | -100 | 1 |
| 27 | Chart x label | 250 attempts | 2 |
| 28 | Chart legend | Run 1 | 2 |
| 29 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 30 | Drawer: sensor row (name, bar, reading) | Front · clear | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Right · wall | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Speed · stopped | 2 |
| 34 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 35 | Move eyebrow | THE MOVE | 2 |
| 36 | Move line 1 | Speed 0 · 0 moves | 4 |
| 37 | Move line 2 | Driving. | 1 |
| 38 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 39 | Drawer: rule book row 1 (highlighted: the current situation) | front clear, left wall, right wall, speed stopped · Speed up | 10 |
| 40 | Drawer: rule book row 2 | front wall, left wall, right wall, speed stopped · Turn right | 10 |
| 41 | Drawer: rule book row 3 | front wall, left wall, right 2 cells, speed slow · Turn right | 11 |
| 42 | Drawer: rule book row 4 | front wall, left wall, right 2 cells, speed fast · Turn right | 11 |
| 43 | Drawer: rule book row 5 | front wall, left wall, right clear, speed stopped · Turn right | 10 |
| 44 | Drawer: rule book row 6 | front wall, left 1 cell, right clear, speed stopped · Hold | 10 |
| 45 | Drawer: rule book row 7 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 46 | Drawer: rule book row 8 | front wall, left clear, right wall, speed stopped · Turn left | 10 |
| 47 | Drawer: rule book row 9 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 48 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 49 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed slow · Hold | 10 |

**Words: 227**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- Training switched from 1x to Max mid-run (Settings on the run screen), so it finished at once; the race runs at 10x.
- State: "Lap 1 of 5", REWARD "0/0", "0/5 laps", "17 situations". HUD small line CUT ("lap time 0 m...").
- LUMA: "The race: 5 laps on the hairpin with the rule book as it is." 14 words, complete. (In 1.1 the same line carries "No new moves, no learning."; here the 60-character cut-off drops it.)
- Rule book: 11 of 17 rows visible.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 2.3-09-race-go: Race: GO moment (race already finished under it)

Screenshot: `before/2.3-09-race-go.jpg` (from `rr-runs/10/r06.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Lap 5 of 5 | 4 |
| 3 | HUD 1: small line | Finished 5, crashed 0, wall touches 8.0 a lap, lap time 6 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 5/5 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 6 mv | 2 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 6 mv | 2 |
| 10 | Countdown banner (big, teal, frame flash) | GO | 1 |
| 11 | Countdown banner: line under | 5 laps, the rule book as it is | 8 |
| 12 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 13 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 14 | Bottom bar: button | Pause | 1 |
| 15 | Bottom bar: button | Stop, race now | 3 |
| 16 | Bottom bar: button | Settings | 1 |
| 17 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 18 | Drawer button | Hide | 1 |
| 19 | Drawer mission line | The race on the hairpin: 0/5 laps | 7 |
| 20 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 21 | Drawer book count | 17 situations | 2 |
| 22 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 23 | Chart axis, top | 120 | 1 |
| 24 | Chart marker label (amber) | first finish | 2 |
| 25 | Chart marker label (teal, over the curve) | settled | 1 |
| 26 | Chart axis, bottom | -100 | 1 |
| 27 | Chart x label | 250 attempts | 2 |
| 28 | Chart legend | Run 1 | 2 |
| 29 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 30 | Drawer: sensor row (name, bar, reading) | Front · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Right · clear | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 34 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 35 | Move eyebrow | THE MOVE | 2 |
| 36 | Move line 1 | Speed 2 · 6 moves | 4 |
| 37 | Move line 2 | Finished. | 1 |
| 38 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 39 | Drawer: rule book row 1 | front wall, left wall, right wall, speed stopped · Turn right | 10 |
| 40 | Drawer: rule book row 2 | front wall, left wall, right 2 cells, speed slow · Turn right | 11 |
| 41 | Drawer: rule book row 3 | front wall, left wall, right 2 cells, speed fast · Turn right | 11 |
| 42 | Drawer: rule book row 4 | front wall, left wall, right clear, speed stopped · Turn right | 10 |
| 43 | Drawer: rule book row 5 | front wall, left 1 cell, right clear, speed stopped · Hold | 10 |
| 44 | Drawer: rule book row 6 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 45 | Drawer: rule book row 7 | front wall, left clear, right wall, speed stopped · Turn left | 10 |
| 46 | Drawer: rule book row 8 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 47 | Drawer: rule book row 9 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 48 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed slow · Hold | 10 |
| 49 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed fast · Slow down | 11 |

**Words: 224**.

**Controls (6):** View toggle segment: Follow (selected); View toggle segment: The track; Pause; Stop, race now; Settings; Hide (drawer).

**Notes:**
- "GO" is still on screen while the HUD already reads "Lap 5 of 5", REWARD "5/5" and the move "Finished.": at 10x the 5 laps run out under the countdown, so the player does not see the race being driven.
- Drawer still says "0/5 laps" (stale) against the HUD's 5/5. HUD small line CUT ("lap time 6 m...").
- No LUMA strip in this frame (the race line has faded).
- Rule book: 11 of 17 rows visible, no row highlighted.
- No main action: the robot drives itself; the only forward control is "Stop, race now".


### 2.3-10-race-over-banner: Race over: RACE OVER moment

Screenshot: `before/2.3-10-race-over-banner.jpg` (from `rr-runs/10/r07.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Lap 5 of 5 | 4 |
| 3 | HUD 1: small line | Finished 5, crashed 0, wall touches 8.0 a lap, lap time 6 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 5/5 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 6 mv | 2 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 6 mv | 2 |
| 10 | Moment banner (big, teal) | RACE OVER | 2 |
| 11 | Moment banner: line under | 5 of 5 laps finished | 5 |
| 12 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 13 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 14 | Bottom bar: button | Pause | 1 |
| 15 | Bottom bar: button | Stop, race now | 3 |
| 16 | Bottom bar: button | Settings | 1 |
| 17 | Button (main, accent) | See the debrief | 3 |
| 18 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 19 | Drawer button | Hide | 1 |
| 20 | Drawer mission line | Done. | 1 |
| 21 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 22 | Drawer book count | 17 situations | 2 |
| 23 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 24 | Chart axis, top | 120 | 1 |
| 25 | Chart marker label (amber) | first finish | 2 |
| 26 | Chart marker label (teal, over the curve) | settled | 1 |
| 27 | Chart axis, bottom | -100 | 1 |
| 28 | Chart x label | 250 attempts | 2 |
| 29 | Chart legend | Run 1 | 2 |
| 30 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 31 | Drawer: sensor row (name, bar, reading) | Front · wall | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Right · clear | 2 |
| 34 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 35 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 36 | Move eyebrow | THE MOVE | 2 |
| 37 | Move line 1 | Speed 2 · 6 moves | 4 |
| 38 | Move line 2 | Finished. | 1 |
| 39 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 40 | Drawer: rule book row 1 | front wall, left wall, right wall, speed stopped · Turn right | 10 |
| 41 | Drawer: rule book row 2 | front wall, left wall, right 2 cells, speed slow · Turn right | 11 |
| 42 | Drawer: rule book row 3 | front wall, left wall, right 2 cells, speed fast · Turn right | 11 |
| 43 | Drawer: rule book row 4 | front wall, left wall, right clear, speed stopped · Turn right | 10 |
| 44 | Drawer: rule book row 5 | front wall, left 1 cell, right clear, speed stopped · Hold | 10 |
| 45 | Drawer: rule book row 6 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 46 | Drawer: rule book row 7 | front wall, left clear, right wall, speed stopped · Turn left | 10 |
| 47 | Drawer: rule book row 8 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 48 | Drawer: rule book row 9 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 49 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed slow · Hold | 10 |
| 50 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed fast · Slow down | 11 |

**Words: 219**.

**Controls (7):** View toggle segment: Follow (selected); View toggle segment: The track; Pause (disabled, looks active); Stop, race now (disabled, looks active); Settings; Hide (drawer); **See the debrief (main)**.

**Notes:**
- State: "5 of 5 laps finished", REWARD "5/5", BEST/LAP "6 mv", "Done.".
- After the last lap the camera turns slowly round the car, then cuts to a high view of the whole track, though the view toggle still says Follow.
- Rule book: 11 of 17 rows visible. HUD small line CUT. No LUMA strip.


### 2.3-11-run-done: Run done (after the banner)

Screenshot: `before/2.3-11-run-done.jpg` (from `rr-runs/10/r10.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | HUD 1: level tag | LEVEL 2.3 | 2 |
| 2 | HUD 1: big line | Lap 5 of 5 | 4 |
| 3 | HUD 1: small line | Finished 5, crashed 0, wall touches 8.0 a lap, lap time 6 m... **CUT** | 13 |
| 4 | HUD 2: label | REWARD | 1 |
| 5 | HUD 2: value | 5/5 | 1 |
| 6 | HUD 2: label | BEST | 1 |
| 7 | HUD 2: value | 6 mv | 2 |
| 8 | HUD 2: label | LAP | 1 |
| 9 | HUD 2: value | 6 mv | 2 |
| 10 | Bottom bar: view toggle, segment 1 (selected) | Follow | 1 |
| 11 | Bottom bar: view toggle, segment 2 | The track | 2 |
| 12 | Bottom bar: button | Pause | 1 |
| 13 | Bottom bar: button | Stop, race now | 3 |
| 14 | Bottom bar: button | Settings | 1 |
| 15 | Button (main, accent) | See the debrief | 3 |
| 16 | Drawer eyebrow | LEVEL 2.3 · THE RUN | 4 |
| 17 | Drawer button | Hide | 1 |
| 18 | Drawer mission line | Done. | 1 |
| 19 | Drawer points line | The rule book as it is · unknown situations 0 | 9 |
| 20 | Drawer book count | 17 situations | 2 |
| 21 | Chart title | POINTS PER ATTEMPT · THE CURVE | 5 |
| 22 | Chart axis, top | 120 | 1 |
| 23 | Chart marker label (amber) | first finish | 2 |
| 24 | Chart marker label (teal, over the curve) | settled | 1 |
| 25 | Chart axis, bottom | -100 | 1 |
| 26 | Chart x label | 250 attempts | 2 |
| 27 | Chart legend | Run 1 | 2 |
| 28 | Sensors eyebrow | WHAT IT SENSES | 3 |
| 29 | Drawer: sensor row (name, bar, reading) | Front · wall | 2 |
| 30 | Drawer: sensor row (name, bar, reading) | Left · wall | 2 |
| 31 | Drawer: sensor row (name, bar, reading) | Right · clear | 2 |
| 32 | Drawer: sensor row (name, bar, reading) | Speed · fast | 2 |
| 33 | Drawer: sensor row (name, bar, reading) | Bend ahead · off | 3 |
| 34 | Move eyebrow | THE MOVE | 2 |
| 35 | Move line 1 | Speed 2 · 6 moves | 4 |
| 36 | Move line 2 | Finished. | 1 |
| 37 | Rule book eyebrow | THE RULE BOOK · THE MOVE THAT PAID | 7 |
| 38 | Drawer: rule book row 1 | front wall, left wall, right wall, speed stopped · Turn right | 10 |
| 39 | Drawer: rule book row 2 | front wall, left wall, right 2 cells, speed slow · Turn right | 11 |
| 40 | Drawer: rule book row 3 | front wall, left wall, right 2 cells, speed fast · Turn right | 11 |
| 41 | Drawer: rule book row 4 | front wall, left wall, right clear, speed stopped · Turn right | 10 |
| 42 | Drawer: rule book row 5 | front wall, left 1 cell, right clear, speed stopped · Hold | 10 |
| 43 | Drawer: rule book row 6 | front wall, left 2 cells, right clear, speed stopped · Hold | 10 |
| 44 | Drawer: rule book row 7 | front wall, left clear, right wall, speed stopped · Turn left | 10 |
| 45 | Drawer: rule book row 8 | front wall, left clear, right 1 cell, speed stopped · Speed up | 11 |
| 46 | Drawer: rule book row 9 | front 1 cell, left wall, right wall, speed stopped · Speed up | 11 |
| 47 | Drawer: rule book row 10 | front 1 cell, left wall, right wall, speed slow · Hold | 10 |
| 48 | Drawer: rule book row 11 | front 1 cell, left wall, right wall, speed fast · Slow down | 11 |

**Words: 212**.

**Controls (7):** View toggle segment: Follow (selected); View toggle segment: The track; Pause (disabled, looks active); Stop, race now (disabled, looks active); Settings; Hide (drawer); **See the debrief (main)**.

**Notes:**
- Same as 2.3-10 without the banner; waits for "See the debrief". Pause and "Stop, race now" disabled but look active.
- Level 2.3 has no debrief questions: "See the debrief" goes straight to the debrief.


### 2.3-12-debrief: Debrief (checks, medal, next)

Screenshot: `before/2.3-12-debrief.jpg` (from `rr-runs/10/e03.jpg`)

| # | Element | Text (verbatim) | Words |
|---|---|---|---|
| 1 | Page title | Debrief · Level 2.3 | 3 |
| 2 | Chart card: title | POINTS PER ATTEMPT · EVERY RUN OF THIS LEVEL | 8 |
| 3 | Chart: axis top | 120 | 1 |
| 4 | Chart: marker label (amber) | first finish | 2 |
| 5 | Chart: marker label (teal, over the curve) | settled | 1 |
| 6 | Chart: axis bottom | -100 | 1 |
| 7 | Chart: x label | 250 attempts | 2 |
| 8 | Chart: legend | Run 1 | 2 |
| 9 | Window caption over the 3D car | THE HAIRPIN · THE TEST TRACK · 5 OF 5 LAPS FINISHED | 10 |
| 10 | Medal card: eyebrow | THE MEDAL | 2 |
| 11 | Medal card: medal (gold) | GOLD | 1 |
| 12 | Medal card: 3 star dots (all lit) | (no words) | 0 |
| 13 | Medal card: line | 3 of 3 stars · 3 in all · Trainee | 8 |
| 14 | Checks card: eyebrow | THE CHECKS | 2 |
| 15 | Check row (teal lamp) | Set the three points | 4 |
| 16 | Check row (teal lamp) | Learned within the budget | 4 |
| 17 | Check row (teal lamp) | A crash costs more than a move | 7 |
| 18 | Runs card: eyebrow | THE RUNS · FIRST FINISH, SETTLED, THE RACE | 7 |
| 19 | Runs card: line 1 | Run 1 · first finish 23 · settled 131 of 250 | 9 |
| 20 | Runs card: line 2 | race 5/5 · 0.0 crashes, 8.0 touches, 6 moves a lap | 10 |
| 21 | LUMA strip: name | LUMA | 1 |
| 22 | LUMA strip: line (the takeaway) | Points only at the end leave the robot in the dark. Small points on the way light the path. | 19 |
| 23 | Unlock card: eyebrow | UNLOCKED FOR LEVEL 2.4 | 4 |
| 24 | Unlock card: what opens (the next level's title, 2 lines) | What does it learn when you reward the wrong thing? | 10 |
| 25 | Unlock card: line | 3 of 3 stars banked · 3 in all · Trainee | 9 |
| 26 | Button (main; label wraps to 2 lines) | Next: 2.4 What does it learn when you reward the wrong thing? | 12 |
| 27 | Button | Train again | 2 |
| 28 | Button | The file | 2 |
| 29 | Button | Title screen | 2 |
| 30 | Button | Games menu | 2 |

**Words: 147**.

**Controls (5):** **Next: 2.4 What does it learn when you reward the wrong thing? (main)**; Train again; The file; Title screen; Games menu.

**Notes:**
- No debrief questions in 2.3 (its quiz is empty), so no "Before the checks" screen.
- State as numbers: "5 OF 5 LAPS FINISHED", "3 of 3 stars · 3 in all · Trainee" (twice), "first finish 23 · settled 131 of 250", "race 5/5 · 0.0 crashes, 8.0 touches, 6 moves a lap".
- Medal GOLD (all 3 checks). Future shown: "UNLOCKED FOR LEVEL 2.4" names 2.4's title; the Next button repeats it, and its label wraps to a second line ("thing?").
- LUMA: "Points only at the end leave the robot in the dark. Small points on the way light the path." 19 words, complete; the same sentence as the briefing's kicker.
- The chart's "settled" label sits on the curve, partly hidden.


---

## Summary

| Screen | Words | Controls (main action) | Notes |
|---|---|---|---|
| 1.1-01-title | 84 | 17 (Start) | 13-level map in 3 tiers (all clickable, empty stars); LUMA 17 w complete; running clock in the camera frame. |
| 1.1-02-briefing | 98 | 15 (Go to the pit lane) | "LEVEL 1.1 OF 3.4"; 13-step ladder; 6 of 7 part chips dim; LUMA 18 w complete ("Press run" vs button "TRAIN IT"). |
| 1.1-03-the-file | 261 (376 with scroll) | 2 (Close) | Extra: the text prop; 9 sections; 115 words below the fold; only place "Watch the pro" / "Fix the broken bot" appear. |
| 1.1-04-pit-lane | 125 | 5 (TRAIN IT) | All 7 car tags locked with OPENS AT 2.1-3.3; settings as numbers; LUMA 34 w complete, then fades; asks to turn on points that are locked. |
| 1.1-05-pit-settings | 10 (+90 behind = 100) | 6 (Done) | Extra: Settings sheet (speed 1x/10x/100x/Max, default Max; sound); no LUMA. |
| 1.1-06-run-attempt-1 | 103 | 6 (no main action: the robot drives itself) | "Attempt 1 of 120", "seed 7", "0.90", "0 situations"; ATTEMPT 1 moment; LUMA CUT while typing (did not complete in my Max runs). |
| 1.1-07-run-training | 203 | 6 (no main action: the robot drives itself) | At 1x: "Attempt 13 of 120", 9 rule rows all "Speed up", flat curve; LUMA 18 w complete. |
| 1.1-08-race-countdown | 110 | 6 (no main action: the robot drives itself) | Big "2"; HUD line CUT; chart and rule book blank while paused; LUMA 19 w complete. |
| 1.1-09-race | 188 | 6 (no main action: the robot drives itself) | At 1x: "Lap 3 of 5", "0/2" vs drawer "0/5 laps" (stale); HUD line CUT; no LUMA. |
| 1.1-10-race-over-banner | 216 | 7 (See the debrief) | Amber "BACK TO THE PIT LANE" overflows its card; 11 of 16 rule rows visible; HUD CUT; no LUMA. |
| 1.1-11-run-done | 206 | 7 (See the debrief) | Waits for See the debrief; Pause/Stop disabled but look active; HUD CUT; no LUMA. |
| 1.1-12-debrief-questions | 126 (146 with scroll) | 32 (To the checks) | 7 questions, 31 chips (25 are the same 5 chips x5); Q7 choices below the fold; nothing required; no LUMA. |
| 1.1-13-debrief-questions-scrolled | 129 | 27 (To the checks) | End of list; Q2 prompt half hidden (CUT); all of Q7 visible. |
| 1.1-14-debrief | 127 | 5 (Next: 1.2 Is it learning, or just lucky?) | SILVER, "2 of 3 stars · 2 in all · Trainee" twice; 4 checks (2 red); UNLOCKED FOR 1.2 + Next; LUMA 10 w complete. |
| 2.3-01-briefing | 113 | 15 (Go to the pit lane) | "LEVEL 2.3 OF 3.4"; ladder; The points chip lit, 5 dim; LUMA 21 w complete. |
| 2.3-02-pit-lane | 170 | 28 (TRAIN IT) | Points table: 6 rows, 23 number buttons; 6 tags OPENS AT; LUMA 42 w (3 lines) complete, then fades. |
| 2.3-03-pit-lane-fixed | 126 | 28 (TRAIN IT) | After my fix: POINTS tag value CUT ("tow..."); no LUMA. |
| 2.3-04-run-attempt-1 | 105 | 6 (no main action: the robot drives itself) | "Attempt 1 of 250"; ATTEMPT 1 moment; LUMA CUT while typing. |
| 2.3-05-run-training | 190 | 6 (no main action: the robot drives itself) | At 1x: "Attempt 8 of 250", BEST -86, 8 rule rows; LUMA 19 w complete. |
| 2.3-06-run-first-finish | 221 | 6 (no main action: the robot drives itself) | At 10x: amber FIRST FINISH "attempt 23"; 11 of 12 rule rows; LUMA CUT while typing. |
| 2.3-07-run-curve-settled | 235 | 6 (no main action: the robot drives itself) | At 100x: THE CURVE SETTLED "attempt 131"; chart labels; 11 of 17 rows; LUMA 15 w complete. |
| 2.3-08-race-start | 227 | 6 (no main action: the robot drives itself) | "Lap 1 of 5"; HUD CUT; LUMA 14 w complete; 11 of 17 rows. |
| 2.3-09-race-go | 224 | 6 (no main action: the robot drives itself) | GO shown after the race already ended (5/5, Finished.); drawer "0/5 laps" stale; HUD CUT; no LUMA. |
| 2.3-10-race-over-banner | 219 | 7 (See the debrief) | RACE OVER "5 of 5 laps finished"; overhead view though Follow is selected; HUD CUT; no LUMA. |
| 2.3-11-run-done | 212 | 7 (See the debrief) | Waits for See the debrief; no debrief questions follow in 2.3. |
| 2.3-12-debrief | 147 | 5 (Next: 2.4 What does it learn when you reward the wrong thing?) | GOLD, 3 checks teal; UNLOCKED FOR 2.4 + Next (label wraps to 2 lines); LUMA 19 w = the briefing kicker. |

## Coverage and gaps

- **No "Watch the pro" or "Fix the broken bot" phases exist as screens.** Each level runs one setup: the level opens with its broken setup already in the pit lane (1.1 "No points at all", 2.3 "Finish points only"), the player trains it as is or changes it, then one race. The two names appear only in the file (1.1-03). A second robot runs only on levels whose broken setup has a second robot; 1.1 and 2.3 have none.
- **The training and the race are almost invisible at the default speed (Max).** Training ends in one frame. At Max, the race drops to 10x and its 5 laps finish under the countdown banners. Frames 1.1-07, 1.1-09, 2.3-05, 2.3-06 and 2.3-07 were taken at 1x, 10x or 100x, set in the game's own Settings sheet.
- **2.3 has no debrief questions** (its quiz is empty): See the debrief goes straight to the debrief.
- Moment banners seen but not kept as separate screens: "3" and "1" (as 1.1-08, one digit), "LAP 4: CRASHED" (small, red, only at 1x; rr-runs/5/q04.jpg), and the complete 2.3 first-finish LUMA line "First finish at attempt 23. One finish proves nothing: watch whether the curve stays up." (rr-runs/11/f03.jpg).
- Not captured: the folded drawers (Hide gives a slim tab with "Open"), Free play, Train again, the file from the pit lane or the debrief (the debrief adds the full checks and the runs' settings to it), and 2.3's broken setup trained unchanged.
- In 1.1 the robot never reached the finish in my runs (points locked at 0), so the check "Trained it to a finish" stayed red and gold was out of reach.

