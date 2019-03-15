using NetMud.Communication.Lexical;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Room;
using NetMud.Data.Zone;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Architectural.PropertyBinding;
using System;
using System.Collections;
using System.Collections.Generic;
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
                    Type type = null;

                    if (modelType == typeof(ILocationData))
                    {
                        if (bindingContext.ModelName.Contains("Zone"))
                        {
                            type = typeof(ZoneTemplate);
                        }
                        else if (bindingContext.ModelName.Contains("Room"))
                        {
                            type = typeof(RoomTemplate);
                        }
                    }
                    else
                    {
                        type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(modelType));

                        if (type == null)
                        {
                            type = typeof(SensoryEvent).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(modelType));
                        }
                    }

                    if (type == null)
                    {
                        throw new Exception("Invalid Binding Interface");
                    }

                    object concreteInstance = Activator.CreateInstance(type);

                    bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => concreteInstance, type);

                    return concreteInstance;
                }
                else
                {
                    //Our interface involves generics so go find the concrete class by type name match so we can build it out using the correct type for the generic parameter
                    string genericName = modelType.Name.Substring(1);
                    Type type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.IsGenericType && x.Name.Equals(genericName));

                    if (type == null)
                    {
                        throw new Exception("Invalid Binding Interface");
                    }

                    Type genericType = type.MakeGenericType(modelType.GenericTypeArguments);
                    object concreteInstance = Activator.CreateInstance(genericType);

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
            PropertyBinderAttribute propertyBinderAttribute = TryFindPropertyBinderAttribute(propertyDescriptor);
            if (propertyBinderAttribute != null)
            {
                string keyName = string.Format("{0}{2}{1}", bindingContext.ModelName, propertyDescriptor.Name, string.IsNullOrWhiteSpace(bindingContext.ModelName) ? "" : ".");

                //Is this a collection of other things?
                if (propertyDescriptor.PropertyType.IsArray || (!typeof(string).Equals(propertyDescriptor.PropertyType) && typeof(IEnumerable).IsAssignableFrom(propertyDescriptor.PropertyType)))
                {
                    FormValueProvider formValueProvider = (FormValueProvider)((ValueProviderCollection)bindingContext.ValueProvider).FirstOrDefault(vp => vp.GetType() == typeof(FormValueProvider));

                    //We have to get the keys from the valid provider that match the pattern razor is feeding us "type.type.type[#].type[#] potentially"
                    IList<string> keys = new List<string>();

                    var i = 0;
                    foreach (var key in formValueProvider.GetKeysFromPrefix(keyName))
                    {
                        var oldKey = key.Value;
                        var reKeyedKey = string.Format("{1}[{0}]{2}", i, oldKey.Substring(0, oldKey.IndexOf('[')), oldKey.Substring(oldKey.IndexOf(']') + 1));

                        keys.Add(reKeyedKey);

                        i++;
                    }

                    IEnumerable<string> values = keys.Select(kvp => bindingContext.ValueProvider.GetValue(kvp).AttemptedValue);

                    propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(values));
                }
                else
                {
                    ValueProviderResult value = bindingContext.ValueProvider.GetValue(keyName);

                    //Bound values *on* the view model tend to double their names due to stupidity
                    if (value == null)
                    {
                        value = bindingContext.ValueProvider.GetValue(string.Format("{0}.{0}", propertyDescriptor.Name));
                    }

                    if (value == null)
                    {
                        value = bindingContext.ValueProvider.GetValue(string.Format("{0}.{1}.{1}", bindingContext.ModelName, propertyDescriptor.Name));
                    }

                    if (value != null)
                    {
                        //If we got the value we're good just set it
                        propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(value.AttemptedValue));
                    }
                    else if ((propertyDescriptor.PropertyType.IsInterface || propertyDescriptor.PropertyType.IsClass) && !typeof(string).Equals(propertyDescriptor.PropertyType))
                    {
                        object[] props = new object[propertyDescriptor.GetChildProperties().Count];
                        int i = 0;

                        //Do we have a class or interface? We want top parse ALL the submitted values in the post and try to fill that one class object up with its props
                        foreach (PropertyDescriptor prop in propertyDescriptor.GetChildProperties())
                        {
                            PropertyBinderAttribute childBinder = TryFindPropertyBinderAttribute(prop);
                            string childKeyName = string.Format("{0}.{1}", keyName, prop.Name);

                            //Collection shenanigans, we need to create the right collection and put it back on the post value collection so a later call to this can fill it correctly
                            if (prop.PropertyType.IsArray || (!typeof(string).Equals(prop.PropertyType) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)))
                            {
                                FormValueProvider formValueProvider = (FormValueProvider)((ValueProviderCollection)bindingContext.ValueProvider).FirstOrDefault(vp => vp.GetType() == typeof(FormValueProvider));

                                //We have to get the keys from the valid provider that match the pattern razor is feeding us "type.type.type[#].type[#] potentially"
                                IList<string> keys = new List<string>();

                                var index = 0;
                                foreach (var key in formValueProvider.GetKeysFromPrefix(childKeyName))
                                {
                                    var oldKey = key.Value;
                                    var reKeyedKey = string.Format("{1}[{0}]{2}", index, oldKey.Substring(0, oldKey.IndexOf('[')), oldKey.Substring(oldKey.IndexOf(']') + 1));

                                    keys.Add(reKeyedKey);

                                    index++;
                                }

                                if (keys.Count > 0)
                                {
                                    IEnumerable<string> values = keys.Select(kvp => bindingContext.ValueProvider.GetValue(kvp).AttemptedValue);

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
                                ValueProviderResult childValue = bindingContext.ValueProvider.GetValue(childKeyName);

                                if (childValue != null)
                                {
                                    if (childBinder != null)
                                    {
                                        props[i] = childBinder.Convert(childValue);
                                    }
                                    else
                                    {
                                        props[i] = childValue.AttemptedValue;
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
                                Type type = null;

                                if (propertyDescriptor.PropertyType == typeof(ILocationData))
                                {
                                    type = typeof(RoomTemplate);
                                }
                                else
                                {
                                    type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(propertyDescriptor.PropertyType));

                                    if (type == null)
                                    {
                                        type = typeof(SensoryEvent).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(propertyDescriptor.PropertyType));
                                    }
                                }

                                if (type == null)
                                {
                                    throw new Exception("Invalid Binding Interface");
                                }

                                object concreteInstance = Activator.CreateInstance(type, props);

                                if (concreteInstance != null)
                                {
                                    propertyDescriptor.SetValue(bindingContext.Model, concreteInstance);
                                }
                            }
                            else
                            {
                                object newItem = Activator.CreateInstance(propertyDescriptor.PropertyType, props);

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
            else if (propertyDescriptor.PropertyType.IsArray ||
                        (!typeof(string).Equals(propertyDescriptor.PropertyType) && typeof(IEnumerable).IsAssignableFrom(propertyDescriptor.PropertyType)))
            {
                string keyName = string.Format("{0}{2}{1}", bindingContext.ModelName, propertyDescriptor.Name, string.IsNullOrWhiteSpace(bindingContext.ModelName) ? "" : ".");

                ValueProviderResult value = bindingContext.ValueProvider.GetValue(keyName);

                //Bound values *on* the view model tend to double their names due to stupidity
                if (value == null)
                {
                    value = bindingContext.ValueProvider.GetValue(string.Format("{0}.{0}", propertyDescriptor.Name));
                }

                if (value == null)
                {
                    value = bindingContext.ValueProvider.GetValue(string.Format("{0}.{1}.{1}", bindingContext.ModelName, propertyDescriptor.Name));
                }

                if (value != null)
                {
                    //If we got the value we're good just set it
                    if (propertyBinderAttribute == null)
                    {
                        base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    }
                    else
                    {
                        propertyDescriptor.SetValue(bindingContext.Model, propertyBinderAttribute.Convert(value.AttemptedValue));
                    }

                    return;
                }
                else if (!string.IsNullOrWhiteSpace(bindingContext.ModelName))
                {
                    Type containedType = null;
                    Type itemType = null;

                    if (propertyDescriptor.PropertyType.IsArray)
                    {
                        itemType = propertyDescriptor.PropertyType.GetElementType();
                        containedType = itemType;
                    }
                    else
                    {
                        itemType = propertyDescriptor.PropertyType.GetGenericArguments().First();
                        containedType = itemType;
                    }

                    if (containedType.IsInterface)
                    {
                        Type type = null;

                        if (containedType == typeof(ILocationData))
                        {
                            type = typeof(RoomTemplate);
                        }
                        else
                        {
                            type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(containedType));

                            if (type == null)
                            {
                                type = typeof(SensoryEvent).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(containedType));
                            }
                        }

                        containedType = type ?? throw new Exception("Invalid Binding Interface");
                    }

                    if (containedType.IsClass && !typeof(string).Equals(containedType))
                    {
                        var properties = containedType.GetProperties();
                        FormValueProvider formValueProvider = (FormValueProvider)((ValueProviderCollection)bindingContext.ValueProvider).FirstOrDefault(vp => vp.GetType() == typeof(FormValueProvider));

                        //HashSet<object> valueArray = new HashSet<object>();
                        dynamic valueArray = Activator.CreateInstance(propertyDescriptor.PropertyType, false);
                        foreach (var baseKey in formValueProvider.GetKeysFromPrefix(keyName))
                        {
                            int propIterator = 0;
                            dynamic newItem = Activator.CreateInstance(containedType, false);

                            //Do we have a class or interface? We want top parse ALL the submitted values in the post and try to fill that one class object up with its props
                            foreach (var prop in properties)
                            {
                                var propertyBinder = (PropertyBinderAttribute)prop.GetCustomAttributes(typeof(PropertyBinderAttribute), true).FirstOrDefault();

                                string childKeyName = string.Format("{0}.{1}", baseKey.Value, prop.Name);

                                //Collection shenanigans, we need to create the right collection and put it back on the post value collection so a later call to this can fill it correctly
                                if (prop.PropertyType.IsArray || (!typeof(string).Equals(prop.PropertyType) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)))
                                {
                                    //We have to get the keys from the valid provider that match the pattern razor is feeding us "type.type.type[#].type[#] potentially"
                                    IList<string> keys = new List<string>();

                                    var index = 0;
                                    foreach (var key in formValueProvider.GetKeysFromPrefix(childKeyName))
                                    {
                                        var oldKey = key.Value;
                                        var reKeyedKey = string.Format("{1}[{0}]{2}", index, oldKey.Substring(0, oldKey.IndexOf('[')), oldKey.Substring(oldKey.IndexOf(']') + 1));

                                        keys.Add(reKeyedKey);

                                        index++;
                                    }

                                    if (keys.Count > 0)
                                    {
                                        IEnumerable<string> values = keys.Select(kvp => bindingContext.ValueProvider.GetValue(kvp).AttemptedValue);
                                        prop.SetValue(newItem, values);
                                    }
                                }
                                else
                                {
                                    //I guess we didnt have a class so just try and use the modelbinder to convert the value correctly
                                    ValueProviderResult childValue = bindingContext.ValueProvider.GetValue(childKeyName);

                                    if (childValue == null)
                                    {
                                        childValue = bindingContext.ValueProvider.GetValue(string.Format("{0}.{0}", prop.Name));
                                    }

                                    if (childValue == null)
                                    {
                                        childValue = bindingContext.ValueProvider.GetValue(string.Format("{0}.{1}.{1}", baseKey.Value, prop.Name));
                                    }

                                    if (childValue != null)
                                    {
                                        if (propertyBinder != null)
                                        {
                                            prop.SetValue(newItem, propertyBinder.Convert(childValue.AttemptedValue));
                                        }
                                        else
                                        {
                                            prop.SetValue(newItem, childValue.ConvertTo(prop.PropertyType));
                                        }
                                    }
                                }

                                propIterator++;
                            }

                            if (newItem != null)
                            {
                                if (itemType != containedType)
                                {
                                    valueArray.Add(newItem);
                                }
                                else
                                {
                                    valueArray.Add(newItem);
                                }
                            }
                        }

                        propertyDescriptor.SetValue(bindingContext.Model, valueArray);
                        return;
                    }
                }
            }

            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }

        PropertyBinderAttribute TryFindPropertyBinderAttribute(PropertyDescriptor propertyDescriptor)
        {
            var binder = propertyDescriptor.Attributes.OfType<PropertyBinderAttribute>().FirstOrDefault();

            if (binder == null && propertyDescriptor.ComponentType.IsInterface && !propertyDescriptor.PropertyType.IsValueType)
            {
                var componentType = propertyDescriptor.ComponentType;

                //Convert the interface to the concrete class by finding a concrete class that impls this interface
                if (!componentType.IsGenericType)
                {
                    Type type = null;

                    if (componentType == typeof(ILocationData))
                    {
                        if (componentType.Name.Contains("Zone"))
                        {
                            type = typeof(ZoneTemplate);
                        }
                        else if (componentType.Name.Contains("Room"))
                        {
                            type = typeof(RoomTemplate);
                        }
                    }
                    else
                    {
                        type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(componentType));

                        if (type == null)
                        {
                            type = typeof(SensoryEvent).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.GetInterfaces().Contains(componentType));
                        }
                    }

                    if (type == null)
                    {
                        return null;
                    }

                    var typeProps = type.GetProperties();
                    var myProp = typeProps.FirstOrDefault(prop => prop.Name == propertyDescriptor.Name && prop.PropertyType == propertyDescriptor.PropertyType);

                    return (PropertyBinderAttribute)myProp.GetCustomAttributes(typeof(PropertyBinderAttribute), true).FirstOrDefault();
                }
                else
                {
                    //Our interface involves generics so go find the concrete class by type name match so we can build it out using the correct type for the generic parameter
                    string genericName = componentType.Name.Substring(1);
                    Type type = typeof(EntityPartial).Assembly.GetTypes().SingleOrDefault(x => !x.IsAbstract && x.IsGenericType && x.Name.Equals(genericName));

                    if (type == null)
                    {
                        return null;
                    }

                    Type genericType = type.MakeGenericType(componentType.GenericTypeArguments);

                    var typeProps = genericType.GetProperties();
                    var myProp = typeProps.FirstOrDefault(prop => prop.Name == propertyDescriptor.Name && prop.PropertyType == propertyDescriptor.PropertyType);

                    return myProp.CustomAttributes.OfType<PropertyBinderAttribute>().FirstOrDefault();
                }
            }

            return binder;
        }
    }
}