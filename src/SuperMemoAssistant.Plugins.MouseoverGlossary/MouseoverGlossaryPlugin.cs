#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   7/15/2020 5:34:32 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Anotar.Serilog;
  using Ganss.Text;
  using mshtml;
  using SuperMemoAssistant.Extensions;
  using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
  using SuperMemoAssistant.Interop.SuperMemo.Core;
  using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.IO.HotKeys;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Services.UI.Configuration;
  using SuperMemoAssistant.Sys.Remoting;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class MouseoverGlossaryPlugin : SentrySMAPluginBase<MouseoverGlossaryPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public MouseoverGlossaryPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion


    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "MouseoverGlossary";

    /// <inheritdoc />
    public override bool HasSettings => true;

    public MouseoverGlossaryCfg Config;

    private const string ProviderName = "SuperMemo Glossary";

    /// <summary>
    /// Content provider to be registered with MouseoverPopup service
    /// </summary>
    private ContentService _contentProvider => new ContentService();

    /// <summary>
    /// Fast keyword search data structure
    /// </summary>
    private AhoCorasick Aho = new AhoCorasick(Keywords.KeywordMap.Keys);

    private int MaxTextLength = 2000000000;

    // Regex Arrays
    private string[] TitleRegexes => Config.ReferenceTitleRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] AuthorRegexes => Config.ReferenceAuthorRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] LinkRegexes => Config.ReferenceLinkRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] SourceRegexes => Config.ReferenceSourceRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] ConceptRegexes => Config.ConceptNameRegexes?.Replace("\r\n", "\n")?.Split('\n');

    #endregion

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<MouseoverGlossaryCfg>() ?? new MouseoverGlossaryCfg();
    }

    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    #region Methods Impl

    /// <inheritdoc />
    protected override void PluginInit()
    {

      LoadConfig();

      if (!this.RegisterProvider(ProviderName, new List<string> { UrlUtils.HelpGlossaryRegex, UrlUtils.GuruGlossaryRegex }, _contentProvider))
      {
        LogTo.Error($"Failed to Register provider {ProviderName} with MouseoverPopup Service");
        return;
      }

      LogTo.Debug($"Successfully registered provider {ProviderName} with MouseoverPopup Service");

      Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(ElementWdw_OnElementChanged);

    }

    private void ElementWdw_OnElementChanged(SMDisplayedElementChangedEventArgs obj)
    {

      var element = obj.NewElement;
      if (element.IsNull())
        return;

      if (CategoryPathMatches(element) || ReferenceMatches())
      {
        var htmlCtrls = ContentUtils.GetHtmlCtrls();
        if (htmlCtrls.IsNull() || !htmlCtrls.Any())
          return;

        foreach (KeyValuePair<int, IControlHtml> kvpair in htmlCtrls)
        {

          int idx = kvpair.Key;
          var htmlCtrl = kvpair.Value;
          var text = htmlCtrl?.Text?.ToLowerInvariant();
          var htmlDoc = htmlCtrl?.GetDocument();

          if (text.IsNullOrEmpty() || htmlDoc.IsNull())
            continue;

          var matches = Aho.Search(text);
          if (!matches.Any())
            continue;

          var orderedMatches = matches.OrderBy(x => x.Index);
          var selObj = htmlDoc.selection?.createRange() as IHTMLTxtRange;
          if (selObj.IsNull())
            continue;

          foreach (var match in orderedMatches)
          {

            string word = match.Word;
            if (selObj.findText(word))
            {

              var parentEl = selObj.parentElement();
              if (!parentEl.IsNull())
              {
                if (parentEl.tagName.ToLowerInvariant() == "a")
                  continue;
              }
              else
              {

                if (!Keywords.KeywordMap.TryGetValue(word, out var href))
                  continue;

                // selObj.pasteHTML($"<a href='{href}'><a>");

              }

            }

            selObj.collapse(false);
            selObj.moveEnd("character", MaxTextLength);

          }
            
        }
      }

    }

    private bool TryMatchRegexList(string input, string[] regexes)
    {

      if (input.IsNullOrEmpty())
        return false;

      if (regexes.IsNull() || !regexes.Any())
        return false;

      if (regexes.Any(r => new Regex(r).Match(input).Success))
        return true;

      return false;

    }

    private bool ReferenceMatches()
    {

      var htmlCtrl = ContentUtils.GetFirstHtmlCtrl();
      string text = htmlCtrl?.Text;
      if (text.IsNullOrEmpty())
        return false;

      var refs = ReferenceParser.GetReferences(htmlCtrl?.Text);
      if (refs.IsNull())
        return false;

      else if (TryMatchRegexList(refs.Source, SourceRegexes))
        return true;

      else if (TryMatchRegexList(refs.Link, LinkRegexes))
        return true;

      if (TryMatchRegexList(refs.Title, TitleRegexes))
        return true;

      else if (TryMatchRegexList(refs.Author, AuthorRegexes))
        return true;

      return false;

    }

    private bool CategoryPathMatches(IElement element)
    {

      if (element.IsNull())
        return false;

      if (ConceptRegexes.IsNull() || !ConceptRegexes.Any())
        return false;

      var cur = element.Parent;
      while (!cur.IsNull())
      {
        if (cur.Type == Interop.SuperMemo.Elements.Models.ElementType.ConceptGroup)
        {

          // TODO: Check that this works
          var concept = Svc.SM.Registry.Concept[cur.Id];
          string name = concept.Name;

          if (!concept.IsNull() && ConceptRegexes.Any(x => new Regex(x).Match(name).Success))
            return true;

        }
        cur = cur.Parent;
      }

      return false;

    }

    #endregion

    #region Methods

    #endregion
  }
}
