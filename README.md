# WSL-Tools

A collection of utilities for working in WSL

## Commands

### VS Code

`c <search>`

Open a directory or workspace in Visual Studio Code. This requires the **WSL Distro Name** and **Git Folder** settings to be set correctly.

Note that this command supports partial matches, e.g., using `c dfi` would match a directory named `dotfiles`.

### Update

`wt`

Fetch a list of available releases. If a version is selected, it will be downloaded and installed. This requires the **GitHub API Token** setting to be set correctly.

## Settings

| Setting                         | Default value | Description                                             |
| ------------------------------- | :------------ | ------------------------------------------------------- |
| **WSL Distro Name**             | `Ubuntu`      | The name of the Linux distribution to use               |
| **Git Folder**                  | `/git`        | Where to look for Git repositories                      |
| **GitHub API Token**            | `""`          | A GitHub API token for querying GitHub                  |
| **Use SSH for GitHub cloning?** | `false`       | When cloning a repository, should it be cloned via SSH? |
