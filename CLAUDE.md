# XForma

A lightweight .NET UI framework: native text, text entry and scrolling, owner-drawn everything
else, so apps look the same on every platform. See `README.md`. SoonerOrLater
(`../SoonerOrLater/XForms`) is the app built on it, ported phase by phase alongside it.

Renamed from XForms on 2026-11-05. The repo, its folder and the Mac's clone are still called
`XForms`, until that's coordinated, and Sooner Or Later's own folder and projects keep the name.
Names apps' stored data lives under (secure storage, saved files, navigation) also keep it: each
has a comment saying so, and renaming one would lose users' data.

## Where things stand

- The plan, with each phase's status, is the "Plan" section of `docs/alignment.md`; the
  architecture is in `docs/architecture.md`. Read them before starting a phase.
- Work is coordinated by the "Hot Chai Apps coordinator" session; it's the place to plan the next
  phase with the user and to track what's verified.

## Open work

- Phases 1-28 are done. The work toward 1.0 is tracked in the "XForms 1.0 Readiness Review" doc:
  https://claude.ai/artifact/Sys3XsKDPahxbqnyeF8yCc
- Phase 29, `XForma.Xaml`, isn't being done until a real project needs it (decided 2026-11-05).
- Unverified: Win32 from the keyboard (scripted input is blocked when another window holds the
  foreground), what VoiceOver and TalkBack say, a real iPhone or iPad.

## Layout

- `framework/`: the small per-platform framework (all platform-specific code)
- `platform/`: the platform hosts (Win32, Android, iOS, web)
- `xforma/`: the multi-targeted `XForma` package apps reference; `toolkit/`, `themes/` (looks, such
  as the Basic theme), `analyzers/` (the platform contract, XF0001)
- `test/`: unit tests; `spikes/`: experiments, not shipped
- `XForma.slnx`: everything

## Working here

- Commit straight to `master` and push; no branches, no attribution lines.
- Fix behaviour on every platform, not just the one where it showed up; the design favours the
  optimal API over backward compatibility.
- iOS is tested on the Mac's simulator over SSH (see the "Mac iOS testing" memory note).
- After a change, build SoonerOrLater too (`../SoonerOrLater/SoonerOrLater.slnx`).
