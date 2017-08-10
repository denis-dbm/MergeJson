using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text;
using static System.Console;

namespace MergeJson
{
    class Program
    {
        const int Success = 0;
        const int GenericError = 1;

        static int Main(string[] args)
        {
            var configurationPath = args.Length > 0 ? args[0] : null;
            var contentToMergePath = args.Length > 1 ? args[1] : null;
            var othersOptions = args.Length > 2 ? args.Skip(2) : new string[0];
            
            if (!File.Exists(configurationPath) || !File.Exists(contentToMergePath))
            {
                WriteLine("Arquivos de entrada incorretos. Sintaxe: merj <path arquivo matriz> <path arquivo conteudo a mesclar> [-bckp|--backup]");
                return GenericError;
            }

            var configuration = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(configurationPath, Encoding.UTF8));
            var contentToMerge = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(contentToMergePath, Encoding.UTF8));

            switch (contentToMerge.Type)
            {
                case JTokenType.Object:
                    Merge(configuration, contentToMerge);
                    break;
                default:
                    WriteLine("Arquivo de conteúdo inválido. A raíz do conteúdo do arquivo deve ser um objeto");
                    return GenericError;
            }

            if (othersOptions.Contains("-bckp") || othersOptions.Contains("--backup"))
            {
                DoBackup(configurationPath);
            }

            File.WriteAllText(configurationPath, JsonConvert.SerializeObject(configuration, Formatting.Indented), Encoding.UTF8);
            return Success;
        }

        private static void Merge(JObject obj, JObject content)
        {
            obj.Merge(content, new JsonMergeSettings()
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });
        }

        private static void DoBackup(string path)
        {
            var newPath = Path.ChangeExtension(path, "json.bckp");

            if (File.Exists(newPath))
                File.Delete(newPath);

            File.Move(path, newPath);
        }
    }
}