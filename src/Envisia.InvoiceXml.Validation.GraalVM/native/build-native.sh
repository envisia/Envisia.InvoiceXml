#!/usr/bin/env bash
# Builds the native KoSIT library of Envisia.InvoiceXml.Validation.GraalVM for the current platform:
#
#   ../runtimes/<rid>/native/libkosit.so | libkosit.dylib | kosit.dll
#
# Compiles the unmodified official KoSIT validator jar from Maven Central (checksum verified) with the C entry points
# in java/ ahead of time with GraalVM native-image, using the reflection metadata in metadata/ (trace.sh).
# native-image cannot cross-compile, so this runs on each platform (see .github/workflows/graalvm.yml).
# Requires GRAALVM_HOME (GraalVM 25) and curl; on Windows Git Bash and the Visual Studio C++ build tools.
set -euo pipefail
# file names with umlauts for the JVM tools on Linux
if [ "$(uname -s)" = Linux ]; then export LC_ALL=C.UTF-8; fi

KOSIT_VERSION=1.6.3
here="$(cd "$(dirname "$0")" && pwd)"
: "${GRAALVM_HOME:?set GRAALVM_HOME to a GraalVM installation (e.g. GraalVM Community 25)}"

case "$(uname -s)-$(uname -m)" in
    Linux-x86_64) rid=linux-x64; library=libkosit; file=libkosit.so ;;
    Linux-aarch64) rid=linux-arm64; library=libkosit; file=libkosit.so ;;
    Darwin-arm64) rid=osx-arm64; library=libkosit; file=libkosit.dylib ;;
    Darwin-x86_64) rid=osx-x64; library=libkosit; file=libkosit.dylib ;;
    MINGW*-x86_64 | MSYS*-x86_64 | CYGWIN*-x86_64) rid=win-x64; library=kosit; file=kosit.dll ;;
    *) echo "unsupported platform $(uname -s)-$(uname -m)" >&2; exit 1 ;;
esac

# paths and the class path separator for the Java tools
if [ "$rid" = win-x64 ]; then
    GRAALVM_HOME="$(cygpath -u "$GRAALVM_HOME")"
    native() { cygpath -w "$1"; }
    separator=';'
    exe=.exe
    cmd=.cmd
else
    native() { echo "$1"; }
    separator=':'
    exe=
    cmd=
fi

build="$here/build"
mkdir -p "$build"
jar="$build/validator-$KOSIT_VERSION-standalone.jar"
if [ ! -f "$jar" ]; then
    url="https://repo1.maven.org/maven2/org/kosit/validator/$KOSIT_VERSION/validator-$KOSIT_VERSION-standalone.jar"
    curl -fsSL --retry 5 --retry-delay 10 -o "$jar.tmp" "$url"
    expected="$(curl -fsSL --retry 5 --retry-delay 10 "$url.sha256" | cut -d' ' -f1)"
    if command -v sha256sum > /dev/null; then
        actual="$(sha256sum "$jar.tmp" | cut -d' ' -f1)"
    else
        actual="$(shasum -a 256 "$jar.tmp" | cut -d' ' -f1)"
    fi
    [ "$expected" = "$actual" ] || { echo "checksum mismatch for $url" >&2; exit 1; }
    mv "$jar.tmp" "$jar"
fi

rm -rf "$build/classes" "$build/image"
mkdir -p "$build/classes" "$build/image"
"$GRAALVM_HOME/bin/javac$exe" -encoding UTF-8 -d "$(native "$build/classes")" -cp "$(native "$jar")" \
    "$(native "$here/java/de/envisia/invoicexml/validation/graalvm/KositNative.java")"

(cd "$build/image" && "$GRAALVM_HOME/bin/native-image$cmd" --shared --no-fallback -march=compatibility \
    -cp "$(native "$jar")$separator$(native "$build/classes")" \
    -H:ConfigurationFileDirectories="$(native "$here/metadata")" -o "$library")

target="$here/../runtimes/$rid/native"
mkdir -p "$target"
cp "$build/image/$file" "$target/$file"
echo "Built $target/$file"
