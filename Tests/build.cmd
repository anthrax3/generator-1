REM ==========================================
REM ||    AnnotationsWithMultipleOutputs    ||
REM ==========================================
REM 
cd AnnotationsWithMultipleOutputs
rmdir /S/Q Output
dotnet add package KY.Generator
dotnet add package KY.Generator.Reflection.Annotations
dotnet build --no-incremental
cd ..

REM ==========================================
REM ||               Types                 ||
REM ==========================================
REM 
cd Types
rmdir /S/Q Output
dotnet add package KY.Generator
dotnet add package KY.Generator.Reflection.Annotations
dotnet build --no-incremental
cd ..

REM ==========================================
REM ||           WebApiController           ||
REM ==========================================
REM 
cd WebApiController
rmdir /S/Q ClientApp\src\app\fix-casing\models
rmdir /S/Q ClientApp\src\app\fix-casing\services
rmdir /S/Q ClientApp\src\app\keep-casing\models
rmdir /S/Q ClientApp\src\app\keep-casing\services
rmdir /S/Q ClientApp\src\app\produces\models
rmdir /S/Q ClientApp\src\app\produces\services
rmdir /S/Q ClientApp\src\app\routed\models
rmdir /S/Q ClientApp\src\app\routed\services
rmdir /S/Q ClientApp\src\app\versioned-api\models
rmdir /S/Q ClientApp\src\app\versioned-api\services
dotnet add package KY.Generator
dotnet add package KY.Generator.Reflection.Annotations
dotnet build --no-incremental
cd ..

PAUSE