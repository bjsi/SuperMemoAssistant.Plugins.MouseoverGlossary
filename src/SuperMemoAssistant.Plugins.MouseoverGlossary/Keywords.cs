using Anotar.Serilog;
using SuperMemoAssistant.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  public static class Keywords
  {

    private const string GuruJsonPath = @"C:\Users\james\SuperMemoAssistant\Plugins\Development\SuperMemoAssistant.Plugins.MouseoverCSDict\dictionary\guru_dictionary_entries";
    private const string HelpJsonPath = @"C:\Users\james\SuperMemoAssistant\Plugins\Development\SuperMemoAssistant.Plugins.MouseoverCSDict\dictionary\help_dictionary_entries";

    public static Dictionary<string, string> GuruKeywordMap => CreateKeywordMap(GuruJsonPath);
    public static Dictionary<string, string> HelpKeywordMap => CreateKeywordMap(HelpJsonPath);

    private static Dictionary<string, string> CreateKeywordMap(string jsonPath)
    {
      // Copied manually to development plugins folder
      // var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
      // TODO: Wouldn't work unless hard coded ?????????????

      try
      {

        using (StreamReader r = new StreamReader(jsonPath))
        {

          string json = r.ReadToEnd();
          return json.Deserialize<Dictionary<string, string>>();

        }
      }
      catch (FileNotFoundException)
      {

        LogTo.Error($"Failed to CreateKeywordMap because {jsonPath} does not exist");
        return null;

      }
      catch (IOException e)
      {

        LogTo.Error($"Exception {e} thrown when attempting to read from {jsonPath}");
        return null;

      }
    }

  }
}
