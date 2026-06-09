# Acz

Acz is a lightweight CLI utility to **extract and verify `.zip` archives**.  
It ensures that files are decompressed correctly and validates their integrity using CRC32 checks.

## ✨ Features
- Extract multiple `.zip` files into a chosen root folder.
- Automatically creates subfolders named after each archive.
- Verifies extracted files against CRC32 values stored in the archive.
- Reports missing files, mismatched CRCs, and other extraction errors.
- Supports quoted file paths and multiple arguments.
- Provides clear error counts at the end of execution.

## 📦 Usage

```bash
Acz.exe -fs <files...> -r <rootFolder>
```

### Options

- `-fs` \<files...\>  
List of .zip files to extract. Paths may be quoted and separated by spaces.

- `-r` \<folder\>  
Root folder where archives will be extracted. Each archive gets its own subfolder.

- `-h`, `--help`, `/?`  
Show usage instructions.

### 🔧 Examples

Extract two archives into C:\Extracted:

```bash
Acz.exe -fs "C:\Downloads\archive1.zip" "C:\Downloads\archive2.zip" -r "C:\Extracted"
```

Prompt for root folder if not provided:

```bash
Acz.exe -fs "archive.zip"
```

## ⚠️ Error Handling

The tool reports warnings and errors during extraction:

`OriginFileNotFoundException = Source archive file not found.`

`ExtractedFileNotFoundException = Expected file missing after extraction.`

`ExtractedFolderNotFoundException = Destination folder missing or inaccessible.`

`EmptyCRC32Exception = Archive entry has no CRC32 stored (zero or missing).`

`MismatchCRC32Exception = Extracted file’s CRC32 does not match the archive’s stored CRC32.`

## At the end, the program prints a summary:

```Code
Extract && Verify finished.
Errors: <count>
```

## 📜 Version Info

On startup, Acz prints assembly version details:

- Assembly Version
- File Version
- Informational Version

## 🛠️ Requirements

- .NET 6.0 or later
- Windows, Linux, or macOS (cross‑platform via .NET)

## 🚀 Roadmap

- Add support for other archive formats (.tar, .xz, .7z).


## Dependencies

This project uses [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)  
for CRC32 verification of extracted ZIP files.

Install via NuGet:
```bash
dotnet add package SharpZipLib
```
