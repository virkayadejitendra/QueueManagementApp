# Restaurant Queue Management App

## Project Goal
Build a simple free restaurant waitlist system to replace paper notebook queue management.

Main success criteria:
- Can one restaurant stop using paper for customer queue management?
- Can non-technical restaurant staff run the queue from a mobile-friendly dashboard?

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
  -> Owner creates restaurant during registration
  -> System generates RestaurantCode
  -> Owner/manager can create manager accounts
  -> Manager opens queue for the day
  -> System provides customer QR join URL
  -> Customers join queue using QR
  -> Customer receives private live status link
  -> Manager calls and manages queue
  -> Display screen shows current called token and last seated token
```

## Architecture
Frontend communicates with ASP.NET Core REST APIs.

Backend handles:
- authentication and authorization
- owner and manager account management
- restaurant registration and setup
- restaurant code generation
- QR join URL generation
- token generation
- business-day calculation
- queue state and queue transitions
- customer private tracking tokens

Database stores:
- users
- restaurants
- user restaurant roles
- queue entries
- queue status/history fields

## User Roles

### Owner
- Registers a business account and restaurant.
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

### Restaurant
- Id
- Name
- Address
- Mobile
- RestaurantCode
- QueueResetTime
- IsQueueOpen
- CreatedAt

`RestaurantCode` is used in public/customer URLs. It should not be treated as a secret.

### UserRestaurant
- Id
- UserId
- RestaurantId
- Role: Owner | Manager
- CreatedAt

This keeps the design flexible for future multi-restaurant support.

### QueueEntry
- Id
- RestaurantId
- BusinessDate
- TokenNumber
- CustomerName
- MobileNumber nullable
- GuestCount
- Status
- TrackingToken
- CallCount
- SkipCount
- SortOrder
- CreatedAt
- CalledAt nullable
- SeatedAt nullable
- CancelledAt nullable
- LastSkippedAt nullable

## Queue Status Values
- Waiting
- Called
- Seated
- Skipped
- Cancelled

## Business Day and Token Reset
Token numbers reset daily per restaurant, but not at midnight.

Each restaurant has a configurable `QueueResetTime`, for example `04:00`.

Business date rule:
```text
If current local time is before QueueResetTime:
  BusinessDate = yesterday
Else:
  BusinessDate = today
```

This supports restaurants that stay open past midnight.

Token numbers are generated per:
```text
RestaurantId + BusinessDate
```

## Queue Open and Close Rules
- Customers can join only when the restaurant queue is open.
- Manager/owner must open the queue before accepting customers.
- When the queue is closed:
  - new customers cannot join
  - active entries become Cancelled
  - customer live status links become inaccessible
  - history remains stored in the database

## Customer Join Rules
Customer QR URL:
```text
/join/{restaurantCode}
```

Required customer fields:
- CustomerName
- GuestCount

Optional customer fields:
- MobileNumber

Validation:
- CustomerName is required.
- GuestCount is required.
- GuestCount max is 20.
- MobileNumber is optional and may be local or country-code format.

After joining, customer receives a private status URL:
```text
/queue/{queueEntryId}/{trackingToken}
```

The tracking token prevents customers from guessing other queue entries.

Customer live status page should show:
- restaurant name
- token number
- current status
- current queue position when waiting

Customer live status page should work only while the queue is open.

## Manager Dashboard Rules
Managers can:
- open queue
- close queue
- call next customer
- mark called customer as seated
- mark called customer as no response
- move customer to skipped list
- cancel customer
- manually add walk-in customer
- create manager accounts

Multiple managers can be logged in for the same restaurant at the same time.

Backend must own queue state and protect against conflicts. The UI should refresh/poll regularly, but business rules must be enforced by the API.

## Call Next Rules
- `Call Next` picks the earliest Waiting entry by priority.
- `Call Next` is disabled while any entry is currently Called for the same restaurant and business date.
- Backend must enforce this rule and return `409 Conflict` if another Called entry already exists.
- This must be handled atomically to support multiple managers using the dashboard.

Basic flow:
```text
Call Next
  -> earliest Waiting entry becomes Called
  -> Call Next becomes disabled
  -> manager must choose Seated, No Response, Cancelled, or Move to Skipped
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
- last seated token

For MVP, no separate display role is needed.

## MVP Screens

### Owner Registration
- owner name
- email and/or mobile
- password
- restaurant name
- restaurant address
- restaurant mobile

Registration immediately creates:
- owner user
- restaurant
- owner-restaurant role
- restaurant code

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
- seated/recent activity
- manual walk-in entry
- manager creation

### Customer Join
- restaurant name
- customer name
- guest count
- optional mobile number
- join queue button

### Customer Status
- token number
- current queue status
- queue position when waiting
- inaccessible after queue is closed

### Display Screen
- current called token
- last seated token

## MVP Scope
Must include:
- owner self-registration
- restaurant creation during owner registration
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
- table allocation
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
- `404 Not Found` for missing restaurant/queue entry.
- `409 Conflict` for invalid queue state conflicts, such as calling next while another token is already Called.

## Suggested API Endpoints

### Auth
- `POST /api/auth/register-owner`
- `POST /api/auth/login`

### Restaurants
- `GET /api/restaurants/{restaurantCode}`
- `GET /api/restaurants/{restaurantId}/qr`

### Managers
- `POST /api/restaurants/{restaurantId}/managers`
- `GET /api/restaurants/{restaurantId}/managers`

### Queue
- `POST /api/restaurants/{restaurantCode}/queue/join`
- `GET /api/queue/{queueEntryId}/{trackingToken}`
- `POST /api/restaurants/{restaurantId}/queue/open`
- `POST /api/restaurants/{restaurantId}/queue/close`
- `GET /api/restaurants/{restaurantId}/queue/today`
- `POST /api/restaurants/{restaurantId}/queue/walk-in`
- `POST /api/restaurants/{restaurantId}/queue/call-next`
- `POST /api/queue/{queueEntryId}/seated`
- `POST /api/queue/{queueEntryId}/no-response`
- `POST /api/queue/{queueEntryId}/skipped`
- `POST /api/queue/{queueEntryId}/restore`
- `POST /api/queue/{queueEntryId}/cancel`

### Display
- `GET /api/restaurants/{restaurantId}/display`

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

## UI Guidelines
- Mobile first.
- Large buttons.
- Minimal typing.
- Easy for non-technical restaurant staff.
- Make queue state obvious.
- Disable actions that are not currently valid.
- Show clear messages when another manager already changed queue state.

## Future Scope
- PostgreSQL for production durability
- WhatsApp notifications
- Wait-time estimation
- Multi-language support
- Table allocation system
- Multi-branch management
- Separate read-only display role
- SignalR real-time updates
- Mobile number normalization by country
