# Time Tracker Domain Requirements

## Overview
A bi-weekly time tracking system that allows users to manage their time entries across projects without approval workflows. The system focuses on personal time management and utilization reporting.

## Domain Entities

### User
Represents a registered user of the system.

**Properties:**
- `id` (Guid/string): Unique identifier
- `email` (string): User's email (unique, used for login)
- `passwordHash` (string): Hashed password for authentication
- `firstName` (string): User's first name
- `lastName` (string): User's last name
- `createdAt` (DateTime): Registration timestamp
- `isActive` (bool): Account status

**Business Rules:**
- Email must be unique and valid format
- Password must meet minimum security requirements (8+ chars, mixed case, number, special char)
- Users can only access their own timesheets and entries

### TimeSheet
Represents a two-week time tracking period for a user.

**Properties:**
- `id` (Guid/string): Unique identifier
- `userId` (string): Reference to owning user
- `startDate` (DateTime): Beginning of pay period (typically Monday)
- `endDate` (DateTime): End of pay period (14 days from start)
- `status` (enum): Open, Closed
- `totalHours` (decimal): Sum of all time entries
- `createdAt` (DateTime): Creation timestamp
- `lastModifiedAt` (DateTime): Last update timestamp

**Business Rules:**
- Each timesheet covers exactly 14 consecutive days
- Start date must be a Monday
- Users can have multiple timesheets but only one per period
- Total hours automatically calculated from time entries
- Once closed, timesheet becomes read-only (optional business rule)

### TimeEntry
Represents a single time tracking record on a timesheet.

**Properties:**
- `id` (Guid/string): Unique identifier
- `timeSheetId` (string): Parent timesheet reference
- `createdAt` (DateTime): Creation timestamp
- `createdBy` (string): User who created the entry
- `projectCode` (string): Project identifier
- `workTypeCode` (string): Work type/category identifier (e.g., "DEV", "MEET", "TEST", "ADMIN")
- `startTime` (DateTime?): Optional start time for detailed tracking
- `endTime` (DateTime?): Optional end time for detailed tracking
- `hours` (decimal): Hours worked (required)
- `entryDate` (DateTime): Date of work (must fall within timesheet period)
- `notes` (string): Optional description/notes
- `lastModifiedAt` (DateTime): Last update timestamp

**Business Rules:**
- Entry date must fall within parent timesheet's date range
- Hours must be positive and reasonable (e.g., max 24 per day)
- If startTime and endTime are provided, hours should match calculated duration
- Project code must be valid (validate against project catalog)
- Work type code must be valid (validate against work type catalog)
- User can only modify their own time entries

### Project
Represents a project that time can be tracked against.

**Properties:**
- `code` (string): Unique project identifier
- `name` (string): Project display name
- `description` (string): Project description
- `isActive` (bool): Whether project accepts new time entries
- `createdAt` (DateTime): Creation timestamp

**Business Rules:**
- Project codes must be unique
- Only active projects can receive new time entries
- Historical entries for inactive projects remain visible

### WorkType
Represents a category or type of work that can be tracked.

**Properties:**
- `code` (string): Unique work type identifier (e.g., "DEV", "MEET", "TEST")
- `name` (string): Work type display name (e.g., "Development", "Meeting", "Testing")
- `description` (string): Work type description
- `isActive` (bool): Whether work type accepts new time entries
- `createdAt` (DateTime): Creation timestamp

**Business Rules:**
- Work type codes must be unique
- Only active work types can receive new time entries
- Historical entries for inactive work types remain visible

## Use Cases / User Stories

### Authentication & User Management

#### UC-001: User Registration
**Actor:** Unregistered User
**Description:** User creates a new account in the system
**Flow:**
1. User provides email, password, first name, last name
2. System validates email format and uniqueness
3. System validates password strength
4. System hashes password and creates user account
5. System sends confirmation (optional)

**Success Result:** User account created, user can login

#### UC-002: User Login
**Actor:** Registered User
**Description:** User authenticates to access the system
**Flow:**
1. User provides email and password
2. System validates credentials
3. System generates authentication token/session
4. User gains access to their timesheets

**Success Result:** User authenticated and redirected to dashboard

