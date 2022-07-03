using System.IO;
using Newtonsoft.Json;

namespace SystemTests.ServiceCalls.Environment {
    public static class ValueHelper {

        private static readonly string Filename = "testdata.json";
        private static readonly string DataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData); 


        public static void ClearJsonTestData() {
            if (File.Exists(GetFilename())) { File.Delete(GetFilename()); }
        }

        public static void SaveJsonTestData(object testData) {
            var json = JsonConvert.SerializeObject(testData);
            File.WriteAllText(GetFilename(), json);
        }

        public static T GetJsonTestData<T>() {

            var jsonStr = File.ReadAllText(GetFilename());
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

        private static string GetFilename() => $"{DataFolder}/{Filename}";
    }
}