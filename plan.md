# Queue Management App

## Project Goal
Build a simple free queue management system to replace paper notebook or manual token tracking.

Main success criteria:
- Can one business/location stop using paper for customer queue management?
- Can non-technical staff run the queue from a mobile-friendly dashboard?
- Can the same product work for restaurants, clinics, salons, service counters, offices, and similar queue-based operations?

## Tech Stack
- Frontend: Angular
- Backend: ASP.NET Core Web API
- Database: SQLite initially
- Hosting:
  - Frontend: Vercel
  - Backend: Render

SQLite is acceptable for MVP. If the app becomes production-critical or needs durable hosted storage, move to PostgreSQL later.

## High-Level Flow
```text
Owner registers business account
  -> Owner creates a queue location during registration
  -> System generates LocationCode
  -> Owner/manager can create staff manager accounts
  -> Manager opens queue for the day/session
  -> System provides customer QR join URL
  -> Customers join queue using QR
  -> Customer receives private live status link
  -> Manager calls and manages queue
  -> Display screen shows current called token and last served token
```

## Architecture
Frontend communicates with ASP.NET Core REST APIs.

Backend handles:
- authentication and authorization
- owner and manager account management
- business/location registration and setup
- location code generation
- QR join URL generation
- token generation
- business-day calculation
- queue state and queue transitions
- customer private tracking tokens

Database stores:
- users
- queue locations
- user location roles
- queue entries
- queue status/history fields

## User Roles

### Owner
- Registers a business account and queue location.
- Can access manager dashboard.
- Can create multiple manager accounts.
- Can open and close the queue.
- Can manage queue entries.

### Manager
- Created manually by owner or another manager.
- Can access manager dashboard.
- Can create multiple manager accounts.
- Can open and close the queue.
- Can manage queue entries.

For MVP, display screen uses normal owner/manager login. No separate display role is needed.

## Authentication
- Owners and managers log in using either email or mobile number.
- Password is required.
- At least one of email or mobile is required.
- Email should be unique when provided.
- Mobile should be unique when provided.
- Mobile numbers may be local format or country-code format.

Example login request:
```json
{
  "identifier": "owner@example.com",
  "password": "password123"
}
```

or:

```json
{
  "identifier": "+919876543210",
  "password": "password123"
}
```

## Core Entities

### User
- Id
- Name
- Email nullable
- Mobile nullable
- PasswordHash
- IsActive
- CreatedAt

### QueueLocation
- Id
- BusinessName
- LocationName nullable
- Address
- Mobile
- LocationCode
- QueueResetTime
- IsQueueOpen
- CreatedAt

`LocationCode` is used in public/customer URLs. It should not be treated as a secret.

### UserLocation
- Id
- UserId
- QueueLocationId
- Role: Owner | Manager
- CreatedAt

This keeps the design flexible for future multi-location support.

### QueueEntry
- Id
- QueueLocationId
- BusinessDate
- TokenNumber
- CustomerName
- MobileNumber nullable
- PartySize nullable
- ServiceReason nullable
- Status
- TrackingToken
- CallCount
- SkipCount
- SortOrder
- CreatedAt
- CalledAt nullable
- ServedAt nullable
- CancelledAt nullable
- LastSkippedAt nullable

## Queue Status Values
- Waiting
- Called
- Served
- Skipped
- Cancelled

## Business Day and Token Reset
Token numbers reset daily per queue location, but not necessarily at midnight.

Each queue location has a configurable `QueueResetTime`, for example `04:00`.

Business date rule:
```text
If current local time is before QueueResetTime:
  BusinessDate = yesterday
Else:
  BusinessDate = today
```

This supports businesses that stay open past midnight or operate across long service sessions.

Token numbers are generated per:
```text
QueueLocationId + BusinessDate
```

## Queue Open and Close Rules
- Customers can join only when the queue is open.
- Manager/owner must open the queue before accepting customers.
- When the queue is closed:
  - new customers cannot join
  - active entries become Cancelled
  - customer live status links become inaccessible
  - history remains stored in the database

## Customer Join Rules
Customer QR URL:
```text
/join/{locationCode}
```

Required customer fields:
- CustomerName

Optional customer fields:
- MobileNumber
- PartySize
- ServiceReason

Validation:
- CustomerName is required.
- PartySize is optional and must be a positive number when provided.
- PartySize max is 20 for MVP.
- MobileNumber is optional and may be local or country-code format.
- ServiceReason is optional and should be short free text or a predefined option later.

After joining, customer receives a private status URL:
```text
/queue/{queueEntryId}/{trackingToken}
```

The tracking token prevents customers from guessing other queue entries.

Customer live status page should show:
- business/location name
- token number
- current status
- current queue position when waiting

Customer live status page should work only while the queue is open.

## Manager Dashboard Rules
Managers can:
- open queue
- close queue
- call next customer
- mark called customer as served
- mark called customer as no response
- move customer to skipped list
- cancel customer
- manually add walk-in customer
- create manager accounts

Multiple managers can be logged in for the same queue location at the same time.

Backend must own queue state and protect against conflicts. The UI should refresh/poll regularly, but business rules must be enforced by the API.

## Call Next Rules
- `Call Next` picks the earliest Waiting entry by priority.
- `Call Next` is disabled while any entry is currently Called for the same queue location and business date.
- Backend must enforce this rule and return `409 Conflict` if another Called entry already exists.
- This must be handled atomically to support multiple managers using the dashboard.

