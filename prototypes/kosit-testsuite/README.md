# KoSIT prototype test suite

Conformance tests for the prototypes that run the KoSIT validator from .NET (`prototypes/kosit-*`). They test
through a small engine interface (`src/IKositEngine.cs`: load a configuration and validate, apply an XSLT, evaluate
an XPath), so every prototype runs the same tests. Each prototype has a test project that compiles the shared sources
from `src/` and supplies its engine.

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
- the ZUGFeRD / Factur-X samples in `documentation/` (55 + 6 documents) with a Factur-X 1.09.2 configuration
  (`facturx-scenarios.xml`, built from `documentation/zugferd252`).

The XRechnung configuration itself comes from the release `validator-configuration-xrechnung_3.0.2_2026-08-31.zip`
of the configuration's GitHub releases. Unpack it and pass the directory:

```shell
XRECHNUNG_CONFIGURATION=/path/to/validator-configuration-xrechnung_3.0.2_2026-08-31 ./prepare-testdata.sh
```

Requirements: git, curl, a Java 11+ runtime and sha256sum.

Note: the committed reference was created with the configuration built from tag `v2026-08-31` of
validator-configuration-xrechnung, because the environment that created it could not download GitHub release
assets. The release is built from that tag; if the reports of every XRechnung document differ with the release,
recreate the reference with `make-reference.sh`.

## Reference

`make-reference.sh` recreates both files in `reference/` with `ReferenceRunner.java` on the JVM (needed only when
the test data changes):

- `kosit-1.6.3-jvm.txt`: the report summaries of the official KoSIT validator,
- `cen-unit-tests-jvm.txt`: the CEN unit test expectations that Saxon does not meet with CEN's compiled Schematron.
  This is one stale test: `BR-CO-25.xml` test 6 expects BR-CO-25, which is no longer part of the rules of
  release 1.3.16.

A report that differs from the reference fails the test with the differing lines. A prototype can declare documents
whose difference is understood (`KnownDifferences`); those are reported as skipped with the reason.
