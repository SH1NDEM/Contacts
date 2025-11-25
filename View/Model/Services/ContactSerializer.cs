using Newtonsoft.Json;     
using System;
using System.IO;
using View.Model;


namespace Model.Services
{
    public class ContactSerializer
    {
        // Путь до файла (по умолчанию)
        public string FilePath { get; set; }


        public ContactSerializer()
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            FilePath = Path.Combine(docPath, "Contacts", "contacts.json");
        }

        // Сохранение контакта в файл
        public void Save(Contact contact)
        {
            // Создаём директорию если её нет
            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonConvert.SerializeObject(contact, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        // Загрузка контакта из файла
        public Contact Load()
        {
            if (!File.Exists(FilePath))
                return new Contact(); // если файла нет — возвращаем пустой контакт

            string json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<Contact>(json);
        }
    }
}
