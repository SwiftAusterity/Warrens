using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace NetMud.Models
{
    public static class MvcHtmlHelpers
    {
        public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, self.ViewData);
            var description = GetDescriptionHtml(metadata.Description);

            if (description == null)
                return MvcHtmlString.Empty;

            return MvcHtmlString.Create(description.ToString());
        }

        public static MvcHtmlString DescriptiveLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        {
            return DescriptiveLabelFor(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString DescriptiveLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            var description = GetDescriptionHtml(metadata.Description);

            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();

            if (string.IsNullOrWhiteSpace(labelText))
                return MvcHtmlString.Empty;

            TagBuilder tag = new TagBuilder("label");
            tag.MergeAttributes(htmlAttributes);
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

            if (description == null)
            {
                tag.SetInnerText(labelText);
                return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
            }
            else
            {
                var labelSpan = new TagBuilder("span");
                labelSpan.SetInnerText(labelText);

                var outputHtml = tag.ToString(TagRenderMode.StartTag) 
                                    + labelSpan.ToString(TagRenderMode.Normal) 
                                    + description.ToString(TagRenderMode.Normal) 
                                    + tag.ToString(TagRenderMode.EndTag);

                return MvcHtmlString.Create(outputHtml);
            }
        }

        private static TagBuilder GetDescriptionHtml(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var descTag = new TagBuilder("span");
            descTag.AddCssClass("glyphicon glyphicon-question-sign helpTip");
            descTag.Attributes.Add(new KeyValuePair<string, string>("title", description));

            return descTag;
        }
    }
}