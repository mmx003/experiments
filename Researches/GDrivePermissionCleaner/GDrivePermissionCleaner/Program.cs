using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleDrivePermissionsReset
{
    class Program
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Google Drive API .NET Permissions Reset";

        static async Task Main(string[] args)
        {
            // Проверяем, был ли передан параметр -r
            string rootFolderId = GetRootFolderIdFromArgs(args);
            if (string.IsNullOrEmpty(rootFolderId))
            {
                Console.WriteLine("Ошибка: необходимо указать корневую папку с помощью параметра -r.");
                return;
            }

            UserCredential credential;

            // Аутентификация пользователя
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            // Создаем сервис Google Drive
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Запуск рекурсивного сброса разрешений
            await RemovePermissionsRecursively(service, rootFolderId);
            
            Console.WriteLine("Готово");
        }

        static string GetRootFolderIdFromArgs(string[] args)
        {
            // Находим аргумент -r и возвращаем значение после него
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-r" && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        static async Task RemovePermissionsRecursively(DriveService service, string folderId)
        {
            try
            {
                Console.WriteLine($"Начало обработки папки: {folderId}");
                int filesProcessedCount = 0;
                string pageToken = null;
                do
                {
                    // Получаем список содержимого папки
                    var request = service.Files.List();
                    request.Q = $"'{folderId}' in parents and trashed = false";
                    request.Fields = "nextPageToken, files(id, name, mimeType)";
                    request.PageSize = 100;
                    request.PageToken = pageToken;

                    var result = await request.ExecuteAsync();
                    IList<Google.Apis.Drive.v3.Data.File> files = result.Files;

                    // Обрабатываем каждый файл/папку
                    foreach (var file in files)
                    {
                        Console.WriteLine($"Обработка: {file.Name} ({file.Id})");
                        await RemovePermissions(service, file.Id);
                        filesProcessedCount++;

                        // Если это папка, обрабатываем ее рекурсивно
                        if (file.MimeType == "application/vnd.google-apps.folder")
                        {
                            await RemovePermissionsRecursively(service, file.Id);
                        }
                    }

                    pageToken = result.NextPageToken;
                } while (pageToken != null);

                Console.WriteLine($"Завершена обработка папки: {folderId}. Обработано файлов: {filesProcessedCount}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Произошла ошибка: {e.Message}");
            }
        }

        static async Task RemovePermissions(DriveService service, string fileId)
        {
            try
            {
                // Получаем список разрешений
                var permissionsRequest = service.Permissions.List(fileId);
                permissionsRequest.Fields = "permissions(id, role)";
                var permissions = await permissionsRequest.ExecuteAsync();

                // Удаляем все разрешения, кроме владельца
                foreach (var permission in permissions.Permissions)
                {
                    if (permission.Role != "owner")
                    {
                        Console.WriteLine($"Удаление разрешения: {permission.Id} для файла {fileId}");
                        await service.Permissions.Delete(fileId, permission.Id).ExecuteAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Не удалось удалить разрешения для файла {fileId}: {e.Message}");
            }
        }
    }
}
