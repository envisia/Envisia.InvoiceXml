#!/usr/bin/env bash
# Prepares the test data of the KoSIT conformance tests in build/:
#
#   build/kosit/validator-<version>-standalone.jar   official KoSIT validator from Maven Central (checksum verified)
#   build/sources/...                                 upstream test documents and expectations at pinned versions
#   build/kosit-tests                                 src/test of the XRechnung configuration, placeholders replaced
#   build/config/xrechnung                            XRechnung configuration + scenarios-test.xml (KoSIT test variant)
#   build/config/facturx                              Factur-X 1.09.2 configuration of Envisia.InvoiceXml.Validation.GraalVM
#   build/manifest.txt                                <set> TAB <id> TAB <path> of every document of the report tests
#
# The XRechnung configuration is the one Envisia.InvoiceXml.Validation.GraalVM contains (its Configurations/xrechnung);
# XRECHNUNG_CONFIGURATION selects another one, e.g. an unpacked release of
# https://github.com/itplr-kosit/validator-configuration-xrechnung.
#
# Requires git, curl, perl and a Java 11+ runtime (for the XSLT that creates the test scenarios); runs on Linux,
# macOS and in Git Bash on Windows.
set -euo pipefail
# file names with umlauts for the JVM tools on Linux
if [ "$(uname -s)" = Linux ]; then export LC_ALL=C.UTF-8; fi

# paths for Java and .NET (Windows paths in Git Bash)
case "$(uname -s)" in
    MINGW* | MSYS* | CYGWIN*) native() { cygpath -m "$1"; } ;;
    *) native() { echo "$1"; } ;;
esac
sha256() {
    if command -v sha256sum > /dev/null; then sha256sum "$1" | cut -d' ' -f1; else shasum -a 256 "$1" | cut -d' ' -f1; fi
}

KOSIT_VERSION=1.6.3
XRECHNUNG_TAG=v2026-08-31
EN16931_TAG=validation-1.3.16

here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../.." && pwd)"
build="$here/build"
package="$repo/src/Envisia.InvoiceXml.Validation.GraalVM"
XRECHNUNG_CONFIGURATION="${XRECHNUNG_CONFIGURATION:-$package/Configurations/xrechnung}"

mkdir -p "$build/kosit" "$build/sources" "$build/config"

# official KoSIT validator (used for the JVM reference and to run the XSLT below)
jar="$build/kosit/validator-$KOSIT_VERSION-standalone.jar"
if [ ! -f "$jar" ]; then
    url="https://repo1.maven.org/maven2/org/kosit/validator/$KOSIT_VERSION/validator-$KOSIT_VERSION-standalone.jar"
    curl -fsSL --retry 5 --retry-delay 10 -o "$jar.tmp" "$url"
    [ "$(curl -fsSL --retry 5 --retry-delay 10 "$url.sha256" | cut -d' ' -f1)" = "$(sha256 "$jar.tmp")" ] || { echo "checksum mismatch for $url" >&2; exit 1; }
    mv "$jar.tmp" "$jar"
fi

clone() {
    local name=$1 url=$2 tag=$3
    if [ ! -d "$build/sources/$name" ]; then
        git -c advice.detachedHead=false clone --quiet --depth 1 --branch "$tag" "$url" "$build/sources/$name"
    fi
}
clone xrechnung-testsuite https://github.com/itplr-kosit/xrechnung-testsuite "$XRECHNUNG_TAG"
clone validator-configuration-xrechnung https://github.com/itplr-kosit/validator-configuration-xrechnung "$XRECHNUNG_TAG"
clone eInvoicing-EN16931 https://github.com/ConnectingEurope/eInvoicing-EN16931 "$EN16931_TAG"

# the tests of the XRechnung configuration with @xrechnung.spec.id@ replaced, as its build does (compile-test-sources)
XRECHNUNG_SPEC_ID="urn:cen.eu:en16931:2017#compliant#urn:xeinkauf.de:kosit:xrechnung_3.0"
rm -rf "$build/kosit-tests"
cp -r "$build/sources/validator-configuration-xrechnung/src/test" "$build/kosit-tests"
export XRECHNUNG_SPEC_ID
find "$build/kosit-tests" -name '*.xml' -type f -exec perl -pi -e 's/\@xrechnung\.spec\.id\@/$ENV{XRECHNUNG_SPEC_ID}/g' {} +

# XRechnung configuration plus the test variant of its scenarios (customLevel BR-09, as in the KoSIT build)
rm -rf "$build/config/xrechnung"
cp -r "$XRECHNUNG_CONFIGURATION" "$build/config/xrechnung"
java -cp "$(native "$jar")" net.sf.saxon.Transform -s:"$(native "$build/config/xrechnung/scenarios.xml")" \
    -xsl:"$(native "$build/sources/validator-configuration-xrechnung/src/create-test-scenario.xsl")" \
    -o:"$(native "$build/config/xrechnung/scenarios-test.xml")"

# Factur-X configuration as Envisia.InvoiceXml.Validation.GraalVM unpacks it
fx="$build/config/facturx"
rm -rf "$fx"
mkdir -p "$fx/resources/facturx/xsd" "$fx/resources/facturx/xslt"
cp "$package/Configurations/facturx/scenarios.xml" "$fx/scenarios.xml"
cp -r "$repo/documentation/zugferd252/Schema/." "$fx/resources/facturx/xsd/"
cp "$repo"/documentation/zugferd252/XSLT/*.xslt "$repo"/documentation/zugferd252/XSLT/*_codedb.xml "$fx/resources/facturx/xslt/"
cp "$XRECHNUNG_CONFIGURATION/resources/default-report.xsl" "$fx/resources/default-report.xsl"

# documents of the report tests: <set> TAB <id> TAB <path>, ids relative to build/sources or the repository
manifest="$build/manifest.txt.tmp"
: > "$manifest"
add() {
    local set=$1 base=$2 prefix=$3
    shift 3
    (cd "$base" && find "$@" -name '*.xml' -type f | LC_ALL=C sort) | while read -r path; do
        printf '%s\t%s\t%s\n' "$set" "$prefix/${path#./}" "$(native "$base/${path#./}")" >> "$manifest"
    done
}
add xrechnung "$build/sources/xrechnung-testsuite" xrechnung-testsuite src/test/business-cases src/test/technical-cases
add xrechnung "$build/kosit-tests" validator-configuration-xrechnung/src/test instances integration cen-unit-test
add xrechnung "$build/sources/eInvoicing-EN16931" eInvoicing-EN16931 ubl/examples cii/examples
add facturx "$repo/documentation" documentation zugferd240en/Examples zugferd211de/Beispiele zugferd21de/Beispiele
mv "$manifest" "$build/manifest.txt"
echo "Prepared $(wc -l < "$build/manifest.txt") documents in $build"
