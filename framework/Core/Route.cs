using System;
using System.Collections.Generic;
using System.Globalization;

namespace XForms
{
    /// <summary>
    /// A path matched to one of the app's routes (see
    /// <see cref="Application.OnConfigureRoutes"/>): the values of its
    /// parameters, as text, for the route's function to make its page from.
    /// </summary>
    public sealed class Route
    {
        private readonly Dictionary<string, string> _values;
        private readonly HashSet<string> _names;

        internal Route(
            string path,
            string pattern,
            Dictionary<string, string> values,
            HashSet<string> names)
        {
            this.Path = path;
            this.Pattern = pattern;
            this._values = values;
            this._names = names;
        }

        /// <summary>
        /// The path navigated to, such as <c>/item/42</c>.
        /// </summary>
        public string Path
        {
            get;
        }

        /// <summary>
        /// The route it matched, such as <c>/item/{id}</c>.
        /// </summary>
        public string Pattern
        {
            get;
        }

        /// <summary>
        /// A parameter's value, decoded: a path segment's (always there), or a
        /// query value's (null when the path doesn't give it). Throws for a
        /// name the route doesn't have.
        /// </summary>
        public string this[string name]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(name);
                if (!this._names.Contains(name))
                {
                    throw new KeyNotFoundException($"The route {this.Pattern} has no parameter named '{name}'.");
                }

                return this._values.TryGetValue(name, out var value) ? value : null;
            }
        }

        /// <summary>
        /// A parameter's value as an <see cref="int"/>. Throws when it's missing
        /// or isn't a number.
        /// </summary>
        public int GetInt32(
            string name)
        {
            return int.Parse(this.GetRequired(name), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// A parameter's value as a <see cref="long"/>. Throws when it's missing
        /// or isn't a number.
        /// </summary>
        public long GetInt64(
            string name)
        {
            return long.Parse(this.GetRequired(name), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// A parameter's value as a <see cref="Guid"/>. Throws when it's missing
        /// or isn't one.
        /// </summary>
        public Guid GetGuid(
            string name)
        {
            return Guid.Parse(this.GetRequired(name));
        }

        private string GetRequired(
            string name)
        {
            return this[name] ?? throw new FormatException($"The path {this.Path} has no value for '{name}'.");
        }
    }
}
