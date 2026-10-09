# KoSIT conformance tests

Conformance tests for running the KoSIT validator from .NET, used by
`tests/Envisia.InvoiceXml.Validation.GraalVM.Tests` (the native library of `Envisia.InvoiceXml.Validation.GraalVM`).
They test through a small engine interface (`src/IKositEngine.cs`: load a configuration and validate, apply an
XSLT, evaluate an XPath), so another engine can run the same tests: a test project compiles the shared sources from
`src/` and supplies its engine.

```shell
tests/KositConformance/prepare-testdata.sh
dotnet test tests/Envisia.InvoiceXml.Validation.GraalVM.Tests
```

Without the prepared test data the tests are skipped (inconclusive).

| Test | Oracle | Cases |
|---|---|---|
| `ReportMatchesJvmReference` | The report of the official KoSIT validator 1.6.3 on the JVM (`reference/kosit-1.6.3-jvm.txt`): validity, scenario, assessment, steps and every message's id, code, level, location and text | 230 documents |
| `KositConfigurationAssertionsHold` | The tests of the XRechnung configuration itself: `assertions.xml` (instances, test scenarios with `customLevel` BR-09) and `assertions-integration-testing.xml` (integration documents), as KoSIT's build runs them | 327 assertions on 25 reports |
| `CenUnitTestsPass` | The unit tests of the CEN EN 16931 artefacts (vefa format) with CEN's compiled Schematron, compared with Saxon on the JVM (`reference/cen-unit-tests-jvm.txt`) | 1,146 test cases in 280 files |
| `ManifestMatchesReference` | The prepared documents are the ones the reference was made from | 1 |
| `ConcurrentValidationsMatchSequentialOnes` | Results under 4 parallel threads equal sequential results | 48 validations |

The tests run in parallel (4 workers), which also exercises the thread safety of the engine.

## Test data

`prepare-testdata.sh` fetches everything at pinned versions into `build/`:

- the official KoSIT validator 1.6.3 from Maven Central (`org.kosit:validator`, checksum verified), used for the JVM
  reference and to create the test scenarios,
- [xrechnung-testsuite](https://github.com/itplr-kosit/xrechnung-testsuite) `v2026-08-31`: 86 business and
  technical cases,
- [validator-configuration-xrechnung](https://github.com/itplr-kosit/validator-configuration-xrechnung)
  `v2026-08-31`: its `src/test` (instances, integration and CEN unit test documents, assertions), with
  `@xrechnung.spec.id@` replaced as the configuration's build does, and the test variant of its scenarios
  (`create-test-scenario.xsl`),
- [eInvoicing-EN16931](https://github.com/ConnectingEurope/eInvoicing-EN16931) `validation-1.3.16`: UBL and CII
  examples, the unit tests and CEN's compiled Schematron,
- the ZUGFeRD / Factur-X samples in `documentation/` (55 + 6 documents) with the Factur-X 1.09.2 configuration of
  `Envisia.InvoiceXml.Validation.GraalVM` (`Configurations/facturx/scenarios.xml` with `documentation/zugferd252`).

The XRechnung configuration is the one the package contains (`Configurations/xrechnung`, XRechnung 3.0.2,
2026-08-31). To test another one, e.g. an unpacked release of the configuration, pass its directory:

```shell
XRECHNUNG_CONFIGURATION=/path/to/validator-configuration-xrechnung_3.0.2_2026-08-31 ./prepare-testdata.sh
```

Requirements: git, curl, perl and a Java 11+ runtime; runs on Linux, macOS and in Git Bash on Windows.

Note: the configuration in the package and the committed reference were built from tag `v2026-08-31` of
validator-configuration-xrechnung, because the environment that created them could not download GitHub release
assets. The release `validator-configuration-xrechnung_3.0.2_2026-08-31.zip` is built from that tag; if the reports
of every XRechnung document differ with the release, recreate the reference with `make-reference.sh`.

## Reference

`make-reference.sh` recreates both files in `reference/` with `ReferenceRunner.java` on the JVM (needed only when
the test data changes):

- `kosit-1.6.3-jvm.txt`: the report summaries of the official KoSIT validator,
- `cen-unit-tests-jvm.txt`: the CEN unit test expectations that Saxon does not meet with CEN's compiled Schematron.
  This is one stale test: `BR-CO-25.xml` test 6 expects BR-CO-25, which is no longer part of the rules of
  release 1.3.16.

A report that differs from the reference fails the test with the differing lines. An engine can declare documents
whose difference is understood (`KnownDifferences`); those are reported as skipped with the reason. The GraalVM
library declares none: its reports equal those of KoSIT on the JVM.
