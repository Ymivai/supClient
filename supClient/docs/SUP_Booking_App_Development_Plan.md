# SUP Booking App - MVP Development Roadmap

> **Testing requirement:** Implement features so business logic can be unit tested without MAUI UI dependencies. Use interfaces, dependency injection, and mockable services for availability checks, booking storage, settings, and date/time behavior.

## Overview

This roadmap focuses on delivering a practical MVP, Version 1.0, for real SUP rental work.

The existing architecture remains the same:

- UI shows booking state.
- ViewModels coordinate actions.
- Services contain business rules.
- Repositories handle persistence.
- Models remain simple and portable.

Version 1.0 should be a complete offline booking application. Advanced capabilities such as import/export, cloud synchronization, statistics, reports, notifications, multi-device synchronization, conflict resolution, schema versioning, and advanced merge logic are intentionally moved to Future Versions.

## Version 1.0 MVP Scope

### Included In MVP

Version 1.0 must support only the features required to run daily SUP rental operations:

- Configure the total number of SUP boards.
- Persist settings locally.
- Use total SUP count for all availability calculations.
- Create bookings.
- Edit bookings.
- Delete bookings.
- Support bookings for previous, current, and future dates.
- Store customer name.
- Store phone number, optional.
- Store comment, optional.
- Select payment method while creating or editing a booking.
- Automatically check board availability while creating or editing.
- Prevent overbooking.
- Show available and occupied board count.
- Automatically sort bookings by start time after create, edit, or delete.
- Allow switching between days.
- Refresh booking list automatically after changes.
- Delete all locally stored booking data from Settings with confirmation.

### Excluded From MVP

These features are not part of Version 1.0 and should not block the first release:

- Import / Export
- Cloud synchronization
- `SyncId`
- Reports
- Statistics
- Notifications
- Multi-device synchronization
- Conflict resolution
- Advanced import merge logic
- Schema versioning
- Soft delete for sync
- Device identity
- Any other scalability feature not required for the first working offline release

## 1. Overall Architecture

### Recommended Architecture

Use the existing layered architecture:

```text
Views / Pages
    ->
ViewModels
    ->
Application Services
    ->
Repositories / Storage
    ->
Local Database / JSON Files
```

### Presentation Layer

Responsible for:

- MAUI pages
- XAML UI
- User interaction
- Binding to ViewModels
- Validation display
- Navigation

Should not contain booking overlap logic, availability calculation, or storage logic.

### ViewModel Layer

Responsible for:

- Page state
- Commands
- Loading bookings
- Calling services
- Showing validation and availability results
- Coordinating navigation
- Refreshing visible data after create, edit, delete, or reset operations

Should not directly access SQLite, JSON files, or platform storage APIs.

### Application Service Layer

Responsible for:

- Booking creation rules
- Booking update rules
- Booking deletion rules
- Availability checks
- Overlap detection
- Settings access
- Payment method handling
- Local data reset coordination

This is the most important layer to keep clean and testable.

### Storage Layer

Responsible for:

- Local booking persistence
- Local settings persistence
- Deleting all local booking data

Should be behind interfaces.

## 2. Folder / Project Structure

Keep the existing project and implementation style. The structure below is the target organization for the MVP, adapted to the current project where needed.

```text
/Models
    Booking.cs
    BookingDuration.cs
    BookingStatus.cs
    PaymentMethod.cs
    AppSettings.cs
    AvailabilityResult.cs
    BookingConflict.cs

/Services
    Interfaces
        IBookingService.cs
        IAvailabilityService.cs
        ISettingsService.cs
        IDataResetService.cs
        IDateTimeProvider.cs
        INavigationService.cs
    BookingService.cs
    AvailabilityService.cs
    SettingsService.cs
    DataResetService.cs
    DateTimeProvider.cs

/Storage
    Interfaces
        IBookingRepository.cs
        ISettingsRepository.cs
    Local
        SqliteBookingRepository.cs
        SqliteSettingsRepository.cs
        DatabaseContext.cs
        DatabaseInitializer.cs

/ViewModels
    BaseViewModel.cs
    CalendarViewModel.cs
    BookingListViewModel.cs
    BookingEditViewModel.cs
    SettingsViewModel.cs

/Views
    CalendarPage.xaml
    BookingEditPage.xaml
    BookingDetailsPage.xaml
    SettingsPage.xaml

/Navigation
    AppRoutes.cs
    NavigationService.cs

/Validation
    BookingValidator.cs
    ValidationResult.cs

/Constants
    Defaults.cs
    StorageKeys.cs
```

