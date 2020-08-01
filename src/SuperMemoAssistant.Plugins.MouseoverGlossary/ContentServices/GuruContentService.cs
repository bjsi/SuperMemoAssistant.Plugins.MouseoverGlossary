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

namespace SuperMemoAssistant.Plugins.MouseoverGlossary.ContentServices
{

  [Serializable]
  class GuruContentService : ContentServiceBase, IMouseoverContentProvider
  {
    public RemoteTask<PopupContent> FetchHtml(RemoteCancellationToken ct, string url)
    {

      try
      {

        if (url.IsNullOrEmpty() || ct.IsNull())
          return null;

        Match match = new Regex(UrlUtils.GuruGlossaryRegex).Match(url);

        if (!match.Success)
          return null;

        if (!UrlUtils.GuruGlossaryTerms.Any(x => x == match.Groups[1].Value))
          return null;

        string term = match.Groups[1].Value;
        if (term.IsNullOrEmpty())
          return null;

        return GetGuruGlossaryItem(ct, url, term);

      }
      catch (TaskCanceledException) { }
      catch (Exception ex)
      {
        LogTo.Error($"Failed to FetchHtml for url {url} with exception {ex}");
        throw;
      }

      return null;

    }

    private async Task<PopupContent> GetGuruGlossaryItem(RemoteCancellationToken ct, string url, string term)
    {

      string response = await GetAsync(ct.Token(), url);
      return CreateGuruGlossaryContent(response, url, term);

    }

    private PopupContent CreateGuruGlossaryContent(string content, string url, string term)
    {

      if (content.IsNullOrEmpty() || url.IsNullOrEmpty() || term.IsNullOrEmpty())
        return null;

      var doc = new HtmlDocument();
      doc.LoadHtml(content);

      doc = doc.ConvRelToAbsLinks("https://supermemo.guru/");

      var titleNode = doc.DocumentNode.Descendants().Where(x => x.Id == "firstHeading").FirstOrDefault();
      var contentNode = doc.DocumentNode.Descendants().Where(x => x.Id == "mw-content-text").FirstOrDefault();

      if (titleNode.IsNull() || contentNode.IsNull())
        return null;

      string title = titleNode.OuterHtml;
      string definition = contentNode.OuterHtml;

      if (title.IsNullOrEmpty() || definition.IsNullOrEmpty())
        return null;

      string html = @"
          <html>
            <body>
              <h1>{0}</h1>
              <p>{1}</p>
            </body>
          </html>";

      html = string.Format(html, title, definition);

      var refs = new References();
      refs.Title = titleNode.InnerText;
      refs.Author = "Piotr Wozniak";
      refs.Link = url;
      refs.Source = "SuperMemo Guru Glossary";

      return new PopupContent(refs, html, true, browserQuery: url, editUrl: $"https://supermemo.guru/index.php?title={term}&action=edit");

    }
  }
}
