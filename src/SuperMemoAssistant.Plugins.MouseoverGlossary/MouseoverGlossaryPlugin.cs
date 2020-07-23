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
  using MouseoverPopup.Interop;
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

    // Reference Regexes
    private string[] TitleRegexes => Config.ReferenceTitleRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] AuthorRegexes => Config.ReferenceAuthorRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] LinkRegexes => Config.ReferenceLinkRegexes?.Replace("\r\n", "\n")?.Split('\n');
    private string[] SourceRegexes => Config.ReferenceSourceRegexes?.Replace("\r\n", "\n")?.Split('\n');

    // Category Path Regexes
    private string[] CategoryPathRegexes => Config.ConceptNameRegexes?.Replace("\r\n", "\n")?.Split('\n');

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

      var referenceRegexes = new ReferenceRegexes(TitleRegexes, AuthorRegexes, LinkRegexes, SourceRegexes);
      KeywordScanningOptions opts = new KeywordScanningOptions(referenceRegexes, Keywords.KeywordMap, CategoryPathRegexes);

      // Register with MouseoverPopup
      if (!this.RegisterProvider(ProviderName, new string[] { UrlUtils.HelpGlossaryRegex, UrlUtils.GuruGlossaryRegex }, opts, _contentProvider))
      {
        LogTo.Error($"Failed to Register provider {ProviderName} with MouseoverPopup Service");
        return;
      }

      LogTo.Debug($"Successfully registered provider {ProviderName} with MouseoverPopup Service");

    }

    #endregion

    #region Methods

    #endregion
  }
}
