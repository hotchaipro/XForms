using System;
using System.Collections.Generic;
using Xunit;

namespace XForms.Tests
{
    public class RoutesTests
    {
        private static Routes Create(
            params string[] patterns)
        {
            var routes = new Routes();
            foreach (var pattern in patterns)
            {
                routes.Add(pattern, _ => new Page());
            }

            return routes;
        }

        private static Route Match(
            Routes routes,
            string path)
        {
            return routes.Match(path, out _);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/about")]
        [InlineData("/ABOUT")]
        [InlineData("/about/")]
        [InlineData("/about?unused=1")]
        public void FixedSegmentsMatchRegardlessOfCaseAndTrailingSlash(
            string path)
        {
            var routes = Create("/", "/about");
            string expected = (path == "/") ? "/" : "/about";

            Assert.Equal(expected, Match(routes, path)?.Pattern);
        }

        [Theory]
        [InlineData("")]
        [InlineData("about")]
        [InlineData("/about/more")]
        [InlineData("/contact")]
        public void PathsThatMatchNothing(
            string path)
        {
            var routes = Create("/", "/about", "/item/{id}");

            Assert.Null(Match(routes, path));
        }

        [Fact]
        public void ParametersAreDecodedSegments()
        {
            var route = Match(Create("/item/{id}/part/{part}"), "/item/a%20b%2Fc/part/7");

            Assert.Equal("a b/c", route["id"]);
            Assert.Equal("7", route["part"]);
            Assert.Equal(7, route.GetInt32("part"));
            Assert.Equal("/item/a%20b%2Fc/part/7", route.Path);
        }

        [Theory]
        [InlineData("/item/{id}", "/item/new")]
        [InlineData("/item/new", "/item/{id}")]
        public void FixedTextWinsOverAParameterInAnyOrder(
            string first,
            string second)
        {
            var routes = Create(first, second);

            Assert.Equal("/item/new", Match(routes, "/item/new").Pattern);
            Assert.Equal("/item/{id}", Match(routes, "/item/42").Pattern);
        }

        [Fact]
        public void TheFirstDifferenceDecidesWhichIsMoreSpecific()
        {
            var routes = Create("/{a}/b", "/a/{b}");

            Assert.Equal("/a/{b}", Match(routes, "/a/b").Pattern);
        }

        [Fact]
        public void QueryValuesAreOptionalAndDecoded()
        {
            var routes = Create("/item/new?category={category}&title={title}");

            var route = Match(routes, "/item/new?title=Buy+milk%21&category=Soon&other=1");
            Assert.Equal("Soon", route["category"]);
            Assert.Equal("Buy milk!", route["title"]);

            var bare = Match(routes, "/item/new");
            Assert.Null(bare["category"]);
            Assert.Null(bare["title"]);
        }

        [Fact]
        public void TheFirstOfARepeatedQueryValueCounts()
        {
            var route = Match(Create("/search?q={q}"), "/search?q=one&q=two");

            Assert.Equal("one", route["q"]);
        }

        [Fact]
        public void ANameTheRouteDoesntHaveThrows()
        {
            var route = Match(Create("/item/{id}"), "/item/42");

            Assert.Throws<KeyNotFoundException>(() => route["ID"]);
        }

        [Fact]
        public void TypedValues()
        {
            var id = Guid.NewGuid();
            var route = Match(Create("/{guid}/{number}/{text}?missing={missing}"), $"/{id}/9000000000/abc");

            Assert.Equal(id, route.GetGuid("guid"));
            Assert.Equal(9000000000L, route.GetInt64("number"));
            Assert.Throws<FormatException>(() => route.GetInt32("text"));
            Assert.Throws<FormatException>(() => route.GetInt32("missing"));
        }

        [Theory]
        [InlineData("/item/{id}", "/ITEM/{key}")]
        [InlineData("/", "/")]
        [InlineData("/item/{id}", "/item/{id}?x={x}")]
        public void RoutesThatMatchTheSamePathsThrow(
            string first,
            string second)
        {
            var routes = Create(first);

            Assert.Throws<ArgumentException>(() => routes.Add(second, _ => new Page()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("about")]
        [InlineData("/item/{id}/{id}")]
        [InlineData("/item/x{id}")]
        [InlineData("/item/{}")]
        [InlineData("/search?q")]
        [InlineData("/search?q=text")]
        public void BadPatternsThrow(
            string pattern)
        {
            Assert.Throws<ArgumentException>(() => Create(pattern));
        }

        [Fact]
        public void CreatePageMakesThePageAndGivesItItsPath()
        {
            var routes = new Routes();
            Route given = null;
            routes.Add("/item/{id}", route =>
            {
                given = route;
                return new Page();
            });

            var page = routes.CreatePage("/item/42");

            Assert.NotNull(page);
            Assert.Equal("/item/42", page.NavigationPath);
            Assert.Equal("42", given["id"]);
        }

        [Fact]
        public void APageFunctionReturningNullIsNoMatch()
        {
            var routes = new Routes();
            routes.Add("/item/{id}", route => (route["id"] == "42") ? new Page() : null);

            Assert.NotNull(routes.CreatePage("/item/42"));
            Assert.Null(routes.CreatePage("/item/43"));
            Assert.Null(routes.CreatePage("/nothing"));
        }
    }
}
