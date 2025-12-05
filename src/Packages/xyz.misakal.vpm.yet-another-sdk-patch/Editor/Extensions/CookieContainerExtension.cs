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

    public static void Clear(this CookieContainer cookieContainer)
    {
        foreach (var cookie in GetAllCookies(cookieContainer))
        {
            cookie.Expired = true;
        }
    }

    public static void AddRange(this CookieContainer cookieContainer, IEnumerable<Cookie> cookies)
    {
        foreach (var cookie in cookies)
        {
            cookieContainer.Add(cookie);
        }
    }
}