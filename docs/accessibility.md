# Accessibility checklist

Keep new pages usable with keyboard navigation, zoom, screen readers, and
touch input.

- Give every form control a visible label or an appropriate accessible name.
- Keep keyboard focus visible and preserve a logical tab order.
- Use headings in document order so page sections are easy to navigate.
- Provide meaningful `alt` text for informative images and empty `alt` text for
  decorative images.
- Do not communicate status using color alone; pair it with text or an icon
  label.
- Ensure loading, success, and error states are announced or visible without
  requiring a mouse.
- Keep interactive controls large enough for touch and avoid hover-only actions.
- Check responsive layouts at narrow widths and with browser text zoom.

For JavaScript-rendered content, update the relevant live region or visible
status element after asynchronous work. Error messages should identify the
problem and the next action a user can take.
