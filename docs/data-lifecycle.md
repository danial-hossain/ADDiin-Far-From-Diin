# Data lifecycle

## Request data

Controllers receive browser or provider input and pass validated values to
services. View models describe page-specific input, while entities represent
records that can be persisted.

## Database data

Entity Framework Core stores Identity records and application data in SQL
Server. `DbInitializer` applies migrations and seeds baseline development data
when the application starts. New schema changes should be represented by a
reviewed migration.

## External-provider data

Email, payment, media, AI, and halal-detector operations cross service
boundaries. Services normalize provider responses before controllers or views
use them. Provider failures should become explicit logs and user-safe
responses, not silent success.

## User-generated content

Messages, donations, registrations, profile data, AI conversations, and worship
progress are associated with users where applicable. Authorization checks must
remain in place when loading or mutating user-owned records.

## Retention and privacy

Avoid logging passwords, OTPs, API keys, payment credentials, or unnecessary
personal data. If a future retention policy is introduced, apply it to
notifications, verification codes, messages, and AI history deliberately and
document the policy alongside the cleanup mechanism.
