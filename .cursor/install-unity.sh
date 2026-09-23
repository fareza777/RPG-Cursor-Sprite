#!/usr/bin/env bash
# Idempotent Cloud Agent setup for the Emberwake Unity project.
# Installs the Unity 6000.6.1f1 Editor (Linux, with bundled Linux Standalone
# build support) plus Unity Hub so agents can compile, run and build the game
# in batch mode. License activation is a one-time interactive step (see notes
# at the bottom of this file).
set -euo pipefail

UNITY_VERSION="6000.6.1f1"
UNITY_CHANGESET="7efac9f6c10e"
UNITY_ROOT="/opt/unity"
EDITOR_DIR="${UNITY_ROOT}/editors/${UNITY_VERSION}"
EDITOR_BIN="${EDITOR_DIR}/Editor/Unity"
DL_DIR="${UNITY_ROOT}/downloads"
TARBALL="${DL_DIR}/Unity-${UNITY_VERSION}.tar.xz"
EDITOR_URL="https://download.unity3d.com/download_unity/${UNITY_CHANGESET}/LinuxEditorInstaller/Unity-${UNITY_VERSION}.tar.xz"

SUDO=""
if [ "$(id -u)" -ne 0 ]; then SUDO="sudo"; fi

echo "==> Installing system dependencies for the Unity Editor"
export DEBIAN_FRONTEND=noninteractive
$SUDO apt-get update -y
$SUDO apt-get install -y --no-install-recommends \
  ca-certificates curl gnupg xz-utils \
  libgtk-3-0 libnss3 libxss1 libasound2t64 libgbm1 libnspr4 libxtst6 \
  libxrandr2 libxcursor1 libxi6 libxcomposite1 libxdamage1 libxfixes3 \
  libcups2 libdrm2 libglu1-mesa libunwind8 libssl3 clang \
  libgl1 libglx-mesa0 xvfb scrot \
  libnotify4 libatspi2.0-0 libsecret-1-0

echo "==> Preparing ${UNITY_ROOT}"
$SUDO mkdir -p "${UNITY_ROOT}/editors" "${DL_DIR}" "${UNITY_ROOT}/out"
$SUDO chown -R "$(id -u):$(id -g)" "${UNITY_ROOT}"

if [ ! -x "${EDITOR_BIN}" ]; then
  echo "==> Downloading Unity ${UNITY_VERSION} Editor (~4.2 GB, one-time)"
  curl -L -C - -o "${TARBALL}" "${EDITOR_URL}"
  echo "==> Extracting Unity Editor (~9.3 GB installed)"
  mkdir -p "${EDITOR_DIR}"
  tar -xf "${TARBALL}" -C "${EDITOR_DIR}"
  rm -f "${TARBALL}"
else
  echo "==> Unity Editor already present at ${EDITOR_BIN}, skipping download"
fi

"${EDITOR_BIN}" -version >/dev/null 2>&1 || {
  echo "Unity editor binary check via -version"; "${EDITOR_BIN}" -batchmode -nographics -quit -version || true; }

echo "==> Installing Unity Hub (for GUI sign-in / license activation)"
if ! command -v unityhub >/dev/null 2>&1; then
  curl -sL https://hub.unity3d.com/linux/keys/public | gpg --dearmor \
    | $SUDO tee /usr/share/keyrings/Unity_Technologies_ApS.gpg >/dev/null
  echo 'deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main' \
    | $SUDO tee /etc/apt/sources.list.d/unityhub.list >/dev/null
  $SUDO apt-get update -y
  $SUDO apt-get install -y unityhub
fi

echo "==> Registering Editor with Unity Hub"
export DISPLAY="${DISPLAY:-:1}"
unityhub --headless install-path --set "${UNITY_ROOT}/editors" 2>/dev/null || true
unityhub --headless editors --add "${EDITOR_BIN}" 2>/dev/null || true

echo "==> Unity setup complete: ${EDITOR_BIN}"
cat <<'NOTE'

------------------------------------------------------------------------
LICENSE (one-time, interactive):
Unity refuses to compile/build in batch mode until a license is activated.
On the Cloud Agent virtual desktop (DISPLAY=:1) run `unityhub`, sign in to
your Unity account (Google sign-in works), and Unity grants a free Personal
license automatically. The entitlement is cached under
  ~/.config/unity3d/Unity/licenses/UnityEntitlementLicense.xml
after which headless batch-mode builds work, e.g.:

  /opt/unity/editors/6000.6.1f1/Editor/Unity -batchmode -nographics -quit \
    -projectPath Emberwake -executeMethod <YourEditorBuildMethod> \
    -logFile /dev/stdout

Alternatively provide UNITY_EMAIL / UNITY_PASSWORD (+ UNITY_SERIAL for
Pro/Plus, or UNITY_LICENSE .ulf contents for Personal) as secrets to
activate non-interactively with the Unity licensing client.
------------------------------------------------------------------------
NOTE
