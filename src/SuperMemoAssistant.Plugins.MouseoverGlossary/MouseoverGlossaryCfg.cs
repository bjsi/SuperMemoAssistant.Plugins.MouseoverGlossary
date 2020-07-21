using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  [Form(Mode = DefaultFields.None)]
  [Title("Dictionary Settings",
       IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
      "Cancel",
      IsCancel = true)]
  [DialogAction("save",
      "Save",
      IsDefault = true,
      Validates = true)]
  public class MouseoverGlossaryCfg : CfgBase<MouseoverGlossaryCfg>, INotifyPropertyChangedEx
  {

    [Title("Mouseover Glossary Plugin")]

    [Heading("By Jamesb | Experimental Learning")]

    [Heading("Features:")]
    [Text(@"- Open glossary definitions for SuperMemo in popup windows
- Supports glossary definitions from help.supermemo.org and supermemo.guru")]

    [Heading("General Settings")]

    [Field(Name = "Scan SM-related articles for glossary terms?")]
    public bool ScanElements { get; set; } = true;

    [Heading("Keyword Scanning Settings")]

    [Heading("Reference Regexes")]
    [Field(Name = "Title Regexes")]
    [MultiLine]
    public string ReferenceTitleRegexes { get; set; }

    [Field(Name = "Author Regexes")]
    [MultiLine]
    public string ReferenceAuthorRegexes { get; set; } = @"
piotr wozniak
woz";

    [Field(Name = "Source Regexes")]
    public string ReferenceSourceRegexes { get; set; } = @"
.*supermemo.*
.*super-memo.*
.*SM.*";

    [Field(Name = "Link Regexes")]
    public string ReferenceLinkRegexes { get; set; } = @"
.*supermemo.*";

    [Field(Name = "Concept Regexes")]
    [MultiLine]
    public string ConceptNameRegexes { get; set; }

    [Field(Name = "Highlight Color")]
    public string KeywordHighlightColor { get; set; }

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "Mouseover Glossary Settings";
    }

    public event PropertyChangedEventHandler PropertyChanged;

  }
}
