using System;
using Windows.UI.Xaml.Media;
using XForms.Controls;
using XamlGrid = global::Windows.UI.Xaml.Controls.Grid;
using XamlTextBlock = global::Windows.UI.Xaml.Controls.TextBlock;
using XamlRectangle = global::Windows.UI.Xaml.Shapes.Rectangle;

namespace XForms.Windows.Renderers
{
    public class TextViewRenderer : ViewRenderer, ITextViewRenderer
    {
        private XamlGrid _grid;
        private XamlTextBlock _textBlock;
        private XamlRectangle _strikethrough;

        public TextViewRenderer(
            TextView textView)
            : base(textView)
        {
            this._textBlock = new XamlTextBlock()
            {
                FontWeight = global::Windows.UI.Text.FontWeights.SemiLight,
                Margin = new global::Windows.UI.Xaml.Thickness(0),
                Padding = new global::Windows.UI.Xaml.Thickness(0),
            };

            // Wrap the TextBlock in a Grid element to support vertical alignment
            this._grid = new XamlGrid()
            {
            };
            this._grid.Children.Add(this._textBlock);

            this.SetNativeElement(this._grid);
        }

        public string Text
        {
            get
            {
                return this._textBlock.Text;
            }

            set
            {
                this._textBlock.Text = value ?? String.Empty;
            }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get
            {
                return this._textBlock.TextAlignment.ToTextAlignment();
            }

            set
            {
                this._textBlock.TextAlignment = value.ToXamlTextAlignment();
            }
        }

        public TextAlignment VerticalTextAlignment
        {
            get
            {
                return this._textBlock.VerticalAlignment.ToTextAlignment();
            }

            set
            {
                this._textBlock.VerticalAlignment = value.ToXamlVerticalAlignment();
            }
        }

        public bool WordWrap
        {
            get
            {
                return (this._textBlock.TextWrapping != global::Windows.UI.Xaml.TextWrapping.NoWrap);
            }

            set
            {
                this._textBlock.TextWrapping = (value ? global::Windows.UI.Xaml.TextWrapping.Wrap : global::Windows.UI.Xaml.TextWrapping.NoWrap);
            }
        }


        public bool TextTrimming
        {
            get
            {
                return (this._textBlock.TextTrimming != global::Windows.UI.Xaml.TextTrimming.None);
            }

            set
            {
                this._textBlock.TextTrimming = (value ? global::Windows.UI.Xaml.TextTrimming.CharacterEllipsis : global::Windows.UI.Xaml.TextTrimming.CharacterEllipsis);
            }
        }

        public Color ForegroundColor
        {
            get
            {
                var solidColorBrush = this._textBlock.Foreground as SolidColorBrush;
                if (null != solidColorBrush)
                {
                    return solidColorBrush.Color.ToColor();
                }

                return Colors.Transparent;
            }

            set
            {
                Brush foregroundBrush = new SolidColorBrush(value.ToXamlColor());
                this._textBlock.Foreground = foregroundBrush;
                if (null != this._strikethrough)
                {
                    this._strikethrough.Fill = foregroundBrush;
                }
            }
        }

        public float FontSize
        {
            get
            {
                return (float)this._textBlock.FontSize;
            }

            set
            {
                this._textBlock.FontSize = value;
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this._textBlock.FontStyle.ToFontStyle();
            }

            set
            {
                this._textBlock.FontStyle = value.ToXamlFontStyle();
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return this._textBlock.FontWeight.ToFontWeight();
            }

            set
            {
                this._textBlock.FontWeight = value.ToXamlFontWeight();
            }
        }

        public bool Strikethrough
        {
            get
            {
                return ((this._strikethrough != null) && (this._strikethrough.Visibility == global::Windows.UI.Xaml.Visibility.Visible));
            }

            set
            {
                if (value)
                {
                    if (null == this._strikethrough)
                    {
                        // Add a rectangle to support strikethrough
                        this._strikethrough = new global::Windows.UI.Xaml.Shapes.Rectangle()
                        {
                            Height = 2,
                            Fill = this._textBlock.Foreground,
                            StrokeThickness = 0,
                            VerticalAlignment = global::Windows.UI.Xaml.VerticalAlignment.Center,
                            HorizontalAlignment = global::Windows.UI.Xaml.HorizontalAlignment.Stretch,
                            Margin = new global::Windows.UI.Xaml.Thickness(0, 5, 0, 0),
                        };
                        this._grid.Children.Add(this._strikethrough);
                    }

                    this._strikethrough.Visibility = global::Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (null != this._strikethrough)
                    {
                        this._strikethrough.Visibility = global::Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }
            }
        }

        public Color BackgroundColor
        {
            get
            {
                var solidColorBrush = this._grid?.Background as SolidColorBrush;
                if (null != solidColorBrush)
                {
                    return solidColorBrush.Color.ToColor();
                }

                return Colors.Transparent;
            }

            set
            {
                var nativeControl = this._grid;
                if (null != nativeControl)
                {
                    Brush backgroundBrush = new SolidColorBrush(value.ToXamlColor());
                    nativeControl.Background = backgroundBrush;
                }
            }
        }
    }
}
