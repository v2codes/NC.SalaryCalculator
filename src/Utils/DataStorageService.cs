using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace NC.SalaryCalculator.Utils
{
    public class DataStorageService
    {

        private readonly IConfiguration _configuration;
        private readonly StringEncryptionService _stringEncryptionService;
        private readonly string _rootSection = "App";
        private readonly string _localIniFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "local.ini");

        private static DataStorageService _instance;
        public static DataStorageService Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    _instance = new DataStorageService();
                }
                return _instance;
            }
        }

        public DataStorageService()
        {
            if (!File.Exists(_localIniFile))
            {
                CreateIniFile();
            }

            _configuration = new ConfigurationBuilder()
                                    .AddIniFile(_localIniFile, optional: true, reloadOnChange: true)
                                    .Build();
            _stringEncryptionService = new StringEncryptionService();
        }

        private void CreateIniFile()
        {
            using StreamWriter streamWriter = File.CreateText(_localIniFile);
            streamWriter.WriteLine($"[App]");
            streamWriter.Flush();
            streamWriter.Close();
        }

        public T GetValue<T>(string key, T defaultValue = default!, bool shouldDecrpyt = false)
        {
            T value;
            if (TypeHelperExtended.IsPrimitive(typeof(T), false))
            {
                value = GetPrimitive(key, defaultValue, shouldDecrpyt);
            }
            else
            {
                value = RetrieveObject(key, defaultValue, shouldDecrpyt);
            }
            return value;
        }

        public void SetValue<T>(string key, T value, bool shouldEncrypt = false)
        {
            if (TypeHelperExtended.IsPrimitive(typeof(T), false))
            {
                if (shouldEncrypt)
                    InternalSetValue(key, _stringEncryptionService.Encrypt(Convert.ToString(value))!);
                else
                    InternalSetValue(key, value!);
            }
            else
            {
                SetJsonValue(key, value!, shouldEncrypt);
            }
        }

        public void SetValues<T>(Dictionary<string, T> keyValuePairs, bool shouldEncrypt = false)
        {
            foreach (var kv in keyValuePairs)
            {
                SetValue(kv.Key, kv.Value, shouldEncrypt);
            }
        }

        private void InternalSetValue(string key, object value)
        {
            if (value == null)
                AppendToIniFile(key, string.Empty);
            else
                AppendToIniFile(key, value.ToString()!);
        }

        private void SetJsonValue(string key, object value, bool shouldEncrypt = false)
        {
            var jsonString = JsonSerializer.Serialize(value, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (shouldEncrypt)
            {
                AppendToIniFile(key, _stringEncryptionService.Encrypt(jsonString)!);
            }
            else
            {
                AppendToIniFile(key, jsonString);
            }
        }
        private void AppendToIniFile(string key, string value)
        {
            var lines = File.ReadAllLines(_localIniFile);
            bool exists = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"{key}={value}";
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                var newLines = lines.ToList();
                newLines.Add($"{key}={value}");
                lines = newLines.ToArray();
            }

            File.WriteAllLines(_localIniFile, lines);
        }
        public void Remove(string key)
        {
            RemoveFromIniFile(key);
        }

        private void RemoveFromIniFile(string key)
        {
            var lines = File.ReadAllLines(_localIniFile);
            var newLines = lines.Where(p => p.StartsWith(key, StringComparison.OrdinalIgnoreCase));
            File.WriteAllLines(_localIniFile, newLines);
        }

        private T GetPrimitive<T>(string key, T defaultValue = default!, bool shouldEncrypt = false)
        {
            var value = _configuration[$"{_rootSection}:{key}"];
            if (shouldEncrypt)
            {
                value = _stringEncryptionService.Decrypt(value);
            }
            return (T)Convert.ChangeType(value, typeof(T))!;
        }

        private T RetrieveObject<T>(string key, T defaultValue = default!, bool shouldEncrypt = false)
        {
            var jsonString = _configuration[$"{_rootSection}:{key}"];
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return defaultValue;
            }

            if (shouldEncrypt)
            {
                jsonString = _stringEncryptionService.Decrypt(jsonString);
            }
            return JsonSerializer.Deserialize<T>(jsonString!)!;
        }
    }
}
