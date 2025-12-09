using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using View.Model;

namespace View.Services
{
    public class ContactSerializer
    {
        private readonly string _filePath;

        public ContactSerializer(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(IEnumerable<Contact> contacts)
        {
            var json = JsonSerializer.Serialize(
                contacts,
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(_filePath, json);
        }

        public List<Contact> Load()
        {
            if (!File.Exists(_filePath))
                return new List<Contact>();

            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<List<Contact>>(json)
                   ?? new List<Contact>();
        }
    }
}
