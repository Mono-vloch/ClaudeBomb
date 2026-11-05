# Robot Racing: content, levels and progression

**Topic:** teaching a machine to learn. **Follows:** the reinforcement learning module of the academy's self-paced track AI
Foundations for Data Engineers (edX, *AI Fundamentals: Core Concepts and Principles*, module 5, "Reinforcement Learning:
The Pavlovian Approach"), framed by the machine learning foundations of the Full Stack Data Bootcamp (Course 6, day 47) and
Applied Data Science (day 9). Read [README.md](README.md) first: it sets the steps every level follows.

**What the game is, from the outline.** Every student gets an identical robot, and nobody drives it. They design how it
learns: which sensors to switch on, which actions to allow, what earns points and what loses them. Then they watch it crash
for a hundred attempts and slowly get good, and find out that a robot rewarded slightly wrong will happily spin in circles
forever instead of finishing the race.

**A note on the sources.** The academy's instructor-led programs give reinforcement learning one slide each, as "the third
paradigm", and no lab; the one place it is taught in depth is the 25-minute module above, which the academy carries in a
self-paced track but did not write. So this game is the academy's first hands-on reinforcement learning material. Its words
come from that module and from the bootcamps' machine learning foundations, and it adds no term the courses do not use
(see "Words the game does not use"). The module's own racing example is the outline's game: a boat race rewarded only at
the finish that stays random "like trying to navigate a maze in complete darkness", then a speed bonus that makes it
"spin in tight circles, accumulating speed bonuses without ever finishing the race".

## Sources

| Source | What the game takes from it |
|---|---|
| edX module 5, `academy-os/.../artifacts/edx/ai-fundamentals---core-concepts-and-principles/transcripts.md` | The five parts and the loop; trial and error; cumulative reward; the policy; exploration and exploitation, epsilon and its decay; reward design, shaping and hacking; the racing example; simulation first, then the real world |
| edX *AI for Everyone*, `.../artifacts/edx/ai-for-everyone---master-the-basics/transcripts.md` | The student's job in one sentence: "You define the state, the desired goal, allowed actions, and constraints. The algorithm figures out how to achieve the goal" |
| Full Stack Data Bootcamp, Course 6, `academy-os/.../instructor-guides/Full-Stack-Data-Bootcamp/Course-06-Instructor-Guide.md`, `day-47-facilitator-guide.md`, `handouts/.../day-47-summary.md` | The three paradigms and classifying problems; "learning to ride a bike"; train and test; overfitting as memorizing; hyperparameters as knobs; learning rate too large or too small; "ML is not magic, it is disciplined experimentation"; "accuracy alone can lie to you" |
| Applied Data Science, day 9, `academy-os/.../course-powerpoints/applied-data-science/revised/generation-scripts/generate_revised_days_08_10.py` | "Reinforcement learning answers 'what should I do next?'"; choosing a measure by what a mistake costs |

## What a student can do after each tier

The courses have no tiered outcomes for reinforcement learning. These are built from the module's agenda and the
bootcamps' day 47 and day 9 outcomes, in the order the module teaches.

| Tier | Follows | The student can |
|---|---|---|
| 1. How a machine learns from reward | edX module 5, first half; Full Stack day 47 | Name the five parts (the robot, the track, what it senses, what it may do, the points) and the loop. Read the curve of points per attempt and tell learning from luck. Distinguish supervised, unsupervised and reinforcement learning and classify real problems into the right one. |
| 2. Design how it learns | edX module 5, second half; *AI for Everyone* | Choose what the robot senses, what it may do, and what earns and loses points, so that it learns to finish. Predict what a robot will learn from a points table before running it, spot the loophole, and close it. Set how much it tries new things, early and late. |
| 3. Does it really work? | Full Stack Course 6, days 1 to 4; edX module 5, closing | Tell a robot that memorized the track from one that learned to drive, on a track it never trained on. Choose the measure that matches what a mistake costs. Change one knob at a time on the same seed and compare. Explain why training happens in simulation and what can break on the real track. |

## Words on screen

The plain words come first; the term in brackets is shown once, when its level introduces it. The definitions are the
module's.

| Plain words | Term | Introduced |
|---|---|---|
| the robot: the one making the decisions | agent | 1.1 |
| the track: the world it drives in | environment | 1.1 |
| what it senses right now | state | 1.1 |
| what it does: speed up, slow down, turn | action | 1.1 |
| the points: a number that says how good or bad the move was | reward | 1.1 |
| the loop: sense, move, get points, again | (the module: "one loop, endlessly repeating") | 1.1 |
| an attempt: from the start line to the finish or a crash | (the courses say attempts, trials, iterations) | 1.2 |
| total points over the attempt, not the last move | cumulative reward | 1.2 |
| its rule book: for each situation, the move that paid | policy | 1.2 |
| studying with an answer key; finding groups with no key; learning to ride a bike | supervised, unsupervised, reinforcement learning | 1.3 |
| the sensors it has on | (the state, designed) | 2.1 |
| the moves it is allowed, and what it must never do | allowed actions and constraints | 2.2 |
| the points table | reward design | 2.3 |
| small points on the way | reward shaping | 2.3 |
| earning the points without doing the job | reward hacking | 2.4 |
| trying something new, or sticking with what worked | exploration and exploitation | 2.5 |
| the chance it tries something new, and how it falls over attempts | epsilon, epsilon decay | 2.5 |
| the training tracks and the test track | train and test | 3.1 |
| it memorized the track | overfitting | 3.1 |
| it learned to drive: good on a track it never saw | generalization | 3.1 |
| the measure you judge it by | metric | 3.2 |
| the knobs set before training | hyperparameters | 3.3 |
| how fast it rewrites its rule book | learning rate | 3.3 |
| the same seed: the same luck twice | random seed | 3.3 |
| train in the simulator, then one real run | simulation, then transfer | 3.4 |

**Words the game does not use.** Episode, return, discount, value, Q-table, Q-learning, Markov, action space, observation
space, terminal state. None of the courses uses them. The learning method under the hood is the developer's choice; the
student never sees it named. What the student must always see is the attempt count, the points per attempt as a curve,
the sensors' readings, and the rule book in some readable form.

## The progression at a glance

| Level | Question | Opens | Fix it (the planned mistake) |
|---|---|---|---|
| 1.1 | Who is driving? | nothing: press run | No points at all: attempt 100 looks like attempt 1 |
| 1.2 | Is it learning, or just lucky? | how many attempts | Stopping at the first finish |
| 1.3 | Capstone: What kind of learning is this? | (none) | (none) |
| 2.1 | What does it sense? | the sensors | No sensor facing the bend |
| 2.2 | What may it do? | the allowed moves, a constraint | A robot that cannot slow down |
| 2.3 | What earns points? | the points table | Finish points only |
| 2.4 | What does it learn when you reward the wrong thing? | (same) | The speed bonus |
| 2.5 | Should it try something new? | the chance of trying new things, and how it falls | Never again, and forever |
| 2.6 | Capstone: Your robot, your rules | everything in tier 2 | (none) |
| 3.1 | Did it learn the track, or learn to drive? | training tracks, the test track | One track only |
| 3.2 | Which number tells the truth? | the measure | Judged by finishes alone |
| 3.3 | Which knob, and how do you know? | the knobs, the seed | Three knobs at once |
| 3.4 | Capstone: From the simulator to the real track | everything | (staged: a difference the simulator did not have) |

Every setting a level has not opened is fixed at a sensible value, so only the level's idea is in play. Training must run
many attempts a second with a visible speed control: the module talks of thousands of attempts, and the class has minutes.

## Tier 1: How a machine learns from reward

**Placement check:** name the five parts; read a curve and say whether the robot has learned; sort three problems into the
three kinds of learning. A pass starts the student at 1.3.

### Level 1.1: Who is driving?

- **Follows:** edX module 5, the five core components ("one loop, endlessly repeating") and trial and error: "the agent
  starts knowing nothing. It takes random actions. Most of them fail, but occasionally, by chance, something works"; Full
  Stack day 47: "learning to ride a bike: you get feedback (rewards/penalties) but no step-by-step instructions".
- **The student can:** name the five parts and describe the loop, and explain that nobody drives and the robot starts
  knowing nothing.
- **The idea:** nobody drives. Every moment the robot senses, chooses a move, makes it, and gets points. It starts knowing
  nothing and moves at random; the moves that earned points it makes more often. That is all.
- **See it work:** a ready robot on a short track, over many attempts. The class watches it crash, then crash later, then
  finish.
- **Fix it: No points at all.** The ready setup gives zero points for everything. Attempt 100 looks like attempt 1. The
  student turns the finish points on and runs again: the points are the only thing it learns from.
- **The run must show:** the attempt count; each sensor's reading as it drives; the points for this move and for this
  attempt; the moment of a crash.
- **Checks:** named the five parts on the debrief; said what the robot knows at the start (nothing); found why the first
  setup never improved.
- **Takeaway:** The robot learns from the points, and from nothing else.
- **Ask back at work:** What do we reward, and what do we hope for?

### Level 1.2: Is it learning, or just lucky?

- **Follows:** edX module 5: the goal is "the cumulative reward across the entire sequence of decisions", not the last move;
  the policy, "a mapping from the state to actions that tells the agent what to do in each situation. A good policy is one
  that consistently leads to high cumulative rewards"; the baby learning to walk "over 1000s of attempts"; Full Stack
  Course 6: "ML is not magic, it is disciplined experimentation".
- **The student can:** read the curve of total points per attempt, tell learning from luck, and read the rule book the robot
  wrote itself.
- **The idea:** a random robot finishes sometimes, so one finish proves nothing. Learning shows as the total points per
  attempt rising and staying up. What it has learned is a rule book it wrote itself: for each situation it has met, the
  move that paid.
- **Opens:** how many attempts to train before the race.
- **Fix it: Stopping at the first finish.** The ready setup stops training the first time the robot finishes. Sent to race,
  it crashes. The student trains until the curve settles, and races again.
- **The run must show:** the curve of points per attempt; the rule book, readable for any situation (what it senses, what
  it does); the race result.
- **Checks:** trained until the curve settled; the robot finished most of several races, not one; read the rule book for
  one situation and said what the robot would do there.
- **Takeaway:** One good attempt proves nothing. Learning is the curve going up and staying up.
- **Ask back at work:** When we say a system "works", how many attempts did we watch?
- **Recall:** What does the robot know at the start?

### Level 1.3: Capstone: What kind of learning is this?

- **Follows:** Full Stack day 47, "Distinguish supervised, unsupervised, and reinforcement learning paradigms and classify
  real-world problems into the correct category", its "Classify the Problem" list (spam email, grouping customers, a
  used-car price, unusual network traffic, loan default) and its misconception "Reinforcement learning is NOT the same as
  supervised learning with feedback"; Applied Data Science day 9: "reinforcement learning answers 'what should I do next?'".
