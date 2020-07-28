using Anotar.Serilog;
using HtmlAgilityPack;
using MouseoverPopup.Interop;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  
  [Serializable]
  class HelpContentService : ContentServiceBase, IMouseoverContentProvider
  {

    public RemoteTask<PopupContent> FetchHtml(RemoteCancellationToken ct, string url)
    {
      try
      {

        if (url.IsNullOrEmpty() || ct.IsNull())
          return null;

        Match Help = new Regex(UrlUtils.HelpGlossaryRegex).Match(url);

        if (!Help.Success)
          return null;

        string term = Help.Groups[1].Value;
        if (term.IsNullOrEmpty())
          return null;

        return GetHelpGlossaryItem(ct, url, term);

      }
      catch (TaskCanceledException) { }
      catch (Exception ex)
      {
        LogTo.Error($"Failed to FetchHtml for url {url} with exception {ex}");
        throw;
      }

      return null;
    }

    private async Task<PopupContent> GetHelpGlossaryItem(RemoteCancellationToken ct, string url, string term)
    {

      string response = await GetAsync(ct.Token(), url);
      return CreateHelpGlossaryContent(response, url, term);

    }

    private PopupContent CreateHelpGlossaryContent(string content, string url, string term)
    {

      if (content.IsNullOrEmpty() || url.IsNullOrEmpty() || term.IsNullOrEmpty())
        return null;

      var doc = new HtmlDocument();
      doc.LoadHtml(content);

      doc = doc.ConvRelToAbsLinks("https://www.help.supermemo.org");

      var defNode = doc.DocumentNode.SelectSingleNode("//dl");
      var titleNode = defNode.SelectSingleNode("//dt");

      if (defNode.IsNull() || titleNode.IsNull())
        return null;

      var definition = defNode.OuterHtml;
      var title = titleNode.OuterHtml;

      if (definition.IsNullOrEmpty() || title.IsNullOrEmpty())
        return null;

      string html = @"
          <html>
            <body>
              <h1>{0}</h1>
              <p>{1}</p>
            </body>
          </html>";

      html = String.Format(html, title, definition);


      var refs = new References();
      refs.Author = "Piotr Wozniak";
      refs.Link = url;
      refs.Source = "SuperMemo Help Glossary";
      refs.Title = titleNode.InnerText;
      return new PopupContent(refs, html, true, browserQuery: refs.Link, editUrl: $"https://www.help.supermemo.org/index.php?title=Glossary:{term}&action=edit");

    }
  }
}
