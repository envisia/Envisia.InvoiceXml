# Contributing

## Development

- Install the .NET 10 SDK (see `global.json`); the tests also run on .NET 8.
- Build and test:

  ```shell
  dotnet build Envisia.InvoiceXml.sln
  dotnet test Envisia.InvoiceXml.sln
  ```

- Every `.cs` file starts with the Apache license header used throughout the repository
  (checked by the `License headers` workflow).
- Changes to the writers should keep the conformance tests green:
  - `FacturXConformanceTests` validates against the official Factur-X 1.08 and 1.09.2 XSDs
    (`documentation/zugferd240en`, `documentation/zugferd252`).
  - `XRechnungConformanceTests` validates UBL output against the OASIS UBL 2.1 XSDs
    (`documentation/ubl21`).
- For business rules beyond the XSD, check generated invoices with the official Schematron
  (`documentation/zugferd252/XSLT`, XSLT 2.0, e.g. with Saxon-HE) and, for XRechnung, with the
  KoSIT validator (`documentation/xRechnung`).

## Releasing

Packages are published to nuget.org by the `Release` workflow when a tag `v<version>` is pushed:

```shell
git tag v1.0.0
git push origin v1.0.0
```

The version is taken from the tag (`v1.2.3-preview.1` creates a pre-release). The workflow builds,
tests, packs (`.nupkg` + `.snupkg`), pushes to nuget.org and creates a GitHub release with the
packages attached.

One-time setup for nuget.org [trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing):

1. On nuget.org open *Trusted Publishing* and add a policy for this repository with the workflow
   file `release.yml`.
2. Add the repository secret `NUGET_USER` with the nuget.org profile name (not the e-mail
   address) of the account that owns the package.

Update `CHANGELOG.md` before tagging a release.
