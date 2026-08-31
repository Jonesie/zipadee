# Zipadee

A Visual Studio project type that produces archive files (zip, 7z, tar, self-extracting exe, ...) from the files, links, and project outputs it contains.

See the [issue tracker](https://github.com/Jonesie/zipadee/issues) for the current milestone plan (M0–M5).

## Branching

This repo uses [GitFlow](https://github.com/gittower/git-flow-next):

- `main` — released code only
- `develop` — integration branch, base for new work
- `feature/*` — one branch per feature/milestone, branched from and merged back into `develop`
- `release/*` — release stabilization, branched from `develop`, merged into `main` and `develop`
- `hotfix/*` — urgent fixes, branched from `main`, merged into `main` and `develop`
