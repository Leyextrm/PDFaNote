# PDFaNote

PDFaNote is a Windows application for taking notes and making annotations on PDF documents.

## License

This project is licensed under the **GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later)**.
See the [LICENSE](LICENSE) file for the full text.

### Third-Party Licenses
This project uses **iText 9.7.0**, which is dual-licensed (AGPLv3 or Commercial). This project uses the AGPLv3 license.
For a complete list of third-party libraries and their licenses, please see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## Source Code

The public source code repository is located at:
https://github.com/Leyextrm/PDFaNote

## Development & Build Environment

- **.NET SDK**: 10.0 (or newer)
- **Visual Studio**: Visual Studio 2022 (with .NET desktop development and Windows App SDK components)
- **Target OS**: Windows 10 (Version 1809, Build 17763) or newer

### How to Build

1. **Restore NuGet dependencies:**
   `cmd
   dotnet restore
   `

2. **Build the project in Release mode:**
   `cmd
   dotnet build -c Release
   `
   
   *Alternatively, you can open PDFaNoter/PDFaNoter.slnx in Visual Studio 2022 and build from the IDE.*

## Release Procedure

To publish a new version to the Microsoft Store and GitHub:

1. Determine the new version number (e.g., 1.0.0). Update Package.appxmanifest version accordingly.
2. Commit the changes.
   `cmd
   git commit -am "Bump version to 1.0.0"
   `
3. Create a Git tag for the release.
   `cmd
   git tag v1.0.0
   git push origin main v1.0.0
   `
4. Create a **GitHub Release** for 1.0.0.
5. Package the MSIX from Visual Studio or CLI and upload it to the **Microsoft Store**.
6. Ensure the Microsoft Store version always matches the corresponding GitHub tag.