- **The student can:** run a ready robot on a new track and read its curve, name the five parts, and sort real problems into
  the three kinds of learning, explaining why the robot is not supervised learning.
- **The task:** a new track with a ready setup that learns. The student trains it, says when it has learned, names the five
  parts on a diagram, and sorts six problems: the five from the course's slide, plus the robot.
- **The debrief must:** put the three kinds side by side in the course's words: studying with an answer key (supervised),
  finding groups with no key (unsupervised), learning to ride a bike (reinforcement). The robot was never shown a correct
  lap.
- **Checks:** named the five parts; read the curve correctly; sorted the six problems; explained in one line why the robot
  is not supervised learning.
- **Takeaway:** No answer key and no examples. Only tries, points, and more tries.
- **If-then plans:** two, about the student's own work.

## Tier 2: Design how it learns

Follows the second half of the module and the one sentence in *AI for Everyone* that is the student's whole job: "You
define the state, the desired goal, allowed actions, and constraints. The algorithm figures out how to achieve the goal by
trying different combinations of allowed actions and is rewarded or punished." One setting opens per level, in that order:
the sensors (state), the moves (actions and constraints), the points (reward), then how much it explores.

**Placement check:** given a points table, predict what the robot will learn, and pick which of three tables has a loophole.
A pass starts the student at 2.6.

