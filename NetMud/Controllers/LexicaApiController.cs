using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    public class LexicaApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        [Route("api/LexicaApi/Describe/{language}", Name = "LexicaAPI_Describe")]
        public JsonResult<LexicalOutput> Describe(string language, int quality, int elegance, int severity, string request)
        {
            ILanguage lang = ConfigDataCache.Get<ILanguage>(new ConfigDataCacheKey(typeof(ILanguage), language, ConfigDataType.Language));
            bool success = false;
            List<string> errors = new List<string>();

            // Describe { Paragraphs : [{ Context, Paragraph}] } }
            // Context { Anonymize, GenderForm, Tense, Position, Perspective, Semantics : [string] }
            // Paragraph { Events : [Event] }
            // Event { Role, Type, Phrase, Modifiers : [Event] }

            dynamic parsedRequest = JValue.Parse(request);

            List<ILexicalParagraph> paragraphs = new List<ILexicalParagraph>();
            foreach (dynamic paragraphPair in parsedRequest.Paragraphs)
            {
                dynamic paragraph = paragraphPair.Paragraph;
                if (paragraph != null)
                {
                    LexicalContext globalContext = new LexicalContext();
                    if (paragraphPair.Context != null)
                    {
                        dynamic context = paragraphPair.Context;
                        globalContext.Anonymize = context.Anonymize ?? false;
                        globalContext.GenderForm = context.GenderForm ?? false;
                        globalContext.Tense = context.Tense ?? LexicalTense.Past;
                        globalContext.Position = context.Position ?? LexicalPosition.None;
                        globalContext.Perspective = context.Perspective ?? NarrativePerspective.ThirdPerson;
                        globalContext.Semantics = context.Semantics ?? new HashSet<string>();
                        globalContext.Language = lang;
                    }

                    List<ISensoryEvent> lexEvents = new List<ISensoryEvent>();
                    foreach (dynamic lexEvent in paragraph.Events)
                    {
                        if (lexEvent != null && !string.IsNullOrWhiteSpace(lexEvent.Phrase))
                        {
                            ILexica lex = ParseLexica(globalContext, lexEvent);

                            SensoryEvent mainEvent = new SensoryEvent();
                            mainEvent.SensoryType = MessagingType.Visible;
                            mainEvent.Strength = short.MaxValue;
                            mainEvent.Event = lex;
                            lexEvents.Add(mainEvent);
                        }
                    }

                    paragraphs.Add(new LexicalParagraph(lexEvents));
                }
            }

            StringBuilder description = new StringBuilder();
            foreach (var graph in paragraphs)
            {
                description.AppendLine(graph.Describe());
            }

            LexicalOutput returnValue = new LexicalOutput()
            {
                Success = success,
                Description = description.ToString(),
                Errors = errors.ToArray()
            };

            return Json(returnValue);
        }

        private static ILexica ParseLexica(LexicalContext globalContext, dynamic lexEvent)
        {
            ILexica lex = new Lexica
            {
                Role = lexEvent.Role ?? GrammaticalType.None,
                Type = lexEvent.Type ?? LexicalType.None,
                Phrase = lexEvent.Phrase,
                Context = globalContext
            };

            foreach (dynamic modifier in lexEvent.Modifiers)
            {
                if (modifier != null && !string.IsNullOrWhiteSpace(modifier.Phrase))
                {
                    ParseLexica(globalContext, modifier);
                    ILexica newModifier = new Lexica()
                    {
                        Role = modifier.Role ?? GrammaticalType.None,
                        Type = modifier.Type ?? LexicalType.None,
                        Phrase = modifier.Phrase,
                        Context = globalContext
                    };

                    lex.Modifiers.Add(newModifier);
                }
            }

            return lex;
        }
    }
}