### Timesheet Management

#### UC-003: Create Timesheet
**Actor:** Authenticated User
**Description:** System automatically creates timesheet for current pay period
**Flow:**
1. User accesses system
2. System checks for existing timesheet covering current date
3. If none exists, system creates new timesheet with:
   - Start date: Most recent Monday (or current if Monday)
   - End date: Start date + 13 days
   - Status: Open

**Success Result:** Timesheet available for time entry

#### UC-004: View Timesheets
**Actor:** Authenticated User
**Description:** User browses their historical and current timesheets
**Flow:**
1. User requests timesheet list
2. System retrieves all timesheets for user
3. System displays with summary information (dates, total hours, status)

**Success Result:** User sees list of timesheets ordered by date

#### UC-005: Close Timesheet
**Actor:** Authenticated User
**Description:** User finalizes a timesheet period
**Flow:**
1. User selects open timesheet
2. System validates all entries
3. User confirms closure
4. System marks timesheet as closed
5. System recalculates total hours

**Success Result:** Timesheet marked closed (optional: becomes read-only)

### Time Entry Management

#### UC-006: Create Time Entry
**Actor:** Authenticated User
**Description:** User logs time worked on a project
**Flow:**
1. User selects timesheet and entry date
2. User provides: project code, work type code, hours, entry date, notes (optional)
3. System validates:
   - Entry date within timesheet period
   - Project code exists and is active
   - Work type code exists and is active
   - Hours are positive and reasonable
4. System creates time entry
5. System updates timesheet total hours

**Success Result:** Time entry created and visible on timesheet

#### UC-007: Update Time Entry
**Actor:** Authenticated User
**Description:** User modifies existing time entry
**Flow:**
1. User selects time entry to edit
2. User modifies fields (project, work type, hours, notes, etc.)
3. System validates changes
4. System updates entry and timestamps
5. System recalculates timesheet total hours

**Success Result:** Time entry updated successfully

#### UC-008: Delete Time Entry
**Actor:** Authenticated User
**Description:** User removes incorrect time entry
**Flow:**
1. User selects time entry to delete
2. User confirms deletion
3. System removes entry
4. System recalculates timesheet total hours

**Success Result:** Time entry deleted, totals adjusted

#### UC-009: Bulk Time Entry
**Actor:** Authenticated User
**Description:** User creates multiple entries at once (e.g., same project, multiple days)
**Flow:**
1. User specifies date range, project, and hours per day
2. System creates individual entries for each day
3. System validates each entry
4. System updates timesheet totals

**Success Result:** Multiple entries created efficiently

### Project Management

#### UC-010: List Projects
**Actor:** Authenticated User
**Description:** User views available projects for time tracking
**Flow:**
1. User requests project list
2. System retrieves active projects
3. System displays project codes, names, descriptions

**Success Result:** User sees available projects to track time against

#### UC-011: List Work Types
**Actor:** Authenticated User
**Description:** User views available work types for time tracking
**Flow:**
1. User requests work type list
2. System retrieves active work types
3. System displays work type codes, names, descriptions

**Success Result:** User sees available work types to track time against

## Proposed Reports

### R-001: Time Utilization Summary
**Purpose:** Overview of time distribution across projects
**Filters:** Date range (default: last 30 days)
**Metrics:**
- Total hours logged
- Hours per project (with percentage)
- Average hours per day/week
- Days with no time logged

**Visualization:** Pie chart showing project distribution

### R-002: Work Type Summary for Timesheet
**Purpose:** Summary of hours by work type for a specific timesheet period
**Filters:** Timesheet ID or date range
**Metrics:**
- Total hours by work type code
- Percentage of time by work type
- Work type breakdown with detailed entries
- Hours per work type per day

**Format:**
- Summary table showing: Work Type Code, Work Type Name, Total Hours, Percentage
- Drill-down detail showing individual entries by work type
- Sorted by total hours (descending)

**Visualization:** Bar chart or pie chart showing work type distribution

### R-003: Project Time Detail
**Purpose:** Deep dive into time spent on specific projects
**Filters:** Project code, date range
**Metrics:**
- Total hours on project
- Time entries list with dates and notes
- Hours per day worked
- Most productive days