Basic flow:
```text
Call Next
  -> earliest Waiting entry becomes Called
  -> Call Next becomes disabled
  -> manager must choose Served, No Response, Cancelled, or Move to Skipped
  -> Call Next becomes available again
```

## Skipped Customer Rules
- A called customer can be marked as No Response.
- If no response happens once, the customer should be called again soon.
- They should not be pushed to the end of the entire waiting queue.
- After 2 no-response attempts, manager can manually move the customer to the Skipped list.
- Skipped customers are excluded from `Call Next`.
- Manager can restore a skipped customer to Waiting if needed.

Recommended tracking fields:
- CallCount
- SkipCount
- LastSkippedAt
- SortOrder

## Display Screen
Display screen requires owner/manager login.

Display screen is read-only and shows:
- currently called token
- last served token

For MVP, no separate display role is needed.

## MVP Screens

### Owner Registration
- owner name
- email and/or mobile
- password
- business name
- location name optional
- address
- business/location mobile

Registration immediately creates:
- owner user
- queue location
- owner-location role
- location code

No admin approval is required for MVP.

### Login
- identifier field for email or mobile
- password

### Manager Dashboard
- queue open/close control
- current called customer
- Call Next button
- waiting list
- skipped list
- served/recent activity
- manual walk-in entry
- manager creation

### Customer Join
- business/location name
- customer name
- optional party size
- optional mobile number
- optional service reason
- join queue button

### Customer Status
- token number
- current queue status
- queue position when waiting
- inaccessible after queue is closed

### Display Screen
- current called token
- last served token

## MVP Scope
Must include:
- owner self-registration
- queue location creation during owner registration
- owner/manager login
- manager account creation
- QR customer join
- private customer status link
- manager dashboard
- display screen
- queue open/close
- daily business-date token reset
- manual walk-in entry
- queue status operations

Must NOT include:
- payment systems
- loyalty programs
- AI prediction
- multi-branch management
- WhatsApp notifications
- resource/counter/room allocation
- multi-language support

## API Rules
- Use RESTful APIs.
- Use proper status codes.
- Validate input properly.
- Use DTOs for requests and responses.
- Keep controllers thin.
- Put business rules in services.
- Use async/await.
- Protect write APIs with authentication.
- Enforce queue transition rules in backend, not only frontend.
- Use transactions for queue operations that change call/order state.

Important status codes:
- `400 Bad Request` for validation errors.
- `401 Unauthorized` for missing/invalid login.
- `403 Forbidden` for authenticated users without access.
- `404 Not Found` for missing queue location/queue entry.
- `409 Conflict` for invalid queue state conflicts, such as calling next while another token is already Called.

## Suggested API Endpoints

### Auth
- `POST /api/auth/register-owner`
- `POST /api/auth/login`

### Queue Locations
- `GET /api/locations/{locationCode}`
- `GET /api/locations/{queueLocationId}/qr`

### Managers
- `POST /api/locations/{queueLocationId}/managers`
- `GET /api/locations/{queueLocationId}/managers`

### Queue
- `POST /api/locations/{locationCode}/queue/join`
- `GET /api/queue/{queueEntryId}/{trackingToken}`
- `POST /api/locations/{queueLocationId}/queue/open`
- `POST /api/locations/{queueLocationId}/queue/close`
- `GET /api/locations/{queueLocationId}/queue/today`
- `POST /api/locations/{queueLocationId}/queue/walk-in`
- `POST /api/locations/{queueLocationId}/queue/call-next`
- `POST /api/queue/{queueEntryId}/served`
- `POST /api/queue/{queueEntryId}/no-response`
- `POST /api/queue/{queueEntryId}/skipped`
- `POST /api/queue/{queueEntryId}/restore`
- `POST /api/queue/{queueEntryId}/cancel`

### Display
- `GET /api/locations/{queueLocationId}/display`

## Coding Guidelines
- Use clean architecture concepts.
- Avoid overengineering.
- Prioritize readability.
- Keep controllers thin.
- Business logic should live in services.
- Use DTOs.
- Use async/await.
- Keep validation explicit and easy to understand.
- Prefer simple polling for live updates in MVP; SignalR can be added later.

## Auto Refresh / Polling
For MVP, use simple polling instead of SignalR.

Screens that should auto-refresh:
- Manager Dashboard: every 3-5 seconds.
- Customer Status Page: every 5-10 seconds.
- Display Screen: every 2-3 seconds.

Customer Status Page and Display Screen should show a small "Last updated X seconds ago" indicator.

The indicator should update every second on the frontend based on the last successful refresh time. Do not call the API every second just to update this label.

If refresh fails, show a simple message such as:
```text
Unable to refresh. Retrying...
```

Polling should refresh queue state from the backend. The frontend may disable invalid buttons, but backend APIs must still enforce all queue rules.

## UI Guidelines
- Mobile first.
- Large buttons.
- Minimal typing.
- Easy for non-technical staff.
- Make queue state obvious.
- Disable actions that are not currently valid.
- Show clear messages when another manager already changed queue state.
- Avoid domain-specific labels unless the owner configures them for their business type.

## Future Scope
- PostgreSQL for production durability
- WhatsApp notifications
- Wait-time estimation
- Multi-language support
- Configurable service types/reasons
- Resource/counter/room assignment
- Multi-branch management
- Separate read-only display role
- SignalR real-time updates
- Mobile number normalization by country
