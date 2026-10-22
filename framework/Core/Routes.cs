using System;
using System.Collections.Generic;

namespace XForms
{
    /// <summary>
    /// The app's pages by path, each with a function that makes the page (see
    /// <see cref="Application.OnConfigureRoutes"/>). Nothing is found by
    /// reflection: a page is made by the app's own code, so routes are safe
    /// for trimming and AOT.
    /// </summary>
    /// <remarks>
    /// A route is a path of fixed segments and <c>{name}</c> segments,
    /// optionally followed by query values: <c>/</c>, <c>/about</c>,
    /// <c>/item/{id}</c>, <c>/item/new?category={category}</c>. Fixed segments
    /// match regardless of case, and win over parameters wherever both match.
    /// A path segment is always there; a query value may be left out, and
    /// queries the route doesn't name are ignored.
    /// </remarks>
    public sealed class Routes
    {
        private readonly List<RouteEntry> _entries = new List<RouteEntry>();

        internal Routes()
        {
        }

        /// <summary>
        /// Adds a route: its pattern, and the function that makes its page from
        /// a path that matches. The function may return null when the path's
        /// values name nothing (an item that's gone), as if no route matched.
        /// </summary>
        public void Add(
            string pattern,
            Func<Route, Page> createPage)
        {
            ArgumentNullException.ThrowIfNull(pattern);
            ArgumentNullException.ThrowIfNull(createPage);

            var entry = RouteEntry.Parse(pattern, createPage);
            foreach (var existing in this._entries)
            {
                if (existing.Shape == entry.Shape)
                {
                    throw new ArgumentException($"The route {pattern} matches the same paths as {existing.Pattern}.", nameof(pattern));
                }
            }

            this._entries.Add(entry);
        }

        /// <summary>
        /// The route a path matches, or null.
        /// </summary>
        internal Route Match(
            string path,
            out Func<Route, Page> createPage)
        {
            createPage = null;
            if (!TrySplit(path, out var segments, out var query))
            {
                return null;
            }

            RouteEntry best = null;
            foreach (var entry in this._entries)
            {
                if (entry.Matches(segments) && ((null == best) || entry.IsMoreSpecificThan(best)))
                {
                    best = entry;
                }
            }

            if (null == best)
            {
                return null;
            }

            createPage = best.CreatePage;
            return best.CreateRoute(path, segments, query);
        }

        /// <summary>
        /// Makes the page for a path, or returns null when no route matches (or
        /// its function returns null).
        /// </summary>
        internal Page CreatePage(
            string path)
        {
            var route = this.Match(path, out var createPage);
            if (null == route)
            {
                return null;
            }

            var page = createPage(route);
            if (null != page)
            {
                page.NavigationPath = path;
            }

            return page;
        }

        // A path's decoded segments, and its query's decoded values (the first
        // of each name)
        private static bool TrySplit(
            string path,
            out string[] segments,
            out Dictionary<string, string> query)
        {
            segments = null;
            query = null;
            if (string.IsNullOrEmpty(path) || (path[0] != '/'))
            {
                return false;
            }

            int queryStart = path.IndexOf('?');
            string pathPart = (queryStart < 0) ? path : path.Substring(0, queryStart);

            var parts = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries);
            segments = new string[parts.Length];
            for (int i = 0; i < parts.Length; i += 1)
            {
                segments[i] = Uri.UnescapeDataString(parts[i]);
            }

            query = new Dictionary<string, string>(StringComparer.Ordinal);
            if (queryStart >= 0)
            {
                foreach (var pair in path.Substring(queryStart + 1).Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    int separator = pair.IndexOf('=');
                    string key = DecodeQuery((separator < 0) ? pair : pair.Substring(0, separator));
                    string value = (separator < 0) ? string.Empty : DecodeQuery(pair.Substring(separator + 1));
                    query.TryAdd(key, value);
                }
            }

            return true;
        }

        private static string DecodeQuery(
            string text)
        {
            return Uri.UnescapeDataString(text.Replace('+', ' '));
        }

        private sealed class RouteEntry
        {
            // Each path segment: its fixed text, or null for a parameter
            private string[] _literals;

            // Each path segment's parameter name, or null for fixed text
            private string[] _parameters;

            // Query names in the path, and the parameter each fills
            private List<(string Key, string Name)> _queryParameters;

            private HashSet<string> _names;

            public string Pattern
            {
                get;
                private set;
            }

