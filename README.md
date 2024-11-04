# WSL-Tools

A collection of utilities for working in WSL

==================

A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).

## Commands

### VS Code

`c <search>`

Open a directory or workspace in Visual Studio Code. This requires the **WSL Distro Name** and **Git Folder** settings to be set correctly.

Note that this command supports partial matches, e.g., using `c dfi` would match a directory named `dotfiles`.

## Settings

| Setting                         | Default value | Description                                             |
| ------------------------------- | :------------ | ------------------------------------------------------- |
| **WSL Distro Name**             | `Ubuntu`      | The name of the Linux distribution to use               |
| **Git Folder**                  | `/git`        | Where to look for Git repositories                      |
| **Use SSH for GitHub cloning?** | `false`       | When cloning a repository, should it be cloned via SSH? |
