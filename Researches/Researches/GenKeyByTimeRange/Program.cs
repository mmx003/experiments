// See https://aka.ms/new-console-template for more information

using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        var time = new DateTime(2024, 1, 1, 0, 59, 59);
        for (int i = 0; i < 15; i++)
        {
            // Получаем текущее время и округляем его до ближайшего интервала 5 секунд
            DateTime currentTime = time;
            var roundedTiks = RoundToNearestInterval(currentTime, TimeSpan.FromSeconds(5));

            // Вычисляем хэш от округленного времени
            var hashedTime = CalculateHash(roundedTiks.ToString());

            Console.WriteLine("Текущее время: " + currentTime + "Хэш: " + hashedTime);

            time = time.AddSeconds(1);
        }
    }

    // Округляем время до ближайшего интервала
    private static long RoundToNearestInterval(DateTime dt, TimeSpan d)
    {
        var delta = dt.Ticks / d.Ticks;
        return delta * d.Ticks;
    }

    // Вычисляем хэш
    private static string CalculateHash(string input)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}