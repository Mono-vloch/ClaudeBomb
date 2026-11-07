# Robot Racing: the game-feel design

Level 1.1 "Who is driving?" and level 2.3 "What earns points?". Shared rules (menu, LUMA,
briefing, debrief, measuring): [README](README.md). Every word of every screen today, verbatim:
[inventory/robot-racing.md](inventory/robot-racing.md); screenshots in
`games/robot-racing/Builds/look-audit/game-feel/before/`.

## The game in one paragraph

Today the player reads a drawer beside the car: seven grey tags that say "OPENS AT", a 6 by 4
grid of point buttons, then a run with a chart, a rule book of machine strings and sensor bars.
At the default speed the whole training ends in one frame, so the robot is never seen learning.
After: the player **puts rewards on the track** (a chequered flag on the finish, cones against
the walls, arrows along the way, each with a number on it) and **watches the robot learn** in a
training run paced to be watched: attempt 1 crashes, the ghost lines of earlier attempts pile
up on the asphalt, the fuel gauge of the attempt budget drains, and one attempt finally crosses
the line. Then it races.

## Two problems found while counting

1. **Level 1.1 cannot be won.** It opens with its broken setup, "No points at all", and LUMA
   says "Turn the finish points on and run again", but the points open at 2.3: the player cannot
   do it, the robot never finishes, and the check "Trained a robot to a finish" stays red. (The
   code never calls `Lesson.Apply`, which would reset locked settings.) The design below gives
   1.1 exactly one thing to place, the finish flag, which is what LUMA asks for.
2. **The run is invisible at the default speed.** At Max, training finishes in one frame and the
   five race laps finish under the "3, 2, 1, GO" banners. The design paces the run to be watched
   (below) and keeps Max for teachers in the menu's settings.

## Forms today, actions after

| Today | After |
|---|---|
| The points table: 6 rows, 23 number buttons (2.3 on) | Reward markers placed on the track, each with its number on a small dial: the chequered flag on the finish (0, 50, 100, 200), red cones against the walls (a crash: 0, −20, −100, −200), teal arrows along the racing line (a step toward the finish: 0, 1, 2, 5), red arrows pointing back (a step away: 0, −1, −2, −5), a clock post at the pit exit (each moment: 0, −0.2, −1, −3), a speed sign on the straight (speed bonus: 0, 1, 3). Drag a marker from the pit wall onto the track; tap it to turn its dial. Each placed marker shows one number. The table is what the player built, readable in the file. |
| Car tag SENSES (2.1 on), sensor bars in the run drawer | Sensors fitted to the car as parts: the front eye, the side whiskers, the speed gauge, the bend-ahead antenna. Tap a part to take it off or fit it; its ray on the track goes with it (the rays are already drawn). |
| Car tag MOVES (2.2 on) | The moves as the car's controls on the lift: the pedal (speed up, hold, slow down), the wheel (left, right), and the top-speed limiter on the dash. Tap one to block or allow it. |
| Attempts (1.2 on) | The fuel can: fill the tank before training; the tank is the attempt budget (the fuel gauge, below). |
| New moves (2.5 on), knobs (3.3 on), tracks (3.1 on), the measure (3.2 on) | Sketched only, for the build of the later levels: a die on the dash (the chance to try something new, and how fast it falls), the engineer's toolbox (the rewrite rate, the seed), track cards on the pit garage's board, the stopwatch or the finish counter on the pit wall. |
| Seven tags with OPENS AT | Nothing: a part not yet open is not on the car. When it opens, it is fitted in front of the player ("NEW: SENSORS"). |
| Follow / The track | One camera button. |
| Pause, Settings, Briefing, The file, Hide | The menu (Esc or the icon). |

## Tension: the fuel gauge

The attempt budget is the car's fuel: a gauge on the dash and in the corner of the screen, a
needle and no digits, full at attempt 1 and empty at the last. Each attempt burns a notch. A
setup that has not learned by the time the needle is low is plain to see; LUMA says so once.

## The training run, paced to be watched

The default pace is a montage of about 30 seconds, whatever the budget. Attempt 1 drives at
race speed and crashes (sparks, a shake). Then the attempts roll faster and faster: each leaves
a faint ghost line on the asphalt, crashes leave scuffs, and the points of each move float up
from the car (+2 teal, −100 red). The run slows to race speed for the moments: the first
finish, and the attempt where the curve settles. The fuel needle falls all along. In the menu,
a teacher can choose "Every attempt" (1x), the montage, or Max.

## Level 1.1

### Title

