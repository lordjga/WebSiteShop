using Microsoft.AspNetCore.Http;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSiteShop_Utility
{
    public static class SessionExtensions //по умолчанию код для обработки сессий
                                          //может хранить только целые числа или строки.
                                          //и если нужно сохранить списки целых чисел и объектов
                                          //можно добавить методы реширений для сессий
    {
        public static void Set<T>(this ISession session, string key, T value)//сохраняет данные в сессию
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
        public static T Get<T>(this ISession session, string key)//извлекает
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