            // The paths it matches, regardless of names and queries
            public string Shape
            {
                get;
                private set;
            }

            public Func<Route, Page> CreatePage
            {
                get;
                private set;
            }

            public static RouteEntry Parse(
                string pattern,
                Func<Route, Page> createPage)
            {
                if ((0 == pattern.Length) || (pattern[0] != '/'))
                {
                    throw new ArgumentException($"The route '{pattern}' must start with '/'.", nameof(pattern));
                }

                var names = new HashSet<string>(StringComparer.Ordinal);
                void AddName(string name)
                {
                    if (!names.Add(name))
                    {
                        throw new ArgumentException($"The route {pattern} names '{name}' twice.", nameof(pattern));
                    }
                }

                int queryStart = pattern.IndexOf('?');
                string pathPart = (queryStart < 0) ? pattern : pattern.Substring(0, queryStart);
                var parts = pathPart.Split('/', StringSplitOptions.RemoveEmptyEntries);

                var literals = new string[parts.Length];
                var parameters = new string[parts.Length];
                var shape = new List<string>();
                for (int i = 0; i < parts.Length; i += 1)
                {
                    string name = ParseParameter(parts[i], pattern);
                    if (null != name)
                    {
                        AddName(name);
                        parameters[i] = name;
                        shape.Add("{}");
                    }
                    else
                    {
                        literals[i] = parts[i];
                        shape.Add(parts[i].ToUpperInvariant());
                    }
                }

                var queryParameters = new List<(string Key, string Name)>();
                if (queryStart >= 0)
                {
                    foreach (var pair in pattern.Substring(queryStart + 1).Split('&', StringSplitOptions.RemoveEmptyEntries))
                    {
                        int separator = pair.IndexOf('=');
                        string name = (separator > 0) ? ParseParameter(pair.Substring(separator + 1), pattern) : null;
                        if (null == name)
                        {
                            throw new ArgumentException($"The route {pattern} has a query '{pair}' that isn't name={{parameter}}.", nameof(pattern));
                        }

                        AddName(name);
                        queryParameters.Add((pair.Substring(0, separator), name));
                    }
                }

                return new RouteEntry()
                {
                    Pattern = pattern,
                    Shape = "/" + string.Join("/", shape),
                    CreatePage = createPage,
                    _literals = literals,
                    _parameters = parameters,
                    _queryParameters = queryParameters,
                    _names = names,
                };
            }

            // A segment's parameter name, or null for fixed text
            private static string ParseParameter(
                string segment,
                string pattern)
            {
                if ((segment.Length > 2) && (segment[0] == '{') && (segment[segment.Length - 1] == '}'))
                {
                    string name = segment.Substring(1, segment.Length - 2);
                    if ((name.IndexOfAny(new[] { '{', '}' }) < 0) && (name.Trim().Length == name.Length))
                    {
                        return name;
                    }
                }

                if (segment.IndexOfAny(new[] { '{', '}' }) >= 0)
                {
                    throw new ArgumentException($"The route {pattern} has a segment '{segment}' that's neither fixed text nor a whole {{parameter}}.", nameof(pattern));
                }

                return null;
            }

            public bool Matches(
                string[] segments)
            {
                if (segments.Length != this._literals.Length)
                {
                    return false;
                }

                for (int i = 0; i < segments.Length; i += 1)
                {
                    var literal = this._literals[i];
                    if ((null != literal) && !string.Equals(literal, segments[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }

            // At the first segment where one is fixed and the other a
            // parameter, the fixed one wins
            public bool IsMoreSpecificThan(
                RouteEntry other)
            {
                for (int i = 0; i < this._literals.Length; i += 1)
                {
                    bool isFixed = (null != this._literals[i]);
                    bool isOtherFixed = (null != other._literals[i]);
                    if (isFixed != isOtherFixed)
                    {
                        return isFixed;
                    }
                }

                return false;
            }

            public Route CreateRoute(
                string path,
                string[] segments,
                Dictionary<string, string> query)
            {
                var values = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int i = 0; i < segments.Length; i += 1)
                {
                    if (null != this._parameters[i])
                    {
                        values[this._parameters[i]] = segments[i];
                    }
                }

                foreach (var (key, name) in this._queryParameters)
                {
                    if (query.TryGetValue(key, out var value))
                    {
                        values[name] = value;
                    }
                }

                return new Route(path, this.Pattern, values, this._names);
            }
        }
    }
}
