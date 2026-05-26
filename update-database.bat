@echo off
echo ============================================
echo  The League Platform - Update Database
echo  Run from solution root folder
echo ============================================
echo.

echo [1/16] Identity...
dotnet ef database update --project src/Modules/TheLeague.Modules.Identity --startup-project src/TheLeague.Host --context IdentityModuleDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Identity & pause & exit /b 1)

echo [2/16] Clubs...
dotnet ef database update --project src/Modules/TheLeague.Modules.Clubs --startup-project src/TheLeague.Host --context ClubsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Clubs & pause & exit /b 1)

echo [3/16] Members...
dotnet ef database update --project src/Modules/TheLeague.Modules.Members --startup-project src/TheLeague.Host --context MembersDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Members & pause & exit /b 1)

echo [4/16] Memberships...
dotnet ef database update --project src/Modules/TheLeague.Modules.Memberships --startup-project src/TheLeague.Host --context MembershipsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Memberships & pause & exit /b 1)

echo [5/16] Sessions...
dotnet ef database update --project src/Modules/TheLeague.Modules.Sessions --startup-project src/TheLeague.Host --context SessionsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Sessions & pause & exit /b 1)

echo [6/16] Events...
dotnet ef database update --project src/Modules/TheLeague.Modules.Events --startup-project src/TheLeague.Host --context EventsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Events & pause & exit /b 1)

echo [7/16] Competitions...
dotnet ef database update --project src/Modules/TheLeague.Modules.Competitions --startup-project src/TheLeague.Host --context CompetitionsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Competitions & pause & exit /b 1)

echo [8/16] Payments...
dotnet ef database update --project src/Modules/TheLeague.Modules.Payments --startup-project src/TheLeague.Host --context PaymentsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Payments & pause & exit /b 1)

echo [9/16] Facilities...
dotnet ef database update --project src/Modules/TheLeague.Modules.Facilities --startup-project src/TheLeague.Host --context FacilitiesDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Facilities & pause & exit /b 1)

echo [10/16] Equipment...
dotnet ef database update --project src/Modules/TheLeague.Modules.Equipment --startup-project src/TheLeague.Host --context EquipmentDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Equipment & pause & exit /b 1)

echo [11/16] Programs...
dotnet ef database update --project src/Modules/TheLeague.Modules.Programs --startup-project src/TheLeague.Host --context ProgramsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Programs & pause & exit /b 1)

echo [12/16] Communications...
dotnet ef database update --project src/Modules/TheLeague.Modules.Communications --startup-project src/TheLeague.Host --context CommunicationsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Communications & pause & exit /b 1)

echo [13/16] Analytics...
dotnet ef database update --project src/Modules/TheLeague.Modules.Analytics --startup-project src/TheLeague.Host --context AnalyticsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Analytics & pause & exit /b 1)

echo [14/16] Shop...
dotnet ef database update --project src/Modules/TheLeague.Modules.Shop --startup-project src/TheLeague.Host --context ShopDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Shop & pause & exit /b 1)

echo [15/16] Documents...
dotnet ef database update --project src/Modules/TheLeague.Modules.Documents --startup-project src/TheLeague.Host --context DocumentsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Documents & pause & exit /b 1)

echo [16/16] Subscriptions...
dotnet ef database update --project src/Modules/TheLeague.Modules.Subscriptions --startup-project src/TheLeague.Host --context SubscriptionsDbContext
if %ERRORLEVEL% NEQ 0 (echo FAILED: Subscriptions & pause & exit /b 1)

echo.
echo ============================================
echo  DATABASE UPDATED SUCCESSFULLY (16 schemas)
echo ============================================
pause
