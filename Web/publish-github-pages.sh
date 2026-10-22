#!/usr/bin/env bash
# Publishes the web build (Web/public) to GitHub Pages:
#   https://mono-vloch.github.io/ClaudeBomb/
#
# Steps for an update:
#   1. In Unity: save the scene (Ctrl+S), then Claude Bomb > Build Web Version.
#   2. Run this script from Git Bash:  bash Web/publish-github-pages.sh
#
# The gh-pages branch only ever holds the latest build, so it is replaced on each
# publish (force push) instead of growing by ~70 MB per update. Nothing else in the
# repository is touched. The Claude key keeper stays on Netlify and is not affected.
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
BUILD_DIR="$PROJECT_DIR/Web/public"
REPO_URL="https://github.com/Mono-vloch/ClaudeBomb.git"
WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

if [ ! -f "$BUILD_DIR/index.html" ]; then
  echo "No web build found in Web/public. In Unity: Claude Bomb > Build Web Version." >&2
  exit 1
fi

# Never publish an API key: in a web build every file is public.
if grep -rqs "sk-ant-" "$BUILD_DIR"; then
  echo "Refusing to publish: something in Web/public looks like an Anthropic API key." >&2
  exit 1
fi

cp -r "$BUILD_DIR/." "$WORK_DIR/"
touch "$WORK_DIR/.nojekyll" # serve every file as-is (no Jekyll processing)

cd "$WORK_DIR"
git init -q -b gh-pages
git add -A
git commit -q -m "Publish the Claude Bomb web build" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
git remote add origin "$REPO_URL"
git config http.postBuffer 524288000
git config http.version HTTP/1.1 # large pushes survive flaky connections better than over HTTP/2
for attempt in 1 2 3; do
  if git push --force origin gh-pages; then break; fi
  if [ "$attempt" = 3 ]; then echo "Push failed 3 times - check the internet connection and run this again." >&2; exit 1; fi
  echo "Push attempt $attempt failed, retrying..." >&2
done

echo
echo "Published. GitHub Pages updates within a minute or two:"
echo "  https://mono-vloch.github.io/ClaudeBomb/"
