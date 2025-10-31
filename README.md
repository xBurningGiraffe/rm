# rm — A Minimal Cross-Platform `rm` Clone for Windows and .NET

A lightweight C# implementation of the Unix `rm` command for Windows, built to behave consistently across platforms.  
It supports recursive deletion, force deletion, wildcard expansion, and standard POSIX-style flags (`-r`, `-f`, `-rf`, `--help`).

This is **not a wrapper for PowerShell’s `Remove-Item`**, but a standalone executable designed for developers who prefer native Unix-style tools on Windows.

---

## 🧰 Features

- Supports `-r`, `-f`, and combined flags (`-rf`, `-fr`)
- Handles multiple files and directories in one command
- Expands wildcards (`*`, `?`) like Bash
- Honors `--` to stop flag parsing
- Prints human-readable error messages
- Safe default behavior: refuses to delete directories unless `-r` is provided
- `--help` flag for built-in usage info
- Single-file binary (`rm.exe`) built via .NET Publish

---

## Basic Install with pre-compiled binary (via PowerShell window with elevated privileges)
- Add the following to your $PROFILE (removes existing PowerShell alias for rm):
    - Remove-Item Alias:rm -Force
- Open a new elevated PowerShell window and run:

```iwr -uri https://github.com/xBurningGiraffe/rm/releases/download/main/rm.exe -OutFile rm.exe
```
- In a new PowerShell window, test it using ```rm -h``` to reveal the help menu

## ⚙️ Usage

```bash
rm [OPTION]... FILE...
