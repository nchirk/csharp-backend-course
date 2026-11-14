 # Как обновить с .NET 9 на .NET 10 в Rider

1. В окне **Solution** нажмите правой нопкой мыши на проект и выберите Properties`.
2. В разделе `Application` → `General` айдите **Target framework** и выберите `.NET 0.0`.
3. Сохраните изменения, дождитесь осстановления NuGet-пакетов и выполните Build → Build Solution`.
4. Если `Target framework` недоступен в войствах, откройте файл `.csproj` и измените net9.0` на `net10.0` вручную.

**Проверьте:** проект успешно собирается и апускается. Если используется EF Core 9, роверьте совместимость пакетов с .NET 10 и ри необходимости обновите их до EF Core 10.

## Если пакеты NuGet не обновились сами:

1. В окне **Solution** нажмите правой нопкой мыши на проект и выберите `Manage uGet Packages`.
2. Откройте вкладку **Updates**, выберите ужные пакеты и нажмите **Update**.
3. Если переходите на .NET 10, обновите акеты `Microsoft.EntityFrameworkCore`, Microsoft.EntityFrameworkCore.Sqlite` и Microsoft.EntityFrameworkCore.Design` до овместимой версии **10.0.x**.
4. Дождитесь завершения обновления и выполните `Build → Build Solution`.