### Level 2.1: What does it sense?

- **Follows:** edX module 5, state: "the current situation, what the agent observes right now"; for a car, "the current
  position of nearby vehicles, lane markings, traffic signals and speed"; *AI for Everyone*: "You define the state".
- **The student can:** choose which sensors to switch on so the robot can tell apart the situations that need different
  moves.
- **The idea:** the robot's situation is only what its sensors report. Two moments its sensors cannot tell apart are one
  situation to it, and it makes the same move in both. A wall it cannot sense does not exist until it hits it.
- **Opens:** the sensors: which ones are on.
- **Fix it: No sensor facing the bend.** The ready robot has a forward sensor only. On the first bend it crashes into the
  same wall for a hundred attempts, because the straight before the bend and the bend look the same to it. The student
  switches on the sensor that faces the bend, and the curve lifts.
- **The run must show:** each sensor on and its reading, live; the rule book, so the class can see that one situation now
  became two.
- **Checks:** the robot got past the bend; explained why the first robot could not learn it.
- **Takeaway:** The robot's world is its sensors. What it cannot sense, it cannot learn.
- **Ask back at work:** What does our system actually measure, and what do we assume it sees?
- **Recall:** Which of the three kinds of learning has no answer key?

### Level 2.2: What may it do?

- **Follows:** edX module 5, action: "what the agent does ... accelerating, braking or steering. The agent chooses an action
  based on the current state"; *AI for Everyone*: "allowed actions, and constraints".
