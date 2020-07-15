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
  public class ContentService : PerpetualMarshalByRefObject, IContentProvider
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

        if (url.IsNullOrEmpty())
          return null;

        Match match = new Regex(UrlUtils.WikiGlossaryRegex).Match(url);
        bool matched = match.Success;
        if (!matched)
          return null;

        string title = match.Groups[1].Value;
        if (title.IsNullOrEmpty())
          return null;

        return GetSMGlossaryItem(ct, url, title);

      }
      catch (Exception ex)
      {
        LogTo.Error($"Failed to FetchHtml for url {url} with exception {ex}");
        throw;
      }
    }

    private async Task<PopupContent> GetSMGlossaryItem(RemoteCancellationToken ct, string url, string title)
    {

      string response = await GetAsync(ct.Token(), url);
      return CreatePopupContent(response, url);

    }

    private PopupContent CreatePopupContent(string content, string url)
    {

      if (content.IsNullOrEmpty() || url.IsNullOrEmpty())
        return null;

      var doc = new HtmlDocument();
      doc.LoadHtml(content);

      var defNode = doc.DocumentNode.SelectSingleNode("//dl");
      var titleNode = defNode.SelectSingleNode("//dt");

      if (defNode.IsNull() || titleNode.IsNull())
        return null;

      var definition = defNode.InnerText;
      var title = titleNode.InnerText;

      if (definition.IsNullOrEmpty() || title.IsNullOrEmpty())
        return null;

      string html = @"
          <html>
            <body>
              {0}
            </body>
          </html>";

      html = String.Format(html, definition);

      var refs = new References();
      refs.Author = "Piotr Wozniak";
      refs.Link = url;
      refs.Source = "SuperMemo Glossary";
      refs.Title = title;
      return new PopupContent(refs, html);

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
