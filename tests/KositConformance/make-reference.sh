#!/usr/bin/env bash
# Recreates the references of the test suite on the JVM: reference/kosit-<version>-jvm.txt (report summaries of the
# official KoSIT validator for every document of build/manifest.txt) and reference/cen-unit-tests-jvm.txt (the CEN
# unit test expectations that Saxon does not meet with CEN's compiled Schematron).
# Run prepare-testdata.sh first. Requires a Java 11+ runtime.
set -euo pipefail
export LC_ALL=C.UTF-8

KOSIT_VERSION=1.6.3
here="$(cd "$(dirname "$0")" && pwd)"
mkdir -p "$here/reference"
java -cp "$here/build/kosit/validator-$KOSIT_VERSION-standalone.jar" "$here/ReferenceRunner.java" \
    "$here/build" "$here/reference/kosit-$KOSIT_VERSION-jvm.txt" "$here/reference/cen-unit-tests-jvm.txt"
echo "Wrote $here/reference/kosit-$KOSIT_VERSION-jvm.txt ($(grep -c '^## ' "$here/reference/kosit-$KOSIT_VERSION-jvm.txt") documents)"
