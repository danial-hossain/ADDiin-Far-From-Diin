# API reference

This document describes the application endpoints used by the Diin AI and
Product Analyzer pages. Unless noted otherwise, responses are JSON and errors
use the HTTP status returned by the controller.

## Diin AI

### `POST /DiinAI/Chat`

Lightweight chat endpoint used by the basic chat flow.

Request:

```json
{
  "message": "What is the importance of salah?",
  "history": [
    {
      "role": "user",
      "content": "What is wudu?",
      "timestamp": "2026-01-01T10:00:00Z"
    }
  ]
}
```

An empty `message` returns a JSON error with `success: false`. A successful
response contains `response`, normalized `sources`, `contactFallback`, and a
UTC `timestamp`.

### `POST /api/ai/ask`

Sends a question to the AI backend and, for an authenticated user, stores the
exchange in the selected conversation.

Request:

```json
{
  "query": "What invalidates wudu?",
  "conversationId": 12
}
```

`conversationId` is optional. When omitted, the user's active conversation is
used or created. Guests can use the endpoint, but their exchange is not saved
to the database. The controller sends at most the latest 20 saved messages as
history.

Successful response:

```json
{
  "answer": "…",
  "sources": [
    {
      "id": "source-1",
      "source": "Knowledge Base",
      "reference": "…",
      "text": "…"
    }
  ],
  "conversationId": 12,
  "contactFallback": false
}
```

The `contactFallback` flag is true when the service marks the answer with its
contact-admin fallback marker. An empty query returns `400 Bad Request`.

### `GET /api/ai/history`

Returns the authenticated user's active conversation. Use the optional
`conversationId` query parameter to request a specific conversation owned by
that user:

```text
/api/ai/history?conversationId=12
```

Unauthenticated callers receive `authenticated: false` and an empty message
array. Saved messages include their role, content, timestamps, and serialized
source data.

### `GET /api/ai/conversations`

Returns conversation summaries for the signed-in user's history sidebar,
including ID, title, update time, and message count. Guests receive an
unauthenticated response with an empty list.

### `POST /api/ai/new-chat`

Marks the user's active conversations inactive. The next authenticated
question starts a new conversation. The response is:

```json
{ "success": true }
```

### `GET /api/ai/health`

Checks connectivity to the configured Diin AI backend and returns:

```json
{
  "connected": true,
  "details": "…",
  "timestamp": "2026-01-01T10:00:00Z"
}
```

## Product Analyzer

### `POST /api/product-analyzer/analyze`

Analyzes an uploaded ingredient-label image. Send the request as
`multipart/form-data` with an `image` field. Supported formats are JPG, PNG,
and WEBP; the service rejects empty files and files larger than 10 MB.

Example with `curl`:

```powershell
curl.exe -X POST `
  -F "image=@ingredients.jpg" `
  https://localhost:7000/api/product-analyzer/analyze
```

The response is a `HalalDetectorResult` containing a normalized `status`,
optional OCR data, decision details, explanation, and detected ingredients.
Typical statuses are `HARAM_DETECTED`, `MUSHBOOH_DETECTED`,
`NO_HARAM_MATCH`, and `INSUFFICIENT_OCR`.

### `POST /api/product-analyzer/analyze-text`

Analyzes manually entered ingredient text.

Request:

```json
{
  "text": "sugar, wheat flour, sunflower oil, salt"
}
```

The text must be non-empty and at least three characters long. The controller
returns `400 Bad Request` for invalid input or a service-level analysis error.

### `GET /api/product-analyzer/health`

Checks connectivity to the configured halal-detector backend:

```json
{
  "connected": true,
  "details": "…",
  "timestamp": "2026-01-01T10:00:00Z"
}
```

## Configuration

The application resolves the external backend URLs from environment variables
or nested configuration:

| Integration | Preferred configuration |
| --- | --- |
| Diin AI | `DIIN_AI_BACKEND_URL` or `DiinAI:BackendUrl` |
| Halal detector | `HALAL_DETECTOR_BACKEND_URL` or `HalalDetector:BackendUrl` |
| HTTP timeout | `AISettings:TimeoutSeconds` |

Use double underscores for nested environment-variable names, for example
`DiinAI__BackendUrl`. Keep provider URLs and credentials in deployment secrets
or environment configuration rather than committing private values.

## Error handling

The controllers return validation errors as JSON and log unexpected failures
server-side. External-provider timeouts and connection failures are normalized
by the service layer into user-safe messages; raw credentials and request
secrets should not be included in client responses.
