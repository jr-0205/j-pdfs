# JPDFs 1.9

**A modern PDF workspace for Windows.** JPDFs brings document reading, editing, organization, conversion and protection together in one desktop application. It is designed to keep everyday PDF work in one place, with a native Windows experience and local document handling.

> JPDFs is the current product identity and name used throughout this documentation. Legacy promotional artwork and screenshots have been retired from this page so that the documentation presents the current project consistently.

## At a glance

- **Read and navigate:** open documents in tabs or split panes, search text, use bookmarks and inspect comments, attachments, layers and signatures.
- **Edit and annotate:** add text, drawings, highlights, shapes, stamps, images, measurements and reusable signatures. Edit supported text line by line or by paragraph.
- **Organize pages:** merge, split, extract, reorder, rotate, crop, resize and straighten pages. Compare two documents side by side.
- **Recognize scanned documents:** run OCR, review recognized text and save searchable copies.
- **Convert documents:** export to Word, Excel, PowerPoint, HTML, Markdown, text, JSON or images. Convert supported Word and Excel files to PDF.
- **Work with forms:** create and fill forms, recognize fields, flatten forms, and exchange form data and comments through FDF or XFDF.
- **Protect information:** redact by area, text search or sensitive-data pattern; protect copies with passwords or certificate recipients; inspect digital signatures.
- **Automate repeated work:** use guided tasks, macros, watched folders, data merge and command-line tools.
- **Personalize the workspace:** choose from 13 themes and 20 interface languages, customize shortcuts and scale the interface.

## A workspace built around documents

JPDFs combines document navigation, editing tools and page organization in a single desktop workspace. The interface supports tabbed documents, split views and a dockable sidebar. Tool availability depends on the document type and the operation being performed.

The application also includes workflows for OCR, conversion, redaction, forms, signatures and batch operations. These tools are intended to make common document tasks accessible without requiring a separate utility for every operation.

## Important limitations

Text and paragraph editing depend on the source PDF's layout and fonts. Scanned pages need OCR before their text can be edited. Exported Office documents may not preserve every detail of the original layout, so review converted files before distributing them. Signature reports distinguish failed checks from evidence that is unavailable or has not been checked.

## Get JPDFs

Downloads, release notes and the user guide are available from the [official JPDFs website](https://jpdfs.net). You can also find installers, portable packages and source archives on the [GitHub releases page](https://github.com/JPDFs/JPDFs/releases).

## Documentation

- [Product overview and capabilities](docs/PRODUCT-OVERVIEW.md)
- [Application changelog](CHANGELOG.md)
- [Contributing](CONTRIBUTING.md)
- [Engine documentation](engine/README.md)
- [Engine changelog](engine/CHANGELOG.md)
- [Translation guide](TRANSLATING.md)
- [Layered architecture and migration plan](docs/architecture/LAYERED-ARCHITECTURE.md)

## Build and test

Use Windows and the .NET SDK selected by [global.json](global.json).

```powershell
dotnet restore JPDFs.csproj
dotnet build JPDFs.csproj -c Release
dotnet test engine/JPDFs.Engine.Tests/JPDFs.Engine.Tests.csproj
dotnet test JPDFs.Tests/JPDFs.Tests.csproj
```

A few application tests open WPF windows. Run them on an unlocked desktop and follow the prerequisites in the [UI smoke guide](tests/JPDFs.UiSmoke/README.md). Build and test one checkout at a time.

## License

JPDFs is licensed under GPLv3. See [LICENSE](LICENSE) and the [1.9.0 corresponding-source archive](https://github.com/JPDFs/JPDFs/releases/download/v1.9.1/JPDFs-1.9.1-src.zip). Third-party licenses and notices ship in the application payload.
