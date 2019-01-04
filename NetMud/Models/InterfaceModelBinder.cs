using NetMud.Data.Architectural.EntityBase;
using NetMud.DataStructure.Architectural.PropertyBinding;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace NetMud.Models
{
    public class InterfaceModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            if (modelType.IsInterface)
            {
                var type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(modelType));

                if (type == null)
                {
                    throw new Exception("Invalid Binding Interface");
                }

                var concreteInstance = Activator.CreateInstance(type);

                bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => concreteInstance, type);

                return concreteInstance;
            }

            return base.CreateModel(controllerContext, bindingContext, modelType);
        }

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            // Check if the property has the PropertyBinderAttribute, meaning it's specifying a different binder to use.
            var propertyBinderAttribute = TryFindPropertyBinderAttribute(propertyDescriptor);
            if (propertyBinderAttribute != null)
            {
                var keyName = string.Format("{0}.{1}", bindingContext.ModelName, propertyDescriptor.Name);

                if (propertyDescriptor.PropertyType.IsArray || (!typeof(string).Equals(propertyDescriptor.PropertyType) && typeof(IEnumerable).IsAssignableFrom(propertyDescriptor.PropertyType)))
                {
                    var formValueProvider = (FormValueProvider)((ValueProviderCollection)bindingContext.ValueProvider).FirstOrDefault(vp => vp.GetType() == typeof(FormValueProvider));

                    var keys = formValueProvider.GetKeysFromPrefix(keyName);

                    var values = keys.Select(kvp => bindingContext.ValueProvider.GetValue(kvp.Value).AttemptedValue);

                    propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(values));
                }
                else 
                {
                    var value = bindingContext.ValueProvider.GetValue(keyName);

                    if (value != null)
                    {
                        propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(value.AttemptedValue));
                    }
                    else if ((propertyDescriptor.PropertyType.IsInterface || propertyDescriptor.PropertyType.IsClass) && !typeof(string).Equals(propertyDescriptor.PropertyType))
                    {
                        var props = new object[propertyDescriptor.GetChildProperties().Count];
                        var i = 0;

                        foreach (PropertyDescriptor prop in propertyDescriptor.GetChildProperties())
                        {
                            var childBinder = TryFindPropertyBinderAttribute(prop);
                            var childKeyName = string.Format("{0}.{1}", keyName, prop.Name);

                            if (prop.PropertyType.IsArray || (!typeof(string).Equals(prop.PropertyType) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)))
                            {
                                var formValueProvider = (FormValueProvider)((ValueProviderCollection)bindingContext.ValueProvider).FirstOrDefault(vp => vp.GetType() == typeof(FormValueProvider));

                                var keys = formValueProvider.GetKeysFromPrefix(childKeyName);
                                if (keys.Count > 0)
                                {
                                    var values = keys.Select(kvp => bindingContext.ValueProvider.GetValue(kvp.Value).AttemptedValue);

                                    if (childBinder != null)
                                    {
                                        props[i] = childBinder.Convert(values);
                                    }
                                    else
                                    {
                                        props[i] = values;
                                    }
                                }
                            }
                            else
                            {
                                var childValue = bindingContext.ValueProvider.GetValue(childKeyName);

                                if (childValue != null)
                                {
                                    if (childBinder != null)
                                    {
                                        props[i] = childBinder.Convert(childValue);
                                    }
                                    else
                                    {
                                        props[i] = childValue;
                                    }
                                }
                            }

                            i++;
                        }

                        if (!props.Any(prop => prop == null))
                        {
                            if (propertyDescriptor.PropertyType.IsInterface)
                            {
                                var type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(propertyDescriptor.PropertyType));

                                if (type == null)
                                {
                                    throw new Exception("Invalid Binding Interface");
                                }

                                var concreteInstance = Activator.CreateInstance(type, props);

                                if (concreteInstance != null)
                                {
                                    propertyDescriptor.SetValue(bindingContext.Model, concreteInstance);
                                }
                            }
                            else
                            {
                                var newItem = Activator.CreateInstance(propertyDescriptor.PropertyType, props);

                                if (newItem != null)
                                {
                                    propertyDescriptor.SetValue(bindingContext.Model, newItem);
                                }
                            }
                        }
                    }
                }

                return;
            }

            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }

        PropertyBinderAttribute TryFindPropertyBinderAttribute(PropertyDescriptor propertyDescriptor)
        {
            return propertyDescriptor.Attributes.OfType<PropertyBinderAttribute>().FirstOrDefault();
        }
    }
}