- **The student can:** choose the set of moves the robot may make, and set a constraint it must respect.
- **The idea:** the robot can only learn moves it is allowed. A move missing from the set is a bend it can never take. A
  constraint is a move it must never make, whatever the points say.
- **Opens:** the allowed moves (for example: speed up, hold, slow down, turn left, turn right) and one constraint (for
  example a top speed).
- **Fix it: A robot that cannot slow down.** The ready setup allows turns and full speed only. It learns the straights and
  crashes on every tight bend. The student adds "slow down" and watches the rule book use it at the bend.
- **The run must show:** the moves allowed; in the rule book, where the new move is used.
- **Checks:** the robot finished; a constraint was set; the rule book uses the added move where it matters.
- **Takeaway:** It can only learn the moves you allowed.
- **Ask back at work:** Which options have we given the system, and which did we forget?
- **Recall:** What happens to two situations the sensors cannot tell apart?

### Level 2.3: What earns points?

- **Follows:** edX module 5: "Reward design is the most important design choice in RL"; the racing example, "+100 points for
  reaching the finish line, zero points for everything else", after which "the agent takes random action for an incredibly
  long time because it has no signal, like trying to navigate a maze in complete darkness"; the fix, "a small positive
  reward for moving towards the finish line and a small negative reward for moving away from it" (reward shaping); the
  driving table: reach the destination safely, "big positive reward"; crash or break the rules, "strong penalty"; stay in
  lane, "small positive reward".
- **The student can:** write a points table that gets the robot to finish within the attempt budget: a big reward at the
  finish, a strong penalty for a crash, small points on the way.
- **The idea:** points only at the end leave the robot in the dark for thousands of attempts. Small points on the way light
  the path. A crash must cost more than any shortcut earns.
- **Opens:** the points table: the finish, a crash, each step toward the finish, each step away from it, each moment spent.
- **Fix it: Finish points only.** +100 at the finish and nothing else. The curve stays flat far past the attempt budget.
  The student adds the small on-the-way points and compares attempts to first finish.
- **The run must show:** attempts to the first finish and attempts to settle, for each table tried, side by side.
- **Checks:** set finish, crash and on-the-way points; learned within the attempt budget; the crash penalty is larger than
  what any single move can earn.
- **Takeaway:** Points only at the end leave the robot in the dark. Small points on the way light the path.
- **Ask back at work:** Where do we only measure the end result, and leave the people in the dark on the way?
- **Recall:** What is a constraint?

### Level 2.4: What does it learn when you reward the wrong thing?

- **Follows:** edX module 5, reward hacking: a speed bonus, and the agent "found a loop in the environment where it could
  spin in tight circles, accumulating speed bonuses without ever finishing the race"; "It doesn't care about your
  intention. It only cares about the number"; "The agent optimizes exactly what you incentivize, which isn't always what
  you intend"; Atari Breakout tunneling through the side of the wall; the module's own bridge to people, a sales team
  rewarded purely on volume.
- **The student can:** predict what a robot will learn from a points table before running it, spot the loophole, and close
  it.
- **The idea:** the robot does not know what you meant. It finds the cheapest way to the number. A loophole in the table
  is a lap it will never drive.
- **Fix it: The speed bonus.** The ready table pays points for speed. The robot spins at full speed in a corner forever,
  and its curve rises. The curve measures the points, not the job. The student closes the loophole (points for progress,
  not speed, or a cost per moment) and the robot finishes.
- **Your turn:** three points tables. The student writes one line predicting what each robot will do, then runs all three.
- **The run must show:** the rising curve of the spinning robot next to its lap count, which stays at zero.
- **Checks:** predicted the loophole before running; the fixed robot finished; explained why a curve can rise while the
  robot never finishes.
- **Takeaway:** It optimizes exactly what you reward, which is not always what you meant.
- **Ask back at work:** Where do we reward the number and hope for the goal?
- **Recall:** Why did the finish-only robot stay in the dark?

