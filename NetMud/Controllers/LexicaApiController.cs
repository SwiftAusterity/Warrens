using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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

        [HttpPost]
        public JsonResult<LexicalOutput> Describe()
        {
            string payload = Request.Content.ReadAsStringAsync().Result;
            JToken parsedRequest = JValue.Parse(payload);

            ILanguage lang = ConfigDataCache.Get<ILanguage>(new ConfigDataCacheKey(typeof(ILanguage), parsedRequest.Value<string>("language"), ConfigDataType.Language));
            bool success = false;
            List<string> errors = new List<string>();

            // { language : "", severity : #, elegance : #, quality : #, Paragraphs : [{ Context, Paragraph}] } }
            // Context { Anonymize, GenderForm, Tense, Position, Perspective, Semantics : [string] }
            // Paragraph { Events : [Event] }
            // Event { Role, Type, Phrase, Modifiers : [Event] }

            List<ILexicalParagraph> paragraphs = new List<ILexicalParagraph>();
            foreach (JToken paragraphPair in parsedRequest.Value<JArray>("paragraphs").Values<JToken>())
            {
                JToken paragraph = paragraphPair.Value<JToken>("paragraph");
                if (paragraph != null)
                {
                    LexicalContext globalContext = new LexicalContext();
                    JToken context = paragraphPair.Value<JToken>("context");
                    if (context != null)
                    {
                        
                        globalContext.Anonymize = context.Value<bool>("anonymize");
                        globalContext.GenderForm = context.Value<bool>("genderform");
                        globalContext.Tense = context.Value<LexicalTense>("tense");
                        globalContext.Position = context.Value<LexicalPosition>("position");
                        globalContext.Perspective = context.Value<NarrativePerspective>("perspective");
                        globalContext.Semantics = context.Value<HashSet<string>>("semantics") ?? new HashSet<string>();
                        globalContext.Language = lang;
                        globalContext.Severity = parsedRequest.Value<int>("severity");
                        globalContext.Elegance = parsedRequest.Value<int>("elegance");
                        globalContext.Quality = parsedRequest.Value<int>("quality");
                    }

                    List<ISensoryEvent> lexEvents = new List<ISensoryEvent>();
                    foreach (JToken lexEvent in paragraph.Value<JArray>("events").Values("event"))
                    {
                        if (lexEvent != null && !string.IsNullOrWhiteSpace(lexEvent.Value<string>("phrase")))
                        {
                            ILexica lex = ParseLexica(globalContext, lexEvent);

                            SensoryEvent mainEvent = new SensoryEvent
                            {
                                SensoryType = MessagingType.Visible,
                                Strength = short.MaxValue,
                                Event = lex
                            };
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
                success = true;
            }

            LexicalOutput returnValue = new LexicalOutput()
            {
                Success = success,
                Description = description.ToString(),
                Errors = errors.ToArray()
            };

            return Json(returnValue);
        }

        private static ILexica ParseLexica(LexicalContext globalContext, JToken lexEvent)
        {
            ILexica lex = new Lexica
            {
                Role = lexEvent.Value<GrammaticalType>("role"),
                Type = lexEvent.Value<LexicalType>("type"),
                Phrase = lexEvent.Value<string>("phrase"),
                Context = globalContext
            };

            if (lexEvent.Value<JArray>("modifiers") != null)
            {
                foreach (JToken modifier in lexEvent.Value<JArray>("modifiers").Values("event"))
                {
                    if (modifier != null && !string.IsNullOrWhiteSpace(modifier.Value<string>("phrase")))
                    {
                        ParseLexica(globalContext, modifier);
                        ILexica newModifier = new Lexica()
                        {
                            Role = modifier.Value<GrammaticalType>("role"),
                            Type = modifier.Value<LexicalType>("type"),
                            Phrase = lexEvent.Value<string>("phrase"),
                            Context = globalContext
                        };

                        lex.Modifiers.Add(newModifier);
                    }
                }
            }

            return lex;
        }
    }
}