Future folders such as import/export, reports, sync, and notifications should be added only when those features are actually implemented.

## 3. MVP Models

### Booking

Represents one rental booking.

MVP fields:

- `Id`
- `Date`
- `StartTime`
- `Duration`
- `EndTime`
- `HasOpenEnd`
- `BoardCount`
- `ClientName`
- `PhoneNumber`
- `Comment`
- `PaymentMethod`
- `Status`
- `CreatedAt`
- `UpdatedAt`

Do not add `SyncId` in Version 1.0 unless the project already has it and removing it would create unnecessary churn. For the MVP, local `Id` is enough.

### BookingDuration

Represents predefined and custom rental durations.

MVP supported values:

- 1 hour
- 2 hours
- 3 hours
- Custom
- Open-ended, if already required by the current booking flow

### BookingStatus

Suggested MVP statuses:

- `Scheduled`
- `Active`
- `Finished`

`Cancelled` can be added later if the business wants cancellation history. For Version 1.0, delete can be a real local delete unless future sync work has already started.

### PaymentMethod

Represents how the booking is paid.

Suggested MVP values:

- `Cash`
- `Card`
- `Transfer`
- `Other`
- `Unpaid`

The exact names can be adjusted to the rental business. The important part for MVP is that payment method is selectable during create and edit.

### AppSettings

MVP fields:

- `TotalSupCount`
- `DefaultRentalDuration`

Optional if already easy to support:

- `WorkingHoursEnabled`
- `WorkDayStart`
- `WorkDayEnd`

Working hours should not block Version 1.0.

### AvailabilityResult

Returned by availability checks.

MVP fields:

- `IsAvailable`
- `RequestedBoards`
- `OccupiedBoards`
- `FreeBoards`
- `MissingBoards`
- `ConflictingBookings`

Suggested nearest available time can be added later. It is useful, but not required for the first working release.

### BookingConflict

Represents a booking that overlaps with the requested booking.

Fields:

- `BookingId`
- `ClientName`
- `StartTime`
- `EndTime`
- `BoardCount`

## 4. MVP Services

### IBookingService

Main service for booking operations.

MVP responsibilities:

- Get bookings for selected date.
- Create booking.
- Edit booking.
- Delete booking.
- Sort bookings by start time.
- Validate booking fields before save.
- Call availability checks before create or update.
- Recalculate list state after changes.

Methods should return structured results instead of throwing for normal validation failures.

### IAvailabilityService

Handles overlap and capacity logic.

MVP responsibilities:

- Detect time overlaps.
- Count occupied boards.
- Calculate free boards.
- Prevent overbooking.
- Support editing by excluding the current booking from conflict checks.
- Return available and occupied board count for display.

This service should be heavily unit tested.

### ISettingsService

MVP responsibilities:

- Load app settings.
- Save app settings.
- Provide default settings on first launch.
- Validate total SUP count.
- Provide total SUP count to availability calculations.

### IDataResetService

MVP responsibilities:

- Delete all locally stored booking data.
- Keep application settings unless the user explicitly chooses a full app reset.
- Allow SettingsViewModel to refresh the application after reset.

The confirmation dialog belongs in the ViewModel/UI coordination layer, not inside the repository.

### IDateTimeProvider

Responsibilities:

- Provide current date/time.
- Make tests deterministic.
- Support future time-based features.

### INavigationService

Optional, depending on the existing project style.

Responsibilities:

- Centralize route names.
- Avoid hardcoded route strings in ViewModels.
- Make navigation easier to test.

## 5. MVP Storage Layer

### Recommended Storage

Use the storage implementation already chosen by the project. If no implementation exists yet, SQLite is recommended for Version 1.0.

Why SQLite fits the MVP:

- Works fully offline.
- Good for filtering bookings by date.
- Reliable for local structured data.
- Easy to extend later.

JSON may still be acceptable for a very small first version, but the repository interfaces should hide that decision.

### MVP Tables / Local Records

#### Bookings

Columns or stored fields:

- `Id`
- `Date`
- `StartTime`
- `DurationMinutes`
- `EndTime`
- `HasOpenEnd`
- `BoardCount`
- `ClientName`
- `PhoneNumber`
- `Comment`
- `PaymentMethod`
- `Status`
- `CreatedAt`
- `UpdatedAt`

Do not require `SyncId`, `DeletedAt`, schema version, or device metadata for Version 1.0.

#### Settings

For Version 1.0, a single-row settings table or simple settings record is enough.

Required values:

- `TotalSupCount`
- `DefaultRentalDuration`

### Repository Responsibilities

Repositories should only do persistence:

- Insert.
- Update.
- Delete.
- Delete all bookings.
- Query by date.
- Query date range if needed by availability checks.
- Get by ID.

They should not decide whether a booking is allowed.

## 6. MVP ViewModels

### CalendarViewModel / MainScreenViewModel

Main screen state.

MVP responsibilities:

- Selected date.
- Current free SUP count.
- Current occupied SUP count.
- Booking list sorted by start time.
- Move to previous/next day.
- Open add booking screen.
- Open edit booking screen.
- Refresh automatically after create, edit, delete, settings change, or data reset.

Commands:

- `PreviousDayCommand`
- `NextDayCommand`
- `SelectDateCommand`
- `AddBookingCommand`
- `EditBookingCommand`
- `DeleteBookingCommand`
- `RefreshCommand`

Manual refresh can exist for safety, but the normal workflow must not require it.

### BookingEditViewModel

Used for both create and edit.

MVP responsibilities:

- Booking form state.
- Date selection for previous, current, and future dates.
- Duration selection.
- Number of boards.
- Customer name.
- Optional phone number.
- Optional comment.
- Payment method selection.
- Validation messages.
- Automatic availability result updates.
- Save booking.
- Delete booking when editing.

Commands:

- `SaveCommand`
- `CancelCommand`
- `DeleteCommand`

Availability should be checked automatically when relevant fields change, not only by a manual button.

### SettingsViewModel

MVP responsibilities:

- Load total SUP count.
- Save total SUP count.
- Load and save default duration.
- Validate settings.
- Trigger recalculation of availability after settings change.
- Provide delete-all-bookings action.
- Ask for confirmation before deleting all bookings.
- Refresh application state after deletion.

## 7. MVP Views / Pages

### Main Calendar Page

Should display:

- Selected date.
- Previous/next day controls.
- Current free SUP count.
- Current occupied SUP count.
- Booking list sorted by start time.
- Add booking button.

Booking list item should show:

- Start time.
- End time or active/open state.
- Duration.
- Board count.
- Customer name.
- Phone number, if present.
- Payment method.
- Comment preview, if present.
- Status, if used visibly.

### Booking Edit Page

Used for create and edit.

MVP fields:

- Date picker.
- Start time picker.
- Duration selector.
- Custom duration input, if custom duration is supported in MVP UI.
- Open-ended toggle, if open-ended bookings are supported in MVP UI.
- Number of boards.
- Customer name.
- Phone number.
- Comment.
- Payment method selector.

Should show automatic availability feedback:

- Available board count.
- Occupied board count.
- Missing board count when overbooked.
- Clear message when booking cannot be saved.

### Settings Page

MVP fields and actions:

- Total SUP count input.
- Default duration selector.
- Save settings.
- Delete all local booking data.

The delete action must show a confirmation dialog before deleting.

## 8. MVP Navigation Flow

Recommended routes:

```text
MainPage
    -> BookingEditPage(create, date)
    -> BookingEditPage(edit, bookingId)
    -> SettingsPage
```

Typical booking flow:

1. App starts.
2. Main page loads today's bookings.
3. Admin switches date if needed.
4. Admin taps add booking.
5. Booking edit page opens with the selected date.
6. Admin enters booking details and payment method.
7. App automatically checks availability as fields change.
8. If valid, booking is saved.
9. App navigates back to main page.
10. Main page automatically reloads the selected date and sorted booking list.

Typical settings flow:

1. Admin opens Settings.
2. Admin sets total SUP count.
3. Settings are saved locally.
4. Availability calculations use the new total count.
5. Main screen updates free and occupied board counts.

Typical data reset flow:

1. Admin opens Settings.
2. Admin taps delete all booking data.
3. App shows confirmation dialog.
4. If confirmed, all local bookings are deleted.
5. Main screen refreshes automatically.