### Level 2.5: Should it try something new?

- **Follows:** edX module 5, exploration and exploitation (a new city: the restaurant with "a solid four out of five stars"
  or "a hidden five star gem"); "If it always sticks with the first action that gave a positive reward, it might miss much
  better strategies"; "Too much exploration wastes time, but too much exploitation early can prevent the agent from
  discovering better strategies"; epsilon, "the probability that AI will explore new actions instead of choosing the best
  action it already knows", high at the start and gradually decreasing.
- **The student can:** set how often the robot tries something new, high early and falling later, and say what goes wrong
  at either extreme.
- **The idea:** a robot that only repeats the first thing that worked is stuck with a slow line. A robot that keeps trying
  new things forever never settles. Try a lot early, settle later.
- **Opens:** the chance it tries something new, and how it falls over attempts.
- **Fix it: Never again, and forever.** Two ready robots. One stops trying anything new after its first finish: it keeps a
  slow, wide line and never improves. The other tries new things on every move to the end: its curve never settles. The
  student sets a high chance that falls, and both problems go.
- **The run must show:** the chance of a new move, live, and the curve of each robot side by side.
- **Checks:** set a high chance that falls; the robot's line improved after its first finish; the curve settled.
- **Takeaway:** Try a lot early, settle later.
- **Ask back at work:** When did we last try something other than what worked first?
- **Recall:** What does a rising curve measure?

### Level 2.6: Capstone: Your robot, your rules

- **Follows:** everything in tier 2; the class race is the outline's "every student gets an identical robot".
- **The student can:** set sensors, moves, points and exploration for a new track with no help, predict how the robot will
  behave, and get it to finish within the attempt budget.
- **The task:** a new track with at least one feature that punishes a loophole. Before running, the student writes one line
  on what the robot will do. Then the class race.
- **Checks:** finished within the attempt budget; no loophole exploited; the prediction matched what happened, or the
  student explained the difference; the sensors on are the ones the track needed.
- **Takeaway:** You define what it senses, what it may do and what earns points. It figures out the rest, exactly as you
  wrote it.
- **If-then plans:** two, about the student's own work.

## Tier 3: Does it really work?

Follows the machine learning foundations the bootcamps teach around the one reinforcement learning slide: train and test,
overfitting, hyperparameters, metrics (Full Stack Course 6, days 1 to 4), and the module's closing: training in simulation,
then the real world.

**Placement check:** given two curves and a test-track result, say which robot learned to drive and which memorized the
track. A pass starts the student at 3.4.

### Level 3.1: Did it learn the track, or learn to drive?

- **Follows:** Full Stack Course 6: "Why do we split data into train and test? To measure how the model performs on data it
  has never seen"; "Overfitting is memorizing the exam answers instead of understanding the material"; day 47: the test set
  is "like taking a final exam with questions you have never studied"; edX module 3: a good model "generalizes, meaning it
  performs well, not just on training data, but on new unseen data too", and "the integrity of the test set is sacred".
- **The student can:** test a robot on a track it never trained on, tell memorizing from learning, and train on several
  tracks to get a robot that drives.
- **The idea:** a perfect lap on the training track proves the robot knows that track. The only proof it learned to drive is
  a track it has never seen. The test track is never trained on.
- **Opens:** the training tracks (one or several) and a test track the robot never trains on.
- **Fix it: One track only.** Trained on one track, the ready robot laps it perfectly. On the test track it crashes at the
  first bend that is different: it memorized. The student trains it on several tracks and tests again.
- **The run must show:** the curve on the training tracks and, separately, the result on the test track.
- **Checks:** the test track was never trained on; the robot finished the test track; explained what the one-track robot
  had actually learned.
- **Takeaway:** A robot that learned the track memorized. A robot that learned to drive passes a track it has never seen.
- **Ask back at work:** What has our system only ever been tested on?
- **Recall:** Name the setting you change to make the robot try more new moves.

### Level 3.2: Which number tells the truth?

- **Follows:** Full Stack Course 6: "Accuracy alone can lie to you"; Applied Data Science day 3: "Choosing metrics by business
  context (cost of false positives vs false negatives)"; day 9, Mitchell: performance at a task "as measured by P".
- **The student can:** choose the measure to judge a robot by, from what a mistake costs.
- **The idea:** finishes, crashes, wall touches, lap time: each measure tells a different story about the same robot. The one
  to judge by is the one that matches what a mistake costs.
