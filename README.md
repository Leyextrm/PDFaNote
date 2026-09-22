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
   ```cmd
   dotnet restore
   ```

2. **Build the project in Release mode:**
   ```cmd
   dotnet build -c Release
   ```
   
   *Alternatively, you can open PDFaNote/PDFaNoter.slnx in Visual Studio 2022 and build from the IDE.*

## Privacy Policy
PDFaNote is a local desktop application. It does not collect, store, or transmit any personal data, telemetry, or user files to any external servers. All processing is done locally on your device.