**Visualization:** Bar chart by day, detailed entry table

### R-004: Daily Activity Log
**Purpose:** Detailed breakdown of a specific day's work
**Filters:** Specific date
**Metrics:**
- All time entries for the day
- Total hours
- Projects worked on
- Timeline view if start/end times available

**Visualization:** Timeline or list view with notes




## Business Rules Summary

### Time Entry Rules
1. **Valid Hours:** Hours must be between 0.25 and 24.0 per entry
2. **Date Boundary:** Entry date must fall within timesheet period
3. **Project Validation:** Project code must exist and be active
4. **Ownership:** Users can only create/modify their own entries
5. **Calculation:** If start/end times provided, hours calculated automatically with manual override option
6. **Rounding:** Hours should be rounded to nearest 0.25 (15 minutes)

### Timesheet Rules
1. **Period Length:** Exactly 14 days starting on Monday
2. **Uniqueness:** One timesheet per user per period
3. **Auto-Creation:** System creates timesheet on first access of new period
4. **Total Calculation:** Total hours = sum of all time entry hours
5. **Closure:** Optional - once closed, timesheet is read-only

### User Rules
1. **Email Uniqueness:** Each email can only have one account
2. **Password Security:** Minimum 8 characters, must include uppercase, lowercase, number, special character
3. **Data Isolation:** Users can only access their own data

### Project Rules
1. **Code Uniqueness:** Project codes must be unique across system
2. **Active Status:** Only active projects appear in time entry dropdowns
3. **Historical Preservation:** Deactivating project doesn't affect existing entries

### Work Type Rules
1. **Code Uniqueness:** Work type codes must be unique across system
2. **Active Status:** Only active work types appear in time entry dropdowns
3. **Historical Preservation:** Deactivating work type doesn't affect existing entries
4. **Standard Codes:** System should provide default work types (e.g., DEV, MEET, TEST, ADMIN, TRAIN, SUPPORT)

## Value Objects

### DateRange
Represents a period with start and end dates
- Ensures end date is after start date
- Provides helper methods for checking date inclusion

### TimeSheetPeriod
Specific date range implementation for bi-weekly periods
- Validates start date is Monday
- Enforces 14-day duration
- Provides period overlap detection

### ProjectCode
Value object wrapping project identifier
- Enforces format rules (e.g., alphanumeric, max length)
- Ensures uppercase consistency

## Domain Events (Optional for Future Enhancement)

- `UserRegistered`: When new user account created
- `TimeSheetCreated`: When new timesheet period started
- `TimeEntryClosed`: When timesheet finalized
- `TimeEntryAdded`: When time logged
- `TimeEntryModified`: When entry updated
- `TimeEntryDeleted`: When entry removed
- `ProjectDeactivated`: When project marked inactive

These events could trigger notifications, auditing, or analytics processing.

## Technical Considerations

### Data Validation
- Use Data Annotations or FluentValidation in Core
- Validation happens at entity level and application service level
- Return validation errors in Result objects

### Query Optimization
- Timesheets with time entries are frequently queried together (eager loading)
- Reports may need database-level aggregation for performance
- Consider read models for complex reports (CQRS pattern)

### Scalability Considerations
- Time entries will grow continuously (archive old data strategy)
- Index on userId, entryDate, projectCode for report performance
- Consider pagination for large timesheet lists

### Security
- Authentication required for all operations
- Authorization ensures users only access their own data
- Audit trail of changes (createdAt, lastModifiedAt)
- Consider soft deletes for time entries (add isDeleted flag)

## Future Enhancements (Out of Scope for V1)

1. **Team/Manager View:** Allow managers to view team member timesheets
2. **Approval Workflow:** Add timesheet approval process
3. **Budget Tracking:** Track project hours against budgets
4. **Billing Integration:** Mark entries as billable/non-billable
5. **Mobile App:** Native mobile time tracking
6. **Offline Support:** Allow time entry offline, sync later
7. **Timer Feature:** Start/stop timer for real-time tracking
8. **Integration:** Export to payroll/accounting systems
9. **Custom Fields:** Allow user-defined fields on time entries
10. **Templates:** Save frequent time entry patterns for quick logging
