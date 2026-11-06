using System;
using XForma.Controls;
using XForma.Layouts;
using XForma.Primitives;

namespace XForma.Test
{
    // Drag and drop: boxes that can be dragged onto a target (with a mouse, a
    // held finger, or Ctrl+Space, Tab and Enter), and text or files from other
    // apps dropped on it
    public class DragDropPage : Page
    {
        private sealed class Box : Border
        {
            public Box(
                string text,
                Color color)
            {
                this.Focusable = true;
                this.Background = color;
                this.Child = new TextBlock() { Text = text, Margin = new Thickness(8) };
            }
        }

        protected override View Build()
        {
            var status = new TextBlock() { Text = "Drag a box to the target", Name = "status" };
            var target = new Box("Drop here", Colors.LightGray)
            {
                AllowDrop = true,
                Width = 240,
                Height = 120,
                Name = "target",
            };
            target.DragEnter += (sender, e) =>
            {
                e.AcceptedOperation = DragOperation.Copy;
                target.Background = Colors.LightGreen;
            };
            target.DragLeave += (sender, e) => target.Background = Colors.LightGray;
            target.Drop += (sender, e) =>
            {
                target.Background = Colors.LightGray;
                status.Text = $"Dropped: {e.Data.LocalData ?? e.Data.Text ?? e.Data.Uri?.ToString()}, files: {e.Data.Files.Count}, from this app: {e.IsFromThisApp}";
            };

            var panel = new StackPanel() { Spacing = 12, Margin = new Thickness(16) };
            panel.Children.Add(status);
            foreach (var name in new[] { "First", "Second" })
            {
                var box = new Box(name, Colors.LightBlue) { CanDrag = true, Width = 160, Height = 48, Name = name };
                box.DragStarting += (sender, e) =>
                {
                    e.Data.Text = name;
                    e.Data.LocalData = name + " (local)";
                };
                box.DropCompleted += (sender, e) => status.Text += $"; {name} ended: {e.Operation}";
                panel.Children.Add(box);
            }

            panel.Children.Add(target);
            return panel;
        }
    }
}
