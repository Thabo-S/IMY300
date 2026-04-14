# IMY300: Coding
## Branching Strategy
### Long-lived Branches

- **`main`** — Always stable and production-ready. 
- **`coding`** — Main integration branch for all programming work. New code features and bug fixes are merged here.

### Branch Naming Convention

- **Coding work**: `coding/<description>`  
  Examples:  
  - `coding/player-combat`  
  - `coding/feature/inventory-system`  
  - `coding/bugfix/crash-on-save`

When **`coding`** is stable and tested, it is merged into **`main`** .

### Rules

- Never push directly to `main`, `coding`, or `written`.
- All changes must go through Pull Requests with code/content review.
- Use clear commit messages and descriptive branch names.
- Keep branches short-lived where possible.

This strategy helps us maintain a clean `main` branch while supporting parallel development between code and story teams.