**Before: 84 words, 17 controls.** Wordmark 1 · "Games menu" 2 · "Sound on" 2 · the feed "LIVE CAM 3 /
THE GRID" 5, "19:42:14" 1, "MAALOOMATIIA RACEWAY / EVENING PRACTICE" 4 · "ROBOT RACING · GAME 03" 4 ·
"Nobody Drives" 2 · "You set the points. It teaches itself to race." 9 · LUMA 1 + "I'm LUMA, your coach.
Thirteen levels in three tiers. Press Start: the robot is on the grid." 17 · the level map: "TIER 1 ·
HOW A MACHINE LEARNS FROM REWARD" 8, "1.1 1.2 1.3" 3, "TIER 2 · DESIGN HOW IT LEARNS" 6, six nodes 6,
"TIER 3 · DOES IT REALLY WORK?" 6, four nodes 4 · "Start" 1 · "Free play" 2. Controls: Games menu,
Sound on, 13 nodes, Start, Free play.

**After: about 40 words, the map + Start + the menu.** As today (the car on the grid in its
camera feed, the wordmark, "Nobody Drives", the tagline), with the level map as the only ladder
in the game. The tier names shorten to "TIER 1", "TIER 2", "TIER 3"; their full names move to
the file. "Games menu", "Sound on" and "Free play" move to the menu. LUMA: "Press Start: the
robot is on the grid." (7)

### Briefing (a transmission)

**Before: 98 words, 15 controls.** "LEVEL 1.1 OF 3.4 · THE BRIEFING" 6 · "Who is driving?" 3 · "Nobody. It
senses, moves, gets points, and goes again." 9 · LUMA 1 + "Hi, I'm LUMA. Nobody drives this robot. Press
run and watch it crash, then crash later, then finish." 18 · "THE TRACK · ONE BEND" 4 · "A straight, one
right bend, the finish." 7 · a tile map · "YOUR ROBOT, PART BY PART · THIS LEVEL OPENS" 8 · seven part
chips, six dim 18 · "Go to the pit lane" 5 · "The file" 2 · "Title screen" 2 · "THE LADDER" 2 + thirteen steps 13.
Controls: Go to the pit lane, The file, Title screen, 12 ladder steps.

**After: about 45 words, 1 control (Skip) + the button.** The car on the grid of the one-bend
track, low, in the camera feed (LIVE, CAM 3, the clock). On the right:

> **Race control** · evening practice
> The robot on the grid has never driven.
> Nobody steers it. It senses, moves, gets points, and goes again.
> Watch it crash, then crash later, then finish.

Button: "To the pit lane". The second and third lines are the kicker and LUMA's intro, cut; the
first is new. The question, the track box (the track is on screen), the part chips and the
ladder go.

### Pit lane

**Before: 125 words, 5 controls.** Seven car tags, all locked: "SENSES · OPENS AT 2.1 · Front, Left,
Right, Speed" 8, "MOVES · OPENS AT 2.2 · 5 of 5 · top speed 2" 10, "TRACKS · OPENS AT 3.1 · One bend" 6,
"MEASURE · OPENS AT 3.2 · Finish rate" 6, "POINTS · OPENS AT 2.3 · finish 0 · crash 0 · toward 0" 10,
"KNOBS · OPENS AT 3.3 · rewrite 0.30 · 120 attempts" 8, "NEW MOVES · OPENS AT 2.5 · 0.9, falls fast" 8
· drawer: "LEVEL 1.1 · THE PIT LANE" 5, "Hide" 1, "Who is driving?" 3, "THE TRACK · ONE BEND" 4, "A straight,
one right bend, the finish." 7, "120 attempts · 5 laps on the test track" 8, "Briefing" 1, "The file" 2,
"Settings" 1, "TRAIN IT" 2 · LUMA 1 + "Fix it: No points at all. Every move earns zero. Attempt 100 looks like
attempt 1, because the points are the only thing it learns from. Turn the finish points on and run
again." 34. Controls: Hide, Briefing, The file, Settings, TRAIN IT.

**After: about 20 words, 1 control (menu) + "Train".** The car on the lift in the pit box, the
one-bend track beyond, the fuel gauge full. The chequered flag leans against the pit wall,
unlit.
- Objective: "Get it to the finish." (5)
- Teach by trying: the first time, "Train" runs the broken setup as it is. The montage shows 120
  attempts of random crashes, no ghost line reaching the bend, the fuel running out. LUMA: "Every
  move earns zero." (4), then "Turn the finish points on and run again." (8) as the flag lights
  up.
- The verb: drag the flag onto the finish line; it snaps there and shows "100". That is the one
  thing 1.1 lets the player change (it is what the broken setup asks for; the rest of the points
  stay closed until 2.3).
- Gone: all seven tags (their parts are not on the car until they open), the drawer, the track
  note (the track is in view), the attempts line (the fuel gauge). To the menu: Briefing, The file,
  Settings. To the file: LUMA's full line.

### The training run

