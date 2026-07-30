using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using HarmonyLib;

namespace YetAnotherPatchForVRChatSdk.Extensions;

internal static class CookieContainerExtension
{
    public static IEnumerable<Cookie> GetAllCookies(this CookieContainer cookieContainer)
    {
        var cookieContainerType = cookieContainer.GetType();
        var domainTableField = AccessTools.Field(cookieContainerType, "m_domainTable");

        if (domainTableField is null || domainTableField.FieldType != typeof(Hashtable))
            throw new MissingFieldException(cookieContainerType.Name, "m_domainTable");

        if (domainTableField.GetValue(cookieContainer) is not Hashtable domainKeys)
            throw new Exception("Failed to get domain table from CookieContainer, m_domainTable is null.");

        foreach (DictionaryEntry element in domainKeys)
        {
            if (element.Value is null)
                throw new Exception("Failed to get path list from domain table, value is null.");
            var valueType = element.Value.GetType();

            var internalListField = AccessTools.Field(valueType, "m_list");
            if (internalListField is null || internalListField.FieldType != typeof(SortedList))
                throw new MissingFieldException(valueType.Name, "m_list");

            var list = (SortedList)internalListField.GetValue(element.Value);
            foreach (var listEntity in list)
            {
                var cookieCollection = (CookieCollection)((DictionaryEntry)listEntity).Value;
                foreach (Cookie cookie in cookieCollection)
                {
                    yield return cookie;
                }
            }
        }
    }

    public static CookieContainer Clone(this CookieContainer cookieContainer, Func<Cookie, bool>? predicate = null)
    {
        var clone = new CookieContainer();
        foreach (var cookie in cookieContainer.GetAllCookies())
        {
            if (predicate is null || predicate(cookie))
                clone.AddRange(new[] { cookie });
        }

        return clone;
    }

    public static void AddRange(this CookieContainer cookieContainer, IEnumerable<Cookie> cookies)
    {
        foreach (var cookie in cookies)
        {
            cookieContainer.Add(CloneCookie(cookie));
        }
    }

    private static Cookie CloneCookie(Cookie cookie)
    {
        var clone = new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain)
        {
            Discard = cookie.Discard,
            Expired = cookie.Expired,
            HttpOnly = cookie.HttpOnly,
            Secure = cookie.Secure,
            Version = cookie.Version
        };

        if (!string.IsNullOrEmpty(cookie.Comment))
            clone.Comment = cookie.Comment;
        if (cookie.CommentUri is not null)
            clone.CommentUri = cookie.CommentUri;
        if (cookie.Expires != DateTime.MinValue)
            clone.Expires = cookie.Expires;
        if (!string.IsNullOrEmpty(cookie.Port))
            clone.Port = cookie.Port;

        return clone;
    }

}