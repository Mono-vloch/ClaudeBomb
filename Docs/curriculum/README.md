# Curriculum for the games

What each game teaches, level by level, and how a level is built as a lesson. One file per game:

| Game | Topic | Follows the course | File |
|---|---|---|---|
| AI Arena | Building an AI agent | Applied Agentic AI | [ai-arena.md](ai-arena.md) |
| Robot Racing | Teaching a machine to learn | The reinforcement learning module of AI Foundations for Data Engineers (edX, module 5), framed by the Full Stack Data Bootcamp's Course 6 | [robot-racing.md](robot-racing.md) |
| Tech Escape | Defending a real system | Applied Introduction to Information Security, then Professional Practices in Information Security; the incident day of Cyber Security Fundamentals for the exit | [tech-escape.md](tech-escape.md) |

Claude Bomb is not covered here.

These files set the **content, levels and progression**: what each level teaches, what the student controls, which mistake
the level makes happen, and what counts as having learned it. How a level looks, sounds and plays, the theme, the
names of things on screen and the art are the game developer's call. Where a file says "the board", "an item" or "a
room", that is the outline's word for a mechanic, not a design.

## Where the content comes from

Every level points to the course session it follows. The sources, in the order they were used:

- **Academy OS**, `~/Projects/academy-os/academy-server/storage/content/`: the developed programs, course decks, handouts,
  instructor guides, labs and assessments.
- **The course overviews**, `~/Desktop/Maaloomatiia Course Overviews 2026-10-26/`: Applied Agentic AI, the plain-wording
  pass of 27 Oct 2026 and the instructional design panel's decision.
- **The presentations**, `~/Desktop/presentations/Qatar/`: the Business AI series (Deploying and Supervising AI Agents,
  Prompting and Context Engineering).
- **The four-games outline**, `~/Downloads/four-games-overview.html`.

A game teaches the course's words, in the course's order, on the course's examples. It does not add a concept the course
does not teach, and where a game needs one, the game's file lists it under "Open points".

## How every level is built

The courses follow eight rules from learning research (Applied Agentic AI, section 5). Each one becomes a rule for the games:

| The course does | So every game does |
|---|---|
| Opens each session with a 10-minute, no-stakes recall quiz on earlier sessions | Opens each level with one or two recall questions on earlier levels. No score, no penalty. A wrong answer shows the right one and the level it came from. |
| Worked example, then partly completed work, then independent work | Each level runs the same three steps: **See it work**, **Fix it**, **Your turn** (below). Early tiers give more help in Your turn; the last tier gives none. |
| Hides a planned failure in every lab, found and fixed by the learners | Every level has one named mistake it makes happen on purpose, the **Fix it** step. The mistake is the one the course names for that session. |
| Lets placement-checked experts skip ahead | A short placement check at the start of each tier lets a student who passes start that tier at its capstone. |
| Spaces sessions a week apart | Each tier is one class session. Tiers are meant to be played on different days, and each tier opens with recall from the tiers before. |
| Counts skill and scenario checks; satisfaction is feedback only | The level's checks (what the student actually did) decide complete, partial or incomplete. Points, rankings and stars are for motivation and never replace the checks. |
| Ends each scene with a one-sentence takeaway and an "Ask as a manager" question | Every debrief ends with one takeaway sentence and one question to take back to work. |
| Two written if-then plans per participant at the capstone | Every tier capstone ends with the student writing two "If ..., then I ..." plans about their own work. |

### The steps of a level

1. **Recall.** One or two questions from earlier levels. Skipped on the very first level.
2. **The idea.** One screen, under 80 words, in plain words. Introduces at most one new term.
3. **See it work.** A ready-made setup that gets the job done. The student runs it and is shown why it works. Short:
   one run.
4. **Fix it.** A ready-made setup with the level's planned mistake in it. The student runs it, watches it fail, finds the
   cause and fixes it. The fix is checked by running again.
5. **Your turn.** A new task the student has not seen. In tier 1 some of the setup is filled in; by the last tier the
   student starts from nothing.
6. **Debrief.** What happened and why, the level's checks ticked or missed, the takeaway, the question to take back to work.

In a class game (AI Arena, Robot Racing in class mode), See it work and Fix it can be one shared run on the projector and
Your turn is the round everyone plays.

### Tiers

Every game has three tiers, which mirror the course's Introduction, Foundation and Advanced levels. A tier ends with a
capstone that checks the tier's outcomes together, on a task with no help. A student can replay any finished level.

### Words on screen

From the course overviews' plain-wording pass (27 Oct 2026) and the panel's conventions:

- Everything the student reads is in plain words. A technical term appears once, in brackets, the first time the level
  introduces it: "hidden instructions in what it reads (prompt injection)". After that the plain words are used.
- Level names are a task or a question, never a subject: "Level 3: What does it see?", not "Level 3: Context".
- The planned mistake is introduced as "Fix it: <the mistake>", for example "Fix it: An agent that never stops".
- American spelling. No em or en dashes, no emoji (Academy OS rule D-13).

### Reporting to the platform

The kit's `Platform.ReportProgress(result, stars, detail)` is called once per level finished:

- `result`: `"<tier>.<level>:complete"`, `":partial"` or `":incomplete"`, from the level's checks (all ticked, at least
  half ticked, fewer than half).
- `stars`: optional, for motivation only.
- `detail`: the checks as a short list, ticked or missed, so a facilitator can see what to reteach.

The keeper only acknowledges progress today; storing it per student comes with the student codes.

## Two things the sources settled

- **Tech Escape does not follow the built "Cyber Security Fundamentals".** That course is application security for
  software engineers (threat modelling, secure coding, OWASP, supply chain) on a fictional checkout service, and holds few of
  the outline's twelve rooms. The two information security programs on the training sheet hold eleven of them and the
  exit, as outcomes and hands-on practice, not yet as decks. The game's file follows the sheet; the catalogue's course
  line for the game has to change.
- **Reinforcement learning is taught in depth in one place:** a 25-minute edX module inside the self-paced AI Foundations
  track. Every bootcamp gives it one slide as "the third paradigm", and no course has a lab or a quiz item on it. Robot
  Racing follows that module's sequence and the bootcamps' machine learning foundations around it, and is the academy's
  first hands-on reinforcement learning material.

## Open points shared by all three games

1. **Session length.** The catalogue gives AI Arena and Tech Escape 20 minutes and Robot Racing 15. That fits one
   level, not one tier. The files assume a tier is one class session of 60 to 90 minutes. Confirm, or the tiers are split.
2. **Placement checks** are listed per game but not written yet.
3. **If-then plans** need a place to be kept until the course's follow-up session (4 to 6 weeks later) reviews them.
