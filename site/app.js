// Maaloomatiia Games: the menu page.
//
// Two states. The door (dark cover) asks for the class code; the menu (light) lists the
// games and opens them. The code goes to /api/session, which answers with a cookie that
// the games' own Claude calls then carry, so a game never sees the code. /api/me says
// whether that cookie is still good, so a returning student skips the door.
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
  const grid = $("#games");
  const template = $("#card-template");
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

  // ---- Boot ----

  async function boot() {
    const [me, catalog] = await Promise.all([json("/api/me"), json("games.json")]);
    render(catalog.data.games || []);
    if (me.data.signedIn && !forceSignIn) {
      if (returnTo) return go(returnTo, titleFor(returnTo));
      openDoor(true);
    } else {
      field.start();
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

  function openDoor(instant) {
    menu.setAttribute("aria-hidden", "false");
    if (instant || reduced) {
      door.classList.add("door--open", "door--gone");
      menu.classList.add("menu--shown");
      field.stop();
      return;
    }
    door.classList.add("door--open");
    setTimeout(() => menu.classList.add("menu--shown"), 150);
    setTimeout(() => { door.classList.add("door--gone"); field.stop(); }, 1000);
  }

  $("#signout").addEventListener("click", async () => {
    await json("/api/session", { method: "DELETE" });
    location.replace("/");
  });

  // ---- The menu ----

  let games = [];

  function render(list) {
    games = list;
    grid.replaceChildren();
    for (const game of games) {
      const node = template.content.firstElementChild.cloneNode(true);
      const ready = game.status === "ready" && game.path;
      node.classList.add(ready ? "card--ready" : "card--soon");
      node.dataset.id = game.id;
      if (game.tone === "light") node.classList.add("card--light");

      const cover = node.querySelector(".card__cover");
      const img = node.querySelector(".card__img");
      img.src = game.cover;
      img.alt = "";
      node.querySelector(".card__num").textContent = game.number;
      node.querySelector(".card__topic").textContent = game.topic;
      node.querySelector(".card__title").textContent = game.title;
      node.querySelector(".card__pitch").textContent = game.pitch;
      node.querySelector(".card__about").textContent = game.about;
      node.querySelector(".card__course").textContent = game.course || "To be announced";
      node.querySelector(".card__time").textContent = "About " + game.minutes + " min";
      node.querySelector(".card__players").textContent = game.players;

      const actions = node.querySelector(".card__actions");
      if (ready) {
        cover.href = game.path;
        cover.setAttribute("aria-label", "Play " + game.title);
        cover.addEventListener("click", (e) => { e.preventDefault(); go(game.path, game.title); });
        const play = document.createElement("a");
        play.className = "btn btn--primary";
        play.href = game.path;
        play.textContent = "Play";
        play.addEventListener("click", (e) => { e.preventDefault(); go(game.path, game.title); });
        actions.appendChild(play);
        tilt(cover, img);
        checkBuild(game, node, play);
      } else {
        cover.removeAttribute("href");
        cover.setAttribute("role", "img");
        cover.setAttribute("aria-label", game.title + ", in the works");
        const status = document.createElement("span");
        status.className = "card__status";
        status.textContent = "In the works";
        actions.appendChild(status);
      }
      grid.appendChild(node);
    }
  }

  // On a computer where a game has not been built yet, say so instead of a dead link.
  async function checkBuild(game, node, play) {
    try {
      const res = await fetch(game.path + "index.html", { method: "HEAD", credentials: "same-origin" });
      if (res.ok) return;
    } catch (e) { /* treated as missing */ }
    play.remove();
    node.classList.remove("card--ready");
    node.classList.add("card--soon");
    const status = document.createElement("span");
    status.className = "card__status";
    status.textContent = "Not built on this computer yet";
    node.querySelector(".card__actions").appendChild(status);
    const cover = node.querySelector(".card__cover");
    cover.removeAttribute("href");
    cover.replaceWith(cover.cloneNode(true)); // drops the click handler
  }

  // The cover leans a little towards the pointer: a mouse-only touch of depth.
  function tilt(cover, img) {
    if (reduced || !window.matchMedia("(hover: hover) and (pointer: fine)").matches) return;
    cover.addEventListener("pointermove", (e) => {
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

  function go(path, title) {
    $("#launch-title").textContent = title || "";
    launch.setAttribute("aria-hidden", "false");
    launch.classList.add("launch--shown");
    setTimeout(() => { location.href = path; }, reduced ? 0 : 420);
  }

  // ---- The field behind the door: a slow drift of points, joined when near ----

  const field = (function () {
    const canvas = $("#field");
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
      // a soft glow where the title sits
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
  })();

  boot().catch((err) => {
    console.error(err);
    say("Something went wrong loading the menu. Reload the page.", true);
  });
})();
