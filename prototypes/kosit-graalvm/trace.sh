#!/usr/bin/env bash
# Records the reflection and resource metadata native-image needs (java/metadata) by running the code paths of
# the library on the JVM with the native-image tracing agent, over the documents of the KoSIT test suite.
# Run prototypes/kosit-testsuite/prepare-testdata.sh and build-native.sh (for the jar and classes) first.
set -euo pipefail
export LC_ALL=C.UTF-8

KOSIT_VERSION=1.6.3
here="$(cd "$(dirname "$0")" && pwd)"
: "${GRAALVM_HOME:?set GRAALVM_HOME to a GraalVM installation}"

rm -rf "$here/java/metadata"
"$GRAALVM_HOME/bin/java" -agentlib:native-image-agent=config-output-dir="$here/java/metadata" \
    -cp "$here/build/validator-$KOSIT_VERSION-standalone.jar:$here/build/classes" \
    de.envisia.kosit.KositNative "$(cd "$here/../kosit-testsuite/build" && pwd)"
