#!/usr/bin/env bash
# Runs inside the Unity container (see compose.yaml):
#   docker compose run --rm unity login   sign in to Unity and activate the free license (once)
#   docker compose run --rm unity build   build the game named by GAME (default claude-bomb)
#                                         into site/games/<GAME>
set -euo pipefail

GAME="${GAME:-claude-bomb}"
PROJECT="/project/games/$GAME"
LOG="$PROJECT/Logs/web-build.log"

# A keyring for the Unity sign-in, saved in the unity-home volume. Its password only
# guards that volume's own keyring file, so it is made once and kept beside it.
if [ -z "${DBUS_SESSION_BUS_ADDRESS:-}" ]; then exec dbus-run-session -- "$0" "$@"; fi
export XDG_RUNTIME_DIR=/tmp/runtime-root
mkdir -p -m 700 "$XDG_RUNTIME_DIR" ~/.local/share/keyrings
[ -f ~/.keyring-pass ] || (umask 077 && head -c 24 /dev/urandom | base64 > ~/.keyring-pass)
gnome-keyring-daemon --unlock --components=secrets < ~/.keyring-pass > /dev/null

case "${1:-}" in
  login)
    unity auth login
    unity license activate --personal --accept-eula
    unity license status
    ;;
  build)
    # The license can lapse while the sign-in stays valid; renewing it needs no browser.
    unity license status >/dev/null 2>&1 || unity license activate --personal --accept-eula
    mkdir -p "$(dirname "$LOG")"
    echo "Building the web version (the first build imports every asset and takes longest)."
    echo "Full log: games/$GAME/Logs/web-build.log"
    start=$SECONDS
    status=0
    unity-editor -nographics -quit -projectPath "$PROJECT" -buildTarget WebGL \
      -executeMethod WebBuild.BuildFromCommandLine -logFile "$LOG" || status=$?
    echo "Took $(( (SECONDS - start) / 60 )) min."
    grep -h "\[WebBuild\]" "$LOG" || true
    if [ "$status" -ne 0 ]; then
      echo "Build failed (exit $status). Errors from the log:" >&2
      grep -E "error|Error|Exception|\[WebBuild\]" "$LOG" | tail -30 >&2 || true
      exit "$status"
    fi
    echo "Done. Play it with: docker compose up, then open http://localhost:8080"
    ;;
  *)
    echo "usage: docker compose run --rm unity login|build" >&2
    exit 2
    ;;
esac
