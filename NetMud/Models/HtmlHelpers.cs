using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace NetMud.Models
{
    public static class MvcHtmlHelpers
    {
        public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> self, Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, self.ViewData);
            var description = metadata.Description;

            if (string.IsNullOrWhiteSpace(description))
                return MvcHtmlString.Create(string.Empty);
            return MvcHtmlString.Create(string.Format(@"<span class='glyphicon glyphicon-question-sign helpTip' title='{0}'></span>", description));
        }
    }
}