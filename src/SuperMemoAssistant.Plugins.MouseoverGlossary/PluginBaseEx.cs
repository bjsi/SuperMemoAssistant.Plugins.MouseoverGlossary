using MouseoverPopup.Interop;
using SuperMemoAssistant.Interop.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  public static class PluginBaseEx
  {

    public static bool RegisterProvider<T>(this SMAPluginBase<T> plugin, string name, List<string> urlRegexes, Dictionary<string, string> keywordUrlMap, string color, IMouseoverContentProvider provider) where T : SMAPluginBase<T>
    {
      var svc = plugin.GetService<IMouseoverSvc>();

      if (svc == null)
        return false;

      return svc.RegisterProvider(name, urlRegexes, keywordUrlMap, color, provider);
    }

  }
}
