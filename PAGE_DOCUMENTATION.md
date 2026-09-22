# AdDiin Complete Page Documentation

## Document Purpose

This document describes the user-facing pages and supporting Razor views in the AdDiin platform. It is intended for code review, Git commit planning, onboarding, QA, route verification, and feature handover.

The document follows the real MVC structure in `AdDiin/Controllers`, `AdDiin/Views`, `AdDiin/Models`, and `AdDiin/Services`.

## How To Read This Document

Each page description uses the same structure:

- **Route**: The URL a user opens.
- **Controller**: The MVC controller and action that prepare the page.
- **View**: The Razor file that renders the HTML.
- **Model**: The data contract supplied to the view.
- **Access**: Whether the page is public, authenticated, or administrator-only.
- **Inputs**: Forms, query parameters, uploads, or browser data.
- **Actions**: Requests sent after the page is displayed.
- **User flow**: What a normal visitor can do.
- **Commit boundary**: A logical Git change that can be reviewed independently.
- **QA checklist**: Practical checks for this page.

## Global Page Architecture

### MVC request flow

1. A browser requests a public alias or conventional MVC URL.
2. ASP.NET Core routing selects a controller action.
3. The controller loads entities through the application database context or a service.
4. The controller builds a view model where required.
