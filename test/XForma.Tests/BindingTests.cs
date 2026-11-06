using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XForma.Primitives;
using Xunit;

namespace XForma.Tests
{
    // A view's property following a source's value, and back
    public class BindingTests
    {
        private sealed class Person : INotifyPropertyChanged
        {
            private string _name = "Ada";

            public event PropertyChangedEventHandler PropertyChanged;

            public int Reads
            {
                get;
                set;
            }

            public string Name
            {
                get
                {
                    this.Reads += 1;
                    return this._name;
                }

                set
                {
                    this._name = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Name)));
                }
            }
        }

        private sealed class Plain
        {
            public string Name { get; set; } = "Ada";
        }

        private static readonly ViewProperty<Marker, string> MarkerNameProperty = new ViewProperty<Marker, string>("MarkerName", (view) => view.MarkerName, (view, value) => view.MarkerName = value);

        private sealed class Marker : View
        {
            public event EventHandler Changed;

            private string _markerName;

            public string MarkerName
            {
                get
                {
                    return this._markerName;
                }

                set
                {
                    this._markerName = value;
                    this.OnPropertyChanged();
                    this.Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [Fact]
        public void ABindingSetsTheViewAtOnce()
        {
            var view = new Marker();

            view.Bind(MarkerNameProperty, new Person(), (p) => p.Name);

            Assert.Equal("Ada", view.MarkerName);
        }

        [Fact]
        public void ABindingFollowsTheSource()
        {
            var view = new Marker();
            var person = new Person();
            view.Bind(MarkerNameProperty, person, (p) => p.Name);

            person.Name = "Grace";

            Assert.Equal("Grace", view.MarkerName);
        }

        [Fact]
        public void ABindingConverts()
        {
            var view = new Marker();
            var person = new Person();
            view.Bind(MarkerNameProperty, person, (p) => p.Name.ToUpperInvariant());

            person.Name = "Grace";

            Assert.Equal("GRACE", view.MarkerName);
        }

        [Fact]
        public void ANeverChangingSourceNeedsARefresh()
        {
            var view = new Marker();
            var plain = new Plain();
            var binding = view.Bind(MarkerNameProperty, plain, (p) => p.Name);

            plain.Name = "Grace";
            Assert.Equal("Ada", view.MarkerName);

            binding.Refresh();
            Assert.Equal("Grace", view.MarkerName);
        }

        [Fact]
        public void ATwoWayBindingWritesBack()
        {
            var view = new Marker();
            var person = new Person();
            view.Bind(MarkerNameProperty, person, (p) => p.Name, (p, value) => p.Name = value);

            view.MarkerName = "Grace";

            Assert.Equal("Grace", person.Name);
        }

        [Fact]
        public void ATwoWayBindingDoesNotLoop()
        {
            var view = new Marker();
            var person = new Person();
            int changes = 0;
            view.Bind(MarkerNameProperty, person, (p) => p.Name, (p, value) => p.Name = value);
            view.Changed += (sender, e) => changes += 1;

            person.Name = "Grace";

            Assert.Equal(1, changes);
        }

        [Fact]
        public void ADisposedBindingStops()
        {
            var view = new Marker();
            var person = new Person();
            var binding = view.Bind(MarkerNameProperty, person, (p) => p.Name);

            binding.Dispose();
            person.Name = "Grace";

            Assert.Equal("Ada", view.MarkerName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference BoundView(
            Person person)
        {
            var view = new Marker();
            view.Bind(MarkerNameProperty, person, (p) => p.Name);
            return new WeakReference(view);
        }

        [Fact]
        public void ASourceDoesNotKeepItsViewsAlive()
        {
            var person = new Person();
            var view = BoundView(person);

            for (int i = 0; i < 3; i += 1)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Assert.False(view.IsAlive);
            person.Name = "Grace";
        }

        [Fact]
        public void AValueThatDidNotChangeLeavesTheViewAlone()
        {
            var view = new Marker();
            var person = new Person();
            int changes = 0;
            view.Bind(MarkerNameProperty, person, (p) => p.Name);
            view.Changed += (sender, e) => changes += 1;

            person.Name = "Ada";

            Assert.Equal(0, changes);
        }
    }
}