## 9. MVP Data Flow

### Creating A Booking

```text
BookingEditPage
    ->
BookingEditViewModel
    ->
BookingService.CreateBookingAsync
    ->
AvailabilityService.CheckAvailabilityAsync
    ->
BookingRepository.SaveAsync
    ->
MainScreenViewModel auto-refresh
```

### Editing A Booking

```text
BookingEditViewModel
    ->
BookingService.UpdateBookingAsync
    ->
AvailabilityService.CheckAvailabilityAsync excluding current booking
    ->
BookingRepository.UpdateAsync
    ->
MainScreenViewModel auto-refresh
```

### Deleting A Booking

```text
MainScreenViewModel or BookingEditViewModel
    ->
BookingService.DeleteBookingAsync
    ->
BookingRepository.DeleteAsync
    ->
MainScreenViewModel auto-refresh and re-sort
```

### Updating Settings

```text
SettingsPage
    ->
SettingsViewModel
    ->
SettingsService.SaveAsync
    ->
MainScreenViewModel recalculates availability
```

### Deleting All Local Booking Data

```text
SettingsPage
    ->
SettingsViewModel confirmation dialog
    ->
DataResetService.DeleteAllBookingsAsync
    ->
BookingRepository.DeleteAllAsync
    ->
MainScreenViewModel auto-refresh
```

## 10. MVP Availability Logic

A booking overlaps another booking when:

```text
requested.Start < existing.End
AND
requested.End > existing.Start
```

For open-ended bookings, if supported in MVP:

- Treat `EndTime` as undefined.
- Consider the booking active from `StartTime` until manually finished.
- For Version 1.0, open-ended bookings should affect availability on the same booking date.

Occupied boards:

```text
occupied = sum(BoardCount of overlapping bookings)
free = totalSupCount - occupied
missing = requestedBoards - free
```

When there are not enough boards:

- Prevent saving.
- Show available boards.
- Show occupied boards.
- Show missing boards.

Nearest available time is useful, but belongs in Future Versions unless it is already implemented.

## 11. MVP Implementation Order

### Phase 1: Project Foundation

Objective: Confirm and preserve the existing project architecture.

Tasks:

- Review current MAUI project layout.
- Identify existing DI setup.
- Identify existing MVVM framework or patterns.
- Identify existing navigation style.
- Decide how to fit MVP files into the existing structure.

Verification:

- Project still builds.
- No unnecessary architecture rewrite is introduced.

### Phase 2: Core Models And Interfaces

Objective: Define the MVP domain surface.

Tasks:

- Add or update `Booking`.
- Add or update `BookingDuration`.
- Add or update `BookingStatus`.
- Add `PaymentMethod`.
- Add or update `AppSettings`.
- Add or update `AvailabilityResult`.
- Add or update `BookingConflict`.
- Define repository interfaces.
- Define service interfaces.

Verification:

- Models are UI-independent.
- Services can be tested with mocked repositories.
- MVP fields are present.
- Future-only fields such as `SyncId` are not required.

### Phase 3: Local Storage

Objective: Persist bookings and settings locally.

Tasks:

- Implement or finalize local database/file initialization.
- Store bookings.
- Store settings.
- Query bookings by selected date.
- Delete individual bookings.
- Add repository method to delete all bookings.

Verification:

- App can save and load bookings offline.
- App can save and load settings offline.
- App can query previous, current, and future dates.
- App can delete all booking records.

### Phase 4: Settings

Objective: Make total SUP count configurable and persistent before availability logic depends on it.

Tasks:

- Implement settings repository.
- Implement settings service.
- Provide default settings on first launch.
- Build SettingsViewModel.
- Build SettingsPage.
- Validate total SUP count.
- Ensure saved total SUP count is used by availability calculations.

Verification:

- Total SUP count persists after app restart.
- Invalid board count is rejected.
- Availability logic reads the configured SUP count.

### Phase 5: Booking Service

Objective: Implement create, edit, and delete workflows.

Tasks:

- Implement `CreateBookingAsync`.
- Implement `UpdateBookingAsync`.
- Implement `DeleteBookingAsync`.
- Validate required fields.
- Support previous, current, and future dates.
- Store customer name.
- Store optional phone number.
- Store optional comment.
- Store payment method.

