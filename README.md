📂 Directory Synchronizer in C#
📖 Overview
This C# console application ensures that a replica directory is always an exact, up-to-date copy of a specified source directory. It supports recursive synchronization, meaning that it copies not just files in the root of the source directory, but also every subfolder and file contained within, no matter how deeply nested.

🛠️ How It Works
User Input at Runtime:
When the program starts, it prompts the user to:

Provide the source directory path (this directory must already exist).

Provide the replica directory path (if it doesn’t exist, it will be created automatically).

Provide a log file path (if it doesn’t exist, it will be created. Any provided filename is automatically given a .txt extension if not already present).

Set a synchronization interval (in seconds), defining how often the source and replica directories are compared and synchronized.

Recursive Synchronization:
This isn't a simple file sync. The program:

Recursively traverses all folders and subfolders within the source directory.

Ensures every file and subfolder in the source exists and is up to date in the replica.

Uses file content comparison via MD5 hashes to detect changes, ensuring even if a file's timestamp hasn't changed but its content has, it will still be updated.

Deletion Handling:
If a file or folder is removed from the source, the corresponding item in the replica will also be removed.

Real-time Logging:
Every operation (creation, update, or deletion of files) is logged with a timestamp into the specified log file, as well as printed to the console.

Configurable Timer:
Synchronization runs periodically at a user-defined interval, controlled by a System.Timers.Timer. The program remains active until the user presses q to quit.

---

## Tecnologias

- Linguagem: C#
- Framework: .NET 7
- IDE: Visual Studio Code

---

## Como usar

Terminal:
1. Clone the repository.
   "git clone https://github.com/cesarroncon/Task_Test01.git"
2. Run
   "dotnet run"

---