**Before (at 1x, attempt 13): 203 words, 6 controls.** HUD "LEVEL 1.1" 2, "Attempt 13 of 120" 4, "No
finish yet" 3, "REWARD 0 BEST 0 LAP –" 5 · LUMA 1 + "Nobody is steering. The moves are random at first; the
ones that earned points come back more often." 18 · "Follow", "The track", "Pause", "Stop, race now",
"Settings" 8 · drawer: "LEVEL 1.1 · THE RUN" 4, "Hide" 1, "Training on one bend: 120 attempts, seed 7" 8,
"This attempt 0 · best 0 · last move +0" 8, "New move chance 0.75" 4, "9 situations" 2, the chart "POINTS PER
ATTEMPT · THE CURVE" 5 + axes 6, "WHAT IT SENSES" 3 + five sensor rows 11, "THE MOVE" 2 + "Speed up · +0" 3 +
"Speed 1 · 1 moves" 4 + "Driving." 1, "THE RULE BOOK · THE MOVE THAT PAID" 7 + nine rows of ten or eleven
words 93. Controls: Follow, The track, Pause, Stop race now, Settings, Hide.

**After: about 15 words, 3 controls (menu, camera, Details) + "Race".** The montage above, the
camera following the car, the ghost lines the hero of the frame.
- Top left: the objective (5). Top right: the fuel gauge. Floating over the car: each move's
  points.
- Moments: "ATTEMPT 1" (2); "FIRST FINISH" (2); "SETTLED" (1) when the curve settles.
- LUMA: "Nobody is steering." (3) on attempt 1; on the first finish, "One finish proves nothing:
  watch whether the curve stays up." (10), the second half of her moment line.
- Details (closed): the curve, the rule book, the sensors' readings, the move, the new-move
  chance, the seed: everything the drawer holds today, for the teacher who wants it.