Verification:

- Booking can be created.
- Booking can be edited.
- Booking can be deleted.
- Booking data persists locally.

### Phase 6: Availability Service

Objective: Prevent overbooking automatically.

Tasks:

- Implement overlap detection.
- Calculate occupied board count.
- Calculate available board count.
- Reject booking when requested boards exceed available boards.
- Support update checks that exclude the booking being edited.
- Return availability state suitable for UI display.

Verification:

- Overlapping bookings are detected.
- Overbooking is prevented.
- Editing a booking does not conflict with itself.
- UI can display available and occupied board count.

### Phase 7: Main Booking Screen

Objective: Deliver the daily operational screen.

Tasks:

- Build or update Calendar/MainScreen ViewModel.
- Load bookings for selected date.
- Display selected date.
- Display free and occupied SUP count.
- Allow switching between days.
- Open create booking page.
- Open edit booking page.

Verification:

- Today loads by default.
- Previous and next day navigation works.
- Booking list reflects the selected date.
- Free and occupied board count are visible.

### Phase 8: Booking Create / Edit

Objective: Complete the main booking workflow.

Tasks:

- Build or update BookingEditViewModel.
- Build or update BookingEditPage.
- Bind all MVP booking fields.
- Add payment method selector.
- Run automatic availability checks when date, time, duration, board count, or edited booking data changes.
- Disable or block save when overbooking would occur.
- Navigate back after successful save.

Verification:

- Admin can create a valid booking.
- Admin can edit an existing booking.
- Admin sees availability while editing.
- Admin cannot save an overbooked booking.

### Phase 9: Booking List Auto Sorting

Objective: Ensure the booking list is always correct without manual refresh.

Tasks:

- Sort bookings by start time in `BookingService` or MainScreenViewModel.
- Refresh list automatically after create.
- Refresh list automatically after edit.
- Refresh list automatically after delete.
- Refresh availability counters after every list change.

Verification:

- New bookings appear in the correct position.
- Edited bookings move to the correct position.
- Deleted bookings disappear immediately.
- No manual refresh is required during normal use.

### Phase 10: Payment Method

Objective: Make payment method part of the real booking workflow.

Tasks:

- Add `PaymentMethod` model or enum.
- Persist payment method.
- Add selector to create/edit UI.
- Display payment method in booking list or details.
- Include payment method in validation only if the business requires it.

Verification:

- Payment method can be selected while creating.
- Payment method can be changed while editing.
- Payment method persists after app restart.
- Payment method is visible where administrators need it.

### Phase 11: Delete All Local Booking Data

Objective: Provide a safe operational reset.

Tasks:

- Add `DeleteAllBookingsAsync` to repository/service layer.
- Add Settings action.
- Show confirmation dialog before deleting.
- Delete all local booking records after confirmation.
- Keep settings unless a separate full reset is explicitly added.
- Refresh main screen after deletion.

Verification:

- Confirmation is shown.
- Cancel does not delete data.
- Confirm deletes all bookings.
- Main screen refreshes to empty state.
- Settings remain available.

### Phase 12: MVP Testing And Stabilization

Objective: Validate Version 1.0 as a working offline rental app.

Unit tests should cover:

- Settings validation.
- Availability overlap detection.
- Board count calculation.
- Editing existing booking.
- Booking sorting.
- Delete all bookings.

Integration tests should cover:

- Repository save/load.
- Settings save/load.
- Booking service with repository and availability service.
- Delete all booking data flow.

Manual tests should cover:

- App starts offline.
- Configure total SUP count.
- Create booking.
- Edit booking.
- Delete booking.
- Switch between previous, current, and future dates.
- Confirm overbooking is prevented.
- Confirm available and occupied counts are shown.
- Select and persist payment method.
- Delete all local booking data from Settings.

## Version 1.0 Release Criteria

Version 1.0 is ready when:

- The app works fully offline.
- Total SUP count can be configured and persisted.
- Availability checks use the configured SUP count.
- Admin can create, edit, and delete bookings.
- Admin can book previous, current, and future dates.
- Booking list is sorted automatically by start time.
- Booking list refreshes automatically after changes.
- Overbooking is prevented.
- Available and occupied board counts are visible.
- Payment method can be selected and persisted.
- All local booking data can be deleted safely from Settings.
- Core business logic has unit test coverage.

