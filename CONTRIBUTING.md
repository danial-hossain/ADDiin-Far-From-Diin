# Contributing to Ad-Diin

Thank you for helping improve Ad-Diin. Keep changes focused, explain the
problem they solve, and preserve the existing user experience unless a
behavior change is intentional and documented.

## Before making changes

1. Create a branch from the current default branch.
2. Read the relevant controller, service, model, and view before editing.
3. Keep credentials, generated output, and local configuration out of commits.
4. Prefer an existing service or helper over duplicating application logic.

## Local checks

Run the checks that match the files you changed:

```powershell
dotnet restore
dotnet build
dotnet test --no-build
```

For documentation-only changes, review the rendered Markdown and confirm that
all referenced paths, commands, and configuration keys still exist.

## Commit guidance

Use short, imperative commit subjects with a clear area prefix, for example:

```text
docs: clarify local configuration
fix: validate activity registration input
feat: add scheduled hadith management
```

Keep unrelated cleanup out of a feature or fix commit. A pull request should
describe the user-facing result, list the validation performed, and call out
any required environment or database changes.
