# The Unity Editor for web builds (GameCI's image of the exact version this project
# uses), plus Unity's command-line Hub, which signs in with a free Personal license.
FROM unityci/editor:ubuntu-6000.3.9f1-webgl-3

# The Unity CLI keeps its sign-in in the system keyring; a container has none, so
# unity.sh starts gnome-keyring on each run.
RUN apt-get update \
    && apt-get install -y --no-install-recommends dbus gnome-keyring libsecret-1-0 \
    && rm -rf /var/lib/apt/lists/*

RUN curl -fsSL https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh \
    | UNITY_CLI_CHANNEL=beta UNITY_CLI_VERSION=1.0.0-beta.14 UNITY_CLI_HOME=/opt/unity-cli bash
ENV PATH="/opt/unity-cli/bin:${PATH}"

COPY unity.sh /usr/local/bin/claude-bomb
ENTRYPOINT ["claude-bomb"]