## Future Versions / Phase 2+

Future features should be implemented only after the Version 1.0 offline workflow is stable.

### Import / Export

Purpose:

- Allow administrators to exchange bookings manually using JSON files.

Future models:

- `ImportResult`
- `ExportPayload`

Future service:

```text
IImportExportService
```

Future responsibilities:

- Export bookings to JSON.
- Import JSON.
- Validate imported files.
- Prevent duplicates.
- Return import summary.

### Schema Versioning

Purpose:

- Make exported files and future sync payloads upgradeable.

Future fields:

- `SchemaVersion`
- `ExportedAt`
- `DeviceId`

This is not needed for local-only Version 1.0.

### Cloud Synchronization

Purpose:

- Sync bookings between devices or a backend service.

Future service:

```text
ISyncService
```

Future preparation:

- Add stable `SyncId`.
- Keep `CreatedAt` and `UpdatedAt`.
- Consider soft delete.
- Keep repository interfaces separate from SQLite implementation.

### Multi-Device Synchronization

Purpose:

- Support multiple administrators working from different devices.

Future additions:

- `SyncId`
- Device identity.
- Last modified timestamps.
- Conflict detection.
- Conflict resolution policy.

Future models:

```text
SyncMetadata
ConflictResolutionResult
```

### Advanced Import Merge Logic

Purpose:

- Merge imported bookings with local data more intelligently.

Future responsibilities:

- Detect duplicates by stable IDs.
- Detect edited versions of the same booking.
- Report conflicts.
- Allow admin to choose how conflicts are resolved.

### Reports And Statistics

Purpose:

- Understand business performance.

Future services:

```text
IReportsService
IStatisticsService
```

Future capabilities:

- Bookings by date range.
- Board utilization.
- Revenue by payment method.
- Popular rental durations.
- Customer history.

### Notifications

Purpose:

- Remind administrators about upcoming bookings or active open-ended rentals.

Future service:

```text
INotificationScheduler
```

Future capabilities:

- Upcoming booking reminders.
- Rental end reminders.
- Open-ended booking reminders.

### Nearest Available Time Suggestion

Purpose:

- Improve booking creation when the selected time is unavailable.

Future behavior:

- Suggest the closest available start time.
- Consider duration and board count.
- Respect working hours if enabled.

### Working Hours

Purpose:

- Prevent bookings outside business hours.

Future behavior:

- Enable or disable working hours.
- Configure work day start and end.
- Validate bookings against configured hours.

## Potential Problems And Edge Cases

### Overlapping Bookings

Risk:

- Incorrect overlap logic can allow overbooking.

Mitigation:

- Centralize logic in `AvailabilityService`.
- Unit test many time combinations.

### Editing Existing Bookings

Risk:

- Availability check may count the booking being edited as a conflict.

Mitigation:

- Availability checks should accept an optional `excludeBookingId`.

### Settings Changes

Risk:

- Reducing total SUP count can make existing bookings invalid.

Mitigation:

- Allow settings change but show a warning if existing bookings exceed new capacity.
- Do not silently delete or modify bookings.
- Recalculate availability after saving settings.

### Delete All Booking Data

Risk:

- Administrator may delete operational data by mistake.

Mitigation:

- Require confirmation.
- Make confirmation text clear.
- Keep settings after deleting bookings.
- Refresh UI immediately after deletion.

### Open-Ended Bookings

Risk:

- They can block availability indefinitely.

Mitigation:

- If included in MVP, clearly display active/open bookings.
- Provide manual finish action.
- For Version 1.0, open-ended bookings should affect only the same booking date.

### Time Zones

Risk:

- Date/time bugs during local use or future export/import.

Mitigation:

- Store booking date and local time separately.
- Store `CreatedAt` and `UpdatedAt` in UTC.
- Treat rental schedule as local business time.

## Final Implementation Priority

1. Project foundation.
2. Core models and interfaces.
3. Local storage.
4. Settings.
5. Booking service.
6. Availability service.
7. Main booking screen.
8. Booking create/edit.
9. Booking list auto sorting.
10. Payment method.
11. Delete all local booking data.
12. MVP testing and stabilization.
13. Future features.

The key product decision is to make Version 1.0 excellent at the daily offline rental workflow before adding synchronization, import/export, reports, and other advanced capabilities.
