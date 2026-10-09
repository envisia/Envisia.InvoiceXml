#!/usr/bin/env bash
# Builds the KoSIT validator as a native shared library with GraalVM native-image:
#
#   build/native/libkosit.so (Linux), libkosit.dylib (macOS) or kosit.dll (Windows, run the same steps there)
#
# Uses the unmodified official KoSIT validator jar from Maven Central (checksum verified) and the C entry points in
# java/src (KositNative.java). The reflection metadata in java/metadata was recorded with trace.sh.
# native-image cannot cross-compile: build on each target platform. Requires GRAALVM_HOME (GraalVM 25) and curl.
set -euo pipefail
export LC_ALL=C.UTF-8

KOSIT_VERSION=1.6.3
here="$(cd "$(dirname "$0")" && pwd)"
: "${GRAALVM_HOME:?set GRAALVM_HOME to a GraalVM installation (e.g. GraalVM Community 25)}"

mkdir -p "$here/build"
jar="$here/build/validator-$KOSIT_VERSION-standalone.jar"
if [ ! -f "$jar" ]; then
    url="https://repo1.maven.org/maven2/org/kosit/validator/$KOSIT_VERSION/validator-$KOSIT_VERSION-standalone.jar"
    curl -fsSL --retry 5 --retry-delay 10 -o "$jar.tmp" "$url"
    echo "$(curl -fsSL --retry 5 --retry-delay 10 "$url.sha256" | cut -d' ' -f1)  $jar.tmp" | sha256sum -c --quiet
    mv "$jar.tmp" "$jar"
fi

rm -rf "$here/build/classes" "$here/build/native"
mkdir -p "$here/build/classes" "$here/build/native"
"$GRAALVM_HOME/bin/javac" -d "$here/build/classes" -cp "$jar" "$here"/java/src/de/envisia/kosit/*.java

(cd "$here/build/native" && "$GRAALVM_HOME/bin/native-image" --shared --no-fallback -march=compatibility \
    -cp "$jar:$here/build/classes" -H:ConfigurationFileDirectories="$here/java/metadata" -o libkosit)
echo "Built $(ls "$here"/build/native/*kosit.{so,dylib,dll} 2>/dev/null)"
