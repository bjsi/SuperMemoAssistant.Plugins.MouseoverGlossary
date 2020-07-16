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

    private ContentService _contentProvider => new ContentService();
    private const string ProviderName = "SuperMemo Glossary";
    private readonly Dictionary<string, string> GlossaryTermUrlMap = new Dictionary<string, string>
    {
      { "", "" },
      { "", "" },
      { "", "" },
      { "", "" },
      { "", "" },
      { "", "" },
    };
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

      if (!this.RegisterProvider(ProviderName, new List<string> { UrlUtils.WikiGlossaryRegex }, _contentProvider))
      {
        LogTo.Error($"Failed to Register provider {ProviderName} with MouseoverPopup Service");
        return;
      }
      LogTo.Debug($"Successfully registered provider {ProviderName} with MouseoverPopup Service");

      //Svc.SM.UI.ElementWdw.OnElementChanged += new ActionProxy<SMDisplayedElementChangedEventArgs>(ElementWdw_OnElementChanged);

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


        // TODO: Use proper keyword search algo
        foreach (KeyValuePair<int, IControlHtml> kvpair in htmlCtrls)
        {

          int idx = kvpair.Key;
          var htmlCtrl = kvpair.Value;
          var words = htmlCtrl?.Text?.Split((char[])null);
          if (words.IsNull() || words.Length == 0)
            continue;

          foreach (var word in words)
          {
          }
        }
      }

    }

    private bool ReferenceMatches()
    {

      var htmlCtrl = ContentUtils.GetFirstHtmlCtrl();
      string text = htmlCtrl?.Text;
      if (text.IsNullOrEmpty())
        return false;

      var refs = ReferenceParser.GetReferences(htmlCtrl?.Text);

      string[] SMRegexes = Config.ReferenceRegexes
        ?.Replace("\r\n", "\n")
        ?.Split('\n');

      if (SMRegexes.IsNull() || !SMRegexes.Any())
        return false;

      foreach (var pattern in SMRegexes)
      {
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        if (regex.Match(refs.Link).Success || regex.Match(refs.Source).Success)
        {
          return true;
        }
      }

      return false;

    }

    private bool CategoryPathMatches(IElement element)
    {

      if (element.IsNull())
        return false;

      var conceptRegexes = Config.ConceptRegexes
        ?.Replace("\r\n", "\n")
        ?.Split('\n');

      if (conceptRegexes.IsNull() || !conceptRegexes.Any())
        return false;

      var cur = element.Parent;
      while (!cur.IsNull())
      {
        if (cur.Type == Interop.SuperMemo.Elements.Models.ElementType.ConceptGroup)
        {

          // TODO: Check that this works
          var concept = Svc.SM.Registry.Concept[cur.Id];
          if (!concept.IsNull() && conceptRegexes.Any(x => new Regex(x).Match(concept.Name).Success))
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
