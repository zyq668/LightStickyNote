# Changelog

## Unreleased

### Added

- Added a compact top progress summary with remaining tasks, encouraging feedback, and a subtle completion animation.
- Added an in-window delete confirmation card to reduce accidental deletions.
- Added a friendlier empty state for days without tasks.
- Added optional right-edge auto-hide with a narrow reveal tab, hover-to-expand editing, and configurable delay.
- Added a packaging helper note with the exact portable publish command.
- Added long-press drag-to-reorder for tasks, including floating drag feedback, insertion indicators, and persisted sort order.

### Changed

- Updated the development launcher to rebuild current code before startup and avoid duplicate running instances.
- Updated portable packaging instructions with the exact distributable ZIP path.
- Added edge snap controls to the existing settings card.
- Changed sticky-note dragging to use app-controlled movement so Windows edge snap assist no longer interrupts right-edge hide.
- Changed edge auto-hide behavior to distinguish preview mode from active editing, so typing and title clicks are less likely to be interrupted.
- Changed task drag activation to a 300ms long press and strengthened the lifted-card visual state for clearer drag feedback.
