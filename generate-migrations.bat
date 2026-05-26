@echo off
echo ============================================
echo  The League Platform - EF Core Migrations
echo  Run from solution root folder
echo ============================================
echo.

echo [1/16] Identity...
dotnet ef migrations add CreateIdentitySchema --project src/Modules/TheLeague.Modules.Identity --startup-project src/TheLeague.Host --context IdentityModuleDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Identity & pause & exit /b 1)

echo [2/16] Clubs...
dotnet ef migrations add CreateClubsSchema --project src/Modules/TheLeague.Modules.Clubs --startup-project src/TheLeague.Host --context ClubsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Clubs & pause & exit /b 1)

echo [3/16] Members...
dotnet ef migrations add CreateMembersSchema --project src/Modules/TheLeague.Modules.Members --startup-project src/TheLeague.Host --context MembersDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Members & pause & exit /b 1)

echo [4/16] Memberships...
dotnet ef migrations add CreateMembershipsSchema --project src/Modules/TheLeague.Modules.Memberships --startup-project src/TheLeague.Host --context MembershipsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Memberships & pause & exit /b 1)

echo [5/16] Sessions...
dotnet ef migrations add CreateSessionsSchema --project src/Modules/TheLeague.Modules.Sessions --startup-project src/TheLeague.Host --context SessionsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Sessions & pause & exit /b 1)

echo [6/16] Events...
dotnet ef migrations add CreateEventsSchema --project src/Modules/TheLeague.Modules.Events --startup-project src/TheLeague.Host --context EventsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Events & pause & exit /b 1)

echo [7/16] Competitions...
dotnet ef migrations add CreateCompetitionsSchema --project src/Modules/TheLeague.Modules.Competitions --startup-project src/TheLeague.Host --context CompetitionsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Competitions & pause & exit /b 1)

echo [8/16] Payments...
dotnet ef migrations add CreatePaymentsSchema --project src/Modules/TheLeague.Modules.Payments --startup-project src/TheLeague.Host --context PaymentsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Payments & pause & exit /b 1)

echo [9/16] Facilities...
dotnet ef migrations add CreateFacilitiesSchema --project src/Modules/TheLeague.Modules.Facilities --startup-project src/TheLeague.Host --context FacilitiesDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Facilities & pause & exit /b 1)

echo [10/16] Equipment...
dotnet ef migrations add CreateEquipmentSchema --project src/Modules/TheLeague.Modules.Equipment --startup-project src/TheLeague.Host --context EquipmentDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Equipment & pause & exit /b 1)

echo [11/16] Programs...
dotnet ef migrations add CreateProgramsSchema --project src/Modules/TheLeague.Modules.Programs --startup-project src/TheLeague.Host --context ProgramsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Programs & pause & exit /b 1)

echo [12/16] Communications...
dotnet ef migrations add CreateCommunicationsSchema --project src/Modules/TheLeague.Modules.Communications --startup-project src/TheLeague.Host --context CommunicationsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Communications & pause & exit /b 1)

echo [13/16] Analytics...
dotnet ef migrations add CreateAnalyticsSchema --project src/Modules/TheLeague.Modules.Analytics --startup-project src/TheLeague.Host --context AnalyticsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Analytics & pause & exit /b 1)

echo [14/16] Shop...
dotnet ef migrations add CreateShopSchema --project src/Modules/TheLeague.Modules.Shop --startup-project src/TheLeague.Host --context ShopDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Shop & pause & exit /b 1)

echo [15/16] Documents...
dotnet ef migrations add CreateDocumentsSchema --project src/Modules/TheLeague.Modules.Documents --startup-project src/TheLeague.Host --context DocumentsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Documents & pause & exit /b 1)

echo [16/16] Subscriptions...
dotnet ef migrations add CreateSubscriptionsSchema --project src/Modules/TheLeague.Modules.Subscriptions --startup-project src/TheLeague.Host --context SubscriptionsDbContext --output-dir Infrastructure/Persistence/Migrations
if %ERRORLEVEL% NEQ 0 (echo FAILED: Subscriptions & pause & exit /b 1)

echo.
echo ============================================
echo  ALL 16 MIGRATIONS GENERATED SUCCESSFULLY
echo ============================================
pause