- "Race": goes to the race now (today's "Stop, race now"). Pause and Settings are in the menu.

### The race

**Before (lap 3, at 1x): 188 words, 6 controls**, with the HUD line cut ("lap time 0 m..."), the drawer
still saying "0/5 laps" while the HUD says "Lap 3 of 5", the race over banner overflowing its card
(216 words), and at the default speed the five laps finishing under the countdown.

**After: about 15 words, 2 controls (menu, camera).** The countdown on the start gantry's red
lights going out (no "3, 2, 1" banner); the laps on a lap board by the track and as "LAP 3/5"
(2) at the top; a crash is sparks and a red flash on the lap board. LUMA at the start: "No new
moves, no learning." (5). At the end: "RACE OVER" with "0 of 5 laps" (4), then the debrief's
door. The race runs at race speed always.

### Before the checks: the questions

**Before: 146 words, 32 controls.** Seven questions in a scrolling list: five that name the five
parts, each with the same five chips ("The robot · The track · What it senses · What it does · The
points") 85 words and 25 chips; "What does the robot know at the start?" with three chips; "Why did the
no-points robot never improve?" with three answers below the fold; "To the checks".

**After: at most 30 words a question, no buttons.** The car back on the lift, the track behind.
LUMA asks one question at a time, and the player taps the answer in the world:
- "The one making the decisions." → the car. "The world it drives in." → the track. "What it knows
  about this moment." → the sensor rays. "Speed up, slow down, turn." → the wheels. "A number that
  says how good or bad the move was." → a floating point number. A right tap lights the part teal;
  a wrong one lights the right part and goes on.
- "What does the robot know at the start?" → three things on the pit wall: an empty rule book
  ("Nothing"), the track map ("The shape of the track"), a page of moves ("Which moves are good").
- "Why did the no-points robot never improve?" → three signs on the pit wall, with its three
  answers.
The answers are stored as the chips store them, so the checks do not change.

### Debrief

**Before: 127 words, 5 controls.** "Debrief · Level 1.1" 3 · the chart "POINTS PER ATTEMPT · EVERY RUN OF THIS
LEVEL" 8 + axes 6 · "ONE BEND · THE TEST TRACK · 0 OF 5 LAPS FINISHED" 10 · "THE MEDAL" 2, "SILVER" 1, "2 of 3
stars · 2 in all · Trainee" 8 · "THE CHECKS" 2 + four checks 19 · "THE RUNS · FIRST FINISH, SETTLED, THE RACE" 7 +
"Run 1 · first finish never · settled never of 120" 9 + "race 0/5 · 1.0 crashes, 4.0 touches, 0 moves a lap"
10 · LUMA 1 + "The robot learns from the points, and from nothing else." 10 · "UNLOCKED FOR LEVEL 1.2" 4,
"The attempts" 2, "2 of 3 stars banked · 2 in all · Trainee" 9 · "Next: 1.2 Is it learning, or just lucky?" 8 ·
"Train again" 2 · "The file" 2 · "Title screen" 2 · "Games menu" 2.

**After: about 20 words, 1 control (Details) + "Next", "Again".** The car crossing the finish of
the test track in slow motion (or stopped against the wall, if it crashed), the lap board
reading "5/5". The medal over it, its stars lighting one by one, "GOLD" (1). LUMA: "The robot
learns from the points, and from nothing else." (10). "Next" and a quiet "Again". Details: the
chart, the checks, the runs, the stars in all and the rank. The unlock card goes (1.2's fuel can
arrives when 1.2 opens); The file, Title screen and Games menu are in the menu.

## Level 2.3

### Briefing

**Before: 113 words, 15 controls.** As 1.1's, with "LEVEL 2.3 OF 3.4", "What earns points?", the kicker,
LUMA's 21-word intro, "THE TRACK · THE HAIRPIN", "The points" chip lit and five dim, the ladder.

**After: about 40 words.** The hairpin seen from the drone camera in the feed. On the right:

> **Race control** · the hairpin
> A long way round, with a tight turn back.
> Points only at the end leave the robot in the dark.
> Small points on the way light the path.

Button: "To the pit lane". (The track note and the kicker, verbatim.)

### Pit lane

**Before: 170 words, 28 controls.** Six tags with OPENS AT and the POINTS tag "finish 100 · crash 0 ·
toward 0" · the drawer as in 1.1 with "250 attempts" · "THE POINTS TABLE" 3 and six rows of a label and
four options: "The finish 0 · 50 · 100 · 200", "A crash 0 · -20 · -100 · -200", "Each moment 0 · -0.2 · -1 · -3",
"Step toward 0 · 1 · 2 · 5", "Step away 0 · -1 · -2 · -5", "Speed bonus 0 · 1 · 3" · LUMA 1 + "Fix it: Finish
points only. 100 at the finish and nothing else. The curve stays flat far past the budget: in the dark, a
random walk rarely reaches the end. Add the small on-the-way points and compare attempts to the first
finish." 42. Controls: Hide, Briefing, The file, Settings, TRAIN IT, 23 option buttons. (After a change
the POINTS tag's value is cut: "… tow...".)

**After: about 25 words, 1 control (menu) + "Train".** The hairpin from above at a low angle,
the car on the grid. The flag already stands on the finish with "100" (the broken setup). On the
pit wall shelf: the cones, the teal arrows, the red arrows, the clock post and the speed sign.
- Objective: "Light the path to the finish." (6)
- Arrival, first visit: the shelf slides out with "NEW: REWARDS" (2) and LUMA's "Points only at
  the finish leave the robot in the dark for hundreds of attempts." (15)
- Teach by trying: the player can train as it is; the montage burns the whole tank with almost
  no ghost line reaching the hairpin. LUMA: "Add the small on-the-way points and compare attempts to
  the first finish." (13)
- The verbs: drag a marker onto the track, tap it to turn its number. Six numbers on the track at
  most (6 words).
- Gone: the tags, the drawer, the table, the attempts line.

### Training run, race, debrief

As 1.1's. On 2.3 the race's "Before" is 224 to 227 words (with the "GO" banner showing after the
race had already finished), the training's 190 to 235. After: the same 15 words. Moments: "FIRST
FINISH" with the attempt number on the fuel gauge, "SETTLED". LUMA on the first finish: "One finish
proves nothing: watch whether the curve stays up." (10).

**Debrief before: 147 words, 5 controls** (the Next button's label wraps on two lines, and LUMA
repeats the briefing's kicker word for word). **After: about 25 words.** The car's best lap on
the hairpin with the markers the player placed still on the track; the medal; a small number
on the flag: the attempt of the first finish ("first finish at 23", 4); LUMA: "Small points on
the way light the path." (8, the takeaway's second sentence); Next, Again, Details.

## How the checks are met

| Check | Met by |
|---|---|
| 1.1 Named the five parts; knows nothing at the start; found why it never improved | The questions as taps on the world, stored as the chips store them. |
| 1.1 Trained a robot to a finish | The finish flag placed on the line (the one change 1.1 allows), then training. |
| 2.3 Set finish, crash and on-the-way points | The flag above 0, a cone below 0, an arrow toward above 0. |
| 2.3 The crash costs more than any single move can earn | The cones' number against the arrows' and the speed sign's: computed as today. |
| 2.3 Learned within the attempt budget | Unchanged: the training. |

## Open questions

1. **1.1's flag.** Letting the player place the finish flag in 1.1 opens one setting before 2.3.
   It is what LUMA and the broken setup already ask for, and without it 1.1 cannot be won. Agreed?
2. **The montage.** About 30 seconds of training whatever the budget, with Max kept in the menu.
   Or would you rather keep every attempt visible at a fixed fast speed?
3. **The curve.** The prompt puts it behind Details. Level 1.2's lesson is "learning is the curve
   going up and staying up": on 1.2 I would show it open by default, as the one screen where the
   curve is the point. Agreed?
