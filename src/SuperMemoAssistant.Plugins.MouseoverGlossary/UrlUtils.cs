using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.MouseoverGlossary
{
  public static class UrlUtils
  {
    public static readonly string WikiGlossaryRegex = @"^https?\:\/\/help\.supermemo\.org\/wiki\/Glossary\:([\w]+)+";
    // TODO:
    //private const string WikiGlossary = @"^https?\:\/\/([\w\.]+)supermemo.guru\/wiki\/Glossary([\w]+)+";
    // Wiki glossary https://supermemo.guru/wiki/Glossary
  }
}
