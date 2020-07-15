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
  using Anotar.Serilog;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.IO.HotKeys;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Services.UI.Configuration;

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

    }


    #endregion

    #region Methods

    #endregion
  }
}
