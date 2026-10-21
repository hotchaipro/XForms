# Benchmarks

## Native objects per view

This benchmark measures what a native object per view costs, to size the
gain from lightweight views (see
[alignment.md](alignment.md#6-lightweight-controls-and-layers)).

The test page (`test/XForms.Test/BenchmarkPage.cs`) shows 1,000 owner-drawn
tiles (100 rows of 10, each tile a filled and outlined square with a number),
in a scroll view, in two variants:
- **One view per tile.** Each tile is a `UserControl`, so it has its own native
  object, as every view does today. With the row layouts, that's about 1,100
  views.
- **Drawn as one view.** One `UserControl` draws all 1,000 tiles, as
  lightweight views would draw into a shared layer.

The page reports how long it took to build, and when the last tile first
finished drawing ("first frame drawn"), both measured from the start of the
page's constructor. Memory is measured from outside the process after five
seconds. The text-and-list test page is the baseline.

Choose the page with `XFORMS_TEST_PAGE` on Win32, or an intent extra on
Android (`--es page benchmark-views` or `benchmark-drawn`).

### Results

Measured on 2026-10-20, Release builds.

**Win32** (this development PC, integrated GPU), three runs each:

| | One view per tile | Drawn as one view | Baseline |
|---|---|---|---|
| Build the page | 20–26 ms | 7–10 ms | |
| First frame drawn | 344–519 ms | 168–223 ms | |
| Private memory | 26–27 MB | 21 MB | 20 MB |
| GPU shared memory | 30.7–31.0 MB | 32.6–32.9 MB | 26.6 MB |

**Android** (Pixel 7 emulator, API 36), two runs each, including three
scroll gestures for the frame statistics:

| | One view per tile | Drawn as one view | Baseline |
|---|---|---|---|
| Build the page | 357–679 ms | 97–104 ms | |
| First frame drawn | 2.6–3.1 s | 0.84–0.98 s | |
| Memory (PSS) | 68.6 MB | 50.7 MB | 37.6 MB |
| Janky frames while scrolling | 30–37% | 7–27% | 4–30% |
| Median frame time while scrolling | 40 ms | 27–34 ms | 25–34 ms |

**iOS** wasn't measured; it needs a Mac.

Emulator numbers are noisy, as the baseline's spread shows, but the
difference between the variants was consistent across runs.

### With lightweight views

Measured again on 2026-10-20 after phase 8, which made the tiles (and their
row layouts) lightweight. "Native" is the same build with lightweight views
turned off (`XFORMS_LIGHTWEIGHT=0` on Win32, `--es lightweight off` on
Android). The managed heap is measured after a full collection.

**Win32**, three runs each:

| | Lightweight | Native |
|---|---|---|
| Build the page | 20–41 ms | 26–30 ms |
| First frame drawn | 331–383 ms | 639–1,019 ms |
| Private memory | 24.3–24.6 MB | 29.4–29.5 MB |

**Android** (Pixel 7 emulator, API 36), two runs each:

| | Lightweight | Native | Drawn as one view |
|---|---|---|---|
| Native views | 63 | 1,164 | |
| Memory (PSS) | 58.3–60.6 MB | 61.0–61.2 MB | 54.9 MB |
| Managed heap | 5.2 MB | 6.9 MB | 4.5 MB |
| First frame drawn | 1.5–3.9 s | 3.3–4.8 s | 1.2–2.4 s |

The emulator's timings varied widely between sessions; within a session,
lightweight views showed the page about twice as fast. The first Android
runs also showed lightweight views using more memory: per-frame allocations
(an Android `Matrix` per translation, and an iterator per child) inflated
the heap with garbage. Translations now go straight to the canvas, and
children are walked without allocating.

### Findings

- **Android gains the most.** One native view per tile made the page take
  about three times as long to show, used about 18 MB more memory than
  drawing the tiles in one view, and scrolled with more dropped frames. The
  earlier assumption that Android would gain least, because it records
  display lists, was wrong: the cost of each `View` (measure, layout, the
  view tree itself) dominates.
- **Win32 gains in time, less in memory.** Drawing the tiles in one view
  halved the time to show the page and saved about 6 MB of process memory.
  GPU memory didn't favor it: DirectComposition packs small surfaces
  efficiently, while the single view needed one tall 640 × 6,400-pixel
  surface.
- **Large layers need tiled surfaces.** Lightweight views can't simply draw a
  whole layer's content into one surface. A tall scrolled layer needs a
  virtual or tiled surface that allocates only what's on screen
  (DirectComposition's virtual surfaces, iOS's tiled layers, and Android's
  display lists, which aren't bitmaps).
- **Not yet measured:** frame time while animating many views, which needs
  the Animator, and Win32 frame time while scrolling.
