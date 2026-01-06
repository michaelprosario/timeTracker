# Project Time Report Implementation Design

## Overview
Create a report that shows time used on each project for each time sheet, allowing users to see how their time is distributed across projects.

## Requirements
- Display time entries grouped by time sheet and project
- Show total hours per project per time sheet
- Include project name, work type, and time entry details
- Filter by user (show only the current user's time entries)
- Display in a clear, readable format

## Architecture

### Core Layer (Domain/Business Logic)
**Query: ProjectTimeReportQuery**
- Input: UserId, optional TimeSheetId filter, optional date range
- Output: List of time sheets with projects and time entries
- Group by: TimeSheet → Project → TimeEntries
- Calculate: Total hours per project, total hours per time sheet

**Model: ProjectTimeReportDto**
```
- TimeSheetId
- TimeSheetWeekEnding
- Projects: List<ProjectTimeDto>
  - ProjectId
  - ProjectName
  - TotalHours
  - TimeEntries: List<TimeEntryDto>
    - Date
    - Hours
    - WorkType
    - Description
```

### Infrastructure Layer
- Use existing repositories (TimeEntryRepository, ProjectRepository, TimeSheetRepository)
- Join queries to get all necessary data efficiently

### Web Layer
**Controller: ReportsController**
- Action: ProjectTimeReport()
- Get current user from session/authentication
- Call ProjectTimeReportQuery service
- Return view with report data

**View: ProjectTimeReport.cshtml**
- Display time sheets in reverse chronological order
- For each time sheet, show:
  - Week ending date
  - Projects with time breakdown
  - Total hours for the time sheet
- Use tables or cards for clear visualization
- Add optional filters (date range, specific time sheet)

## Implementation Steps
1. Create DTOs for report data in Core layer
2. Create ProjectTimeReportQuery service in Core/Queries
3. Create ReportsController in Web layer
4. Create ProjectTimeReport view
5. Add navigation link to reports in the main menu
6. Add unit tests for the query logic

## UI Considerations
- Mobile-friendly responsive design
- Color coding for different projects
- Export to PDF/Excel (future enhancement)
- Print-friendly CSS
