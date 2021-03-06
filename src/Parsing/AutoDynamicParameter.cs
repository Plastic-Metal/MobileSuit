﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PlasticMetal.MobileSuit.Core;
using PlasticMetal.MobileSuit.ObjectModel;

namespace PlasticMetal.MobileSuit.Parsing
{
    /// <summary>
    ///     A DynamicParameter which can parse itself automatically
    /// </summary>
    public abstract class AutoDynamicParameter : IDynamicParameter
    {
        /// <summary>
        ///     Initialize a AutoDynamicParameter
        /// </summary>
        protected AutoDynamicParameter()
        {
            var myType = GetType();
            foreach (var property in myType.GetProperties(SuitObject.Flags))
            {
                var memberAttr = property.GetCustomAttribute<ParsingMemberAttribute>(true);
                if (memberAttr is null) continue;
                var parseAttr = property.GetCustomAttribute<SuitParserAttribute>(true);
                Members.Add(memberAttr.Name, new ParsingMember(
                    property.GetCustomAttribute<AsCollectionAttribute>(true) != null
                        ? new Action<object?, object?>((obj, value) =>
                            {
                                property.PropertyType.GetMethod("Add", new[]
                                {
                                    property.GetType().GetElementType() ?? typeof(string)
                                })?.Invoke(property.GetValue(obj), new[] {value});
                            }
                        )
                        : property.SetValue,
                    parseAttr?.Converter ??
                    (a => a),
                    memberAttr.Length,
                    property.GetCustomAttribute<WithDefaultAttribute>(true) != null));
            }
        }

        /// <summary>
        ///     Members of this AutoDynamicParameter
        /// </summary>
        private Dictionary<string, ParsingMember> Members { get; } = new Dictionary<string, ParsingMember>();

        private static Regex ParseMemberRegex { get; } = new Regex(@"^-");

        /// <inheritdoc />
        public bool Parse(string[]? options = null)
        {
            if (options == null || options.Length <= 0)
                return Members.Values.All(member => member.Assigned);
            for (var i = 0; i < options.Length;)
            {
                if (!ParseMemberRegex.IsMatch(options[i]))
                    throw new ArgumentException(
                        $@"{options[i]}{Lang.AutoDynamicParameter_Parse__0__not_match_format______}: '^-'",
                        nameof(options));

                var name = options[i][1..];
                if (!Members.ContainsKey(name))
                    throw new ArgumentException(
                        $@"{options[i]}{Lang.AutoDynamicParameter_Parse__0__not_in_dictionary___1__}:{{{string.Join(',', Members.Keys)}}}",
                        nameof(options));

                var parseMember = Members[name];
                i++;
                var j = i + parseMember.ParseLength;
                if (j > options.Length)
                    throw new ArgumentException($@"{options[i]}{Lang.AutoDynamicParameter_Parse__0__length_not_match}",
                        nameof(options));

                parseMember.Set(this,
                    ConnectStringArray(options[i..j] ?? Array.Empty<string>()));
                i = j;
            }

            return Members.Values.All(member => member.Assigned);
        }

        private static string ConnectStringArray(string[] array)
        {
            if (array.Length == 0) return "";
            var r = array[0];
            if (array.Length <= 1) return r;
            for (var i = 1; i < array.Length; i++)
                r += ' ' + array[i];
            return r;
        }


        private class ParsingMember
        {
            public ParsingMember(Action<object?, object?> setter,
                Converter<string, object> converter, int parseLength, bool withDefault
            )
            {
                Setter = setter;
                Converter = converter;
                ParseLength = parseLength;
                Assigned = withDefault || parseLength == 0;
            }

            private Action<object?, object?> Setter { get; }
            private Converter<string, object> Converter { get; }

            public bool Assigned { get; private set; }
            public int ParseLength { get; }

            public void Set(AutoDynamicParameter instance, string value)
            {
                Setter(instance, ParseLength == 0 ? true : Converter(value));
                Assigned = true;
            }
        }
    }
}