- **Opens:** the measure the student judges by: finish rate, crashes per lap, wall touches per lap, lap time.
- **Fix it: Judged by finishes alone.** Two ready robots. One finishes nine laps in ten and scrapes the wall every lap. The
  other finishes seven in ten and never touches a wall. Judged by finishes, the first wins. The student is told what a
  wall touch costs on the real track, and picks again.
- **The run must show:** every measure for every robot on the test track, side by side.
- **Checks:** picked the measure that fits the stated cost; explained in one line why finish rate lied.
- **Takeaway:** Pick the measure that matches what a mistake costs.
- **Ask back at work:** Which number do we report, and what does it hide?
- **Recall:** Why is the test track never trained on?

### Level 3.3: Which knob, and how do you know?

- **Follows:** Full Stack Course 6: "Hyperparameters are the knobs you set before training"; learning rate too large, "you
  overshoot and oscillate", too small, "you converge, but it takes forever"; "set random_state=42"; "ML is not magic, it is
  disciplined experimentation".
- **The student can:** change one knob at a time, on the same seed, and say which change helped.
- **The idea:** the knobs are set before training: how fast it rewrites its rule book, how long it keeps trying new things,
  how many attempts it gets. The same luck twice (the same seed) is the only way to compare two runs fairly. Change one
  knob, keep the rest, compare.
- **Opens:** the knobs, and the seed.
- **Fix it: Three knobs at once.** The ready experiment changed three knobs between two runs. The curve improved. Which
  knob did it? Nobody can tell. The student reruns with one knob changed at a time on the same seed, and finds the one that
  mattered (and the one that made it worse).
- **Then:** the rewrite speed set too high: the curve jumps up and down and never settles; too low: it settles, far past the
  attempt budget.
- **The run must show:** runs side by side with their knobs listed and their seed, so a difference is traceable to one knob.
- **Checks:** changed one knob per run; kept the seed; named the knob that helped; recognized the too-high and the too-low
  rewrite speed from their curves.
- **Takeaway:** Change one thing, keep the rest the same, compare.
- **Ask back at work:** When we changed three things and it got better, what did we learn?
- **Recall:** Which measure did finish rate hide?

### Level 3.4: Capstone: From the simulator to the real track

- **Follows:** edX module 5: "RL typically requires enormous amounts of trial and error ... You can't let a self driving car
  crash 1000s of times to learn. This is why much of RL training happens in simulation first, with careful transfer to
  real world, a process that remains an active area of research"; the robot hand trained "entirely in simulation, then
  transferred to a physical robot's hand".
- **The student can:** train a robot in the simulator within an attempt budget, pass the test track, and take one real run
  on a track with a difference the simulator did not have, predicting beforehand what might break.
- **The task:** several training tracks, a test track, a fixed attempt budget and a seed. Then one real run: a track the
  simulator did not have, with a difference in it (the developer's choice), one attempt, no learning during it. Before
  the real run, the student writes one line on what could break.
- **Checks:** trained within the budget; the test track passed; the knobs were set one at a time; the real run finished, or
  the student named the difference that broke it; the measure chosen matched the stated cost of a crash.
- **Takeaway:** Train where crashing is cheap. Test on what it has never seen. Expect the real track to be different.
- **Ask back at work:** Where did our system learn, and how different is the place it will be used?
- **If-then plans:** two, about the student's own work.

## Not taught by the game

Reinforcement learning from human feedback (named in the SE-AI Bootcamp, day 8, never explained); self-play and the game
histories the module tells (the debrief may mention them, nothing is checked); any named learning method or its numbers.

## Open points

1. **The catalogue has no course for Robot Racing** (`site/games.json`, `"course": null`). The candidates are the
   self-paced track AI Foundations for Data Engineers, whose edX module this file follows, or the Full Stack Data
   Bootcamp's Course 6. Someone has to pick.
2. **The academy has no assessment items on reinforcement learning.** The recall questions and checks in this file are the
   first; whoever owns Course 6 should read them.
3. **"Attempt" is the game's word.** The courses say attempts, trials and iterations loosely and never "episode".
4. **The module is not the academy's.** Its words are HP's, carried in an edX course. If the academy writes its own
   reinforcement learning session, this file follows it.
5. **The real-run difference** in 3.4 and the loophole feature in 2.6 are the developer's to design; the course names only
   the spinning-in-circles case.
