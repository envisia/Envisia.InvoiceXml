#!/usr/bin/env bash
# Builds the KoSIT validator for Java 8 class files, the only class file version IKVM can convert.
#
# Clones the KoSIT validator at the release tag, applies kosit-java8.patch (Java 8 target, JAXB 3 instead of
# JAXB 4, Java 8 replacements for three Java 9+ API calls and loops instead of two stream pipelines that IKVM
# translates into invalid IL) and copies the standalone jar to lib/kosit-validator-java8.jar.
#
# Requires git, a JDK (11 or newer) and Maven.
set -euo pipefail

KOSIT_VERSION="${KOSIT_VERSION:-v1.6.3}"
here="$(cd "$(dirname "$0")" && pwd)"

rm -rf "$here/build/validator"
git clone --quiet --depth 1 --branch "$KOSIT_VERSION" https://github.com/itplr-kosit/validator "$here/build/validator"
git -C "$here/build/validator" apply "$here/kosit-java8.patch"

(cd "$here/build/validator" && mvn -B -q -DskipTests -Denforcer.skip=true -Ddependency-check.skip=true \
    -Djacoco.skip=true -Dformatter.skip=true package)

mkdir -p "$here/lib"
cp "$here"/build/validator/target/validator-*-standalone.jar "$here/lib/kosit-validator-java8.jar"
echo "Built $here/lib/kosit-validator-java8.jar"
