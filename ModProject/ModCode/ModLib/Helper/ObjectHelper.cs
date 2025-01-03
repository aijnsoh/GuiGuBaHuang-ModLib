﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ObjectHelper
{
    public static int goIndex = 0;

    public static readonly Newtonsoft.Json.JsonSerializerSettings CLONE_JSON_SETTINGS = new Newtonsoft.Json.JsonSerializerSettings
    {
        Formatting = Newtonsoft.Json.Formatting.Indented,
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
        PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
    };

    public static T Clone<T>(this T obj)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(obj, CLONE_JSON_SETTINGS), CLONE_JSON_SETTINGS);
    }

    public static void Map<T>(T src, T dest)
    {
        Map(src, dest, typeof(T));
    }

    public static void Map(object src, object dest, System.Type objType)
    {
        foreach (var p in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanWrite || !p.CanRead)
                continue;
            var srcValue = p.GetValue(src);
            if (srcValue != null)
                p.SetValue(dest, srcValue);
        }
    }

    public static void MapBySourceProp(object src, object dest)
    {
        foreach (var p1 in src.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var p2 = dest.GetType().GetProperty(p1.Name, BindingFlags.Public | BindingFlags.Instance);
            if (p2 == null || !p2.CanWrite || !p1.CanRead)
                continue;
            var srcValue = p1.GetValue(src);
            if (srcValue != null)
                p2.SetValue(dest, srcValue);
        }
    }

    public static void MapByDestProp(object src, object dest)
    {
        foreach (var p1 in dest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var p2 = src.GetType().GetProperty(p1.Name, BindingFlags.Public | BindingFlags.Instance);
            if (p2 == null || !p1.CanWrite || !p2.CanRead)
                continue;
            var srcValue = p2.GetValue(src);
            if (srcValue != null)
                p1.SetValue(dest, srcValue);
        }
    }

    public static object GetValue(this object obj, string fieldNm, bool ignoreError = false)
    {
        var prop = obj.GetType().GetProperty(fieldNm);
        if (prop == null)
        {
            if (ignoreError)
                return null;
            throw new NullReferenceException();
        }
        return prop.GetValue(obj);
    }

    public static void SetValue(this object obj, string fieldNm, object newValue, bool ignoreError = false)
    {
        var prop = obj.GetType().GetProperty(fieldNm);
        if (prop == null)
        {
            if (ignoreError)
                return;
            throw new NullReferenceException();
        }
        var type = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
        prop.SetValue(obj, ParseHelper.ParseUnknown(newValue, type));
    }

    public static bool IsDeclaredMethod(this object obj, string medName)
    {
        return obj?.GetType()?.GetMethod(medName)?.DeclaringType == obj.GetType();
    }

    public static string GetBackingFieldName(string propertyName)
    {
        return string.Format("<{0}>k__BackingField", propertyName);
    }

    public static FieldInfo GetBackingField(object obj, string propertyName)
    {
        return obj.GetType().GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
    }

    public static void SetBackingField(object obj, string propertyName, object value)
    {
        GetBackingField(obj, propertyName).SetValue(obj, value);
    }

    public static T Replace<T>(this T obj, Transform transform = null) where T : MonoBehaviour
    {
        var newObj = MonoBehaviour.Instantiate(obj, transform ?? obj.transform.parent, false);
        newObj.gameObject.name = $"ObjectHelper:{goIndex++}";
        newObj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
        newObj.gameObject.SetActive(true);
        obj.gameObject.SetActive(false);
        if (newObj is Button)
        {
            var o = newObj as Button;
            o.onClick.RemoveAllListeners();
        }
        return newObj;
    }

    public static T Create<T>(this T obj, Transform transform = null) where T : MonoBehaviour
    {
        var newObj = MonoBehaviour.Instantiate(obj, transform ?? obj.transform.parent, false);
        newObj.gameObject.name = $"ObjectHelper:{goIndex++}";
        newObj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
        newObj.gameObject.SetActive(true);
        if (newObj is Text)
        {
            var o = newObj as Text;
            o.text = string.Empty;
        }
        if (newObj is Button)
        {
            var o = newObj as Button;
            o.onClick.RemoveAllListeners();
        }
        return newObj;
    }

    public static T Align<T>(this T obj, TextAnchor tanchor = TextAnchor.MiddleLeft, VerticalWrapMode vMode = VerticalWrapMode.Overflow, HorizontalWrapMode hMode = HorizontalWrapMode.Overflow) where T : Text
    {
        obj.alignment = tanchor;
        obj.verticalOverflow = vMode;
        obj.horizontalOverflow = hMode;
        return obj;
    }

    public static T Format<T>(this T obj, Color? color = null, int fsize = 15, FontStyle fstype = FontStyle.Normal) where T : Text
    {
        obj.fontSize = fsize;
        obj.fontStyle = fstype;
        obj.color = color ?? Color.black;
        return obj;
    }

    public static T Pos<T>(this T obj, GameObject origin, float deltaX = 0f, float deltaY = 0f, float deltaZ = 0f) where T : UIBehaviour
    {
        obj.transform.position = new Vector3((origin?.transform?.position.x ?? 0) + deltaX, (origin?.transform?.position.y ?? 0) + deltaY, (origin?.transform?.position.z ?? 0) + deltaZ);
        return obj;
    }

    public static T Pos<T>(this T obj, float deltaX, float deltaY, float deltaZ) where T : UIBehaviour
    {
        obj.transform.position = new Vector3(deltaX, deltaY, deltaZ);
        return obj;
    }

    public static T Pos<T>(this T obj, float deltaX, float deltaY) where T : UIBehaviour
    {
        obj.transform.position = new Vector3(deltaX, deltaY, obj.transform.position.z);
        return obj;
    }

    public static T Size<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : UIBehaviour
    {
        Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(scaleX, scaleY));
        return obj;
    }

    public static T AddSize<T>(this T obj, float scaleX = 0f, float scaleY = 0f) where T : UIBehaviour
    {
        Parallel.ForEach(obj.GetComponentsInChildren<RectTransform>(), s => s.sizeDelta = new Vector2(s.sizeDelta.x + scaleX, s.sizeDelta.y + scaleY));
        return obj;
    }

    public static Text Setup(this Text obj, string def)
    {
        obj.text = def;
        return obj;
    }

    public static Slider Setup(this Slider obj, float min, float max, float def)
    {
        obj.minValue = min;
        obj.maxValue = max;
        obj.value = def.FixValue(min, max);
        return obj;
    }

    public static Toggle Setup(this Toggle obj, bool def)
    {
        obj.isOn = def;
        return obj;
    }

    public static Button Setup(this Button obj, string def)
    {
        var txt = obj.GetComponentInChildren<Text>();
        if (txt != null)
            txt.text = def;
        return obj;
    }
}