# Maintainer guide

## Keeping changes focused

Prefer one user-facing concern per commit. Keep generated migration output,
provider configuration, and UI changes separate when they can be reviewed
independently. Explain behavior changes in the pull request description and
include the validation command that was run.

## Reviewing external integrations

Treat SMTP, payment, Cloudinary, AI, and halal-detector calls as boundaries.
Check timeout behavior, safe error messages, logging, and configuration
fallbacks when modifying them. Do not log credentials, verification codes,
payment passwords, or complete provider responses that may contain sensitive
data.

## Reviewing database changes

Check foreign-key delete behavior, indexes, required fields, and migration
ordering. Confirm that seed data remains idempotent and that startup recovery
does not hide an actionable production failure.

## Reviewing views

Keep page-specific formatting in view models or views rather than controllers.
Use the shared layouts and validation partials. Confirm nullable navigation
properties are handled safely when related records may be absent.
