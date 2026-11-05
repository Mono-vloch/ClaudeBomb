// Maaloomatiia Games: the menu page.
//
// Two states. The door (dark cover) asks for the class code; the menu puts the games on a
// stage, one at a time, with a strip of all of them underneath and a sheet for the details
// and the Play button. The code goes to /api/session, which answers with a cookie that the
// games' own Claude calls then carry, so a game never sees the code. /api/me says whether
// that cookie is still good, so a returning student skips the door.
//
// A game sends the player back here with ?signin=1&return=/games/<id>/ when its calls
// are refused; after the code, the player goes straight back to that game.

(function () {
  "use strict";

  const $ = (sel) => document.querySelector(sel);
  const door = $("#door");
  const menu = $("#menu");
  const launch = $("#launch");
  const form = $("#signin");
  const input = $("#code");
  const enter = $("#enter");
  const note = $("#signin-note");
  const stage = $("#stage");
  const slidesEl = $("#slides");
  const stripEl = $("#strip");
  const slideTemplate = $("#slide-template");
  const stripTemplate = $("#strip-template");
  const reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  const params = new URLSearchParams(location.search);
  const returnTo = safeReturn(params.get("return"));
  const forceSignIn = params.get("signin") === "1";

  // Only a game on this site is a place to return to.
  function safeReturn(path) {
    return typeof path === "string" && /^\/games\/[a-z0-9-]+\/?(\?[^#]*)?$/.test(path) ? path : null;
  }

  async function json(url, options) {
    const res = await fetch(url, { credentials: "same-origin", ...options });
    let data = null;
    try { data = await res.json(); } catch (e) { /* no body */ }
    return { ok: res.ok, status: res.status, data: data || {} };
  }

  function el(tag, className, text) {
    const node = document.createElement(tag);
    node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
  }

  // ---- Boot ----

  async function boot() {
    const [me, catalog] = await Promise.all([json("/api/me"), json("games.json")]);
    render(catalog.data.games || []);
    if (me.data.signedIn && !forceSignIn) {
      if (returnTo) return go(returnTo, titleFor(returnTo));
      openDoor(true);
    } else {
      doorField.start();
      if (forceSignIn) say("Your sign-in has run out. Enter the code again to pick up where you were.", false);
      if (me.status === 503) say("The platform is not set up yet. The instructor needs to finish the server settings.", true);
      input.focus();
    }
  }

  function titleFor(path) {
    const id = (path.match(/^\/games\/([a-z0-9-]+)/) || [])[1];
    const game = games.find((g) => g.id === id);
    return game ? game.title : "";
  }

  // ---- The door ----

  function say(text, bad) {
    note.textContent = text;
    note.classList.toggle("signin__note--bad", Boolean(bad));
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const code = input.value.trim();
    if (!code) { say("Type the code your instructor gave the class.", true); input.focus(); return; }
    enter.disabled = true;
    say("Checking", false);
    const res = await json("/api/session", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ code }),
    });
    enter.disabled = false;
    if (!res.ok) {
      say(res.data.error || (res.status === 503 ? "The platform is not set up yet." : "That code did not work."), true);
      input.classList.remove("signin__input--shake");
      void input.offsetWidth; // restart the animation
      input.classList.add("signin__input--shake");
      input.select();
      return;
    }
    say("", false);
    if (returnTo) return go(returnTo, titleFor(returnTo));
    openDoor(false);
  });

  let menuOpen = false;

  function openDoor(instant) {
    menu.setAttribute("aria-hidden", "false");
    menuOpen = true;
    heroField.start();
    if (instant || reduced) {
      door.classList.add("door--open", "door--gone");
      menu.classList.add("menu--shown");
      doorField.stop();
      show(0, true);
      return;
    }
    door.classList.add("door--open");
    setTimeout(() => { menu.classList.add("menu--shown"); show(0, true); }, 150);
    setTimeout(() => { door.classList.add("door--gone"); doorField.stop(); }, 1000);
  }

  $("#signout").addEventListener("click", async () => {
    await json("/api/session", { method: "DELETE" });
    location.replace("/");
  });

  // ---- The stage ----

  let games = [];
  let index = 0;

  // Every game gets a slide (the stage) and an item (the strip). Only the slide's words and
  // cover are fixed here; its buttons depend on its state, which mark() sets.
  function render(list) {
    games = list;
    slidesEl.replaceChildren();
    stripEl.replaceChildren();
    $("#count-all").textContent = String(games.length).padStart(2, "0");
    games.forEach((game, i) => {
      const slide = slideTemplate.content.firstElementChild.cloneNode(true);
      slide.setAttribute("aria-label", (i + 1) + " of " + games.length);
      if (game.tone === "light") slide.classList.add("slide--light");
      slide.querySelector(".slide__ghost").textContent = game.number;
      slide.querySelector(".slide__topic").textContent = game.topic;
      slide.querySelector(".slide__title").textContent = game.title;
      slide.querySelector(".slide__pitch").textContent = game.pitch;
      slide.querySelector(".slide__device").textContent = game.device ? "Plays on " + game.device.charAt(0).toLowerCase() + game.device.slice(1) + "." : "";
      const cover = slide.querySelector(".slide__cover");
      const img = slide.querySelector(".slide__img");
      img.src = game.cover;
      img.alt = "";
      cover.setAttribute("aria-label", "About " + game.title);
      cover.addEventListener("click", () => open(game));
      slidesEl.appendChild(slide);

      const item = stripTemplate.content.firstElementChild.cloneNode(true);
      item.querySelector(".strip__thumb").src = game.cover;
      item.querySelector(".strip__num").textContent = game.number;
      item.querySelector(".strip__title").textContent = game.title;
      item.querySelector(".strip__btn").addEventListener("click", () => { settle(); show(i); });
      // The line under the lit item fills over --dwell; when it is full, the next game comes
      // on. After one full round the stage holds still, so a projector stops turning.
      item.querySelector(".strip__bar").addEventListener("animationend", () => {
        if (!autoplay) return;
        turned += 1;
        show(index + 1);
        if (turned >= games.length) settle();
      });
      stripEl.appendChild(item);

      game.slide = slide;
      game.item = item;
      mark(game, game.status === "ready" && game.path ? "ready" : "soon");
      if (game.state === "ready") { tilt(game, cover, img); checkBuild(game); }
    });
    stage.classList.toggle("stage--still", reduced);
  }

  // ready: it can be played. soon: not made yet. missing: not built on this computer.
  function mark(game, state) {
    game.state = state;
    game.slide.classList.toggle("slide--soon", state !== "ready");
    const actions = game.slide.querySelector(".slide__actions");
    actions.replaceChildren();
    if (state === "ready") {
      const play = el("a", "btn btn--on-dark", "Play");
      play.href = game.path;
      play.addEventListener("click", (e) => { e.preventDefault(); go(game.path, game.title); });
      actions.appendChild(play);
    } else {
      actions.appendChild(el("span", "slide__status", statusOf(game)));
    }
    const about = el("button", "btn btn--ghost", "About the game");
    about.type = "button";
    about.addEventListener("click", () => open(game));
    actions.appendChild(about);
    game.item.querySelector(".strip__note").textContent = state === "ready" ? "" : statusOf(game);
    if (current === game) fill(game);
  }

  function statusOf(game) {
    return game.state === "missing" ? "Not built on this computer yet" : "In the works";
  }

  // On a computer where a game has not been built yet, say so instead of a dead link.
  async function checkBuild(game) {
    try {
      const res = await fetch(game.path + "index.html", { method: "HEAD", credentials: "same-origin" });
      if (res.ok) return;
    } catch (e) { /* treated as missing */ }
    mark(game, "missing");
  }

  // Puts game i on the stage. The incoming slide travels from the side it is on in the
  // series, so next feels like next and previous like previous.
  function show(i, instant) {
    const n = games.length;
    if (!n) return;
    const next = ((i % n) + n) % n;
    const step = (next - index + n) % n;
    if (step !== 0) slidesEl.style.setProperty("--dir", step <= n / 2 ? "1" : "-1");
    index = next;
    games.forEach((g, k) => {
      const on = k === index;
      g.slide.classList.toggle("slide--active", on);
      g.slide.setAttribute("aria-hidden", String(!on));
      g.item.classList.remove("strip__item--active");
    });
    const item = games[index].item;
    void item.offsetWidth; // so the line under it starts again from nothing
    item.classList.add("strip__item--active");
    $("#count-now").textContent = games[index].number;

    // Keep the lit item in view when the strip scrolls (phones).
    const left = item.offsetLeft, right = left + item.offsetWidth;
    if (left < stripEl.scrollLeft || right > stripEl.scrollLeft + stripEl.clientWidth)
      stripEl.scrollTo({ left: left - 16, behavior: instant || reduced ? "auto" : "smooth" });
  }

  // The stage turns by itself only until someone takes over, or one full round is done.
  let autoplay = !reduced;
  let turned = 0;
  function settle() {
    if (!autoplay) return;
    autoplay = false;
    stage.classList.add("stage--still");
  }

  $("#prev").addEventListener("click", () => { settle(); show(index - 1); });
  $("#next").addEventListener("click", () => { settle(); show(index + 1); });

  document.addEventListener("keydown", (e) => {
    if (!menuOpen || sheet.open || e.altKey || e.ctrlKey || e.metaKey) return;
    if (e.target instanceof Element && e.target.matches("input, textarea, select")) return;
    if (e.key === "ArrowRight") { e.preventDefault(); settle(); show(index + 1); }
    else if (e.key === "ArrowLeft") { e.preventDefault(); settle(); show(index - 1); }
  });

  // A sideways swipe on the slides turns the page; a swipe is not a click on the cover.
  let swipe = null;
  let swiped = false;
  slidesEl.addEventListener("pointerdown", (e) => {
    if (e.pointerType === "mouse" && e.button !== 0) return;
    swipe = { x: e.clientX, y: e.clientY, id: e.pointerId };
  });
  slidesEl.addEventListener("pointerup", (e) => {
    if (!swipe || e.pointerId !== swipe.id) return;
    const dx = e.clientX - swipe.x, dy = e.clientY - swipe.y;
    swipe = null;
    if (Math.abs(dx) > 48 && Math.abs(dx) > Math.abs(dy) * 1.5) { swiped = true; settle(); show(index + (dx < 0 ? 1 : -1)); }
  });
  slidesEl.addEventListener("pointercancel", () => { swipe = null; });
  slidesEl.addEventListener("click", (e) => { if (swiped) { swiped = false; e.preventDefault(); e.stopPropagation(); } }, true);

  // The stage waits while the player is on it: the pointer over it, the keyboard in it,
  // the sheet open, or the tab out of sight.
  const pauses = new Set();
  function pause(reason, on) {
    if (on) pauses.add(reason); else pauses.delete(reason);
    stage.classList.toggle("stage--paused", pauses.size > 0);
  }
  stage.addEventListener("pointerenter", (e) => { if (e.pointerType === "mouse") pause("hover", true); });
  stage.addEventListener("pointerleave", (e) => { if (e.pointerType === "mouse") pause("hover", false); });
  stage.addEventListener("focusin", () => pause("focus", true));
  stage.addEventListener("focusout", (e) => { if (!stage.contains(e.relatedTarget)) pause("focus", false); });
  document.addEventListener("visibilitychange", () => pause("hidden", document.hidden));

  // The cover leans a little towards the pointer: a mouse-only touch of depth.
  function tilt(game, cover, img) {
    if (reduced || !window.matchMedia("(hover: hover) and (pointer: fine)").matches) return;
    cover.addEventListener("pointermove", (e) => {
      if (game.state !== "ready") return;
      const r = cover.getBoundingClientRect();
      const x = (e.clientX - r.left) / r.width - 0.5;
      const y = (e.clientY - r.top) / r.height - 0.5;
      img.classList.add("is-tilting");
      img.style.transform = "scale(1.045) rotateX(" + (-y * 5).toFixed(2) + "deg) rotateY(" + (x * 6).toFixed(2) + "deg)";
    });
    cover.addEventListener("pointerleave", () => {
      img.classList.remove("is-tilting");
      img.style.transform = "";
    });
  }

  // ---- The sheet: one game's details and its Play button ----

  const sheet = $("#sheet");
  const actions = $("#sheet-actions");
  let current = null;

  function open(game) {
    current = game;
    fill(game);
    document.documentElement.classList.add("has-sheet");
    pause("sheet", true);
    sheet.showModal();
  }

  function fill(game) {
    sheet.classList.toggle("sheet--light", game.tone === "light");
    sheet.classList.toggle("sheet--soon", game.state !== "ready");
    $("#sheet-img").src = game.cover;
    $("#sheet-num").textContent = game.number;
    $("#sheet-topic").textContent = game.topic;
    $("#sheet-title").textContent = game.title;
    $("#sheet-pitch").textContent = game.pitch;
    $("#sheet-about").textContent = game.about;
    $("#sheet-course").textContent = game.course || "To be announced";
    $("#sheet-time").textContent = "About " + game.minutes + " min";
    $("#sheet-players").textContent = game.players;
    $("#sheet-device").textContent = game.device || "A computer";
    actions.replaceChildren();
    if (game.state === "ready") {
      const play = el("a", "btn btn--primary", "Play");
      play.href = game.path;
      play.autofocus = true;
      // The sheet sits above everything, the launch screen included, so it closes first.
      play.addEventListener("click", (e) => { e.preventDefault(); sheet.close(); go(game.path, game.title); });
      actions.appendChild(play);
    } else {
      actions.appendChild(el("span", "sheet__status", statusOf(game)));
    }
  }

  sheet.addEventListener("close", () => {
    current = null;
    document.documentElement.classList.remove("has-sheet");
    pause("sheet", false);
  });
  sheet.addEventListener("click", (e) => { if (e.target === sheet) sheet.close(); }); // the backdrop
  $("#sheet-close").addEventListener("click", () => sheet.close());

  function go(path, title) {
    $("#launch-title").textContent = title || "";
    launch.setAttribute("aria-hidden", "false");
    launch.classList.add("launch--shown");
    setTimeout(() => { location.href = path; }, reduced ? 0 : 420);
  }

  // ---- The field: a slow drift of points, joined when near. Behind the door and the stage. ----

  function makeField(canvas) {
    const ctx = canvas.getContext("2d");
    let points = [];
    let frame = 0;
    let running = false;
    let w = 0, h = 0, dpr = 1;

    function size() {
      dpr = Math.min(window.devicePixelRatio || 1, 2);
      w = canvas.clientWidth; h = canvas.clientHeight;
      canvas.width = w * dpr; canvas.height = h * dpr;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
      const count = Math.round(Math.min(140, (w * h) / 11000));
      while (points.length < count) points.push(make());
      points.length = count;
    }

    function make() {
      return { x: Math.random() * w, y: Math.random() * h, vx: (Math.random() - 0.5) * 0.25, vy: (Math.random() - 0.5) * 0.25, r: 1 + Math.random() * 1.6 };
    }

    function draw() {
      if (!running) return;
      ctx.clearRect(0, 0, w, h);
      // a soft glow where the words sit
      const g = ctx.createRadialGradient(w * 0.28, h * 0.5, 0, w * 0.28, h * 0.5, Math.max(w, h) * 0.55);
      g.addColorStop(0, "rgba(14, 53, 86, 0.9)");
      g.addColorStop(1, "rgba(11, 21, 54, 0)");
      ctx.fillStyle = g;
      ctx.fillRect(0, 0, w, h);

      const reach = 110;
      for (let i = 0; i < points.length; i++) {
        const p = points[i];
        p.x += p.vx; p.y += p.vy;
        if (p.x < -10) p.x = w + 10; else if (p.x > w + 10) p.x = -10;
        if (p.y < -10) p.y = h + 10; else if (p.y > h + 10) p.y = -10;
        for (let j = i + 1; j < points.length; j++) {
          const q = points[j];
          const dx = p.x - q.x, dy = p.y - q.y;
          const d = dx * dx + dy * dy;
          if (d < reach * reach) {
            ctx.strokeStyle = "rgba(36, 224, 192, " + (0.16 * (1 - Math.sqrt(d) / reach)).toFixed(3) + ")";
            ctx.lineWidth = 1;
            ctx.beginPath(); ctx.moveTo(p.x, p.y); ctx.lineTo(q.x, q.y); ctx.stroke();
          }
        }
      }
      ctx.fillStyle = "rgba(36, 224, 192, 0.55)";
      for (const p of points) { ctx.beginPath(); ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2); ctx.fill(); }
      frame = requestAnimationFrame(draw);
    }

    function start() {
      if (running) return;
      running = true;
      size();
      window.addEventListener("resize", size);
      if (reduced) { draw(); running = false; return; } // one still frame
      draw();
    }
    function stop() {
      running = false;
      cancelAnimationFrame(frame);
      window.removeEventListener("resize", size);
    }
    return { start, stop };
  }

  const doorField = makeField($("#field"));
  const heroField = makeField($("#hero-field"));

  // The field behind the stage only drifts while the stage is in view.
  if ("IntersectionObserver" in window)
    new IntersectionObserver(([entry]) => {
      if (!menuOpen) return;
      if (entry.isIntersecting) heroField.start(); else heroField.stop();
    }, { threshold: 0.05 }).observe($(".hero"));

  boot().catch((err) => {
    console.error(err);
    say("Something went wrong loading the menu. Reload the page.", true);
  });
})();
