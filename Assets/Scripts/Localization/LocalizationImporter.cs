#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LocalizationImporter : EditorWindow
{
    [MenuItem("Tools/Import Localization From CSV")]
    public static void ImportLocalization()
    {
        // Путь к твоему скачанному общему CSV файлу
        string csvPath = Path.Combine(Application.dataPath, "OceanSpire_Localization.csv");
        // Папка, куда скрипт сам разложит готовые .json файлы
        string targetFolder = Path.Combine(Application.dataPath, "Localization/");

        if (!File.Exists(csvPath)) {
            Debug.LogError($"[Localization] Не найден исходный CSV файл по пути: {csvPath}");
            return;
        }

        // Создаем папку назначения, если её ещё нет
        if (!Directory.Exists(targetFolder)) {
            Directory.CreateDirectory(targetFolder);
        }

        // Читаем все строки из CSV
        string[] lines = File.ReadAllLines(csvPath, Encoding.UTF8);
        if (lines.Length == 0) return;

        // Парсим первую строчку (заголовки), чтобы понять, какие языки у нас есть
        string[] headers = SplitCsvLine(lines[0]);

        // Список языков (начиная со 2-й колонки, т.к. 1-я — это "Key")
        List<string> languages = new List<string>();
        for (int i = 1; i < headers.Length; i++) {
            if (!string.IsNullOrEmpty(headers[i])) {
                languages.Add(headers[i].Trim()); // "en-US", "ru-RU" и т.д.
            }
        }

        // Подготавливаем словари для сборки JSON под каждый язык
        Dictionary<string, Dictionary<string, string>> localizationDb = new Dictionary<string, Dictionary<string, string>>();
        foreach (var lang in languages) {
            localizationDb[lang] = new Dictionary<string, string>();
        }

        // Идем по всем остальным строкам таблицы (пропуская заголовки)
        for (int i = 1; i < lines.Length; i++) {
            if (string.IsNullOrEmpty(lines[i])) continue;

            string[] columns = SplitCsvLine(lines[i]);
            if (columns.Length == 0) continue;

            string key = columns[0].Trim(); // Наш ключ локализации
            if (string.IsNullOrEmpty(key)) continue;

            // Заполняем переводы для каждого языка в этой строке
            for (int col = 1; col < columns.Length; col++) {
                if (col - 1 >= languages.Count) break;

                string langStr = languages[col - 1];
                string translation = col < columns.Length ? columns[col].Trim() : "";

                // Записываем в словарь этого языка
                localizationDb[langStr][key] = translation;
            }
        }

        // Генерируем отдельные JSON файлы для каждого языка
        foreach (var lang in languages) {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.AppendLine("{");

            var translations = localizationDb[lang];
            int count = 0;

            foreach (var kvp in translations) {
                count++;
                // Экранируем кавычки в тексте, чтобы JSON не ломался
                string safeValue = kvp.Value.Replace("\"", "\\\"");
                string comma = (count < translations.Count) ? "," : "";

                jsonBuilder.AppendLine($"  \"{kvp.Key}\": \"{safeValue}\"{comma}");
            }

            jsonBuilder.AppendLine("}");

            // Сохраняем файл (например, Assets/Resources/Localization/en-US.json)
            string fileOutputPath = Path.Combine(targetFolder, $"{lang}.json");
            File.WriteAllText(fileOutputPath, jsonBuilder.ToString(), Encoding.UTF8);
            Debug.Log($"[Localization] Сгенерирован файл: {lang}.json");
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Локализация", $"Успешно импортировано {languages.Count} языков!", "ОК");
    }

    // Вспомогательный метод для корректного разделения CSV с учетом запятых внутри текстов
    private static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder currentToken = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++) {
            char c = line[i];
            if (c == '"') {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes) {
                result.Add(currentToken.ToString());
                currentToken.Clear();
            }
            else {
                currentToken.Append(c);
            }
        }
        result.Add(currentToken.ToString());
        return result.ToArray();
    }
}
#endif