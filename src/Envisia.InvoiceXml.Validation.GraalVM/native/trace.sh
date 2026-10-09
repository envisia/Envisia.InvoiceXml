#!/usr/bin/env bash
# Records the reflection and resource metadata native-image needs (metadata/) by running the code paths of the
# library on the JVM with the native-image tracing agent: validates the given documents with the built-in
# configurations (Factur-X, XRechnung, in this order), applies an XSLT stylesheet and evaluates an XPath expression.
# Run build-native.sh first (for the jar and classes).
#
# usage: trace.sh <document.xml>...
set -euo pipefail
export LC_ALL=C.UTF-8

KOSIT_VERSION=1.6.3
here="$(cd "$(dirname "$0")" && pwd)"
repo="$(cd "$here/../../.." && pwd)"
: "${GRAALVM_HOME:?set GRAALVM_HOME to a GraalVM installation}"

# the Factur-X configuration as the package unpacks it
facturx="$here/build/trace/facturx"
rm -rf "$facturx"
mkdir -p "$facturx/resources/facturx/xsd" "$facturx/resources/facturx/xslt"
cp "$here/../Configurations/facturx/scenarios.xml" "$facturx/"
cp "$here/../Configurations/xrechnung/resources/default-report.xsl" "$facturx/resources/"
cp -r "$repo/documentation/zugferd252/Schema/." "$facturx/resources/facturx/xsd/"
cp "$repo"/documentation/zugferd252/XSLT/*.xslt "$repo"/documentation/zugferd252/XSLT/*_codedb.xml "$facturx/resources/facturx/xslt/"
xrechnung="$here/../Configurations/xrechnung"
printf '%s\t%s\n%s\t%s\n' "$facturx/scenarios.xml" "$facturx" "$xrechnung/scenarios.xml" "$xrechnung" > "$here/build/trace/configurations.txt"

rm -rf "$here/metadata"
"$GRAALVM_HOME/bin/java" -agentlib:native-image-agent=config-output-dir="$here/metadata" \
    -cp "$here/build/validator-$KOSIT_VERSION-standalone.jar:$here/build/classes" \
    de.envisia.invoicexml.validation.graalvm.KositNative "$here/build/trace/configurations.txt" \
    "$facturx/resources/facturx/xslt/FACTUR-X_EN16931.xslt" "$@"
