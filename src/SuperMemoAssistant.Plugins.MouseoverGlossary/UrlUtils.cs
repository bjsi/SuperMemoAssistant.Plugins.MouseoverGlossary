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
    public static readonly string HelpGlossaryRegex = @"^https?\:\/\/(?:www\.)?help\.supermemo\.org\/wiki\/Glossary\:([\w]+)+";

    public static readonly string GuruGlossaryRegex = @"^https?\:\/\/(?:www\.)?supermemo\.guru\/wiki\/([\w-]+)+";

    public static readonly string[] GuruGlossaryTerms = new string[]
    {

      "Abstractness",
      "Knowledge_acquisition_rate",
      "Active_recall",
      "Adaptability",
      "Applicability",
      "Behavioral_space",
      "Behavioral_system",
      "Biphasic_sleep",
      "Circadian_cycle",
      "Circadian_phase",
      "Circadian_sleep_propensity",
      "Childhood_amnesia",
      "Chronic_stress",
      "Cloze_deletion",
      "Coherence",
      "Concept_network",
      "Conceptual_computation",
      "Conceptualization",
      "Consistency",
      "Consolidation",
      "Democratic_school",
      "Deschooling",
      "Factory_model_of_education",
      "Forgetting_curve",
      "Free_learning",
      "Free_running_sleep",
      "Fundamental_law_of_learning",
      "Generalization",
      "Homeostatic_sleep_propensity",
      "Homeschooling",
      "Idiocracy_problem",
      "Increading",
      "Incremental_learning",
      "Incremental_reading",
      "Incremental_video",
      "Interference",
      "Learn_drive",
      "Learned_helplessness",
      "Learntropy",
      "Knowledge_darwinism",
      "Knowledge_redundancy",
      "Knowledge_valuation network",
      "Memory_complexity",
      "Memory_optimization",
      "Minimum_information_principle",
      "Natural_creativity cycle",
      "Passive_review",
      "Passive_schooling",
      "Pattern_completion",
      "Permastore",
      "Polyphasic_sleep",
      "Problem_valuation_network",
      "Push_zone",
      "Priority_queue",
      "Rational_procrastination",
      "Redundancy",
      "Regress_zone",
      "Retrievability",
      "Self-directed_learning",
      "Self-learning",
      "Semantic_learning",
      "Sleep_phase",
      "Spaced_repetition",
      "Spacing_effect",
      "Spacing_effect_gain",
      "Stability",
      "Stabilization_curve",
      "Stabilization_decay",
      "Toxic_memory",
      "Two_component_model_of_memory",
      "Two-process_model_of_sleep_regulation",
      "Unschooling",
      "Variable_reward",
      "War_of_the_networks",

    };

    public static string ConvRelToAbsLink(string baseUrl, string relUrl)
    {
      if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(relUrl))
      {
        // UriKind.Relative will be false for rel urls containing #
        if (Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
        {
          if (baseUrl.EndsWith("/"))
          {
            baseUrl = baseUrl.TrimEnd('/');
          }

          if (relUrl.StartsWith("/") && !relUrl.StartsWith("//"))
          {
            if (relUrl.StartsWith("/wiki") || relUrl.StartsWith("/w/"))
            {
              return $"{baseUrl}{relUrl}";
            }
            return $"{baseUrl}/wiki{relUrl}";
          }
          else if (relUrl.StartsWith("./"))
          {
            if (relUrl.StartsWith("./wiki") || relUrl.StartsWith("./w/"))
            {
              return $"{baseUrl}{relUrl.Substring(1)}";
            }
            return $"{baseUrl}/wiki{relUrl.Substring(1)}";
          }
          else if (relUrl.StartsWith("#"))
          {
            return $"{baseUrl}/wiki/{relUrl}";
          }
          else if (relUrl.StartsWith("//"))
          {
            return $"https:{relUrl}";
          }
        }
      }
      return relUrl;
    }
  }
}
