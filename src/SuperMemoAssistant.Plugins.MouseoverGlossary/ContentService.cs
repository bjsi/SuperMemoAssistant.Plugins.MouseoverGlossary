using Anotar.Serilog;
using HtmlAgilityPack;
using MouseoverPopup.Interop;
using PluginManager.Interop.Sys;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Sys.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  public class ContentService : PerpetualMarshalByRefObject, IMouseoverContentProvider
  {

    private readonly HttpClient _httpClient;

    public ContentService()
    {
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Accept.Clear();
      _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
    }

    public RemoteTask<PopupContent> FetchHtml(RemoteCancellationToken ct, string url)
    {
      try
      {

        if (url.IsNullOrEmpty() || ct.IsNull())
          return null;

        Match Help = new Regex(UrlUtils.HelpGlossaryRegex).Match(url);
        Match Guru = new Regex(UrlUtils.GuruGlossaryRegex).Match(url);

        if (!(Guru.Success || Help.Success))
          return null;

        if (Guru.Success && UrlUtils.GuruGlossaryTerms.Any(x => x == Guru.Groups[1].Value))
        {

          string term = Guru.Groups[1].Value;
          return GetGuruGlossaryItem(ct, url, term);

        }
        else
        {

          string term = Help.Groups[1].Value;
          return GetHelpGlossaryItem(ct, url, term);

        }

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
      refs.Title = title;
      refs.Author = "Piotr Wozniak";
      refs.Link = url;
      refs.Source = "SuperMemo Guru Glossary";

      return new PopupContent(refs, html, true, browserQuery: url, editUrl: $"https://supermemo.guru/index.php?title={term}&action=edit");

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

    private async Task<string> GetAsync(CancellationToken ct, string url)
    {
      HttpResponseMessage responseMsg = null;

      try
      {
        responseMsg = await _httpClient.GetAsync(url, ct);

        if (responseMsg.IsSuccessStatusCode)
        {
          return await responseMsg.Content.ReadAsStringAsync();
        }
        else
        {
          return null;
        }
      }
      catch (HttpRequestException)
      {
        if (responseMsg != null && responseMsg.StatusCode == System.Net.HttpStatusCode.NotFound)
          return null;
        else
          throw;
      }
      catch (OperationCanceledException)
      {
        return null;
      }
      finally
      {
        responseMsg?.Dispose();
      }
    }
  }
}
