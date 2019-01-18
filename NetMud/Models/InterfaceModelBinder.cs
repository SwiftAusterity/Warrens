using NetMud.Communication.Lexical;
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
                //Convert the interface to the concrete class by finding a concrete class that impls this interface
                if (!modelType.IsGenericType)
                {
                    var type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(modelType));

                    if(type == null)
                    {
                        type = typeof(SensoryEvent).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(modelType));
                    }

                    if (type == null)
                    {
                        throw new Exception("Invalid Binding Interface");
                    }

                    var concreteInstance = Activator.CreateInstance(type);

                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => concreteInstance, type);

                    return concreteInstance;
                }
                else
                {
                    //Our interface involves generics so go find the concrete class by type name match so we can build it out using the correct type for the generic parameter
                    var genericName = modelType.Name.Substring(1);
                    var type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.IsGenericType && x.Name.Equals(genericName));

                    if (type == null)
                    {
                        throw new Exception("Invalid Binding Interface");
                    }

                    var genericType = type.MakeGenericType(modelType.GenericTypeArguments);
                    var concreteInstance = Activator.CreateInstance(genericType);

                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => concreteInstance, genericType);

                    return concreteInstance;
                }
            }

            //Nothing was weird so we can just do it
            return base.CreateModel(controllerContext, bindingContext, modelType);
        }

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            // Check if the property has the PropertyBinderAttribute, meaning it's specifying a different binder to use.
            var propertyBinderAttribute = TryFindPropertyBinderAttribute(propertyDescriptor);
            if (propertyBinderAttribute != null)
            {
                var keyName = string.Format("{0}.{1}", bindingContext.ModelName, propertyDescriptor.Name);

                //Is this a collection of other things?
                if (propertyDescriptor.PropertyType.IsArray || (!typeof(string).Equals(propertyDescriptor.PropertyType) && typeof(IEnumerable).IsAssignableFrom(propertyDescriptor.PropertyType)))
                {
                    var formValueProvider = (FormValueProvider)((ValueProviderCollection)bindingContext.ValueProvider).FirstOrDefault(vp => vp.GetType() == typeof(FormValueProvider));

                    //We have to get the keys from the valid provider that match the pattern razor is feeding us "type.type.type[#].type[#] potentially"
                    var keys = formValueProvider.GetKeysFromPrefix(keyName);

                    var values = keys.Select(kvp => bindingContext.ValueProvider.GetValue(kvp.Value).AttemptedValue);

                    propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(values));
                }
                else 
                {
                    var value = bindingContext.ValueProvider.GetValue(keyName);

                    if (value != null)
                    {
                        //If we got the value we're good just set it
                        propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(value.AttemptedValue));
                    }
                    else if ((propertyDescriptor.PropertyType.IsInterface || propertyDescriptor.PropertyType.IsClass) && !typeof(string).Equals(propertyDescriptor.PropertyType))
                    {
                        var props = new object[propertyDescriptor.GetChildProperties().Count];
                        var i = 0;

                        //Do we have a class or interface? We want top parse ALL the submitted values in the post and try to fill that one class object up with its props
                        foreach (PropertyDescriptor prop in propertyDescriptor.GetChildProperties())
                        {
                            var childBinder = TryFindPropertyBinderAttribute(prop);
                            var childKeyName = string.Format("{0}.{1}", keyName, prop.Name);

                            //Collection shenanigans, we need to create the right collection and put it back on the post value collection so a later call to this can fill it correctly
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
                                //I guess we didnt have a class so just try and use the modelbinder to convert the value correctly
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

                        //Did we actually find the properties for the class?
                        if (!props.Any(prop => prop == null))
                        {
                            //Interface shenanigans again
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
                        else
                        {
                            //I guess we didnt actually want the entire class, just the ID so we can find it in the cache (eg. we had a dropdown of "select a class" as a prop)
                            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
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